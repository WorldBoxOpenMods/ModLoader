using NeoModLoader.api;
using NeoModLoader.constants;
using NeoModLoader.General;
using NeoModLoader.General.UI;
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
internal class ModBrowserWindow : ModHubListWindow<ModBrowserWindow, ModListing>
{
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
    private int shown_count;

    // Set while hidden or when author names arrive; the list is rebuilt once instead of on every change.
    private bool needs_refresh;

    private void Update()
    {
        if (!IsOpened || !needs_refresh) return;
        needs_refresh = false;
        Refresh();
    }

    protected override void InitHub()
    {
        LoadSettings();

        CreateToolbar(Frame.Header, Frame.InsertToolbar("SearchLine", 1));
        CreateBottomBar(Frame.Footer);

        // Drawn over the list, in this order.
        Transform scroll_view = ContentTransform.parent.parent;
        message_label = CreateEmptyMessage();
        CreateProgressBar(scroll_view);
        category_panel = CategoryFilterPanel.Create(scroll_view, query.Categories, OnCategoriesChanged);

        ModDownloadService.Catalog.Changed += OnCatalogChanged;
        ModDownloadService.Authors.Changed += OnAuthorsChanged;
        ModDownloadService.Queue.Changed += OnDownloadChanged;
    }

    public override void OnNormalEnable()
    {
        base.OnNormalEnable();
        if (ModDownloadService.Catalog.Status is not (CatalogStatus.Ready or CatalogStatus.Loading)) StartLoading();
    }

    public override void OnNormalDisable()
    {
        base.OnNormalDisable();
        category_panel.gameObject.SetActive(false);
    }

    private void OnCatalogChanged() => needs_refresh = true;

    private void OnDestroy()
    {
        ModDownloadService.Catalog.Changed -= OnCatalogChanged;
        ModDownloadService.Authors.Changed -= OnAuthorsChanged;
        ModDownloadService.Queue.Changed -= OnDownloadChanged;
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

    internal override void Refresh()
    {
        WorkshopCatalog catalog = ModDownloadService.Catalog;
        query.SearchText = search_text;
        List<ModListing> listings = query.Apply(catalog.Items, DateTime.UtcNow, ModDownloadService.GetStatus,
            ModDownloadService.Authors.Get, catalog.TrendingRank);
        ShowItems(listings);

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
        if (catalog.TotalCount > 0) progress.anchorMax = new Vector2(
            Mathf.Clamp01((float)catalog.Items.Count / catalog.TotalCount), 1);

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
        // Installing changes whether an item matches the selected install filter.
        if (pEntry.IsFinished) needs_refresh = true;
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
        ResetScroll();
    }

    private void CreateToolbar(ModHubToolbar pFilters, ModHubToolbar pSearchLine)
    {
        sort_button = pFilters.AddIcon("Sort", null,
            () => ChangeQuery(() => query.Sort = ModHubUI.NextValue(query.Sort)), "mod_hub_sort");
        updated_button = pFilters.AddIcon("Updated", ModHubUI.GameIcon("iconClock"),
            () => ChangeQuery(() => query.UpdatedWithin = ModHubUI.NextValue(query.UpdatedWithin)),
            "mod_hub_updated_within");
        install_button = pFilters.AddIcon("Install", null,
            () => ChangeQuery(() => query.Install = ModHubUI.NextValue(query.Install)), "mod_hub_install");
        category_button = pFilters.AddIcon("Category", null,
            () => category_panel.gameObject.SetActive(!category_panel.gameObject.activeSelf), "mod_hub_category");
        ai_button = pFilters.AddIcon("AIContent", null,
            () => ChangeQuery(() => query.AIContent = ModHubUI.NextValue(query.AIContent)), "mod_hub_ai");
        pFilters.AddSpacer();
        pFilters.AddIcon("Refresh", InternalResourcesGetter.GetReloadIcon(), StartLoading,
            "mod_hub_refresh");

        search_mode_button = pSearchLine.AddIcon("SearchMode", null,
            () => ChangeQuery(() => query.SearchMode = ModHubUI.NextValue(query.SearchMode)), "mod_hub_search_mode");
        TextInput search = pSearchLine.AddSearch(search_text, SetSearchText);
        pSearchLine.AddIcon("ClearSearch", ModHubUI.GameIcon("iconClose"),
            () => search.input.text = "", "mod_hub_clear_search");
    }

    private void CreateBottomBar(ModHubToolbar pFooter)
    {
        ModHubToolbar status_line = Frame.InsertToolbar("StatusLine", pFooter.transform.GetSiblingIndex(), 24);
        status_label = OT.CreateText(status_line.transform, "Status", 7, pColor: Color.gray);
        status_label.gameObject.SetLayoutSize(pFlexibleWidth: 1, pFlexibleHeight: 1);

        retry_button = pFooter.AddText("Retry", "mod_hub_retry", StartLoading);
        update_all_button = pFooter.AddText("UpdateAll", "mod_hub_update_all", UpdateAll, true);
        cancel_button = pFooter.AddText("CancelLoading", "mod_hub_cancel_loading", ModDownloadService.Catalog.Cancel);
    }

    private void CreateProgressBar(Transform pParent)
    {
        var bar = new GameObject("Progress", typeof(Image));
        bar.transform.SetParent(pParent, false);
        bar.GetComponent<Image>().color = new Color(0, 0, 0, 0.35f);
        bar.GetComponent<Image>().raycastTarget = false;
        var rect = (RectTransform)bar.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = new Vector2(1, 0);
        rect.pivot = new Vector2(0.5f, 0);
        rect.sizeDelta = new Vector2(0, 2);
        var fill = new GameObject("Fill", typeof(Image));
        fill.transform.SetParent(bar.transform, false);
        fill.GetComponent<Image>().color = new Color(0.45f, 0.9f, 0.45f);
        fill.GetComponent<Image>().raycastTarget = false;
        progress = (RectTransform)fill.transform;
        progress.Stretch();
    }

    private void UpdateAll()
    {
        CatalogStatus status = ModDownloadService.CheckSteam();
        if (status != CatalogStatus.Ready)
        {
            ModDownloadService.Catalog.SetUnavailable(status);
            return;
        }
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
        ResetScroll();
    }

    private void ChangeQuery(Action pChange)
    {
        pChange();
        SaveSettings();
        if (query.Sort == ListingSort.Trending) _ = ModDownloadService.Catalog.LoadTrendingAsync();
        Refresh();
        ResetScroll();
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
