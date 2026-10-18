using System;
using System.IO;
using NeoModLoader.api;
using NeoModLoader.constants;
using NeoModLoader.General;
using NeoModLoader.services;
using NeoModLoader.utils;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui;

internal class AIDisclosureWindow : AbstractWindow<AIDisclosureWindow>
{
    private Action onConfirmAction;
    private IMod selected_mod;

    private Image preview_icon_image;
    private Image preview_badge_image;
    private RectTransform preview_badge_rect;

    private Text ai_disclosure_text;
    private Text ai_corner_text;
    private Text ai_style_text;

    private GameObject corner_btn_obj;
    private GameObject style_btn_obj;

    private string current_attribution = "not_ai";
    private string current_corner = "bottom_right";
    private string current_style = "standard";

    private GameObject checklist_container;
    private Text toggle_code_text;
    private Text toggle_art_text;
    private Text toggle_trans_text;
    private Text toggle_audio_text;

    private Image toggle_code_bg;
    private Image toggle_art_bg;
    private Image toggle_trans_bg;
    private Image toggle_audio_bg;

    private bool check_code;
    private bool check_art;
    private bool check_translation;
    private bool check_audio;

    private Text warning_text;

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
        layout.spacing = 6;
        layout.padding = new RectOffset(6, 6, 6, 6);

        ScrollWindow scrollWindow = GetComponent<ScrollWindow>();
        if (scrollWindow != null && scrollWindow.titleText != null)
        {
            scrollWindow.titleText.text = LM.Get("AIDisclosureWindow Title");
        }

        GameObject preview_box = new GameObject("LivePreviewBox", typeof(RectTransform));
        preview_box.transform.SetParent(ContentTransform);
        preview_box.transform.localScale = Vector3.one;
        preview_box.GetComponent<RectTransform>().sizeDelta = new Vector2(90, 90);

        GameObject preview_frame = new GameObject("Frame", typeof(Image));
        preview_frame.transform.SetParent(preview_box.transform);
        preview_frame.transform.localScale = Vector3.one;
        preview_frame.transform.localPosition = Vector3.zero;
        preview_frame.GetComponent<RectTransform>().sizeDelta = new Vector2(90, 90);
        Image frame_img = preview_frame.GetComponent<Image>();
        frame_img.sprite = InternalResourcesGetter.GetIconFrame();
        frame_img.type = Image.Type.Sliced;

