using NeoModLoader.General;
using NeoModLoader.General.UI.Window;
using NeoModLoader.services;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui;

/// <summary>
///     A window that shows simple text.
/// </summary>
public class InformationWindow : SingleAutoLayoutWindow<InformationWindow>
{
    private Action on_close;
    private Text   text;

    /// <inheritdoc cref="InformationWindow.Init" />
    protected override void Init()
    {
        text = new GameObject("Text", typeof(Text)).GetComponent<Text>();
        OT.InitializeCommonText(text);
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 10;
        text.resizeTextMaxSize = 14;
        text.alignment = TextAnchor.MiddleCenter;

        AddChild(text.gameObject);
    }

    /// <summary>
    ///     Show the window with the given text.
    /// </summary>
    /// <param name="info"></param>
    public static void ShowWindow(string info, Action on_close = null)
    {
        Instance.text.text = info;
        Instance.on_close = on_close;
        ScrollWindow.showWindow(WindowId);
    }

    public override void OnNormalDisable()
    {
        try
        {
            on_close?.Invoke();
        }
        catch (Exception e)
        {
            LogService.LogError(e.Message);
            LogService.LogError(e.StackTrace);
        }

        on_close = null;
    }

    /// <summary>
    ///     Hide all windows.
    /// </summary>
    public static void HideWindow()
    {
        Instance.ScrollWindowComponent.clickHide();
    }

    /// <summary>
    ///     Go back to the previous window.
    /// </summary>
    public static void Back()
    {
        Instance.ScrollWindowComponent.clickBack();
    }
}