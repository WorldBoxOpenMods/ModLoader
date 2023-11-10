using System.Reflection;
using NeoModLoader.api;
using NeoModLoader.services;
using Steamworks.Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace NeoModLoader.General.UI.Tab;

public static class TabManager
{
    private static readonly Transform tab_entry_container =
        CanvasMain.instance.canvas_ui.transform.Find("CanvasBottom/BottomElements/BottomElementsMover/TabsButtons");
    private static readonly Transform tab_container = CanvasMain.instance.canvas_ui.transform.Find("CanvasBottom/BottomElements/BottomElementsMover/CanvasScrollView/Scroll View/Viewport/Content/buttons");
    private static readonly List<Button> tab_entries = new (PowerTabController.instance._buttons); // To avoid other mods' modifies
    private static readonly List<string> tab_names = new();
    private static readonly HashSet<string> tab_names_set = new();
    private const int tab_count_each_line = 10;
    private const float check_new_tabs_interval = 1;
    private const float shrink_coef = 0.79f;
    private const float default_tab_width = 43f;
    private const float default_tab_height = 18f;
    private const float default_icon_width = 33f;
    private const float default_icon_height = 11f;
    private const float default_tab_y = 49.62f;
    internal static void _init()
    {
        var next_tab = AssetManager.hotkey_library.get("next_tab");
        var prev_tab = AssetManager.hotkey_library.get("prev_tab");
        next_tab.just_pressed_action = _ =>
        {
            PowersTab.showTabFromButton(PowerTabController.instance._getNext_Overwrite(PowersTab.getActiveTab().name),
                false);
        };
        prev_tab.just_pressed_action = _ =>
        {
            PowersTab.showTabFromButton(PowerTabController.instance._getPrev_Overwrite(PowersTab.getActiveTab().name),
                false);
        };
        var _tab_names = PowerTabNames.GetNames();
        for (int i = 1; i < _tab_names.Count; i++)
        {
            tab_names.Add(_tab_names[i]);
            tab_names_set.Add(_tab_names[i]);
        }
    }

    private static Button _getNext_Overwrite(this PowerTabController instance, string pActiveTab)
    {
        LogService.LogInfo($"Current active {pActiveTab}: {tab_names.IndexOf(pActiveTab)}");
        return tab_entries[(tab_names.IndexOf(pActiveTab) + 1) % tab_entries.Count];
    }

    private static Button _getPrev_Overwrite(this PowerTabController instance, string pActiveTab)
    {
        int index = tab_names.IndexOf(pActiveTab);
        if (index < 0)
        {
            index = 1;
        }

        if (index == 0)
        {
            index = tab_entries.Count;
        }
        LogService.LogInfo($"Current active {pActiveTab}: {tab_names.IndexOf(pActiveTab)}");
        return tab_entries[index-1];
    }

    private static float _check_timer = 0;
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
        
        bool need_update = false;
        foreach (var tab in curr_tabs)
        {
            string tab_name = tab.name;
            LogService.LogInfo($"Check tab: {tab_name}");
            if(tab_names_set.Contains(tab_name)) continue;

            string assumed_entry_button_name = tab_name.Replace("Tab_", "Button_");

            LogService.LogInfo($"\t Assume button name: {assumed_entry_button_name}");
            foreach (var tab_entry in curr_tab_entries)
            {
                LogService.LogInfo($"\t\t Check button name: {tab_entry.name}");
                if (tab_entry.name != assumed_entry_button_name) continue;
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
        int pos_x;
        int pos_y;
        int row_nr = Math.Min(tab_count_each_line, tab_entries.Count);
        int cur_active_count = 0;
        foreach (var tab in tab_entries)
        {
            pos_x = (cur_active_count%row_nr)-row_nr/2;
            pos_y = cur_active_count/row_nr;
				
				
            RectTransform tab_rect = tab.gameObject.GetComponent<RectTransform>();
				
            float new_y = default_tab_y + (Mathf.Pow(shrink_coef, pos_y) * default_tab_height - default_tab_height) / 2 + (1 - Mathf.Pow(shrink_coef, pos_y)) / (1 - shrink_coef) * default_tab_height;
				
            tab_rect.sizeDelta = new Vector2(Mathf.Pow(shrink_coef, pos_y)*default_tab_width, Mathf.Pow(shrink_coef, pos_y)*default_tab_height);
				
            tab_rect.localPosition = new Vector3(pos_x*default_tab_width, new_y, tab_rect.localPosition.z);
				
            try{
                RectTransform icon_rect = tab.transform.Find("Icon").gameObject.GetComponent<RectTransform>();
				
                icon_rect.sizeDelta = new Vector2(Mathf.Pow(shrink_coef, pos_y)*default_icon_width, Mathf.Pow(shrink_coef, pos_y)*default_icon_height);
            }
            catch(Exception){
                // DO NOTHING HERE, ONLY NOT FIND "Icon" IN TAB
            }
				
            cur_active_count++;
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
    /// <returns>The tab created</returns>
    public static PowersTab CreateTab(string name, string pTitleKey, string pDescKey, Sprite pIcon)
    {
        GameObject tab_entry = GameObject.Instantiate(ResourcesFinder.FindResources<GameObject>("Button_Other")[0], tab_entry_container);

        tab_entry.name = "Button_" + name;
        tab_entry.transform.Find("Icon").GetComponent<Image>().sprite = pIcon;
        
        PowersTab tab = GameObject.Instantiate(ResourcesFinder.FindResources<GameObject>("Tab_Other")[0].GetComponent<PowersTab>(),
            tab_container);
        
        tab.name = "Tab_" + name;

        Button tab_entry_button = tab_entry.GetComponent<Button>();
        tab_entry_button.onClick = new Button.ButtonClickedEvent();
        tab_entry_button.onClick.AddListener(()=>tab.showTab(tab_entry_button));
        tab_entry_button.onClick.AddListener(()=>tab_entry.GetComponent<ButtonSfx>().playSound());
            
        TipButton tab_entry_tip = tab_entry.GetComponent<TipButton>();
        tab_entry_tip.textOnClick = pTitleKey;
        tab_entry_tip.text_description_2 = pDescKey;
        // Clear tab content
        for(int i = 7; i < tab.transform.childCount; i++)
        {
            GameObject.Destroy(tab.transform.GetChild(i).gameObject);
        }
        tab.powerButtons.Clear();
        // Add default powerButtons
        foreach (PowerButton power_button in tab.GetComponentsInChildren<PowerButton>())
        {
            if (!(power_button == null) && !(power_button.rectTransform == null))
            {
                tab.powerButtons.Add(power_button);
            }
        }
        foreach (PowerButton power_button in tab.powerButtons)
        {
            power_button.findNeighbours(tab.powerButtons);
        }

        _addTabEntry(tab_entry, tab.name);
        _updateTabLayout();
        
        tab.gameObject.SetActive(false);
        
        
        
        tab.gameObject.SetActive(true);
        return tab;
    }
}