using System.Reflection;
using UnityEngine;

namespace NeoModLoader.constants;

public static class Paths
{
    public static readonly string NMLModPath = Assembly.GetExecutingAssembly().Location;

    //public static readonly string NMLModPath =
    //    @"C:\Program Files (x86)\Steam\steamapps\common\worldbox\worldbox_Data\StreamingAssets\mods\NeoModLoader.dll";
    public static readonly string NMLPath = Path.Combine(NMLModPath, "..", "NML");

    public static readonly string StreamingAssetsPath = Path.Combine(NMLPath, "..", "..");
    public static readonly string ManagedPath = Path.Combine(StreamingAssetsPath, "..", "Managed");
    public static readonly string GamePath = Path.Combine(StreamingAssetsPath, "..", "..");
    public static readonly string ModsPath = Path.Combine(GamePath , "Mods");

    public static readonly string NMLAssembliesPath = Path.Combine(NMLPath, "Assemblies");
    public static readonly string CompiledModsPath = Path.Combine(NMLPath , "CompiledMods");
    public static readonly string ModCompileRecordPath = Path.Combine(NMLPath, "mod_compile_records.json");
    public static readonly string ModConfigFileName = "mod.json";
    public static readonly string ModResourceFolderName = "GameResources";
}