using NeoModLoader.api;
using NeoModLoader.constants;
using NeoModLoader.General;
using NeoModLoader.General.UI.Prefabs;
using NeoModLoader.services;
using NeoModLoader.ui.prefabs;
using NeoModLoader.utils;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui;

/// <summary>
///     Browse the mods offered for NML on the Steam Workshop and download them.
/// </summary>
internal class ModBrowserWindow : AbstractListWindow<ModBrowserWindow, ModListing>
{
    private const float list_height = 210;

    /// <summary>
    ///     Kept for this game session only, see <see cref="ModHubSettings" />.
    /// </summary>
    private static string search_text = "";

    private readonly ModListingQuery query = new();
    private SimpleButton sort_button;
    private SimpleButton updated_button;
    private SimpleButton install_button;
    private SimpleButton category_button;
    private SimpleButton ai_button;
    private SimpleButton search_mode_button;
    private SimpleButton update_all_button;
    private SimpleButton cancel_button;
    private SimpleButton retry_button;
    private CategoryFilterPanel category_panel;
    private Text status_label;
    private Text message_label;
    private float scroll_position;
    private int shown_count;

    // Set while hidden or when author names arrive; the list is rebuilt once instead of on every change.
    private bool needs_refresh;

    private void Update()
    {
        if (!IsOpened || !needs_refresh) return;
        needs_refresh = false;
        Refresh();
    }

    protected override void Init()
    {
        LoadSettings();

        Transform scroll_view = BackgroundTransform.Find("Scroll View");
        scroll_view.GetComponent<RectTransform>().sizeDelta = new Vector2(232, list_height);
        scroll_view.localPosition = new Vector3(0, -20);
        scroll_view.Find("Viewport").localPosition = new Vector3(-131, list_height / 2);

        CreateToolbar();
        CreateBottomBar();
        message_label = ModHubUI.CreateText(BackgroundTransform, "Message", new Vector2(200, 60), new Vector2(0, 30),
            8, TextAnchor.MiddleCenter);
        retry_button = ModHubUI.CreateTextButton(BackgroundTransform, "Retry", "mod_hub_retry", new Vector2(60, 16),
            new Vector2(0, -15), StartLoading);
        category_panel = CategoryFilterPanel.Create(BackgroundTransform, query.Categories, OnCategoriesChanged);

        ModDownloadService.Catalog.Changed += () => needs_refresh = true;
        ModDownloadService.Authors.Changed += OnAuthorsChanged;
        ModDownloadService.Queue.Changed += OnDownloadChanged;
    }

    public override void OnNormalEnable()
    {
        Refresh();
        StartCoroutine(ModHubUI.RestoreScrollPosition(ContentTransform, scroll_position));
        if (ModDownloadService.Catalog.Status is not (CatalogStatus.Ready or CatalogStatus.Loading)) StartLoading();
    }

    public override void OnNormalDisable()
    {
        scroll_position = ContentTransform.localPosition.y;
        category_panel.gameObject.SetActive(false);
    }

    /// <summary>
    ///     Download a listing, after the one-time notice that Workshop mods are unreviewed code.
    /// </summary>
    internal void Download(ModListing pListing)
    {
        ModHubSettings settings = ModHubSettings.Instance;
        if (settings.SecurityNoticeAccepted)
        {
            ModDownloadService.Download(pListing);
            return;
        }

        ConfirmWindow.ShowWindow(LM.Get("mod_hub_security_notice"), () =>
        {
            settings.SecurityNoticeAccepted = true;
            settings.Save();
            ModDownloadService.Download(pListing);
        });
    }

    protected override AbstractListWindowItem<ModListing> CreateItemPrefab()
    {
        return ModBrowserRow.CreatePrefab();
    }

    private void StartLoading()
    {
        CatalogStatus steam = ModDownloadService.CheckSteam();
        if (steam != CatalogStatus.Ready)
        {
            ModDownloadService.Catalog.SetUnavailable(steam);
            return;
        }

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        WorkshopCatalog catalog = ModDownloadService.Catalog;
        await catalog.LoadAsync();
        if (query.Sort == ListingSort.Trending) await catalog.LoadTrendingAsync();
        await ModDownloadService.Authors.RequestAsync(catalog.Items.Select(listing => listing.AuthorId));
    }

    private void Refresh()
    {
        if (!IsOpened)
        {
            needs_refresh = true;
            return;
        }

        WorkshopCatalog catalog = ModDownloadService.Catalog;
        query.SearchText = search_text;
        List<ModListing> listings = query.Apply(catalog.Items, DateTime.UtcNow, ModDownloadService.GetStatus,
            ModDownloadService.Authors.Get, catalog.TrendingRank);
        ClearList();
        foreach (ModListing listing in listings) AddItemToList(listing);

        shown_count = listings.Count;
        UpdateToolbar();
        UpdateMessage();
        UpdateBottomBar();
    }

