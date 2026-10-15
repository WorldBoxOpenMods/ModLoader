using System.Collections;
using NeoModLoader.api;
using NeoModLoader.constants;
using NeoModLoader.General;
using NeoModLoader.General.UI.Prefabs;
using NeoModLoader.services;
using NeoModLoader.ui.prefabs;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui;

/// <summary>
///     Lists every recognized mod with its state, and lets the player enable, disable, configure and inspect them.
/// </summary>
internal class ModManagerWindow : AbstractListWindow<ModManagerWindow, InstalledMod>
{
    private const float list_height = 230;

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

        // Make room for the toolbar above the list and the buttons below it.
        Transform scroll_view = BackgroundTransform.Find("Scroll View");
        scroll_view.GetComponent<RectTransform>().sizeDelta = new Vector2(232, list_height);
        scroll_view.localPosition = new Vector3(0, -11);
        scroll_view.Find("Viewport").localPosition = new Vector3(-131, list_height / 2);

        CreateToolbar();
        CreateBottomButtons();
        empty_label = ModHubUI.CreateText(BackgroundTransform, "NoMatch", new Vector2(200, 30), new Vector2(0, 60), 8,
            TextAnchor.MiddleCenter);
        empty_label.color = ModHubUI.MutedTextColor;
        ModHubUI.SetLocaleKey(empty_label, "mod_hub_no_match");
    }

    public override void OnNormalEnable()
    {
        Refresh();
        StartCoroutine(RestoreScrollPosition());
    }

    public override void OnNormalDisable()
    {
        scroll_position = ContentTransform.localPosition.y;
    }

    /// <summary>
    ///     The game scrolls every window back to the top when it is shown. Coming back from a mod's info or config
    ///     page should keep the player's place in a long list, so the position is restored one frame later.
    /// </summary>
    private IEnumerator RestoreScrollPosition()
    {
        yield return null;
        Vector3 position = ContentTransform.localPosition;
        position.y = scroll_position;
        ContentTransform.localPosition = position;
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

    private void CreateToolbar()
    {
        Vector2 size = new(20, 20);
        filter_button = ModHubUI.CreateIconButton(BackgroundTransform, "Filter", null, size, new Vector2(-104, 118),
            () => ChangeQuery(() => query.Filter = Next(query.Filter)), "mod_hub_filter");
        sort_button = ModHubUI.CreateIconButton(BackgroundTransform, "Sort", null, size, new Vector2(-82, 118),
            () => ChangeQuery(() => query.Sort = Next(query.Sort)), "mod_hub_sort");
        search_mode_button = ModHubUI.CreateIconButton(BackgroundTransform, "SearchMode", null, size,
            new Vector2(-60, 118), () => ChangeQuery(() => query.SearchMode = Next(query.SearchMode)),
            "mod_hub_search_mode");

        TextInput search = TextInput.Instantiate(BackgroundTransform, pName: "Search");
        search.Setup(search_text, SetSearchText);
        search.SetSize(new Vector2(140, 18));
        search.transform.localPosition = new Vector3(23, 118);
        // Filter while typing, not only when the edit ends.
        search.input.onValueChanged.AddListener(SetSearchText);

        ModHubUI.CreateIconButton(BackgroundTransform, "ClearSearch", ModHubUI.GameIcon("iconClose"),
            new Vector2(16, 16), new Vector2(105, 118), () => search.input.text = "", "mod_hub_clear_search");
    }

    private void SetSearchText(string pText)
    {
        if (pText == search_text) return;
        search_text = pText;
        Refresh();
    }

    private void CreateBottomButtons()
    {
        Vector2 size = new(72, 16);
        enable_all_button = ModHubUI.CreateTextButton(BackgroundTransform, "EnableAll", "mod_hub_enable_all", size,
            new Vector2(-76, -138), ConfirmEnableAll);
        disable_all_button = ModHubUI.CreateTextButton(BackgroundTransform, "DisableAll", "mod_hub_disable_all",
            size, new Vector2(0, -138), ConfirmDisableAll);
        ModHubUI.CreateTextButton(BackgroundTransform, "OpenModsFolder", "mod_hub_open_mods_folder", size,
            new Vector2(76, -138), () => ModManagerService.OpenFolder(Paths.ModsPath));
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
        ShowChoice(filter_button, ModHubUI.LocaleKey("mod_hub_filter", query.Filter), query.Filter switch
        {
            InstalledModFilter.Enabled => "iconOn",
            InstalledModFilter.Disabled => "iconOff",
            InstalledModFilter.Problems => "iconWarning",
            _ => "iconBooks"
        });
        ShowChoice(sort_button, ModHubUI.LocaleKey("mod_hub_sort", query.Sort), query.Sort switch
        {
            InstalledModSort.NameDescending => "iconArrowUP",
            InstalledModSort.RecentlyChanged => "iconClock",
            _ => "iconArrowDOWN"
        });
        ShowChoice(search_mode_button, ModHubUI.LocaleKey("mod_hub_search_mode", query.SearchMode),
            query.SearchMode == ModSearchMode.Author ? "iconInterestingPeople" : "iconCityName");
    }

    private static void ShowChoice(SimpleButton pButton, string pDescriptionKey, string pIconName)
    {
        pButton.Icon.sprite = ModHubUI.GameIcon(pIconName);
        pButton.TipButton.textOnClickDescription = pDescriptionKey;
    }

    private static T Next<T>(T pValue) where T : Enum
    {
        var values = (T[])Enum.GetValues(typeof(T));
        return values[(Array.IndexOf(values, pValue) + 1) % values.Length];
    }
}
