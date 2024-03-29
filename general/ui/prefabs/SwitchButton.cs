using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.General.UI.Prefabs;

public class SwitchButton : APrefab<SwitchButton>
{
    public Button    button     { get; private set; }
    public Image     icon       { get; private set; }
    public Text      text       { get; private set; }
    public TipButton tip_button { get; private set; }

    private void Awake()
    {
        if (!Initialized) Init();
    }

    protected override void Init()
    {
        base.Init();
        button = GetComponent<Button>();
        text = transform.Find("Text").GetComponent<Text>();
        icon = transform.Find("Icon").GetComponent<Image>();
        tip_button = GetComponent<TipButton>();
    }

    public void Setup(bool value, Action value_update)
    {
        if (!Initialized) Init();
        icon.sprite = value
            ? SpriteTextureLoader.getSprite("ui/icons/iconOn")
            : SpriteTextureLoader.getSprite("ui/icons/iconOff");
        text.text = value ? LM.Get("short_on") : LM.Get("short_off");
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            value_update();
            Setup(!value, value_update);
        });
    }

    internal static void _init()
    {
        GameObject switch_button = new GameObject("SwitchButton", typeof(Image), typeof(Button), typeof(TipButton),
                                                  typeof(HorizontalLayoutGroup));
        switch_button.transform.SetParent(WorldBoxMod.Transform);
        switch_button.transform.localScale = Vector3.one;
        switch_button.GetComponent<RectTransform>().sizeDelta = new(50, 18);

        HorizontalLayoutGroup layout = switch_button.GetComponent<HorizontalLayoutGroup>();
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childAlignment = TextAnchor.MiddleCenter;

        switch_button.GetComponent<Image>().sprite = SpriteTextureLoader.getSprite("ui/special/special_buttonRed");
        switch_button.GetComponent<Image>().type = Image.Type.Sliced;
        GameObject switch_button_icon = new GameObject("Icon", typeof(Image));
        switch_button_icon.transform.SetParent(switch_button.transform);
        switch_button_icon.transform.localScale = Vector3.one;
        switch_button_icon.GetComponent<RectTransform>().sizeDelta = new(18, 18);
        GameObject switch_button_text = new GameObject("Text", typeof(Text));
        switch_button_text.transform.SetParent(switch_button.transform);
        switch_button_text.transform.localScale = Vector3.one;
        switch_button_text.GetComponent<RectTransform>().sizeDelta = new(24, 18);
        Text text = switch_button_text.GetComponent<Text>();
        text.resizeTextForBestFit = true;
        OT.InitializeCommonText(text);
        text.alignment = TextAnchor.MiddleCenter;

        Prefab = switch_button.AddComponent<SwitchButton>();
    }
}