    private void UpdateMessage()
    {
        CatalogStatus status = ModDownloadService.Catalog.Status;
        string key = status switch
        {
            CatalogStatus.SteamUnavailable => "mod_hub_steam_unavailable",
            CatalogStatus.Offline => "mod_hub_steam_offline",
            CatalogStatus.Failed => "mod_hub_query_failed",
            _ when shown_count == 0 && status != CatalogStatus.Loading => "mod_hub_no_match",
            _ => null
        };
        message_label.gameObject.SetActive(key != null);
        if (key != null) message_label.text = LM.Get(key);
        retry_button.gameObject.SetActive(status is CatalogStatus.SteamUnavailable or CatalogStatus.Offline
            or CatalogStatus.Failed);
    }

    private void UpdateBottomBar()
    {
        WorkshopCatalog catalog = ModDownloadService.Catalog;
        WorkshopDownloadQueue queue = ModDownloadService.Queue;
        string text = catalog.Status switch
        {
            CatalogStatus.Loading => string.Format(LM.Get("mod_hub_loading"), catalog.Items.Count,
                Math.Max(catalog.TotalCount, 0)),
            CatalogStatus.Cancelled => string.Format(LM.Get("mod_hub_loading_stopped"), catalog.Items.Count),
            _ => string.Format(LM.Get("mod_hub_shown_count"), shown_count, catalog.Items.Count)
        };
        if (queue.UnfinishedCount > 0)
        {
            text += "\n" + string.Format(LM.Get("mod_hub_downloads_waiting"), queue.UnfinishedCount);
        }
        if (catalog.ReachedPageCap) text += "\n" + LM.Get("mod_hub_page_cap");
        status_label.text = text;

        ModHubUI.SetInteractable(update_all_button, catalog.Status == CatalogStatus.Ready);
        ModHubUI.SetInteractable(cancel_button, catalog.IsLoading);
    }

    private void OnDownloadChanged(DownloadEntry pEntry)
    {
        if (ItemMap.TryGetValue(pEntry.Listing, out AbstractListWindowItem<ModListing> row))
        {
            ((ModBrowserRow)row).UpdateStatus();
        }

        if (IsOpened) UpdateBottomBar();
    }

    private void OnAuthorsChanged()
    {
        if (query.SearchMode == ModSearchMode.Author && !string.IsNullOrWhiteSpace(search_text))
        {
            needs_refresh = true;
            return;
        }

        foreach (AbstractListWindowItem<ModListing> row in ItemMap.Values) ((ModBrowserRow)row).UpdateAuthor();
    }

    private void OnCategoriesChanged()
    {
        SaveSettings();
        Refresh();
    }

    private void CreateToolbar()
    {
        Vector2 size = new(20, 20);
        sort_button = ToolbarButton("Sort", -104, 118, () => ChangeQuery(() => query.Sort = ModHubUI.NextValue(query.Sort)),
            "mod_hub_sort");
        updated_button = ToolbarButton("Updated", -82, 118,
            () => ChangeQuery(() => query.UpdatedWithin = ModHubUI.NextValue(query.UpdatedWithin)), "mod_hub_updated_within");
        install_button = ToolbarButton("Install", -60, 118,
            () => ChangeQuery(() => query.Install = ModHubUI.NextValue(query.Install)), "mod_hub_install");
        category_button = ToolbarButton("Category", -38, 118,
            () => category_panel.gameObject.SetActive(!category_panel.gameObject.activeSelf), "mod_hub_category");
        ai_button = ToolbarButton("AIContent", -16, 118,
            () => ChangeQuery(() => query.AIContent = ModHubUI.NextValue(query.AIContent)), "mod_hub_ai");
        ToolbarButton("Refresh", 105, 118, StartLoading, "mod_hub_refresh").Icon.sprite =
            InternalResourcesGetter.GetReloadIcon();
        search_mode_button = ToolbarButton("SearchMode", -104, 96,
            () => ChangeQuery(() => query.SearchMode = ModHubUI.NextValue(query.SearchMode)), "mod_hub_search_mode");

        TextInput search = TextInput.Instantiate(BackgroundTransform, pName: "Search");
        search.Setup(search_text, SetSearchText);
        search.SetSize(new Vector2(184, 18));
        search.transform.localPosition = new Vector3(1, 96);
        // Filter while typing, not only when the edit ends.
        search.input.onValueChanged.AddListener(SetSearchText);
        ModHubUI.CreateIconButton(BackgroundTransform, "ClearSearch", ModHubUI.GameIcon("iconClose"),
            new Vector2(16, 16), new Vector2(105, 96), () => search.input.text = "", "mod_hub_clear_search");

        SimpleButton ToolbarButton(string pName, float pX, float pY, Action pOnClick, string pTooltipKey)
        {
            return ModHubUI.CreateIconButton(BackgroundTransform, pName, null, size, new Vector2(pX, pY),
                () => pOnClick(), pTooltipKey);
        }
    }

