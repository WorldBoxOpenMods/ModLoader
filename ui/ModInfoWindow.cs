using System.Text;
using NeoModLoader.api;
using NeoModLoader.constants;
using NeoModLoader.General;
using NeoModLoader.General.UI.Prefabs;
using NeoModLoader.services;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui;

/// <summary>
///     Details of one installed mod: description, state, source, tags and dependencies.
/// </summary>
internal class ModInfoWindow : AbstractWindow<ModInfoWindow>
{
    private const float content_height = 254;

    // Unity's Text cannot draw much more than 16k characters.
    private const int max_description_length = 8000;

    private ModDeclare mod;
    private ModConfig config;
    private ulong workshop_id;

    private Image icon;
    private Text title;
    private Text author;
    private Text details;
    private Text description;
    private SimpleButton workshop_button;
    private SimpleButton configure_button;

    public static void ShowWindow(ModDeclare pMod)
    {
        Instance.mod = pMod;
        ScrollWindow.showWindow(WindowId);
    }

    protected override void Init()
    {
        Transform scroll_view = BackgroundTransform.Find("Scroll View");
        scroll_view.GetComponent<RectTransform>().sizeDelta = new Vector2(232, content_height);
        scroll_view.localPosition = new Vector3(0, 2);
        scroll_view.Find("Viewport").localPosition = new Vector3(-131, content_height / 2);

        var layout = ContentTransform.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.spacing = 6;
        layout.padding = new RectOffset(30, 30, 8, 8);
        ContentTransform.gameObject.AddComponent<ContentSizeFitter>().verticalFit =
            ContentSizeFitter.FitMode.PreferredSize;

        CreateHeader();
        details = CreateParagraph("Details", 7);
        description = CreateParagraph("Description", 7);
        CreateButtons();
    }

    public override void OnNormalEnable()
    {
        InstalledMod installed = ModManagerService.Describe(mod);
        config = ModManagerService.GetConfig(mod);
        bool from_workshop = InstalledMod.TryGetWorkshopId(mod.FolderPath, CoreConstants.GameId, out workshop_id);

        icon.sprite = ModManagerService.GetIcon(mod);
        title.text = string.IsNullOrEmpty(mod.Version) ? installed.Name : $"{installed.Name}  {mod.Version}";
        author.text = installed.Author;
        details.text = BuildDetails(installed);
        description.text = BuildDescription();

        workshop_button.gameObject.SetActive(from_workshop);
        configure_button.gameObject.SetActive(config != null);
        GetComponent<ScrollWindow>().resetScroll();
    }

    private string BuildDetails(InstalledMod pMod)
    {
        var text = new StringBuilder();
        text.AppendLine(string.Format(LM.Get("mod_hub_info_status"), LM.Get(ModHubUI.LocaleKey("mod_hub_status", pMod.Status))));
        string fail_reason = mod.FailReason.ToString().Trim();
        if (pMod.HasProblem && fail_reason.Length > 0) text.AppendLine(fail_reason);

        text.AppendLine(string.Format(LM.Get("mod_hub_info_source"), LM.Get(ModHubUI.LocaleKey("mod_hub_source", pMod.Source))));
        text.AppendLine(string.Format(LM.Get("mod_hub_info_folder"), mod.FolderPath));
        if (mod.Tags.Count > 0) text.AppendLine(string.Format(LM.Get("mod_hub_info_tags"), string.Join(", ", mod.Tags)));

        AppendDependencies(text, "mod_hub_info_dependencies", mod.Dependencies);
        AppendDependencies(text, "mod_hub_info_optional_dependencies", mod.OptionalDependencies);
        return text.ToString().TrimEnd();
    }

    private static void AppendDependencies(StringBuilder pText, string pTitleKey, string[] pModUids)
    {
        if (pModUids.Length == 0) return;

        pText.AppendLine(LM.Get(pTitleKey));
        foreach (string uid in pModUids)
        {
            string state = LM.Get(ModHubUI.LocaleKey("mod_hub_dependency", ModManagerService.GetDependencyState(uid)));
            pText.AppendLine($"  - {uid} ({state})");
        }
    }

    private string BuildDescription()
    {
        string text = mod.GetDisplayDesc()?.Trim();
        if (string.IsNullOrEmpty(text)) return LM.Get("mod_hub_info_no_description");
        return text.Length > max_description_length ? text.Substring(0, max_description_length) + "..." : text;
    }

    private void OpenWorkshopPage()
    {
        Application.OpenURL($"steam://url/CommunityFilePage/{workshop_id}");
    }

    private void CreateHeader()
    {
        var header = new GameObject("Header", typeof(RectTransform), typeof(LayoutElement));
        header.transform.SetParent(ContentTransform, false);
        header.GetComponent<LayoutElement>().preferredHeight = 46;

        var icon_object = new GameObject("Icon", typeof(Image));
        icon_object.transform.SetParent(header.transform, false);
        icon = icon_object.GetComponent<Image>();
        PlaceInHeader(icon.rectTransform, new Vector2(0, 0), new Vector2(44, 44), false);

        title = ModHubUI.CreateText(header.transform, "Title", Vector2.zero, Vector2.zero, 10, TextAnchor.UpperLeft);
        PlaceInHeader(title.rectTransform, new Vector2(50, -4), new Vector2(-50, 24), true);
        author = ModHubUI.CreateText(header.transform, "Author", Vector2.zero, Vector2.zero, 7, TextAnchor.UpperLeft);
        author.color = ModHubUI.MutedTextColor;
        PlaceInHeader(author.rectTransform, new Vector2(50, -30), new Vector2(-50, 12), true);
    }

    /// <summary>
    ///     Anchor to the header's top-left corner. Stretched elements keep a fixed margin to its right edge.
    /// </summary>
    private static void PlaceInHeader(RectTransform pRect, Vector2 pPosition, Vector2 pSize, bool pStretch)
    {
        pRect.anchorMin = new Vector2(0, 1);
        pRect.anchorMax = new Vector2(pStretch ? 1 : 0, 1);
        pRect.pivot = new Vector2(0, 1);
        pRect.anchoredPosition = pPosition;
        pRect.sizeDelta = pSize;
    }

    private Text CreateParagraph(string pName, int pFontSize)
    {
        Text text = ModHubUI.CreateText(ContentTransform, pName, Vector2.zero, Vector2.zero, pFontSize,
            TextAnchor.UpperLeft);
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private void CreateButtons()
    {
        var bar = new GameObject("Buttons", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        bar.transform.SetParent(BackgroundTransform, false);
        bar.transform.localPosition = new Vector3(0, -138);
        bar.GetComponent<RectTransform>().sizeDelta = new Vector2(232, 16);
        var layout = bar.GetComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.spacing = 4;

        Vector2 size = new(72, 16);
        workshop_button = ModHubUI.CreateTextButton(bar.transform, "WorkshopPage", "mod_hub_workshop_page", size,
            Vector2.zero, OpenWorkshopPage);
        ModHubUI.CreateTextButton(bar.transform, "OpenFolder", "mod_hub_open_folder", size, Vector2.zero,
            () => ModManagerService.OpenFolder(mod.FolderPath));
        configure_button = ModHubUI.CreateTextButton(bar.transform, "Configure", "mod_hub_configure", size,
            Vector2.zero, () => ModConfigureWindow.ShowWindow(config));
    }
}
