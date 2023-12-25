namespace NeoModLoader.General.Event.Handlers;
/// <summary>
/// This class is used to handle war start event.
/// </summary>
public abstract class WarStartHandler : AbstractHandler<WarStartHandler>
{
    /// <summary>
    /// This method is called when a war is started. Detailedly, at the end of <see cref="WarManager.newWar"/>
    /// </summary>
    /// <param name="pWar"></param>
    /// <param name="pAttacker"></param>
    /// <param name="pDefender"></param>
    /// <param name="pWarType"></param>
    public abstract void Handle(War pWar, Kingdom pAttacker, Kingdom pDefender, WarTypeAsset pWarType);
}