using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using HarmonyLib;
using HarmonyLib.Tools;
using Mono.Cecil;
using Mono.Cecil.Pdb;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using NeoModLoader.api;
using NeoModLoader.api.attributes;
using NeoModLoader.constants;
using NeoModLoader.services;
using ExceptionHandler = Mono.Cecil.Cil.ExceptionHandler;
using Instruction = Mono.Cecil.Cil.Instruction;
using MemoryStream = System.IO.MemoryStream;
using OpCode = Mono.Cecil.Cil.OpCode;
using OpCodes = System.Reflection.Emit.OpCodes;
using VariableReference = Mono.Cecil.Cil.VariableReference;

namespace NeoModLoader.utils;

internal static class ModReloadUtils
{
    private static IReloadable _mod;
    private static ModDeclare _mod_declare;
    private static string _new_compiled_dll_path;
    private static string _new_compiled_pdb_path;
    private static AssemblyDefinition _old_assembly_definition;
    private static Dictionary<string, MethodDefinition> _old_method_definitions = new();
    private static Dictionary<OpCode, System.Reflection.Emit.OpCode> _op_code_map = new();

    private static Dictionary<MethodDefinition, MethodInfo> _regenerated_brand_new_methods = new();

    private static Dictionary<Type, MethodInfo> _emit_method_cache = new();

    private static readonly Dictionary<MethodInfo, ILHook> _create_hooks = new();

    public static bool Prepare(IReloadable pMod, ModDeclare pModDeclare)
    {
        _mod = pMod;
        _mod_declare = pModDeclare;

        _new_compiled_dll_path = Path.Combine(Paths.CompiledModsPath, $"{_mod_declare.UID}.dll");
        _new_compiled_pdb_path = Path.Combine(Paths.CompiledModsPath, $"{_mod_declare.UID}.pdb");

        try
        {
            _old_assembly_definition.Dispose();
            _old_assembly_definition = null;
            _old_method_definitions.Clear();
        }
        catch (Exception)
        {
            // ignored
        }

        if (!File.Exists(_new_compiled_dll_path))
        {
            LogService.LogError($"No compiled dll found for mod {_mod_declare.UID}");
            return false;
        }

        if (File.Exists(_new_compiled_pdb_path + ".bak"))
        {
            File.Delete(_new_compiled_pdb_path + ".bak");
        }

        File.Copy(_new_compiled_dll_path, _new_compiled_dll_path + ".bak", true);
        _old_assembly_definition = AssemblyDefinition.ReadAssembly(_new_compiled_dll_path + ".bak");


        return true;
    }

    public static bool CompileNew()
    {
        if (!ModCompileLoadService.TryCompileModAtRuntime(_mod_declare, true)) return false;
        foreach (var type in _old_assembly_definition.MainModule.Types)
        {
            foreach (var method in type.Methods)
            {
                _old_method_definitions[method.FullName] = method;
            }

            foreach (MethodDefinition method in type.NestedTypes.SelectMany(nested_type => nested_type.Methods))
            {
                _old_method_definitions[method.FullName] = method;
            }
        }

        return true;
    }

