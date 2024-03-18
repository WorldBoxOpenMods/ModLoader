using NeoModLoader.General.UI.Prefabs;
using NeoModLoader.General.UI.Window.Layout;
using NeoModLoader.utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NeoModLoader.General.UI.Window;

/// <summary>
///     A window with multiple tabs.
/// </summary>
public abstract class MultiTabWindow<T> : AutoLayoutWindow<T> where T : MultiTabWindow<T>
{
    private readonly Dictionary<SimpleButton, AutoVertLayoutGroup> m_tabs = new();
    private          RectTransform                                 m_tab_entries_left;
    private          RectTransform                                 m_tab_entries_right;
    protected        string                                        CurrentTab { get; private set; } = "Default";

    public static T CreateWindow(string pWindowID, string pWindowTitleKey)
    {
        var window = WindowCreator.CreateEmptyWindow(pWindowID, pWindowTitleKey);

        window.gameObject.SetActive(false);

        window.transform_content.gameObject.AddComponent<VerticalLayoutGroup>();
        var auto_layout_window = window.transform_content.gameObject.AddComponent<T>();

        auto_layout_window.BackgroundTransform = window.transform.Find("Background");
        window.transform_scrollRect.gameObject.SetActive(true);
        window.transform_scrollRect.sizeDelta = new Vector2(210, window.transform_scrollRect.sizeDelta.y);

        auto_layout_window.ContentTransform = window.transform_content;
        auto_layout_window.ScrollWindowComponent = window;

        var layout_group = auto_layout_window.GetLayoutGroup();

        layout_group.childAlignment = TextAnchor.UpperCenter;
        layout_group.childControlHeight = false;
        layout_group.childControlWidth = false;
        layout_group.childForceExpandHeight = false;
        layout_group.childForceExpandWidth = false;
        layout_group.childScaleHeight = false;
        layout_group.childScaleWidth = false;
        layout_group.spacing = 10;
        layout_group.padding = new RectOffset(3, 3, 10, 10);

        var fitter = window.transform_content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        GameObject tab_entries_container =
            new("TabEntriesContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        tab_entries_container.transform.SetParent(auto_layout_window.BackgroundTransform);
        tab_entries_container.transform.SetAsFirstSibling();
        tab_entries_container.transform.localPosition = Vector3.zero;
        tab_entries_container.transform.localScale = Vector3.one;
        tab_entries_container.GetComponent<RectTransform>().sizeDelta = new Vector2(256, 220);
        var tab_entries_container_layout = tab_entries_container.GetComponent<HorizontalLayoutGroup>();
        tab_entries_container_layout.childAlignment = TextAnchor.MiddleCenter;
        tab_entries_container_layout.childControlHeight = false;
        tab_entries_container_layout.childControlWidth = false;
        tab_entries_container_layout.childForceExpandHeight = false;
        tab_entries_container_layout.childForceExpandWidth = false;
        tab_entries_container_layout.childScaleHeight = false;
        tab_entries_container_layout.childScaleWidth = false;
        tab_entries_container_layout.spacing = 208;

        GameObject left_container = new("LeftContainer", typeof(RectTransform), typeof(VerticalLayoutGroup),
                                        typeof(Mask), typeof(Image));
        left_container.transform.SetParent(tab_entries_container.transform);
        left_container.transform.localScale = Vector3.one;
        left_container.GetComponent<Mask>().showMaskGraphic = false;
        var left_container_layout = left_container.GetComponent<VerticalLayoutGroup>();
        left_container_layout.childAlignment = TextAnchor.UpperCenter;
        left_container_layout.childControlHeight = false;
        left_container_layout.childControlWidth = false;
        left_container_layout.childForceExpandHeight = false;
        left_container_layout.childForceExpandWidth = false;
        left_container_layout.childScaleHeight = false;
        left_container_layout.childScaleWidth = false;
        left_container_layout.spacing = 4;
        left_container_layout.padding = new RectOffset(4, 0, 0, 0);
        left_container.GetComponent<RectTransform>().sizeDelta = new Vector2(24, 220);
        auto_layout_window.m_tab_entries_left = left_container.GetComponent<RectTransform>();

        var right_container = Instantiate(left_container, tab_entries_container.transform);
        right_container.name = "RightContainer";
        right_container.transform.localScale = Vector3.one;
        right_container.GetComponent<VerticalLayoutGroup>().padding = new RectOffset(0, 4, 0, 0);
        auto_layout_window.m_tab_entries_right = right_container.GetComponent<RectTransform>();


        auto_layout_window.WindowID = pWindowID;
        auto_layout_window.Init();

        auto_layout_window.Initialized = true;

        return auto_layout_window;
    }

    protected AutoVertLayoutGroup CreateTab(string              pTabID, Sprite pTabIcon,
                                            UnityAction<string> pAdditionTabSwitchAction = null)
    {
        var tab = Instantiate(Prefab, ContentTransform.parent);
        tab.Setup(default, TextAnchor.UpperCenter, 10, new RectOffset(3, 3, 10, 10));
        tab.transform.localScale = Vector3.one;
        tab.transform.localPosition = Vector3.zero;
        tab.GetComponent<RectTransform>().pivot = new Vector2(0, 1);

        tab.gameObject.SetActive(false);

        tab.name = pTabID;

        var tab_entry = Instantiate(SimpleButton.Prefab,
                                    m_tab_entries_left.childCount > m_tab_entries_right.childCount
                                        ? m_tab_entries_right
                                        : m_tab_entries_left);
        tab_entry.Setup(() =>
        {
            foreach (Transform tab in ContentTransform.parent) tab.gameObject.SetActive(false);
            if (tab_entry.Background.color == Color.gray)
            {
                tab_entry.Background.color = Color.white;
                CurrentTab = "Default";
                tab.gameObject.SetActive(false);
                ContentTransform.gameObject.SetActive(true);
            }
            else
            {
                tab_entry.Background.color = Color.gray;
                CurrentTab = pTabID;
                tab.gameObject.SetActive(true);
                pAdditionTabSwitchAction?.Invoke(pTabID);
            }

            foreach (var tab_entry_pair in m_tabs.Where(tab_entry_pair => tab_entry_pair.Key != tab_entry))
            {
                tab_entry_pair.Key.Background.color = Color.white;
                tab_entry_pair.Value.gameObject.SetActive(false);
            }
        }, pTabIcon, pSize: new Vector2(24, 48), pTipType: "normal", pTipData: new TooltipData
        {
            tip_name = pTabID,
            tip_description = pTabID + " Description"
        });
        tab_entry.Background.sprite = InternalResourcesGetter.GetWindowVertNamePlate();

        m_tabs.Add(tab_entry, tab);

        ResizeTabEntries();
        return tab;
    }

    private void ResizeTabEntries()
    {
        var tab_count_one_side = 0;
        VerticalLayoutGroup layout_one_side = null;
        RectTransform tab_entries_one_side = null;

        tab_count_one_side = m_tab_entries_left.childCount;
        layout_one_side = m_tab_entries_left.GetComponent<VerticalLayoutGroup>();
        tab_entries_one_side = m_tab_entries_left;

        if (tab_count_one_side <= 4)
            layout_one_side.spacing = 4;
        else
            layout_one_side.spacing =
                (tab_entries_one_side.sizeDelta.y - tab_count_one_side * 48) / (tab_count_one_side - 1);

        tab_count_one_side = m_tab_entries_right.childCount;
        layout_one_side = m_tab_entries_right.GetComponent<VerticalLayoutGroup>();
        tab_entries_one_side = m_tab_entries_right;

        if (tab_count_one_side <= 4)
            layout_one_side.spacing = 4;
        else
            layout_one_side.spacing =
                (tab_entries_one_side.sizeDelta.y - tab_count_one_side * 48) / (tab_count_one_side - 1);
    }
}