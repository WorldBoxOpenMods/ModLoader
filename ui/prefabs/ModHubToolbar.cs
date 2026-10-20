using NeoModLoader.General;
using NeoModLoader.General.UI;
using NeoModLoader.General.UI.Prefabs;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NeoModLoader.ui.prefabs;

/// <summary>One row of Mod Hub actions. Buttons and search fields use the existing NML prefabs.</summary>
internal class ModHubToolbar : MonoBehaviour
{
    internal static ModHubToolbar Create(Transform pParent, string pName, float pHeight = 20)
    {
        var obj = new GameObject(pName, typeof(RectTransform), typeof(HorizontalLayoutGroup));
        obj.transform.SetParent(pParent, false);
        var layout = obj.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 4;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = layout.childForceExpandHeight = false;
        obj.SetLayoutSize(pHeight: pHeight, pFlexibleHeight: 0);
        return obj.AddComponent<ModHubToolbar>();
    }

    internal SimpleButton AddIcon(string pName, Sprite pIcon, UnityAction pOnClick, string pTooltipKey)
    {
        SimpleButton button = SimpleButton.Instantiate(transform, pName: pName);
        button.Setup(pOnClick, pIcon, pSize: new Vector2(20, 20));
        button.Background.sprite = SpriteTextureLoader.getSprite("ui/special/special_buttonGray");
        button.gameObject.SetLayoutSize(20, 20);
        if (pTooltipKey != null)
        {
            button.TipButton.enabled = true;
            button.TipButton.type = "normal";
            button.TipButton.textOnClick = pTooltipKey;
            button.TipButton.hoverAction = button.TipButton.showTooltipDefault;
        }
        return button;
    }

    /// <summary>Cycle through enum choices and keep the icon and localized description in sync.</summary>
    internal SimpleButton AddChoice<T>(string pName, string pTooltipKey, Func<T> pGetValue, Action<T> pOnChanged,
        Func<T, string> pIconName, string pDescriptionPrefix = null) where T : Enum
    {
        var values = (T[])Enum.GetValues(typeof(T));
        SimpleButton button = AddIcon(pName, null, () =>
        {
            int next = (Array.IndexOf(values, pGetValue()) + 1) % values.Length;
            pOnChanged(values[next]);
        }, pTooltipKey);
        void UpdateChoice()
        {
            T value = pGetValue();
            button.Icon.sprite = SpriteTextureLoader.getSprite("ui/icons/" + pIconName(value));
            // Native tooltips normalize keys to lower case before looking them up.
            button.TipButton.textOnClickDescription =
                (pDescriptionPrefix ?? pTooltipKey) + "_" + value.ToString().ToLowerInvariant();
        }
        button.Button.onClick.AddListener(UpdateChoice);
        UpdateChoice();
        return button;
    }

    internal SimpleButton AddText(string pName, string pTextKey, UnityAction pOnClick, bool pPrimary = false)
    {
        SimpleButton button = SimpleButton.Instantiate(transform, pName: pName);
        button.Setup(pOnClick, null, LM.Get(pTextKey), new Vector2(60, 20));
        button.Text.gameObject.AddComponent<LocalizedText>().setKeyAndUpdate(pTextKey);
        button.Background.sprite = SpriteTextureLoader.getSprite(pPrimary
            ? "ui/special/special_buttonRed" : "ui/special/special_buttonGray");
        button.Text.resizeTextMinSize = 7;
        button.Text.resizeTextMaxSize = 8;
        button.Text.supportRichText = false;
        button.Text.raycastTarget = false;
        button.Text.rectTransform.Stretch(4);
        button.gameObject.SetLayoutSize(pHeight: 20, pFlexibleWidth: 1);
        return button;
    }

    internal void AddSpacer()
    {
        var spacer = new GameObject("Spacer", typeof(RectTransform));
        spacer.transform.SetParent(transform, false);
        spacer.SetLayoutSize(pFlexibleWidth: 1);
    }

    internal TextInput AddSearch(string pText, UnityAction<string> pOnChanged)
    {
        TextInput search = TextInput.Instantiate(transform, pName: "Search");
        search.Setup(pText, null);
        search.input.onValueChanged.AddListener(pOnChanged);
        search.gameObject.SetLayoutSize(pHeight: 20, pFlexibleWidth: 1);
        // The toolbar assigns the final width after Setup, so anchor the input's contents to it.
        search.text.rectTransform.Stretch(4, 2);
        search.text.rectTransform.offsetMax = new Vector2(-20, -2);
        search.text.resizeTextForBestFit = false;
        search.text.fontSize = 8;
        search.text.supportRichText = false;
        search.text.raycastTarget = true;
        search.icon.rectTransform.anchorMin = search.icon.rectTransform.anchorMax = new Vector2(1, 0.5f);
        search.icon.rectTransform.anchoredPosition = new Vector2(-10, 0);
        search.icon.rectTransform.sizeDelta = new Vector2(16, 16);
        search.icon.raycastTarget = false;
        search.tip_button.enabled = false;
        Text placeholder = OT.CreateText(search.transform, "Placeholder", 8, pColor: new Color(0.7f, 0.72f, 0.66f));
        placeholder.rectTransform.Stretch(4, 2);
        placeholder.rectTransform.offsetMax = new Vector2(-20, -2);
        placeholder.gameObject.AddComponent<LocalizedText>().setKeyAndUpdate("mod_hub_search_placeholder");
        search.input.placeholder = placeholder;
        placeholder.gameObject.SetActive(string.IsNullOrEmpty(pText));
        return search;
    }
}
