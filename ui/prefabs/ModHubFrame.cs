using NeoModLoader.General.UI;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui.prefabs;

/// <summary>The stock window's header, scroll area and footer, fitted inside its native frame.</summary>
internal class ModHubFrame : MonoBehaviour
{
    internal ModHubToolbar Header { get; private set; }
    internal ModHubToolbar Footer { get; private set; }
    internal ScrollRect Scroll { get; private set; }
    internal RectTransform Content => Scroll.content;

    internal static ModHubFrame Create(Transform pBackground, bool pParagraphs = false)
    {
        Transform gradient = pBackground.Find("Scrollgradient");
        if (gradient != null)
        {
            // ScrollWindow toggles this object itself. Disable its graphics so its fixed-size fade cannot
            // cover our footer when the list gets a scrollbar.
            foreach (Graphic graphic in gradient.GetComponentsInChildren<Graphic>(true)) graphic.enabled = false;
        }
        var root = new GameObject("ModHubContent", typeof(RectTransform), typeof(VerticalLayoutGroup));
        root.transform.SetParent(pBackground, false);
        var rect = (RectTransform)root.transform;
        rect.Stretch();
        rect.offsetMin = new Vector2(12, 22);
        rect.offsetMax = new Vector2(-12, -44);
        var layout = root.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 8;
        layout.childControlHeight = layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        ModHubFrame frame = root.AddComponent<ModHubFrame>();
        frame.Header = ModHubToolbar.Create(root.transform, "Header");
        Transform scroll_view = pBackground.Find("Scroll View");
        scroll_view.SetParent(root.transform, false);
        scroll_view.gameObject.SetLayoutSize(pFlexibleHeight: 1);
        frame.Scroll = scroll_view.GetComponent<ScrollRect>();
        frame.Scroll.horizontal = false;
        frame.Scroll.movementType = ScrollRect.MovementType.Clamped;
        frame.Scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
        frame.FitViewport();
        frame.Footer = ModHubToolbar.Create(root.transform, "Footer");

        frame.Content.anchorMin = new Vector2(0, 1);
        frame.Content.anchorMax = Vector2.one;
        frame.Content.pivot = new Vector2(0.5f, 1);
        frame.Content.sizeDelta = frame.Content.anchoredPosition = Vector2.zero;
        frame.Content.Find("runes_parent")?.gameObject.SetActive(false);
        var list = frame.Content.GetComponent<VerticalLayoutGroup>() ?? frame.Content.gameObject.AddComponent<VerticalLayoutGroup>();
        list.padding = new RectOffset();
        list.spacing = pParagraphs ? 8 : 4;
        list.childAlignment = TextAnchor.UpperCenter;
        list.childControlWidth = list.childForceExpandWidth = true;
        list.childControlHeight = pParagraphs;
        list.childForceExpandHeight = false;
        var fitter = frame.Content.GetComponent<ContentSizeFitter>() ?? frame.Content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        return frame;
    }

    internal ModHubToolbar InsertToolbar(string pName, int pIndex, float pHeight = 20)
    {
        ModHubToolbar toolbar = ModHubToolbar.Create(transform, pName, pHeight);
        toolbar.transform.SetSiblingIndex(pIndex);
        return toolbar;
    }

    private void FitViewport()
    {
        var scrollbar = (RectTransform)Scroll.verticalScrollbar.transform;
        var mask = (RectTransform)Scroll.transform.Find("Scrollbar Vertical Mask");
        float width = scrollbar.rect.width;
        float right_margin = -((RectTransform)transform).offsetMax.x;
        const float frame_overlap = 4;
        mask.anchorMin = new Vector2(1, 0);
        mask.anchorMax = Vector2.one;
        mask.pivot = new Vector2(0.5f, 0.5f);
        // The pixel frame's visible side is inset from its rectangular bounds. Overlap that border to close
        // the visual gap, while keeping the native scrollbar's shown local X at zero.
        mask.anchoredPosition = new Vector2(right_margin + width / 2 - frame_overlap, 0);
        mask.sizeDelta = new Vector2(width, 0);

        scrollbar.anchorMin = new Vector2(0.5f, 0);
        scrollbar.anchorMax = new Vector2(0.5f, 1);
        scrollbar.pivot = new Vector2(0.5f, 0.5f);
        scrollbar.anchoredPosition = Vector2.zero;
        scrollbar.sizeDelta = new Vector2(width, 0);
        var background = (RectTransform)scrollbar.Find("Background");
        background.anchorMin = new Vector2(0.5f, 0);
        background.anchorMax = new Vector2(0.5f, 1);
        background.sizeDelta = new Vector2(background.sizeDelta.x, 0);
        Scroll.viewport.Stretch();
    }
}
