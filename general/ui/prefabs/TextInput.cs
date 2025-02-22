using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NeoModLoader.General.UI.Prefabs;

/// <summary>
///     This class is used to create a text input with prefab.
/// </summary>
/// <inheritdoc cref="APrefab{T}" />
public class TextInput : APrefab<TextInput>
{
    [SerializeField] private Image _icon;

    [SerializeField] private InputField _input;

    [SerializeField] private Text _text;

    [SerializeField] private TipButton _tip_button;

    public Image icon => _icon;
    public InputField input => _input;

    /// <summary>
    ///     The <see cref="Text" /> component
    /// </summary>
    public Text text => _text;

    /// <summary>
    ///     The <see cref="TipButton" /> component
    /// </summary>
    public TipButton tip_button => _tip_button;

    private void Awake()
    {
        if (!Initialized) Init();
    }

    /// <summary>
    ///     Setup a TextInput with initial text <paramref name="value" />
    /// </summary>
    /// <param name="value">Initial text value</param>
    /// <param name="value_update">Callback when value updated</param>
    /// <param name="pIcon">icon at the right</param>
    /// <param name="pBackground"></param>
    public virtual void Setup(string value, UnityAction<string> value_update, Sprite pIcon = null,
        Sprite pBackground = null)
    {
        if (!Initialized) Init();
        input.onEndEdit.RemoveAllListeners();
        input.text = value;
        input.onEndEdit.AddListener(value_update);

        if (pIcon == null)
        {
            icon.sprite = SpriteTextureLoader.getSprite("ui/special/inputFieldIcon");
        }
        else
        {
            icon.sprite = pIcon;
        }

        if (pBackground == null)
        {
            GetComponent<Image>().sprite = SpriteTextureLoader.getSprite("ui/special/darkInputFieldEmpty");
        }
        else
        {
            GetComponent<Image>().sprite = pBackground;
        }
    }

    /// <summary>
    ///     Set the size of the text input, other components will be resized automatically
    /// </summary>
    /// <inheritdoc cref="APrefab{T}.SetSize" />
    public override void SetSize(Vector2 size)
    {
        if (!Initialized) Init();
        GetComponent<RectTransform>().sizeDelta = size;
        text.GetComponent<RectTransform>().sizeDelta = size - new Vector2(size.y / 2 + 4, 2);
        icon.GetComponent<RectTransform>().sizeDelta = new Vector2(size.y, size.y) - new Vector2(2, 2);
        text.transform.localPosition = new Vector3(-size.x / 2, 0);
        icon.transform.localPosition = new Vector3((size.x - size.y / 2) / 2, 0);
    }

    internal static void _init()
    {
        GameObject text_input = new GameObject("TextInput", typeof(TipButton), typeof(Image));
        text_input.transform.SetParent(WorldBoxMod.Transform);

        Image bg = text_input.GetComponent<Image>();
        bg.sprite = SpriteTextureLoader.getSprite("ui/special/darkInputFieldEmpty");
        bg.type = Image.Type.Sliced;

        GameObject input_field = new GameObject("InputField", typeof(Text), typeof(InputField));
        input_field.transform.SetParent(text_input.transform);
        input_field.transform.localScale = Vector3.one;
        input_field.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);

        Text text = input_field.GetComponent<Text>();
        OT.InitializeCommonText(text);
        text.alignment = TextAnchor.MiddleLeft;
        text.resizeTextForBestFit = true;

        InputField input = input_field.GetComponent<InputField>();
        input.textComponent = text;
        input.text = "";
        input.lineType = InputField.LineType.SingleLine;

        GameObject icon = new GameObject("Icon", typeof(Image));
        icon.transform.SetParent(text_input.transform);
        icon.transform.localScale = Vector3.one;
        icon.GetComponent<Image>().sprite = SpriteTextureLoader.getSprite("ui/special/inputFieldIcon");

        Prefab = text_input.AddComponent<TextInput>();
        Prefab._icon = icon.GetComponent<Image>();
        Prefab._input = input;
        Prefab._text = text;
        Prefab._tip_button = text_input.GetComponent<TipButton>();
    }
}