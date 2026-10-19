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
///     Lists every recognized mod with its state, and lets the player enable, disable, configure and inspect them.
/// </summary>
internal class ModManagerWindow : ModHubListWindow<ModManagerWindow, InstalledMod>
{
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

    protected override void InitHub()
    {
        ModHubSettings settings = ModHubSettings.Instance;
        query.Filter = settings.ManagerFilter;
        query.Sort = settings.ManagerSort;
        query.SearchMode = settings.ManagerSearchMode;

        CreateToolbar(Frame.Header);
        CreateBottomButtons(Frame.Footer);
        CreateVersionLine(Frame.transform);
        empty_label = CreateEmptyMessage("mod_hub_no_match");
    }

    /// <summary>
    ///     Read the mods again and rebuild the list.
    /// </summary>
    internal override void Refresh()
    {
        query.SearchText = search_text;
        List<InstalledMod> mods = query.Apply(ModManagerService.GetInstalledMods());
        ShowItems(mods);

        empty_label.gameObject.SetActive(mods.Count == 0);

        UpdateToolbar();
        ModHubUI.SetInteractable(enable_all_button, ModManagerService.CountDisabled() > 0);
        ModHubUI.SetInteractable(disable_all_button, ModManagerService.CountEnabled() > 0);
    }

    protected override AbstractListWindowItem<InstalledMod> CreateItemPrefab()
    {
        return ModManagerRow.CreatePrefab();
    }

    private void CreateToolbar(ModHubToolbar pHeader)
    {
        filter_button = pHeader.AddIcon("Filter", null,
            () => ChangeQuery(() => query.Filter = ModHubUI.NextValue(query.Filter)), "mod_hub_filter");
        sort_button = pHeader.AddIcon("Sort", null,
            () => ChangeQuery(() => query.Sort = ModHubUI.NextValue(query.Sort)), "mod_hub_sort");
        search_mode_button = pHeader.AddIcon("SearchMode", null,
            () => ChangeQuery(() => query.SearchMode = ModHubUI.NextValue(query.SearchMode)), "mod_hub_search_mode");

        TextInput search = pHeader.AddSearch(search_text, SetSearchText);
        pHeader.AddIcon("ClearSearch", ModHubUI.GameIcon("iconClose"), () => search.input.text = "",
            "mod_hub_clear_search");
    }

    private void SetSearchText(string pText)
    {
        if (pText == search_text) return;
        search_text = pText;
        Refresh();
        ResetScroll();
    }

    private void CreateBottomButtons(ModHubToolbar pFooter)
    {
        enable_all_button = pFooter.AddText("EnableAll", "mod_hub_enable_all", ConfirmEnableAll);
        disable_all_button = pFooter.AddText("DisableAll", "mod_hub_disable_all", ConfirmDisableAll);
        pFooter.AddText("OpenModsFolder", "mod_hub_open_mods_folder",
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
        var tip = line.AddComponent<TipButton>();
        tip.override_click_scale_animation = true;
        tip.overridden_click_scale_animation = 1;
        tip.setHoverAction(() =>
            ModHubUI.ShowTextTooltip(line, version, LM.Get("NeoModLoader Report")));
        line.SetLayoutSize(pHeight: version_line_height);
        var layout = line.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 4;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = layout.childForceExpandHeight = false;
        var icon = new GameObject("Icon", typeof(Image));
        icon.transform.SetParent(line.transform, false);
        icon.GetComponent<Image>().sprite = InternalResourcesGetter.GetIcon();
        icon.SetLayoutSize(version_line_height, version_line_height);
        OT.CreateText(line.transform, "Text", 7, pColor: Color.gray).text = version;
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
        ResetScroll();
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
