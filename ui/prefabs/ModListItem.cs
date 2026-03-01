using NeoModLoader.AndroidCompatibilityModule;
using NeoModLoader.api;
using NeoModLoader.General;
using NeoModLoader.General.UI.Prefabs;
using NeoModLoader.utils;
using static NeoModLoader.AndroidCompatibilityModule.IL2CPPHelper;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NeoModLoader.ui.prefabs;

internal class ModListItem : APrefab<ModListItem>
{
    private Image icon;
    private Text  text;
    private Button button;

    protected override void Init()
    {
        if (Initialized) return;
        base.Init();
        icon = transform.Find("ModIcon").GetComponent<Image>();
        text = transform.Find("ModName").GetComponent<Text>();
        button = transform.Find("ModIcon").GetComponent<Button>();
    }

    public void Setup(ModDeclare pDeclare, Action pAction)
    {
        Init();

        icon.sprite = pDeclare.GetIcon();
        name = pDeclare.Name;

        var mod_name = pDeclare.GetDisplayName();
        var mod_author = pDeclare.GetDisplayAuthor();
        mod_name = mod_name.Length > 10 ? mod_name.Substring(0, 10) : mod_name;
        mod_author = mod_author.Length > 20 ? mod_author.Substring(0, 20) : mod_author;
        var mod_state = WorldBoxMod.AllRecognizedMods[pDeclare] switch
        {
            ModState.LOADED => "mod_state_enabled",
            ModState.DISABLED => "mod_state_disabled",
            ModState.FAILED => "mod_state_failed",
            _ => "mod_state_failed"
        };
        text.text = $"{mod_name}\n{mod_author}\n{LM.Get(mod_state)}";
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(C<UnityAction>(() => {pAction?.Invoke();}));
    }

    private static void _init()
    {
        var obj = CreateGameObject("ModListItem", typeof(Image));
        obj.GetComponent<Image>().sprite = SpriteTextureLoader.getSprite("ui/special/windowInnerSliced");
        obj.GetComponent<Image>().type = Image.Type.Sliced;
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(88, 40);

        var mod_icon = CreateGameObject("ModIcon", typeof(Image), typeof(Button));
        mod_icon.transform.SetParent(obj.transform);
        mod_icon.transform.localPosition = new Vector3(-24.5f, 0, 0);
        mod_icon.transform.localScale = Vector3.one;
        mod_icon.GetComponent<Image>().sprite = InternalResourcesGetter.GetIcon();
        mod_icon.GetComponent<RectTransform>().sizeDelta = new Vector2(28, 28);

        var icon_frame = CreateGameObject("IconFrame", typeof(Image));
        icon_frame.transform.SetParent(mod_icon.transform);
        icon_frame.transform.localPosition = Vector3.zero;
        icon_frame.transform.localScale = Vector3.one;
        icon_frame.GetComponent<Image>().sprite = InternalResourcesGetter.GetIconFrame();
        icon_frame.GetComponent<RectTransform>().sizeDelta = new Vector2(36, 36);

        var text = CreateGameObject("ModName", typeof(Text));
        text.transform.SetParent(obj.transform);
        text.transform.localPosition = new Vector3(20, 0, 0);
        text.transform.localScale = Vector3.one;
        text.GetComponent<RectTransform>().sizeDelta = new Vector2(48, 34);
        var text_text = text.GetComponent<Text>();
        text_text.text = "Mod Name\nMod Author\nLoaded";
        text_text.alignment = TextAnchor.UpperLeft;
        text_text.font = LocalizedTextManager.current_font;
        text_text.fontSize = 6;
        text_text.supportRichText = true;

        Prefab = obj.AddComponent<ModListItem>();
    }
}