using NeoModLoader.General;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

#pragma warning disable CS1591 // No comment for NCMS compatible layer
namespace NCMS.Utils
{
    [Obsolete("Compatible Layer will not be maintained and be removed in the future")]
    public class PowerButtons
    {
        private static Dictionary<string, PowerButton> toggle_buttons = new Dictionary<string, PowerButton>();

        public static Dictionary<string, PowerButton> CustomButtons = new Dictionary<string, PowerButton>();
        public static Dictionary<string, bool> ToggleValues = new();

        /// <remarks>
        ///     From [NCMS](https://denq04.github.io/ncms/)
        /// </remarks>
        public static PowerButton CreateButton(string name, Sprite sprite, string title, string description,
            Vector2 position, ButtonType type = ButtonType.Click,
            Transform parent = null, UnityAction call = null)
        {
            LM.AddToCurrentLocale(name, title);
            LM.AddToCurrentLocale(name + " Description", description);
            LM.ApplyLocale(false);
            PowerButton asPowerButton;
            switch (type)
            {
                case ButtonType.Click:
                    asPowerButton = PowerButtonCreator.CreateSimpleButton(name, call, sprite, parent, position);
                    CustomButtons[name] = asPowerButton;
                    return asPowerButton;
                case ButtonType.GodPower:
                    asPowerButton = PowerButtonCreator.CreateGodPowerButton(name, sprite, parent, position);
                    if (call != null) asPowerButton._button.onClick.AddListener(call);
                    CustomButtons[name] = asPowerButton;
                    return asPowerButton;
                case ButtonType.Toggle:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            // NCMS implements toggle button with its own way. So we cannot use PowerButtonCreator.CreateToggleButton here.
            var prefab = ResourcesFinder.FindResource<PowerButton>("map_kings_leaders").gameObject;

            // To avoid PowerButton's OnEnable() method
            bool found_active = prefab.activeSelf;
            prefab.SetActive(false);

            var obj = parent == null ? Object.Instantiate(prefab) : Object.Instantiate(prefab, parent);

            prefab.SetActive(found_active);
            obj.transform.localPosition = position;


            asPowerButton = obj.GetComponent<PowerButton>();
            Button asButton = obj.GetComponent<Button>();

            asButton.onClick.RemoveAllListeners();

            asPowerButton.open_window_id = string.Empty;

            // Set name
            asPowerButton.name = name;

            // Set sprite
            asPowerButton.icon.sprite = sprite;

            asPowerButton.type = PowerButtonType.Library;

            toggle_buttons[name] = asPowerButton;
            // DO NOT catch repeat key exception here. There is a NCMS mod that use it.
            ToggleValues.Add(name, false);

            asButton.onClick.AddListener(() => ToggleButton(name));
            // Set custom click callback
            if (call != null) asButton.onClick.AddListener(call);
            obj.transform.Find("ToggleIcon").GetComponent<ToggleIcon>().updateIcon(false);

            obj.SetActive(true);

            CustomButtons[name] = asPowerButton;
            return asPowerButton;
        }

        /// <remarks>
        ///     From [NCMS](https://denq04.github.io/ncms/).
        ///     <para>ATTENTION! button background color is modified to RED</para>
        /// </remarks>
        public static Button CreateTextButton(string name, string text, Vector2 position, Color color,
            Transform parent = null, UnityAction callback = null)
        {
            // Since this will be removed, it's not necessary to move it into APrefab
            GameObject button_obj = new GameObject(name, typeof(Image), typeof(Button));
            if (parent != null) button_obj.transform.SetParent(parent);
            button_obj.transform.localScale = Vector3.one;
            button_obj.transform.localPosition = position;
            // There may be wrong color compared to NCMS in most cases. But it's not an actual problem.
            button_obj.GetComponent<Image>().sprite = SpriteTextureLoader.getSprite("ui/special/special_buttonRed");
            button_obj.GetComponent<Image>().color = color;
            button_obj.GetComponent<Image>().SetNativeSize();
            button_obj.GetComponent<Button>().onClick.AddListener(callback);

            GameObject text_obj = new GameObject(name + "_text", typeof(Text), typeof(Outline));
            text_obj.transform.SetParent(button_obj.transform);
            text_obj.transform.localScale = Vector3.one;
            text_obj.transform.localPosition = Vector3.zero;

            Text text_text = text_obj.GetComponent<Text>();
            text_text.font = Resources.Load<Font>("fonts/roboto-bold");
            text_text.color = Color.white;
            text_text.text = text;
            text_text.fontSize = 12;
            text_text.alignment = TextAnchor.MiddleCenter;
            text_obj.GetComponent<RectTransform>().sizeDelta = button_obj.GetComponent<RectTransform>().sizeDelta;

            Outline outline = text_obj.GetComponent<Outline>();
            outline.effectDistance = new Vector2(1f, -1f);
            outline.effectColor = new Color(0f, 0f, 0f, 0.2f);

            return button_obj.GetComponent<Button>();
        }

        public static void AddButtonToTab(PowerButton button, PowerTab tab, Vector2 position)
        {
            PowerButtonCreator.AddButtonToTab(button, PowerButtonCreator.GetTab("Tab_" + tab.ToString()), position);
        }

        /// <remarks>
        ///     From [NCMS](https://denq04.github.io/ncms/)
        /// </remarks>
        public static bool GetToggleValue(string name)
        {
            if (!toggle_buttons.TryGetValue(name, out var button))
                throw new Exception($"Toggle button added by NCMS Method not found for {name}");
            if (button.transform.Find("ToggleIcon") == null)
                throw new Exception($"Toggle button added by NCMS Method is invalid for {name}");

            var power = AssetManager.powers.get(name);
            return power == null ? ToggleValues[name] : PlayerConfig.dict[power.toggle_name].boolVal;
        }

        /// <remarks>
        ///     From [NCMS](https://denq04.github.io/ncms/)
        /// </remarks>
        public static void ToggleButton(string name)
        {
            if (toggle_buttons.TryGetValue(name, out PowerButton button))
            {
                Transform toggle = button.transform.Find("ToggleIcon");
                if (button.transform.Find("ToggleIcon") == null)
                {
                    throw new Exception($"Toggle button added by NCMS Method is invalid for {name}");
                }

                GodPower power = AssetManager.powers.get(name);
                if (power == null)
                {
                    ToggleValues[name] = !ToggleValues[name];
                    toggle.GetComponent<ToggleIcon>().updateIcon(ToggleValues[name]);
                    return;
                }

                PlayerConfig.dict[power.toggle_name].boolVal = !PlayerConfig.dict[power.toggle_name].boolVal;
                button.checkToggleIcon();
            }
            else
            {
                throw new Exception($"Toggle button added by NCMS Method not found for {name}");
            }
        }
    }
}