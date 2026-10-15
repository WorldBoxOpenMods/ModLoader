using NeoModLoader.constants;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NeoModLoader.services;

/// <summary>
///     Mod Hub preferences, stored in NML/mod_hub_settings.json.
/// </summary>
/// <remarks>
///     Search text is deliberately not stored, so an old search never narrows the list after a restart.
///     Kept out of mod_compile_records.json, which maps mod UIDs to compile records.
/// </remarks>
internal sealed class ModHubSettings
{
    private static readonly string default_path = Path.Combine(Paths.NMLPath, "mod_hub_settings.json");
    private static ModHubSettings instance;
    private List<string> browser_categories = new();
    private List<ulong> pending_downloads = new();

    [JsonProperty("manager_filter")]
    [JsonConverter(typeof(StringEnumConverter))]
    public InstalledModFilter ManagerFilter { get; set; } = InstalledModFilter.All;

    [JsonProperty("manager_sort")]
    [JsonConverter(typeof(StringEnumConverter))]
    public InstalledModSort ManagerSort { get; set; } = InstalledModSort.NameAscending;

    [JsonProperty("manager_search_mode")]
    [JsonConverter(typeof(StringEnumConverter))]
    public ModSearchMode ManagerSearchMode { get; set; } = ModSearchMode.Name;

    [JsonProperty("browser_sort")]
    [JsonConverter(typeof(StringEnumConverter))]
    public ListingSort BrowserSort { get; set; } = ListingSort.RecentlyUpdated;

    [JsonProperty("browser_updated_within")]
    [JsonConverter(typeof(StringEnumConverter))]
    public UpdatedWithin BrowserUpdatedWithin { get; set; } = UpdatedWithin.Anytime;

    [JsonProperty("browser_install")]
    [JsonConverter(typeof(StringEnumConverter))]
    public InstallFilter BrowserInstall { get; set; } = InstallFilter.All;

    [JsonProperty("browser_ai_content")]
    [JsonConverter(typeof(StringEnumConverter))]
    public AIContentFilter BrowserAIContent { get; set; } = AIContentFilter.Any;

    [JsonProperty("browser_categories")]
    public List<string> BrowserCategories
    {
        get => browser_categories ??= new List<string>();
        set => browser_categories = value;
    }

    [JsonProperty("browser_search_mode")]
    [JsonConverter(typeof(StringEnumConverter))]
    public ModSearchMode BrowserSearchMode { get; set; } = ModSearchMode.Name;

    /// <summary>
    ///     The player confirmed the notice that Workshop mods are unreviewed code.
    /// </summary>
    [JsonProperty("security_notice_accepted")]
    public bool SecurityNoticeAccepted { get; set; }

    /// <summary>
    ///     Workshop items Download Mods subscribed to that Steam has not installed yet. If Steam finishes one while
    ///     the game is closed, the startup scan saves it as disabled, as Download Mods would have.
    /// </summary>
    [JsonProperty("pending_downloads")]
    public List<ulong> PendingDownloads
    {
        get => pending_downloads ??= new List<ulong>();
        set => pending_downloads = value;
    }

    /// <summary>
    ///     Settings of this game session, read from disk on first use.
    /// </summary>
    public static ModHubSettings Instance => instance ??= Load(default_path);

    /// <summary>
    ///     Read settings, falling back to defaults (with a warning) when the file is invalid.
    /// </summary>
    public static ModHubSettings Load(string pPath)
    {
        if (!File.Exists(pPath)) return new ModHubSettings();
        try
        {
            return JsonConvert.DeserializeObject<ModHubSettings>(File.ReadAllText(pPath)) ??
                   throw new JsonException("The file is empty.");
        }
        catch (Exception e)
        {
            LogService.LogWarning($"Ignoring invalid Mod Hub settings at {pPath}: {e.Message}");
            return new ModHubSettings();
        }
    }

    public void Save()
    {
        Save(default_path);
    }

    public void Save(string pPath)
    {
        try
        {
            File.WriteAllText(pPath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
        catch (Exception e)
        {
            LogService.LogError($"Failed to save Mod Hub settings to {pPath}: {e.Message}");
        }
    }
}
