using NeoModLoader.api;
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
        BackgroundTransform.Find("Scroll View").gameObject.SetActive(false);
        message = ModHubUI.CreateText(BackgroundTransform, "Message", new Vector2(200, 170), new Vector2(0, 25), 9,
            TextAnchor.MiddleCenter);
        ModHubUI.CreateTextButton(BackgroundTransform, "Confirm", "mod_hub_confirm", new Vector2(80, 24),
            new Vector2(-45, -100), Confirm);
        ModHubUI.CreateTextButton(BackgroundTransform, "Cancel", "mod_hub_cancel", new Vector2(80, 24),
            new Vector2(45, -100), Close);
    }

    private void Confirm()
    {
        Action action = on_confirm;
        on_confirm = null;
        // Run first, so the previous window shows the result when it is enabled again.
        action?.Invoke();
        Close();
    }

    private void Close()
    {
        on_confirm = null;
        GetComponent<ScrollWindow>().clickBack();
    }
}
