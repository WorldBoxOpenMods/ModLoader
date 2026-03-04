using System.Reflection.Emit;
using HarmonyLib;
using NeoModLoader.AndroidCompatibilityModule;
using NeoModLoader.constants;
using NeoModLoader.utils;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using static NeoModLoader.AndroidCompatibilityModule.Converter;
using UnityEngine.UI;
using Object = UnityEngine.Object;
namespace NeoModLoader.General.UI.Tab;

public static class TabManager
{
    private const int tab_count_each_line = 14;
    private const float check_new_tabs_interval = 1;
    private const float shrink_coef = 0.79f;
    private const float default_tab_width = 43f;
    private const float default_tab_height = 18f;
    private const float default_icon_width = 33f;
    private const float default_icon_height = 11f;
    private const float default_tab_y = 2.0082f;

    private static readonly Transform tab_entry_container =
        CanvasMain.instance.canvas_ui.transform.Find("CanvasBottom/BottomElements/BottomElementsMover/TabsButtons");

    private static readonly Transform tab_container = CanvasMain.instance.canvas_ui.transform.Find(
        "CanvasBottom/BottomElements/BottomElementsMover/CanvasScrollView/Scroll View/Viewport/Content/Power Tabs");

    private static readonly List<Button>
        tab_entries = new (PowerTabController.instance._buttons.C()); // To avoid other mods' modifies

    private static readonly List<string> tab_names = new();
    private static readonly HashSet<string> tab_names_set = new();

    private static float _check_timer;
    private static Vector3 _last_mouse_pos = Vector3.zero;
    public static readonly TabMain TabMain = new();
    public static readonly TabDrawing TabDrawing = new();
    public static readonly TabKingdoms TabKingdoms = new();
    public static readonly TabCreatures TabCreatures = new();
    public static readonly TabNature TabNature = new();
    public static readonly TabOther TabOther = new();

    private static readonly List<string> common_fix_for_tab_button = new()
    {
        "newtab", "new_tab", "tab", "newbutton", "new_button", "button", "additional", "_", " "
    };

    internal static void _init()
    {
        HarmonyLib.Harmony.CreateAndPatchAll(typeof(TabManager), Others.harmony_id);
        var _tab_names = PowerTabNames.GetNames();
        for (int i = 1; i < _tab_names.Count; i++)
        {
            tab_names.Add(_tab_names[i]);
            tab_names_set.Add(_tab_names[i]);
        }

        TabMain.Init();
        TabDrawing.Init();
        TabKingdoms.Init();
        TabCreatures.Init();
        TabNature.Init();
        TabOther.Init();
    }

