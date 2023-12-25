using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using NeoModLoader.General.Event.Handlers;
using NeoModLoader.services;

namespace NeoModLoader.General.Event.Listeners;

public class PlotStartListener : AbstractListener<PlotStartListener, PlotStartHandler>
{
    protected static void HandleAll(Plot pPlot, Actor pActor, PlotAsset pAsset)
    {
        StringBuilder sb = null;
        foreach (var handler in instance.handlers)
        {
            if(!handler.enabled) continue;
            try
            {
                handler.Handle(pPlot, pActor, pAsset);
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
    [HarmonyPatch(typeof(PlotManager), nameof(PlotManager.newPlot), new Type[] {typeof(Actor), typeof(PlotAsset)})]
    private static IEnumerable<CodeInstruction> _newPlot_Patch(IEnumerable<CodeInstruction> instr)
    {
        List<CodeInstruction> codes = new(instr);

        int insert_index = codes.FindIndex(code => code.opcode == OpCodes.Ret);
        codes.Insert(insert_index++, new CodeInstruction(OpCodes.Dup));
        codes.Insert(insert_index++, new CodeInstruction(OpCodes.Ldarg_1));
        codes.Insert(insert_index++, new CodeInstruction(OpCodes.Ldarg_2));
        
        InsertCallHandleCode(codes, insert_index);
        return codes;
    }
}