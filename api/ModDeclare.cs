using System.Text;
using NeoModLoader.utils;
using Newtonsoft.Json;

namespace NeoModLoader.api;

/// <summary>
/// Mod type, determine which mod loader to load it.
/// </summary>
public enum ModTypeEnum
{
    /// <summary>
    /// NeoMod
    /// </summary>
    NORMAL,

    /// <summary>
    /// BepInEx
    /// </summary>
    BEPINEX
}

internal enum ModState
{
    DISABLED,
    LOADED,
    FAILED
}

/// <summary>
/// Declaration of a mod
/// </summary>
[Serializable]
public class ModDeclare
{
#pragma warning disable CS8618
    ModDeclare()
#pragma warning restore CS8618
    {
    }

    /// <summary>
    /// Create a ModDeclare object with given parameters
    /// </summary>
    /// <param name="pName"></param>
    /// <param name="pAuthor"></param>
    /// <param name="pIconPath"></param>
    /// <param name="pVersion"></param>
    /// <param name="pDescription"></param>
    /// <param name="pFolderPath"></param>
    /// <param name="pDependencies"></param>
    /// <param name="pOptionalDependencies"></param>
    /// <param name="pIncompatibleWith"></param>
    public ModDeclare(string pName, string pAuthor, string pIconPath, string pVersion, string pDescription,
        string pFolderPath, string[] pDependencies, string[] pOptionalDependencies, string[] pIncompatibleWith)
    {
        Name = pName;
        Author = pAuthor;
        IconPath = pIconPath;
        Version = pVersion;
        Description = pDescription;
        Dependencies = pDependencies ?? new string[0];
        OptionalDependencies = pOptionalDependencies ?? new string[0];
        IncompatibleWith = pIncompatibleWith ?? new string[0];

        UID = ModDependencyUtils.ParseDepenNameToPreprocessSymbol($"{Author}.{Name}");

        for (int i = 0; i < Dependencies.Length; i++)
            Dependencies[i] = ModDependencyUtils.ParseDepenNameToPreprocessSymbol(Dependencies[i]);
        for (int i = 0; i < OptionalDependencies.Length; i++)
            OptionalDependencies[i] = ModDependencyUtils.ParseDepenNameToPreprocessSymbol(OptionalDependencies[i]);
        for (int i = 0; i < IncompatibleWith.Length; i++)
            IncompatibleWith[i] = ModDependencyUtils.ParseDepenNameToPreprocessSymbol(IncompatibleWith[i]);

        FolderPath = pFolderPath;
    }

    /// <summary>
    /// Read a mod config file and parse it into a ModDeclare object
    /// </summary>
    /// <param name="pFilePath"></param>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="Exception"></exception>
    public ModDeclare(string pFilePath)
    {
        ModDeclare modDeclare = JsonConvert.DeserializeObject<ModDeclare>(File.ReadAllText(pFilePath)) ??
                                throw new InvalidOperationException("Input Mod Config file path cannot be null");
        if (modDeclare == null)
        {
            throw new Exception($"Mod Config file at \"{pFilePath}\" is invalid");
        }

        Name = modDeclare.Name;
        Author = modDeclare.Author;
        Version = modDeclare.Version;
        IconPath = modDeclare.IconPath;
        Description = modDeclare.Description;
        Dependencies = modDeclare.Dependencies;
        OptionalDependencies = modDeclare.OptionalDependencies;
        IncompatibleWith = modDeclare.IncompatibleWith;
        ModType = modDeclare.ModType;
        UsePublicizedAssembly = modDeclare.UsePublicizedAssembly;

        Dependencies ??= new string[0];
        OptionalDependencies ??= new string[0];
        IncompatibleWith ??= new string[0];

        UID = modDeclare.UID;
        if (string.IsNullOrEmpty(UID)) UID = $"{Author}.{Name}";
        UID = ModDependencyUtils.ParseDepenNameToPreprocessSymbol(UID);

        for (int i = 0; i < Dependencies.Length; i++)
            Dependencies[i] = ModDependencyUtils.ParseDepenNameToPreprocessSymbol(Dependencies[i]);
        for (int i = 0; i < OptionalDependencies.Length; i++)
            OptionalDependencies[i] = ModDependencyUtils.ParseDepenNameToPreprocessSymbol(OptionalDependencies[i]);
        for (int i = 0; i < IncompatibleWith.Length; i++)
            IncompatibleWith[i] = ModDependencyUtils.ParseDepenNameToPreprocessSymbol(IncompatibleWith[i]);

        FolderPath = Path.GetDirectoryName(pFilePath) ??
                     throw new Exception("Cannot get folder path from input file path");
    }

