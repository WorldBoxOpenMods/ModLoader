namespace NeoModLoader.General.Event.Handlers;

public abstract class PlotStartHandler : AbstractHandler<PlotStartHandler>
{
    public abstract void Handle(Plot pPlot, Actor pActor, PlotAsset pAsset);
}