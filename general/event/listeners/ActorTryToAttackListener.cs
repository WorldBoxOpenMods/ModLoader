using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using NeoModLoader.General.Event.Handlers;
using NeoModLoader.services;

namespace NeoModLoader.General.Event.Listeners;

public class ActorTryToAttackListener : AbstractListener<ActorTryToAttackListener, ActorTryToAttackHandler>
{
    /// <summary>
    ///     Call all <see cref="ActorTryToAttackHandler.Handle" /> when the event is triggered.
    /// </summary>
    /// <inheritdoc cref="ActorTryToAttackHandler.Handle" />
    protected static void HandleAll(Actor      pAttacker, BaseSimObject pTarget, CombatActionAsset pCombatActionAsset,
                                    AttackData pAttackData)
    {
        StringBuilder sb = null;
        var idx = 0;
        var count = instance.handlers.Count;
        var finished = false;
        while (!finished)
            try
            {
                for (; idx < count; idx++)
                    instance.handlers[idx].Handle(pAttacker, pTarget, pCombatActionAsset, pAttackData);

                finished = true;
            }
            catch (Exception e)
            {
                instance.handlers[idx].HitException();
                sb ??= new StringBuilder();
                sb.AppendLine($"Failed to handle event in {instance.handlers[idx].GetType().FullName}");
                sb.AppendLine(e.Message);
                sb.AppendLine(e.StackTrace);
                idx++;
            }

        if (sb != null) LogService.LogError(sb.ToString());
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Actor), nameof(Actor.tryToAttack))]
    private static IEnumerable<CodeInstruction> _tryToAttack_Patch(IEnumerable<CodeInstruction> instr)
    {
        List<CodeInstruction> codes = new(instr);

        var insert_index =
            codes.FindIndex(x => x.opcode == OpCodes.Stloc_S && ((LocalBuilder)x.operand).LocalIndex == 7) - 1;
        codes.Insert(insert_index++, new CodeInstruction(OpCodes.Ldarg_0));
        codes.Insert(insert_index++, new CodeInstruction(OpCodes.Ldarg_1));
        codes.Insert(insert_index++, new CodeInstruction(OpCodes.Ldloc_S, 6));
        codes.Insert(insert_index++, new CodeInstruction(OpCodes.Ldloc_S, 4));

        InsertCallHandleCode(codes, insert_index);
        return codes;
    }
}