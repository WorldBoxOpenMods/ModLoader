using System.Text;
using NeoModLoader.services;

namespace NeoModLoader.General.Event.Handlers;

public abstract class CultureCreateHandler : AbstractHandler<CultureCreateHandler>
{
    public abstract void Handle(Culture pCulture, Race pRace, City pCity);
}