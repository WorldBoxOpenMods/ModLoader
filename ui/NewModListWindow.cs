using NeoModLoader.api;
using NeoModLoader.General;
using NeoModLoader.General.UI.Prefabs;
using NeoModLoader.services;
using NeoModLoader.ui.prefabs;
using NeoModLoader.utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static NeoModLoader.AndroidCompatibilityModule.IL2CPPHelper;
namespace NeoModLoader.ui;

internal class NewModListWindow : AbstractWideWindow<NewModListWindow>
{
    private readonly Dictionary<ModDeclare, ModInfoPanel> ModInfoPanels = new();
    private          DisplayType                          CurrentDisplayType;
    private          ModDeclare                           CurrentSelected;
    private          ObjectPoolGenericMono<ModListItem>   ListItemPool;
    private          RectTransform                        ListPart;
    private          List<ModDeclare>                     ListToShow;
    private          SimpleButton                         ModCommunityButton;

    private SimpleButton  ModConfigureButton;
    private RectTransform ModInfoPart;
    private SimpleButton  OpenModFolderButton;
    private SimpleButton  ReloadModButton;
    private SimpleButton  ToggleModButton;
    private SimpleButton  UploadModButton;

    protected override void Init()
    {
        var type_select_part = CreateGameObject("TypeSelectPart", typeof(Image), typeof(VerticalLayoutGroup));
        type_select_part.transform.SetParent(BackgroundTransform);
        type_select_part.transform.localPosition = new Vector3(-260, 0);
        type_select_part.transform.localScale = Vector3.one;
        type_select_part.GetComponent<Image>().sprite = InternalResourcesGetter.GetWindowEmptyFrame();
        type_select_part.GetComponent<Image>().type = Image.Type.Sliced;
        type_select_part.GetComponent<RectTransform>().sizeDelta = new Vector2(48, 255);
        OT.InitializeNoActionVerticalLayoutGroup(type_select_part.GetComponent<VerticalLayoutGroup>());
        type_select_part.GetComponent<VerticalLayoutGroup>().padding = new RectOffset(0, 0, 12, 0);

        SimpleButton type_mod = Instantiate(SimpleButton.Prefab, type_select_part.transform);
        type_mod.name = "TypeMod";
        type_mod.Setup(C<UnityAction>(ShowMods), InternalResourcesGetter.GetIcon(), pSize: new Vector2(32, 32), pTipType: "normal",
                       pTipData: new TooltipData
                       {
                           tip_name = "TypeMod Title"
                       });
        type_mod.Background.enabled = false;
        SimpleButton type_resources = Instantiate(SimpleButton.Prefab, type_select_part.transform);
        type_resources.name = "TypeResource";
        type_resources.Setup(C<UnityAction>(ShowResources), SpriteTextureLoader.getSprite("ui/icons/tech/icon_tech_city_storage_3"),
                             pSize: new Vector2(32, 32), pTipType: "normal", pTipData: new TooltipData
                             {
                                 tip_name = "TypeResource Title"
                             });
        type_resources.Background.enabled = false;


        GameObject list_part = BackgroundTransform.Find("Scroll View").gameObject;
        list_part.name = "List Scroll View";
        var rect_transform = list_part.GetComponent<RectTransform>();
        rect_transform.sizeDelta = new Vector2(108, 255);
        rect_transform.localPosition = new Vector3(-174, 0, 0);
        rect_transform.localScale = Vector3.one;
        var scroll_rect = list_part.GetComponent<ScrollRect>();
        scroll_rect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
        scroll_rect.verticalScrollbar.GetComponent<RectTransform>().sizeDelta = new Vector2(10, 0);
        var scroll_area_bg = list_part.GetComponent<Image>();
        scroll_area_bg.sprite = SpriteTextureLoader.getSprite("ui/special/windowEmptyFrame");
        scroll_area_bg.type = Image.Type.Sliced;
        scroll_area_bg.color = Color.white;
        var scroll_view_port = list_part.transform.Find("Viewport").GetComponent<RectTransform>();
        scroll_view_port.sizeDelta = new Vector2(0, -20);
        scroll_view_port.localPosition = new Vector3(-54, 117.5f);
        var scrollbar = list_part.transform.Find("Scrollbar Vertical Mask");
        scrollbar.transform.localPosition = new Vector3(62.5f, 0);
        scrollbar.gameObject.SetActive(false);

        var vert_layout = ContentTransform.gameObject.AddComponent<VerticalLayoutGroup>();
        OT.InitializeNoActionVerticalLayoutGroup(vert_layout);
        var fitter = ContentTransform.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        BackgroundTransform.Find("Scrollgradient").GetComponent<Image>().enabled = false;

        ListPart = ContentTransform as RectTransform;
        ListItemPool = new ObjectPoolGenericMono<ModListItem>(ModListItem.Prefab, ListPart);

        var mod_info_part = CreateGameObject("ModInfoPart", typeof(Image), typeof(VerticalLayoutGroup));
        mod_info_part.transform.SetParent(BackgroundTransform);
        mod_info_part.transform.localPosition = new Vector3(60, 25);
        mod_info_part.transform.localScale = Vector3.one;
        mod_info_part.GetComponent<Image>().sprite = SpriteTextureLoader.getSprite("ui/special/windowInnerSliced");
        mod_info_part.GetComponent<Image>().type = Image.Type.Sliced;
        ModInfoPart = mod_info_part.GetComponent<RectTransform>();

        var mod_control_part = CreateGameObject("ModControlPart", typeof(Image), typeof(HorizontalLayoutGroup));
        mod_control_part.transform.SetParent(BackgroundTransform);
        mod_control_part.transform.localPosition = new Vector3(60, -102);
        mod_control_part.transform.localScale = Vector3.one;
        mod_control_part.GetComponent<Image>().sprite = InternalResourcesGetter.GetWindowEmptyFrame();
        mod_control_part.GetComponent<Image>().type = Image.Type.Sliced;

        var nml_general_part = CreateGameObject("NMLGeneralPart", typeof(Image), typeof(VerticalLayoutGroup));
        nml_general_part.transform.SetParent(BackgroundTransform);
        nml_general_part.transform.localPosition = new Vector3(264, 0);
        nml_general_part.transform.localScale = Vector3.one;
        nml_general_part.GetComponent<Image>().sprite = InternalResourcesGetter.GetWindowEmptyFrame();
        nml_general_part.GetComponent<Image>().type = Image.Type.Sliced;

        rect_transform = mod_control_part.GetComponent<RectTransform>();
        rect_transform.sizeDelta = new Vector2(350, 48);

        var hori_layout = mod_control_part.GetComponent<HorizontalLayoutGroup>();
        hori_layout.childAlignment = TextAnchor.MiddleLeft;
        hori_layout.childControlHeight = false;
        hori_layout.childControlWidth = false;
        hori_layout.childForceExpandHeight = false;
        hori_layout.childForceExpandWidth = false;
        hori_layout.childScaleHeight = false;
        hori_layout.childScaleWidth = false;
        hori_layout.spacing = 4;
        hori_layout.padding = new RectOffset(12, 0, 0, 0);

        ModConfigureButton = Instantiate(SimpleButton.Prefab, rect_transform);
        ModConfigureButton.name = "ModConfigureButton";
        ModConfigureButton.Setup(C<UnityAction>(ConfigureSelectedMod), SpriteTextureLoader.getSprite("ui/icons/iconOptions"),
                                 pSize: new Vector2(32, 32), pTipType: "normal", pTipData: new TooltipData
                                 {
                                     tip_name = "ModConfigure Title"
                                 });
        ModConfigureButton.Background.enabled = false;
        ModCommunityButton = Instantiate(SimpleButton.Prefab, rect_transform);
        ModCommunityButton.name = "ModCommunityButton";
        ModCommunityButton.Setup(C<UnityAction>(CommunityOfSelectedMod), SpriteTextureLoader.getSprite("ui/icons/actor_traits/iconcommunity"),
                                 pSize: new Vector2(32, 32), pTipType: "normal", pTipData: new TooltipData
                                 {
                                     tip_name = "ModCommunity Title"
                                 });
        ModCommunityButton.Background.enabled = false;
        OpenModFolderButton = Instantiate(SimpleButton.Prefab, rect_transform);
        OpenModFolderButton.name = "OpenModFolderButton";
        OpenModFolderButton.Setup(C<UnityAction>(FolderOfSelectedMod), SpriteTextureLoader.getSprite("ui/icons/iconCustomWorld"),
                                  pSize: new Vector2(32, 32), pTipType: "normal", pTipData: new TooltipData
                                  {
                                      tip_name = "OpenFolder Title"
                                  });
        OpenModFolderButton.Background.enabled = false;
        ToggleModButton = Instantiate(SimpleButton.Prefab, rect_transform);
        ToggleModButton.name = "ToggleModButton";
        ToggleModButton.Setup(C<UnityAction>(ToggleSelectedMod), SpriteTextureLoader.getSprite("ui/icons/iconOn"),
                              pSize: new Vector2(32, 32), pTipType: "normal");
        ToggleModButton.TipButton.textOnClick = "ToggleMod Title";
        ToggleModButton.Background.enabled = false;
        ReloadModButton = Instantiate(SimpleButton.Prefab, rect_transform);
        ReloadModButton.name = "ReloadModButton";
        ReloadModButton.Setup(C<UnityAction>(ReloadSelectedMod), InternalResourcesGetter.GetReloadIcon(), pSize: new Vector2(32, 32),
                              pTipType: "normal", pTipData: new TooltipData
                              {
                                  tip_name = "ReloadMod Title"
                              });
        ReloadModButton.Background.enabled = false;
        UploadModButton = Instantiate(SimpleButton.Prefab, rect_transform);
        UploadModButton.name = "UploadModButton";
        UploadModButton.Setup(C<UnityAction>(UploadSelectedMod), SpriteTextureLoader.getSprite("ui/icons/iconSaveCloud"),
                              pSize: new Vector2(32, 32), pTipType: "normal", pTipData: new TooltipData
                              {
                                  tip_name = "UploadMod Title"
                              });
        UploadModButton.Background.enabled = false;

        rect_transform = mod_info_part.GetComponent<RectTransform>();
        rect_transform.sizeDelta = new Vector2(350, 200);

        rect_transform = nml_general_part.GetComponent<RectTransform>();
        rect_transform.sizeDelta = new Vector2(48, 255);
    }

