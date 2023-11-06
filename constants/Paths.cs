using System.Reflection;
using UnityEngine;

namespace NeoModLoader.constants;

public static class Paths
{
    public static readonly string NMLModPath = Assembly.GetExecutingAssembly().Location;

    //public static readonly string NMLModPath =
    //    @"C:\Program Files (x86)\Steam\steamapps\common\worldbox\worldbox_Data\StreamingAssets\mods\NeoModLoader.dll";
    public static readonly string NMLPath = Combine(NMLModPath, "..", "NML");

    public static readonly string StreamingAssetsPath = Combine(NMLPath, "..", "..");
    public static readonly string ManagedPath = Combine(StreamingAssetsPath, "..", "Managed");
    public static readonly string GamePath = Combine(StreamingAssetsPath, "..", "..");
    public static readonly string BepInExPluginsPath = Combine(GamePath, "BepInEx", "plugins");
    public static readonly string ModsPath = Combine(GamePath , "Mods");

    public static readonly string NMLAssembliesPath = Combine(NMLPath, "Assemblies");
    public static readonly string CompiledModsPath = Combine(NMLPath , "CompiledMods");
    public static readonly string ModCompileRecordPath = Combine(NMLPath, "mod_compile_records.json");
    public static readonly string ModConfigFileName = "mod.json";
    public static readonly string ModResourceFolderName = "GameResources";
    public static readonly string ModsWorkshopPath = Combine(GamePath, "..", "..", "workshop", "content", "1206560");
    public static readonly HashSet<string> IgnoreSearchDirectories = new HashSet<string>()
    {
        "bin", "obj", "Properties", "packages", "packages.config", "packages-lock.json", "packages-lock.xml",
    };
    private static string Combine(params string [] paths)
    {
        string result = "";
        foreach (var path in paths)
        {
            result = Path.Combine(result, path);
        }

        return new FileInfo(result).FullName;
    }
}