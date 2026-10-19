using NeoModLoader.api;
using NeoModLoader.General;
using NeoModLoader.General.UI;
using NeoModLoader.General.UI.Prefabs;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui.prefabs;

/// <summary>
///     Shared card layout for installed mods and Workshop listings. Serialized references survive pool cloning.
/// </summary>
internal abstract class ModHubRow<TItem> : AbstractListWindowItem<TItem>
{
    [SerializeField] protected Image Icon;
    [SerializeField] protected Text Title;
    [SerializeField] protected Transform TitleLine;
    [SerializeField] protected Transform Controls;

    protected static TRow CreateRow<TRow>() where TRow : ModHubRow<TItem>
    {
        var obj = new GameObject(typeof(TRow).Name, typeof(Image), typeof(HorizontalLayoutGroup));
        obj.transform.SetParent(WorldBoxMod.Transform, false);
        obj.GetComponent<Image>().sprite = SpriteTextureLoader.getSprite("ui/special/windowInnerSlicedRound");
        obj.GetComponent<Image>().type = Image.Type.Sliced;
        obj.SetActive(false);
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 48);
        var layout = obj.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 8;
        layout.padding = new RectOffset(8, 4, 4, 4);
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = layout.childForceExpandHeight = false;

        TRow row = obj.AddComponent<TRow>();
        var image = new GameObject("Icon", typeof(Image));
        image.transform.SetParent(obj.transform, false);
        row.Icon = image.GetComponent<Image>();
        row.Icon.preserveAspect = true;
        image.SetLayoutSize(32, 32);
        var body = new GameObject("Body", typeof(RectTransform));
        body.transform.SetParent(obj.transform, false);
        var body_layout = body.AddComponent<VerticalLayoutGroup>();
        body_layout.spacing = 2;
        body_layout.childControlWidth = body_layout.childControlHeight = true;
        body_layout.childForceExpandWidth = true;
        body_layout.childForceExpandHeight = false;
        body.SetLayoutSize(pFlexibleWidth: 1);

        row.TitleLine = Line(body.transform, "TitleLine", 18);
        row.Title = OT.CreateText(row.TitleLine, "Title", 8);
        row.Title.gameObject.SetLayoutSize(pFlexibleWidth: 1, pHeight: 18);
        row.Title.raycastTarget = true;
        row.Title.gameObject.AddComponent<Button>();
        var tip = row.Title.gameObject.AddComponent<TipButton>();
        tip.type = "normal";
        tip.override_click_scale_animation = true;
        tip.overridden_click_scale_animation = 1;
        row.Title.gameObject.AddComponent<ScrollableButton>();
        var divider = new GameObject("Divider", typeof(Image));
        divider.transform.SetParent(body.transform, false);
        divider.GetComponent<Image>().color = new Color(0, 0, 0, 0.35f);
        divider.GetComponent<Image>().raycastTarget = false;
        divider.SetLayoutSize(pHeight: 1);
        row.Controls = Line(body.transform, "ControlLine", 16);
        return row;
    }

    private static Transform Line(Transform pParent, string pName, float pHeight)
    {
        var line = new GameObject(pName, typeof(RectTransform));
        line.transform.SetParent(pParent, false);
        var layout = line.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 4;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = layout.childForceExpandHeight = false;
        line.SetLayoutSize(pHeight: pHeight);
        return line.transform;
    }

    protected SimpleButton AddAction(string pName, Sprite pIcon, string pTooltipKey)
    {
        SimpleButton button = SimpleButton.Instantiate(Controls, pName: pName);
        button.Setup(null, pIcon, pSize: new Vector2(16, 16));
        button.Background.sprite = SpriteTextureLoader.getSprite("ui/special/special_buttonGray");
        button.gameObject.SetLayoutSize(16, 16);
        button.TipButton.enabled = true;
        button.TipButton.type = "normal";
        button.TipButton.textOnClick = pTooltipKey;
        button.TipButton.hoverAction = button.TipButton.showTooltipDefault;
        button.gameObject.AddComponent<ScrollableButton>();
        return button;
    }
}