    [HarmonyPrefix, HarmonyPatch(typeof(PowerTabController), nameof(PowerTabController.getNext))]
    private static bool _getNext_Patch(string pActiveTab, ref Button __result)
    {
        tab_entries.Sort((Button a, Button b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
        __result = tab_entries[(tab_names.IndexOf(pActiveTab) + 1) % tab_entries.Count];
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(PowerTabController), nameof(PowerTabController.getPrev))]
    private static bool _getPrev_Patch(string pActiveTab, ref Button __result)
    {
        tab_entries.Sort((Button a, Button b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
        int index = tab_names.IndexOf(pActiveTab);
        if (index < 0)
        {
            index = 1;
        }

        if (index == 0)
        {
            index = tab_entries.Count;
        }

        __result = tab_entries[index - 1];
        return false;
    }

    internal static void _checkNewTabs()
    {
        if (_check_timer > 0)
        {
            _check_timer -= Time.deltaTime;
            return;
        }

        _check_timer = check_new_tabs_interval;

        PowersTab[] curr_tabs = tab_container.GetComponentsInChildren<PowersTab>(true);
        Button[] curr_tab_entries = tab_entry_container.GetComponentsInChildren<Button>(false);

        if (curr_tab_entries.Length == tab_entries.Count) return;

        string GetTabMainPart(string name)
        {
            return common_fix_for_tab_button.Aggregate(name.ToLower(), (current, fix) => current.Replace(fix, ""));
        }

        bool need_update = false;
        foreach (var tab in curr_tabs)
        {
            string tab_name = tab.name;
            if (tab_names_set.Contains(tab_name)) continue;

            string assumed_entry_button_main_part = GetTabMainPart(tab_name);

            foreach (var tab_entry in curr_tab_entries)
            {
                if (GetTabMainPart(tab_entry.name) != assumed_entry_button_main_part) continue;
                need_update = true;
                _addTabEntry(tab_entry.gameObject, tab_name);
                break;
            }
        }

        if (need_update)
        {
            _updateTabLayout();
        }
    }
    private static void _updateTabLayout()
    {
        int cur_active_count = 0;
        foreach (var tab in tab_entries)
        {
            _updateTabEntryRectAs(tab, cur_active_count++);
        }
    }

    private static void _updateTabEntryRectAs(Button tab, int index)
    {
        int row_nr = Math.Min(tab_count_each_line, tab_entries.Count);
        int pos_x = (index % row_nr) - row_nr / 2;
        int pos_y = index / row_nr;


        RectTransform tab_rect = tab.gameObject.GetComponent<RectTransform>();

        float new_y = default_tab_y + (Mathf.Pow(shrink_coef, pos_y) * default_tab_height - default_tab_height) / 2 +
                      (1 - Mathf.Pow(shrink_coef, pos_y)) / (1 - shrink_coef) * default_tab_height;

        tab_rect.sizeDelta = new Vector2(Mathf.Pow(shrink_coef, pos_y) * default_tab_width,
            Mathf.Pow(shrink_coef, pos_y) * default_tab_height);

        tab_rect.localPosition = new Vector3(pos_x * default_tab_width, new_y, tab_rect.localPosition.z);

        try
        {
            RectTransform icon_rect = tab.transform.Find("Icon").gameObject.GetComponent<RectTransform>();

            icon_rect.sizeDelta = new Vector2(Mathf.Pow(shrink_coef, pos_y) * default_icon_width,
                Mathf.Pow(shrink_coef, pos_y) * default_icon_height);
        }
        catch (Exception)
        {
            // DO NOTHING HERE, ONLY NOT FIND "Icon" IN TAB
        }
    }

    private static void _addTabEntry(GameObject pTabEntry, string pTabId)
    {
        if (tab_entries.Count % 2 == 0)
        {
            tab_entries.Insert(0, pTabEntry.GetComponent<Button>());
            tab_names.Insert(0, pTabId);
        }
        else
        {
            tab_entries.Add(pTabEntry.GetComponent<Button>());
            tab_names.Add(pTabId);
        }

        tab_names_set.Add(pTabId);
    }

    /// <summary>
    /// Create a tab which can act as vanilla tabs
    /// </summary>
    /// <param name="name">The name of the tab, "Tab_" prefix will be added automatically</param>
    /// <param name="pTitleKey">The title key of the tooltip when hover on the tab</param>
    /// <param name="pDescKey">The description key of the tooltip when hover on the tab</param>
    /// <param name="pIcon">The icon sprite of the tab</param>
    /// <param name="pOptionDescKey">The description key of the tooltip (the bottom gray one) when hover one the tab</param>
    /// <returns>The tab created</returns>
    public static PowersTab CreateTab(string name, string pTitleKey, string pDescKey, Sprite pIcon,
        string pOptionDescKey = "hotkey_tip_tab_other")
    {
        GameObject tab_entry = Object.Instantiate(ResourcesFinder.FindResources<GameObject>("Button_Other")[0],
            tab_entry_container);

        Object.DestroyImmediate(tab_entry.GetComponent<GraphicRaycaster>());
        Object.DestroyImmediate(tab_entry.GetComponent<Canvas>());
        tab_entry.name = "Button_" + name;
        tab_entry.transform.Find("Icon").GetComponent<Image>().sprite = pIcon;

        PowersTab tab = Object.Instantiate(
            ResourcesFinder.FindResources<GameObject>(PowerTabNames.Creatures).Select(tgo => tgo.GetComponent<PowersTab>()).First(t => t != null),
            tab_container);

        tab.name = name;

        var asset = new PowerTabAsset
        {
            id = name,
            locale_key = pTitleKey,
            tab_type_main = true,
            get_power_tab =  C<PowerTabGetter>(() => tab)
        };
        AssetManager.power_tab_library.add(asset);
        tab._asset = asset;

        Button tab_entry_button = tab_entry.GetComponent<Button>();
        tab_entry_button.onClick = new Button.ButtonClickedEvent();
        tab_entry_button.onClick.AddListener(C<UnityAction>(() => tab.showTab(tab_entry_button)));
        tab_entry_button.onClick.AddListener(C<UnityAction>(() => tab_entry.GetComponent<ButtonSfx>().playSound()));

        TipButton tab_entry_tip = tab_entry.GetComponent<TipButton>();
        tab_entry_tip.textOnClick = pTitleKey;
        tab_entry_tip.textOnClickDescription = pDescKey;
        tab_entry_tip.text_description_2 = pOptionDescKey;
        // Clear tab content
        for (int i = 6; i < tab.transform.childCount; i++)
        {
            Object.Destroy(tab.transform.GetChild(i).gameObject);
        }

        tab._power_buttons.Clear();
        // Add default powerButtons
        foreach (PowerButton power_button in tab.GetComponentsInChildren<PowerButton>())
        {
            if (!(power_button == null) && !(power_button.rect_transform == null))
            {
                tab._power_buttons.Add(power_button);
            }
        }

        foreach (PowerButton power_button in tab._power_buttons)
        {
            power_button.findNeighbours(tab._power_buttons);
        }

        _addTabEntry(tab_entry, tab.name);
        _updateTabLayout();

        tab.gameObject.SetActive(false);


        tab.gameObject.SetActive(true);
        return tab;
    }
}