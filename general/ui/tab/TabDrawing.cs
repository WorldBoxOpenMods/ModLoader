namespace NeoModLoader.General.UI.Tab;

public class TabDrawing : ReconstructedVanillaTab
{
    public const string TILE_BRUSH = "tile_brush";
    public const string MAP_HELPER = "map_helper";
    public const string CLEANER = "cleaner";
    public const string DELETOR = "deletor";

    protected override string[] Groups => new string[] { TILE_BRUSH, MAP_HELPER, CLEANER, DELETOR };

    protected override void InitTab()
    {
        tab = new WrappedPowersTab(PowerButtonCreator.GetTab(PowerTabNames.Drawing));
    }
}