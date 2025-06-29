using System.Collections;
using NeoModLoader.api;
using NeoModLoader.constants;
using NeoModLoader.General;
using NeoModLoader.services;
using NeoModLoader.utils;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui;

/// <summary>
///     List window of all mods recognized by NeoModLoader.
/// </summary>
public class ModListWindow : AbstractListWindow<ModListWindow, IMod>
{
    private readonly Queue<IMod> to_add = new();
    private ModDeclare clickedMod;
    private int clickTimes;
    private float lastClickTime;
    private bool needRefresh;

    private void Update()
    {
        if (!IsOpened) return;
        if (needRefresh)
        {
            if (to_add.Any())
            {
                AddItemToList(to_add.Dequeue());
                return;
            }

            needRefresh = false;
        }
    }

    /// <inheritdoc cref="AbstractListWindow{T,TItem}.Init" />
    protected override void Init()
    {
        GameObject workshopButton = new GameObject("WorkshopButton", typeof(Image), typeof(Button), typeof(TipButton));
        workshopButton.transform.SetParent(BackgroundTransform);
        workshopButton.transform.localPosition = new Vector3(140, 0);
        workshopButton.transform.localScale = Vector3.one;
        workshopButton.GetComponent<RectTransform>().sizeDelta = new(20, 20);
        Image workshopButtonImage = workshopButton.GetComponent<Image>();
        workshopButtonImage.sprite = Resources.Load<Sprite>("ui/icons/iconSteam");
        Button workshopButtonButton = workshopButton.GetComponent<Button>();
        workshopButtonButton.onClick.AddListener(() =>
        {
            if (Others.is_editor)
            {
                InformationWindow.ShowWindow("WorkshopMods Window is not supported in editor environment");
                return;
            }

            ScrollWindow.showWindow("WorkshopMods");
        });
        TipButton workshopButtonTipButton = workshopButton.GetComponent<TipButton>();
        workshopButtonTipButton.textOnClick = "WorkshopMods Title";

        GameObject modloaderButton =
            new GameObject("ModLoaderButton", typeof(Image), typeof(Button), typeof(TipButton));
        modloaderButton.transform.SetParent(BackgroundTransform);
        modloaderButton.transform.localPosition = new(-125, 0);
        modloaderButton.transform.localScale = Vector3.one;
        modloaderButton.GetComponent<RectTransform>().sizeDelta = new(20, 20);
        Image modloaderButtonImage = modloaderButton.GetComponent<Image>();
        modloaderButtonImage.sprite = InternalResourcesGetter.GetIcon();
        TipButton modloaderButtonTipButton = modloaderButton.GetComponent<TipButton>();
        modloaderButtonTipButton.textOnClick = "NeoModLoader-v" + WorldBoxMod.NeoModLoaderAssembly.GetName().Version;
        foreach (var lang in LocalizedTextManager.getAllLanguages())
            LM.Add(lang, "NMLCommit", $"commit\n{InternalResourcesGetter.GetCommit()}");
        modloaderButtonTipButton.text_description_2 = "NMLCommit";
        modloaderButtonTipButton.textOnClickDescription = "NeoModLoader Report";
        Button modloaderButtonButton = modloaderButton.GetComponent<Button>();
        modloaderButtonButton.onClick.AddListener(() => { Application.OpenURL(CoreConstants.RepoURL); });
    }

    /// <inheritdoc cref="AbstractListWindow{T,TItem}.OnNormalEnable" />
    public override void OnNormalEnable()
    {
        needRefresh = true;
        ClearList();
        foreach (var loaded_mod in WorldBoxMod.LoadedMods)
        {
            to_add.Enqueue(loaded_mod);
        }

        foreach (var mod in WorldBoxMod.AllRecognizedMods.Keys)
        {
            if (WorldBoxMod.AllRecognizedMods[mod] == ModState.LOADED) continue;
            var virtual_mod = new VirtualMod();
            virtual_mod.OnLoad(mod, null);
            to_add.Enqueue(virtual_mod);
        }
    }

