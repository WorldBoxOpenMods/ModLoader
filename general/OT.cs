using UnityEngine.UI;

namespace NeoModLoader.General;

public static class OT
{
    public static void InitializeCommonText(Text text)
    {
        text.font = LocalizedTextManager.currentFont;
        text.supportRichText = true;
    }
}