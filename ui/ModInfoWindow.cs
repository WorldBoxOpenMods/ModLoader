using NeoModLoader.api;
using NeoModLoader.constants;
using NeoModLoader.General;
using NeoModLoader.General.UI.Prefabs;
using NeoModLoader.services;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui;

/// <summary>
///     The description of one installed mod, with a notice when it has a problem.
/// </summary>
internal class ModInfoWindow : AbstractWindow<ModInfoWindow>
{
    private const float header_height = 32;

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
        ModHubStyle.SetUpListWindow(BackgroundTransform, out Transform header, out Transform footer);
        CreateHeader(header);
        CreateBody();
        CreateButtons(footer);
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

        workshop_button.gameObject.SetActive(from_workshop);
        unsubscribe_button.gameObject.SetActive(from_workshop && !ModDownloadService.NeedsRestart(mod.UID) &&
                                                ModDownloadService.CheckSteam() == CatalogStatus.Ready);
    }

    private string BuildDescription()
    {
        string text = mod.GetDisplayDesc()?.Trim();
        if (string.IsNullOrEmpty(text)) return LM.Get("mod_hub_info_no_description");
        return text.Length > max_description_length ? text.Substring(0, max_description_length) + "..." : text;
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
        ModHubStyle.Size(pHeader.gameObject, pHeight: header_height);
        icon = ModHubStyle.Icon(pHeader, "Icon", null, header_height);

        var texts = new GameObject("Texts", typeof(RectTransform));
        texts.transform.SetParent(pHeader, false);
        ModHubStyle.Vertical(texts, 2).childAlignment = TextAnchor.MiddleLeft;
        ModHubStyle.Size(texts, pFlexibleWidth: 1);
        title = ModHubStyle.Label(texts.transform, "Title", ModHubStyle.FontTitle, ModHubStyle.TextBright);
        author = ModHubStyle.Label(texts.transform, "Author", ModHubStyle.FontSmall, ModHubStyle.TextMuted);
    }

    private void CreateBody()
    {
        ModHubStyle.Vertical(ContentTransform.gameObject, ModHubStyle.SpaceM).childForceExpandWidth = true;
        ContentTransform.gameObject.AddComponent<ContentSizeFitter>().verticalFit =
            ContentSizeFitter.FitMode.PreferredSize;
        notice = Paragraph("Notice", ModHubStyle.Warning);
        description = Paragraph("Description", ModHubStyle.TextBright);
    }

    private Text Paragraph(string pName, Color pColor)
    {
        Text text = ModHubStyle.Label(ContentTransform, pName, ModHubStyle.FontBody, pColor, TextAnchor.UpperLeft);
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private void CreateButtons(Transform pFooter)
    {
        workshop_button = ModHubStyle.TextButton(pFooter, "WorkshopPage", "mod_hub_workshop_page",
            () => Application.OpenURL($"steam://url/CommunityFilePage/{workshop_id}"));
        ModHubStyle.TextButton(pFooter, "OpenFolder", "mod_hub_open_folder",
            () => ModManagerService.OpenFolder(mod.FolderPath));
        unsubscribe_button = ModHubStyle.TextButton(pFooter, "Unsubscribe", "mod_hub_unsubscribe",
            ConfirmUnsubscribe, true);
    }
}
