using NeoModLoader.AndroidCompatibilityModule;
using NeoModLoader.api;
using NeoModLoader.General.UI.Prefabs;
using static NeoModLoader.AndroidCompatibilityModule.Converter;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui.prefabs;

/// <summary>
///     Panel of displaying mod's information.
/// </summary>
public class ModInfoPanel : APrefab<ModInfoPanel>
{
    public ModDeclare ModDeclaration { get; private set; }
    public static void BaseDecorate(ModInfoPanel pPanel)
    {
        var icon = CreateGameObject("Icon", typeof(Image));
        icon.transform.SetParent(pPanel.transform);
        icon.transform.localPosition = new Vector3(28, -28);
        icon.transform.localScale = Vector3.one;
        icon.GetComponent<RectTransform>().sizeDelta = new Vector2(48, 48);
        icon.GetComponent<Image>().sprite = pPanel.ModDeclaration.GetIcon();

        var mod_name = CreateGameObject("ModName", typeof(Text));
        mod_name.transform.SetParent(pPanel.transform);
        mod_name.transform.localPosition = new Vector3(54, -24, 0);
        mod_name.transform.localScale = Vector3.one;
        mod_name.GetComponent<RectTransform>().sizeDelta = new Vector2(280, 34);
        mod_name.GetComponent<RectTransform>().SetPivot(PivotPresets.MiddleLeft);
        var mod_name_text = mod_name.GetComponent<Text>();
        mod_name_text.text = pPanel.ModDeclaration.GetDisplayName() + (string.IsNullOrEmpty(pPanel.ModDeclaration.Version) ? "" : $"({pPanel.ModDeclaration.Version})");
        mod_name_text.alignment = TextAnchor.UpperLeft;
        mod_name_text.font = LocalizedTextManager.current_font;
        mod_name_text.fontSize = 12;
        mod_name_text.supportRichText = true;

        
        var mod_author = CreateGameObject("ModAuthor", typeof(Text));
        mod_author.transform.SetParent(pPanel.transform);
        mod_author.transform.localPosition = new Vector3(54, -45, 0);
        mod_author.transform.localScale = Vector3.one;
        mod_author.GetComponent<RectTransform>().sizeDelta = new Vector2(280, 34);
        mod_author.GetComponent<RectTransform>().SetPivot(PivotPresets.MiddleLeft);
        var mod_author_text = mod_author.GetComponent<Text>();
        mod_author_text.text = pPanel.ModDeclaration.GetDisplayAuthor();
        mod_author_text.alignment = TextAnchor.UpperLeft;
        mod_author_text.font = LocalizedTextManager.current_font;
        mod_author_text.fontSize = 12;
        mod_name_text.supportRichText = true;


        var mod_desc = CreateGameObject("ModDesc", typeof(Text));
        mod_desc.transform.SetParent(pPanel.transform);
        mod_desc.transform.localPosition = new Vector3(8, -54, 0);
        mod_desc.transform.localScale = Vector3.one;
        mod_desc.GetComponent<RectTransform>().sizeDelta = new Vector2(280, 150);
        mod_desc.GetComponent<RectTransform>().SetPivot(PivotPresets.TopLeft);
        var mod_desc_text = mod_desc.GetComponent<Text>();
        mod_desc_text.text = pPanel.ModDeclaration.GetDisplayDesc();
        mod_desc_text.alignment = TextAnchor.UpperLeft;
        mod_desc_text.font = LocalizedTextManager.current_font;
        mod_desc_text.fontSize = 12;
        mod_desc_text.supportRichText = true;
    }
    internal void Setup(ModDeclare pModDeclaration)
    {
        ModDeclaration = pModDeclaration;
        name = pModDeclaration.Name;
        ModState mod_state = WorldBoxMod.AllRecognizedMods[pModDeclaration];
        if (mod_state == ModState.LOADED)
        {
            IMod mod = WorldBoxMod.LoadedMods.Find(x => x.GetDeclaration() == pModDeclaration);
            if (mod is IDecoratePanel decorate) 
            {
                decorate.DecoratePanel(this);
            }
            else
            {
                BaseDecorate(this);
            }
        }
        else
        {
            BaseDecorate(this);
        }
    }

    private static void _init()
    {
        var obj = CreateGameObject("ModInfoPanel", typeof(RectTransform));
        obj.GetComponent<RectTransform>().pivot = new(0, 1);
        obj.GetComponent<RectTransform>().sizeDelta = new(350, 200);


        Prefab = obj.AddComponent<ModInfoPanel>();
    }
}