using UnityEngine;

namespace NeoModLoader.constants;
/// <summary>
/// Some constants that are not related to mod loader itself.
/// </summary>
public static class Others
{
    internal const long confirmed_compile_time = 100000000;
    internal const string harmony_id = "wbom.nml";
    /// <summary>
    /// Determine whether the game is running on unity player. (Editor included). For unit test.
    /// </summary>
    public static bool unity_player_enabled { get; internal set; } = false;
    /// <summary>
    /// Check whether the game is running on unity editor. For NeoModSDK
    /// </summary>
    public static bool is_editor
    {
        get
        {
            if (unity_player_enabled) return Application.platform switch
            {
                RuntimePlatform.WindowsEditor => true,
                RuntimePlatform.OSXEditor => true,
                RuntimePlatform.LinuxEditor => true,
                _ => false
            };
            return false;
        }
    }
}