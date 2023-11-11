namespace NeoModLoader.General.Event.Handlers;

public abstract class CityCreateHandler : AbstractHandler<CityCreateHandler>
{
    public abstract void Handle(City pCity);
}