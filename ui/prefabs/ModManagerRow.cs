using System.Collections;
using NeoModLoader.api;
using NeoModLoader.General;
using NeoModLoader.General.UI.Prefabs;
using NeoModLoader.services;
using NeoModLoader.utils;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui.prefabs;

/// <summary>
///     One mod in <see cref="ModManagerWindow" />: icon, title with the author on hover, a status pill that
///     toggles the mod, and its buttons.
/// </summary>
/// <remarks>
///     <see cref="CreatePrefab" /> builds the visuals only. Click and hover handlers are added in
///     <see cref="Awake" />, because Unity does not copy handlers added in code when the pool clones the prefab.
/// </remarks>
internal class ModManagerRow : AbstractListWindowItem<InstalledMod>
{
    private const float height = 44;
    private const int upload_click_count = 8;
    private const int max_reason_length = 600;

    [SerializeField] private Image icon;
    [SerializeField] private Button icon_button;
    [SerializeField] private Text title;
    [SerializeField] private SimpleButton status;
    [SerializeField] private SimpleButton source;
    [SerializeField] private SimpleButton info;
    [SerializeField] private SimpleButton configure;
    [SerializeField] private SimpleButton reload;

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
        ModHubUI.SetTextTooltip(title.GetComponent<TipButton>(), () => mod.Name,
            () => string.Format(LM.Get("mod_hub_by_author"), mod.Author));
        ModHubUI.SetTextTooltip(status, () => status.Text.text, StatusTooltip);
        ModHubUI.SetTextTooltip(source, () => LM.Get(ModHubUI.LocaleKey("mod_hub_source", mod.Source)),
            () => mod.Declaration.FolderPath);
    }

    public override void Setup(InstalledMod pMod)
    {
        mod = pMod;
        icon_clicks = 0;
        ModDeclare declaration = pMod.Declaration;

        icon.sprite = ModManagerService.GetIcon(declaration);
        title.text = $"{pMod.Name}  {ModHubUI.FormatVersion(declaration.Version)}".TrimEnd();
        status.Text.text = LM.Get(StatusKey(pMod.Status));
        status.Text.color = StatusColor(pMod.Status);
        source.Icon.sprite = SourceIcon(pMod.Source);

        config = ModManagerService.GetConfig(declaration);
        configure.gameObject.SetActive(config != null);
        reload.gameObject.SetActive(ModManagerService.CanReload(declaration));
    }

    private void Toggle()
    {
        ModManagerService.Toggle(mod.Declaration);
        ModManagerWindow.Instance.Refresh();
    }

    private void Reload()
    {
        if (!ModReloadService.ReloadMod(mod.Declaration))
        {
            LogService.LogWarning($"Failed to reload mod {mod.Declaration.Name}.");
        }
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
        return ModHubUI.LocaleKey("mod_hub_status", pStatus);
    }

    private static Color StatusColor(InstalledModStatus pStatus)
    {
        return pStatus switch
        {
            InstalledModStatus.Enabled => ModHubStyle.Positive,
            InstalledModStatus.Disabled or InstalledModStatus.Failed => ModHubStyle.Negative,
            _ => ModHubStyle.Warning
        };
    }

    private static Sprite SourceIcon(ModSource pSource)
    {
        return pSource switch
        {
            ModSource.Workshop => ModHubUI.GameIcon("iconSteam"),
            ModSource.BepInEx => ModHubUI.GameIcon("iconOptions"),
            _ => ModHubUI.GameIcon("iconSaveLocal")
        };
    }

    /// <summary>
    ///     Build the row prefab that the pool of <see cref="AbstractListWindow{T,TItem}" /> clones.
    /// </summary>
    internal static ModManagerRow CreatePrefab()
    {
        GameObject obj = ModHubStyle.Box(WorldBoxMod.Transform, nameof(ModManagerRow), ModHubStyle.Card);
        obj.SetActive(false);
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(0, height);
        ModHubStyle.Horizontal(obj, ModHubStyle.SpaceM, ModHubStyle.SpaceS).padding.left = ModHubStyle.SpaceM;

        ModManagerRow row = obj.AddComponent<ModManagerRow>();
        row.icon = ModHubStyle.Icon(obj.transform, "Icon", null, 32);
        row.icon_button = row.icon.gameObject.AddComponent<Button>();

        var body = new GameObject("Body", typeof(RectTransform));
        body.transform.SetParent(obj.transform, false);
        ModHubStyle.Vertical(body, 2).childForceExpandWidth = true;
        ModHubStyle.Size(body, pFlexibleWidth: 1);
        CreateTitleLine(row, body.transform);
        ModHubStyle.Divider(body.transform);
        CreateControlLine(row, body.transform);
        return row;
    }

    private static void CreateTitleLine(ModManagerRow pRow, Transform pBody)
    {
        var line = new GameObject("TitleLine", typeof(RectTransform));
        line.transform.SetParent(pBody, false);
        ModHubStyle.Horizontal(line, ModHubStyle.SpaceS);
        ModHubStyle.Size(line, pHeight: 12);

        pRow.title = ModHubStyle.Label(line.transform, "Title", ModHubStyle.FontBody, ModHubStyle.TextBright);
        // Hovering the title shows the author.
        pRow.title.raycastTarget = true;
        pRow.title.gameObject.AddComponent<Button>();
        pRow.title.gameObject.AddComponent<TipButton>().type = "normal";
        ModHubStyle.Size(pRow.title.gameObject, pFlexibleWidth: 1);

        pRow.source = ModHubStyle.IconButton(line.transform, "Source", null, null, null);
        pRow.source.Background.enabled = false;
        ModHubStyle.Size(pRow.source.gameObject, 10, 10);
    }

    private static void CreateControlLine(ModManagerRow pRow, Transform pBody)
    {
        var line = new GameObject("ControlLine", typeof(RectTransform));
        line.transform.SetParent(pBody, false);
        ModHubStyle.Horizontal(line, ModHubStyle.SpaceS);
        ModHubStyle.Size(line, pHeight: ModHubStyle.IconButtonSize);

        pRow.status = SimpleButton.Instantiate(line.transform, pName: "Status");
        pRow.status.Setup(null, null, " ", new Vector2(72, ModHubStyle.IconButtonSize));
        pRow.status.Background.sprite = ModHubStyle.Pill;
        pRow.status.Text.resizeTextForBestFit = false;
        pRow.status.Text.fontSize = ModHubStyle.FontBody;
        pRow.status.Text.horizontalOverflow = HorizontalWrapMode.Overflow;
        ModHubStyle.Size(pRow.status.gameObject, 72, ModHubStyle.IconButtonSize);

        ModHubStyle.Spacer(line.transform);
        pRow.configure = ModHubStyle.IconButton(line.transform, "Configure", ModHubUI.GameIcon("iconOptions"), null,
            "mod_hub_configure");
        pRow.reload = ModHubStyle.IconButton(line.transform, "Reload", InternalResourcesGetter.GetReloadIcon(), null,
            "mod_hub_reload");
        pRow.info = ModHubStyle.IconButton(line.transform, "Info", ModHubUI.GameIcon("iconQuestionMark"), null,
            "mod_hub_info");
    }
}
