namespace NeoModLoader.General.Event.Handlers;
/// <summary>
/// This class is used to handle plot start event.
/// </summary>
public abstract class PlotStartHandler : AbstractHandler<PlotStartHandler>
{
    /// <summary>
    /// This method is called when a plot is started. Detailedly, at the end of <see cref="PlotManager.newPlot(Actor, PlotAsset)"/>
    /// </summary>
    /// <param name="pPlot"></param>
    /// <param name="pActor"></param>
    /// <param name="pAsset"></param>
    public abstract void Handle(Plot pPlot, Actor pActor, PlotAsset pAsset);
}