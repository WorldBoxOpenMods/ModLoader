using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Mono.Cecil;
using MonoMod.Utils;
using NeoModLoader.api;
using NeoModLoader.api.attributes;
using NeoModLoader.constants;
using NeoModLoader.services;

namespace NeoModLoader.utils;

internal static class ModReloadUtils
{
    private static IMod _mod;
    private static ModDeclare _mod_declare;
    private static string _new_compiled_dll_path;
    private static string _new_compiled_pdb_path;
    public static bool Prepare(IMod pMod)
    {
        _mod = pMod;
        _mod_declare = pMod.GetDeclaration();
        return true;
    }

    public static bool CompileNew()
    {
        if (ModCompileLoadService.TryCompileModAtRuntime(_mod_declare, true))
        {
            _new_compiled_dll_path = Path.Combine(Paths.CompiledModsPath, $"{_mod_declare.UID}.dll");
            _new_compiled_pdb_path = Path.Combine(Paths.CompiledModsPath, $"{_mod_declare.UID}.pdb");
            return true;
        }
        return false;
    }
    private static Dictionary<Mono.Cecil.Cil.OpCode, OpCode> _op_code_map = new();
    public static bool PatchHotfixMethods()
    {
        AssemblyDefinition assembly_definition = AssemblyDefinition.ReadAssembly(_new_compiled_dll_path);
        MethodDefinition[] method_definitions = assembly_definition.MainModule.Types.SelectMany(type => type.Methods).ToArray();

        Assembly old_assembly = _mod.GetType().Assembly;
        
        Harmony harmony = new Harmony(_mod_declare.UID);

        if (_op_code_map.Count == 0)
        {
            InitializeOpcodeMap();
        }
        foreach (var new_method in method_definitions)
        {
            if(new_method.HasBody is false)
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
            
            try
            {
                MethodInfo old_method = old_assembly.GetType(new_method.DeclaringType.FullName).GetMethod(new_method.Name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (old_method == null)
                {
                    LogService.LogWarning($"No found method {new_method.DeclaringType.FullName}::{new_method.Name} in old assembly");
                    continue;
                }
                HotfixMethod(harmony, new_method, old_method);
            }
            catch (Exception e)
            {
                LogService.LogError($"Failed to hotfix method {new_method.Name}");
                LogService.LogError(e.Message);
                LogService.LogError(e.StackTrace);
                continue;
            }
        }
        return false;
    }

    private static void InitializeOpcodeMap()
    {
        foreach (var field in typeof(OpCodes).GetFields())
        {
            if (field.FieldType != typeof(OpCode)) continue;
            var op_code = (OpCode) field.GetValue(null);
            try
            {
                _op_code_map.Add(
                    (Mono.Cecil.Cil.OpCode)typeof(Mono.Cecil.Cil.OpCodes).GetField(field.Name).GetValue(null)
                    , op_code);
            }
            catch (Exception e)
            {
                LogService.LogError($"Failed to initialize opcode map for {field.Name}");
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
        _new_method = pNewMethod;
        pHarmony.Patch(pOldMethod, transpiler: new HarmonyMethod(AccessTools.Method(typeof(ModReloadUtils), nameof(il_code_replace_transpiler))));
    }
    static MethodDefinition _new_method;
    static IEnumerable<CodeInstruction> il_code_replace_transpiler(IEnumerable<CodeInstruction> pInstructions)
    {
        List<CodeInstruction> new_instr = new();
        LogService.LogInfo($"Hotfixing method {_new_method.Name} with following instructions:");
        foreach (var cecil_instr in _new_method.Body.Instructions)
        {
            new_instr.Add(new CodeInstruction(
                _op_code_map[cecil_instr.OpCode], cecil_instr.Operand
            ));
            LogService.LogInfo($"\t{cecil_instr.OpCode}\t\t {cecil_instr.Operand}");
        }
        return new_instr;
    }
    public static bool Reload()
    {
        try
        {
            ((IReloadable)_mod).Reload();
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