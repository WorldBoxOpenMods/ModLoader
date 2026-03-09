using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using HarmonyLib;
using HarmonyLib.Tools;
using Mono.Cecil;
using Mono.Cecil.Pdb;
using MonoMod.RuntimeDetour;
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
    private static Assembly _new_runtime_assembly;
    private static Dictionary<string, MethodDefinition> _old_method_definitions_by_identity = new();
    private static Dictionary<string, FieldDefinition> _old_field_definitions_by_identity = new();
    private static Dictionary<OpCode, System.Reflection.Emit.OpCode> _op_code_map = new();

    private static Dictionary<MethodDefinition, MethodInfo> _generated_methods_by_definition = new();

    private static Dictionary<Type, MethodInfo> _emit_method_cache = new();

    private static readonly Dictionary<MethodInfo, Detour> _replacement_detours = new();
    private static readonly Dictionary<MethodInfo, MethodInfo> _replacement_methods = new();
    private static readonly List<object> _generated_wrapper_delegates = new();
    private static readonly List<Type> _generated_wrapper_types = new();

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
        }
        catch (Exception)
        {
            // ignored
        }

        _old_method_definitions_by_identity.Clear();
        _old_field_definitions_by_identity.Clear();
        _generated_methods_by_definition.Clear();
        _new_runtime_assembly = null;
        _generated_wrapper_delegates.Clear();
        _generated_wrapper_types.Clear();
        foreach (var detour in _replacement_detours.Values)
        {
            detour.Dispose();
        }

        _replacement_detours.Clear();
        _replacement_methods.Clear();

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
        foreach (var type in EnumerateTypesRecursive(_old_assembly_definition.MainModule.Types))
        {
            foreach (var method in type.Methods)
            {
                _old_method_definitions_by_identity[GetMethodIdentity(method)] = method;
            }
            foreach (var field in type.Fields)
            {
                _old_field_definitions_by_identity[GetFieldIdentity(field)] = field;
            }
        }

        return true;
    }

    private static IEnumerable<TypeDefinition> EnumerateTypesRecursive(IEnumerable<TypeDefinition> types)
    {
        foreach (var type in types)
        {
            yield return type;

            foreach (var nested in EnumerateTypesRecursive(type.NestedTypes))
            {
                yield return nested;
            }
        }
    }

    private static bool IsHotfixable(MethodDefinition methodDefinition)
    {
        if (Config.isAndroid)
        {
            return false;
        }
        return methodDefinition.CustomAttributes.Any(
            attribute => attribute.AttributeType.FullName == typeof(HotfixableAttribute).FullName
        );
    }

    private static bool IsCompilerGenerated(Mono.Cecil.ICustomAttributeProvider provider)
    {
        return provider.CustomAttributes.Any(
            attribute => attribute.AttributeType.FullName == typeof(CompilerGeneratedAttribute).FullName
        );
    }

    private static bool IsCompilerGenerated(MethodDefinition methodDefinition)
    {
        return IsCompilerGenerated((Mono.Cecil.ICustomAttributeProvider)methodDefinition)
               || IsCompilerGenerated((Mono.Cecil.ICustomAttributeProvider)methodDefinition.DeclaringType)
               || methodDefinition.Name.Contains("<");
    }

    private static bool IsGenericHotfixUnsupported(MethodDefinition methodDefinition)
    {
        return methodDefinition.HasGenericParameters || methodDefinition.DeclaringType.HasGenericParameters;
    }

    private static string GetMethodIdentity(MethodReference methodReference)
    {
        var builder = new StringBuilder();
        builder.Append(GetTypeIdentity(methodReference.DeclaringType));
        builder.Append("::");
        builder.Append(methodReference.Name);
        builder.Append("|g=");
        builder.Append(GetMethodGenericArity(methodReference));
        builder.Append("|margs=");
        foreach (var generic_argument in GetMethodGenericArguments(methodReference))
        {
            builder.Append('[');
            builder.Append(GetTypeIdentity(generic_argument));
            builder.Append(']');
        }
        builder.Append("|ret=");
        builder.Append(GetTypeIdentity(methodReference.ReturnType));
        builder.Append("|params=");

        foreach (var parameter in methodReference.Parameters)
        {
            builder.Append('[');
            builder.Append(GetTypeIdentity(parameter.ParameterType));
            builder.Append("|in=");
            builder.Append(parameter.IsIn ? '1' : '0');
            builder.Append("|out=");
            builder.Append(parameter.IsOut ? '1' : '0');
            builder.Append(']');
        }

        return builder.ToString();
    }

    private static string GetMethodIdentity(MethodBase methodBase)
    {
        var builder = new StringBuilder();
        builder.Append(GetTypeIdentity(methodBase.DeclaringType));
        builder.Append("::");
        builder.Append(methodBase.Name);
        builder.Append("|g=");
        builder.Append(GetMethodGenericArity(methodBase));
        builder.Append("|margs=");
        foreach (var generic_argument in GetMethodGenericArguments(methodBase))
        {
            builder.Append('[');
            builder.Append(GetTypeIdentity(generic_argument));
            builder.Append(']');
        }
        builder.Append("|ret=");
        builder.Append(methodBase is MethodInfo methodInfo
            ? GetTypeIdentity(methodInfo.ReturnType)
            : typeof(void).FullName);
        builder.Append("|params=");

        foreach (var parameter in methodBase.GetParameters())
        {
            builder.Append('[');
            builder.Append(GetTypeIdentity(parameter.ParameterType));
            builder.Append("|in=");
            builder.Append(parameter.IsIn ? '1' : '0');
            builder.Append("|out=");
            builder.Append(parameter.IsOut ? '1' : '0');
            builder.Append(']');
        }

        return builder.ToString();
    }

    private static int GetMethodGenericArity(MethodReference methodReference)
    {
        return methodReference switch
        {
            GenericInstanceMethod genericInstanceMethod => genericInstanceMethod.ElementMethod.GenericParameters.Count,
            _ => methodReference.GenericParameters.Count
        };
    }

    private static int GetMethodGenericArity(MethodBase methodBase)
    {
        return methodBase is not MethodInfo methodInfo
            ? 0
            : methodInfo.IsGenericMethodDefinition
                ? methodInfo.GetGenericArguments().Length
                : methodInfo.IsGenericMethod
                    ? methodInfo.GetGenericMethodDefinition().GetGenericArguments().Length
                    : 0;
    }

    private static IEnumerable<TypeReference> GetMethodGenericArguments(MethodReference methodReference)
    {
        return methodReference is GenericInstanceMethod genericInstanceMethod
            ? genericInstanceMethod.GenericArguments
            : Enumerable.Empty<TypeReference>();
    }

    private static IEnumerable<Type> GetMethodGenericArguments(MethodBase methodBase)
    {
        return methodBase is MethodInfo methodInfo && methodInfo.IsGenericMethod && !methodInfo.IsGenericMethodDefinition
            ? methodInfo.GetGenericArguments()
            : Enumerable.Empty<Type>();
    }

    private static string GetFieldIdentity(FieldReference fieldReference)
    {
        return $"{GetTypeIdentity(fieldReference.DeclaringType)}::{fieldReference.Name}|{GetTypeIdentity(fieldReference.FieldType)}";
    }

    private static string GetTypeIdentity(TypeReference typeReference)
    {
        return typeReference switch
        {
            null => string.Empty,
            ByReferenceType byReferenceType => $"{GetTypeIdentity(byReferenceType.ElementType)}&",
            PointerType pointerType => $"{GetTypeIdentity(pointerType.ElementType)}*",
            ArrayType arrayType => arrayType.Rank == 1
                ? $"{GetTypeIdentity(arrayType.ElementType)}[]"
                : $"{GetTypeIdentity(arrayType.ElementType)}[{new string(',', arrayType.Rank - 1)}]",
            RequiredModifierType requiredModifierType =>
                $"{GetTypeIdentity(requiredModifierType.ElementType)} modreq({GetTypeIdentity(requiredModifierType.ModifierType)})",
            OptionalModifierType optionalModifierType =>
                $"{GetTypeIdentity(optionalModifierType.ElementType)} modopt({GetTypeIdentity(optionalModifierType.ModifierType)})",
            GenericParameter genericParameter => $"!{genericParameter.Position}:{genericParameter.Name}",
            GenericInstanceType genericInstanceType => $"{GetTypeIdentity(genericInstanceType.ElementType)}<{string.Join(",", genericInstanceType.GenericArguments.Select(GetTypeIdentity))}>",
            SentinelType sentinelType => $"{GetTypeIdentity(sentinelType.ElementType)} sentinel",
            PinnedType pinnedType => $"{GetTypeIdentity(pinnedType.ElementType)} pinned",
            _ => typeReference.FullName
        };
    }

    private static string GetTypeIdentity(Type type)
    {
        if (type == null) return string.Empty;
        if (type.IsByRef) return $"{GetTypeIdentity(type.GetElementType())}&";
        if (type.IsPointer) return $"{GetTypeIdentity(type.GetElementType())}*";
        if (type.IsArray)
        {
            return type.GetArrayRank() == 1
                ? $"{GetTypeIdentity(type.GetElementType())}[]"
                : $"{GetTypeIdentity(type.GetElementType())}[{new string(',', type.GetArrayRank() - 1)}]";
        }

        if (type.IsGenericParameter)
        {
            return $"!{type.GenericParameterPosition}:{type.Name}";
        }

        if (type.IsGenericType && !type.IsGenericTypeDefinition)
        {
            var generic_definition = type.GetGenericTypeDefinition();
            return $"{generic_definition.FullName}<{string.Join(",", type.GetGenericArguments().Select(GetTypeIdentity))}>";
        }

        return type.FullName ?? type.Name;
    }

    private static string ToReflectionTypeName(string fullName)
    {
        return fullName.Replace('/', '+');
    }

    private static Assembly FindLoadedAssembly(string assemblyName)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(assembly => assembly.GetName().Name == assemblyName);
    }

    private static Type FindLoadedType(string fullName)
    {
        var reflection_name = ToReflectionTypeName(fullName);
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var runtime_type = assembly.GetType(reflection_name, false);
            if (runtime_type != null)
            {
                return runtime_type;
            }
        }

        return null;
    }

    private sealed class RuntimeGenericContext
    {
        public RuntimeGenericContext(Type[] typeArguments, Type[] methodArguments)
        {
            TypeArguments = typeArguments ?? Array.Empty<Type>();
            MethodArguments = methodArguments ?? Array.Empty<Type>();
        }

        public Type[] TypeArguments { get; }
        public Type[] MethodArguments { get; }
    }

    private static RuntimeGenericContext CreateRuntimeGenericContext(TypeReference declaringTypeReference,
        MethodReference methodReference = null)
    {
        Type[] type_arguments = Array.Empty<Type>();
        if (declaringTypeReference is GenericInstanceType generic_instance_type)
        {
            type_arguments = generic_instance_type.GenericArguments
                .Select(argument => ResolveRuntimeType(argument))
                .ToArray();
        }

        Type[] method_arguments = Array.Empty<Type>();
        if (methodReference is GenericInstanceMethod generic_instance_method)
        {
            method_arguments = generic_instance_method.GenericArguments
                .Select(argument => ResolveRuntimeType(argument))
                .ToArray();
        }

        return type_arguments.Length == 0 && method_arguments.Length == 0
            ? null
            : new RuntimeGenericContext(type_arguments, method_arguments);
    }

    private static Type ResolveRuntimeType(TypeReference typeReference)
    {
        return ResolveRuntimeType(typeReference, null);
    }

    private static Type ResolveRuntimeType(TypeReference typeReference, RuntimeGenericContext genericContext)
    {
        if (typeReference == null) return null;

        switch (typeReference)
        {
            case ByReferenceType byReferenceType:
            {
                var elementType = ResolveRuntimeType(byReferenceType.ElementType, genericContext);
                return elementType?.MakeByRefType();
            }
            case PointerType pointerType:
            {
                var elementType = ResolveRuntimeType(pointerType.ElementType, genericContext);
                return elementType?.MakePointerType();
            }
            case ArrayType arrayType:
            {
                var elementType = ResolveRuntimeType(arrayType.ElementType, genericContext);
                if (elementType == null) return null;
                return arrayType.Rank == 1 ? elementType.MakeArrayType() : elementType.MakeArrayType(arrayType.Rank);
            }
            case RequiredModifierType requiredModifierType:
                return ResolveRuntimeType(requiredModifierType.ElementType, genericContext);
            case OptionalModifierType optionalModifierType:
                return ResolveRuntimeType(optionalModifierType.ElementType, genericContext);
            case PinnedType pinnedType:
                return ResolveRuntimeType(pinnedType.ElementType, genericContext);
            case SentinelType sentinelType:
                return ResolveRuntimeType(sentinelType.ElementType, genericContext);
            case GenericInstanceType genericInstanceType:
            {
                var genericType = ResolveRuntimeType(genericInstanceType.ElementType, genericContext);
                if (genericType == null) return null;
                var genericArguments = genericInstanceType.GenericArguments
                    .Select(argument => ResolveRuntimeType(argument, genericContext))
                    .ToArray();
                return genericArguments.Any(argument => argument == null)
                    ? null
                    : genericType.MakeGenericType(genericArguments);
            }
            case GenericParameter genericParameter:
            {
                if (genericParameter.Type == GenericParameterType.Type
                    && genericParameter.Position < genericContext?.TypeArguments.Length)
                {
                    return genericContext.TypeArguments[genericParameter.Position];
                }

                if (genericParameter.Type == GenericParameterType.Method
                    && genericParameter.Position < genericContext?.MethodArguments.Length)
                {
                    return genericContext.MethodArguments[genericParameter.Position];
                }

                return null;
            }
        }

        var runtimeAssembly = _mod?.GetType().Assembly;
        var runtimeType = runtimeAssembly?.GetType(ToReflectionTypeName(typeReference.FullName));
        if (runtimeType != null) return runtimeType;

        runtimeType = AccessTools.TypeByName(ToReflectionTypeName(typeReference.FullName));
        if (runtimeType != null) return runtimeType;

        runtimeType = _new_runtime_assembly?.GetType(ToReflectionTypeName(typeReference.FullName));
        if (runtimeType != null) return runtimeType;

        runtimeType = FindLoadedType(typeReference.FullName);
        if (runtimeType != null) return runtimeType;

        try
        {
            var resolved_type = typeReference.Resolve();
            if (resolved_type == null) return null;
            var loaded_assembly = FindLoadedAssembly(resolved_type.Module.Assembly.Name.Name);
            return loaded_assembly?.GetType(ToReflectionTypeName(resolved_type.FullName), false);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static bool RuntimeTypeMatches(TypeReference typeReference, Type runtimeType, RuntimeGenericContext genericContext)
    {
        if (runtimeType == null) return false;

        var resolved_runtime_type = ResolveRuntimeType(typeReference, genericContext);
        if (resolved_runtime_type != null)
        {
            return GetTypeIdentity(resolved_runtime_type) == GetTypeIdentity(runtimeType);
        }

        return GetTypeIdentity(typeReference) == GetTypeIdentity(runtimeType);
    }

    private static bool RuntimeParameterMatches(ParameterDefinition parameterDefinition, ParameterInfo parameterInfo,
        RuntimeGenericContext genericContext, bool requireParameterDirection)
    {
        if (!RuntimeTypeMatches(parameterDefinition.ParameterType, parameterInfo.ParameterType, genericContext))
        {
            return false;
        }

        if (!requireParameterDirection)
        {
            return true;
        }

        return parameterDefinition.IsIn == parameterInfo.IsIn
               && parameterDefinition.IsOut == parameterInfo.IsOut;
    }

    private static MethodInfo CloseRuntimeGenericMethod(MethodReference methodReference, MethodInfo methodInfo,
        RuntimeGenericContext genericContext)
    {
        if (methodReference is not GenericInstanceMethod genericInstanceMethod)
        {
            return methodInfo;
        }

        if (!methodInfo.IsGenericMethodDefinition)
        {
            return methodInfo.IsGenericMethod ? methodInfo : null;
        }

        var runtime_method_arguments = genericInstanceMethod.GenericArguments
            .Select(argument => ResolveRuntimeType(argument, genericContext))
            .ToArray();
        if (runtime_method_arguments.Any(argument => argument == null))
        {
            return null;
        }

        try
        {
            return methodInfo.MakeGenericMethod(runtime_method_arguments);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static bool RuntimeMethodMatches(MethodReference methodReference, MethodBase methodBase,
        bool requireParameterDirection = true)
    {
        if (methodBase == null || methodBase.Name != methodReference.Name) return false;

        var runtime_generic_arity = GetMethodGenericArity(methodBase);
        if (runtime_generic_arity != GetMethodGenericArity(methodReference)) return false;

        var generic_context = CreateRuntimeGenericContext(methodReference.DeclaringType, methodReference);
        if (methodReference is GenericInstanceMethod generic_instance_method)
        {
            if (methodBase is not MethodInfo generic_runtime_method
                || !generic_runtime_method.IsGenericMethod
                || generic_runtime_method.IsGenericMethodDefinition)
            {
                return false;
            }

            var runtime_method_arguments = generic_runtime_method.GetGenericArguments();
            if (generic_instance_method.GenericArguments.Count != runtime_method_arguments.Length)
            {
                return false;
            }

            for (var i = 0; i < runtime_method_arguments.Length; i++)
            {
                if (!RuntimeTypeMatches(generic_instance_method.GenericArguments[i], runtime_method_arguments[i], generic_context))
                {
                    return false;
                }
            }
        }

        if (methodBase is MethodInfo runtime_method
            && !RuntimeTypeMatches(methodReference.ReturnType, runtime_method.ReturnType, generic_context))
        {
            return false;
        }

        var reference_parameters = methodReference.Parameters;
        var runtime_parameters = methodBase.GetParameters();
        if (reference_parameters.Count != runtime_parameters.Length) return false;

        for (int i = 0; i < reference_parameters.Count; i++)
        {
            if (!RuntimeParameterMatches(reference_parameters[i], runtime_parameters[i], generic_context,
                    requireParameterDirection))
            {
                return false;
            }
        }

        return true;
    }

    private static MethodBase ResolveRuntimeMethodCandidate(MethodReference methodReference,
        IEnumerable<MethodBase> candidates)
    {
        var exact_matches = candidates
            .Where(candidate => candidate != null && RuntimeMethodMatches(methodReference, candidate))
            .Take(2)
            .ToList();
        if (exact_matches.Count == 1)
        {
            return exact_matches[0];
        }

        if (exact_matches.Count > 1)
        {
            return null;
        }

        var relaxed_matches = candidates
            .Where(candidate => candidate != null && RuntimeMethodMatches(methodReference, candidate, false))
            .Take(2)
            .ToList();
        return relaxed_matches.Count == 1
            ? relaxed_matches[0]
            : null;
    }

    private static bool RuntimeFieldMatches(FieldReference fieldReference, FieldInfo fieldInfo)
    {
        if (fieldInfo == null || fieldInfo.Name != fieldReference.Name) return false;
        return RuntimeTypeMatches(fieldReference.FieldType, fieldInfo.FieldType,
            CreateRuntimeGenericContext(fieldReference.DeclaringType));
    }

    private static Type ResolveRuntimeThisType(MethodDefinition methodDefinition)
    {
        if (methodDefinition.IsStatic) return null;

        var declaringType = ResolveRuntimeType(methodDefinition.DeclaringType);
        if (declaringType == null) return null;

        return declaringType.IsValueType ? declaringType.MakeByRefType() : declaringType;
    }

    private static Type EnsureClosedRuntimeType(Type runtimeType, string context)
    {
        if (runtimeType == null)
        {
            throw new InvalidOperationException($"Failed to resolve runtime type for {context}");
        }

        if (runtimeType.ContainsGenericParameters)
        {
            throw new InvalidOperationException($"Resolved open runtime type {runtimeType} for {context}");
        }

        return runtimeType;
    }

    private static MemberInfo EnsureClosedRuntimeMember(MemberInfo memberInfo, string context)
    {
        if (memberInfo == null)
        {
            throw new InvalidOperationException($"Failed to resolve runtime member for {context}");
        }

        switch (memberInfo)
        {
            case MethodBase methodBase when methodBase.ContainsGenericParameters
                                            || methodBase.DeclaringType?.ContainsGenericParameters == true:
                throw new InvalidOperationException($"Resolved open runtime method {methodBase} for {context}");
            case FieldInfo fieldInfo when fieldInfo.FieldType.ContainsGenericParameters
                                         || fieldInfo.DeclaringType?.ContainsGenericParameters == true:
                throw new InvalidOperationException($"Resolved open runtime field {fieldInfo} for {context}");
        }

        return memberInfo;
    }

    private static MethodInfo ResolveOldRuntimeMethod(MethodDefinition oldMethodDefinition)
    {
        return _mod?.GetType().Assembly.ManifestModule.ResolveMethod(oldMethodDefinition.MetadataToken.ToInt32()) as MethodInfo;
    }

    private static FieldInfo ResolveOldRuntimeField(FieldDefinition oldFieldDefinition)
    {
        return _mod?.GetType().Assembly.ManifestModule.ResolveField(oldFieldDefinition.MetadataToken.ToInt32()) as FieldInfo;
    }

    private static bool TryGetOldMethod(MethodDefinition methodDefinition, out MethodDefinition oldMethodDefinition)
    {
        return _old_method_definitions_by_identity.TryGetValue(GetMethodIdentity(methodDefinition), out oldMethodDefinition);
    }

    private static bool TryGetOldField(FieldReference fieldReference, out FieldDefinition oldFieldDefinition)
    {
        return _old_field_definitions_by_identity.TryGetValue(GetFieldIdentity(fieldReference), out oldFieldDefinition);
    }

    private static bool HasMethodBodyChanged(MethodDefinition oldMethodDefinition, MethodDefinition newMethodDefinition)
    {
        if (!oldMethodDefinition.HasBody || !newMethodDefinition.HasBody)
        {
            return oldMethodDefinition.HasBody != newMethodDefinition.HasBody;
        }

        return GetMethodBodyFingerprint(oldMethodDefinition) != GetMethodBodyFingerprint(newMethodDefinition);
    }

    private static string GetMethodBodyFingerprint(MethodDefinition methodDefinition)
    {
        var builder = new StringBuilder();
        builder.Append("init:");
        builder.Append(methodDefinition.Body.InitLocals ? '1' : '0');
        builder.AppendLine();

        foreach (var variable in methodDefinition.Body.Variables)
        {
            builder.Append("local:");
            builder.Append(GetTypeIdentity(variable.VariableType));
            builder.AppendLine();
        }

        var instructions = methodDefinition.Body.Instructions;
        foreach (var instruction in instructions)
        {
            builder.Append("inst:");
            builder.Append(instruction.OpCode);
            builder.Append('|');
            builder.Append(GetOperandFingerprint(instruction, instructions));
            builder.AppendLine();
        }

        foreach (var handler in methodDefinition.Body.ExceptionHandlers)
        {
            builder.Append("eh:");
            builder.Append(handler.HandlerType);
            builder.Append('|');
            builder.Append(GetInstructionIndex(instructions, handler.TryStart));
            builder.Append('|');
            builder.Append(GetInstructionIndex(instructions, handler.TryEnd));
            builder.Append('|');
            builder.Append(GetInstructionIndex(instructions, handler.HandlerStart));
            builder.Append('|');
            builder.Append(GetInstructionIndex(instructions, handler.HandlerEnd));
            builder.Append('|');
            builder.Append(GetInstructionIndex(instructions, handler.FilterStart));
            builder.Append('|');
            builder.Append(GetTypeIdentity(handler.CatchType));
            builder.AppendLine();
        }

        return builder.ToString();
    }

    private static string GetOperandFingerprint(Instruction instruction, Mono.Collections.Generic.Collection<Instruction> instructions)
    {
        return instruction.Operand switch
        {
            null => string.Empty,
            Instruction targetInstruction => $"target:{GetInstructionIndex(instructions, targetInstruction)}",
            Instruction[] targetInstructions => $"targets:{string.Join(",", targetInstructions.Select(target => GetInstructionIndex(instructions, target)))}",
            MethodReference methodReference => $"method:{GetMethodIdentity(methodReference)}",
            FieldReference fieldReference => $"field:{GetFieldIdentity(fieldReference)}",
            TypeReference typeReference => $"type:{GetTypeIdentity(typeReference)}",
            ParameterDefinition parameterDefinition => $"param:{parameterDefinition.Index}:{GetTypeIdentity(parameterDefinition.ParameterType)}",
            VariableReference variableReference => $"var:{variableReference.Index}:{GetTypeIdentity(variableReference.VariableType)}",
            Mono.Cecil.CallSite callSite => $"callsite:{GetTypeIdentity(callSite.ReturnType)}({string.Join(",", callSite.Parameters.Select(parameter => GetTypeIdentity(parameter.ParameterType)))})",
            string stringValue => $"string:{stringValue}",
            _ => instruction.Operand.ToString()
        };
    }

    private static int GetInstructionIndex(Mono.Collections.Generic.Collection<Instruction> instructions, Instruction instruction)
    {
        if (instruction == null) return -1;
        return instructions.IndexOf(instruction);
    }

    private static MemberInfo ResolveRuntimeMember(MemberReference memberReference)
    {
        switch (memberReference)
        {
            case MethodReference methodReference:
            {
                var generic_context = CreateRuntimeGenericContext(methodReference.DeclaringType, methodReference);
                MethodDefinition resolvedMethod = null;
                try
                {
                    resolvedMethod = methodReference.Resolve();
                }
                catch (Exception)
                {
                    // ignored
                }

                if (resolvedMethod != null
                    && _generated_methods_by_definition.TryGetValue(resolvedMethod, out var generatedMethod))
                {
                    return generatedMethod;
                }

                var is_generic_instance_reference = methodReference.DeclaringType is GenericInstanceType
                                                    || methodReference is GenericInstanceMethod;
                if (resolvedMethod != null
                    && !is_generic_instance_reference
                    && !resolvedMethod.HasGenericParameters
                    && !resolvedMethod.DeclaringType.HasGenericParameters
                    && resolvedMethod.Module.Assembly.Name.Name == _old_assembly_definition.MainModule.Assembly.Name.Name)
                {
                    if (TryGetOldMethod(resolvedMethod, out var oldMethodDefinition))
                    {
                        return ResolveOldRuntimeMethod(oldMethodDefinition);
                    }
                }

                var declaring_runtime_type = ResolveRuntimeType(methodReference.DeclaringType, generic_context);
                if (declaring_runtime_type == null) return null;

                try
                {
                    if (methodReference.Name == ".ctor" || methodReference.Name == ".cctor")
                    {
                        return ResolveRuntimeMethodCandidate(
                            methodReference,
                            declaring_runtime_type.GetConstructors(BindingFlags.Instance | BindingFlags.Static |
                                                                   BindingFlags.Public | BindingFlags.NonPublic)
                                .Cast<MethodBase>()
                        );
                    }

                    return ResolveRuntimeMethodCandidate(
                        methodReference,
                        declaring_runtime_type.GetMethods(BindingFlags.Instance | BindingFlags.Static |
                                                          BindingFlags.Public | BindingFlags.NonPublic)
                            .Select(method => CloseRuntimeGenericMethod(methodReference, method, generic_context))
                            .Cast<MethodBase>()
                    );
                }
                catch (Exception)
                {
                    if (resolvedMethod == null) return null;
                }

                try
                {
                    if (resolvedMethod != null)
                    {
                        var loaded_assembly = FindLoadedAssembly(resolvedMethod.Module.Assembly.Name.Name);
                        return loaded_assembly?.ManifestModule.ResolveMethod(resolvedMethod.MetadataToken.ToInt32()) as MemberInfo;
                    }
                }
                catch (Exception)
                {
                    // ignored
                }

                return null;
            }
            case FieldReference fieldReference:
            {
                var generic_context = CreateRuntimeGenericContext(fieldReference.DeclaringType);
                FieldDefinition resolvedField = null;
                try
                {
                    resolvedField = fieldReference.Resolve();
                }
                catch (Exception)
                {
                    // ignored
                }

                if (resolvedField != null
                    && resolvedField.Module.Assembly.Name.Name == _old_assembly_definition.MainModule.Assembly.Name.Name
                    && TryGetOldField(fieldReference, out var oldFieldDefinition))
                {
                    return ResolveOldRuntimeField(oldFieldDefinition);
                }

                var declaring_runtime_type = ResolveRuntimeType(fieldReference.DeclaringType, generic_context);
                if (declaring_runtime_type != null)
                {
                    var current_type = declaring_runtime_type;
                    while (current_type != null)
                    {
                        var runtime_field = current_type
                            .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                       BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                            .FirstOrDefault(field => RuntimeFieldMatches(fieldReference, field));
                        if (runtime_field != null)
                        {
                            return runtime_field;
                        }

                        current_type = current_type.BaseType;
                    }
                }

                try
                {
                    if (resolvedField != null)
                    {
                        var loaded_assembly = FindLoadedAssembly(resolvedField.Module.Assembly.Name.Name);
                        return loaded_assembly?.ManifestModule.ResolveField(resolvedField.MetadataToken.ToInt32()) as MemberInfo;
                    }
                }
                catch (Exception)
                {
                    // ignored
                }

                return null;
            }
            default:
                return null;
        }
    }

    private static HashSet<MethodDefinition> CollectHotfixTargets(IEnumerable<MethodDefinition> methodDefinitions)
    {
        var targets = new HashSet<MethodDefinition>();
        var queue = new Queue<MethodDefinition>(methodDefinitions.Where(IsHotfixable));

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!targets.Add(current)) continue;
            if (!current.HasBody) continue;

            foreach (var instruction in current.Body.Instructions)
            {
                if (instruction.Operand is not MethodReference methodReference) continue;

                MethodDefinition resolvedMethod;
                try
                {
                    resolvedMethod = methodReference.Resolve();
                }
                catch (Exception)
                {
                    continue;
                }

                if (resolvedMethod == null || resolvedMethod.IsConstructor || !resolvedMethod.HasBody) continue;
                if (resolvedMethod.Module.Assembly.Name.Name != current.Module.Assembly.Name.Name) continue;
                if (!IsCompilerGenerated(resolvedMethod)) continue;

                queue.Enqueue(resolvedMethod);
            }
        }

        return targets;
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

    public static bool PatchHotfixMethodsNT()
    {
        if (_op_code_map.Count == 0)
        {
            InitializeOpcodeMap();
        }

        var assembly_bytes = File.ReadAllBytes(_new_compiled_dll_path);
        var pdb_bytes = File.ReadAllBytes(_new_compiled_pdb_path);
        _new_runtime_assembly = Assembly.Load(assembly_bytes, pdb_bytes);
        using var f_stream = new MemoryStream(assembly_bytes);
        using var p_stream = new MemoryStream(pdb_bytes);
        var assembly_definition = AssemblyDefinition.ReadAssembly(f_stream, new ReaderParameters
        {
            ReadSymbols = true,
            SymbolStream = p_stream,
            SymbolReaderProvider = new PdbReaderProvider()
        });

        var method_definitions = EnumerateTypesRecursive(assembly_definition.MainModule.Types)
            .SelectMany(type => type.Methods)
            .Where(method => method.HasBody)
            .ToList();

        var changed_hotfix_roots = new List<MethodDefinition>();
        foreach (var hotfixable_method in method_definitions.Where(IsHotfixable))
        {
            if (TryGetOldMethod(hotfixable_method, out var old_hotfixable_method)
                && !HasMethodBodyChanged(old_hotfixable_method, hotfixable_method))
            {
                continue;
            }

            if (IsGenericHotfixUnsupported(hotfixable_method))
            {
                LogService.LogWarning(
                    $"Skip hotfix method {hotfixable_method.FullName} because generic hotfix is not supported.");
                continue;
            }

            changed_hotfix_roots.Add(hotfixable_method);
        }

        HashSet<MethodDefinition> methods_to_create = new();
        List<(MethodInfo OldMethod, MethodDefinition NewMethod)> methods_to_replace = new();

        foreach (var new_method in CollectHotfixTargets(changed_hotfix_roots))
        {
            if (IsGenericHotfixUnsupported(new_method))
            {
                LogService.LogWarning($"Skip hotfix method {new_method.FullName} because generic hotfix is not supported.");
                continue;
            }

            if (TryGetOldMethod(new_method, out var old_method_definition))
            {
                if (!HasMethodBodyChanged(old_method_definition, new_method))
                {
                    continue;
                }

                var old_method = ResolveOldRuntimeMethod(old_method_definition);
                if (old_method == null)
                {
                    LogService.LogWarning($"Failed to resolve runtime method for {new_method.FullName}");
                    continue;
                }

                methods_to_replace.Add((old_method, new_method));
                continue;
            }

            var declaring_type = _mod.GetType().Assembly.GetType(ToReflectionTypeName(new_method.DeclaringType.FullName));
            if (declaring_type == null)
            {
                LogService.LogWarning(
                    $"Skip generated helper {new_method.FullName} because declaring type {new_method.DeclaringType.FullName} does not exist in the loaded assembly.");
                continue;
            }

            methods_to_create.Add(new_method);
        }

        var methods_to_generate = methods_to_create
            .Concat(methods_to_replace.Select(item => item.NewMethod))
            .Distinct()
            .ToList();

        Dictionary<MethodDefinition, MethodInfo> generated_methods = new();
        bool success = true;
        if (methods_to_generate.Count > 0)
        {
            try
            {
                generated_methods = GenerateHotfixMethods(methods_to_generate);
            }
            catch (Exception e)
            {
                _generated_methods_by_definition.Clear();
                LogService.LogError("Failed to generate hotfix methods.");
                LogService.LogError(e.ToString());
                assembly_definition.Dispose();
                return false;
            }
        }

        List<MethodInfo> applied_methods = new();
        foreach (var (old_method, new_method) in methods_to_replace)
        {
            if (!generated_methods.TryGetValue(new_method, out var replacement_method))
            {
                LogService.LogError($"Failed to locate generated replacement for {new_method.FullName}");
                success = false;
                continue;
            }

            try
            {
                ApplyReplacement(old_method, replacement_method);
                applied_methods.Add(old_method);
            }
            catch (Exception e)
            {
                LogService.LogError($"Failed to hotfix method {new_method.FullName}");
                LogService.LogError(e.ToString());
                success = false;
            }
        }

        if (!success)
        {
            foreach (var old_method in applied_methods)
            {
                if (_replacement_detours.TryGetValue(old_method, out var detour))
                {
                    detour.Dispose();
                    _replacement_detours.Remove(old_method);
                }

                _replacement_methods.Remove(old_method);
            }
        }

        assembly_definition.Dispose();
        return success;
    }

    private static Type[] GetGeneratedParameterTypes(MethodDefinition methodDefinition)
    {
        var parameter_types = new List<Type>(methodDefinition.Parameters.Count + (methodDefinition.IsStatic ? 0 : 1));
        if (!methodDefinition.IsStatic)
        {
            parameter_types.Add(EnsureClosedRuntimeType(
                ResolveRuntimeThisType(methodDefinition),
                $"this parameter of {methodDefinition.FullName}"
            ));
        }

        foreach (var parameter in methodDefinition.Parameters)
        {
            parameter_types.Add(EnsureClosedRuntimeType(
                ResolveRuntimeType(parameter.ParameterType),
                $"parameter type {parameter.ParameterType.FullName} of {methodDefinition.FullName}"
            ));
        }

        return parameter_types.ToArray();
    }

    private static Module ResolveHotfixAccessModule(MethodDefinition methodDefinition)
    {
        if (TryGetOldMethod(methodDefinition, out var old_method_definition))
        {
            var old_runtime_method = ResolveOldRuntimeMethod(old_method_definition);
            if (old_runtime_method != null)
            {
                return old_runtime_method.Module;
            }
        }

        var declaring_type = ResolveRuntimeType(methodDefinition.DeclaringType);
        if (declaring_type != null)
        {
            return declaring_type.Module;
        }

        return _mod?.GetType().Assembly.ManifestModule
               ?? typeof(ModReloadUtils).Module;
    }

    private static void ApplyGeneratedParameterMetadata(MethodBuilder methodBuilder, MethodDefinition methodDefinition)
    {
        int parameter_position = 1;
        if (!methodDefinition.IsStatic)
        {
            methodBuilder.DefineParameter(parameter_position++, System.Reflection.ParameterAttributes.None, "__this");
        }

        foreach (var parameter in methodDefinition.Parameters)
        {
            var parameter_attributes = System.Reflection.ParameterAttributes.None;
            if (parameter.IsIn) parameter_attributes |= System.Reflection.ParameterAttributes.In;
            if (parameter.IsOut) parameter_attributes |= System.Reflection.ParameterAttributes.Out;
            if (parameter.IsOptional) parameter_attributes |= System.Reflection.ParameterAttributes.Optional;

            methodBuilder.DefineParameter(parameter_position++, parameter_attributes, parameter.Name);
        }
    }

    private static Type CreateHotfixDelegateType(ModuleBuilder moduleBuilder, MethodDefinition methodDefinition,
        Type returnType, Type[] parameterTypes, string typeName)
    {
        var delegate_type_builder = moduleBuilder.DefineType(
            typeName,
            System.Reflection.TypeAttributes.Class | System.Reflection.TypeAttributes.Public |
            System.Reflection.TypeAttributes.Sealed | System.Reflection.TypeAttributes.AutoClass |
            System.Reflection.TypeAttributes.AnsiClass,
            typeof(MulticastDelegate)
        );

        var ctor_builder = delegate_type_builder.DefineConstructor(
            System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.HideBySig |
            System.Reflection.MethodAttributes.RTSpecialName | System.Reflection.MethodAttributes.SpecialName,
            CallingConventions.Standard,
            new[] { typeof(object), typeof(IntPtr) }
        );
        ctor_builder.SetImplementationFlags(System.Reflection.MethodImplAttributes.Runtime |
                                           System.Reflection.MethodImplAttributes.Managed);

        var invoke_builder = delegate_type_builder.DefineMethod(
            "Invoke",
            System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.HideBySig |
            System.Reflection.MethodAttributes.NewSlot | System.Reflection.MethodAttributes.Virtual,
            returnType,
            parameterTypes
        );
        ApplyGeneratedParameterMetadata(invoke_builder, methodDefinition);
        invoke_builder.SetImplementationFlags(System.Reflection.MethodImplAttributes.Runtime |
                                             System.Reflection.MethodImplAttributes.Managed);

        return delegate_type_builder.CreateType();
    }

    private static MethodInfo CreateHotfixWrapperMethod(MethodDefinition methodDefinition, DynamicMethod dynamicMethod,
        int wrapperIndex)
    {
        var assembly_name = new AssemblyName($"NeoModLoader.HotfixWrappers.{System.Guid.NewGuid():N}");
        var assembly_builder = AssemblyBuilder.DefineDynamicAssembly(assembly_name, AssemblyBuilderAccess.Run);
        var module_builder = assembly_builder.DefineDynamicModule("HotfixWrappers");
        var parameter_types = GetGeneratedParameterTypes(methodDefinition);
        var return_type = ResolveRuntimeType(methodDefinition.ReturnType)
                          ?? throw new InvalidOperationException(
                              $"Failed to resolve return type for {methodDefinition.FullName}");

        var delegate_type = CreateHotfixDelegateType(
            module_builder,
            methodDefinition,
            return_type,
            parameter_types,
            $"HotfixDelegate_{wrapperIndex}"
        );

        var wrapper_type_builder = module_builder.DefineType(
            $"NeoModLoader.Hotfix.GeneratedWrapper_{wrapperIndex}_{System.Guid.NewGuid():N}",
            System.Reflection.TypeAttributes.Class | System.Reflection.TypeAttributes.Public |
            System.Reflection.TypeAttributes.Abstract | System.Reflection.TypeAttributes.Sealed
        );

        const string delegate_field_name = "_delegate";
        var delegate_field_builder = wrapper_type_builder.DefineField(
            delegate_field_name,
            delegate_type,
            System.Reflection.FieldAttributes.Private | System.Reflection.FieldAttributes.Static
        );

        var wrapper_method_name = $"Hotfix_{wrapperIndex}";
        var wrapper_method_builder = wrapper_type_builder.DefineMethod(
            wrapper_method_name,
            System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.Static,
            return_type,
            parameter_types
        );
        ApplyGeneratedParameterMetadata(wrapper_method_builder, methodDefinition);

        var wrapper_il = wrapper_method_builder.GetILGenerator();
        wrapper_il.Emit(OpCodes.Ldsfld, delegate_field_builder);
        for (var i = 0; i < parameter_types.Length; i++)
        {
            wrapper_il.Emit(OpCodes.Ldarg, i);
        }

        wrapper_il.Emit(OpCodes.Callvirt, delegate_type.GetMethod("Invoke"));
        wrapper_il.Emit(OpCodes.Ret);

        var wrapper_type = wrapper_type_builder.CreateType();
        var delegate_instance = dynamicMethod.CreateDelegate(delegate_type);
        wrapper_type.GetField(delegate_field_name, BindingFlags.Static | BindingFlags.NonPublic)
            ?.SetValue(null, delegate_instance);

        _generated_wrapper_delegates.Add(delegate_instance);
        _generated_wrapper_types.Add(wrapper_type);

        return wrapper_type.GetMethod(wrapper_method_name, BindingFlags.Static | BindingFlags.Public)
               ?? throw new InvalidOperationException(
                   $"Failed to build wrapper method for {methodDefinition.FullName}");
    }

    private static Dictionary<MethodDefinition, MethodInfo> GenerateHotfixMethods(IReadOnlyCollection<MethodDefinition> methodDefinitions)
    {
        _generated_methods_by_definition.Clear();
        if (methodDefinitions.Count == 0)
        {
            return new Dictionary<MethodDefinition, MethodInfo>();
        }

        var generated_methods = new Dictionary<MethodDefinition, MethodInfo>(methodDefinitions.Count);
        int method_index = 0;
        foreach (var method_definition in methodDefinitions)
        {
            var return_type = EnsureClosedRuntimeType(
                ResolveRuntimeType(method_definition.ReturnType),
                $"return type of {method_definition.FullName}"
            );
            var parameter_types = GetGeneratedParameterTypes(method_definition);
            var dynamic_method = new DynamicMethod(
                $"Hotfix_{method_index++}",
                return_type,
                parameter_types
                , ResolveHotfixAccessModule(method_definition)
                , true
            );
            dynamic_method.InitLocals = method_definition.Body.InitLocals;

            _generated_methods_by_definition[method_definition] = dynamic_method;
        }

        foreach (var generated_method in _generated_methods_by_definition)
        {
            EmitMethodBody(generated_method.Key, ((DynamicMethod)generated_method.Value).GetILGenerator());
        }

        int wrapper_index = 0;
        foreach (var generated_method in _generated_methods_by_definition)
        {
            generated_methods[generated_method.Key] = CreateHotfixWrapperMethod(
                generated_method.Key,
                (DynamicMethod)generated_method.Value,
                wrapper_index++
            );
        }

        return generated_methods;
    }

    private static void ApplyReplacement(MethodInfo oldMethod, MethodInfo newMethod)
    {
        if (newMethod.ContainsGenericParameters || newMethod.DeclaringType?.ContainsGenericParameters == true)
        {
            throw new InvalidOperationException(
                $"Replacement method {newMethod} still contains open generic parameters.");
        }

        if (_replacement_detours.TryGetValue(oldMethod, out var existing_detour))
        {
            existing_detour.Dispose();
        }

        _replacement_methods[oldMethod] = newMethod;
        _replacement_detours[oldMethod] = new Detour(oldMethod, newMethod);
    }

    private static void EmitMethodBody(MethodDefinition methodDefinition, ILGenerator il)
    {
        var local_builders = new LocalBuilder[methodDefinition.Body.Variables.Count];
        foreach (var local_var in methodDefinition.Body.Variables)
        {
            var local_type = EnsureClosedRuntimeType(
                ResolveRuntimeType(local_var.VariableType),
                $"local {local_var.Index}:{local_var.VariableType.FullName} of {methodDefinition.FullName}"
            );
            local_builders[local_var.Index] = il.DeclareLocal(local_type, local_var.IsPinned);
        }

        var labels = new Dictionary<Instruction, Label>();
        // Track labels
        foreach (var inst in methodDefinition.Body.Instructions)
        {
            if (inst.Operand is Instruction opinst)
            {
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

        foreach (var excep in methodDefinition.Body.ExceptionHandlers)
        {
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
            for (var instruction_index = 0; instruction_index < methodDefinition.Body.Instructions.Count; instruction_index++)
            {
                var inst = methodDefinition.Body.Instructions[instruction_index];
                try
                {
                if (labels.TryGetValue(inst, out var label))
                {
                    il.MarkLabel(label);
                }

                if (excep_handlers.TryGetValue(inst, out var excep_handler))
                {
                    if (inst == excep_handler.TryEnd)
                    {
                        //il.EndExceptionBlock();
                    }
                    else if (inst == excep_handler.HandlerStart)
                    {
                        /*
                        switch (excep_handler.HandlerType)
                        {
                            case ExceptionHandlerType.Catch:
                                il.BeginCatchBlock(ResolveRuntimeType(excep_handler.CatchType));
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
                        //il.EndExceptionBlock();
                    }
                    else
                    {
                        // TryStart
                        //il.MarkLabel(il.BeginExceptionBlock());
                    }
                }

                var op_code = _op_code_map[inst.OpCode];

                if (op_code == OpCodes.Endfinally) continue;

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


                if (inst.Operand is TypeReference type_reference)
                {
                    var resolved_type = EnsureClosedRuntimeType(
                        ResolveRuntimeType(type_reference),
                        $"type operand {type_reference.FullName} in {methodDefinition.FullName}"
                    );
                    il.Emit(op_code, resolved_type);
                }
                else if (inst.Operand is MemberReference member_reference)
                {
                    var resolved = EnsureClosedRuntimeMember(
                        ResolveRuntimeMember(member_reference),
                        $"{member_reference.FullName} in {methodDefinition.FullName}"
                    );

                    operand_type = resolved switch
                    {
                        ConstructorInfo => typeof(ConstructorInfo),
                        MethodInfo => typeof(MethodInfo),
                        FieldInfo => typeof(FieldInfo),
                        _ => resolved.GetType()
                    };
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

                    emit_method.Invoke(il, new object[] { op_code, resolved });
                }
                else if (inst.Operand is VariableReference variable_reference)
                {
                    if (variable_reference.Index < 0 || variable_reference.Index >= local_builders.Length)
                    {
                        throw new InvalidOperationException(
                            $"Invalid local index {variable_reference.Index} for {methodDefinition.FullName}");
                    }

                    il.Emit(op_code, local_builders[variable_reference.Index]);
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
                    var parameter_index = methodDefinition.IsStatic
                        ? parameter_definition.Index
                        : parameter_definition.Index + 1;
                    il.Emit(op_code, parameter_index);
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
                        emit_method.Invoke(il, new object[] { op_code, inst.Operand });
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
                                $"Failed to emit {op_code} {inst.Operand}({inst.Operand?.GetType().FullName}) in {methodDefinition.FullName}");
                            LogService.LogError(e.Message);
                            LogService.LogError(e.StackTrace);
                        }
                    }
                }
                }
                catch (Exception instructionException)
                {
                    throw new InvalidOperationException(
                        $"Failed to emit instruction #{instruction_index} {inst.OpCode} {GetOperandFingerprint(inst, methodDefinition.Body.Instructions)} in {methodDefinition.FullName}",
                        instructionException);
                }
            }
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Failed to emit method body for {methodDefinition.FullName}", e);
        }
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
