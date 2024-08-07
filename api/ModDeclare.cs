using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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
    NEOMOD,

    /// <summary>
    ///     Compiled NeoMod
    /// </summary>
    COMPILED_NEOMOD,

    /// <summary>
    /// BepInEx
    /// </summary>
    BEPINEX,

    /// <summary>
    ///     Resource Pack
    /// </summary>
    RESOURCE_PACK
}

internal enum ModState
{
    DISABLED,
    LOADED,
    FAILED
}

/// <summary>
/// Declaration of a mod.
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
    /// <param name="pIsWorkshopLoaded"></param>
    public ModDeclare(string pName, string pAuthor, string pIconPath, string pVersion, string pDescription,
        string pFolderPath, string[] pDependencies, string[] pOptionalDependencies, string[] pIncompatibleWith, bool pIsWorkshopLoaded = false)
    {
        Name = pName;
        Author = pAuthor;
        IconPath = pIconPath;
        Version = pVersion;
        Description = pDescription;
        Dependencies = pDependencies ?? Array.Empty<string>();
        OptionalDependencies = pOptionalDependencies ?? Array.Empty<string>();
        IncompatibleWith = pIncompatibleWith ?? Array.Empty<string>();
        IsWorkshopLoaded = pIsWorkshopLoaded;

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

        Dependencies ??= Array.Empty<string>();
        OptionalDependencies ??= Array.Empty<string>();
        IncompatibleWith ??= Array.Empty<string>();

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
        var pathSegments = FolderPath.Split(Path.DirectorySeparatorChar);
        int currentSearchIndex = pathSegments.IndexOf("workshop");
        if (currentSearchIndex == -1) return;
        if (currentSearchIndex + 3 >= pathSegments.Length) return;
        if (pathSegments[++currentSearchIndex] != "content") return;
        if (pathSegments[++currentSearchIndex] != "1206560" /* workshop ID of WorldBox */) return;
        Regex regex = new(@"^\d+$");
        if (!regex.IsMatch(pathSegments[++currentSearchIndex])) return;
        IsWorkshopLoaded = true;
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
    public ModTypeEnum ModType { get; private set; } = ModTypeEnum.NEOMOD;

    /// <summary>
    /// Whether to use publicized assembly.
    /// </summary>
    [JsonProperty("UsePublicizedAssembly")]
    public bool UsePublicizedAssembly { get; private set; } = true;

    /// <summary>
    /// Whether this mod has been identified as an NCMS mod.
    /// </summary>
    public bool IsNCMSMod { get; internal set; } = false;

    /// <summary>
    /// Reason of failing to compile or load.
    /// </summary>
    public StringBuilder FailReason { get; } = new();
    
    /// <summary>
    /// Whether the files of this mod were downloaded using the Steam workshop.
    /// </summary>
    public bool IsWorkshopLoaded { get; internal set; } = false;

    internal void SetRepoUrlToWorkshopPage(string id)
    {
        RepoUrl = $"https://steamcommunity.com/sharedfiles/filedetails/?id={id}";
    }

    internal void SetModType(ModTypeEnum modType)
    {
        if (modType < ModTypeEnum.NEOMOD || modType > ModTypeEnum.RESOURCE_PACK)
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

/// <summary>
/// Extensions for improving ease of use of ModDeclare.
/// </summary>
public static class ModDeclareExtensions
{
    /// <summary>
    /// Tries to get the declaration of a mod.
    /// Note that this method cannot reliably be used for precompiled NML mods, as there's no simple way to link their Assemblies to their ModDeclares.
    /// </summary>
    /// <param name="pModAssembly">The Assembly which a ModDeclare should be found for.</param>
    /// <param name="pModDeclare">The ModDeclare of the Assembly, if one was found.</param>
    /// <returns>Whether a ModDeclare could be matched to the provided Assembly.</returns>
    public static bool TryGetDeclaration(this Assembly pModAssembly, out ModDeclare pModDeclare)
    {
        foreach (var mod in WorldBoxMod.AllRecognizedMods.Keys)
        {
            switch (mod.ModType)
            {
                case ModTypeEnum.NEOMOD:
                    if (mod.UID == pModAssembly.GetName().Name)
                    {
                        pModDeclare = mod;
                        return true;
                    }
                    break;
                case ModTypeEnum.COMPILED_NEOMOD:
                    IMod modObj = WorldBoxMod.LoadedMods.FirstOrDefault(m => m.GetDeclaration() == mod);
                    if (modObj != null)
                    {
                        if (pModAssembly == modObj.GetType().Assembly)
                        {
                            pModDeclare = mod;
                            return true;
                        }

                        if (pModAssembly.Modules.SelectMany(m => m.GetTypes())
                                        .Where(t => t.GetInterfaces().Contains(typeof(IMod)))
                                        .Any(modClass => modClass.IsInstanceOfType(modObj)))
                        {
                            pModDeclare = mod;
                            return true;
                        }
                    }
                    else
                    {
                        if (Directory.GetFiles(mod.FolderPath).Any(possible_file =>
                                                                       Path.GetFullPath(possible_file) ==
                                                                       Path.GetFullPath(pModAssembly.Location)))
                            /* It might be organized as following:
                             * -ModFolder
                             * |-mod.json
                             * |-main_assembly.dll
                             * |-submodule1.dll
                             * |-submodule2.dll
                             * ...
                             * Hope submodule can get mod's declaration successfully
                             */
                        {
                            pModDeclare = mod;
                            return true;
                        }

                        if (string.Concat(mod.Name.Where(c => new Regex(@"\S").IsMatch(c.ToString()))) ==
                            pModAssembly.GetName().Name)
                        {
                            pModDeclare = mod;
                            return true;
                        }
                    }
                    break;
                case ModTypeEnum.BEPINEX:
                    if (mod.Name == pModAssembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title)
                    {
                        pModDeclare = mod;
                        return true;
                    }
                    break;
                case ModTypeEnum.RESOURCE_PACK:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        pModDeclare = null;
        return false;
    }
}
