using NeoModLoader.api;
using NeoModLoader.services;
using NeoModLoader.utils;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui;

internal class WorkshopModListWindow : AbstractListWindow<WorkshopModListWindow, ModDeclare>
{
    private float checkTimer = 0.015f;
    private HashSet<string> showedMods = new();

    private void Update()
    {
        if (checkTimer > 0)
        {
            checkTimer -= Time.deltaTime;
            return;
        }

        checkTimer = 0.015f;
        showNextMod();
    }

    protected override void Init()
    {
    }

    public override void OnNormalEnable()
    {
        ModWorkshopService.steamWorkshopPromise.Then(ModWorkshopService.FindSubscribedMods).Catch(
            delegate(Exception err)
            {
                Debug.LogError(err);
                ErrorWindow.errorMessage =
                    "Error happened while connecting to Steam Workshop:\n" + err.Message.ToString();
                ScrollWindow.get("error_with_reason").clickShow();
            });
    }

    private void showNextMod()
    {
        ModDeclare mod = ModWorkshopService.GetNextModFromWorkshopItem();
        if (mod == null)
        {
            return;
        }

        AddItemToList(mod);
    }

    protected override void AddItemToList(ModDeclare item)
    {
        if (showedMods.Contains(item.UID))
        {
            return;
        }

        showedMods.Add(item.UID);
        base.AddItemToList(item);
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
        iconFrame.GetComponent<RectTransform>().sizeDelta =
            icon.GetComponent<RectTransform>().sizeDelta + new Vector2(5, 5);
        Image iconFrameImage = iconFrame.GetComponent<Image>();
        iconFrameImage.sprite = InternalResourcesGetter.GetIconFrame();
        iconFrameImage.type = Image.Type.Sliced;

        GameObject text = new GameObject("Text", typeof(Text));
        text.transform.SetParent(obj.transform);
        text.transform.localPosition = new(12.5f, 0);
        text.transform.localScale = Vector3.one;
        text.GetComponent<RectTransform>().sizeDelta = new(125, 50);
        Text textText = text.GetComponent<Text>();
        textText.font = LocalizedTextManager.current_font;
        textText.fontSize = 6;
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

    public class WorkshopModListItem : AbstractListWindowItem<ModDeclare>
    {
        public override void Setup(ModDeclare modDeclare)
        {
            Text text = transform.Find("Text").GetComponent<Text>();
            text.text = $"{modDeclare.Name}\t{modDeclare.Version}\n{modDeclare.Author}\n{modDeclare.Description}";
            Sprite sprite = null;
            if (!string.IsNullOrEmpty(modDeclare.IconPath))
            {
                sprite = SpriteLoadUtils.LoadSingleSprite(Path.Combine(modDeclare.FolderPath, modDeclare.IconPath));
            }

            if (sprite == null)
            {
                sprite = InternalResourcesGetter.GetIcon();
            }

            Image icon = transform.Find("Icon").GetComponent<Image>();
            icon.sprite = sprite;

            Button loadButton = transform.Find("Load").GetComponent<Button>();
            loadButton.onClick.AddListener(() =>
            {
                if (ModCompileLoadService.IsModLoaded(modDeclare.UID))
                {
                    ErrorWindow.errorMessage = $"Failed to load mod {modDeclare.Name}:\n" +
                                               $"Mod already loaded.";
                    ScrollWindow.get("error_with_reason").clickShow();
                    return;
                }

                // Check mod loaded or not has been done in the following method.
                ModCompileLoadService.TryCompileAndLoadModAtRuntime(modDeclare);
            });
            Button websiteButton = transform.Find("Website").GetComponent<Button>();
            websiteButton.onClick.AddListener(() =>
            {
                string name = Path.GetFileName(modDeclare.FolderPath);
                Application.OpenURL($"https://steamcommunity.com/sharedfiles/filedetails/?id={name}");
            });
        }
    }
}