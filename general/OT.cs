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
        text.font = LocalizedTextManager.currentFont;
        text.supportRichText = true;
    }
}