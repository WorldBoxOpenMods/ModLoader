using UnityEngine;

namespace NeoModLoader.General.UI.Tab;

public class TabMain : ReconstructedVanillaTab
{
    public const string WORLD_INFO = "world_info";
    public const string REBUILD = "rebuild";
    public const string GAME_SETTING = "game_setting";
    public const string OTHERS = "others";
    public const string CUSTOM = "custom";
    protected override string[] Groups => _groups;
    private static readonly string[] _groups = new string[] { WORLD_INFO, REBUILD, GAME_SETTING, OTHERS, CUSTOM };

    protected override void InitTab()
    {
        tab = new WrappedPowersTab(PowerButtonCreator.GetTab(PowerTabNames.Main));
    }
}