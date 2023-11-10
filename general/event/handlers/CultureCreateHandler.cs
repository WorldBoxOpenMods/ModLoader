using System.Text;
using NeoModLoader.services;

namespace NeoModLoader.General.Event.Handlers;

public class CultureCreateHandler : AbstractHandler<CultureCreateHandler>
{
    protected static void HandleAll(Culture pCulture, Race pRace, City pCity)
    {
        StringBuilder sb = null;
        foreach (var handler in _handlers)
        {
            if(!handler.enabled) continue;
            try
            {
                handler.Handle(pCulture, pRace, pCity);
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
    protected void Handle(Culture pCulture, Race pRace, City pCity)
    {
        LogService.LogInfo(pCulture.name);
    }
}