    public static bool PatchHotfixMethods()
    {
        HarmonyFileLog.Enabled = true;
        AssemblyDefinition assembly_definition = AssemblyDefinition.ReadAssembly(_new_compiled_dll_path);
        List<MethodDefinition> method_definitions = new();
        method_definitions.AddRange(assembly_definition.MainModule.Types.SelectMany(type => type.Methods));

        foreach (TypeDefinition nested_type in
                 assembly_definition.MainModule.Types.SelectMany(type => type.NestedTypes))
        {
            method_definitions.AddRange(nested_type.Methods);
        }

        Assembly old_assembly = _mod.GetType().Assembly;

        Harmony harmony = new Harmony(_mod_declare.UID);

        if (_op_code_map.Count == 0)
        {
            InitializeOpcodeMap();
        }

        HashSet<MethodDefinition> brand_new_methods = new();
        foreach (var new_method in method_definitions)
        {
            if (new_method.HasBody is false)
            {
                continue;
            }

            bool hotfixable = false;
            foreach (var attribute in new_method.CustomAttributes)
            {
                if (attribute.AttributeType.FullName == typeof(HotfixableAttribute).FullName)
                {
                    hotfixable = true;
                    break;
                }
            }

            if (hotfixable is false)
            {
                continue;
            }

            MethodInfo old_method = old_assembly.GetType(new_method.DeclaringType.FullName).GetMethod(
                new_method.Name,
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null,
                new_method.Parameters.Select(x => x.ParameterType.ResolveReflection()).ToArray(), null);
            if (old_method != null)
            {
                continue;
            }

            LogService.LogWarning(
                $"No found method {new_method.DeclaringType.FullName}::{new_method.Name} in old assembly");
            brand_new_methods.Add(new_method);
        }

        if (brand_new_methods.Count > 0)
        {
            CreateBrandNewMethods(brand_new_methods);
        }

        foreach (var new_method in method_definitions)
        {
            if (new_method.HasBody is false)
            {
                continue;
            }

            bool hotfixable = false;
            foreach (var attribute in new_method.CustomAttributes)
            {
                if (attribute.AttributeType.FullName == typeof(HotfixableAttribute).FullName)
                {
                    hotfixable = true;
                    break;
                }
            }

            if (hotfixable is false)
            {
                continue;
            }

            if (brand_new_methods.Contains(new_method))
            {
                continue;
            }

            try
            {
                MethodInfo old_method = old_assembly.GetType(new_method.DeclaringType.FullName).GetMethod(
                    new_method.Name,
                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null,
                    new_method.Parameters.Select(x => x.ParameterType.ResolveReflection()).ToArray(), null);
                if (old_method == null)
                {
                    continue;
                }

                if (!NeedHotfix(old_method, new_method))
                {
                    LogService.LogInfo($"Method {new_method.Name} does not need hotfix");
                    continue;
                }

                LogService.LogInfo(
                    $"Hotfixing method {new_method.Name} with following instructions(total {new_method.Body.Instructions.Count}):");
                HotfixMethod(harmony, new_method, old_method);
            }
            catch (Exception e)
            {
                LogService.LogError(
                    $"Failed to hotfix method {new_method.Name}, Most likely because NeoModLoader does not support such method hotfix now.");
                LogService.LogError(e.Message);
                LogService.LogError(e.StackTrace);
                continue;
            }
        }

        assembly_definition.Dispose();
        return true;
    }

    private static void CreateBrandNewMethods(HashSet<MethodDefinition> pBrandNewMethods)
    {
        LogService.LogWarning($"Find {pBrandNewMethods.Count} brand new methods, creating...");
        int count = pBrandNewMethods.Count;
        HashSet<MethodDefinition> dup_brand_new_methods = new(pBrandNewMethods);
        while (count-- > 0)
        {
            foreach (MethodDefinition method_definition in dup_brand_new_methods)
            {
                try
                {
                    DynamicMethodDefinition dynamic_method_definition = regenerate(method_definition);
                    var generated = dynamic_method_definition.Generate();
                    _regenerated_brand_new_methods[method_definition] = generated;
                }
                catch (Exception e)
                {
                    LogService.LogError($"Failed to create brand new method {method_definition.FullName}");
                    LogService.LogError(e.Message);
                    LogService.LogError(e.StackTrace);
                    continue;
                }

                pBrandNewMethods.Remove(method_definition);
            }
        }
    }

    private static bool NeedHotfix(MethodInfo pOldMethod, MethodDefinition pNewMethod)
    {
        if (_old_method_definitions.TryGetValue(pNewMethod.FullName, out var old_method_definition) is false)
        {
            LogService.LogWarning($"No found method {pNewMethod.FullName} in old assembly");
            return true;
        }

        var old_il = old_method_definition.Body.Instructions;
        var new_il = pNewMethod.Body.Instructions;

        if (old_il.Count != new_il.Count)
        {
            return true;
        }

        var old_sb = new StringBuilder();
        var new_sb = new StringBuilder();

        const string skipAddrFormat = "IL_0000: ";
        foreach (var inst in old_il)
        {
            if (inst.Operand is Instruction inst_inst)
            {
                old_sb.AppendLine($"{inst.OpCode} {inst_inst.Offset - inst.Offset}");
            }
            else
            {
                old_sb.AppendLine(inst.ToString().Substring(skipAddrFormat.Length));
            }
        }

        foreach (var inst in new_il)
        {
            if (inst.Operand is Instruction inst_inst)
            {
                new_sb.AppendLine($"{inst.OpCode} {inst_inst.Offset - inst.Offset}");
            }
            else
            {
                new_sb.AppendLine(inst.ToString().Substring(skipAddrFormat.Length));
            }
        }

        return old_sb.ToString().GetHashCode() != new_sb.ToString().GetHashCode();
    }

