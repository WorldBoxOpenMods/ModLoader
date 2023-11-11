namespace NeoModLoader.General.Event.Handlers;

public abstract class WarStartHandler : AbstractHandler<WarStartHandler>
{
    public abstract void Handle(WarManager pWarManager, War pWar, Kingdom pAttacker, Kingdom pDefender, WarTypeAsset pWarType);
}