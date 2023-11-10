using System.Reflection.Emit;
using HarmonyLib;
using NeoModLoader.General.Event.Handlers;

namespace NeoModLoader.General.Event.Listeners;

public class CultureCreateListener : AbstractListener<CultureCreateHandler>
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Culture), nameof(Culture.createCulture))]
    private static IEnumerable<CodeInstruction> _createCulture_Patch(IEnumerable<CodeInstruction> instr)
    {
        List<CodeInstruction> codes = new(instr);
        
        codes.Insert(42, new CodeInstruction(OpCodes.Ldarg_0));
        codes.Insert(43, new CodeInstruction(OpCodes.Ldarg_1));
        codes.Insert(44, new CodeInstruction(OpCodes.Ldarg_2));
        
        AppendCallHandleCode(codes, 45);
        return codes;
    }
}