    private void CreateBottomBar()
    {
        status_label = ModHubUI.CreateText(BackgroundTransform, "Status", new Vector2(112, 24),
            new Vector2(-59, -138), 6, TextAnchor.MiddleLeft);
        status_label.color = ModHubUI.MutedTextColor;
        Vector2 size = new(54, 16);
        update_all_button = ModHubUI.CreateTextButton(BackgroundTransform, "UpdateAll", "mod_hub_update_all", size,
            new Vector2(30, -138), UpdateAll);
        cancel_button = ModHubUI.CreateTextButton(BackgroundTransform, "CancelLoading", "mod_hub_cancel_loading",
            size, new Vector2(88, -138), ModDownloadService.Catalog.Cancel);
    }

    private void UpdateAll()
    {
        if (ModDownloadService.UpdateAll() == 0)
        {
            InformationWindow.ShowWindow(LM.Get("mod_hub_all_up_to_date"));
        }
    }

    private void SetSearchText(string pText)
    {
        if (pText == search_text) return;
        search_text = pText;
        Refresh();
    }

    private void ChangeQuery(Action pChange)
    {
        pChange();
        SaveSettings();
        if (query.Sort == ListingSort.Trending) _ = ModDownloadService.Catalog.LoadTrendingAsync();
        Refresh();
    }

    private void LoadSettings()
    {
        ModHubSettings settings = ModHubSettings.Instance;
        query.Sort = settings.BrowserSort;
        query.UpdatedWithin = settings.BrowserUpdatedWithin;
        query.Install = settings.BrowserInstall;
        query.AIContent = settings.BrowserAIContent;
        query.SearchMode = settings.BrowserSearchMode;
        foreach (string category in settings.BrowserCategories.Where(CoreConstants.ModCategories.Contains))
        {
            query.Categories.Add(category);
        }
    }

    private void SaveSettings()
    {
        ModHubSettings settings = ModHubSettings.Instance;
        settings.BrowserSort = query.Sort;
        settings.BrowserUpdatedWithin = query.UpdatedWithin;
        settings.BrowserInstall = query.Install;
        settings.BrowserAIContent = query.AIContent;
        settings.BrowserSearchMode = query.SearchMode;
        settings.BrowserCategories = query.Categories.ToList();
        settings.Save();
    }

    private void UpdateToolbar()
    {
        ModHubUI.ShowChoice(sort_button, ModHubUI.LocaleKey("mod_hub_sort", query.Sort), query.Sort switch
        {
            ListingSort.MostSubscribed => "iconPopulation",
            ListingSort.Trending => "iconFavoriteStar",
            ListingSort.TopRated => "iconRenown",
            ListingSort.Newest => "iconNewWorld",
            ListingSort.Name => "iconArrowDOWN",
            _ => "iconClock"
        });
        ModHubUI.ShowChoice(updated_button, ModHubUI.LocaleKey("mod_hub_updated_within", query.UpdatedWithin),
            query.UpdatedWithin == UpdatedWithin.Anytime ? "iconClockX1" : "iconClockX3");
        ModHubUI.ShowChoice(install_button, ModHubUI.LocaleKey("mod_hub_install", query.Install), query.Install switch
        {
            InstallFilter.NotInstalled => "iconArrowDOWN",
            InstallFilter.Installed => "iconOn",
            InstallFilter.UpdateAvailable => "iconSaveCloud",
            _ => "iconBooks"
        });
        // The selected categories are not a fixed text, so they go through a locale key set here.
        LM.AddToCurrentLocale("mod_hub_category_selected", query.Categories.Count == 0
            ? LM.Get("mod_hub_category_any")
            : string.Join(", ", query.Categories));
        ModHubUI.ShowChoice(category_button, "mod_hub_category_selected", "iconFavoritesList");
        ModHubUI.ShowChoice(ai_button, ModHubUI.LocaleKey("mod_hub_ai_filter", query.AIContent),
            query.AIContent == AIContentFilter.Any ? "iconBrain" : "iconWarning");
        ModHubUI.ShowChoice(search_mode_button, ModHubUI.LocaleKey("mod_hub_browser_search_mode", query.SearchMode),
            query.SearchMode == ModSearchMode.Author ? "iconInterestingPeople" : "iconCityName");
    }

}
