using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using NeoModLoader.General.Event.Handlers;
using NeoModLoader.services;

namespace NeoModLoader.General.Event.Listeners;

public class WarEndListener : AbstractListener<WarEndListener, WarEndHandler>
{
    protected static void HandleAll(WarManager pWarManager, War pWar)
    {
        StringBuilder sb = null;
        foreach (var handler in instance.handlers)
        {
            if(!handler.enabled) continue;
            try
            {
                handler.Handle(pWarManager, pWar);
            }
            catch (Exception e)
            {
                handler.HitException();
                sb ??= new();

                sb.AppendLine($"Failed to handle event in {handler.GetType().FullName}");
                sb.AppendLine(e.Message);
                sb.AppendLine(e.StackTrace);
            }
        }
        if(sb != null)
        {
            LogService.LogError(sb.ToString());
        }
    }
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(WarManager), nameof(WarManager.endWar))]
    private static IEnumerable<CodeInstruction> _endWar_Patch(IEnumerable<CodeInstruction> instr)
    {
        List<CodeInstruction> codes = new(instr);
        
        int insert_index = 14;
        codes.Insert(insert_index++, new CodeInstruction(OpCodes.Ldarg_0));
        codes.Insert(insert_index++, new CodeInstruction(OpCodes.Ldarg_1));
        
        InsertCallHandleCode(codes, insert_index);
        return codes;
    }
}