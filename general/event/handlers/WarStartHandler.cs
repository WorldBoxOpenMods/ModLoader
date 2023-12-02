namespace NeoModLoader.General.Event.Handlers;

public abstract class WarStartHandler : AbstractHandler<WarStartHandler>
{
    public abstract void Handle(War pWar, Kingdom pAttacker, Kingdom pDefender, WarTypeAsset pWarType);
}