using UnityEngine;

namespace NeoModLoader.constants;

public static class Others
{
    internal const long confirmed_compile_time = 100000000;
    internal const string harmony_id = "wbom.nml";
    public static bool unity_player_enabled { get; internal set; } = false;

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