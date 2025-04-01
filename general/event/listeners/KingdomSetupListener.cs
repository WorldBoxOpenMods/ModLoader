using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using NeoModLoader.General.Event.Handlers;
using NeoModLoader.services;

namespace NeoModLoader.General.Event.Listeners;

public class KingdomSetupListener : AbstractListener<KingdomSetupListener, KingdomSetupHandler>
{
    protected static void HandleAll(Kingdom pKingdom, bool pCiv)
    {
        StringBuilder sb = null;
        foreach (var handler in instance.handlers)
        {
            if(!handler.enabled) continue;
            try
            {
                handler.Handle(pKingdom, pCiv);
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
    [HarmonyPatch(typeof(KingdomManager), nameof(KingdomManager.makeNewCivKingdom))]
    private static IEnumerable<CodeInstruction> _setupKingdom_Patch(IEnumerable<CodeInstruction> instr)
    {
        List<CodeInstruction> codes = new(instr);
        
        int insert_index = 28;
        codes.Insert(insert_index++, new CodeInstruction(OpCodes.Ldarg_1));
        codes.Insert(insert_index++, new CodeInstruction(OpCodes.Ldarg_2));
        
        InsertCallHandleCode(codes, insert_index);
        return codes;
    }
}