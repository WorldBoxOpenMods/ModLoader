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
    private SimpleButton                       DisableModButton;
    private SimpleButton                       EnableModButton;
    private ObjectPoolGenericMono<ModListItem> ListItemPool;
    private RectTransform                      ListPart;
    private SimpleButton                       ModCommunityButton;

    private SimpleButton ModConfigureButton;
    private SimpleButton OpenModFolderButton;
    private SimpleButton ReloadModButton;
    private SimpleButton UploadModButton;

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


        GameObject list_part = BackgroundTransform.Find("Scroll View").gameObject;
        list_part.name = "List Scroll View";
        var rect_transform = list_part.GetComponent<RectTransform>();
        rect_transform.sizeDelta = new Vector2(108, 255);
        rect_transform.localPosition = new Vector3(-232, 0, 0);
        rect_transform.localScale = Vector3.one;
        var scroll_rect = list_part.GetComponent<ScrollRect>();
        scroll_rect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
        scroll_rect.verticalScrollbar.GetComponent<RectTransform>().sizeDelta = new Vector2(10, 0);
        var scroll_area_bg = list_part.GetComponent<Image>();
        scroll_area_bg.sprite = SpriteTextureLoader.getSprite("ui/special/windowEmptyFrame");
        scroll_area_bg.type = Image.Type.Sliced;
        scroll_area_bg.color = Color.white;

        BackgroundTransform.Find("Scrollgradient").GetComponent<Image>().enabled = false;

        ListPart = ContentTransform as RectTransform;
        ListItemPool = new ObjectPoolGenericMono<ModListItem>(ModListItem.Prefab, ListPart);

        var mod_info_part = new GameObject("ModInfoPart", typeof(Image), typeof(VerticalLayoutGroup));
        mod_info_part.transform.SetParent(BackgroundTransform);
        mod_info_part.transform.localPosition = new Vector3(0, 0);
        mod_info_part.transform.localScale = Vector3.one;
        mod_info_part.GetComponent<Image>().sprite = SpriteTextureLoader.getSprite("ui/special/windowInnerSliced");
        mod_info_part.GetComponent<Image>().type = Image.Type.Sliced;

        var mod_control_part = new GameObject("ModControlPart", typeof(Image), typeof(HorizontalLayoutGroup));
        mod_control_part.transform.SetParent(BackgroundTransform);
        mod_control_part.transform.localPosition = new Vector3(0, 0);
        mod_control_part.transform.localScale = Vector3.one;
        mod_control_part.GetComponent<Image>().sprite = InternalResourcesGetter.GetWindowEmptyFrame();
        mod_control_part.GetComponent<Image>().type = Image.Type.Sliced;


        var nml_general_part = new GameObject("NMLGeneralPart", typeof(Image), typeof(VerticalLayoutGroup));
        nml_general_part.transform.SetParent(BackgroundTransform);
        nml_general_part.transform.localPosition = new Vector3(0, 0);
        nml_general_part.transform.localScale = Vector3.one;
        nml_general_part.GetComponent<Image>().sprite = InternalResourcesGetter.GetWindowEmptyFrame();
        nml_general_part.GetComponent<Image>().type = Image.Type.Sliced;

        rect_transform = mod_control_part.GetComponent<RectTransform>();

        ModConfigureButton = Instantiate(SimpleButton.Prefab, rect_transform);
        ModConfigureButton.name = "ModConfigureButton";
        ModCommunityButton = Instantiate(SimpleButton.Prefab, rect_transform);
        ModCommunityButton.name = "ModCommunityButton";
        OpenModFolderButton = Instantiate(SimpleButton.Prefab, rect_transform);
        OpenModFolderButton.name = "OpenModFolderButton";
        EnableModButton = Instantiate(SimpleButton.Prefab, rect_transform);
        EnableModButton.name = "EnableModButton";
        DisableModButton = Instantiate(SimpleButton.Prefab, rect_transform);
        DisableModButton.name = "DisableModButton";
        ReloadModButton = Instantiate(SimpleButton.Prefab, rect_transform);
        ReloadModButton.name = "ReloadModButton";
        UploadModButton = Instantiate(SimpleButton.Prefab, rect_transform);
        UploadModButton.name = "UploadModButton";

        rect_transform = nml_general_part.GetComponent<RectTransform>();
    }
}