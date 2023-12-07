using NeoModLoader.api;
using NeoModLoader.constants;
using NeoModLoader.services;
using NeoModLoader.utils;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui;

public class ModListWindow : AbstractListWindow<ModListWindow, IMod>
{
    private ModDeclare clickedMod;
    private int clickTimes;
    private float lastClickTime;
    private bool needRefresh = false;
    private List<IMod> to_add = new();

    private void Update()
    {
        if (!IsOpened) return;
        if (needRefresh)
        {
            if (to_add.Any())
            {
                AddItemToList(to_add[to_add.Count - 1]);
                to_add.RemoveAt(to_add.Count - 1);
                return;
            }

            needRefresh = false;
        }
    }

    protected override void Init()
    {
        GameObject workshopButton = new GameObject("WorkshopButton", typeof(Image), typeof(Button), typeof(TipButton));
        workshopButton.transform.SetParent(BackgroundTransform);
        workshopButton.transform.localPosition = new(125, 0);
        workshopButton.transform.localScale = Vector3.one;
        workshopButton.GetComponent<RectTransform>().sizeDelta = new(20, 20);
        Image workshopButtonImage = workshopButton.GetComponent<Image>();
        workshopButtonImage.sprite = Resources.Load<Sprite>("ui/icons/iconSteam");
        Button workshopButtonButton = workshopButton.GetComponent<Button>();
        workshopButtonButton.onClick.AddListener(() => { ScrollWindow.showWindow("WorkshopMods"); });
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
        modloaderButtonTipButton.text_description_2 = "commit\n" + InternalResourcesGetter.GetCommit();
        modloaderButtonTipButton.textOnClickDescription = "NeoModLoader Report";
        Button modloaderButtonButton = modloaderButton.GetComponent<Button>();
        modloaderButtonButton.onClick.AddListener(() => { Application.OpenURL(CoreConstants.RepoURL); });
    }

    public override void OnNormalEnable()
    {
        needRefresh = true;
        ClearList();
        foreach (var loaded_mod in WorldBoxMod.LoadedMods)
        {
            to_add.Add(loaded_mod);
        }

        foreach (var mod in WorldBoxMod.AllRecognizedMods.Keys)
        {
            if (WorldBoxMod.AllRecognizedMods[mod] == ModState.LOADED) continue;
            var virtual_mod = new VirtualMod();
            virtual_mod.OnLoad(mod, null);
            to_add.Add(virtual_mod);
        }
    }

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
        textText.font = LocalizedTextManager.currentFont;
        textText.fontSize = 6;
        textText.supportRichText = true;

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
        configureIconImage.sprite = Resources.Load<Sprite>("ui/icons/iconOptions");

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
        websiteIconImage.sprite = Resources.Load<Sprite>("ui/icons/iconCommunity");

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

    public class ModListItem : AbstractListWindowItem<IMod>
    {
        public override void Setup(IMod mod)
        {
            ModDeclare mod_declare = mod.GetDeclaration();
            ModState mod_state = WorldBoxMod.AllRecognizedMods[mod_declare];

            Text text = transform.Find("Text").GetComponent<Text>();
            text.text = $"{mod_declare.Name}\t{mod_declare.Version}\n{mod_declare.Author}\n{mod_declare.Description}";

            Sprite sprite = null;
            if (!string.IsNullOrEmpty(mod_declare.IconPath))
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
            configure_button.gameObject.SetActive(mod is IConfigurable);

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
                        new Task(() =>
                        {
                            Thread.Sleep(3000);
                            if (Instance.clickTimes == 8)
                            {
                                ModUploadWindow.ShowWindow(mod);
                            }
                        }).Start();
                    }
                });
            }

            if (mod_state == ModState.FAILED)
            {
                icon_tip_button.textOnClick = "ModLoadFailed Title";
                icon_tip_button.textOnClickDescription = "ModLoadFailed Description";
                icon_tip_button.text_description_2 = mod_declare.FailReason.ToString();
                icon.color = Color.red;
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
                if (mod is IConfigurable configurable)
                {
                    ModConfigureWindow.ShowWindow(configurable.GetConfig());
                }
            });
            website_button.onClick.AddListener(() => { Application.OpenURL(mod.GetUrl()); });

            if (Config.isEditor && mod is IReloadable reloadable)
            {
                Button reload_button = transform.Find("Reload").GetComponent<Button>();
                reload_button.gameObject.SetActive(true);
                reload_button.onClick.RemoveAllListeners();
                reload_button.onClick.AddListener(() =>
                {
                    if (!ModReloadUtils.Prepare(mod))
                    {
                        LogService.LogWarning($"Failed to prepare mod {mod_declare.Name} for reloading.");
                        return;
                    }

                    if (!ModReloadUtils.CompileNew())
                    {
                        LogService.LogWarning($"Failed to compile new mod {mod_declare.Name} for reloading.");
                        return;
                    }

                    if (!ModReloadUtils.PatchHotfixMethods())
                    {
                        LogService.LogWarning(
                            $"Failed to patch hotfix methods of mod {mod_declare.Name} for reloading.");
                        return;
                    }

                    if (!ModReloadUtils.Reload())
                    {
                        LogService.LogWarning($"Failed to reload mod {mod_declare.Name}.");
                        return;
                    }
                });
            }
            else
            {
                transform.Find("Reload").gameObject.SetActive(false);
            }
        }
    }
}