    /// <inheritdoc cref="AbstractListWindow{T,TItem}.CreateItemPrefab" />
    protected override AbstractListWindowItem<IMod> CreateItemPrefab()
    {
        GameObject obj = new GameObject("ModListItemPrefab", typeof(Image), typeof(ModListItem));
        obj.SetActive(false);

        obj.transform.SetParent(WorldBoxMod.Transform);

        obj.GetComponent<RectTransform>().sizeDelta = new(0, 50);
        Image bg = obj.GetComponent<Image>();
        bg.sprite = Resources.Load<Sprite>("ui/special/windowInnerSliced");
        bg.type = Image.Type.Sliced;

        GameObject icon = new GameObject("Icon", typeof(Image), typeof(Button), typeof(TipButton));
        icon.transform.SetParent(obj.transform);
        icon.transform.localPosition = new(-75, 0);
        icon.transform.localScale = Vector3.one;
        icon.GetComponent<RectTransform>().sizeDelta = new(40, 40);
        icon.GetComponent<TipButton>().type = "normal";
        Image iconImage = icon.GetComponent<Image>();
        iconImage.sprite = InternalResourcesGetter.GetIcon();

        GameObject iconFrame = new GameObject("IconFrame", typeof(Image));
        iconFrame.transform.SetParent(icon.transform);
        iconFrame.transform.localPosition = Vector3.zero;
        iconFrame.transform.localScale = Vector3.one;
        iconFrame.GetComponent<RectTransform>().sizeDelta =
            icon.GetComponent<RectTransform>().sizeDelta + new Vector2(5, 5);
        Image iconFrameImage = iconFrame.GetComponent<Image>();
        iconFrameImage.sprite = InternalResourcesGetter.GetIconFrame();
        iconFrameImage.type = Image.Type.Sliced;

        GameObject text = new GameObject("Text", typeof(Text));
        text.transform.SetParent(obj.transform);
        text.transform.localPosition = new Vector3(2.5f, 0);
        text.transform.localScale = Vector3.one;
        text.GetComponent<RectTransform>().sizeDelta = new Vector2(105, 50);
        Text textText = text.GetComponent<Text>();
        textText.font = LocalizedTextManager.current_font;
        textText.fontSize = 6;
        textText.supportRichText = true;


        var state_text = new GameObject("StateText", typeof(Text));
        state_text.transform.SetParent(obj.transform);
        state_text.transform.localPosition = new Vector3(2.5f, -15.5f);
        state_text.transform.localScale = Vector3.one;
        state_text.GetComponent<RectTransform>().sizeDelta = new Vector2(105, 10);
        var state_textText = state_text.GetComponent<Text>();
        state_textText.font = LocalizedTextManager.current_font;
        state_textText.fontSize = 6;
        state_textText.supportRichText = true;
        state_textText.alignment = TextAnchor.LowerLeft;

        Vector2 single_button_size = new(22, 22);
        GameObject configure = new GameObject("Configure", typeof(Image), typeof(Button), typeof(TipButton));
        configure.transform.SetParent(obj.transform);
        configure.transform.localPosition = new(87, 12);
        configure.transform.localScale = Vector3.one;
        configure.GetComponent<RectTransform>().sizeDelta = single_button_size;
        configure.GetComponent<TipButton>().textOnClick = "ModConfigure Title";
        Image configureImageBG = configure.GetComponent<Image>();
        configureImageBG.sprite = Resources.Load<Sprite>("ui/special/button2");
        configureImageBG.type = Image.Type.Sliced;
        GameObject configureIcon = new GameObject("Icon", typeof(Image));
        configureIcon.transform.SetParent(configure.transform);
        configureIcon.transform.localPosition = Vector3.zero;
        configureIcon.transform.localScale = Vector3.one;
        configureIcon.GetComponent<RectTransform>().sizeDelta = single_button_size * 0.875f;
        Image configureIconImage = configureIcon.GetComponent<Image>();
        configureIconImage.sprite = Resources.Load<Sprite>("ui/icons/iconoptions");

        GameObject website = new GameObject("Website", typeof(Image), typeof(Button), typeof(TipButton));
        website.transform.SetParent(obj.transform);
        website.transform.localPosition = new(87, -12);
        website.transform.localScale = Vector3.one;
        website.GetComponent<RectTransform>().sizeDelta = single_button_size;
        website.GetComponent<TipButton>().textOnClick = "ModCommunity Title";
        Image websiteImageBG = website.GetComponent<Image>();
        websiteImageBG.sprite = Resources.Load<Sprite>("ui/special/button2");
        websiteImageBG.type = Image.Type.Sliced;
        GameObject websiteIcon = new GameObject("Icon", typeof(Image));
        websiteIcon.transform.SetParent(website.transform);
        websiteIcon.transform.localPosition = Vector3.zero;
        websiteIcon.transform.localScale = Vector3.one;
        websiteIcon.GetComponent<RectTransform>().sizeDelta = single_button_size * 0.875f;
        Image websiteIconImage = websiteIcon.GetComponent<Image>();
        websiteIconImage.sprite = Resources.Load<Sprite>("ui/icons/actor_traits/iconcommunity");

        GameObject reload = new GameObject("Reload", typeof(Image), typeof(Button), typeof(TipButton));
        reload.transform.SetParent(obj.transform);
        reload.transform.localPosition = new Vector3(64, -12);
        reload.transform.localScale = Vector3.one;
        reload.GetComponent<RectTransform>().sizeDelta = single_button_size * 0.9f;
        reload.GetComponent<TipButton>().textOnClick = "ModReload Title";
        Image reloadImageBG = reload.GetComponent<Image>();
        reloadImageBG.sprite = Resources.Load<Sprite>("ui/special/special_buttonred");
        reloadImageBG.type = Image.Type.Sliced;
        GameObject reloadIcon = new GameObject("Icon", typeof(Image));
        reloadIcon.transform.SetParent(reload.transform);
        reloadIcon.transform.localPosition = Vector3.zero;
        reloadIcon.transform.localScale = Vector3.one;
        reloadIcon.GetComponent<RectTransform>().sizeDelta = single_button_size * 0.875f * 0.9f;
        Image reloadIconImage = reloadIcon.GetComponent<Image>();
        reloadIconImage.sprite = InternalResourcesGetter.GetReloadIcon();

        GameObject open_folder = new("OpenFolder", typeof(Image), typeof(Button), typeof(TipButton));
        open_folder.transform.SetParent(obj.transform);
        open_folder.transform.localPosition = new Vector3(64, 11);
        open_folder.transform.localScale = Vector3.one;
        open_folder.GetComponent<RectTransform>().sizeDelta = single_button_size * 0.9f;
        open_folder.GetComponent<TipButton>().textOnClick = "OpenFolder Title";
        Image open_folderImageBG = open_folder.GetComponent<Image>();
        open_folderImageBG.sprite = Resources.Load<Sprite>("ui/special/special_buttonred");
        open_folderImageBG.type = Image.Type.Sliced;
        GameObject open_folderIcon = new("Icon", typeof(Image));
        open_folderIcon.transform.SetParent(open_folder.transform);
        open_folderIcon.transform.localPosition = Vector3.zero;
        open_folderIcon.transform.localScale = Vector3.one;
        open_folderIcon.GetComponent<RectTransform>().sizeDelta = single_button_size * 0.875f * 0.9f;
        Image open_folderIconImage = open_folderIcon.GetComponent<Image>();
        open_folderIconImage.sprite = SpriteTextureLoader.getSprite("ui/icons/iconCustomWorld");

        return obj.GetComponent<ModListItem>();
    }

