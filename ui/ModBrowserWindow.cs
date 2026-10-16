using NeoModLoader.api;
using NeoModLoader.constants;
using NeoModLoader.General;
using NeoModLoader.General.UI.Prefabs;
using NeoModLoader.services;
using NeoModLoader.ui.prefabs;
using NeoModLoader.utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NeoModLoader.ui;

/// <summary>
///     Browse the mods offered for NML on the Steam Workshop and download them.
/// </summary>
internal class ModBrowserWindow : AbstractListWindow<ModBrowserWindow, ModListing>
{
    private const float search_width = 172;
    private const float footer_button_width = 56;

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
    private RectTransform progress;
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

        RectTransform content = ModHubStyle.SetUpListWindow(BackgroundTransform, out Transform header,
            out Transform footer);
        ModHubStyle.StyleList(ContentTransform);
        ModHubUI.AddHubTabs(BackgroundTransform.parent, WindowId);
        CreateToolbar(header, ModHubStyle.InsertStrip(content, 1, "SearchLine"));
        CreateBottomBar(footer);

        // Drawn over the list, in this order.
        Transform scroll_view = ContentTransform.parent.parent;
        message_label = ModHubStyle.EmptyListLabel(ContentTransform, null);
        progress = ModHubStyle.ProgressBar(scroll_view);
        category_panel = CategoryFilterPanel.Create(scroll_view, query.Categories, OnCategoriesChanged);

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

        progress.parent.gameObject.SetActive(catalog.IsLoading && catalog.TotalCount > 0);
        if (catalog.TotalCount > 0) ModHubStyle.SetProgress(progress, (float)catalog.Items.Count / catalog.TotalCount);

        ModHubUI.SetInteractable(update_all_button, catalog.Status == CatalogStatus.Ready);
        ModHubUI.SetInteractable(cancel_button, catalog.IsLoading);
        // Retry takes the place of Cancel loading, which has nothing to stop after an error.
        cancel_button.gameObject.SetActive(!retry_button.gameObject.activeSelf);
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
        // Rows read author names when hovered; only a search by author has to filter again.
        if (query.SearchMode == ModSearchMode.Author && !string.IsNullOrWhiteSpace(search_text)) needs_refresh = true;
    }

    private void OnCategoriesChanged()
    {
        SaveSettings();
        Refresh();
    }

    private void CreateToolbar(Transform pFilters, Transform pSearchLine)
    {
        sort_button = ModHubStyle.IconButton(pFilters, "Sort", null,
            () => ChangeQuery(() => query.Sort = ModHubUI.NextValue(query.Sort)), "mod_hub_sort");
        updated_button = ModHubStyle.IconButton(pFilters, "Updated", ModHubUI.GameIcon("iconClock"),
            () => ChangeQuery(() => query.UpdatedWithin = ModHubUI.NextValue(query.UpdatedWithin)),
            "mod_hub_updated_within");
        install_button = ModHubStyle.IconButton(pFilters, "Install", null,
            () => ChangeQuery(() => query.Install = ModHubUI.NextValue(query.Install)), "mod_hub_install");
        category_button = ModHubStyle.IconButton(pFilters, "Category", null,
            () => category_panel.gameObject.SetActive(!category_panel.gameObject.activeSelf), "mod_hub_category");
        ai_button = ModHubStyle.IconButton(pFilters, "AIContent", null,
            () => ChangeQuery(() => query.AIContent = ModHubUI.NextValue(query.AIContent)), "mod_hub_ai");
        ModHubStyle.Spacer(pFilters);
        ModHubStyle.IconButton(pFilters, "Refresh", InternalResourcesGetter.GetReloadIcon(), StartLoading,
            "mod_hub_refresh");

        search_mode_button = ModHubStyle.IconButton(pSearchLine, "SearchMode", null,
            () => ChangeQuery(() => query.SearchMode = ModHubUI.NextValue(query.SearchMode)), "mod_hub_search_mode");
        TextInput search = ModHubStyle.SearchBox(pSearchLine, search_text, search_width, SetSearchText);
        ModHubStyle.IconButton(pSearchLine, "ClearSearch", ModHubUI.GameIcon("iconClose"),
            () => search.input.text = "", "mod_hub_clear_search");
    }

    private void CreateBottomBar(Transform pFooter)
    {
        status_label = ModHubStyle.Label(pFooter, "Status", ModHubStyle.FontSmall, ModHubStyle.TextMuted);
        // Up to three short lines: the count, waiting downloads and the page limit.
        status_label.resizeTextForBestFit = true;
        status_label.resizeTextMinSize = 4;
        status_label.resizeTextMaxSize = ModHubStyle.FontSmall;
        ModHubStyle.Size(status_label.gameObject, 0, ModHubStyle.ButtonHeight, 1);

        retry_button = FooterButton("Retry", "mod_hub_retry", StartLoading);
        update_all_button = FooterButton("UpdateAll", "mod_hub_update_all", UpdateAll, true);
        cancel_button = FooterButton("CancelLoading", "mod_hub_cancel_loading", ModDownloadService.Catalog.Cancel);

        SimpleButton FooterButton(string pName, string pTextKey, UnityAction pOnClick, bool pPrimary = false)
        {
            SimpleButton button = ModHubStyle.TextButton(pFooter, pName, pTextKey, pOnClick, pPrimary);
            ModHubStyle.Size(button.gameObject, footer_button_width, ModHubStyle.ButtonHeight);
            return button;
        }
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
            _ => "iconAges"
        });
        updated_button.TipButton.textOnClickDescription =
            ModHubUI.LocaleKey("mod_hub_updated_within", query.UpdatedWithin);
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
