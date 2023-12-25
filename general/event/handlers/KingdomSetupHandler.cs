namespace NeoModLoader.General.Event.Handlers;
/// <summary>
/// This class is used to handle kingdom setup event.
/// </summary>
public abstract class KingdomSetupHandler : AbstractHandler<KingdomSetupHandler>
{
    /// <summary>
    /// This method is called when a kingdom is setup. Detailedly, at the end of <see cref="KingdomManager.setupKingdom"/>
    /// </summary>
    /// <param name="pKingdom"></param>
    /// <param name="pCiv"></param>
    public abstract void Handle(Kingdom pKingdom, bool pCiv);
}