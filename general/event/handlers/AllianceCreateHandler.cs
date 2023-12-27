namespace NeoModLoader.General.Event.Handlers;

/// <summary>
/// This class is used to handle alliance create event.
/// </summary>
public abstract class AllianceCreateHandler : AbstractHandler<AllianceCreateHandler>
{
    /// <summary>
    /// This method is called when an alliance is created. Detailedly, at the end of <see cref="AllianceManager.newAlliance"/>
    /// </summary>
    /// <param name="pAlliance">The alliance created</param>
    /// <param name="pKingdom">One kingdom support the alliance</param>
    /// <param name="pKingdom2">Another kingdom support the alliance</param>
    public abstract void Handle(Alliance pAlliance, Kingdom pKingdom, Kingdom pKingdom2);
}