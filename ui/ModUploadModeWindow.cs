using System.IO;
using NeoModLoader.api;
using NeoModLoader.General;
using NeoModLoader.utils;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui;

internal class ModUploadModeWindow : AbstractWindow<ModUploadModeWindow>
{
    private IMod selected_mod;
    private Image mod_icon_image;
    private Text mod_name_text;
    private Text mod_status_text;
    private Button upload_button;
    private Button update_button;
    private GameObject clear_id_btn_obj;

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
        layout.spacing = 8;
        layout.padding = new RectOffset(10, 10, 10, 10);

        ScrollWindow scrollWindow = GetComponent<ScrollWindow>();
        if (scrollWindow != null && scrollWindow.titleText != null)
        {
            scrollWindow.titleText.text = LM.Get("ModUploadModeWindow Title");
        }

        GameObject info_box = new GameObject("ModInfoBox", typeof(RectTransform));
        info_box.transform.SetParent(ContentTransform);
        info_box.transform.localScale = Vector3.one;
        info_box.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 50);

        GameObject icon_obj = new GameObject("Icon", typeof(Image));
        icon_obj.transform.SetParent(info_box.transform);
        icon_obj.transform.localScale = Vector3.one;
        icon_obj.transform.localPosition = new Vector3(-65, 0, 0);
        icon_obj.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 40);
        mod_icon_image = icon_obj.GetComponent<Image>();

        GameObject icon_frame = new GameObject("Frame", typeof(Image));
        icon_frame.transform.SetParent(icon_obj.transform);
        icon_frame.transform.localScale = Vector3.one;
        icon_frame.transform.localPosition = Vector3.zero;
        icon_frame.GetComponent<RectTransform>().sizeDelta = new Vector2(44, 44);
        Image frame_img = icon_frame.GetComponent<Image>();
        frame_img.sprite = InternalResourcesGetter.GetIconFrame();
        frame_img.type = Image.Type.Sliced;

        GameObject name_obj = new GameObject("Name", typeof(Text));
        name_obj.transform.SetParent(info_box.transform);
        name_obj.transform.localScale = Vector3.one;
        name_obj.transform.localPosition = new Vector3(20, 10, 0);
        name_obj.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 20);
        mod_name_text = name_obj.GetComponent<Text>();
        OT.InitializeCommonText(mod_name_text);
        mod_name_text.alignment = TextAnchor.MiddleLeft;
        mod_name_text.resizeTextForBestFit = true;
        mod_name_text.resizeTextMinSize = 8;
        mod_name_text.resizeTextMaxSize = 12;

        GameObject status_obj = new GameObject("Status", typeof(Text));
        status_obj.transform.SetParent(info_box.transform);
        status_obj.transform.localScale = Vector3.one;
        status_obj.transform.localPosition = new Vector3(20, -10, 0);
        status_obj.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 18);
        mod_status_text = status_obj.GetComponent<Text>();
        OT.InitializeCommonText(mod_status_text);
        mod_status_text.alignment = TextAnchor.MiddleLeft;
        mod_status_text.resizeTextForBestFit = true;
        mod_status_text.resizeTextMinSize = 6;
        mod_status_text.resizeTextMaxSize = 9;

        GameObject upload_btn_obj = new GameObject("UploadButton", typeof(Image), typeof(Button));
        upload_btn_obj.transform.SetParent(ContentTransform);
        upload_btn_obj.transform.localScale = Vector3.one;
        upload_btn_obj.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 28);
        Image upload_bg = upload_btn_obj.GetComponent<Image>();
        upload_bg.sprite = SpriteTextureLoader.getSprite("ui/special/button2");
        upload_bg.type = Image.Type.Sliced;

        GameObject upload_txt_obj = new GameObject("Text", typeof(Text));
        upload_txt_obj.transform.SetParent(upload_btn_obj.transform);
        upload_txt_obj.transform.localScale = Vector3.one;
        upload_txt_obj.transform.localPosition = Vector3.zero;
        upload_txt_obj.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 28);
        Text upload_txt = upload_txt_obj.GetComponent<Text>();
        OT.InitializeCommonText(upload_txt);
        upload_txt.alignment = TextAnchor.MiddleCenter;
        upload_txt.text = "Upload";
        upload_txt.fontSize = 10;

        upload_button = upload_btn_obj.GetComponent<Button>();
        upload_button.onClick.AddListener(OnUploadClicked);

        GameObject update_btn_obj = new GameObject("UpdateButton", typeof(Image), typeof(Button));
        update_btn_obj.transform.SetParent(ContentTransform);
        update_btn_obj.transform.localScale = Vector3.one;
        update_btn_obj.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 28);
        Image update_bg = update_btn_obj.GetComponent<Image>();
        update_bg.sprite = SpriteTextureLoader.getSprite("ui/special/button2");
        update_bg.type = Image.Type.Sliced;

        GameObject update_txt_obj = new GameObject("Text", typeof(Text));
        update_txt_obj.transform.SetParent(update_btn_obj.transform);
        update_txt_obj.transform.localScale = Vector3.one;
        update_txt_obj.transform.localPosition = Vector3.zero;
        update_txt_obj.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 28);
        Text update_txt = update_txt_obj.GetComponent<Text>();
        OT.InitializeCommonText(update_txt);
        update_txt.alignment = TextAnchor.MiddleCenter;
        update_txt.text = "Update";
        update_txt.fontSize = 10;

        update_button = update_btn_obj.GetComponent<Button>();
        update_button.onClick.AddListener(OnUpdateClicked);

        GameObject back_btn_obj = new GameObject("BackButton", typeof(Image), typeof(Button));
        back_btn_obj.transform.SetParent(ContentTransform);
        back_btn_obj.transform.localScale = Vector3.one;
        back_btn_obj.GetComponent<RectTransform>().sizeDelta = new Vector2(85, 24);
        Image back_bg = back_btn_obj.GetComponent<Image>();
        back_bg.sprite = SpriteTextureLoader.getSprite("ui/special/special_buttonred");
        back_bg.type = Image.Type.Sliced;

        GameObject back_txt_obj = new GameObject("Text", typeof(Text));
        back_txt_obj.transform.SetParent(back_btn_obj.transform);
        back_txt_obj.transform.localScale = Vector3.one;
        back_txt_obj.transform.localPosition = Vector3.zero;
        back_txt_obj.GetComponent<RectTransform>().sizeDelta = new Vector2(85, 24);
        Text back_txt = back_txt_obj.GetComponent<Text>();
        OT.InitializeCommonText(back_txt);
        back_txt.alignment = TextAnchor.MiddleCenter;
        back_txt.text = "Back";

        back_btn_obj.GetComponent<Button>().onClick.AddListener(() =>
        {
            ScrollWindow.moveAllToLeftAndRemove(false);
        });

        clear_id_btn_obj = new GameObject("ClearIdButton", typeof(Image), typeof(Button));
        clear_id_btn_obj.transform.SetParent(ContentTransform);
        clear_id_btn_obj.transform.localScale = Vector3.one;
        clear_id_btn_obj.GetComponent<RectTransform>().sizeDelta = new Vector2(110, 18);
        Image clear_id_bg = clear_id_btn_obj.GetComponent<Image>();
        clear_id_bg.sprite = SpriteTextureLoader.getSprite("ui/special/special_buttonred");
        clear_id_bg.type = Image.Type.Sliced;

        GameObject clear_id_txt_obj = new GameObject("Text", typeof(Text), typeof(LocalizedText));
        clear_id_txt_obj.transform.SetParent(clear_id_btn_obj.transform);
        clear_id_txt_obj.transform.localScale = Vector3.one;
        clear_id_txt_obj.transform.localPosition = Vector3.zero;
        clear_id_txt_obj.GetComponent<RectTransform>().sizeDelta = new Vector2(110, 18);
        Text clear_id_txt = clear_id_txt_obj.GetComponent<Text>();
        OT.InitializeCommonText(clear_id_txt);
        clear_id_txt.alignment = TextAnchor.MiddleCenter;
        clear_id_txt.text = LM.Get("Clear Steam ID");
        clear_id_txt.fontSize = 8;
        clear_id_txt.resizeTextForBestFit = true;
        clear_id_txt.resizeTextMinSize = 6;
        clear_id_txt.resizeTextMaxSize = 9;

        LocalizedText clear_id_loc = clear_id_txt_obj.GetComponent<LocalizedText>();
        clear_id_loc.key = "Clear Steam ID";
        LocalizedTextManager.addTextField(clear_id_loc);

        clear_id_btn_obj.GetComponent<Button>().onClick.AddListener(OnClearIdClicked);
    }

    private void OnUploadClicked()
    {
        if (selected_mod == null) return;
        ScrollWindow.moveAllToLeftAndRemove(false);
        ModUploadWindow.ShowWindow(selected_mod, false);
    }

    private void OnUpdateClicked()
    {
        if (selected_mod == null) return;
        var decl = selected_mod.GetDeclaration();
        if (decl.SteamId == 0)
        {
            AIConfirmWindow.ShowWindow(
                "No Steam Workshop ID found for this mod.\n\nPlease enter the Workshop ID in the upload window to update it.",
                "Enter ID",
                () =>
                {
                    ScrollWindow.moveAllToLeftAndRemove(false);
                    ModUploadWindow.ShowWindow(selected_mod, true);
                },
                "Back",
                null
            );
            return;
        }
        AskIfWantToChange();
    }

    private void AskIfWantToChange()
    {
        AIConfirmWindow.ShowWindow(
            "Do you want to change anything?",
            "Yes",
            () =>
            {
                ModUploadWindow.ShowWindow(selected_mod, true);
            },
            "No",
            () =>
            {
                AskAiDisclaimerWarning();
            }
        );
    }

    private void AskAiDisclaimerWarning()
    {
        AIConfirmWindow.ShowWindow(
            "Are you sure?\n\nIf you used AI, you need to change the disclaimer before updating.",
            "Change Disclaimer",
            () =>
            {
                ModUploadWindow.ShowWindow(selected_mod, true);
            },
            "Update",
            () =>
            {
                ModUploadWindow.ExecuteDirectUpload(selected_mod);
            }
        );
    }

    private void OnClearIdClicked()
    {
        if (selected_mod == null) return;
        var decl = selected_mod.GetDeclaration();
        if (decl.SteamId == 0) return;
        AIConfirmWindow.ShowWindow(
            LM.Get("Clear Steam ID Confirm"),
            "Yes",
            () =>
            {
                decl.SteamId = 0;
                decl.Save();
                ShowWindow(selected_mod);
            },
            "Cancel",
            null
        );
    }

    public static void ShowWindow(IMod mod)
    {
        Instance.selected_mod = mod;
        var decl = mod.GetDeclaration();
        if (string.IsNullOrEmpty(decl.IconPath) || !File.Exists(Path.Combine(decl.FolderPath, decl.IconPath)))
        {
            Instance.mod_icon_image.sprite = InternalResourcesGetter.GetIcon();
        }
        else
        {
            Instance.mod_icon_image.sprite = SpriteLoadUtils.LoadSingleSprite(Path.Combine(decl.FolderPath, decl.IconPath));
        }

        Instance.mod_name_text.text = decl.Name;
        if (decl.SteamId > 0)
        {
            Instance.mod_status_text.text = $"ID: {decl.SteamId}";
        }
        else
        {
            Instance.mod_status_text.text = "New Upload";
        }

        if (Instance.clear_id_btn_obj != null)
        {
            Instance.clear_id_btn_obj.SetActive(decl.SteamId > 0);
        }

        ScrollWindow scrollWindow = Instance.GetComponent<ScrollWindow>();
        if (scrollWindow != null && scrollWindow.titleText != null)
        {
            scrollWindow.titleText.text = LM.Get("ModUploadModeWindow Title");
            var loc = scrollWindow.titleText.GetComponent<LocalizedText>();
            if (loc != null)
            {
                loc.key = "ModUploadModeWindow Title";
                loc.updateText();
            }
        }

        ScrollWindow.showWindow(WindowId);
    }
}
