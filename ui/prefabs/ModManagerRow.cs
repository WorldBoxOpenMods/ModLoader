using System.Collections;
using NeoModLoader.api;
using NeoModLoader.General;
using NeoModLoader.General.UI;
using NeoModLoader.General.UI.Prefabs;
using NeoModLoader.services;
using NeoModLoader.utils;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui.prefabs;

/// <summary>
///     One mod in <see cref="ModManagerPage" />: icon, title with the author on hover, a status pill that
///     toggles the mod, and its buttons.
/// </summary>
/// <remarks>
///     <see cref="CreatePrefab" /> builds the visuals only. Click and hover handlers are added in
///     <see cref="Awake" />, because Unity does not copy handlers added in code when the pool clones the prefab.
/// </remarks>
internal class ModManagerRow : ModHubRow<InstalledMod>
{
    private const int upload_click_count = 8;
    private const int max_reason_length = 600;

    [SerializeField] private Button icon_button;
    [SerializeField] private Text version;
    [SerializeField] private SimpleButton status;
    [SerializeField] private SimpleButton source;
    [SerializeField] private SimpleButton info;
    [SerializeField] private SimpleButton configure;
    [SerializeField] private SimpleButton reload;
    [SerializeField] private SimpleButton delete;

    private InstalledMod mod;
    private ModConfig config;
    private int icon_clicks;
    private float last_icon_click;

    private void Awake()
    {
        icon_button.onClick.AddListener(CountIconClick);
        status.Button.onClick.AddListener(Toggle);
        info.Button.onClick.AddListener(() => ModInfoWindow.ShowWindow(mod.Declaration));
        configure.Button.onClick.AddListener(() => ModConfigureWindow.ShowWindow(config));
        reload.Button.onClick.AddListener(Reload);
        delete.Button.onClick.AddListener(ConfirmDelete);
        Title.GetComponent<Button>().onClick.AddListener(() => ModInfoWindow.ShowWindow(mod.Declaration));
        Title.GetComponent<TipButton>().SetTextTooltip(() => mod.Name,
            () => $"{string.Format(LM.Get("mod_hub_by_author"), mod.Author)}\n{version.text}");
        status.TipButton.SetTextTooltip(() => status.Text.text, StatusTooltip);
        source.TipButton.SetTextTooltip(() => string.Format(LM.Get("mod_hub_loaded_from"),
            LM.Get("mod_hub_source_" + mod.Source.ToString().ToLowerInvariant())), () => null);
    }

    public override void Setup(InstalledMod pMod)
    {
        mod = pMod;
        icon_clicks = 0;
        ModDeclare declaration = pMod.Declaration;

        Icon.sprite = ModManagerService.GetIcon(declaration);
        Title.text = pMod.Name;
        version.text = declaration.GetDisplayVersion();
        version.gameObject.SetActive(version.text.Length > 0);
        status.Text.text = LM.Get(StatusKey(pMod.Status));
        status.Text.color = StatusColor(pMod.Status);
        source.Icon.sprite = SourceIcon(pMod.Source);

        config = ModManagerService.GetConfig(declaration);
        configure.gameObject.SetActive(config != null);
        reload.gameObject.SetActive(ModManagerService.CanReload(declaration));
        delete.gameObject.SetActive(ModManagerService.CanDelete(pMod));
    }

    private void Toggle()
    {
        ModManagerService.Toggle(mod.Declaration);
        ModManagerPage.Instance.Refresh();
    }

    private void Reload()
    {
        if (!ModReloadService.ReloadMod(mod.Declaration))
        {
            LogService.LogWarning($"Failed to reload mod {mod.Declaration.Name}.");
        }
    }

    private void ConfirmDelete()
    {
        InstalledMod target = mod;
        bool workshop = target.Source == ModSource.Workshop;
        string message = string.Format(LM.Get(workshop ? "mod_hub_unsubscribe_confirm" : "mod_hub_delete_confirm"),
            target.Name);
        ConfirmWindow.ShowWindow(message, () => _ = DeleteAsync(target, workshop));
    }

    private static async Task DeleteAsync(InstalledMod pMod, bool pWorkshop)
    {
        if (!await ModManagerService.DeleteAsync(pMod))
        {
            InformationWindow.ShowWindow(LM.Get(pWorkshop ? "mod_hub_unsubscribe_failed" : "mod_hub_delete_failed"));
            return;
        }

        ModManagerPage.Instance.Refresh();
    }

    /// <summary>
    ///     Clicking a loaded mod's icon 8 times opens the upload window, as in the previous mod list.
    /// </summary>
    private void CountIconClick()
    {
        if (!WorldBoxMod.TryGetLoadedMod(mod.Declaration, out IMod loaded_mod)) return;

        if (Time.time - last_icon_click > 1) icon_clicks = 0;
        last_icon_click = Time.time;
        icon_clicks++;
        if (icon_clicks == upload_click_count) StartCoroutine(OpenUploadWindowIfClicksStop(loaded_mod));
    }

