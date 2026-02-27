using NeoModLoader.api;
using NeoModLoader.General;
using NeoModLoader.General.UI.Prefabs;
using System.Globalization;
using NeoModLoader.AndroidCompatibilityModule;
using NeoModLoader.utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using NeoModLoader.utils;
namespace NeoModLoader.ui;

/// <summary>
///     Configuration window for <see cref="ModConfig" />
/// </summary>
public class ModConfigureWindow : AbstractWindow<ModConfigureWindow>
{
    private static ModConfigGrid _gridPrefab;
    private static ModConfigListItem _itemPrefab;
    private static ObjectPoolGenericMono<ModConfigGrid> _gridPool;
    private static ObjectPoolGenericMono<ModConfigListItem> _itemPool;
    private readonly Dictionary<ModConfigItem, object> _modifiedItems = new();
    private ModConfig _config;

    /// <inheritdoc cref="AbstractWindow{T}.Init" />
    protected override void Init()
    {
        BackgroundTransform.Find("Scroll View").gameObject.SetActive(true);
        BackgroundTransform.Find("Scroll View").GetComponent<RectTransform>().sizeDelta =
            new Vector2(232, 270);
        BackgroundTransform.Find("Scroll View").localPosition = new Vector3(0, -6);
        BackgroundTransform.Find("Scroll View/Viewport").GetComponent<RectTransform>().sizeDelta =
            new Vector2(30, 0);
        BackgroundTransform.Find("Scroll View/Viewport").localPosition = new Vector3(-131, 135);

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
        _itemPool = new ObjectPoolGenericMono<ModConfigListItem>(_itemPrefab, BackgroundTransform);
    }

