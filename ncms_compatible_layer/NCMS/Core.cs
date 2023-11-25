using UnityEngine;

namespace NCMS;

public class Core
{
    // For compatibility, use all same with NCMS.
    public static string WBGamePath = ((Application.platform == RuntimePlatform.WindowsPlayer) ? (Application.dataPath + "/..") : (Application.dataPath + "/../.."));

    public static string ModsPath = Application.streamingAssetsPath + "/Mods";

    public static string ManagedPath = Application.streamingAssetsPath + "/../Managed";

    public static string NCMSPath = ModsPath + "/NCMS";

    public static string NCMSModsPath = WBGamePath + "/Mods";

    public static string CorePath = NCMSPath + "/Core";

    public static string AssembliesPath = CorePath + "/Assemblies";

    public static string TempPath = CorePath + "/Temp";
}