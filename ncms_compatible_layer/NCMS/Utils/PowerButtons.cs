using NeoModLoader.General;
using ReflectionUtility;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NCMS.Utils
{
    [Obsolete("Compatible Layer will not be maintained and be removed in the future")]
    public class PowerButtons
    {
        private static Dictionary<string, PowerButton> toggle_buttons = new Dictionary<string, PowerButton>();
        private static Dictionary<string, bool> bak_toggle_values = new Dictionary<string, bool>();

        public static Dictionary<string, PowerButton> CustomButtons = new Dictionary<string, PowerButton>();
        public static Dictionary<string, bool> ToggleValues = bak_toggle_values;

        public static PowerButton CreateButton(string name, Sprite sprite, string title, string description,
            Vector2 position, ButtonType type = ButtonType.Click, Transform parent = null, UnityAction call = null)
        {
            LM.AddToCurrentLocale(name, title);
            LM.AddToCurrentLocale(name + " Description", description);

            string prefab_name;
            switch (type)
            {
                case ButtonType.GodPower:
                    prefab_name = "inspect";
                    break;
                case ButtonType.Click:
                    prefab_name = "worldlaws";
                    break;
                case ButtonType.Toggle:
                    prefab_name = "kingsAndLeaders";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            GameObject prefab = GameObjects.FindEvenInactive(prefab_name);

            // To avoid PowerButton's OnEnable() method
            bool found_active = prefab.activeSelf;
            prefab.SetActive(false);

            GameObject obj;
            if (parent == null)
            {
                obj = GameObject.Instantiate(prefab);
            }
            else
            {
                obj = GameObject.Instantiate(prefab, parent);
            }

            prefab.SetActive(found_active);
            obj.transform.localPosition = position;


            PowerButton asPowerButton = obj.GetComponent<PowerButton>();
            Button asButton = obj.GetComponent<Button>();

            asButton.onClick.RemoveAllListeners();
            asPowerButton.open_window_id = string.Empty;

            // Set name
            asPowerButton.name = name;

            // Set sprite
            obj.transform.Find("Icon").GetComponent<Image>().sprite = sprite;

            switch (type)
            {
                case ButtonType.Click:
                    asPowerButton.type = PowerButtonType.Library;
                    break;
                case ButtonType.GodPower:
                    asPowerButton.type = PowerButtonType.Active;
                    break;
                case ButtonType.Toggle:
                    asPowerButton.type = PowerButtonType.Library;
                    toggle_buttons.Add(name, asPowerButton);
                    bak_toggle_values.Add(name, false);
                    asButton.onClick.AddListener(() => ToggleButton(name));
                    obj.transform.Find("ToggleIcon").GetComponent<ToggleIcon>()
                        .CallMethod("updateIcon", bak_toggle_values[name]);
                    break;
            }

            // Set custom click callback
            if (call != null)
            {
                asButton.onClick.AddListener(call);
            }

            obj.gameObject.SetActive(true);

            CustomButtons[name] = asPowerButton;
            return asPowerButton;
        }

        public static Button CreateTextButton(string name, string text, Vector2 position, Color color,
            Transform parent = null, UnityAction callback = null)
        {
            GameObject button_obj = new GameObject(name, typeof(Image), typeof(Button));
            button_obj.transform.SetParent(parent);
            button_obj.transform.localScale = Vector3.one;
            button_obj.transform.localPosition = position;
            button_obj.GetComponent<Image>().sprite = SpriteTextureLoader.getSprite("ui/special/buttonWhite");
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
            if (parent != null)
            {
                button_obj.transform.SetParent(parent);
            }

            return button_obj.GetComponent<Button>();
        }

        public static void AddButtonToTab(PowerButton button, PowerTab tab, Vector2 position)
        {
            PowerButtonCreator.AddButtonToTab(button, PowerButtonCreator.GetTab("Tab_" + tab.ToString()), position);
        }

        public static bool GetToggleValue(string name)
        {
            if (toggle_buttons.TryGetValue(name, out PowerButton button))
            {
                Transform toggle = button.transform.Find("ToggleIcon");
                if (toggle == null)
                {
                    throw new Exception($"Toggle button added by NCMS Method is invalid for {name}");
                }

                GodPower power = AssetManager.powers.get(name);
                if (power == null)
                {
                    return bak_toggle_values[name];
                    throw new Exception(
                        $"Toggle button added by NCMS Method is invalid, GodPower not found for {name}");
                }

                return PlayerConfig.dict[power.toggle_name].boolVal;
            }

            throw new Exception($"Toggle button added by NCMS Method not found for {name}");
        }

        public static void ToggleButton(string name)
        {
            if (toggle_buttons.TryGetValue(name, out PowerButton button))
            {
                Transform toggle = button.transform.Find("ToggleIcon");
                if (toggle == null)
                {
                    throw new Exception($"Toggle button added by NCMS Method is invalid for {name}");
                }

                GodPower power = AssetManager.powers.get(name);
                if (power == null)
                {
                    bak_toggle_values[name] = !bak_toggle_values[name];
                    toggle.GetComponent<ToggleIcon>().CallMethod("updateIcon", bak_toggle_values[name]);
                    return;
                    throw new Exception(
                        $"Toggle button added by NCMS Method is invalid, GodPower not found for {name}");
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