    private static void InitializeOpcodeMap()
    {
        foreach (var field in typeof(OpCodes).GetFields())
        {
            if (field.FieldType != typeof(System.Reflection.Emit.OpCode)) continue;
            var op_code = (System.Reflection.Emit.OpCode)field.GetValue(null);
            try
            {
                _op_code_map.Add(
                    (OpCode)typeof(Mono.Cecil.Cil.OpCodes).GetField(field.Name).GetValue(null)
                    , op_code);
            }
            catch (Exception)
            {
                // Ignored, some opcodes are invalid, useful opcodes are added manually below
                //LogService.LogError($"Failed to initialize opcode map for {field.Name}");
                //LogService.LogError(e.Message);
                //LogService.LogError(e.StackTrace);
            }
        }

        _op_code_map.Add(Mono.Cecil.Cil.OpCodes.Stelem_Any, OpCodes.Stelem);
        _op_code_map.Add(Mono.Cecil.Cil.OpCodes.Ldelem_Any, OpCodes.Ldelem);
        _op_code_map.Add(Mono.Cecil.Cil.OpCodes.Tail, OpCodes.Tailcall);
    }

    private static void HotfixMethod(Harmony pHarmony, MethodDefinition pNewMethod, MethodInfo pOldMethod)
    {
        ReplaceMethod(pOldMethod, regenerate(pNewMethod));
    }

/* It causes types inconsistency between old and new assembly
    public static bool PatchHotfixMethodsNT()
    {
        //RewritePatchAssembly(_new_compiled_dll_path);
        var bytes = File.ReadAllBytes(_new_compiled_dll_path);
        var new_assembly = Assembly.Load(bytes);
        var old_assembly = _mod.GetType().Assembly;
        Dictionary<string, MethodInfo> detour_methods = new();
        foreach (var method in new_assembly.GetTypes().SelectMany(x => x.GetMethods()))
        {
            if (method.GetCustomAttribute<HotfixableAttribute>() == null) continue;
            var declaring_type = old_assembly.GetType(method.DeclaringType.FullName);
            if (declaring_type == null) continue;
            var old_method = declaring_type.GetMethod(
                method.Name,
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null,
                method.GetParameters().Select(x => x.ParameterType).ToArray(), null);
            if (old_method == null) continue;

            new Detour(old_method, method);
            LogService.LogInfo($"Detour {method.DeclaringType.FullName}.{method.Name}");
            detour_methods.Add($"{method.DeclaringType.FullName}.{method.Name}", old_method);
        }

        foreach (var type in new_assembly.GetTypes())
        {
            if (type.IsGenericType) continue;
            var old_type = old_assembly.GetType(type.FullName);
            if (old_type == null) continue;
            foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var old_field = old_type.GetField(field.Name,
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (old_field == null) continue;
                bool redirect_result = true;
                try
                {
                    field.SetValue(null, old_field.GetValue(null));
                }
                catch (Exception e)
                {
                    redirect_result = false;
                    try
                    {
                        var new_obj = Activator.CreateInstance(field.FieldType);
                        CreateCopyDelegate(old_field.GetType(), field.GetType())(old_field.GetValue(null), new_obj);
                        field.SetValue(null, new_obj);
                    }
                    catch (Exception new_e)
                    {
                        LogService.LogException(new_e);
                    }
                }

                //LogService.LogInfo(
                //    $"Redirect static fields({(redirect_result ? "success" : "failed")}): {field.Name} {field.FieldType} {field.DeclaringType.FullName}");
            }
        }

        return true;
    }
    public static Action<object, object> CreateCopyDelegate(Type sourceType, Type targetType)
    {
        var source = Expression.Parameter(typeof(object), "source");
        var target = Expression.Parameter(typeof(object), "target");

        var sourceCast = Expression.Convert(source, sourceType);
        var targetCast = Expression.Convert(target, targetType);

        var assignments = new List<Expression>();
        foreach (var sourceField in sourceType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            var targetField = targetType.GetField(sourceField.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (targetField == null) continue;

            var sourceFieldExpr = Expression.Field(sourceCast, sourceField);
            var targetFieldExpr = Expression.Field(targetCast, targetField);
            assignments.Add(Expression.Assign(targetFieldExpr, sourceFieldExpr));
        }

        var body = Expression.Block(assignments);
        return Expression.Lambda<Action<object, object>>(body, source, target).Compile();
    }
    public static void RewritePatchAssembly(
        string patchDllPath
    )
    {
        var patch_data = File.ReadAllBytes(patchDllPath);

        using var patch_stream = new MemoryStream(patch_data);

        var patchAssembly = AssemblyDefinition.ReadAssembly(patch_stream);

        var patchModule = patchAssembly.MainModule;

        foreach (var type in patchModule.Types.ToList())
        {
            RedirectTypeReferences(type, patchModule);
        }

        patchAssembly.Write(patchDllPath);
    }

    private static void RedirectTypeReferences(TypeDefinition type, ModuleDefinition patchModule)
    {
        // 重定向基类
        if (type.BaseType != null)
        {
            type.BaseType = RedirectReference(type.BaseType, patchModule);
        }

        // 重定向字段类型
        foreach (var field in type.Fields)
        {
            field.FieldType = RedirectReference(field.FieldType, patchModule);
        }

        // 重定向方法参数和返回类型
        foreach (var method in type.Methods)
        {
            method.ReturnType = RedirectReference(method.ReturnType, patchModule);
            foreach (var param in method.Parameters)
            {
                param.ParameterType = RedirectReference(param.ParameterType, patchModule);
            }

            // 重定向方法体中的类型引用
            if (method.HasBody)
            {
                foreach (var instr in method.Body.Instructions.ToList()) // 转换为 List 避免迭代时修改异常
                {
                    if (instr.Operand is TypeReference typeRef)
                    {
                        instr.Operand = RedirectReference(typeRef, patchModule);
                    }
                    else if (instr.Operand is MethodReference methodRef)
                    {
                        // 创建新的 MethodReference
                        var newMethodRef = new MethodReference(
                            methodRef.Name,
                            RedirectReference(methodRef.ReturnType, patchModule),
                            RedirectReference(methodRef.DeclaringType, patchModule)
                        );

                        // 复制泛型参数和参数类型
                        foreach (var gp in methodRef.GenericParameters)
                            newMethodRef.GenericParameters.Add(new GenericParameter(gp.Name, newMethodRef));

                        foreach (var param in methodRef.Parameters)
                            newMethodRef.Parameters.Add(new ParameterDefinition(
                                RedirectReference(param.ParameterType, patchModule)
                            ));

                        // 处理泛型方法实例
                        if (methodRef is GenericInstanceMethod genericMethod)
                        {
                            var newGenericMethod = new GenericInstanceMethod(newMethodRef);
                            foreach (var arg in genericMethod.GenericArguments)
                                newGenericMethod.GenericArguments.Add(RedirectReference(arg, patchModule));
                            instr.Operand = newGenericMethod;
                        }
                        else
                        {
                            instr.Operand = newMethodRef;
                        }
                    }
                    else if (instr.Operand is FieldReference fieldRef)
                    {
                        var newFieldRef = new FieldReference(
                            fieldRef.Name,
                            RedirectReference(fieldRef.FieldType, patchModule),
                            RedirectReference(fieldRef.DeclaringType, patchModule)
                        );
                        instr.Operand = newFieldRef;
                    }
                }
            }
        }
    }

    private static TypeReference RedirectReference(
        TypeReference typeRef,
        ModuleDefinition patchModule
    )
    {
        // 若类型已在原程序集中定义，则重定向引用
        var originalType = AccessTools.TypeByName(typeRef.FullName);

        if (originalType != null)
        {
            var originalTypeRef = patchModule.ImportReference(originalType);
            //LogService.LogInfo($"Redirecting type reference: {typeRef.FullName} -> {originalTypeRef.FullName}");
            return originalTypeRef;
        }
        else
        {
            //LogService.LogWarning($"Cannot redirect complex type reference: {typeRef.FullName}");
        }

        // 处理泛型类型（如 BasicMod<T>）
        if (typeRef is GenericInstanceType genericType)
        {
            var genericElement = RedirectReference(genericType.ElementType, patchModule);
            var newGenericType = new GenericInstanceType(genericElement);

            foreach (var arg in genericType.GenericArguments)
            {
                newGenericType.GenericArguments.Add(RedirectReference(arg, patchModule));
            }

            return newGenericType;
        }

        // 处理数组类型（如 int[]）
        if (typeRef is ArrayType arrayType)
        {
            return new ArrayType(
                RedirectReference(arrayType.ElementType, patchModule),
                arrayType.Rank
            );
        }

        return typeRef;
    }
*/
    public static bool PatchHotfixMethodsNT()
    {
        var assembly_bytes = File.ReadAllBytes(_new_compiled_dll_path);
        var pdb_bytes = File.ReadAllBytes(_new_compiled_pdb_path);
        using var f_stream = new MemoryStream(assembly_bytes);
        using var p_stream = new MemoryStream(pdb_bytes);
        var assembly_definition = AssemblyDefinition.ReadAssembly(f_stream, new ReaderParameters
        {
            ReadSymbols = true,
            SymbolStream = p_stream,
            SymbolReaderProvider = new PdbReaderProvider()
        });


        List<MethodDefinition> method_definitions = new();
        method_definitions.AddRange(assembly_definition.MainModule.Types.SelectMany(type => type.Methods));

        foreach (var nested_type in
                 assembly_definition.MainModule.Types.SelectMany(type => type.NestedTypes))
            method_definitions.AddRange(nested_type.Methods);

        HashSet<MethodDefinition> methods_to_create = new();
        List<(MethodInfo, MethodDefinition)> methods_to_replace = new();

        foreach (var new_method in method_definitions)
        {
            if (new_method.HasBody is false) continue;

            var hotfixable = false;
            foreach (var attribute in new_method.CustomAttributes)
                if (attribute.AttributeType.FullName == typeof(HotfixableAttribute).FullName)
                {
                    hotfixable = true;
                    break;
                }

            if (hotfixable is false) continue;

            var declaring_type = AccessTools.TypeByName(new_method.DeclaringType.FullName);
            if (declaring_type == null) continue;

            var old_method = declaring_type.GetMethod(
                new_method.Name,
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null,
                new_method.Parameters.Select(x => x.ParameterType.ResolveReflection()).ToArray(), null);
            if (old_method == null)
                //methods_to_create.Add(new_method);
                continue;

            methods_to_replace.Add((old_method, new_method));
        }

        while (methods_to_create.Count > 0)
        {
            var methods_created = new HashSet<MethodDefinition>();
            foreach (var method_definition in methods_to_create)
            {
                try
                {
                    _regenerated_brand_new_methods[method_definition] = CreateMethod(method_definition);
                }
                catch (Exception e)
                {
                    LogService.LogError($"Failed to create brand new method {method_definition.FullName}");
                    LogService.LogError(e.Message);
                    LogService.LogError(e.StackTrace);
                    continue;
                }

                methods_created.Add(method_definition);
            }

            if (methods_created.Count == 0) break;

            methods_to_create.ExceptWith(methods_created);
        }

        foreach (var (old_method, new_method) in methods_to_replace)
        {
            try
            {
                Replace(old_method, new_method);
            }
            catch (Exception e)
            {
                LogService.LogError($"Failed to hotfix method {new_method.FullName}");
                LogService.LogError(e.Message);
                LogService.LogError(e.StackTrace);
            }
        }

        return true;
    }

