using System.Reflection;
using UnityEngine;

namespace NeoModLoader.constants;

/// <summary>
/// Common used paths
/// </summary>
public static class Paths
{
    /// <summary>
    /// Path to the mod loader file
    /// </summary>
    public static readonly string NMLModPath;

    /// <summary>
    /// Path to persistent data
    /// </summary>
    public static readonly string PersistentDataPath = Combine(Application.persistentDataPath);

    /// <summary>
    /// Path to folder StreamingAssets
    /// </summary>
    public static readonly string StreamingAssetsPath = Combine(Application.streamingAssetsPath);

    /// <summary>
    /// Path to game native Mods folder
    /// </summary>
    public static readonly string NativeModsPath = Combine(StreamingAssetsPath, "mods");

    /// <summary>
    /// Path to game native Managed folder
    /// </summary>
    public static readonly string ManagedPath = Others.is_editor
        ? Combine(StreamingAssetsPath, "..", ".Managed")
        : Combine(StreamingAssetsPath, "..", "Managed");

    /// <summary>
    /// Path to folder contains NML's cache
    /// </summary>
    public static readonly string NMLPath = Combine(NativeModsPath, "NML");

    /// <summary>
    ///     Path to file contains NML's commit hash
    /// </summary>
    public static readonly string NMLCommitPath = Combine(NMLPath, "commit");

    /// <summary>
    ///     Path to file of auto update module
    /// </summary>
    public static readonly string NMLAutoUpdateModulePath =
        Combine(NativeModsPath, "NeoModLoader.AutoUpdate_memload.dll");

    /// <summary>
    /// Path to the publicized Assembly-CSharp.dll file
    /// </summary>
    public static readonly string PublicizedAssemblyPath = Combine(NMLPath, "Assembly-CSharp-Publicized.dll");

    /// <summary>
    /// Path to folder mods config under persistent data folder
    /// </summary>
    public static readonly string ModsConfigPath = Combine(PersistentDataPath, "mods_config");

    /// <summary>
    /// Path to BepInEx plugins folder
    /// </summary>
    public static readonly string BepInExPluginsPath = Combine(GamePath, "BepInEx", "plugins");

    /// <summary>
    /// Path to Mods folder provided by NML
    /// </summary>
    public static readonly string ModsPath =
        Others.is_editor ? Combine(GamePath, "Assets", "Mods") : Combine(GamePath, "Mods");

    /// <summary>
    /// Path to extracted Assemblies cache
    /// </summary>
    public static readonly string NMLAssembliesPath = Combine(NMLPath, "Assemblies");

    /// <summary>
    /// Path to compiled mods cache
    /// </summary>
    public static readonly string CompiledModsPath = Combine(NMLPath, "CompiledMods");

    /// <summary>
    /// Path to tab order record file
    /// </summary>
    public static readonly string TabOrderRecordPath = Combine(NMLPath, "tab_order_records.json");

    /// <summary>
    /// Path to mod compilation timestamps record file
    /// </summary>
    public static readonly string ModCompileRecordPath = Combine(NMLPath, "mod_compile_records.json");

    /// <summary>
    /// Path to disabled mods record file
    /// </summary>
    public static readonly string ModsDisabledRecordPath = Combine(NMLPath, "disabled_mods.txt");

    /// <summary>
    /// File name of a mod declaration
    /// </summary>
    public static readonly string ModDeclarationFileName = "mod.json";

    /// <summary>
    /// File name of a default mod config file for BasicMod
    /// </summary>
    public static readonly string ModDefaultConfigFileName = "default_config.json";

    /// <summary>
    /// Folder name of a mod's resource folder which will be patched to <see cref="Resources"/>
    /// </summary>
    public static readonly string ModResourceFolderName = Others.is_editor ? "Resources" : "GameResources";

    /// <summary>
    /// Folder name of a NCMS mod's additional resource folder which will also be patched to <see cref="Resources"/>
    /// </summary>
    public static readonly string NCMSAdditionModResourceFolderName = "GameResourcesReplace";

    /// <summary>
    /// Folder name of a mod's assetbundles.
    /// </summary>
    public static readonly string ModAssetBundleFolderName = "AssetBundles";

    /// <summary>
    /// Path to the folder of game's workshop content
    /// </summary>
    public static readonly string CommonModsWorkshopPath =
        Combine(GamePath, "..", "..", "workshop", "content", CoreConstants.GameId.ToString());

    /// <summary>
    /// Folder name of a NCMS mod's EmbeddedResource folder
    /// </summary>
    public static readonly string
        NCMSModEmbededResourceFolderName =
            "EmbededResources"; // note that this typo in "Embedded" has to stay, as NCMS also has it

    /// <summary>
    /// All folders/files that will be ignored when searching for mods' content to upload
    /// </summary>
    public static readonly HashSet<string> IgnoreSearchDirectories = new HashSet<string>()
    {
        "bin", "obj", "Properties", "packages", "packages.config", "packages-lock.json", "packages-lock.xml",
    };

    internal static readonly string LinuxSteamLocalConfigPath =
        "~/.local/share/Steam/userdata/{0}/config/localconfig.vdf";

    static Paths()
    {
        var nml_mod_path = Assembly.GetExecutingAssembly().Location;
        if (string.IsNullOrEmpty(nml_mod_path))
        {
            nml_mod_path = Combine(NativeModsPath, "NeoModLoader.dll");
            if (!File.Exists(nml_mod_path)) nml_mod_path = Combine(NativeModsPath, "NeoModLoader_memload.dll");
        }

        NMLModPath = nml_mod_path;
    }

    /// <summary>
    /// Path to game root folder
    /// </summary>
    public static string GamePath => Application.platform switch
    {
        RuntimePlatform.WindowsPlayer => Combine(StreamingAssetsPath, "..", ".."),
        RuntimePlatform.LinuxPlayer   => Combine(StreamingAssetsPath, "..", ".."),
        RuntimePlatform.OSXPlayer     => Combine(StreamingAssetsPath, "..", "..", "..", "..", ".."),
        _                             => Combine(StreamingAssetsPath, "..", "..")
    };

    private static string Combine(params string[] paths) => new FileInfo(paths.Aggregate("", Path.Combine)).FullName;
}