    private void ShowResources()
    {
        Clean();
    }

    private void ShowMods()
    {
        Clean();
        ListToShow = WorldBoxMod.AllRecognizedMods.Keys.ToList();
        foreach (ModDeclare mod in ListToShow)
        {
            ModListItem item = ListItemPool.getNext();
            ModDeclare local_mod = mod;
            item.Setup(mod, () => { Select(local_mod); });
        }
    }

    public override void OnFirstEnable()
    {
        CurrentDisplayType = DisplayType.Mod;
    }

    public override void OnNormalEnable()
    {
        switch (CurrentDisplayType)
        {
            case DisplayType.Mod:
                ShowMods();
                break;
            case DisplayType.Resource:
                ShowResources();
                break;
        }
    }

    private void Clean()
    {
        ListItemPool.clear();
    }

    private void Select(ModDeclare pDeclare)
    {
        if (CurrentSelected == pDeclare) return;
        CurrentSelected = pDeclare;
        RefreshInfoPart();
        RefreshControlPart();
    }

    private void RefreshControlPart()
    {
        IMod selected = null;
        foreach (var mod in WorldBoxMod.LoadedMods)
        {
            if (mod.GetDeclaration() == CurrentSelected)
            {
                selected = mod;
                break;
            }
        }
        if (selected is IReloadable)
        {
            ReloadModButton.gameObject.SetActive(true);
        }
        else
        {
            ReloadModButton.gameObject.SetActive(false);
        }

        if (selected is IConfigurable)
        {
            ModConfigureButton.gameObject.SetActive(true);
        }
        else
        {
            ModConfigureButton.gameObject.SetActive(false);
        }
        ToggleModButton.TipButton.textOnClickDescription = WorldBoxMod.AllRecognizedMods[CurrentSelected] switch
        {
            ModState.LOADED => "mod_enabled_description",
            ModState.DISABLED => "mod_disabled_description",
            ModState.FAILED => "mod_failed_description",
            _ => "mod_state_failed"
        };
        var next_disabled = ModInfoUtils.isModDisabled(CurrentSelected.UID);
        if (next_disabled)
        {
            ToggleModButton.Icon.sprite = SpriteTextureLoader.getSprite("ui/icons/iconOff");
            ToggleModButton.TipButton.text_description_2 = "mod_next_state_disabled";
        }
        else
        {
            ToggleModButton.Icon.sprite = SpriteTextureLoader.getSprite("ui/icons/iconOn");
            ToggleModButton.TipButton.text_description_2 = "mod_next_state_enabled";
        }

    }