    private static MethodInfo CreateMethod(MethodDefinition newMethod)
    {
        var dynamic_method_definition = new DynamicMethodDefinition(newMethod.Name,
            newMethod.ReturnType.ResolveReflection(),
            newMethod.Parameters.Select(x => x.ParameterType.ResolveReflection()).ToArray());
        if (!newMethod.IsStatic)
            dynamic_method_definition.Definition.Parameters.Insert(0,
                new ParameterDefinition(newMethod.DeclaringType));

        var dyn_body = dynamic_method_definition.Definition.Body;
        var method_body = newMethod.Body;
        dyn_body.Variables.Clear();
        dyn_body.Instructions.Clear();
        dyn_body.ExceptionHandlers.Clear();
        dyn_body.Variables.AddRange(method_body.Variables);
        dyn_body.Instructions.AddRange(method_body.Instructions);
        dyn_body.ExceptionHandlers.AddRange(method_body.ExceptionHandlers);

        return dynamic_method_definition.Generate();
    }

    private static void Replace(MethodInfo oldMethod, MethodDefinition newMethod)
    {
        if (_create_hooks.ContainsKey(oldMethod)) _create_hooks[oldMethod].Dispose();

        /*
        var debug_info = newMethod.DebugInformation;
        if (debug_info != null && debug_info.HasSequencePoints)
        {
            foreach (var p in debug_info.GetSequencePointMapping())
            {
                LogService.LogInfo($"{p.Key}: {p.Value.StartLine} {p.Value.Document.Url}");
            }
        }*/
        var hook = new ILHook(
            oldMethod,
            il =>
            {
                il.Body.Variables.Clear();
                il.Body.Instructions.Clear();
                il.Body.ExceptionHandlers.Clear();
                il.Body.Variables.AddRange(newMethod.Body.Variables);
                il.Body.Instructions.AddRange(newMethod.Body.Instructions);
                il.Body.ExceptionHandlers.AddRange(newMethod.Body.ExceptionHandlers);
            }
        );
        hook.Apply();

        _create_hooks[oldMethod] = hook;
    }

