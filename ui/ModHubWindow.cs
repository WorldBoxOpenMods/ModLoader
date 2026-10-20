using NeoModLoader.api;
using NeoModLoader.utils;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui;

/// <summary>One native window containing the installed-mod and Workshop pages.</summary>
internal class ModHubWindow : AbstractWindow<ModHubWindow>
{
    private ScrollWindow window;
    private WindowMetaTabButtonsContainer tabs;

    protected override void Init()
    {
        window = GetComponent<ScrollWindow>();
        var original_content = window.transform_content;
        original_content.gameObject.SetActive(false);

        ModManagerPage manager = ModManagerPage.Create(BackgroundTransform, "ModManager", window.scrollRect);
        ModBrowserPage browser = ModBrowserPage.Create(BackgroundTransform, "ModBrowser", window.scrollRect);
        manager.Frame.Activate(window);
        ContentTransform = manager.Frame.Content;
        Destroy(original_content.gameObject);

        Transform tab_parent = BackgroundTransform.Find("Tabs");
        // Rebuild the empty prefab's container so it discovers these pages on its first enable.
        DestroyImmediate(tab_parent.GetComponent<WindowMetaTabButtonsContainer>());
        tabs = tab_parent.gameObject.AddComponent<WindowMetaTabButtonsContainer>();
        window.tabs = tabs;
        tabs.tab_default = CreateTab("ModManager", InternalResourcesGetter.GetIcon(), manager.transform);
        CreateTab("ModBrowser", SpriteTextureLoader.getSprite("ui/icons/iconSteam"), browser.transform);
        tabs.addTabShowCallback(tab =>
            window.titleText.GetComponent<LocalizedText>().setKeyAndUpdate(tab.name + " Title"));
    }

    private WindowMetaTab CreateTab(string pId, Sprite pIcon, Transform pContent)
    {
        var obj = new GameObject(pId, typeof(Image), typeof(Button), typeof(TipButton), typeof(CanvasGroup));
        obj.SetActive(false);
        obj.transform.SetParent(tabs.transform, false);
        Image background = obj.GetComponent<Image>();
        background.sprite = SpriteTextureLoader.getSprite("ui/tab_button_vertical");
        background.type = Image.Type.Sliced;
        TipButton tip = obj.GetComponent<TipButton>();
        tip.textOnClick = pId + " Title";
        tip.textOnClickDescription = pId + " Description";
        tip.override_click_scale_animation = true;
        tip.overridden_click_scale_animation = 1;

        var image = new GameObject("Icon", typeof(Image));
        image.transform.SetParent(obj.transform, false);
        image.GetComponent<Image>().sprite = pIcon;
        image.GetComponent<Image>().preserveAspect = true;
        image.GetComponent<Image>().raycastTarget = false;
        image.GetComponent<RectTransform>().sizeDelta = new Vector2(18, 18);

        var tab = obj.AddComponent<WindowMetaTab>();
        tab._canvas_group = obj.GetComponent<CanvasGroup>();
        tab.tab_action = new WindowMetaTabEvent();
        // The native container invokes this action again after hiding all pages on close.
        // Only skip a repeated click while the selected page is still shown.
        tab.tab_action.AddListener(selected => tabs.startTabAction(selected, pContent.gameObject.activeSelf));
        tab.tab_elements.Add(pContent);
        obj.SetActive(true);
        return tab;
    }
}
