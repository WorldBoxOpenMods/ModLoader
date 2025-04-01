using System.Text;
using NeoModLoader.services;

namespace NeoModLoader.General.Event.Handlers;
/// <summary>
/// This class is used to handle culture create event.
/// </summary>
public abstract class CultureCreateHandler : AbstractHandler<CultureCreateHandler>
{
    /// <summary>
    /// This method is called when a culture is created. Detailedly, at the end of <see cref="CultureManager.newCulture"/>
    /// </summary>
    /// <param name="pCulture"></param>
    /// <param name="pActor"></param>
    /// <param name="pCity"></param>
    public abstract void Handle(Culture pCulture, Actor pActor, City pCity);
}