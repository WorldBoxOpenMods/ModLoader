using NeoModLoader.api;
using NeoModLoader.services;
using NeoModLoader.utils;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui;

public class WorkshopModListWindow : AbstractListWindow<WorkshopModListWindow, ModDeclare>
{
    public class WorkshopModListItem : AbstractListWindowItem<ModDeclare>
    {
        public override void Setup(ModDeclare modDeclare)
        {
            Text text = transform.Find("Text").GetComponent<Text>();
            text.text = string.Format(text.text, modDeclare.Name, modDeclare.Version, modDeclare.Author, modDeclare.Description);
            LogService.LogInfo($"Try to load icon for mod {modDeclare.Name}'s icon from {modDeclare.FolderPath??"null"}/{modDeclare.IconPath??"null"}");
            if(string.IsNullOrEmpty(modDeclare.IconPath)) return;
            Sprite sprite = SpriteLoadUtils.LoadSprites(Path.Combine(modDeclare.FolderPath, modDeclare.IconPath))[0];
            if (sprite == null)
            {
                return;
            }
            Image icon = transform.Find("Icon").GetComponent<Image>();
            icon.sprite = sprite;
            
            Button loadButton = transform.Find("Load").GetComponent<Button>();
            loadButton.onClick.AddListener(() =>
            {
                if (ModCompileLoadService.IsModLoaded(modDeclare.UUID))
                {
                    ErrorWindow.errorMessage = $"Failed to load mod {modDeclare.Name}:\n" +
                                               $"Mod already loaded.";
                    ScrollWindow.get("error_with_reason").clickShow();
                    return;
                }

                if (modDeclare.ModType == ModTypeEnum.BEPINEX)
                {
                    ModInfoUtils.LinkBepInExModToLocalRequest(modDeclare);
                    ModInfoUtils.DealWithBepInExModLinkRequests();
                    return;
                }

                ModDependencyNode node = ModDepenSolveService.SolveModDependencyRuntime(modDeclare);
                if (node == null)
                {
                    ErrorWindow.errorMessage = $"Failed to load mod {modDeclare.Name}:\n" +
                                               $"Failed to solve mod dependency." +
                                               $"Check Incompatible mods and dependencies, then try again.";
                    ScrollWindow.get("error_with_reason").clickShow();
                    return;
                }
                
                bool success = ModCompileLoadService.compileMod(node);
                if (!success)
                {
                    ErrorWindow.errorMessage = $"Failed to load mod {modDeclare.Name}:\n" +
                                               $"Failed to compile mod." +
                                               $"Check Incompatible mods and dependencies, then try again.";
                    ScrollWindow.get("error_with_reason").clickShow();
                    return;
                }
                
                ModCompileLoadService.LoadMod(node.mod_decl);
            });
            Button websiteButton = transform.Find("Website").GetComponent<Button>();
            websiteButton.onClick.AddListener(() =>
            {
                string name = Path.GetFileName(modDeclare.FolderPath);
                Application.OpenURL($"https://steamcommunity.com/sharedfiles/filedetails/?id={name}");
            });
        }
    }
    protected override void Init()
    {
        
    }

    public override void OnNormalEnable()
    {
        ModWorkshopService.steamWorkshopPromise.Then(prepareModsOrdered).Catch(delegate(Exception err)
        {
            Debug.LogError(err);
            ErrorWindow.errorMessage = "Error happened while connecting to Steam Workshop:\n" + err.Message.ToString();
            ScrollWindow.get("error_with_reason").clickShow();
        });
    }
    private float checkTimer = 0.015f;
    private void Update()
    {
        if(checkTimer > 0)
        {
            checkTimer -= Time.deltaTime;
            return;
        }
        checkTimer = 0.015f;
        showNextMod();
    }
    private Queue<Steamworks.Ugc.Item> modsOrderedQueue = new();
    private void showNextMod()
    {
        if (modsOrderedQueue.Count == 0) return;
        
        Steamworks.Ugc.Item item = modsOrderedQueue.Dequeue();
        ModDeclare mod = ModWorkshopService.GetModFromWorkshopItem(item);
        if (mod == null)
        {
            return;
        }
        AddItemToList(mod);
    }
    private HashSet<string> showedMods = new();
    protected override void AddItemToList(ModDeclare item)
    {
        if (showedMods.Contains(item.UUID))
        {
            return;
        }

        showedMods.Add(item.UUID);
        base.AddItemToList(item);
    }

    private async void prepareModsOrdered()
    {
        List<Steamworks.Ugc.Item> items = await ModWorkshopService.GetSubscribedItems();
        foreach (var item in items)
        {
            modsOrderedQueue.Enqueue(item);
        }
    }

    protected override AbstractListWindowItem<ModDeclare> CreateItemPrefab()
    {
        GameObject obj = new GameObject("WorkshopModListItemPrefab", typeof(Image), typeof(WorkshopModListItem));
        obj.SetActive(false);
        
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
        GameObject download = new GameObject("Load", typeof(Image), typeof(Button));
        download.transform.SetParent(obj.transform);
        download.transform.localPosition = new(87, 12);
        download.transform.localScale = Vector3.one;
        download.GetComponent<RectTransform>().sizeDelta = single_button_size;
        Image downloadImageBG = download.GetComponent<Image>();
        downloadImageBG.sprite = Resources.Load<Sprite>("ui/special/button2");
        downloadImageBG.type = Image.Type.Sliced;
        GameObject downloadIcon = new GameObject("Icon", typeof(Image));
        downloadIcon.transform.SetParent(download.transform);
        downloadIcon.transform.localPosition = Vector3.zero;
        downloadIcon.transform.localScale = Vector3.one;
        downloadIcon.GetComponent<RectTransform>().sizeDelta = single_button_size * 0.875f;
        Image configureIconImage = downloadIcon.GetComponent<Image>();
        configureIconImage.sprite = Resources.Load<Sprite>("ui/icons/iconGameServices");
        
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

        return obj.GetComponent<WorkshopModListItem>();
    }
}