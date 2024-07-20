namespace NeoModLoader.General.UI.Tab;

public class TabNature : ReconstructedVanillaTab
{
    public const string PHENOMENON = "phenomenon";
    public const string BIOMES = "biomes";
    public const string FERTILITY = "fertility";
    public const string RESOURCES = "resources";
    public const string DROP = "drop";

    protected override string[] Groups => new string[] { PHENOMENON, BIOMES, FERTILITY, RESOURCES, DROP };

    protected override void InitTab()
    {
        tab = new WrappedPowersTab(PowerButtonCreator.GetTab(PowerTabNames.Nature));
    }
}