using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.General;

/// <summary>
/// OT is short for "Object Tools", "Object" is short for "UnityEngine.Object"
/// </summary>
public static class OT
{
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