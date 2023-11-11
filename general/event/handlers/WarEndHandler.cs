namespace NeoModLoader.General.Event.Handlers;

public abstract class WarEndHandler : AbstractHandler<WarEndHandler>
{
    public abstract void Handle(WarManager pWarManager, War pWar);
}