    /// <summary>
    ///     A single list item for <see cref="ModListWindow" />.
    /// </summary>
    public class ModListItem : AbstractListWindowItem<IMod>
    {
        private IMod _mod;

        private IEnumerator WaitOpenWindow()
        {
            yield return new WaitForSeconds(3f);
            if (Instance.clickTimes == 8) ModUploadWindow.ShowWindow(_mod);
        }

        /// <inheritdoc cref="AbstractListWindowItem{TItem}.Setup" />
        /// <param name="mod">The mod to display</param>
        public override void Setup(IMod mod)
        {
            _mod = mod;
            ModDeclare mod_declare = mod.GetDeclaration();
            ModState mod_state = WorldBoxMod.AllRecognizedMods[mod_declare];

            Text text = transform.Find("Text").GetComponent<Text>();
            var state_text = transform.Find("StateText").GetComponent<Text>();
            string mod_name = mod_declare.Name;
            string mod_author = mod_declare.Author;
            string mod_desc = mod_declare.Description;
            string multilang_mod_name_key = $"{mod_name}_{LocalizedTextManager.instance.language}";
            string multilang_mod_author_key = $"{mod_author}_{LocalizedTextManager.instance.language}";
            string multilang_mod_desc_key = $"{mod_desc}_{LocalizedTextManager.instance.language}";

            if (LocalizedTextManager.stringExists(multilang_mod_name_key))
            {
                mod_name = LM.Get(multilang_mod_name_key);
            }

            if (LocalizedTextManager.stringExists(multilang_mod_author_key))
            {
                mod_author = LM.Get(multilang_mod_author_key);
            }

            if (LocalizedTextManager.stringExists(multilang_mod_desc_key))
            {
                mod_desc = LM.Get(multilang_mod_desc_key);
            }

            text.text = $"{mod_name}\t{mod_declare.Version}\n{mod_author}\n{mod_desc}";

            Sprite sprite = null;
            if (!string.IsNullOrEmpty(mod_declare.IconPath) &&
                File.Exists(Path.Combine(mod_declare.FolderPath, mod_declare.IconPath)))
            {
                sprite = SpriteLoadUtils.LoadSingleSprite(Path.Combine(mod_declare.FolderPath, mod_declare.IconPath));
            }

            if (sprite == null)
            {
                sprite = InternalResourcesGetter.GetIcon();
            }

            Image icon = transform.Find("Icon").GetComponent<Image>();
            Button configure_button = transform.Find("Configure").GetComponent<Button>();
            Button website_button = transform.Find("Website").GetComponent<Button>();
            Button open_folder_button = transform.Find("OpenFolder").GetComponent<Button>();
            TipButton icon_tip_button = icon.GetComponent<TipButton>();

            icon.sprite = sprite;
            var configurable = mod.GetGameObject()?.GetComponent<IConfigurable>();
            configure_button.gameObject.SetActive(configurable != null);

            icon.GetComponent<Button>().onClick.RemoveAllListeners();
            configure_button.onClick.RemoveAllListeners();
            website_button.onClick.RemoveAllListeners();
            open_folder_button.onClick.RemoveAllListeners();
            open_folder_button.onClick.AddListener(() => { Application.OpenURL(mod_declare.FolderPath); });

            if (mod_state == ModState.LOADED)
            {
                icon.GetComponent<Button>().onClick.AddListener(() =>
                {
                    float current_time = Time.time;
                    if (current_time - Instance.lastClickTime > 1)
                    {
                        Instance.clickTimes = 0;
                    }

                    if (mod_declare != Instance.clickedMod)
                    {
                        Instance.clickedMod = mod_declare;
                        Instance.clickTimes = 0;
                    }

                    Instance.lastClickTime = current_time;
                    Instance.clickTimes++;
                    if (Instance.clickTimes == 8)
                    {
                        StartCoroutine(nameof(WaitOpenWindow));
                        /*
                        new Task(() =>
                        {
                            Thread.Sleep(3000);
                            if (Instance.clickTimes == 8)
                            {
                                ModUploadWindow.ShowWindow(mod);
                            }
                        }).Start();*/
                    }
                });
            }

            var current_state_text = mod_state switch
            {
                ModState.DISABLED => LM.Get("mod_state_disabled"),
                ModState.LOADED => LM.Get("mod_state_enabled"),
                ModState.FAILED => LM.Get("mod_state_failed")
            };
            var next_state_text = LM.Get(ModInfoUtils.isModDisabled(mod_declare.UID)
                ? "mod_next_state_disabled"
                : "mod_next_state_enabled");
            state_text.text = $"{current_state_text}, {next_state_text}";
            if (mod_state == ModState.FAILED)
            {
                icon_tip_button.textOnClick = "ModLoadFailed Title";
                icon_tip_button.textOnClickDescription = "ModLoadFailed Description";
                icon_tip_button.text_description_2 = mod_declare.FailReason.ToString();
                icon.color = Color.red;

                icon.GetComponent<Button>().onClick.AddListener(() =>
                {
                    var curr_state = ModInfoUtils.toggleMod(mod_declare.UID);
                    icon.color = curr_state ? Color.red : Color.yellow;

                    next_state_text = LM.Get(!curr_state
                        ? "mod_next_state_disabled"
                        : "mod_next_state_enabled");
                    state_text.text = $"{current_state_text}, {next_state_text}";
                });
            }
            else
            {
                icon_tip_button.textOnClick = "ToggleMod Title";
                icon_tip_button.textOnClickDescription = ModInfoUtils.isModDisabled(mod_declare.UID)
                    ? "ModDisabled Description"
                    : "ModEnabled Description";
                icon.color = ModInfoUtils.isModDisabled(mod_declare.UID) ? Color.gray : Color.white;
                icon.GetComponent<Button>().onClick.AddListener(() =>
                {
                    bool curr_state = ModInfoUtils.toggleMod(mod_declare.UID);
                    icon_tip_button.textOnClickDescription =
                        curr_state ? "ModEnabled Description" : "ModDisabled Description";
                    icon.color = curr_state ? Color.white : Color.gray;

                    next_state_text = LM.Get(!curr_state
                        ? "mod_next_state_disabled"
                        : "mod_next_state_enabled");
                    state_text.text = $"{current_state_text}, {next_state_text}";

                    if (curr_state)
                    {
                        // Check mod loaded or not has been done in the following method.
                        ModCompileLoadService.TryCompileAndLoadModAtRuntime(mod_declare);
                    }
                });
                icon_tip_button.text_description_2 = "";
            }

            configure_button.onClick.AddListener(() =>
            {
                // It can be sure that if mod is IConfigurable, then mod is loaded actually.
                ModConfigureWindow.ShowWindow(configurable?.GetConfig());
            });
            website_button.onClick.AddListener(() => { Application.OpenURL(mod.GetUrl()); });

            if (!Config.isEditor)
            {
                transform.Find("Reload").gameObject.SetActive(false);
                return;
            }

            var reloadable = mod.GetGameObject()?.GetComponent<IReloadable>();
            if (reloadable == null)
            {
                transform.Find("Reload").gameObject.SetActive(false);
                return;
            }

            var reload_button = transform.Find("Reload").GetComponent<Button>();
            reload_button.gameObject.SetActive(true);
            reload_button.onClick.RemoveAllListeners();
            reload_button.onClick.AddListener(() =>
            {
                if (!ModReloadUtils.Prepare(reloadable, mod_declare))
                {
                    LogService.LogWarning($"Failed to prepare mod {mod_declare.Name} for reloading.");
                    return;
                }

                if (!ModReloadUtils.CompileNew())
                {
                    LogService.LogWarning($"Failed to compile new mod {mod_declare.Name} for reloading.");
                    return;
                }

                if (!ModReloadUtils.PatchHotfixMethodsNT())
                {
                    LogService.LogWarning(
                        $"Failed to patch hotfix methods of mod {mod_declare.Name} for reloading.");
                    return;
                }

                if (!ModReloadUtils.Reload())
                {
                    LogService.LogWarning($"Failed to reload mod {mod_declare.Name}.");
                }
            });
        }
    }
}