        GameObject preview_icon = new GameObject("ModIcon", typeof(Image));
        preview_icon.transform.SetParent(preview_box.transform);
        preview_icon.transform.localScale = Vector3.one;
        preview_icon.transform.localPosition = Vector3.zero;
        preview_icon.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 80);
        preview_icon_image = preview_icon.GetComponent<Image>();

        GameObject preview_badge = new GameObject("BadgeOverlay", typeof(Image));
        preview_badge.transform.SetParent(preview_icon.transform);
        preview_badge.transform.localScale = Vector3.one;
        preview_badge_image = preview_badge.GetComponent<Image>();
        preview_badge_rect = preview_badge.GetComponent<RectTransform>();

        GameObject disc_btn = new GameObject("AIDisclosureButton", typeof(Image), typeof(Button));
        disc_btn.transform.SetParent(ContentTransform);
        disc_btn.transform.localScale = Vector3.one;
        disc_btn.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 22);
        Image disc_bg = disc_btn.GetComponent<Image>();
        disc_bg.sprite = SpriteTextureLoader.getSprite("ui/special/button2");
        disc_bg.type = Image.Type.Sliced;

        GameObject disc_txt_obj = new GameObject("Text", typeof(Text));
        disc_txt_obj.transform.SetParent(disc_btn.transform);
        disc_txt_obj.transform.localScale = Vector3.one;
        disc_txt_obj.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 22);
        ai_disclosure_text = disc_txt_obj.GetComponent<Text>();
        OT.InitializeCommonText(ai_disclosure_text);
        ai_disclosure_text.alignment = TextAnchor.MiddleCenter;
        ai_disclosure_text.resizeTextForBestFit = true;
        ai_disclosure_text.resizeTextMinSize = 6;
        ai_disclosure_text.resizeTextMaxSize = 11;
        disc_btn.GetComponent<Button>().onClick.AddListener(cycleAttribution);

        corner_btn_obj = new GameObject("AICornerButton", typeof(Image), typeof(Button));
        corner_btn_obj.transform.SetParent(ContentTransform);
        corner_btn_obj.transform.localScale = Vector3.one;
        corner_btn_obj.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 22);
        Image corner_bg = corner_btn_obj.GetComponent<Image>();
        corner_bg.sprite = SpriteTextureLoader.getSprite("ui/special/button2");
        corner_bg.type = Image.Type.Sliced;

        GameObject corner_txt_obj = new GameObject("Text", typeof(Text));
        corner_txt_obj.transform.SetParent(corner_btn_obj.transform);
        corner_txt_obj.transform.localScale = Vector3.one;
        corner_txt_obj.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 22);
        ai_corner_text = corner_txt_obj.GetComponent<Text>();
        OT.InitializeCommonText(ai_corner_text);
        ai_corner_text.alignment = TextAnchor.MiddleCenter;
        ai_corner_text.resizeTextForBestFit = true;
        ai_corner_text.resizeTextMinSize = 6;
        ai_corner_text.resizeTextMaxSize = 11;
        corner_btn_obj.GetComponent<Button>().onClick.AddListener(cycleCorner);

        style_btn_obj = new GameObject("AIStyleButton", typeof(Image), typeof(Button));
        style_btn_obj.transform.SetParent(ContentTransform);
        style_btn_obj.transform.localScale = Vector3.one;
        style_btn_obj.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 22);
        Image style_bg = style_btn_obj.GetComponent<Image>();
        style_bg.sprite = SpriteTextureLoader.getSprite("ui/special/button2");
        style_bg.type = Image.Type.Sliced;

        GameObject style_txt_obj = new GameObject("Text", typeof(Text));
        style_txt_obj.transform.SetParent(style_btn_obj.transform);
        style_txt_obj.transform.localScale = Vector3.one;
        style_txt_obj.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 22);
        ai_style_text = style_txt_obj.GetComponent<Text>();
        OT.InitializeCommonText(ai_style_text);
        ai_style_text.alignment = TextAnchor.MiddleCenter;
        ai_style_text.resizeTextForBestFit = true;
        ai_style_text.resizeTextMinSize = 6;
        ai_style_text.resizeTextMaxSize = 11;
        style_btn_obj.GetComponent<Button>().onClick.AddListener(cycleStyle);

        checklist_container = new GameObject("ChecklistGroup", typeof(RectTransform), typeof(VerticalLayoutGroup));
        checklist_container.transform.SetParent(ContentTransform);
        checklist_container.transform.localScale = Vector3.one;
        checklist_container.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 105);
        VerticalLayoutGroup checkLayout = checklist_container.GetComponent<VerticalLayoutGroup>();
        checkLayout.childAlignment = TextAnchor.UpperCenter;
        checkLayout.childControlHeight = false;
        checkLayout.childControlWidth = false;
        checkLayout.childForceExpandHeight = false;
        checkLayout.childForceExpandWidth = false;
        checkLayout.spacing = 3;

        GameObject check_title_obj = new GameObject("ChecklistTitle", typeof(Text));
        check_title_obj.transform.SetParent(checklist_container.transform);
        check_title_obj.transform.localScale = Vector3.one;
        check_title_obj.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 16);
        Text check_title = check_title_obj.GetComponent<Text>();
        OT.InitializeCommonText(check_title);
        check_title.alignment = TextAnchor.MiddleCenter;
        check_title.text = "AI Components:";
        check_title.fontSize = 10;

        (toggle_code_bg, toggle_code_text) = createChecklistToggle("ToggleCode", "Code / Logic", () =>
        {
            check_code = !check_code;
            updateChecklistUI();
        });

        (toggle_art_bg, toggle_art_text) = createChecklistToggle("ToggleArt", "Textures / Sprites", () =>
        {
            check_art = !check_art;
            updateChecklistUI();
        });

        (toggle_trans_bg, toggle_trans_text) = createChecklistToggle("ToggleTrans", "Translations / Text", () =>
        {
            check_translation = !check_translation;
            updateChecklistUI();
        });

        (toggle_audio_bg, toggle_audio_text) = createChecklistToggle("ToggleAudio", "Audio / Music", () =>
        {
            check_audio = !check_audio;
            updateChecklistUI();
        });

        GameObject warn_obj = new GameObject("WarningText", typeof(Text));
        warn_obj.transform.SetParent(ContentTransform);
        warn_obj.transform.localScale = Vector3.one;
        warn_obj.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 24);
        warning_text = warn_obj.GetComponent<Text>();
        OT.InitializeCommonText(warning_text);
        warning_text.alignment = TextAnchor.MiddleCenter;
        warning_text.resizeTextForBestFit = true;
        warning_text.resizeTextMinSize = 6;
        warning_text.resizeTextMaxSize = 9;
        warning_text.text = "Steam Workshop requires accurate AI disclosure.";

        GameObject buttonGroup = new GameObject("Buttons", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        buttonGroup.transform.SetParent(ContentTransform);
        buttonGroup.transform.localScale = Vector3.one;
        buttonGroup.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 25);

        HorizontalLayoutGroup hLayout = buttonGroup.GetComponent<HorizontalLayoutGroup>();
        hLayout.childAlignment = TextAnchor.MiddleCenter;
        hLayout.spacing = 10;

        GameObject backButtonObj = new GameObject("BackButton", typeof(Image), typeof(Button));
        backButtonObj.transform.SetParent(buttonGroup.transform);
        backButtonObj.transform.localScale = Vector3.one;
        backButtonObj.GetComponent<RectTransform>().sizeDelta = new Vector2(85, 25);
        Image backBg = backButtonObj.GetComponent<Image>();
        backBg.sprite = SpriteTextureLoader.getSprite("ui/special/special_buttonred");
        backBg.type = Image.Type.Sliced;

        Text backText = new GameObject("Text", typeof(Text)).GetComponent<Text>();
        backText.transform.SetParent(backButtonObj.transform);
        backText.transform.localScale = Vector3.one;
        backText.GetComponent<RectTransform>().sizeDelta = new Vector2(85, 25);
        OT.InitializeCommonText(backText);
        backText.alignment = TextAnchor.MiddleCenter;
        backText.text = "Back";

        backButtonObj.GetComponent<Button>().onClick.AddListener(() =>
        {
            GetComponent<ScrollWindow>().clickHide();
        });

        GameObject continueButtonObj = new GameObject("ContinueButton", typeof(Image), typeof(Button));
        continueButtonObj.transform.SetParent(buttonGroup.transform);
        continueButtonObj.transform.localScale = Vector3.one;
        continueButtonObj.GetComponent<RectTransform>().sizeDelta = new Vector2(85, 25);
        Image continueBg = continueButtonObj.GetComponent<Image>();
        continueBg.sprite = SpriteTextureLoader.getSprite("ui/special/button2");
        continueBg.type = Image.Type.Sliced;

        Text continueText = new GameObject("Text", typeof(Text)).GetComponent<Text>();
        continueText.transform.SetParent(continueButtonObj.transform);
        continueText.transform.localScale = Vector3.one;
        continueText.GetComponent<RectTransform>().sizeDelta = new Vector2(85, 25);
        OT.InitializeCommonText(continueText);
        continueText.alignment = TextAnchor.MiddleCenter;
        continueText.text = "Continue";

        continueButtonObj.GetComponent<Button>().onClick.AddListener(confirmAndContinue);
    }

    private (Image, Text) createChecklistToggle(string name, string label, Action onClick)
    {
        GameObject btn_obj = new GameObject(name, typeof(Image), typeof(Button));
        btn_obj.transform.SetParent(checklist_container.transform);
        btn_obj.transform.localScale = Vector3.one;
        btn_obj.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 20);
        Image bg = btn_obj.GetComponent<Image>();
        bg.sprite = SpriteTextureLoader.getSprite("ui/special/darkInputFieldEmpty");
        bg.type = Image.Type.Sliced;

        GameObject txt_obj = new GameObject("Text", typeof(Text));
        txt_obj.transform.SetParent(btn_obj.transform);
        txt_obj.transform.localScale = Vector3.one;
        txt_obj.GetComponent<RectTransform>().sizeDelta = new Vector2(190, 20);
        Text text = txt_obj.GetComponent<Text>();
        OT.InitializeCommonText(text);
        text.alignment = TextAnchor.MiddleCenter;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 6;
        text.resizeTextMaxSize = 10;
        text.text = label;

        btn_obj.GetComponent<Button>().onClick.AddListener(() => onClick?.Invoke());
        return (bg, text);
    }

    private void confirmAndContinue()
    {
        if (selected_mod != null)
        {
            var decl = selected_mod.GetDeclaration();
            decl.AIAttribution = current_attribution;
            decl.AIBadgeCorner = current_corner;
            decl.AIBadgeStyle = current_style;
            decl.AICheckCode = check_code;
            decl.AICheckArt = check_art;
            decl.AICheckTranslation = check_translation;
            decl.AICheckAudio = check_audio;
            decl.Save();
        }

        string tagsSummary = "";
        var activeTags = selected_mod?.GetDeclaration().Tags;
        if (activeTags != null && activeTags.Count > 0)
        {
            tagsSummary = $"\nTags: {string.Join(", ", activeTags)}\n";
        }

        string warning = $"Warning: Steam Workshop policy requires accurate AI disclosure.\n\n" +
                         $"This mod will be published with '{selected_mod?.GetDeclaration().GetAIAttributionDisplay()}' tag." +
                         $"{tagsSummary}\n" +
                         $"Misrepresenting AI content may lead to mod removal by Workshop moderators.\n\n" +
                         $"Do you wish to continue?";

        AIConfirmWindow.ShowWindow(warning, onConfirmAction);
    }

    private void cycleAttribution()
    {
        current_attribution = current_attribution switch
        {
            "not_ai" => "ai_assisted",
            "ai_assisted" => "ai_made",
            _ => "not_ai"
        };
        updateAttributionText();
        updateChecklistUI();
        updateLivePreview();
    }

    private void cycleCorner()
    {
        current_corner = current_corner switch
        {
            "bottom_right" => "top_right",
            "top_right" => "top_left",
            "top_left" => "bottom_left",
            _ => "bottom_right"
        };
        updateCornerText();
        updateLivePreview();
    }

    private void cycleStyle()
    {
        current_style = current_style switch
        {
            "standard" => "icon_only",
            "icon_only" => "watermark",
            _ => "standard"
        };
        updateStyleText();
        updateLivePreview();
    }

    private void updateAttributionText()
    {
        if (ai_disclosure_text == null) return;
        string display = current_attribution switch
        {
            "ai_made" => "AI-Made",
            "ai_assisted" => "AI-Assisted",
            _ => "Human Authored"
        };
        ai_disclosure_text.text = "AI Disclosure: " + display;

        if (warning_text != null)
        {
            warning_text.text = current_attribution == "not_ai"
                ? "Handcrafted mod disclosure."
                : "Steam Workshop requires accurate AI disclosure.";
        }
    }

    private void updateCornerText()
    {
        if (ai_corner_text == null) return;
        string display = current_corner switch
        {
            "top_right" => "Top Right",
            "top_left" => "Top Left",
            "bottom_left" => "Bottom Left",
            _ => "Bottom Right"
        };
        ai_corner_text.text = "Badge Corner: " + display;
    }

    private void updateStyleText()
    {
        if (ai_style_text == null) return;
        string display = current_style switch
        {
            "icon_only" => "Icon Stamp",
            "watermark" => "Watermark",
            _ => "Standard"
        };
        ai_style_text.text = "Badge Style: " + display;
    }

    private void updateLivePreview()
    {
        if (preview_badge_image == null || preview_badge_rect == null) return;

        if (!CoreConstants.EnableBadges)
        {
            preview_badge_image.gameObject.SetActive(false);
            return;
        }

        preview_badge_image.gameObject.SetActive(true);

        bool isIconOnly = string.Equals(current_style, "icon_only", StringComparison.OrdinalIgnoreCase);
        bool isWatermark = string.Equals(current_style, "watermark", StringComparison.OrdinalIgnoreCase);

        string spritePath = isIconOnly ? current_attribution switch
        {
            "ai_made" => "i_made.png",
            "ai_assisted" => "i_ast.png",
            _ => "i_na.png"
        } : current_attribution switch
        {
            "ai_made" => "b_made.png",
            "ai_assisted" => "b_ast.png",
            _ => "b_na.png"
        };

        preview_badge_image.sprite = InternalResourcesGetter.GetBadgeSprite(spritePath);

        if (isIconOnly)
        {
            preview_badge_rect.sizeDelta = new Vector2(24, 24);
            preview_badge_image.color = Color.white;
        }
        else if (isWatermark)
        {
            preview_badge_rect.sizeDelta = new Vector2(58, 15);
            preview_badge_image.color = new Color(1f, 1f, 1f, 0.4f);
        }
        else
        {
            preview_badge_rect.sizeDelta = new Vector2(58, 15);
            preview_badge_image.color = Color.white;
        }

        switch (current_corner?.ToLower())
        {
            case "top_left":
                preview_badge_rect.anchorMin = new Vector2(0, 1);
                preview_badge_rect.anchorMax = new Vector2(0, 1);
                preview_badge_rect.pivot = new Vector2(0, 1);
                preview_badge_rect.anchoredPosition = new Vector2(2, -2);
                break;
            case "top_right":
                preview_badge_rect.anchorMin = new Vector2(1, 1);
                preview_badge_rect.anchorMax = new Vector2(1, 1);
                preview_badge_rect.pivot = new Vector2(1, 1);
                preview_badge_rect.anchoredPosition = new Vector2(-2, -2);
                break;
            case "bottom_left":
                preview_badge_rect.anchorMin = new Vector2(0, 0);
                preview_badge_rect.anchorMax = new Vector2(0, 0);
                preview_badge_rect.pivot = new Vector2(0, 0);
                preview_badge_rect.anchoredPosition = new Vector2(2, 2);
                break;
            case "bottom_right":
            default:
                preview_badge_rect.anchorMin = new Vector2(1, 0);
                preview_badge_rect.anchorMax = new Vector2(1, 0);
                preview_badge_rect.pivot = new Vector2(1, 0);
                preview_badge_rect.anchoredPosition = new Vector2(-2, 2);
                break;
        }
    }

    private void updateChecklistUI()
    {
        bool showChecklist = current_attribution == "ai_assisted" || current_attribution == "ai_made";
        if (checklist_container != null)
        {
            checklist_container.SetActive(showChecklist);
        }

        setToggleVisual(toggle_code_bg, toggle_code_text, "Code / Logic", check_code);
        setToggleVisual(toggle_art_bg, toggle_art_text, "Textures / Sprites", check_art);
        setToggleVisual(toggle_trans_bg, toggle_trans_text, "Translations / Text", check_translation);
        setToggleVisual(toggle_audio_bg, toggle_audio_text, "Audio / Music", check_audio);
    }

    private void setToggleVisual(Image bg, Text text, string label, bool active)
    {
        if (text != null)
        {
            text.text = active ? $"<color=#A0FFA0>✓</color> {label}" : $"<color=#888888>{label}</color>";
        }
        if (bg != null)
        {
            bg.sprite = SpriteTextureLoader.getSprite("ui/special/darkInputFieldEmpty");
            bg.type = Image.Type.Sliced;
            bg.color = active ? new Color(0.2f, 0.65f, 0.25f, 0.85f) : new Color(0.18f, 0.18f, 0.18f, 0.7f);
        }
    }

    public static void ShowWindow(IMod pMod, Action pOnConfirm)
    {
        Instance.selected_mod = pMod;
        Instance.onConfirmAction = pOnConfirm;

        if (pMod != null)
        {
            var decl = pMod.GetDeclaration();
            Instance.current_attribution = decl.AIAttribution ?? "not_ai";
            Instance.current_corner = decl.AIBadgeCorner ?? "bottom_right";
            Instance.current_style = decl.AIBadgeStyle ?? "standard";
            Instance.check_code = decl.AICheckCode;
            Instance.check_art = decl.AICheckArt;
            Instance.check_translation = decl.AICheckTranslation;
            Instance.check_audio = decl.AICheckAudio;

            if (string.IsNullOrEmpty(decl.IconPath))
            {
                Instance.preview_icon_image.sprite = InternalResourcesGetter.GetIcon();
            }
            else
            {
                Instance.preview_icon_image.sprite = SpriteLoadUtils.LoadSingleSprite(Path.Combine(decl.FolderPath, decl.IconPath));
            }
        }

        Instance.corner_btn_obj.SetActive(CoreConstants.EnableBadges);
        Instance.style_btn_obj.SetActive(CoreConstants.EnableBadges);

        Instance.updateAttributionText();
        Instance.updateCornerText();
        Instance.updateStyleText();
        Instance.updateLivePreview();
        Instance.updateChecklistUI();

        ScrollWindow.showWindow(WindowId);
    }
}
