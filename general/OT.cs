using UnityEngine.UI;

namespace NeoModLoader.General;
/// <summary>
/// OT is short for "Object Tools"
/// </summary>
public static class OT
{
    public static void InitializeCommonText(Text text)
    {
        text.font = LocalizedTextManager.currentFont;
        text.supportRichText = true;
    }
}