    /// <summary>
    /// Mod Name. Add locale of $"{Name}_{language}" can make it display different name in different language
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; private set; }

    /// <summary>
    /// Unique ID. GUID.ToValid() or $"{Author}_{Name}".ToUpper().ToValid(). ToValid: Replace all characters belong to ASCII but are not letters or numbers with '_'
    /// </summary>
    [JsonProperty("GUID")]
    public string UID { get; private set; }

    /// <summary>
    /// Mod Author. Add locale of $"{Author}_{language}" can make it display different name in different language
    /// </summary>

    [JsonProperty("author")]
    public string Author { get; private set; }

    /// <summary>
    /// Mod Version
    /// </summary>
    [JsonProperty("version")]
    public string Version { get; private set; }

    /// <summary>
    /// Mod Description. Add locale of $"{Description}_{language}" can make it display different name in different language
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; private set; }

    /// <summary>
    /// Url to repo or website of this mod.
    /// </summary>
    [JsonProperty("RepoUrl")]
    public string RepoUrl { get; private set; }

    /// <summary>
    /// List of hard dependencies' UID
    /// </summary>
    [JsonProperty("Dependencies")]
    public string[] Dependencies { get; private set; }

    /// <summary>
    /// List of soft dependencies' UID
    /// </summary>
    [JsonProperty("OptionalDependencies")]
    public string[] OptionalDependencies { get; private set; }

    /// <summary>
    /// List of incompatible mods' UID. Not implemented yet.
    /// </summary>
    [JsonProperty("IncompatibleWith")]
    public string[] IncompatibleWith { get; private set; }

    /// <summary>
    /// The mod's folder path.
    /// </summary>
    public string FolderPath { get; private set; } = null!;

    /// <summary>
    /// Target Game Build. Not implemented yet.
    /// </summary>
    [JsonProperty("targetGameBuild")]
    public int TargetGameBuild { get; private set; }

    /// <summary>
    /// Path to icon file. Relative to mod folder.
    /// </summary>
    [JsonProperty("iconPath")]
    public string IconPath { get; private set; }

    /// <summary>
    /// Mod type.
    /// </summary>
    [JsonProperty("ModType")]
    public ModTypeEnum ModType { get; private set; } = ModTypeEnum.NORMAL;

    /// <summary>
    /// Wheather use publicized assembly.
    /// </summary>
    [JsonProperty("UsePublicizedAssembly")]
    public bool UsePublicizedAssembly { get; private set; } = true;

    /// <summary>
    /// Wheather this mod be determined as a NCMS mod.
    /// </summary>
    public bool IsNCMSMod { get; internal set; } = false;

    /// <summary>
    /// Reason of failing to compile or load.
    /// </summary>
    public StringBuilder FailReason { get; } = new();

    internal void SetRepoUrlToWorkshopPage(string id)
    {
        RepoUrl = $"https://steamcommunity.com/sharedfiles/filedetails/?id={id}";
    }

    internal void SetModType(ModTypeEnum modType)
    {
        if (modType < ModTypeEnum.NORMAL || modType > ModTypeEnum.BEPINEX)
            throw new ArgumentOutOfRangeException(nameof(modType), modType, null);
        ModType = modType;
    }

    /// <summary>
    /// This only called for BepInEx mods
    /// </summary>
    /// <param name="iconPath"></param>
    internal void SetIconPath(string iconPath)
    {
        IconPath = iconPath;
    }
}