    private static void _createItemPrefab()
    {
        GameObject config_item = new GameObject("ConfigItem", typeof(Image).Convert(), typeof(VerticalLayoutGroup).Convert());
        VerticalLayoutGroup layout = config_item.GetComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.padding = new(4, 4, 3, 3);

        #region SWITCH

        GameObject switch_area = new GameObject("SwitchArea", typeof(HorizontalLayoutGroup).Convert());
        HorizontalLayoutGroup switch_layout = switch_area.GetComponent<HorizontalLayoutGroup>();
        switch_layout.childControlWidth = false;
        switch_layout.childControlHeight = false;
        switch_layout.childAlignment = TextAnchor.MiddleLeft;
        switch_area.transform.SetParent(config_item.transform);
        switch_area.transform.localScale = Vector3.one;
        var switch_button = Instantiate(General.UI.Prefabs.SwitchButton.Prefab, switch_area.transform);
        switch_button.transform.localScale = Vector3.one;
        switch_button.name = "Button";
        GameObject switch_config_icon = new GameObject("Icon", typeof(Image).Convert());
        switch_config_icon.transform.SetParent(switch_area.transform);
        switch_config_icon.transform.localScale = Vector3.one;
        switch_config_icon.GetComponent<RectTransform>().sizeDelta = new(16, 16);
        GameObject switch_config_text = new GameObject("Text", typeof(Text).Convert());
        switch_config_text.transform.SetParent(switch_area.transform);
        switch_config_text.transform.localScale = Vector3.one;
        switch_config_text.GetComponent<RectTransform>().sizeDelta = new(100, 16);
        Text switch_text = switch_config_text.GetComponent<Text>();
        OT.InitializeCommonText(switch_text);
        switch_text.alignment = TextAnchor.MiddleLeft;
        switch_text.resizeTextForBestFit = true;
        switch_text.resizeTextMinSize = 1;

        #endregion

        #region SLIDER

        GameObject slider_area = new GameObject("SliderArea", typeof(RectTransform).Convert(), typeof(VerticalLayoutGroup).Convert());
        slider_area.transform.SetParent(config_item.transform);
        slider_area.transform.localScale = Vector3.one;
        VerticalLayoutGroup slider_layout = slider_area.GetComponent<VerticalLayoutGroup>();
        slider_layout.childControlWidth = true;
        slider_layout.childControlHeight = false;
        slider_layout.childForceExpandWidth = false;
        slider_layout.childAlignment = TextAnchor.UpperCenter;
        slider_layout.spacing = 4;

        GameObject slider_info = new GameObject("Info", typeof(RectTransform).Convert(), typeof(HorizontalLayoutGroup).Convert());
        slider_info.transform.SetParent(slider_area.transform);
        slider_info.transform.localScale = Vector3.one;
        slider_info.GetComponent<RectTransform>().sizeDelta = new(0, 18);
        HorizontalLayoutGroup slider_info_layout = slider_info.GetComponent<HorizontalLayoutGroup>();
        slider_info_layout.childControlWidth = false;
        slider_info_layout.childControlHeight = false;
        slider_info_layout.childAlignment = TextAnchor.MiddleLeft;
        GameObject slider_config_icon = new GameObject("Icon", typeof(Image).Convert());
        slider_config_icon.transform.SetParent(slider_info.transform);
        slider_config_icon.transform.localScale = Vector3.one;
        slider_config_icon.GetComponent<RectTransform>().sizeDelta = new(16, 16);
        GameObject slider_config_text = new GameObject("Text", typeof(Text).Convert());
        slider_config_text.transform.SetParent(slider_info.transform);
        slider_config_text.transform.localScale = Vector3.one;
        slider_config_text.GetComponent<RectTransform>().sizeDelta = new(100, 16);
        Text slider_text = slider_config_text.GetComponent<Text>();
        OT.InitializeCommonText(slider_text);
        slider_text.alignment = TextAnchor.MiddleLeft;
        slider_text.resizeTextForBestFit = true;

        GameObject slider_control = new GameObject("Control", typeof(RectTransform).Convert(), typeof(HorizontalLayoutGroup).Convert());
        slider_control.transform.SetParent(slider_area.transform);
        slider_control.transform.localScale = Vector3.one;
        slider_control.GetComponent<RectTransform>().sizeDelta = new(0, 20);
        HorizontalLayoutGroup slider_control_layout = slider_control.GetComponent<HorizontalLayoutGroup>();
        slider_control_layout.childControlWidth = false;
        slider_control_layout.childControlHeight = false;
        slider_control_layout.childForceExpandWidth = false;
        slider_control_layout.childAlignment = TextAnchor.MiddleLeft;
        slider_control_layout.spacing = 4;

        TextInput slider_value_input = Instantiate(TextInput.Prefab, slider_control.transform);
        slider_value_input.transform.localScale = Vector3.one;
        slider_value_input.name = "Input";
        slider_value_input.SetSize(new Vector2(52f, 20f));

        SliderBar slider_bar = Instantiate(SliderBar.Prefab, slider_control.transform);
        slider_bar.transform.localScale = Vector3.one;
        slider_bar.name = "Slider";
        slider_bar.SetSize(new Vector2(114f, 20f));

        #endregion

        #region TEXT

        GameObject text_area = new GameObject("TextArea", typeof(RectTransform).Convert(), typeof(VerticalLayoutGroup).Convert());
        text_area.transform.SetParent(config_item.transform);
        text_area.transform.localScale = Vector3.one;
        VerticalLayoutGroup text_layout = text_area.GetComponent<VerticalLayoutGroup>();
        text_layout.childControlWidth = true;
        text_layout.childControlHeight = false;
        text_layout.childAlignment = TextAnchor.UpperCenter;
        text_layout.spacing = 4;

        GameObject text_info = new GameObject("Info", typeof(RectTransform).Convert(), typeof(HorizontalLayoutGroup).Convert());
        text_info.transform.SetParent(text_area.transform);
        text_info.transform.localScale = Vector3.one;
        text_info.GetComponent<RectTransform>().sizeDelta = new(0, 18);
        HorizontalLayoutGroup text_info_layout = text_info.GetComponent<HorizontalLayoutGroup>();
        text_info_layout.childControlWidth = false;
        text_info_layout.childControlHeight = false;
        text_info_layout.childForceExpandWidth = false;
        text_info_layout.childAlignment = TextAnchor.MiddleLeft;
        text_info_layout.spacing = 8;
        GameObject text_config_icon = new GameObject("Icon", typeof(Image).Convert());
        text_config_icon.transform.SetParent(text_info.transform);
        text_config_icon.transform.localScale = Vector3.one;
        text_config_icon.GetComponent<RectTransform>().sizeDelta = new(16, 16);
        GameObject text_config_text = new GameObject("Text", typeof(Text).Convert());
        text_config_text.transform.SetParent(text_info.transform);
        text_config_text.transform.localScale = Vector3.one;
        text_config_text.GetComponent<RectTransform>().sizeDelta = new(100, 16);
        Text text_text = text_config_text.GetComponent<Text>();
        OT.InitializeCommonText(text_text);
        text_text.alignment = TextAnchor.MiddleLeft;
        text_text.resizeTextForBestFit = true;
        text_text.resizeTextMinSize = 1;

        TextInput text_input = Instantiate(TextInput.Prefab, text_area.transform);
        text_input.transform.localScale = Vector3.one;
        text_input.name = "Input";
        text_input.SetSize(new Vector2(170f, 20));

        #endregion

        #region SELECT

        GameObject select_area = new GameObject("SelectArea", typeof(RectTransform).Convert(), typeof(VerticalLayoutGroup).Convert());
        select_area.transform.SetParent(config_item.transform);
        select_area.transform.localScale = Vector3.one;
        VerticalLayoutGroup select_layout = select_area.GetComponent<VerticalLayoutGroup>();
        select_layout.childControlWidth = true;
        select_layout.childControlHeight = false;
        select_layout.childForceExpandWidth = false;
        select_layout.childAlignment = TextAnchor.UpperCenter;
        select_layout.spacing = 4;

        GameObject select_info = new GameObject("Info", typeof(RectTransform).Convert(), typeof(HorizontalLayoutGroup).Convert());
        select_info.transform.SetParent(select_area.transform);
        select_info.transform.localScale = Vector3.one;
        select_info.GetComponent<RectTransform>().sizeDelta = new(0, 18);
        HorizontalLayoutGroup select_info_layout = select_info.GetComponent<HorizontalLayoutGroup>();
        select_info_layout.childControlWidth = false;
        select_info_layout.childControlHeight = false;
        select_info_layout.childForceExpandWidth = false;
        select_info_layout.childAlignment = TextAnchor.MiddleLeft;
        select_info_layout.spacing = 8;

        GameObject select_config_icon = new GameObject("Icon", typeof(Image).Convert());
        select_config_icon.transform.SetParent(select_info.transform);
        select_config_icon.transform.localScale = Vector3.one;
        select_config_icon.GetComponent<RectTransform>().sizeDelta = new(16, 16);

        GameObject select_config_text = new GameObject("Text", typeof(Text).Convert());
        select_config_text.transform.SetParent(select_info.transform);
        select_config_text.transform.localScale = Vector3.one;
        select_config_text.GetComponent<RectTransform>().sizeDelta = new(140, 16);
        Text select_text = select_config_text.GetComponent<Text>();
        OT.InitializeCommonText(select_text);
        select_text.alignment = TextAnchor.MiddleLeft;
        select_text.resizeTextForBestFit = true;
        select_text.resizeTextMinSize = 1;

        GameObject options_grid = new GameObject("Options", typeof(RectTransform).Convert(), typeof(GridLayoutGroup).Convert(),
            typeof(ContentSizeFitter).Convert());
        options_grid.transform.SetParent(select_area.transform);
        options_grid.transform.localScale = Vector3.one;
        options_grid.GetComponent<RectTransform>().sizeDelta = new(170, 0);
        GridLayoutGroup options_grid_layout = options_grid.GetComponent<GridLayoutGroup>();
        options_grid_layout.cellSize = new Vector2(54, 20);
        options_grid_layout.spacing = new Vector2(4, 4);
        options_grid_layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        options_grid_layout.constraintCount = 3;
        options_grid_layout.startAxis = GridLayoutGroup.Axis.Horizontal;
        options_grid_layout.childAlignment = TextAnchor.UpperLeft;
        ContentSizeFitter options_fitter = options_grid.GetComponent<ContentSizeFitter>();
        options_fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        options_fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

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
        GameObject config_grid = new GameObject("ConfigGrid", typeof(VerticalLayoutGroup).Convert());

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
        GameObject grid_title = new GameObject("Title", typeof(Text).Convert());
        grid_title.transform.SetParent(config_grid.transform);
        grid_title.transform.localScale = Vector3.one;
        Text title = grid_title.GetComponent<Text>();
        title.text = "Mod Config";
        title.font = LocalizedTextManager.current_font;
        title.resizeTextForBestFit = true;
        title.resizeTextMinSize = 1;
        title.resizeTextMaxSize = 10;
        title.alignment = TextAnchor.MiddleCenter;


        GameObject grid = new GameObject("Grid", typeof(Image).Convert(), typeof(VerticalLayoutGroup).Convert());
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

    /// <summary>
    ///     Display window for given mod config.
    /// </summary>
    public static void ShowWindow(ModConfig pConfig)
    {
        if (pConfig == null) return;
        Instance._config = pConfig;
        ScrollWindow.showWindow(WindowId);
    }

    /// <inheritdoc cref="AbstractWindow{T}.OnNormalEnable" />
    public override void OnNormalEnable()
    {
        _modifiedItems.Clear();
        foreach (var group in _config._config)
        {
            ModConfigGrid grid = _gridPool.getNext();
            grid.Setup(group.Key, group.Value);
        }
    }

    /// <summary>
    ///     Apply and save changes.
    /// </summary>
    /// <inheritdoc cref="AbstractWindow{T}.OnNormalDisable" />
    public override void OnNormalDisable()
    {
        _gridPool.clear();
        _itemPool.clear();

        foreach (var modified_item in _modifiedItems)
        {
            object current_value = modified_item.Key.GetValue();
            if (current_value != modified_item.Value)
            {
                modified_item.Key.SetValue(current_value);
            }
        }

        _config?.Save();
        _config = null;
    }

    class ModConfigGrid : WrappedBehaviour
    {
        private Transform grid;
        private Text title;

        private void OnEnable()
        {
            title = transform.Find("Title").GetComponent<Text>();
            grid = transform.Find("Grid");
        }

        public void Setup(string id, Dictionary<string, ModConfigItem> items)
        {
            name = id;
            title.text = LM.Get(id);
            foreach (var item in items)
            {
                ModConfigListItem list_item = _itemPool.getNext();
                Transform item_transform;
                (item_transform = list_item.transform).SetParent(grid);
                item_transform.localScale = Vector3.one;
                list_item.Setup(item.Value);
            }
        }
    }

    class ModConfigListItem : WrappedBehaviour
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
                case ConfigItemType.INT_SLIDER:
                    setup_int_slider(pItem);
                    break;
                case ConfigItemType.TEXT:
                    setup_text(pItem);
                    break;
                case ConfigItemType.SELECT:
                    setup_select(pItem);
                    break;
            }
        }

        private void setup_text(ModConfigItem pItem)
        {
            text_area.SetActive(true);

            TextInput text_input = text_area.transform.Find("Input").GetComponent<TextInput>();

            text_input.Setup(pItem.TextVal, IL2CPPHelper.Convert<UnityAction<string>>((string pStringVal) =>
            {
                if (!Instance._modifiedItems.ContainsKey(pItem))
                {
                    Instance._modifiedItems.Add(pItem, pItem.GetValue());
                }

                pItem.SetValue(pStringVal, true);
            }));
            text_input.tip_button.textOnClick = pItem.Id;
            text_input.tip_button.text_description_2 = pItem.Id + " Description";
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
            TextInput value_input = slider_area.transform.Find("Control/Input").GetComponent<TextInput>();
            SliderBar slider_bar = slider_area.transform.Find("Control/Slider").GetComponent<SliderBar>();
            slider_bar.Setup(pItem.FloatVal, pItem.MinFloatVal, pItem.MaxFloatVal, IL2CPPHelper.Convert<UnityAction<float>>((float pFloatVal) =>
            {
                mark_modified(pItem);
                pItem.SetValue(pFloatVal, true);
                value_input.input.text = pItem.FloatVal.ToString("F2", CultureInfo.InvariantCulture);
            }));
            slider_bar.tip_button.textOnClick = pItem.Id;
            slider_bar.tip_button.text_description_2 = pItem.Id + " Description";

            value_input.Setup(pItem.FloatVal.ToString("F2", CultureInfo.InvariantCulture), IL2CPPHelper.Convert<UnityAction<string>>((string pTextVal) =>
            {
                if (!TryParseFloat(pTextVal, out float parsed))
                {
                    value_input.input.text = pItem.FloatVal.ToString("F2", CultureInfo.InvariantCulture);
                    return;
                }

                mark_modified(pItem);
                pItem.SetValue(parsed, true);
                slider_bar.slider.value = pItem.FloatVal;
                value_input.input.text = pItem.FloatVal.ToString("F2", CultureInfo.InvariantCulture);
            }));

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

        private void setup_int_slider(ModConfigItem pItem)
        {
            slider_area.SetActive(true);
            TextInput value_input = slider_area.transform.Find("Control/Input").GetComponent<TextInput>();
            SliderBar slider_bar = slider_area.transform.Find("Control/Slider").GetComponent<SliderBar>();
            slider_bar.Setup(pItem.IntVal, pItem.MinIntVal, pItem.MaxIntVal, IL2CPPHelper.Convert<UnityAction<float>>((float pIntVal) =>
            {
                mark_modified(pItem);
                pItem.SetValue(pIntVal, true);
                value_input.input.text = pItem.IntVal.ToString(CultureInfo.InvariantCulture);
            }), whole_numbers: true);
            slider_bar.tip_button.textOnClick = pItem.Id;
            slider_bar.tip_button.text_description_2 = pItem.Id + " Description";

            value_input.Setup(pItem.IntVal.ToString(CultureInfo.InvariantCulture), IL2CPPHelper.Convert<UnityAction<string>>((string pTextVal) =>
            {
                if (!TryParseInt(pTextVal, out int parsed))
                {
                    value_input.input.text = pItem.IntVal.ToString(CultureInfo.InvariantCulture);
                    return;
                }

                mark_modified(pItem);
                pItem.SetValue(parsed, true);
                slider_bar.slider.value = pItem.IntVal;
                value_input.input.text = pItem.IntVal.ToString(CultureInfo.InvariantCulture);
            }));

            slider_area.transform.Find("Info/Text").GetComponent<Text>().text = LM.Get(pItem.Id);
            if (string.IsNullOrEmpty(pItem.IconPath))
            {
                slider_area.transform.Find("Info/Icon").gameObject.SetActive(false);
            }
            else
            {
                var icon = slider_area.transform.Find("Info/Icon").GetComponent<Image>();
                icon.gameObject.SetActive(true);
                icon.sprite = SpriteTextureLoader.getSprite(pItem.IconPath);
            }
        }

        private void setup_select(ModConfigItem pItem)
        {
            select_area.SetActive(true);
            select_area.transform.Find("Info/Text").GetComponent<Text>().text = LM.Get(pItem.Id);
            if (string.IsNullOrEmpty(pItem.IconPath))
            {
                select_area.transform.Find("Info/Icon").gameObject.SetActive(false);
            }
            else
            {
                Image icon = select_area.transform.Find("Info/Icon").GetComponent<Image>();
                icon.gameObject.SetActive(true);
                icon.sprite = SpriteTextureLoader.getSprite(pItem.IconPath);
            }

            Transform options_root = select_area.transform.Find("Options");
            clear_children(options_root);
            int option_count = Math.Max(0, pItem.MaxIntVal);
            int selected = ModConfigSelectOptionCodec.ClampIndex(pItem.IntVal, option_count);
            if (selected != pItem.IntVal)
            {
                mark_modified(pItem);
                pItem.SetValue(selected, true);
            }

            GridLayoutGroup grid = options_root.GetComponent<GridLayoutGroup>();
            int column_count = option_count switch
            {
                <= 1 => 1,
                2 => 2,
                _ => 3
            };
            grid.constraintCount = column_count;
            float width = 170f;
            float spacing = grid.spacing.x;
            float cell_width = (width - spacing * (column_count - 1)) / column_count;
            grid.cellSize = new Vector2(cell_width, 20f);

            if (option_count == 0)
            {
                GameObject empty = new GameObject("Empty", typeof(Text).Convert());
                empty.transform.SetParent(options_root);
                empty.transform.localScale = Vector3.one;
                empty.GetComponent<RectTransform>().sizeDelta = new Vector2(170, 20);
                Text empty_text = empty.GetComponent<Text>();
                OT.InitializeCommonText(empty_text);
                empty_text.alignment = TextAnchor.MiddleLeft;
                empty_text.text = "N/A";
                return;
            }

            for (int i = 0; i < option_count; i++)
            {
                int option_index = i;
                string option_id = $"{pItem.Id}_{option_index}";
                string option_text = LM.Get(option_id);
                SimpleButton option_button = Instantiate(SimpleButton.Prefab, options_root);
                option_button.transform.localScale = Vector3.one;
                option_button.name = $"Option_{option_index}";
                option_button.Setup(IL2CPPHelper.Convert<UnityAction>(() =>
                    {
                        mark_modified(pItem);
                        pItem.SetValue(option_index, true);
                        refresh_select_buttons(options_root, pItem.IntVal);
                    }),
                    null, option_text, new Vector2(54, 20));
                option_button.TipButton.enabled = false;
            }

            refresh_select_buttons(options_root, pItem.IntVal);
        }

        private static void refresh_select_buttons(Transform pOptionsRoot, int pSelected)
        {
            int button_index = 0;
            for (int i = 0; i < pOptionsRoot.childCount; i++)
            {
                SimpleButton option_button = pOptionsRoot.GetChild(i).GetComponent<SimpleButton>();
                if (option_button == null) continue;
                bool selected = button_index == pSelected;
                option_button.Background.sprite = SpriteTextureLoader.getSprite(
                    selected ? "ui/special/special_buttonRed" : "ui/special/special_buttonGray");
                option_button.Background.type = Image.Type.Sliced;
                option_button.Text.color = selected ? Color.white : new Color(0.9f, 0.9f, 0.9f, 1f);
                button_index++;
            }
        }

        private static void clear_children(Transform pTransform)
        {
            for (int i = pTransform.childCount - 1; i >= 0; i--)
            {
                Transform child = pTransform.GetChild(i);
                child.SetParent(null);
                Destroy(child.gameObject);
            }
        }

        private static bool TryParseFloat(string pText, out float pValue)
        {
            if (float.TryParse(pText, NumberStyles.Float | NumberStyles.AllowThousands,
                    CultureInfo.InvariantCulture, out pValue))
                return true;
            return float.TryParse(pText, out pValue);
        }

        private static bool TryParseInt(string pText, out int pValue)
        {
            if (int.TryParse(pText, NumberStyles.Integer, CultureInfo.InvariantCulture, out pValue))
                return true;
            return int.TryParse(pText, out pValue);
        }

        private static void mark_modified(ModConfigItem pItem)
        {
            if (!Instance._modifiedItems.ContainsKey(pItem))
            {
                Instance._modifiedItems.Add(pItem, pItem.GetValue());
            }
        }

        private void setup_switch(ModConfigItem pItem)
        {
            switch_area.SetActive(true);
            var switch_button = switch_area.transform.Find("Button").GetComponent<General.UI.Prefabs.SwitchButton>();
            switch_button.Setup(pItem.BoolVal, () =>
            {
                mark_modified(pItem);
                pItem.SetValue(!pItem.BoolVal, true);
            });
            switch_button.tip_button.textOnClick = pItem.Id;
            switch_button.tip_button.text_description_2 = pItem.Id + " Description";

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
}