    private IEnumerator OpenUploadWindowIfClicksStop(IMod pMod)
    {
        yield return new WaitForSeconds(3f);
        if (icon_clicks == upload_click_count && mod.Declaration == pMod.GetDeclaration())
        {
            ModUploadModeWindow.ShowWindow(pMod);
        }
    }

    private string StatusTooltip()
    {
        string click_hint = LM.Get(mod.Enabled ? "mod_hub_click_to_disable" : "mod_hub_click_to_enable");
        return $"{LM.Get(StatusKey(mod.Status) + "_tip")}{StatusDetails()}\n\n{click_hint}";
    }

    private string StatusDetails()
    {
        switch (mod.Status)
        {
            case InstalledModStatus.MissingDependency:
                IEnumerable<string> missing = mod.Declaration.Dependencies.Where(uid =>
                    ModManagerService.GetDependencyState(uid) == DependencyState.Missing);
                return "\n" + string.Join(", ", missing);
            case InstalledModStatus.Failed:
                string reason = mod.Declaration.FailReason.ToString().Trim();
                if (reason.Length > max_reason_length) reason = reason.Substring(0, max_reason_length) + "...";
                return reason.Length == 0 ? "" : "\n\n" + reason;
            default:
                return "";
        }
    }

    private static string StatusKey(InstalledModStatus pStatus)
    {
        return "mod_hub_status_" + pStatus.ToString().ToLowerInvariant();
    }

    private static Color StatusColor(InstalledModStatus pStatus)
    {
        return pStatus switch
        {
            InstalledModStatus.Enabled => new Color(0.45f, 0.9f, 0.45f),
            InstalledModStatus.Disabled or InstalledModStatus.Failed => new Color(1f, 0.42f, 0.36f),
            _ => new Color(1f, 0.72f, 0.25f)
        };
    }

    private static Sprite SourceIcon(ModSource pSource)
    {
        return pSource switch
        {
            ModSource.Workshop => SpriteTextureLoader.getSprite("ui/icons/iconSteam"),
            ModSource.BepInEx => SpriteTextureLoader.getSprite("ui/icons/iconOptions"),
            _ => SpriteTextureLoader.getSprite("ui/icons/iconSaveLocal")
        };
    }

    /// <summary>
    ///     Build the row prefab that the pool of <see cref="AbstractListWindow{T,TItem}" /> clones.
    /// </summary>
    internal static ModManagerRow CreatePrefab()
    {
        ModManagerRow row = CreateRow<ModManagerRow>();
        row.icon_button = row.Icon.gameObject.AddComponent<Button>();
        row.version = OT.CreateText(row.TitleLine, "Version", 7, TextAnchor.MiddleRight, Color.gray);
        row.version.gameObject.SetLayoutSize(34, 18);
        row.source = row.AddAction("Source", null, null);
        row.source.transform.SetParent(row.TitleLine, false);
        row.source.Background.enabled = false;
        row.source.gameObject.SetLayoutSize(10, 10);
        CreateControlLine(row, row.Controls);
        return row;
    }

    private static void CreateControlLine(ModManagerRow pRow, Transform pBody)
    {
        pRow.status = SimpleButton.Instantiate(pBody, pName: "Status");
        pRow.status.Setup(null, null, " ", new Vector2(72, 16));
        pRow.status.Background.sprite = SpriteTextureLoader.getSprite("ui/special/darkInputFieldEmpty");
        pRow.status.Text.resizeTextForBestFit = false;
        pRow.status.Text.fontSize = 8;
        pRow.status.Text.horizontalOverflow = HorizontalWrapMode.Overflow;
        pRow.status.Text.supportRichText = false;
        pRow.status.Text.rectTransform.Stretch(2);
        pRow.status.gameObject.SetLayoutSize(72, 16);
        pRow.status.gameObject.AddComponent<ScrollableButton>();

        var spacer = new GameObject("Spacer", typeof(RectTransform));
        spacer.transform.SetParent(pBody, false);
        spacer.SetLayoutSize(pFlexibleWidth: 1);
        pRow.configure = pRow.AddAction("Configure", SpriteTextureLoader.getSprite("ui/icons/iconOptions"),
            "mod_hub_configure");
        pRow.reload = pRow.AddAction("Reload", InternalResourcesGetter.GetReloadIcon(),
            "mod_hub_reload");
        pRow.delete = pRow.AddAction("Delete", SpriteTextureLoader.getSprite("ui/icons/iconDeleteWorld"),
            "mod_hub_delete");
        pRow.info = pRow.AddAction("Info", SpriteTextureLoader.getSprite("ui/icons/iconQuestionMark"),
            "mod_hub_info");
    }
}
