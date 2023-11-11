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
        foreach (var handler in instance.handlers)
        {
            if(!handler.enabled) continue;
            try
            {
                handler.Handle(pCity);
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