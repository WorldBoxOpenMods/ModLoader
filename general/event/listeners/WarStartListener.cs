using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using NeoModLoader.General.Event.Handlers;
using NeoModLoader.services;

namespace NeoModLoader.General.Event.Listeners;

public class WarStartListener : AbstractListener<WarStartListener, WarStartHandler>
{
    protected static void HandleAll(War pWar, Kingdom pAttacker, Kingdom pDefender, WarTypeAsset pWarType)
    {
        StringBuilder sb = null;
        foreach (var handler in instance.handlers)
        {
            if (!handler.enabled) continue;
            try
            {
                handler.Handle(pWar, pAttacker, pDefender, pWarType);
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

        if (sb != null)
        {
            LogService.LogError(sb.ToString());
        }
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(WarManager), nameof(WarManager.newWar))]
    private static IEnumerable<CodeInstruction> _newWar_Patch(IEnumerable<CodeInstruction> instr)
    {
        List<CodeInstruction> codes = new(instr);

        int insert_index = codes.FindIndex(c => c.opcode == OpCodes.Ret);
        codes.Insert(insert_index++, new CodeInstruction(OpCodes.Dup));
        codes.Insert(insert_index++, new CodeInstruction(OpCodes.Ldarg_1));
        codes.Insert(insert_index++, new CodeInstruction(OpCodes.Ldarg_2));
        codes.Insert(insert_index++, new CodeInstruction(OpCodes.Ldarg_3));

        InsertCallHandleCode(codes, insert_index);
        return codes;
    }
}