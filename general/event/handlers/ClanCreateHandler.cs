namespace NeoModLoader.General.Event.Handlers;
/// <summary>
/// This class is used to handle clan create event.
/// </summary>
public abstract class ClanCreateHandler : AbstractHandler<ClanCreateHandler>
{
    /// <summary>
    /// This method is called when a clan is created. Detailedly, at the end of <see cref="ClanManager.newClan"/>
    /// </summary>
    /// <param name="pClan"></param>
    /// <param name="pFounder"></param>
    public abstract void Handle(Clan pClan, Actor pFounder);
}