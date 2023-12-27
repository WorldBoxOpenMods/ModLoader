using NeoModLoader.api;
using NeoModLoader.General;
using NeoModLoader.services;
using NeoModLoader.utils;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui;

internal class ModUploadWindow : AbstractWindow<ModUploadWindow>
{
    private Text changelog_text;
    private Text mod_author_text;
    private Text mod_description_text;
    private Text mod_fileid_text;
    private Image mod_icon_image;
    private Text mod_name_text;
    private Text mod_version_text;
    private IMod selected_mod;

    public static void ShowWindow(IMod mod)
    {
        Instance.selected_mod = mod;
        ModDeclare mod_decl = mod.GetDeclaration();
        if (string.IsNullOrEmpty(mod_decl.IconPath))
        {
            Instance.mod_icon_image.sprite = InternalResourcesGetter.GetIcon();
        }
        else
        {
            Instance.mod_icon_image.sprite =
                SpriteLoadUtils.LoadSingleSprite(Path.Combine(mod_decl.FolderPath,
                    mod_decl.IconPath));
        }

        Instance.mod_name_text.text = mod_decl.Name;
        Instance.mod_author_text.text = mod_decl.Author;
        Instance.mod_version_text.text = mod_decl.Version;
        Instance.mod_description_text.text = mod_decl.Description;

        ScrollWindow.showWindow(WindowId);
    }

