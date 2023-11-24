namespace NeoModLoader.General.Event.Handlers;

public abstract class KingdomSetupHandler : AbstractHandler<KingdomSetupHandler>
{
    public abstract void Handle(Kingdom pKingdom, bool pCiv);
}