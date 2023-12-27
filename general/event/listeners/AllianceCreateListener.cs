using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using NeoModLoader.General.Event.Handlers;
using NeoModLoader.services;

namespace NeoModLoader.General.Event.Listeners;

/// <summary>
///     This class is used to listen to <see cref="AllianceManager.newAlliance" /> event. And call all
///     <see cref="AllianceCreateHandler.Handle" /> when the event is triggered.
/// </summary>
public class AllianceCreateListener : AbstractListener<AllianceCreateListener, AllianceCreateHandler>
{
    /// <summary>
    ///     Call all <see cref="AllianceCreateHandler.Handle" /> when the event is triggered.
    /// </summary>
    /// <inheritdoc cref="AllianceCreateHandler.Handle" />
    protected static void HandleAll(Alliance pAlliance, Kingdom pKingdom, Kingdom pKingdom2)
    {
        StringBuilder sb = null;
        int idx = 0;
        int count = instance.handlers.Count;
        bool finished = false;
        while (!finished)
        {
            try
            {
                for (; idx < count; idx++)
                {
                    instance.handlers[idx].Handle(pAlliance, pKingdom, pKingdom2);
                }

                finished = true;
            }
            catch (Exception e)
            {
                instance.handlers[idx].HitException();
                sb ??= new();
                sb.AppendLine($"Failed to handle event in {instance.handlers[idx].GetType().FullName}");
                sb.AppendLine(e.Message);
                sb.AppendLine(e.StackTrace);
                idx++;
            }
        }

        if (sb != null)
        {
            LogService.LogError(sb.ToString());
        }
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(AllianceManager), nameof(AllianceManager.newAlliance))]
    private static IEnumerable<CodeInstruction> _newAllianceEvent_Patch(IEnumerable<CodeInstruction> instr)
    {
        List<CodeInstruction> codes = new(instr);

        int insert_index = 9;
        codes.Insert(insert_index++, new CodeInstruction(OpCodes.Dup));
        codes.Insert(insert_index++, new CodeInstruction(OpCodes.Ldarg_1));
        codes.Insert(insert_index++, new CodeInstruction(OpCodes.Ldarg_2));

        InsertCallHandleCode(codes, insert_index);
        return codes;
    }
}