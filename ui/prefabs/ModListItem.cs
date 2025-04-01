using NeoModLoader.api;
using NeoModLoader.General;
using NeoModLoader.General.UI.Prefabs;
using NeoModLoader.utils;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui.prefabs;

internal class ModListItem : APrefab<ModListItem>
{
    private Image icon;
    private Text  text;

    protected override void Init()
    {
        if (Initialized) return;
        base.Init();
        icon = transform.Find("ModIcon").GetComponent<Image>();
        text = transform.Find("SimpleInfo").GetComponent<Text>();
    }

    public void Setup(ModDeclare pDeclare, Action pAction)
    {
        Init();

        if (!string.IsNullOrEmpty(pDeclare.IconPath))
            icon.sprite = SpriteLoadUtils.LoadSingleSprite(Path.Combine(pDeclare.FolderPath, pDeclare.IconPath));

        if (icon.sprite == null) icon.sprite = InternalResourcesGetter.GetIcon();

        name = pDeclare.Name;

        var mod_name = pDeclare.Name;
        var mod_author = pDeclare.Author;
        var multilang_mod_name_key = $"{mod_name}_{LocalizedTextManager.instance.language}";
        var multilang_mod_author_key = $"{mod_author}_{LocalizedTextManager.instance.language}";
        if (LocalizedTextManager.stringExists(multilang_mod_name_key)) mod_name = LM.Get(multilang_mod_name_key);
        if (LocalizedTextManager.stringExists(multilang_mod_author_key)) mod_author = LM.Get(multilang_mod_author_key);
        text.text = $"{mod_name}\n{mod_author}";
    }

    private static void _init()
    {
        var obj = new GameObject("ModListItem", typeof(Image));
        obj.GetComponent<Image>().sprite = SpriteTextureLoader.getSprite("ui/special/windowInnerSliced");
        obj.GetComponent<Image>().type = Image.Type.Sliced;
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(88, 40);

        var mod_icon = new GameObject("ModIcon", typeof(Image));
        mod_icon.transform.SetParent(obj.transform);
        mod_icon.transform.localPosition = new Vector3(-24.5f, 0, 0);
        mod_icon.transform.localScale = Vector3.one;
        mod_icon.GetComponent<Image>().sprite = InternalResourcesGetter.GetIcon();
        mod_icon.GetComponent<RectTransform>().sizeDelta = new Vector2(28, 28);

        var icon_frame = new GameObject("IconFrame", typeof(Image));
        icon_frame.transform.SetParent(mod_icon.transform);
        icon_frame.transform.localPosition = Vector3.zero;
        icon_frame.transform.localScale = Vector3.one;
        icon_frame.GetComponent<Image>().sprite = InternalResourcesGetter.GetIconFrame();
        icon_frame.GetComponent<RectTransform>().sizeDelta = new Vector2(36, 36);

        var text = new GameObject("ModName", typeof(Text));
        text.transform.SetParent(obj.transform);
        text.transform.localPosition = new Vector3(20, 0, 0);
        text.transform.localScale = Vector3.one;
        text.GetComponent<RectTransform>().sizeDelta = new Vector2(48, 34);
        var text_text = text.GetComponent<Text>();
        text_text.text = "Mod Name\nMod Author";
        text_text.alignment = TextAnchor.UpperLeft;
        text_text.font = LocalizedTextManager.current_font;
        text_text.fontSize = 6;
        text_text.supportRichText = true;

        Prefab = obj.AddComponent<ModListItem>();
    }
}