    private void RefreshInfoPart()
    {
        foreach (ModInfoPanel panel in ModInfoPanels.Values) panel.gameObject.SetActive(false);
        if (ModInfoPanels.ContainsKey(CurrentSelected))
        {
            ModInfoPanels[CurrentSelected].gameObject.SetActive(true);
        }
        else
        {
            ModInfoPanel panel = Instantiate(ModInfoPanel.Prefab, ModInfoPart);
            panel.Setup(CurrentSelected);
            ModInfoPanels.Add(CurrentSelected, panel);
        }
    }

    private void CommunityOfSelectedMod()
    {
        foreach (var mod in WorldBoxMod.LoadedMods)
        {
            if (mod.GetDeclaration() == CurrentSelected)
            {
                Application.OpenURL(mod.GetUrl());
                return;
            }
        }
        Application.OpenURL(CurrentSelected.RepoUrl);
    }

    private void ConfigureSelectedMod()
    {
        foreach (var mod in WorldBoxMod.LoadedMods)
        {
            if (mod.GetDeclaration() == CurrentSelected)
            {
                ModConfigureWindow.ShowWindow((mod as IConfigurable)?.GetConfig());
                return;
            }
        }
    }

    private void UploadSelectedMod()
    {
        throw new NotImplementedException();
    }

