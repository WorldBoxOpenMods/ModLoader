using System;
using NeoModLoader.api;
using NeoModLoader.General;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui;

internal class AIConfirmWindow : AbstractWindow<AIConfirmWindow>
{
    private Action onConfirmAction;
    private Text messageText;

    protected override void Init()
    {
        ContentTransform.gameObject.AddComponent<ContentSizeFitter>().verticalFit =
            ContentSizeFitter.FitMode.PreferredSize;
        VerticalLayoutGroup layout = ContentTransform.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlHeight = false;
        layout.childControlWidth = false;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;
        layout.spacing = 10;
        layout.padding = new RectOffset(10, 10, 10, 10);

        ScrollWindow scrollWindow = GetComponent<ScrollWindow>();
        if (scrollWindow != null && scrollWindow.titleText != null)
        {
            scrollWindow.titleText.text = LM.Get("AIConfirmWindow Title");
        }

        messageText = new GameObject("Message", typeof(Text)).GetComponent<Text>();
        messageText.transform.SetParent(ContentTransform);
        messageText.transform.localScale = Vector3.one;
        messageText.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 100);
        OT.InitializeCommonText(messageText);
        messageText.alignment = TextAnchor.MiddleCenter;
        messageText.resizeTextForBestFit = true;
        messageText.resizeTextMinSize = 8;
        messageText.resizeTextMaxSize = 12;

        GameObject buttonGroup = new GameObject("Buttons", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        buttonGroup.transform.SetParent(ContentTransform);
        buttonGroup.transform.localScale = Vector3.one;
        buttonGroup.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 30);

        HorizontalLayoutGroup hLayout = buttonGroup.GetComponent<HorizontalLayoutGroup>();
        hLayout.childAlignment = TextAnchor.MiddleCenter;
        hLayout.spacing = 10;

        GameObject backButtonObj = new GameObject("BackButton", typeof(Image), typeof(Button));
        backButtonObj.transform.SetParent(buttonGroup.transform);
        backButtonObj.transform.localScale = Vector3.one;
        backButtonObj.GetComponent<RectTransform>().sizeDelta = new Vector2(85, 25);
        Image backBg = backButtonObj.GetComponent<Image>();
        backBg.sprite = SpriteTextureLoader.getSprite("ui/special/special_buttonred");
        backBg.type = Image.Type.Sliced;

        Text backText = new GameObject("Text", typeof(Text)).GetComponent<Text>();
        backText.transform.SetParent(backButtonObj.transform);
        backText.transform.localScale = Vector3.one;
        backText.GetComponent<RectTransform>().sizeDelta = new Vector2(85, 25);
        OT.InitializeCommonText(backText);
        backText.alignment = TextAnchor.MiddleCenter;
        backText.text = "Back";

        backButtonObj.GetComponent<Button>().onClick.AddListener(() =>
        {
            GetComponent<ScrollWindow>().clickHide();
        });

        GameObject continueButtonObj = new GameObject("ContinueButton", typeof(Image), typeof(Button));
        continueButtonObj.transform.SetParent(buttonGroup.transform);
        continueButtonObj.transform.localScale = Vector3.one;
        continueButtonObj.GetComponent<RectTransform>().sizeDelta = new Vector2(85, 25);
        Image continueBg = continueButtonObj.GetComponent<Image>();
        continueBg.sprite = SpriteTextureLoader.getSprite("ui/special/button2");
        continueBg.type = Image.Type.Sliced;

        Text continueText = new GameObject("Text", typeof(Text)).GetComponent<Text>();
        continueText.transform.SetParent(continueButtonObj.transform);
        continueText.transform.localScale = Vector3.one;
        continueText.GetComponent<RectTransform>().sizeDelta = new Vector2(85, 25);
        OT.InitializeCommonText(continueText);
        continueText.alignment = TextAnchor.MiddleCenter;
        continueText.text = "Continue";

        continueButtonObj.GetComponent<Button>().onClick.AddListener(() =>
        {
            ScrollWindow.moveAllToLeftAndRemove(false);
            onConfirmAction?.Invoke();
        });
    }

    public static void ShowWindow(string pMessage, Action pOnConfirm)
    {
        Instance.messageText.text = pMessage;
        Instance.onConfirmAction = pOnConfirm;
        ScrollWindow.showWindow(WindowId);
    }
}
