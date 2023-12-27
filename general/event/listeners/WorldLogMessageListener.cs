using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using NeoModLoader.General.Event.Handlers;
using NeoModLoader.services;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.General.Event.Listeners;

/// <summary>
/// This listener is made for adding your own log message.
/// </summary>
public class WorldLogMessageListener : AbstractListener<WorldLogMessageListener, WorldLogMessageHandler>
{
    /// <summary>
    ///     This method is called when a log message is about to be displayed. And call all
    ///     <see cref="WorldLogMessageHandler.Handle" /> when the event is triggered.
    /// </summary>
    /// <inheritdoc cref="WorldLogMessageHandler.Handle" />
    protected static string HandleAll(ref WorldLogMessage pMessage, string pCurrentText, Color pCurrentColor,
        Text pTextfield, bool pColorField, bool pColorTags)
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
                    instance.handlers[idx].Handle(ref pMessage, ref pCurrentText, ref pCurrentColor, ref pColorField,
                        pColorTags);
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

        if (pColorField)
        {
            pTextfield.color = pCurrentColor;
        }
        else
        {
            pTextfield.color = Toolbox.color_log_neutral;
        }

        return pCurrentText;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(WorldLogMessageExtensions), nameof(WorldLogMessageExtensions.getFormatedText))]
    private static IEnumerable<CodeInstruction> _WorldLogMessage_getFormatedText_Patch(
        IEnumerable<CodeInstruction> instr)
    {
        List<CodeInstruction> codes = new(instr);

        int insert_index = codes.Count - 2;
        codes.Insert(insert_index++, new CodeInstruction(OpCodes.Ldarg_0));
        codes.Insert(insert_index++, new CodeInstruction(OpCodes.Ldloc_0));
        codes.Insert(insert_index++, new CodeInstruction(OpCodes.Ldloc_1));
        codes.Insert(insert_index++, new CodeInstruction(OpCodes.Ldarg_1));
        codes.Insert(insert_index++, new CodeInstruction(OpCodes.Ldarg_2));
        codes.Insert(insert_index++, new CodeInstruction(OpCodes.Ldarg_3));

        codes.Insert(insert_index++,
            new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(WorldLogMessageListener), "HandleAll")));
        codes.Insert(insert_index, new CodeInstruction(OpCodes.Stloc_0));
        return codes;
    }
}