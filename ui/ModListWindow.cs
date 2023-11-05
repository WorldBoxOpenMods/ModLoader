using NeoModLoader.api;
using NeoModLoader.services;
using NeoModLoader.utils;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui;

public class ModListWindow : AbstractWindow<ModListWindow>
{
    class ModListItem : MonoBehaviour
    {
        public void SetMod(IMod mod)
        {
            ModDeclare modDeclare = mod.GetDeclaration();
            Text text = transform.Find("Text").GetComponent<Text>();
            text.text = string.Format(text.text, modDeclare.Name, modDeclare.Version, modDeclare.Author, modDeclare.Description);
            
            LogService.LogInfo($"Try to load icon for mod {modDeclare.Name} from {modDeclare.FolderPath}/{modDeclare.IconPath}");
            if(string.IsNullOrEmpty(modDeclare.IconPath)) return;
            Sprite sprite = SpriteLoadUtils.LoadSprites(Path.Combine(modDeclare.FolderPath, modDeclare.IconPath))[0];
            if (sprite == null)
            {
                return;
            }
            Image icon = transform.Find("Icon").GetComponent<Image>();
            icon.sprite = sprite;
            
            Button configureButton = transform.Find("Configure").GetComponent<Button>();
            configureButton.onClick.AddListener(() =>
            {
                //ModConfigureWindow.ShowWindow(mod);
            });
            Button websiteButton = transform.Find("Website").GetComponent<Button>();
            websiteButton.onClick.AddListener(() =>
            {
                Application.OpenURL(mod.GetUrl());
            });
        }
    }
    private HashSet<IMod> showedMods = new();
    private static ModListItem modListItemPrefab;
    protected override void Init()
    {
        VerticalLayoutGroup layoutGroup = ContentTransform.gameObject.AddComponent<VerticalLayoutGroup>();
        ContentSizeFitter sizeFitter = ContentTransform.gameObject.AddComponent<ContentSizeFitter>();
        
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.spacing = 10;
        layoutGroup.padding = new(30, 30, 10, 10);

        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        
        
        GameObject obj = new GameObject("ModListItemPrefab", typeof(Image), typeof(ModListItem));
        obj.SetActive(false);
        
        modListItemPrefab = obj.GetComponent<ModListItem>();
        
        obj.transform.SetParent(WorldBoxMod.Transform);

        obj.GetComponent<RectTransform>().sizeDelta = new(0, 50);
        Image bg = obj.GetComponent<Image>();
        bg.sprite = Resources.Load<Sprite>("ui/special/windowInnerSliced");
        bg.type = Image.Type.Sliced;

        GameObject icon = new GameObject("Icon", typeof(Image));
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
    }

    public override void OnNormalEnable()
    {
        var mods = WorldBoxMod.LoadedMods;
        if(showedMods.IsSubsetOf(mods) && showedMods.IsSupersetOf(mods)) return;

        var added = mods.Except(showedMods);
        var removed = showedMods.Except(mods);
        
        foreach (var mod in added)
        {
            AddModToList(mod);
        }

        foreach (var mod in removed)
        {
            RemoveModFromList(mod);
        }
        showedMods.Clear();
        showedMods.UnionWith(showedMods);
    }

    private void AddModToList(IMod mod)
    {
        ModListItem item = Instantiate(modListItemPrefab, ContentTransform);
        item.transform.localScale = Vector3.one;
        item.SetMod(mod);
        item.gameObject.SetActive(true);
    }

    private void RemoveModFromList(IMod mod)
    {
        
    }
}