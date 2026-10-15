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
///     One mod in <see cref="ModManagerWindow" />.
/// </summary>
/// <remarks>
///     <see cref="CreatePrefab" /> builds the visuals only. Click and hover handlers are added in
///     <see cref="Awake" />, because Unity does not copy handlers added in code when the pool clones the prefab.
/// </remarks>
internal class ModManagerRow : AbstractListWindowItem<InstalledMod>
{
    private const int upload_click_count = 8;
    private const int max_reason_length = 600;

    [SerializeField] private Image icon;
    [SerializeField] private Button icon_button;
    [SerializeField] private Text title;
    [SerializeField] private Text author;
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
        title.text = string.IsNullOrEmpty(declaration.Version) ? pMod.Name : $"{pMod.Name}  {declaration.Version}";
        author.text = pMod.Author;
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
            ModUploadWindow.ShowWindow(pMod);
        }
    }

    private string StatusTooltip()
    {
        string tooltip = LM.Get(StatusKey(mod.Status) + "_tip");
        switch (mod.Status)
        {
            case InstalledModStatus.MissingDependency:
                IEnumerable<string> missing = mod.Declaration.Dependencies.Where(uid =>
                    ModManagerService.GetDependencyState(uid) == DependencyState.Missing);
                return tooltip + "\n" + string.Join(", ", missing);
            case InstalledModStatus.Failed:
                string reason = mod.Declaration.FailReason.ToString().Trim();
                if (reason.Length > max_reason_length) reason = reason.Substring(0, max_reason_length) + "...";
                return reason.Length == 0 ? tooltip : tooltip + "\n\n" + reason;
            default:
                return tooltip;
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
            InstalledModStatus.Enabled => new Color(0.45f, 0.9f, 0.45f),
            InstalledModStatus.Failed => new Color(1f, 0.4f, 0.35f),
            InstalledModStatus.MissingDependency => new Color(1f, 0.65f, 0.2f),
            InstalledModStatus.RestartRequired => new Color(1f, 0.9f, 0.35f),
            _ => ModHubUI.MutedTextColor
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
        var obj = new GameObject(nameof(ModManagerRow), typeof(Image));
        obj.SetActive(false);
        obj.transform.SetParent(WorldBoxMod.Transform);
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 50);
        Image background = obj.GetComponent<Image>();
        background.sprite = SpriteTextureLoader.getSprite("ui/special/windowInnerSliced");
        background.type = Image.Type.Sliced;

        ModManagerRow row = obj.AddComponent<ModManagerRow>();
        CreateIcon(row);
        row.title = ModHubUI.CreateText(obj.transform, "Title", new Vector2(130, 12), new Vector2(12, 15), 7,
            TextAnchor.MiddleLeft);
        row.author = ModHubUI.CreateText(obj.transform, "Author", new Vector2(130, 10), new Vector2(12, 4), 6,
            TextAnchor.MiddleLeft);
        row.author.color = ModHubUI.MutedTextColor;

        row.status = SimpleButton.Instantiate(obj.transform, pName: "Status");
        row.status.Setup(null, null, " ", new Vector2(80, 15));
        row.status.transform.localPosition = new Vector3(-11, -12);
        row.status.Background.sprite = SpriteTextureLoader.getSprite("ui/special/darkInputFieldEmpty");
        row.status.Text.resizeTextForBestFit = false;
        row.status.Text.fontSize = 8;
        row.status.Text.horizontalOverflow = HorizontalWrapMode.Overflow;

        row.source = ModHubUI.CreateIconButton(obj.transform, "Source", null, new Vector2(14, 14),
            new Vector2(89, 15), null, null);
        row.source.Background.enabled = false;

        Vector2 button_size = new(19, 19);
        row.info = ModHubUI.CreateIconButton(obj.transform, "Info", ModHubUI.GameIcon("iconQuestionMark"),
            button_size, new Vector2(88, -11), null, "mod_hub_info");
        row.configure = ModHubUI.CreateIconButton(obj.transform, "Configure", ModHubUI.GameIcon("iconOptions"),
            button_size, new Vector2(66, -11), null, "mod_hub_configure");
        row.reload = ModHubUI.CreateIconButton(obj.transform, "Reload", InternalResourcesGetter.GetReloadIcon(),
            button_size, new Vector2(44, -11), null, "mod_hub_reload");

        return row;
    }

    private static void CreateIcon(ModManagerRow pRow)
    {
        var icon = new GameObject("Icon", typeof(Image), typeof(Button));
        icon.transform.SetParent(pRow.transform, false);
        icon.transform.localPosition = new Vector3(-75, 0);
        icon.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 40);
        pRow.icon = icon.GetComponent<Image>();
        pRow.icon_button = icon.GetComponent<Button>();

        var frame = new GameObject("IconFrame", typeof(Image));
        frame.transform.SetParent(icon.transform, false);
        frame.GetComponent<RectTransform>().sizeDelta = new Vector2(45, 45);
        Image frame_image = frame.GetComponent<Image>();
        frame_image.sprite = InternalResourcesGetter.GetIconFrame();
        frame_image.type = Image.Type.Sliced;
        frame_image.raycastTarget = false;
    }
}
