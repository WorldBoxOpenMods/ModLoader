using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NeoModLoader.General.UI.Prefabs;

public class TextInput : APrefab<TextInput>
{
    private Text _text;
    private InputField _input;
    private Image _icon;
    private void Awake()
    {
        _text = transform.Find("InputField").GetComponent<Text>();
        _input = transform.Find("InputField").GetComponent<InputField>();
        _icon = transform.Find("Icon").GetComponent<Image>();
    }

    public void Setup(string value, UnityAction<string> value_update)
    {
        _input.onEndEdit.RemoveAllListeners();
        _input.text = value;
        _input.onEndEdit.AddListener(value_update);
    }
    public void SetSize(Vector2 size)
    {
        GetComponent<RectTransform>().sizeDelta = size;
        _text.GetComponent<RectTransform>().sizeDelta = size - new Vector2(size.y / 2 + 4, 2);
        _icon.GetComponent<RectTransform>().sizeDelta = new Vector2(size.y, size.y) - new Vector2(2, 2);
        _text.transform.localPosition = new Vector3(-size.x / 2, 0);
        _icon.transform.localPosition = new Vector3((size.x - size.y / 2) / 2, 0);
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
    }
}