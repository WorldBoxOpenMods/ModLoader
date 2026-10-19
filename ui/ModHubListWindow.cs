using System.Collections;
using NeoModLoader.api;
using NeoModLoader.General;
using NeoModLoader.General.UI;
using NeoModLoader.ui.prefabs;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui;

/// <summary>
///     Common frame and scroll lifecycle for the two Mod Hub lists.
/// </summary>
internal abstract class ModHubListWindow<T, TItem> : AbstractListWindow<T, TItem>
    where T : ModHubListWindow<T, TItem>
{
    private ScrollRect scroll;
    private float scroll_position = 1;
    private Coroutine restore_scroll;

    protected ModHubFrame Frame { get; private set; }

    protected sealed override void Init()
    {
        Frame = ModHubFrame.Create(BackgroundTransform);
        scroll = Frame.Scroll;
        ModHubUI.AddHubTabs(BackgroundTransform.parent, WindowId);
        InitHub();
    }

    protected abstract void InitHub();
    internal abstract void Refresh();

    protected Text CreateEmptyMessage(string pLocaleKey = null)
    {
        Text text = OT.CreateText(scroll.transform, "EmptyMessage", 8, TextAnchor.MiddleCenter, Color.gray);
        text.rectTransform.Stretch(12);
        if (pLocaleKey != null) ModHubUI.SetLocaleKey(text, pLocaleKey);
        return text;
    }

    public override void OnNormalEnable()
    {
        Refresh();
        restore_scroll = StartCoroutine(RestoreScroll());
    }

    public override void OnNormalDisable()
    {
        scroll_position = CurrentScrollPosition();
        if (restore_scroll != null) StopCoroutine(restore_scroll);
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

        Canvas.ForceUpdateCanvases();
        scroll.StopMovement();
        scroll.verticalNormalizedPosition = Mathf.Clamp01(position);
    }

    protected void ResetScroll()
    {
        scroll_position = 1;
        scroll.StopMovement();
        scroll.verticalNormalizedPosition = 1;
    }

    private float CurrentScrollPosition()
    {
        return scroll.content.rect.height > scroll.viewport.rect.height ? scroll.verticalNormalizedPosition : 1;
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
