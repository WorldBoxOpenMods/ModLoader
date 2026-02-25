namespace NeoModLoader.General.Event.Handlers;

/// <summary>
/// This class is used to handle city create event.
/// </summary>
[Obsolete("Use patch instead")]
public abstract class CityCreateHandler : AbstractHandler<CityCreateHandler>
{
    /// <summary>
    /// This method is called when a city is created. Detailedly, at the end of <see cref="City.newCityEvent"/>
    /// </summary>
    /// <param name="pCity">The city just created</param>
    public abstract void Handle(City pCity);
}