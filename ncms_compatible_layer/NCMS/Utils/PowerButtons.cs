using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace NCMS.Utils
{
    [Obsolete("Compatible Layer will not be maintained and be removed in the future")]
    public class PowerButtons
    {
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

            // Set custom click callback
            if (call != null)
            {
                asButton.onClick.AddListener(call);
            }
            switch (type)
            {
                case ButtonType.Click:
                    asPowerButton.type = PowerButtonType.Library;
                    break;
                case ButtonType.GodPower:
                    asPowerButton.type = PowerButtonType.Active;
                    break;
                case ButtonType.Toggle:
                    asPowerButton.type = PowerButtonType.Special;
                    break;
            }
            obj.gameObject.SetActive(true);

            return asPowerButton;
        }

        public static Button CreateTextButton(string name, string text, Vector2 position, Color color,
            Transform parent = null, UnityAction callback = null)
        {
            throw new NotImplementedException();
        }

        public static void AddButtonToTab(PowerButton button, PowersTab tab, Vector2 position)
        {

        }

        public static bool GetToggleValue(string name)
        {
            throw new NotImplementedException();
        }

        public static void ToggleButton(string name)
        {

        }
    }
}
