using NeoModLoader.api;
using NeoModLoader.General;
using NeoModLoader.General.UI.Prefabs;
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
            name = id;
            title.text = LM.Get(id);
            int item_idx = 0;
            foreach (var item in items)
            {
                ModConfigListItem list_item = ModConfigureWindow._itemPool.getNext(item_idx++);
                Transform item_transform;
                (item_transform = list_item.transform).SetParent(grid);
                item_transform.localScale = Vector3.one;
                list_item.Setup(item.Value);
            }
        }
    }
    class ModConfigListItem : MonoBehaviour
    {
        public GameObject switch_area;
        public GameObject slider_area;
        public GameObject text_area;
        public GameObject select_area;
        public void Setup(ModConfigItem pItem)
        {
            name = pItem.Id;
            switch_area.SetActive(false);
            slider_area.SetActive(false);
            text_area.SetActive(false);
            select_area.SetActive(false);
            switch (pItem.Type)
            {
                case ConfigItemType.SWITCH:
                    setup_switch(pItem);
                    break;
                case ConfigItemType.SLIDER:
                    setup_slider(pItem);
                    break;
                case ConfigItemType.TEXT:
                    setup_text(pItem);
                    break;
                case ConfigItemType.SELECT:
                    break;
            }
        }

        private void setup_text(ModConfigItem pItem)
        {
            text_area.SetActive(true);
            text_area.transform.Find("Input").GetComponent<TextInput>().Setup(pItem.TextVal, pStringVal => pItem.SetValue(pStringVal));
            text_area.transform.Find("Info/Text").GetComponent<Text>().text = LM.Get(pItem.Id);
            if (string.IsNullOrEmpty(pItem.IconPath))
            {
                text_area.transform.Find("Info/Icon").gameObject.SetActive(false);
            }
            else
            {
                Image icon = text_area.transform.Find("Info/Icon").GetComponent<Image>();
                icon.gameObject.SetActive(true);
                icon.sprite = SpriteTextureLoader.getSprite(pItem.IconPath);
            }
        }

        private void setup_slider(ModConfigItem pItem)
        {
            slider_area.SetActive(true);
            Text value = slider_area.transform.Find("Info/Value").GetComponent<Text>();
            value.text = $"{pItem.FloatVal:F2}";
            slider_area.transform.Find("Slider").GetComponent<SliderBar>().Setup(pItem.FloatVal, 0, 1, pFloatVal =>
            {
                pItem.SetValue(pFloatVal);
                value.text = $"{pItem.FloatVal:F2}";
            });
            slider_area.transform.Find("Info/Text").GetComponent<Text>().text = LM.Get(pItem.Id);
            if (string.IsNullOrEmpty(pItem.IconPath))
            {
                slider_area.transform.Find("Info/Icon").gameObject.SetActive(false);
            }
            else
            {
                Image icon = slider_area.transform.Find("Info/Icon").GetComponent<Image>();
                icon.gameObject.SetActive(true);
                icon.sprite = SpriteTextureLoader.getSprite(pItem.IconPath);
            }
        }

        private void setup_switch(ModConfigItem pItem)
        {
            switch_area.SetActive(true);
            switch_area.transform.Find("Button").GetComponent<SwitchButton>().Setup(pItem.BoolVal, ()=>pItem.SetValue(!pItem.BoolVal));
            switch_area.transform.Find("Text").GetComponent<Text>().text = LM.Get(pItem.Id);
            if (string.IsNullOrEmpty(pItem.IconPath))
            {
                switch_area.transform.Find("Icon").gameObject.SetActive(false);
            }
            else
            {
                Image icon = switch_area.transform.Find("Icon").GetComponent<Image>();
                icon.gameObject.SetActive(true);
                icon.sprite = SpriteTextureLoader.getSprite(pItem.IconPath);
            }
        }
    }
    private ModConfig _config;
    private static ModConfigGrid _gridPrefab;
    private static ModConfigListItem _itemPrefab;
    private static ObjectPoolGenericMono<ModConfigGrid> _gridPool;
    private static ObjectPoolGenericMono<ModConfigListItem> _itemPool;
    protected override void Init()
    {
        VerticalLayoutGroup layout = ContentTransform.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.padding = new RectOffset(32, 32, 0, 0);
        
        ContentSizeFitter fitter = ContentTransform.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        _createGridPrefab();
        _createItemPrefab();

        _gridPool = new ObjectPoolGenericMono<ModConfigGrid>(_gridPrefab, ContentTransform);
        _itemPool = new ObjectPoolGenericMono<ModConfigListItem>(_itemPrefab, null);
    }

    private static void _createItemPrefab()
    {
        GameObject config_item = new GameObject("ConfigItem", typeof(Image), typeof(VerticalLayoutGroup));
        VerticalLayoutGroup layout = config_item.GetComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.padding = new(4, 4, 3, 3);
        #region SWITCH
        GameObject switch_area = new GameObject("SwitchArea", typeof(HorizontalLayoutGroup));
        HorizontalLayoutGroup switch_layout = switch_area.GetComponent<HorizontalLayoutGroup>();
        switch_layout.childControlWidth = false;
        switch_layout.childControlHeight = false;
        switch_layout.childAlignment = TextAnchor.MiddleLeft;
        switch_area.transform.SetParent(config_item.transform);
        switch_area.transform.localScale = Vector3.one;
        SwitchButton switch_button = Instantiate(SwitchButton.Prefab, switch_area.transform);
        switch_button.transform.localScale = Vector3.one;
        switch_button.name = "Button";
        GameObject switch_config_icon = new GameObject("Icon", typeof(Image));
        switch_config_icon.transform.SetParent(switch_area.transform);
        switch_config_icon.transform.localScale = Vector3.one;
        switch_config_icon.GetComponent<RectTransform>().sizeDelta = new(16, 16);
        GameObject switch_config_text = new GameObject("Text", typeof(Text));
        switch_config_text.transform.SetParent(switch_area.transform);
        switch_config_text.transform.localScale = Vector3.one;
        switch_config_text.GetComponent<RectTransform>().sizeDelta = new(100, 16);
        Text switch_text = switch_config_text.GetComponent<Text>();
        OT.InitializeCommonText(switch_text);
        switch_text.alignment = TextAnchor.MiddleLeft;
        switch_text.resizeTextForBestFit = true;
        #endregion
        #region SLIDER
        GameObject slider_area = new GameObject("SliderArea", typeof(RectTransform), typeof(VerticalLayoutGroup));
        slider_area.transform.SetParent(config_item.transform);
        slider_area.transform.localScale = Vector3.one;
        VerticalLayoutGroup slider_layout = slider_area.GetComponent<VerticalLayoutGroup>();
        slider_layout.childControlWidth = true;
        slider_layout.childControlHeight = false;
        slider_layout.childForceExpandWidth = false;
        slider_layout.childAlignment = TextAnchor.UpperCenter;
        slider_layout.spacing = 4;
        
        GameObject slider_info = new GameObject("Info", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        slider_info.transform.SetParent(slider_area.transform);
        slider_info.transform.localScale = Vector3.one;
        slider_info.GetComponent<RectTransform>().sizeDelta = new(0, 18);
        HorizontalLayoutGroup slider_info_layout = slider_info.GetComponent<HorizontalLayoutGroup>();
        slider_info_layout.childControlWidth = false;
        slider_info_layout.childControlHeight = false;
        slider_info_layout.childAlignment = TextAnchor.MiddleLeft;
        GameObject slider_config_icon = new GameObject("Icon", typeof(Image));
        slider_config_icon.transform.SetParent(slider_info.transform);
        slider_config_icon.transform.localScale = Vector3.one;
        slider_config_icon.GetComponent<RectTransform>().sizeDelta = new(16, 16);
        GameObject slider_config_text = new GameObject("Text", typeof(Text));
        slider_config_text.transform.SetParent(slider_info.transform);
        slider_config_text.transform.localScale = Vector3.one;
        slider_config_text.GetComponent<RectTransform>().sizeDelta = new(100, 16);
        Text slider_text = slider_config_text.GetComponent<Text>();
        OT.InitializeCommonText(slider_text);
        slider_text.alignment = TextAnchor.MiddleLeft;
        slider_text.resizeTextForBestFit = true;
        GameObject slider_config_value = new GameObject("Value", typeof(Text));
        slider_config_value.transform.SetParent(slider_info.transform);
        slider_config_value.transform.localScale = Vector3.one;
        slider_config_value.GetComponent<RectTransform>().sizeDelta = new(32, 16);
        Text slider_value = slider_config_value.GetComponent<Text>();
        OT.InitializeCommonText(slider_value);
        slider_value.alignment = TextAnchor.MiddleRight;
        slider_value.resizeTextForBestFit = true;
        SliderBar slider_bar = Instantiate(SliderBar.Prefab, slider_area.transform);
        slider_bar.transform.localScale = Vector3.one;
        slider_bar.name = "Slider";
        slider_bar.SetSize(new Vector2(170f, 20));
        #endregion
        #region TEXT
        GameObject text_area = new GameObject("TextArea", typeof(RectTransform), typeof(VerticalLayoutGroup));
        text_area.transform.SetParent(config_item.transform);
        text_area.transform.localScale = Vector3.one;
        VerticalLayoutGroup text_layout = text_area.GetComponent<VerticalLayoutGroup>();
        text_layout.childControlWidth = true;
        text_layout.childControlHeight = false;
        text_layout.childAlignment = TextAnchor.UpperCenter;
        text_layout.spacing = 4;
        
        GameObject text_info = new GameObject("Info", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        text_info.transform.SetParent(text_area.transform);
        text_info.transform.localScale = Vector3.one;
        text_info.GetComponent<RectTransform>().sizeDelta = new(0, 18);
        HorizontalLayoutGroup text_info_layout = text_info.GetComponent<HorizontalLayoutGroup>();
        text_info_layout.childControlWidth = false;
        text_info_layout.childControlHeight = false;
        text_info_layout.childForceExpandWidth = false;
        text_info_layout.childAlignment = TextAnchor.MiddleLeft;
        text_info_layout.spacing = 8;
        GameObject text_config_icon = new GameObject("Icon", typeof(Image));
        text_config_icon.transform.SetParent(text_info.transform);
        text_config_icon.transform.localScale = Vector3.one;
        text_config_icon.GetComponent<RectTransform>().sizeDelta = new(16, 16);
        GameObject text_config_text = new GameObject("Text", typeof(Text));
        text_config_text.transform.SetParent(text_info.transform);
        text_config_text.transform.localScale = Vector3.one;
        text_config_text.GetComponent<RectTransform>().sizeDelta = new(100, 16);
        Text text_text = text_config_text.GetComponent<Text>();
        OT.InitializeCommonText(text_text);
        text_text.alignment = TextAnchor.MiddleLeft;
        text_text.resizeTextForBestFit = true;
        
        TextInput text_input = Instantiate(TextInput.Prefab, text_area.transform);
        text_input.transform.localScale = Vector3.one;
        text_input.name = "Input";
        text_input.SetSize(new Vector2(170f, 20));
        #endregion
        #region SELECT
        GameObject select_area = new GameObject("SelectArea", typeof(RectTransform));
        select_area.transform.SetParent(config_item.transform);
        select_area.transform.localScale = Vector3.one;
        #endregion
        config_item.GetComponent<Image>().sprite = SpriteTextureLoader.getSprite("ui/special/windowInnerSliced");
        config_item.GetComponent<Image>().type = Image.Type.Sliced;
        config_item.transform.SetParent(WorldBoxMod.Transform);
        _itemPrefab = config_item.AddComponent<ModConfigListItem>();
        _itemPrefab.switch_area = switch_area;
        _itemPrefab.slider_area = slider_area;
        _itemPrefab.text_area = text_area;
        _itemPrefab.select_area = select_area;
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
        /*
        ContentSizeFitter fitter = config_grid.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        */
        GameObject grid_title = new GameObject("Title", typeof(Text));
        grid_title.transform.SetParent(config_grid.transform);
        grid_title.transform.localScale = Vector3.one;
        Text title = grid_title.GetComponent<Text>();
        title.text = "Mod Config";
        title.font = LocalizedTextManager.currentFont;
        title.fontSize = 10;
        title.alignment = TextAnchor.MiddleCenter;
        
        
        GameObject grid = new GameObject("Grid", typeof(Image), typeof(VerticalLayoutGroup));
        grid.transform.SetParent(config_grid.transform);
        grid.transform.localScale = Vector3.one;
        layout = grid.GetComponent<VerticalLayoutGroup>();
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.padding = new RectOffset(4, 4, 5, 5);
        layout.spacing = 4;
        /*
        fitter = grid.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        */
        grid.GetComponent<Image>().sprite = SpriteTextureLoader.getSprite("ui/special/windowInnerSliced");
        grid.GetComponent<Image>().type = Image.Type.Sliced;
        grid.GetComponent<Image>().color = new(1, 1, 1, 0.5608f);
        
        config_grid.transform.SetParent(WorldBoxMod.Transform);
        _gridPrefab = config_grid.AddComponent<ModConfigGrid>();
    }

    public static void ShowWindow(ModConfig pConfig)
    {
        if(pConfig == null) return;
        Instance._config = pConfig;
        ScrollWindow.showWindow(WindowId);
    }

    public override void OnNormalEnable()
    {
        int group_idx = 0;
        foreach (var group in _config._config)
        {
            ModConfigGrid grid = _gridPool.getNext(group_idx++);
            grid.Setup(group.Key, group.Value);
        }
    }

    public override void OnNormalDisable()
    {
        _gridPool.clear();
        _itemPool.clear();
        _config?.Save();
        _config = null;
    }
}