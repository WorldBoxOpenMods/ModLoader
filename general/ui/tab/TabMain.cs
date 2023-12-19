namespace NeoModLoader.General.UI.Tab;

public class TabMain : ReconstructedVanillaTab
{
    public const string WORLD_INFO = "world_info";
    public const string REBUILD = "rebuild";
    public const string GAME_SETTING = "game_setting";
    public const string OTHERS = "others";

    public override void Init()
    {
        tab = new WrappedPowersTab(PowerButtonCreator.GetTab(PowerTabNames.Main));

        tab.AddGroup(WORLD_INFO);
        tab.AddGroup(REBUILD);
        tab.AddGroup(GAME_SETTING);
        tab.AddGroup(OTHERS);
    }
}