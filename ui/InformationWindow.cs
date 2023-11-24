using NeoModLoader.api;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui;

public class InformationWindow : AbstractWindow<InformationWindow>
{
    private Text text;
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

    public static void ShowWindow(string info)
    {
        Instance.text.text = info;
        ScrollWindow.showWindow(WindowId);
    }
}