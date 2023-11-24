namespace NeoModLoader.General.Event.Handlers;

public abstract class ClanCreateHandler : AbstractHandler<ClanCreateHandler>
{
    public abstract void Handle(Clan pClan, Actor pFounder);
}