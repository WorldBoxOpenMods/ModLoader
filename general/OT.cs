using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.General;

/// <summary>
/// OT is short for "Object Tools", "Object" is short for "UnityEngine.Object"
/// </summary>
public static class OT
{
    /// <summary>Create plain text using the game's current font.</summary>
    public static Text CreateText(Transform pParent, string pName, int pFontSize,
        TextAnchor pAlignment = TextAnchor.MiddleLeft, Color? pColor = null)
    {
        var obj = new GameObject(pName, typeof(Text));
        obj.transform.SetParent(pParent, false);
        Text text = obj.GetComponent<Text>();
        InitializeCommonText(text);
        text.supportRichText = false;
        text.fontSize = pFontSize;
        text.color = pColor ?? Color.white;
        text.alignment = pAlignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.raycastTarget = false;
        return text;
    }

    /// <summary>
    /// Initialize common text(with font and rich text support)
    /// </summary>
    /// <param name="text">The text component to initialize</param>
    public static void InitializeCommonText(Text text)
    {
        text.font = LocalizedTextManager.current_font;
        text.supportRichText = true;
    }

    /// <summary>
    ///     Initialize a vertical layout group with no child control, upper center alignment
    /// </summary>
    /// <param name="pVerticalLayoutGroup"></param>
    public static void InitializeNoActionVerticalLayoutGroup(VerticalLayoutGroup pVerticalLayoutGroup)
    {
        pVerticalLayoutGroup.childAlignment = TextAnchor.UpperCenter;
        pVerticalLayoutGroup.childControlHeight = false;
        pVerticalLayoutGroup.childControlWidth = false;
        pVerticalLayoutGroup.childForceExpandHeight = false;
        pVerticalLayoutGroup.childForceExpandWidth = false;
        pVerticalLayoutGroup.childScaleHeight = false;
        pVerticalLayoutGroup.childScaleWidth = false;
    }
}
