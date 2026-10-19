using NeoModLoader.api;
using NeoModLoader.constants;
using NeoModLoader.General;
using NeoModLoader.General.UI;
using NeoModLoader.General.UI.Prefabs;
using NeoModLoader.services;
using NeoModLoader.ui.prefabs;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui;

/// <summary>
///     The description of one installed mod, with a notice when it has a problem.
/// </summary>
internal class ModInfoWindow : AbstractWindow<ModInfoWindow>
{
    private const float header_height = 48;

    // Unity's Text cannot draw much more than 16k characters.
    private const int max_description_length = 8000;

    private ModDeclare mod;
    private ulong workshop_id;

    private Image icon;
    private Text title;
    private Text author;
    private Text notice;
    private Text description;
    private SimpleButton workshop_button;
    private SimpleButton unsubscribe_button;

    public static void ShowWindow(ModDeclare pMod)
    {
        Instance.mod = pMod;
        ScrollWindow.showWindow(WindowId);
    }

    protected override void Init()
    {
        ModHubFrame frame = ModHubFrame.Create(BackgroundTransform, true);
        CreateHeader(frame.Header.transform);
        CreateBody();
        CreateButtons(frame.Footer);
    }

    public override void OnNormalEnable()
    {
        ShowMod();
        GetComponent<ScrollWindow>().resetScroll();
    }

    private void ShowMod()
    {
        InstalledMod installed = ModManagerService.Describe(mod);
        bool from_workshop = InstalledMod.TryGetWorkshopId(mod.FolderPath, CoreConstants.GameId, out workshop_id);

        icon.sprite = ModManagerService.GetIcon(mod);
        title.text = $"{installed.Name}  {ModHubUI.FormatVersion(mod.Version)}".TrimEnd();
        author.text = string.Format(LM.Get("mod_hub_by_author"), installed.Author);
        string fail_reason = mod.FailReason.ToString().Trim();
        notice.gameObject.SetActive(installed.HasProblem && fail_reason.Length > 0);
        notice.text = fail_reason;
        description.text = BuildDescription();
        if (from_workshop) _ = ShowWorkshopDescriptionAsync(mod, workshop_id);

        workshop_button.gameObject.SetActive(from_workshop);
        unsubscribe_button.gameObject.SetActive(from_workshop && !ModDownloadService.NeedsRestart(mod.UID) &&
                                                ModDownloadService.CheckSteam() == CatalogStatus.Ready);
    }

    private string BuildDescription()
    {
        string text = mod.GetDisplayDesc()?.Trim();
        return string.IsNullOrEmpty(text) ? LM.Get("mod_hub_info_no_description") : Truncate(text);
    }

    /// <summary>
    ///     Replace the mod.json description with the Workshop page's once Steam sends it.
    /// </summary>
    private async Task ShowWorkshopDescriptionAsync(ModDeclare pMod, ulong pFileId)
    {
        string text = (await ModDownloadService.GetDescriptionAsync(pFileId))?.Trim();
        if (!string.IsNullOrEmpty(text) && isActiveAndEnabled && mod == pMod) description.text = Truncate(text);
    }

    private static string Truncate(string pText)
    {
        return pText.Length > max_description_length ? pText.Substring(0, max_description_length) + "..." : pText;
    }

    private void ConfirmUnsubscribe()
    {
        ModDeclare target = mod;
        string message = string.Format(LM.Get("mod_hub_unsubscribe_confirm"), target.GetDisplayName());
        ConfirmWindow.ShowWindow(message, () => _ = UnsubscribeAsync(target));
    }

    private async Task UnsubscribeAsync(ModDeclare pMod)
    {
        if (!await ModDownloadService.UnsubscribeAsync(pMod))
        {
            InformationWindow.ShowWindow(LM.Get("mod_hub_unsubscribe_failed"));
            return;
        }

        if (IsOpened && mod == pMod) ShowMod();
    }

    private void CreateHeader(Transform pHeader)
    {
        pHeader.gameObject.SetLayoutSize(pHeight: header_height);
        var image = new GameObject("Icon", typeof(Image));
        image.transform.SetParent(pHeader, false);
        icon = image.GetComponent<Image>();
        icon.preserveAspect = true;
        image.SetLayoutSize(header_height, header_height);

        var texts = new GameObject("Texts", typeof(RectTransform));
        texts.transform.SetParent(pHeader, false);
        var layout = texts.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 2;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        texts.SetLayoutSize(pFlexibleWidth: 1);
        title = OT.CreateText(texts.transform, "Title", 10);
        title.gameObject.SetLayoutSize(pHeight: 28);
        author = OT.CreateText(texts.transform, "Author", 7, pColor: Color.gray);
        author.gameObject.SetLayoutSize(pHeight: 16);
    }

    private void CreateBody()
    {
        notice = Paragraph("Notice", new Color(1f, 0.72f, 0.25f));
        description = Paragraph("Description", Color.white);
    }

    private Text Paragraph(string pName, Color pColor)
    {
        Text text = OT.CreateText(ContentTransform, pName, 8, TextAnchor.UpperLeft, pColor);
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private void CreateButtons(ModHubToolbar pFooter)
    {
        workshop_button = pFooter.AddText("WorkshopPage", "mod_hub_workshop_page",
            () => Application.OpenURL($"steam://url/CommunityFilePage/{workshop_id}"));
        pFooter.AddText("OpenFolder", "mod_hub_open_folder",
            () => ModManagerService.OpenFolder(mod.FolderPath));
        unsubscribe_button = pFooter.AddText("Unsubscribe", "mod_hub_unsubscribe",
            ConfirmUnsubscribe, true);
    }
}
