using System.Collections;
using NeoModLoader.api;
using NeoModLoader.General;
using NeoModLoader.General.UI;
using NeoModLoader.ui.prefabs;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui;

/// <summary>
///     A pooled list inside the shared Mod Hub window, with its own query and scroll position.
/// </summary>
internal abstract class ModHubListPage<T, TItem> : AbstractListWindow<T, TItem>
    where T : ModHubListPage<T, TItem>
{
    private ScrollRect scroll;
    private float scroll_position = 1;
    private Coroutine restore_scroll;

    internal ModHubFrame Frame { get; private set; }

    internal static T Create(Transform pBackground, string pId, ScrollRect pScroll)
    {
        WindowId = pId;
        ModHubFrame frame = ModHubFrame.CreatePage(pBackground, pId, pScroll);
        Instance = frame.gameObject.AddComponent<T>();
        Instance.Frame = frame;
        Instance.scroll = pScroll;
        Instance.BackgroundTransform = pBackground;
        Instance.ContentTransform = frame.Content;
        ItemPrefab = Instance.CreateItemPrefab();
        Instance.Init();
        Instance.Initialized = true;
        return Instance;
    }

    protected sealed override void Init() => InitHub();

    protected abstract void InitHub();
    internal abstract void Refresh();

    protected Text CreateEmptyMessage(string pLocaleKey = null)
    {
        Text text = OT.CreateText(Frame.Overlays, "EmptyMessage", 8, TextAnchor.MiddleCenter, Color.gray);
        text.rectTransform.Stretch(12);
        if (pLocaleKey != null) text.gameObject.AddComponent<LocalizedText>().setKeyAndUpdate(pLocaleKey);
        return text;
    }

    public override void OnNormalEnable()
    {
        Frame.Activate(GetComponentInParent<ScrollWindow>());
        Refresh();
        restore_scroll = StartCoroutine(RestoreScroll());
    }

    public override void OnNormalDisable()
    {
        scroll_position = CurrentScrollPosition();
        if (restore_scroll != null) StopCoroutine(restore_scroll);
        scroll.StopMovement();
        Frame.Content.gameObject.SetActive(false);
        Frame.Overlays.gameObject.SetActive(false);
    }

    protected void ShowItems(IEnumerable<TItem> pItems)
    {
        float position = CurrentScrollPosition();
        ClearList();
        foreach (TItem item in pItems)
        {
            AddItemToList(item);
            // Pool reuse order can differ from the new sort order.
            ItemMap[item].transform.SetAsLastSibling();
        }

        // An asynchronous action can refresh a hidden page. Leave the visible page's scroller alone.
        if (gameObject.activeInHierarchy && scroll.content == Frame.Content)
        {
            Canvas.ForceUpdateCanvases();
            scroll.StopMovement();
            scroll.verticalNormalizedPosition = Mathf.Clamp01(position);
        }
    }

    protected void ResetScroll()
    {
        scroll_position = 1;
        if (gameObject.activeInHierarchy && scroll.content == Frame.Content)
        {
            scroll.StopMovement();
            scroll.verticalNormalizedPosition = 1;
        }
    }

    private float CurrentScrollPosition()
    {
        if (scroll.content != Frame.Content) return scroll_position;
        return Frame.Content.rect.height > scroll.viewport.rect.height ? scroll.verticalNormalizedPosition : 1;
    }

    private IEnumerator RestoreScroll()
    {
        // ScrollWindow resets its content after OnEnable. Restore after that reset and the layout pass.
        yield return null;
        Canvas.ForceUpdateCanvases();
        scroll.StopMovement();
        scroll.verticalNormalizedPosition = Mathf.Clamp01(scroll_position);
        restore_scroll = null;
    }
}
