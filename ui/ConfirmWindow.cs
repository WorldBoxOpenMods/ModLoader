using NeoModLoader.api;
using NeoModLoader.General;
using NeoModLoader.ui.prefabs;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui;

/// <summary>
///     Asks the player to confirm an action before it runs.
/// </summary>
internal class ConfirmWindow : AbstractWindow<ConfirmWindow>
{
    private Text message;
    private Action on_confirm;

    /// <param name="pMessage">Localized question that says what will happen</param>
    /// <param name="pOnConfirm">Runs when the player confirms, before returning to the previous window</param>
    public static void ShowWindow(string pMessage, Action pOnConfirm)
    {
        Instance.message.text = pMessage;
        Instance.on_confirm = pOnConfirm;
        ScrollWindow.showWindow(WindowId);
    }

    protected override void Init()
    {
        ModHubFrame frame = ModHubFrame.Create(BackgroundTransform, true);
        frame.Header.gameObject.SetActive(false);
        message = OT.CreateText(ContentTransform, "Message", 10, TextAnchor.UpperLeft);
        message.verticalOverflow = VerticalWrapMode.Overflow;
        frame.Footer.AddText("Cancel", "mod_hub_cancel", Close);
        frame.Footer.AddText("Confirm", "mod_hub_confirm", Confirm, true);
    }

    public override void OnNormalEnable() => GetComponent<ScrollWindow>().resetScroll();

    public override void OnNormalDisable() => on_confirm = null;

    private void Confirm()
    {
        if (ScrollWindow.isAnimationActive()) return;
        Action action = on_confirm;
        on_confirm = null;
        // Run first, so the previous window shows the result when it is enabled again.
        action?.Invoke();
        if (ScrollWindow.getCurrentWindow() == GetComponent<ScrollWindow>()) Close();
    }

    private void Close()
    {
        if (ScrollWindow.isAnimationActive()) return;
        on_confirm = null;
        GetComponent<ScrollWindow>().clickBack();
    }
}
