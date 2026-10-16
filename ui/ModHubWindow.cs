using NeoModLoader.api;
using NeoModLoader.constants;
using NeoModLoader.General;
using NeoModLoader.utils;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui;

/// <summary>
///     Menu opened by NML's power button. Each entry opens one mod screen.
/// </summary>
internal class ModHubWindow : AbstractWindow<ModHubWindow>
{
    private const float card_height = 44;
    private const float footer_height = 14;

    /// <summary>
    ///     One entry per window. Its title and description are the locale keys "{id} Title" and "{id} Description".
    /// </summary>
    private static readonly (string WindowId, Func<Sprite> Icon)[] entries =
    {
        ("ModManager", InternalResourcesGetter.GetIcon),
        ("ModBrowser", () => ModHubUI.GameIcon("iconSteam"))
    };

    protected override void Init()
    {
        float content_height = entries.Length * (card_height + ModHubStyle.SpaceM) + footer_height;
        RectTransform content = ModHubStyle.SetUpWindow(BackgroundTransform, content_height, ModHubStyle.SpaceS);
        ModHubStyle.Vertical(content.gameObject, ModHubStyle.SpaceM);

        foreach ((string window_id, Func<Sprite> icon) in entries) CreateCard(content, window_id, icon());
        CreateVersionFooter(content);
    }

    private static void CreateCard(Transform pParent, string pWindowId, Sprite pIcon)
    {
        GameObject card = ModHubStyle.Box(pParent, pWindowId, ModHubStyle.Card);
        ModHubStyle.Clickable(card, () => ScrollWindow.showWindow(pWindowId));
        ModHubStyle.Size(card, pHeight: card_height);
        ModHubStyle.Horizontal(card, ModHubStyle.SpaceM, ModHubStyle.SpaceM);
        ModHubStyle.Icon(card.transform, "Icon", pIcon, 28);

        var texts = new GameObject("Texts", typeof(RectTransform));
        texts.transform.SetParent(card.transform, false);
        VerticalLayoutGroup text_layout = ModHubStyle.Vertical(texts, 2);
        text_layout.childAlignment = TextAnchor.MiddleLeft;
        text_layout.childForceExpandWidth = true;
        ModHubStyle.Size(texts, pFlexibleWidth: 1);

        Text title = ModHubStyle.Label(texts.transform, "Title", ModHubStyle.FontTitle, ModHubStyle.TextBright);
        ModHubUI.SetLocaleKey(title, pWindowId + " Title");
        ModHubStyle.Divider(texts.transform);
        Text description = ModHubStyle.Label(texts.transform, "Description", ModHubStyle.FontSmall,
            ModHubStyle.TextMuted);
        ModHubUI.SetLocaleKey(description, pWindowId + " Description");
    }

    /// <summary>
    ///     NML's version; hovering shows the commit, clicking opens NML's repository to report bugs.
    /// </summary>
    private static void CreateVersionFooter(Transform pParent)
    {
        string version = "NeoModLoader " + WorldBoxMod.NeoModLoaderAssembly.GetName().Version;
        var footer = new GameObject("Version", typeof(Image), typeof(Button), typeof(TipButton));
        footer.transform.SetParent(pParent, false);
        footer.GetComponent<Image>().color = Color.clear;
        footer.GetComponent<Button>().onClick.AddListener(() => Application.OpenURL(CoreConstants.RepoURL));
        footer.GetComponent<TipButton>().setHoverAction(() => ModHubUI.ShowTextTooltip(footer, version,
            $"commit {InternalResourcesGetter.GetCommit()}\n{LM.Get("NeoModLoader Report")}"));
        ModHubStyle.Size(footer, pHeight: footer_height);
        ModHubStyle.Horizontal(footer, ModHubStyle.SpaceS);

        ModHubStyle.Icon(footer.transform, "Icon", InternalResourcesGetter.GetIcon(), 12);
        ModHubStyle.Label(footer.transform, "Text", ModHubStyle.FontSmall, ModHubStyle.TextMuted).text = version;
    }
}
