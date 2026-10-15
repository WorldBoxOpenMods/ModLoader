using NeoModLoader.api;
using NeoModLoader.constants;
using NeoModLoader.General;
using NeoModLoader.General.UI.Prefabs;
using NeoModLoader.utils;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui;

/// <summary>
///     Menu opened by NML's power button. Each entry opens one mod screen.
/// </summary>
internal class ModHubWindow : AbstractWindow<ModHubWindow>
{
    private static readonly Vector2 entry_size = new(200, 50);

    /// <summary>
    ///     One entry per window. Its title and description are the locale keys "{id} Title" and "{id} Description".
    /// </summary>
    private static readonly (string WindowId, Func<Sprite> Icon)[] entries =
    {
        ("ModManager", InternalResourcesGetter.GetIcon),
        ("WorkshopMods", () => ModHubUI.GameIcon("iconSteam"))
    };

    protected override void Init()
    {
        BackgroundTransform.Find("Scroll View").gameObject.SetActive(false);
        for (int i = 0; i < entries.Length; i++)
        {
            CreateEntry(entries[i].WindowId, entries[i].Icon(), new Vector2(0, 95 - i * (entry_size.y + 8)));
        }

        CreateVersionFooter();
    }

    private void CreateEntry(string pWindowId, Sprite pIcon, Vector2 pPosition)
    {
        var entry = new GameObject(pWindowId, typeof(Image), typeof(Button));
        entry.transform.SetParent(BackgroundTransform, false);
        entry.transform.localPosition = pPosition;
        entry.GetComponent<RectTransform>().sizeDelta = entry_size;
        Image background = entry.GetComponent<Image>();
        background.sprite = SpriteTextureLoader.getSprite("ui/special/windowInnerSliced");
        background.type = Image.Type.Sliced;
        entry.GetComponent<Button>().onClick.AddListener(() => ScrollWindow.showWindow(pWindowId));

        var icon = new GameObject("Icon", typeof(Image));
        icon.transform.SetParent(entry.transform, false);
        icon.transform.localPosition = new Vector3(-75, 0);
        icon.GetComponent<RectTransform>().sizeDelta = new Vector2(36, 36);
        icon.GetComponent<Image>().sprite = pIcon;

        Text title = ModHubUI.CreateText(entry.transform, "Title", new Vector2(140, 16), new Vector2(23, 10), 10,
            TextAnchor.MiddleLeft);
        ModHubUI.SetLocaleKey(title, pWindowId + " Title");
        Text description = ModHubUI.CreateText(entry.transform, "Description", new Vector2(140, 22),
            new Vector2(23, -9), 6, TextAnchor.UpperLeft);
        description.color = ModHubUI.MutedTextColor;
        ModHubUI.SetLocaleKey(description, pWindowId + " Description");
    }

    /// <summary>
    ///     NML's version and commit, like the old mod list. Clicking opens NML's repository to report bugs.
    /// </summary>
    private void CreateVersionFooter()
    {
        string version = "NeoModLoader " + WorldBoxMod.NeoModLoaderAssembly.GetName().Version;
        SimpleButton button = ModHubUI.CreateIconButton(BackgroundTransform, "NeoModLoader",
            InternalResourcesGetter.GetIcon(), new Vector2(18, 18), new Vector2(-82, -118),
            () => Application.OpenURL(CoreConstants.RepoURL), pTooltipKey: null);
        button.Background.enabled = false;
        ModHubUI.SetTextTooltip(button, () => version,
            () => $"commit {InternalResourcesGetter.GetCommit()}\n{LM.Get("NeoModLoader Report")}");

        Text label = ModHubUI.CreateText(BackgroundTransform, "Version", new Vector2(160, 14), new Vector2(12, -118),
            7, TextAnchor.MiddleLeft);
        label.color = ModHubUI.MutedTextColor;
        label.text = version;
    }
}
