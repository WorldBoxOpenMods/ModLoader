namespace NeoModLoader.General.Event.Handlers;
/// <summary>
/// This class is used to handle war end event.
/// </summary>
public abstract class WarEndHandler : AbstractHandler<WarEndHandler>
{
    /// <summary>
    /// This method is called when a war is ended. Detailedly, at the end of <see cref="WarManager.endWar"/>
    /// </summary>
    /// <param name="pWarManager"></param>
    /// <param name="pWar"></param>
    public abstract void Handle(WarManager pWarManager, War pWar);
}