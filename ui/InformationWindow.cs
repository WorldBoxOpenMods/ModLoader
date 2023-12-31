using NeoModLoader.api;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui;

/// <summary>
///     A window that shows simple text.
/// </summary>
public class InformationWindow : AbstractWindow<InformationWindow>
{
    private Text text;

    /// <inheritdoc cref="InformationWindow.Init" />
    protected override void Init()
    {
        text = new GameObject("Text", typeof(Text)).GetComponent<Text>();
        OT.InitializeCommonText(text);
        RectTransform transform1;
        (transform1 = (RectTransform)text.transform).SetParent(ContentTransform);
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 10;
        text.resizeTextMaxSize = 14;
        text.alignment = TextAnchor.MiddleCenter;

        ContentTransform.gameObject.AddComponent<VerticalLayoutGroup>();
    }

    /// <summary>
    ///     Show the window with the given text.
    /// </summary>
    /// <param name="info"></param>
    public static void ShowWindow(string info)
    {
        Instance.text.text = info;
        ScrollWindow.showWindow(WindowId);
    }
}