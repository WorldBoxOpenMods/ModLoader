using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NeoModLoader.General.UI.Prefabs;

/// <summary>
/// A slider bar
/// </summary>
/// <example>
/// <code>
/// var slider_bar = Instantiate(SliderBar.Prefab, slider_area.transform); // Necessary
/// slider_bar.transform.localScale = Vector3.one;
/// slider_bar.name = "Slider";
/// slider_bar.SetSize(new Vector2(170f, 20)); // Necessary
/// ...
/// slider_bar.Setup(pItem.FloatVal, 0, 1, pFloatVal =>
/// {
///     pItem.SetValue(pFloatVal);
///     value.text = $"{pItem.FloatVal:F2}";
/// }); // Necessary
/// slider_bar.tip_button.textOnClick = pItem.Id;
/// slider_bar.tip_button.text_description_2 = pItem.Id + " Description";
/// </code>
/// </example>
public class SliderBar : APrefab<SliderBar>
{
    [SerializeField] private Slider _slider;

    [SerializeField] private TipButton _tip_button;

    public Slider slider => _slider;

    /// <summary>
    /// The tip button of the slider bar, used to show tooltip
    /// </summary>
    public TipButton tip_button => _tip_button;

    private void Awake()
    {
        if (!Initialized) Init();
    }

    /// <summary>
    /// Setup the slider bar
    /// </summary>
    /// <param name="value">Current value</param>
    /// <param name="min">Min</param>
    /// <param name="max">Max</param>
    /// <param name="value_update">Action when slider value updated</param>
    public void Setup(float value, float min, float max, UnityAction<float> value_update, Vector2 size = default,
        bool whole_numbers = false)
    {
        if (!Initialized) Init();
        slider.onValueChanged.RemoveAllListeners();
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = value;
        slider.wholeNumbers = whole_numbers;
        slider.onValueChanged.AddListener(value_update);
        if (size != default)
        {
            SetSize(size);
        }
    }

    /// <summary>
    /// Set the size of the slider bar, other components will be resized automatically
    /// </summary>
    /// <inheritdoc cref="APrefab{T}.SetSize"/>
    public override void SetSize(Vector2 size)
    {
        if (!Initialized) Init();
        GetComponent<RectTransform>().sizeDelta = size;
        transform.Find("Background").GetComponent<RectTransform>().sizeDelta = size - new Vector2(0, 10);
        transform.Find("Fill Area").GetComponent<RectTransform>().sizeDelta = size - new Vector2(0, 10);
        transform.Find("Fill Area/Fill").GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        transform.Find("Handle Slide Area").GetComponent<RectTransform>().sizeDelta = size - new Vector2(10, 0);
        transform.Find("Handle Slide Area/Handle").GetComponent<RectTransform>().sizeDelta = new Vector2(20, 0);
    }

    internal static void _init()
    {
        GameObject slider_bar = new GameObject("SliderBar", typeof(Slider), typeof(TipButton));
        slider_bar.transform.SetParent(WorldBoxMod.Transform);
        slider_bar.GetComponent<RectTransform>().sizeDelta = new(172, 20);

        GameObject background = new GameObject("Background", typeof(Image));
        background.transform.SetParent(slider_bar.transform);
        background.transform.localScale = Vector3.one;
        background.GetComponent<RectTransform>().sizeDelta = new(0, 0);
        background.GetComponent<Image>().sprite = SpriteTextureLoader.getSprite("ui/special/special_buttonGray");
        background.GetComponent<Image>().type = Image.Type.Sliced;

        GameObject fill_area = new GameObject("Fill Area", typeof(RectTransform));
        fill_area.transform.SetParent(slider_bar.transform);
        fill_area.transform.localScale = Vector3.one;
        fill_area.GetComponent<RectTransform>().sizeDelta = new(-20, 0);
        GameObject fill = new GameObject("Fill", typeof(Image));
        fill.transform.SetParent(fill_area.transform);
        fill.transform.localScale = Vector3.one;
        fill.GetComponent<RectTransform>().sizeDelta = new(10, 0);
        fill.GetComponent<Image>().sprite = SpriteTextureLoader.getSprite("ui/special/special_buttonRed");
        fill.GetComponent<Image>().type = Image.Type.Sliced;

        GameObject handle_area = new GameObject("Handle Slide Area", typeof(RectTransform));
        handle_area.transform.SetParent(slider_bar.transform);
        handle_area.transform.localScale = Vector3.one;
        handle_area.GetComponent<RectTransform>().sizeDelta = new(-20, 0);
        GameObject handle = new GameObject("Handle", typeof(Image));
        handle.transform.SetParent(handle_area.transform);
        handle.transform.localScale = Vector3.one;
        handle.GetComponent<Image>().sprite = SpriteTextureLoader.getSprite("ui/special/special_buttonRed");
        handle.GetComponent<Image>().type = Image.Type.Sliced;
        handle.GetComponent<RectTransform>().sizeDelta = new(20, 0);

        Prefab = slider_bar.AddComponent<SliderBar>();

        Slider slider = slider_bar.GetComponent<Slider>();
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.handleRect = handle.GetComponent<RectTransform>();
        slider.targetGraphic = handle.GetComponent<Image>();
        slider.direction = Slider.Direction.LeftToRight;
        slider.interactable = true;
        Prefab._slider = slider;
        Prefab._tip_button = slider_bar.GetComponent<TipButton>();
    }
}