    protected override void Init()
    {
        ContentTransform.gameObject.AddComponent<ContentSizeFitter>().verticalFit =
            ContentSizeFitter.FitMode.PreferredSize;
        VerticalLayoutGroup layout = ContentTransform.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlHeight = false;
        layout.childControlWidth = false;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;
        layout.childScaleHeight = false;
        layout.childScaleWidth = false;
        layout.spacing = 10;
        layout.padding = new(0, 0, 5, 0);

        GameObject top_bar = new GameObject("TopBar", typeof(RectTransform));
        top_bar.transform.SetParent(ContentTransform);
        top_bar.transform.localScale = Vector3.one;
        top_bar.GetComponent<RectTransform>().sizeDelta = new(190, 17);

        GameObject desc_mod_icon = new GameObject("DescIcon", typeof(Image));
        desc_mod_icon.transform.SetParent(top_bar.transform);
        desc_mod_icon.transform.localPosition = new(-90, 0);
        desc_mod_icon.transform.localScale = Vector3.one;
        desc_mod_icon.GetComponent<RectTransform>().sizeDelta = new(15, 15);
        desc_mod_icon.GetComponent<Image>().sprite = InternalResourcesGetter.GetIcon();

        GameObject input_fileid = new GameObject("Input FileId", typeof(Image));
        input_fileid.transform.SetParent(top_bar.transform);
        input_fileid.transform.localScale = Vector3.one;
        input_fileid.transform.localPosition = new(5, 0);
        Image input_fileid_bg = input_fileid.GetComponent<Image>();
        input_fileid_bg.sprite = SpriteTextureLoader.getSprite("ui/special/darkInputFieldEmpty");
        input_fileid_bg.type = Image.Type.Sliced;
        GameObject input_fileid_inputfield = new GameObject("InputField", typeof(Text), typeof(InputField));
        input_fileid_inputfield.transform.SetParent(input_fileid.transform);
        input_fileid_inputfield.transform.localPosition = Vector3.zero;
        input_fileid_inputfield.transform.localScale = Vector3.one;
        Text input_fileid_inputfield_text = input_fileid_inputfield.GetComponent<Text>();
        input_fileid_inputfield.GetComponent<InputField>().textComponent = input_fileid_inputfield_text;
        input_fileid_inputfield_text.text = "";
        mod_fileid_text = input_fileid_inputfield_text;
        OT.InitializeCommonText(input_fileid_inputfield_text);
        input_fileid_inputfield_text.alignment = TextAnchor.MiddleLeft;
        input_fileid_inputfield_text.resizeTextForBestFit = true;
        input_fileid_inputfield_text.resizeTextMinSize = 6;

        GameObject input_icon = new GameObject("Image", typeof(Image));
        input_icon.transform.SetParent(input_fileid.transform);
        input_icon.transform.localPosition = new(77, 0);
        input_icon.transform.localScale = Vector3.one;
        input_icon.GetComponent<Image>().sprite = SpriteTextureLoader.getSprite("ui/special/inputFieldIcon");
        input_icon.GetComponent<RectTransform>().sizeDelta = new(15, 15);

        NameInput input_fileid_input = input_fileid.AddComponent<NameInput>();
        input_fileid_input.inputField = input_fileid_inputfield.GetComponent<InputField>();
        input_fileid_input.textField = input_fileid_inputfield_text;
        input_fileid_input.addListener((fileid) => { });

        RectTransform input_fileid_inputfield_rect = input_fileid_inputfield.GetComponent<RectTransform>();
        input_fileid_inputfield_rect.sizeDelta = new(170, 15);
        input_fileid.GetComponent<RectTransform>().sizeDelta =
            input_fileid_inputfield_rect.sizeDelta + new Vector2(2, 2);


        GameObject mod_info = new GameObject("ModInfo", typeof(Image));
        mod_info.transform.SetParent(ContentTransform);
        mod_info.transform.localPosition = new(130, -78, 0);
        mod_info.transform.localScale = Vector3.one;
        mod_info.GetComponent<Image>().sprite = SpriteTextureLoader.getSprite("ui/special/windowInnerSliced");
        mod_info.GetComponent<Image>().type = Image.Type.Sliced;
        mod_info.GetComponent<RectTransform>().sizeDelta = new(190, 95);

        GameObject mod_icon = new GameObject("ModIcon", typeof(Image));
        mod_icon.transform.SetParent(mod_info.transform);
        mod_icon.transform.localScale = Vector3.one;
        mod_icon.transform.localPosition = new(-48, 0);
        mod_icon.GetComponent<RectTransform>().sizeDelta = new(90, 90);
        mod_icon_image = mod_icon.GetComponent<Image>();
        GameObject mod_icon_frame = new GameObject("ModIconFrame", typeof(Image));
        mod_icon_frame.transform.SetParent(mod_icon.transform);
        mod_icon_frame.GetComponent<Image>().sprite = InternalResourcesGetter.GetIconFrame();
        mod_icon_frame.GetComponent<Image>().type = Image.Type.Sliced;
        mod_icon_frame.GetComponent<RectTransform>().sizeDelta = mod_icon.GetComponent<RectTransform>().sizeDelta;

        GameObject info_grids = new GameObject("InfoGrids", typeof(GridLayoutGroup));
        info_grids.transform.SetParent(mod_info.transform);
        info_grids.transform.localScale = Vector3.one;
        info_grids.transform.localPosition = new(48, 0);
        info_grids.GetComponent<RectTransform>().sizeDelta = new(92, 92);
        GridLayoutGroup info_grids_layout = info_grids.GetComponent<GridLayoutGroup>();
        info_grids_layout.childAlignment = TextAnchor.UpperCenter;
        info_grids_layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        info_grids_layout.constraintCount = 1;
        info_grids_layout.spacing = new Vector2(0, 1);
        info_grids_layout.cellSize = new Vector2(92, 15);

        Text create_grid_text(string name)
        {
            Text _tmp = new GameObject(name, typeof(Text)).GetComponent<Text>();
            Transform transform1;
            (transform1 = _tmp.transform).SetParent(info_grids.transform);
            transform1.localScale = Vector3.one;

            OT.InitializeCommonText(_tmp);
            _tmp.resizeTextForBestFit = true;
            _tmp.resizeTextMaxSize = 10;
            _tmp.resizeTextMinSize = 6;
            _tmp.text = name;
            _tmp.alignment = TextAnchor.MiddleLeft;
            return _tmp;
        }

        mod_name_text = create_grid_text("Mod Name");
        mod_author_text = create_grid_text("Mod Author");
        mod_version_text = create_grid_text("Mod Version");
        mod_description_text = create_grid_text("Mod Description");


        GameObject input_changelog = new GameObject("Input ChangeLog", typeof(Image));
        input_changelog.transform.SetParent(ContentTransform);
        input_changelog.transform.localScale = Vector3.one;
        input_changelog.transform.localPosition = new(130f, -170f);

        Image input_changelog_bg = input_changelog.GetComponent<Image>();
        input_changelog_bg.sprite = SpriteTextureLoader.getSprite("ui/special/darkInputFieldEmpty");
        input_changelog_bg.type = Image.Type.Sliced;
        GameObject input_changelog_inputfield = new GameObject("InputField", typeof(Text), typeof(InputField));
        input_changelog_inputfield.transform.SetParent(input_changelog.transform);
        input_changelog_inputfield.transform.localScale = Vector3.one;
        input_changelog_inputfield.transform.localPosition = Vector3.zero;
        Text input_changelog_inputfield_text = input_changelog_inputfield.GetComponent<Text>();
        input_changelog_inputfield.GetComponent<InputField>().textComponent = input_changelog_inputfield_text;
        input_changelog_inputfield_text.text = "#CHANGELOG";
        changelog_text = input_changelog_inputfield_text;
        OT.InitializeCommonText(input_changelog_inputfield_text);
        input_changelog_inputfield_text.alignment = TextAnchor.UpperLeft;
        input_changelog_inputfield_text.resizeTextForBestFit = true;
        input_changelog_inputfield_text.resizeTextMinSize = 6;
        input_changelog_inputfield_text.resizeTextMaxSize = 10;
        input_changelog_inputfield.GetComponent<InputField>().lineType = InputField.LineType.MultiLineNewline;

        NameInput input_changelog_input = input_changelog.AddComponent<NameInput>();
        input_changelog_input.inputField = input_changelog_inputfield.GetComponent<InputField>();
        input_changelog_input.textField = input_changelog_inputfield_text;
        input_changelog_input.addListener((fileid) => { });

        RectTransform input_changelog_inputfield_rect = input_changelog_inputfield.GetComponent<RectTransform>();
        input_changelog_inputfield_rect.sizeDelta = new(190, 80);
        input_changelog.GetComponent<RectTransform>().sizeDelta =
            input_changelog_inputfield_rect.sizeDelta + new Vector2(2, 2);

        GameObject upload_button = new GameObject("UploadButton", typeof(Image), typeof(Button));
        upload_button.transform.SetParent(ContentTransform);
        upload_button.transform.localPosition = new(130, -260);
        upload_button.transform.localScale = Vector3.one;
        upload_button.GetComponent<RectTransform>().sizeDelta = new(190, 30);
        Image upload_button_bg = upload_button.GetComponent<Image>();
        upload_button_bg.sprite = SpriteTextureLoader.getSprite("ui/special/special_buttonred");
        upload_button_bg.type = Image.Type.Sliced;
        GameObject upload_button_desc_left = new GameObject("Desc1", typeof(Image));
        upload_button_desc_left.transform.SetParent(upload_button.transform);
        upload_button_desc_left.transform.localPosition = new(-80, 0);
        upload_button_desc_left.transform.localScale = Vector3.one;
        upload_button_desc_left.GetComponent<RectTransform>().sizeDelta = new(30, 30);
        upload_button_desc_left.GetComponent<Image>().sprite = SpriteTextureLoader.getSprite("ui/icons/iconSaveCloud");
        GameObject upload_button_desc_right = new GameObject("Desc2", typeof(Image));
        upload_button_desc_right.transform.SetParent(upload_button.transform);
        upload_button_desc_right.transform.localPosition = new(80, 0);
        upload_button_desc_right.transform.localScale = Vector3.one;
        upload_button_desc_right.GetComponent<RectTransform>().sizeDelta = new(30, 30);
        upload_button_desc_right.GetComponent<Image>().sprite = SpriteTextureLoader.getSprite("ui/icons/iconSteam");
        GameObject upload_button_text = new GameObject("Text", typeof(Text), typeof(LocalizedText));
        upload_button_text.transform.SetParent(upload_button.transform);
        upload_button_text.transform.localPosition = Vector3.zero;
        upload_button_text.transform.localScale = Vector3.one;
        upload_button_text.GetComponent<RectTransform>().sizeDelta = new(190, 30);
        Text upload_button_text_text = upload_button_text.GetComponent<Text>();
        OT.InitializeCommonText(upload_button_text_text);
        upload_button_text_text.alignment = TextAnchor.MiddleCenter;
        LocalizedText upload_button_text_localized = upload_button_text.GetComponent<LocalizedText>();
        upload_button_text_localized.key = "ModUpload Title";

        upload_button.GetComponent<Button>().onClick.AddListener(uploadSelectedMod);
        LocalizedTextManager.addTextField(upload_button_text_localized);
    }

    private void uploadSelectedMod()
    {
        string fileId = mod_fileid_text.text;
        if (fileId.Any(c => !char.IsDigit(c)))
        {
            fileId = null;
        }

        if (string.IsNullOrEmpty(fileId))
        {
            ModUploadAuthenticationService.Authenticate().Then(
                () => ModWorkshopService.UploadMod(selected_mod, changelog_text.text,
                    ModUploadAuthenticationService.Authed)).Then(ModUploadingProgressWindow.FinishUpload,
                ModUploadingProgressWindow.ErrorUpload);
            return;
        }

        ulong fileIdLong = ulong.Parse(fileId);
        ModWorkshopService.TryEditMod(fileIdLong, selected_mod, changelog_text.text)
            .Then(ModUploadingProgressWindow.FinishUpload, ModUploadingProgressWindow.ErrorUpload).Done();
    }
}