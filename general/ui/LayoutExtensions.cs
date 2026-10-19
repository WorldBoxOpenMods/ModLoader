using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.General.UI;

/// <summary>
///     RectTransform and layout sizing shared by code-created UI.
/// </summary>
public static class LayoutExtensions
{
    /// <summary>Set a child's minimum, preferred and flexible size in its parent layout.</summary>
    public static LayoutElement SetLayoutSize(this GameObject pObject, float pWidth = -1, float pHeight = -1,
        float pFlexibleWidth = -1, float pFlexibleHeight = -1)
    {
        LayoutElement element = pObject.GetComponent<LayoutElement>() ?? pObject.AddComponent<LayoutElement>();
        element.preferredWidth = element.minWidth = pFlexibleWidth > 0 && pWidth < 0 ? 0 : pWidth;
        element.preferredHeight = element.minHeight = pFlexibleHeight > 0 && pHeight < 0 ? 0 : pHeight;
        element.flexibleWidth = pFlexibleWidth;
        element.flexibleHeight = pFlexibleHeight;
        return element;
    }

    /// <summary>Stretch to the parent, with symmetric margins in local canvas units.</summary>
    public static void Stretch(this RectTransform pRect, float pHorizontalMargin = 0, float pVerticalMargin = 0)
    {
        pRect.anchorMin = Vector2.zero;
        pRect.anchorMax = Vector2.one;
        pRect.offsetMin = new Vector2(pHorizontalMargin, pVerticalMargin);
        pRect.offsetMax = new Vector2(-pHorizontalMargin, -pVerticalMargin);
    }
}
