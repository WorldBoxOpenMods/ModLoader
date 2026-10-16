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
///     Lists every recognized mod with its state, and lets the player enable, disable, configure and inspect them.
/// </summary>
internal class ModManagerWindow : AbstractListWindow<ModManagerWindow, InstalledMod>
{
    // Fills the search line, so its clear button lines up with the end of the strip.
    private const float search_width = 120;
    private const float version_line_height = 10;

    /// <summary>
    ///     Kept for this game session only, see <see cref="ModHubSettings" />.
    /// </summary>
    private static string search_text = "";

    private readonly InstalledModQuery query = new();
    private SimpleButton filter_button;
    private SimpleButton sort_button;
    private SimpleButton search_mode_button;
    private SimpleButton enable_all_button;
    private SimpleButton disable_all_button;
    private Text empty_label;
    private float scroll_position;

    protected override void Init()
    {
        ModHubSettings settings = ModHubSettings.Instance;
        query.Filter = settings.ManagerFilter;
        query.Sort = settings.ManagerSort;
        query.SearchMode = settings.ManagerSearchMode;

        RectTransform content = ModHubStyle.SetUpListWindow(BackgroundTransform, out Transform header,
            out Transform footer);
        ModHubStyle.StyleList(ContentTransform);
        ModHubUI.AddHubTabs(BackgroundTransform.parent, WindowId);
        CreateToolbar(header);
        CreateBottomButtons(footer);
        CreateVersionLine(content);
        empty_label = ModHubStyle.EmptyListLabel(ContentTransform, "mod_hub_no_match");
    }

    public override void OnNormalEnable()
    {
        Refresh();
        StartCoroutine(ModHubUI.RestoreScrollPosition(ContentTransform, scroll_position));
    }

    public override void OnNormalDisable()
    {
        scroll_position = ContentTransform.localPosition.y;
    }

    /// <summary>
    ///     Read the mods again and rebuild the list.
    /// </summary>
    internal void Refresh()
    {
        query.SearchText = search_text;
        ClearList();
        List<InstalledMod> mods = query.Apply(ModManagerService.GetInstalledMods());
        foreach (InstalledMod mod in mods)
        {
            AddItemToList(mod);
        }

        empty_label.gameObject.SetActive(mods.Count == 0);

        UpdateToolbar();
        ModHubUI.SetInteractable(enable_all_button, ModManagerService.CountDisabled() > 0);
        ModHubUI.SetInteractable(disable_all_button, ModManagerService.CountEnabled() > 0);
    }

    protected override AbstractListWindowItem<InstalledMod> CreateItemPrefab()
    {
        return ModManagerRow.CreatePrefab();
    }

    private void CreateToolbar(Transform pHeader)
    {
        filter_button = ModHubStyle.ToolbarButton(pHeader, "Filter", null,
            () => ChangeQuery(() => query.Filter = ModHubUI.NextValue(query.Filter)), "mod_hub_filter");
        sort_button = ModHubStyle.ToolbarButton(pHeader, "Sort", null,
            () => ChangeQuery(() => query.Sort = ModHubUI.NextValue(query.Sort)), "mod_hub_sort");
        search_mode_button = ModHubStyle.ToolbarButton(pHeader, "SearchMode", null,
            () => ChangeQuery(() => query.SearchMode = ModHubUI.NextValue(query.SearchMode)), "mod_hub_search_mode");

        TextInput search = ModHubStyle.SearchBox(pHeader, search_text, search_width, SetSearchText);
        ModHubStyle.ToolbarButton(pHeader, "ClearSearch", ModHubUI.GameIcon("iconClose"), () => search.input.text = "",
            "mod_hub_clear_search");
    }

    private void SetSearchText(string pText)
    {
        if (pText == search_text) return;
        search_text = pText;
        Refresh();
    }

    private void CreateBottomButtons(Transform pFooter)
    {
        enable_all_button = ModHubStyle.TextButton(pFooter, "EnableAll", "mod_hub_enable_all", ConfirmEnableAll);
        disable_all_button = ModHubStyle.TextButton(pFooter, "DisableAll", "mod_hub_disable_all", ConfirmDisableAll);
        ModHubStyle.TextButton(pFooter, "OpenModsFolder", "mod_hub_open_mods_folder",
            () => ModManagerService.OpenFolder(Paths.ModsPath));
    }

    /// <summary>
    ///     NML's version under the buttons. Clicking it opens NML's repository to report bugs.
    /// </summary>
    private static void CreateVersionLine(Transform pContent)
    {
        string version = "NeoModLoader " + WorldBoxMod.NeoModLoaderAssembly.GetName().Version;
        var line = new GameObject("Version", typeof(Image), typeof(Button));
        line.transform.SetParent(pContent, false);
        line.GetComponent<Image>().color = Color.clear;
        line.GetComponent<Button>().onClick.AddListener(() => Application.OpenURL(CoreConstants.RepoURL));
        ModHubStyle.HoverTip(line).setHoverAction(() =>
            ModHubUI.ShowTextTooltip(line, version, LM.Get("NeoModLoader Report")));
        ModHubStyle.Size(line, pHeight: version_line_height);
        ModHubStyle.Horizontal(line, ModHubStyle.SpaceS).childAlignment = TextAnchor.MiddleCenter;

        ModHubStyle.Icon(line.transform, "Icon", InternalResourcesGetter.GetIcon(), version_line_height);
        ModHubStyle.Label(line.transform, "Text", ModHubStyle.FontSmall, ModHubStyle.TextMuted).text = version;
    }

    private void ConfirmEnableAll()
    {
        string message = string.Format(LM.Get("mod_hub_enable_all_confirm"), ModManagerService.CountDisabled());
        ConfirmWindow.ShowWindow(message, ModManagerService.EnableAll);
    }

    private void ConfirmDisableAll()
    {
        string message = string.Format(LM.Get("mod_hub_disable_all_confirm"), ModManagerService.CountEnabled());
        ConfirmWindow.ShowWindow(message, ModManagerService.DisableAll);
    }

    private void ChangeQuery(Action pChange)
    {
        pChange();
        ModHubSettings settings = ModHubSettings.Instance;
        settings.ManagerFilter = query.Filter;
        settings.ManagerSort = query.Sort;
        settings.ManagerSearchMode = query.SearchMode;
        settings.Save();
        Refresh();
    }

    private void UpdateToolbar()
    {
        ModHubUI.ShowChoice(filter_button, ModHubUI.LocaleKey("mod_hub_filter", query.Filter), query.Filter switch
        {
            InstalledModFilter.Enabled => "iconOn",
            InstalledModFilter.Disabled => "iconOff",
            InstalledModFilter.Problems => "iconWarning",
            InstalledModFilter.Updates => "iconSaveCloud",
            _ => "iconBooks"
        });
        ModHubUI.ShowChoice(sort_button, ModHubUI.LocaleKey("mod_hub_sort", query.Sort), query.Sort switch
        {
            InstalledModSort.NameDescending => "iconArrowUP",
            InstalledModSort.RecentlyChanged => "iconClock",
            _ => "iconArrowDOWN"
        });
        ModHubUI.ShowChoice(search_mode_button, ModHubUI.LocaleKey("mod_hub_search_mode", query.SearchMode),
            query.SearchMode == ModSearchMode.Author ? "iconInterestingPeople" : "iconCityName");
    }

}