    private static void ReplaceMethod(MethodInfo pOldMethod, DynamicMethodDefinition pNewMethod)
    {
        var pNewMethodMethod = pNewMethod.Generate();
        RuntimeHelpers.PrepareMethod(pOldMethod.MethodHandle);
        IntPtr pBody = pOldMethod.MethodHandle.GetFunctionPointer();
        RuntimeHelpers.PrepareMethod(pNewMethodMethod.MethodHandle);
        IntPtr pBorrowed = pNewMethodMethod.MethodHandle.GetFunctionPointer();

        LogService.LogInfo($"Is 64bit: {Environment.Is64BitProcess}");

        unsafe
        {
            var ptr = (byte*)pBody.ToPointer();
            var ptr2 = (byte*)pBorrowed.ToPointer();
            var ptrDiff = ptr2 - ptr - 5;
            if (ptrDiff < (long)0xFFFFFFFF && ptrDiff > (long)-0xFFFFFFFF)
            {
                // 32-bit relative jump, available on both 32 and 64 bit arch.
                LogService.LogInfo($"diff is {ptrDiff} doing relative jmp");
                LogService.LogInfo(string.Format("patching on {0:X}, target: {1:X}", (ulong)ptr, (ulong)ptr2));
                *ptr = 0xe9; // JMP
                *((uint*)(ptr + 1)) = (uint)ptrDiff;
            }
            else
            {
                LogService.LogInfo($"diff is {ptrDiff} doing push+ret trampoline");
                LogService.LogInfo(string.Format("patching on {0:X}, target: {1:X}", (ulong)ptr, (ulong)ptr2));
                if (Environment.Is64BitProcess)
                {
                    // For 64bit arch and likely 64bit pointers, do:
                    // PUSH bits 0 - 32 of addr
                    // MOV [RSP+4] bits 32 - 64 of addr
                    // RET
                    var cursor = ptr;
                    *(cursor++) = 0x68; // PUSH
                    *((uint*)cursor) = (uint)ptr2;
                    cursor += 4;
                    *(cursor++) = 0xC7; // MOV [RSP+4]
                    *(cursor++) = 0x44;
                    *(cursor++) = 0x24;
                    *(cursor++) = 0x04;
                    *((uint*)cursor) = (uint)((ulong)ptr2 >> 32);
                    cursor += 4;
                    *(cursor++) = 0xc3; // RET
                }
                else
                {
                    // For 32bit arch and 32bit pointers, do: PUSH addr, RET.
                    *ptr = 0x68;
                    *((uint*)(ptr + 1)) = (uint)ptr2;
                    *(ptr + 5) = 0xC3;
                }
            }

            LogService.LogInfo(string.Format("Patched 0x{0:X} to 0x{1:X}.", (ulong)ptr, (ulong)ptr2));
        }
    }

