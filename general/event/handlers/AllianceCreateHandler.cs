namespace NeoModLoader.General.Event.Handlers;

public abstract class AllianceCreateHandler : AbstractHandler<AllianceCreateHandler>
{
    public abstract void Handle(Alliance pAlliance, Kingdom pKingdom, Kingdom pKingdom2);
}