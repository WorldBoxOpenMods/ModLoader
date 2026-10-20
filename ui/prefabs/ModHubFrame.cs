using NeoModLoader.General.UI;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui.prefabs;

/// <summary>The stock window's header, scroll area and footer, fitted inside its native frame.</summary>
internal class ModHubFrame : MonoBehaviour
{
    private RectTransform scroll_container;

    internal ModHubToolbar Header { get; private set; }
    internal ModHubToolbar Footer { get; private set; }
    internal ScrollRect Scroll { get; private set; }
    internal RectTransform Content { get; private set; }
    internal RectTransform Overlays { get; private set; }

    internal static ModHubFrame Create(Transform pBackground, bool pParagraphs = false)
    {
        ScrollRect scroll = pBackground.Find("Scroll View").GetComponent<ScrollRect>();
        return Create(pBackground, scroll, scroll.content, "ModHubContent", pParagraphs, false);
    }

    internal static ModHubFrame CreatePage(Transform pBackground, string pName, ScrollRect pScroll)
    {
        var content = new GameObject(pName + "Content", typeof(RectTransform));
        content.transform.SetParent(pScroll.viewport, false);
        content.SetActive(false);
        return Create(pBackground, pScroll, (RectTransform)content.transform, pName, false, true);
    }

    private static ModHubFrame Create(Transform pBackground, ScrollRect pScroll, RectTransform pContent,
        string pName, bool pParagraphs, bool pPage)
    {
        Transform gradient = pBackground.Find("Scrollgradient");
        if (gradient != null)
        {
            // ScrollWindow toggles this object itself. Disable its graphics so its fixed-size fade cannot
            // cover our footer when the list gets a scrollbar.
            foreach (Graphic graphic in gradient.GetComponentsInChildren<Graphic>(true)) graphic.enabled = false;
        }
        var root = new GameObject(pName, typeof(RectTransform), typeof(VerticalLayoutGroup));
        root.transform.SetParent(pBackground, false);
        if (pPage) root.SetActive(false);
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
        Transform scroll_view = pScroll.transform;
        if (pPage)
        {
            var area = new GameObject("ScrollArea", typeof(RectTransform));
            area.transform.SetParent(root.transform, false);
            area.SetLayoutSize(pFlexibleHeight: 1);
            frame.scroll_container = (RectTransform)area.transform;
            scroll_view.SetParent(area.transform, false);
            ((RectTransform)scroll_view).Stretch();
        }
        else
        {
            scroll_view.SetParent(root.transform, false);
        }
        scroll_view.gameObject.SetLayoutSize(pFlexibleHeight: 1);
        frame.Scroll = pScroll;
        frame.Content = pContent;
        frame.Scroll.horizontal = false;
        frame.Scroll.movementType = ScrollRect.MovementType.Clamped;
        frame.Scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
        frame.FitViewport();
        frame.Footer = ModHubToolbar.Create(root.transform, "Footer");

        if (pPage)
        {
            var overlays = new GameObject(pName + "Overlays", typeof(RectTransform));
            overlays.transform.SetParent(scroll_view, false);
            frame.Overlays = (RectTransform)overlays.transform;
            frame.Overlays.Stretch();
            overlays.SetActive(false);
        }

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

    /// <summary>Reuse the native scroller and scrollbar while switching only this page's content.</summary>
    internal void Activate(ScrollWindow pWindow)
    {
        Scroll.StopMovement();
        Scroll.content = Content;
        Scroll.transform.SetParent(scroll_container, false);
        ((RectTransform)Scroll.transform).Stretch();
        Content.gameObject.SetActive(true);
        Overlays.gameObject.SetActive(true);
        pWindow.transform_content = Content;
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