    static DynamicMethodDefinition regenerate(MethodDefinition pMethodDefinition)
    {
        DynamicMethodDefinition dynamic_method_definition = new DynamicMethodDefinition(pMethodDefinition.Name,
            pMethodDefinition.ReturnType.ResolveReflection(),
            pMethodDefinition.Parameters.Select(x => x.ParameterType.ResolveReflection()).ToArray());
        if (!pMethodDefinition.IsStatic)
        {
            dynamic_method_definition.Definition.Parameters.Insert(0,
                new ParameterDefinition(pMethodDefinition.DeclaringType));
        }

        foreach (var parameter in pMethodDefinition.Parameters)
        {
            LogService.LogInfo($"\tDeclare parameter {parameter.ToString()}({parameter.ParameterType.FullName})");
        }

        ILGenerator il = dynamic_method_definition.GetILGenerator();
        if (pMethodDefinition.Body.InitLocals)
        {
            dynamic_method_definition.Definition.Body.InitLocals = true;
        }

        foreach (var local_var in pMethodDefinition.Body.Variables)
        {
            LogService.LogInfo($"\tDeclare local variable {local_var.ToString()}({local_var.VariableType.FullName})");
            il.DeclareLocal(local_var.VariableType.ResolveReflection());
        }

        var labels = new Dictionary<Instruction, Label>();
        // Track labels
        foreach (var inst in pMethodDefinition.Body.Instructions)
        {
            if (inst.Operand is Instruction opinst)
            {
                LogService.LogInfo($"\tDeclare label for {opinst.ToString()}");
                labels[opinst] = il.DefineLabel();
            }
            else if (inst.Operand is Instruction[] opinsts)
            {
                foreach (var label_inst in opinsts)
                {
                    labels[label_inst] = il.DefineLabel();
                }
            }
        }

        Dictionary<Instruction, ExceptionHandler> excep_handlers = new();

        foreach (var excep in pMethodDefinition.Body.ExceptionHandlers)
        {
            LogService.LogInfo($"\tDeclare exception handler for {excep.ToString()}");
            excep_handlers[excep.TryStart] = excep;
            excep_handlers[excep.TryEnd] = excep;
            excep_handlers[excep.HandlerStart] = excep;
            excep_handlers[excep.HandlerEnd] = excep;
            if (excep.TryStart != null)
            {
                //labels[excep.TryStart] = il.BeginExceptionBlock();
            }
        }

        try
        {
            foreach (var inst in pMethodDefinition.Body.Instructions)
            {
                if (labels.TryGetValue(inst, out var label))
                {
                    il.MarkLabel(label);
                }

                if (excep_handlers.TryGetValue(inst, out var excep_handler))
                {
                    if (inst == excep_handler.TryEnd)
                    {
                        LogService.LogWarning("TryEnd");
                        //il.EndExceptionBlock();
                    }
                    else if (inst == excep_handler.HandlerStart)
                    {
                        LogService.LogWarning("HandlerStart");
                        /*
                        switch (excep_handler.HandlerType)
                        {
                            case ExceptionHandlerType.Catch:
                                il.BeginCatchBlock(excep_handler.CatchType.ResolveReflection());
                                break;
                            case ExceptionHandlerType.Filter:
                                il.BeginExceptFilterBlock();
                                break;
                            case ExceptionHandlerType.Finally:
                                il.BeginFinallyBlock();
                                break;
                            case ExceptionHandlerType.Fault:
                                il.BeginFaultBlock();
                                break;
                        }
                        continue;*/
                    }
                    else if (inst == excep_handler.HandlerEnd)
                    {
                        LogService.LogWarning("HandlerEnd");
                        //il.EndExceptionBlock();
                    }
                    else
                    {
                        // TryStart
                        //il.MarkLabel(il.BeginExceptionBlock());
                        LogService.LogWarning("TryStart");
                    }
                }

                var op_code = _op_code_map[inst.OpCode];

                if (op_code == OpCodes.Endfinally) continue;

                LogService.LogInfo($"\t{op_code}\t\t {inst.Operand}({inst.Operand?.GetType().FullName})");

                if (inst.Operand == null)
                {
                    il.Emit(op_code);
                    continue;
                }

                if (inst.Operand is Instruction)
                {
                    il.Emit(op_code, labels[(Instruction)inst.Operand]);
                    continue;
                }

                var operand_type = inst.Operand.GetType();


                if (inst.Operand is MemberReference member_reference)
                {
                    MemberInfo resolved = null;
                    try
                    {
                        resolved = member_reference.ResolveReflection();
                        if (resolved == null)
                            throw new Exception($"Failed to resolve member reference {member_reference.FullName}");
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            if (member_reference is MethodReference method_reference)
                            {
                                resolved = _regenerated_brand_new_methods[method_reference.Resolve()];
                            }
                        }
                        catch (Exception)
                        {
                            LogService.LogError($"Failed to resolve member reference {member_reference.FullName}");
                            LogService.LogError(e.Message);
                            LogService.LogError(e.StackTrace);
                        }
                    }

                    operand_type = resolved.GetType();
                    if (!_emit_method_cache.TryGetValue(operand_type, out var emit_method))
                    {
                        emit_method = AccessTools.Method(typeof(ILGenerator), "Emit",
                            new Type[]
                            {
                                typeof(System.Reflection.Emit.OpCode), operand_type
                            });
                        _emit_method_cache[operand_type] = emit_method;
                    }

                    if (emit_method == null)
                    {
                        throw new Exception($"Failed to get emit method for {operand_type.FullName}");
                    }

                    emit_method.Invoke(il, new object[]
                    {
                        op_code, resolved
                    });
                }
                else if (inst.Operand is VariableReference variable_reference)
                {
                    il.Emit(op_code, variable_reference.Index);
                }
                else if (inst.Operand is Instruction[] jump_to_insts)
                {
                    // switch
                    Label[] switch_labels = new Label[jump_to_insts.Length];
                    for (int i = 0; i < jump_to_insts.Length; i++)
                    {
                        switch_labels[i] = labels[jump_to_insts[i]];
                    }

                    il.Emit(OpCodes.Switch, switch_labels);
                }
                else if (inst.Operand is ParameterDefinition parameter_definition)
                {
                    il.Emit(op_code, parameter_definition.Sequence);
                }
                else
                {
                    if (!_emit_method_cache.TryGetValue(operand_type, out var emit_method))
                    {
                        emit_method = AccessTools.Method(typeof(ILGenerator), "Emit",
                            new Type[]
                            {
                                typeof(System.Reflection.Emit.OpCode), operand_type
                            });
                        _emit_method_cache[operand_type] = emit_method;
                    }

                    if (emit_method == null)
                    {
                        throw new Exception($"Failed to get emit method for {operand_type.FullName}");
                    }

                    try
                    {
                        emit_method.Invoke(il, new object[]
                        {
                            op_code, inst.Operand
                        });
                    }
                    catch (Exception e)
                    {
                        if (inst.Operand is sbyte as_sbyte)
                        {
                            il.Emit(op_code, (int)as_sbyte);
                        }
                        else
                        {
                            LogService.LogError(
                                $"Failed to emit {op_code} {inst.Operand}({inst.Operand?.GetType().FullName})");
                            LogService.LogError(e.Message);
                            LogService.LogError(e.StackTrace);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            LogService.LogError(e.Message);
            LogService.LogError(e.StackTrace);
        }
        finally
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Current instructions:");
            foreach (var inst in dynamic_method_definition.GetILProcessor().Body.Instructions)
            {
                sb.AppendLine($"\t{inst.OpCode}\t\t {inst.Operand}({inst.Operand?.GetType().FullName})");
            }

            LogService.LogWarning(sb.ToString());
        }

        return dynamic_method_definition;
    }

    public static bool Reload()
    {
        try
        {
            _mod.Reload();
        }
        catch (Exception e)
        {
            LogService.LogError(e.Message);
            LogService.LogError(e.StackTrace);
            return false;
        }

        return true;
    }
}