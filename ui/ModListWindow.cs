using NeoModLoader.api;
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
    public class ModListItem : AbstractListWindowItem<IMod>
    {
        public override void Setup(IMod mod)
        {
            ModDeclare modDeclare = mod.GetDeclaration();
            Text text = transform.Find("Text").GetComponent<Text>();
            text.text = string.Format(text.text, modDeclare.Name, modDeclare.Version, modDeclare.Author, modDeclare.Description);
            
            LogService.LogInfo($"Try to load icon for mod {modDeclare.Name} from {modDeclare.FolderPath}/{modDeclare.IconPath}");
            if(string.IsNullOrEmpty(modDeclare.IconPath)) return;
            Sprite sprite = SpriteLoadUtils.LoadSingleSprite(Path.Combine(modDeclare.FolderPath, modDeclare.IconPath));
            if (sprite == null)
            {
                return;
            }
            Image icon = transform.Find("Icon").GetComponent<Image>();
            icon.sprite = sprite;
            icon.GetComponent<Button>().onClick.AddListener(() =>
            {
                float currentTime = Time.time;
                if (currentTime - Instance.lastClickTime > 1)
                {
                    Instance.clickTimes = 0;
                }

                if (modDeclare != Instance.clickedMod)
                {
                    Instance.clickedMod = modDeclare;
                    Instance.clickTimes = 0;
                }
                Instance.lastClickTime = currentTime;
                Instance.clickTimes++;
                if (Instance.clickTimes >= 8)
                {
                    Instance.clickTimes = 0;
                    Instance.clickedMod = null;
                    ModUploadWindow.ShowWindow(mod);
                }
            });
            
            Button configureButton = transform.Find("Configure").GetComponent<Button>();
            configureButton.gameObject.SetActive(mod is IConfigurable);
            configureButton.onClick.AddListener(() =>
            {
                if (mod is IConfigurable configurable)
                {
                    ModConfigureWindow.ShowWindow(configurable.GetConfig());
                }
            });
            Button websiteButton = transform.Find("Website").GetComponent<Button>();
            websiteButton.onClick.AddListener(() =>
            {
                Application.OpenURL(mod.GetUrl());
            });
        }
    }
    private HashSet<IMod> showedMods = new();
    private List<IMod> to_add;
    private List<IMod> to_remove;
    protected override void Init()
    {
        GameObject workshopButton = new GameObject("WorkshopButton", typeof(Image), typeof(Button));
        workshopButton.transform.SetParent(BackgroundTransform);
        workshopButton.transform.localPosition = new(125, 0);
        workshopButton.transform.localScale = Vector3.one;
        workshopButton.GetComponent<RectTransform>().sizeDelta = new(20, 20);
        Image workshopButtonImage = workshopButton.GetComponent<Image>();
        workshopButtonImage.sprite = Resources.Load<Sprite>("ui/icons/iconSteam");
        Button workshopButtonButton = workshopButton.GetComponent<Button>();
        workshopButtonButton.onClick.AddListener(() =>
        {
            ScrollWindow.showWindow("WorkshopMods");
        });
    }
    private bool needRefresh = false;
    public override void OnNormalEnable()
    {
        var mods = WorldBoxMod.LoadedMods;
        if(showedMods.IsSubsetOf(mods) && showedMods.IsSupersetOf(mods)) return;
        needRefresh = true;
        to_add = mods.Except(showedMods).ToList();
        to_remove = showedMods.Except(mods).ToList();
        
        showedMods.Clear();
        showedMods.UnionWith(mods);
    }

    private void Update()
    {
        if(!IsOpened) return;
        if (needRefresh)
        {
            if (to_add.Any())
            {
                AddItemToList(to_add[to_add.Count-1]);
                to_add.RemoveAt(to_add.Count - 1);
                return;
            }
            if (to_remove.Any())
            {
                RemoveModFromList(to_remove[to_remove.Count-1]);
                to_remove.RemoveAt(to_remove.Count - 1);
                return;
            }
            needRefresh = false;
        }
    }

    private void RemoveModFromList(IMod mod)
    {
        
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

        GameObject icon = new GameObject("Icon", typeof(Image), typeof(Button));
        icon.transform.SetParent(obj.transform);
        icon.transform.localPosition = new(-75, 0);
        icon.transform.localScale = Vector3.one;
        icon.GetComponent<RectTransform>().sizeDelta = new(40, 40);
        Image iconImage = icon.GetComponent<Image>();
        iconImage.sprite = InternalResourcesGetter.GetIcon();
        
        GameObject iconFrame = new GameObject("IconFrame", typeof(Image));
        iconFrame.transform.SetParent(icon.transform);
        iconFrame.transform.localPosition = Vector3.zero;
        iconFrame.transform.localScale = Vector3.one;
        iconFrame.GetComponent<RectTransform>().sizeDelta = icon.GetComponent<RectTransform>().sizeDelta + new Vector2(5, 5);
        Image iconFrameImage = iconFrame.GetComponent<Image>();
        iconFrameImage.sprite = InternalResourcesGetter.GetIconFrame();
        iconFrameImage.type = Image.Type.Sliced;
        
        GameObject text = new GameObject("Text", typeof(Text));
        text.transform.SetParent(obj.transform);
        text.transform.localPosition = new(12.5f, 0);
        text.transform.localScale = Vector3.one;
        text.GetComponent<RectTransform>().sizeDelta = new(125, 50);
        Text textText = text.GetComponent<Text>();
        textText.font = LocalizedTextManager.currentFont;
        textText.fontSize = 6;
        textText.text = "{0}\t{1}\n{2}\n{3}";
        textText.supportRichText = true;
        
        Vector2 single_button_size = new(22, 22);
        GameObject configure = new GameObject("Configure", typeof(Image), typeof(Button));
        configure.transform.SetParent(obj.transform);
        configure.transform.localPosition = new(87, 12);
        configure.transform.localScale = Vector3.one;
        configure.GetComponent<RectTransform>().sizeDelta = single_button_size;
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
        
        GameObject website = new GameObject("Website", typeof(Image), typeof(Button));
        website.transform.SetParent(obj.transform);
        website.transform.localPosition = new(87, -12);
        website.transform.localScale = Vector3.one;
        website.GetComponent<RectTransform>().sizeDelta = single_button_size;
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

        return obj.GetComponent<ModListItem>();
    }
}