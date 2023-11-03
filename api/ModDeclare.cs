
using Newtonsoft.Json;

namespace NeoModLoader.api;
[Serializable]
public class ModDeclare
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
    ModDeclare()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
    {
        
    }
    public ModDeclare(string pName, string pAuthor, string pVersion, string pDescription, string[] pDependencies, string[] pOptionalDependencies, string[] pIncompatibleWith)
    {
        Name = pName;
        Author = pAuthor;
        Version = pVersion;
        Description = pDescription;
        Dependencies = pDependencies;
        OptionalDependencies = pOptionalDependencies;
        IncompatibleWith = pIncompatibleWith;

        UUID = $"{Author}.{Name}";
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
        Description = modDeclare.Description;
        Dependencies = modDeclare.Dependencies;
        OptionalDependencies = modDeclare.OptionalDependencies;
        IncompatibleWith = modDeclare.IncompatibleWith;

        Dependencies ??= new string[0];
        OptionalDependencies ??= new string[0];
        IncompatibleWith ??= new string[0];
        
        UUID = $"{Author}.{Name}".Replace(" ", "_");
        FolderPath = Path.GetDirectoryName(pFilePath) ?? throw new Exception("Cannot get folder path from input file path");
    }
    [JsonProperty("name")]
    public string Name { get; private set; }
    public string UUID { get; private set; }
    [JsonProperty("author")]
    public string Author { get; private set; }
    [JsonProperty("version")]
    public string Version { get; private set; }
    [JsonProperty("description")]
    public string Description { get; private set; }

    [JsonProperty("Dependencies")] public string[] Dependencies { get; private set; }
    [JsonProperty("OptionalDependencies")]
    public string[] OptionalDependencies { get; private set; }
    [JsonProperty("IncompatibleWith")]
    public string[] IncompatibleWith { get; private set; }
    public string FolderPath { get; private set; } = null!;
    [JsonProperty("targetGameBuild")]
    public int TargetGameBuild { get; private set; }

    [JsonProperty("iconPath")] public string IconPath { get; private set; } = "icon.png";
}