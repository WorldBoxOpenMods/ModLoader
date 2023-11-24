
using System.Text;
using Newtonsoft.Json;

namespace NeoModLoader.api;

public enum ModTypeEnum
{
    NORMAL,
    BEPINEX
}
internal enum ModState
{
    DISABLED,
    LOADED,
    FAILED
}
[Serializable]
public class ModDeclare
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
    ModDeclare()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
    {
        
    }
    public ModDeclare(string pName, string pAuthor, string pIconPath, string pVersion, string pDescription, string pFolderPath, string[] pDependencies, string[] pOptionalDependencies, string[] pIncompatibleWith)
    {
        Name = pName;
        Author = pAuthor;
        IconPath = pIconPath;
        Version = pVersion;
        Description = pDescription;
        Dependencies = pDependencies ?? new string[0];
        OptionalDependencies = pOptionalDependencies ?? new string[0];
        IncompatibleWith = pIncompatibleWith ?? new string[0];

        UID = $"{Author}.{Name}".Replace(" ", "_");
        FolderPath = pFolderPath;
    }

    public ModDeclare(string pFilePath)
    {
        ModDeclare modDeclare = Newtonsoft.Json.JsonConvert.DeserializeObject<ModDeclare>(File.ReadAllText(pFilePath)) ?? throw new InvalidOperationException("Input Mod Config file path cannot be null");
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

        Dependencies ??= new string[0];
        OptionalDependencies ??= new string[0];
        IncompatibleWith ??= new string[0];
        
        UID = $"{Author}.{Name}".Replace(" ", "_");
        FolderPath = Path.GetDirectoryName(pFilePath) ?? throw new Exception("Cannot get folder path from input file path");
    }

    internal void SetRepoUrlToWorkshopPage(string id)
    {
        RepoUrl = $"https://steamcommunity.com/sharedfiles/filedetails/?id={id}";
    }
    internal void SetModType(ModTypeEnum modType)
    {
        if(modType < ModTypeEnum.NORMAL || modType > ModTypeEnum.BEPINEX)
            throw new ArgumentOutOfRangeException(nameof(modType), modType, null);
        ModType = modType;
    }
    [JsonProperty("name")]
    public string Name { get; private set; }
    public string UID { get; private set; }
    [JsonProperty("author")]
    public string Author { get; private set; }
    [JsonProperty("version")]
    public string Version { get; private set; }
    [JsonProperty("description")]
    public string Description { get; private set; }
    [JsonProperty("RepoUrl")]
    public string RepoUrl { get; private set; }
    [JsonProperty("Dependencies")] public string[] Dependencies { get; private set; }
    [JsonProperty("OptionalDependencies")]
    public string[] OptionalDependencies { get; private set; }
    [JsonProperty("IncompatibleWith")]
    public string[] IncompatibleWith { get; private set; }
    public string FolderPath { get; private set; } = null!;
    [JsonProperty("targetGameBuild")]
    public int TargetGameBuild { get; private set; }

    [JsonProperty("iconPath")] public string IconPath { get; private set; }
    [JsonProperty("ModType")] public ModTypeEnum ModType { get; private set; } = ModTypeEnum.NORMAL;
    public bool IsNCMSMod { get; internal set; } = false;
    public StringBuilder FailReason { get; } = new();
}