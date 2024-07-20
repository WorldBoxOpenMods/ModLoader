namespace NeoModLoader.General.UI.Tab;

public class TabKingdoms : ReconstructedVanillaTab
{
    public const string INSPECT = "inspect";
    public const string RELATION = "relation";
    public const string ACTIVITY = "activity";
    public const string FORCE_VIEW = "force_view";
    public const string MAPLAYER = "maplayer";

    protected override string[] Groups => new string[] { INSPECT, RELATION, ACTIVITY, FORCE_VIEW, MAPLAYER };

    protected override void InitTab()
    {
        tab = new WrappedPowersTab(PowerButtonCreator.GetTab(PowerTabNames.Kingdoms));
    }
}