    private void ReloadSelectedMod()
    {
        foreach (var mod in WorldBoxMod.LoadedMods)
        {
            if (mod.GetDeclaration() == CurrentSelected && mod is IReloadable reloadable)
            {
                if (!ModReloadUtils.Prepare(reloadable, CurrentSelected))
                {
                    LogService.LogWarning($"Failed to prepare mod {CurrentSelected.Name} for reloading.");
                    return;
                }

                if (!ModReloadUtils.CompileNew())
                {
                    LogService.LogWarning($"Failed to compile new mod {CurrentSelected.Name} for reloading.");
                    return;
                }

                if (!ModReloadUtils.PatchHotfixMethodsNT())
                {
                    LogService.LogWarning(
                        $"Failed to patch hotfix methods of mod {CurrentSelected.Name} for reloading.");
                    return;
                }

                if (!ModReloadUtils.Reload())
                {
                    LogService.LogWarning($"Failed to reload mod {CurrentSelected.Name}.");
                }
                return;
            }
        }
    }

    private void ToggleSelectedMod()
    {
        var next_state = ModInfoUtils.toggleMod(CurrentSelected.UID);
        if (next_state)
        {
            ToggleModButton.Icon.sprite = SpriteTextureLoader.getSprite("ui/icons/iconOn");
            ToggleModButton.TipButton.text_description_2 = "mod_next_state_enabled";
        }
        else
        {
            ToggleModButton.Icon.sprite = SpriteTextureLoader.getSprite("ui/icons/iconOff");
            ToggleModButton.TipButton.text_description_2 = "mod_next_state_disabled";
        }
    }

    private void FolderOfSelectedMod()
    {
        Application.OpenURL(CurrentSelected.FolderPath);
    }

    private enum DisplayType
    {
        Mod,
        Resource
    }
}