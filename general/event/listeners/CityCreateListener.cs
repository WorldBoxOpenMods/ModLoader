using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using NeoModLoader.General.Event.Handlers;
using NeoModLoader.services;

namespace NeoModLoader.General.Event.Listeners;

public class CityCreateListener : AbstractListener<CityCreateListener, CityCreateHandler>
{
    protected static void HandleAll(City pCity)
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
                    instance.handlers[idx].Handle(pCity);
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
        if(sb != null)
        {
            LogService.LogError(sb.ToString());
        }
    }
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(City), nameof(City.newCityEvent))]
    private static IEnumerable<CodeInstruction> _newCityEvent_Patch(IEnumerable<CodeInstruction> instr)
    {
        List<CodeInstruction> codes = new(instr);

        int insert_index = 4;
        codes.Insert(insert_index++, new CodeInstruction(OpCodes.Ldarg_0));
        
        InsertCallHandleCode(codes, insert_index);
        return codes;
    }
}