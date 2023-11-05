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
        icon.transform.localPosition = new(-70, 0);
        icon.transform.localScale = Vector3.one;
        icon.GetComponent<RectTransform>().sizeDelta = new(40, 40);
        Image iconImage = icon.GetComponent<Image>();
        iconImage.sprite = InternalResourcesGetter.GetIcon();
        
        GameObject text = new GameObject("Text", typeof(Text));
        text.transform.SetParent(obj.transform);
        text.GetComponent<RectTransform>().sizeDelta = new(100, 50);
        Text textText = text.GetComponent<Text>();
        textText.font = LocalizedTextManager.currentFont;
        textText.fontSize = 6;
        textText.text = "{0}\t{1}\n{2}\n{3}";
        textText.supportRichText = true;
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