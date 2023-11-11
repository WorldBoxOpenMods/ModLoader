using NeoModLoader.api;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui;

public class ModConfigureWindow : AbstractWindow<ModConfigureWindow>
{
    class ModConfigGrid : MonoBehaviour
    {
        private Text title;
        private Transform grid;
        private void OnEnable()
        {
            title = transform.Find("Title").GetComponent<Text>();
            grid = transform.Find("Grid");
        }

        public void Setup(string id, Dictionary<string, ModConfigItem> items)
        {
            
        }
    }
    class ModConfigListItem : MonoBehaviour
    {
        public void Setup(ModConfigItem pItem)
        {
            
        }
    }
    private ModConfig _config;
    private static ModConfigGrid _gridPrefab;
    private static ModConfigListItem _itemPrefab;
    protected override void Init()
    {
        VerticalLayoutGroup layout = ContentTransform.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;
        layout.childAlignment = TextAnchor.UpperCenter;
        
        ContentSizeFitter fitter = ContentTransform.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        _createGridPrefab();
        _createItemPrefab();
    }

    private static void _createItemPrefab()
    {
        GameObject config_item = new GameObject("ConfigItem", typeof(VerticalLayoutGroup));
        
        GameObject switch_area = new GameObject("SwitchArea", typeof(HorizontalLayoutGroup));
        switch_area.transform.SetParent(config_item.transform);
        switch_area.transform.localScale = Vector3.one;
        
        GameObject slider_area = new GameObject("SliderArea", typeof(RectTransform));
        slider_area.transform.SetParent(config_item.transform);
        slider_area.transform.localScale = Vector3.one;
        
        GameObject text_area = new GameObject("TextArea", typeof(RectTransform));
        text_area.transform.SetParent(config_item.transform);
        text_area.transform.localScale = Vector3.one;
        
        GameObject select_area = new GameObject("SelectArea", typeof(RectTransform));
        select_area.transform.SetParent(config_item.transform);
        select_area.transform.localScale = Vector3.one;
        
        
        config_item.transform.SetParent(WorldBoxMod.Transform);
        _itemPrefab = config_item.AddComponent<ModConfigListItem>();
    }

    private static void _createGridPrefab()
    {
        GameObject config_grid = new GameObject("ConfigGrid", typeof(VerticalLayoutGroup));
        
        VerticalLayoutGroup layout = config_grid.GetComponent<VerticalLayoutGroup>();
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;
        layout.childAlignment = TextAnchor.UpperCenter;
        
        ContentSizeFitter fitter = config_grid.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        GameObject grid_title = new GameObject("Title", typeof(Text));
        grid_title.transform.SetParent(config_grid.transform);
        grid_title.transform.localScale = Vector3.one;
        Text title = grid_title.GetComponent<Text>();
        title.text = "Mod Config";
        title.font = LocalizedTextManager.currentFont;
        title.fontSize = 10;
        title.alignment = TextAnchor.MiddleCenter;
        
        
        GameObject grid = new GameObject("Grid", typeof(VerticalLayoutGroup));
        grid.transform.SetParent(config_grid.transform);
        grid.transform.localScale = Vector3.one;
        layout = grid.GetComponent<VerticalLayoutGroup>();
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;
        layout.childAlignment = TextAnchor.UpperCenter;
        
        fitter = grid.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        config_grid.transform.SetParent(WorldBoxMod.Transform);
        _gridPrefab = config_grid.AddComponent<ModConfigGrid>();
    }

    public static void ShowWindow(ModConfig pConfig)
    {
        Instance._config = pConfig;
        ScrollWindow.showWindow(WindowId);
    }
}