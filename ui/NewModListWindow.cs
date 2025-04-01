using NeoModLoader.api;
using NeoModLoader.General;
using NeoModLoader.General.UI.Prefabs;
using NeoModLoader.ui.prefabs;
using NeoModLoader.utils;
using UnityEngine;
using UnityEngine.UI;

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
        var type_select_part = new GameObject("TypeSelectPart", typeof(Image), typeof(VerticalLayoutGroup));
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
        type_mod.Setup(ShowMods, InternalResourcesGetter.GetIcon(), pSize: new Vector2(32, 32), pTipType: "normal",
                       pTipData: new TooltipData
                       {
                           tip_name = "TypeMod Title"
                       });
        type_mod.Background.enabled = false;
        SimpleButton type_resources = Instantiate(SimpleButton.Prefab, type_select_part.transform);
        type_resources.name = "TypeResource";
        type_resources.Setup(ShowResources, SpriteTextureLoader.getSprite("ui/icons/tech/icon_tech_city_storage_3"),
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
        scroll_view_port.localPosition -= new Vector3(0, 10);

        var vert_layout = ContentTransform.gameObject.AddComponent<VerticalLayoutGroup>();
        OT.InitializeNoActionVerticalLayoutGroup(vert_layout);
        var fitter = ContentTransform.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        BackgroundTransform.Find("Scrollgradient").GetComponent<Image>().enabled = false;

        ListPart = ContentTransform as RectTransform;
        ListItemPool = new ObjectPoolGenericMono<ModListItem>(ModListItem.Prefab, ListPart);

        var mod_info_part = new GameObject("ModInfoPart", typeof(Image), typeof(VerticalLayoutGroup));
        mod_info_part.transform.SetParent(BackgroundTransform);
        mod_info_part.transform.localPosition = new Vector3(60, 25);
        mod_info_part.transform.localScale = Vector3.one;
        mod_info_part.GetComponent<Image>().sprite = SpriteTextureLoader.getSprite("ui/special/windowInnerSliced");
        mod_info_part.GetComponent<Image>().type = Image.Type.Sliced;
        ModInfoPart = mod_info_part.GetComponent<RectTransform>();

        var mod_control_part = new GameObject("ModControlPart", typeof(Image), typeof(HorizontalLayoutGroup));
        mod_control_part.transform.SetParent(BackgroundTransform);
        mod_control_part.transform.localPosition = new Vector3(60, -102);
        mod_control_part.transform.localScale = Vector3.one;
        mod_control_part.GetComponent<Image>().sprite = InternalResourcesGetter.GetWindowEmptyFrame();
        mod_control_part.GetComponent<Image>().type = Image.Type.Sliced;

        var nml_general_part = new GameObject("NMLGeneralPart", typeof(Image), typeof(VerticalLayoutGroup));
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
        ModConfigureButton.Setup(ConfigureSelectedMod, SpriteTextureLoader.getSprite("ui/icons/iconOptions"),
                                 pSize: new Vector2(32, 32), pTipType: "normal", pTipData: new TooltipData
                                 {
                                     tip_name = "ModConfigure Title"
                                 });
        ModConfigureButton.Background.enabled = false;
        ModCommunityButton = Instantiate(SimpleButton.Prefab, rect_transform);
        ModCommunityButton.name = "ModCommunityButton";
        ModCommunityButton.Setup(CommunityOfSelectedMod, SpriteTextureLoader.getSprite("ui/icons/iconCommunity"),
                                 pSize: new Vector2(32, 32), pTipType: "normal", pTipData: new TooltipData
                                 {
                                     tip_name = "ModCommunity Title"
                                 });
        ModCommunityButton.Background.enabled = false;
        OpenModFolderButton = Instantiate(SimpleButton.Prefab, rect_transform);
        OpenModFolderButton.name = "OpenModFolderButton";
        OpenModFolderButton.Setup(FolderOfSelectedMod, SpriteTextureLoader.getSprite("ui/icons/iconCustomWorld"),
                                  pSize: new Vector2(32, 32), pTipType: "normal", pTipData: new TooltipData
                                  {
                                      tip_name = "OpenFolder Title"
                                  });
        OpenModFolderButton.Background.enabled = false;
        ToggleModButton = Instantiate(SimpleButton.Prefab, rect_transform);
        ToggleModButton.name = "ToggleModButton";
        ToggleModButton.Setup(ToggleSelectedMod, SpriteTextureLoader.getSprite("ui/icons/iconOn"),
                              pSize: new Vector2(32, 32), pTipType: "normal", pTipData: new TooltipData
                              {
                                  tip_name = "ToggleMod Title"
                              });
        ToggleModButton.Background.enabled = false;
        ReloadModButton = Instantiate(SimpleButton.Prefab, rect_transform);
        ReloadModButton.name = "ReloadModButton";
        ReloadModButton.Setup(ReloadSelectedMod, InternalResourcesGetter.GetReloadIcon(), pSize: new Vector2(32, 32),
                              pTipType: "normal", pTipData: new TooltipData
                              {
                                  tip_name = "ReloadMod Title"
                              });
        ReloadModButton.Background.enabled = false;
        UploadModButton = Instantiate(SimpleButton.Prefab, rect_transform);
        UploadModButton.name = "UploadModButton";
        UploadModButton.Setup(UploadSelectedMod, SpriteTextureLoader.getSprite("ui/icons/iconSaveCloud"),
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    private void ConfigureSelectedMod()
    {
        throw new NotImplementedException();
    }

    private void UploadSelectedMod()
    {
        throw new NotImplementedException();
    }

    private void ReloadSelectedMod()
    {
        throw new NotImplementedException();
    }

    private void ToggleSelectedMod()
    {
        throw new NotImplementedException();
    }

    private void FolderOfSelectedMod()
    {
        throw new NotImplementedException();
    }

    private enum DisplayType
    {
        Mod,
        Resource
    }
}