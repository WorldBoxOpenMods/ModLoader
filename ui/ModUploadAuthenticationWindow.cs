using NeoModLoader.api;
using NeoModLoader.General;
using NeoModLoader.services;
using NeoModLoader.utils;
using NeoModLoader.utils.authentication;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui;

public class ModUploadAuthenticationWindow : AbstractWindow<ModUploadAuthenticationWindow>
{
    private static Button prefab_auth_button;
    private Transform auth_grid_transform;
    private LocalizedText localized_auth_text;
    private Text auth_text;
    protected override void Init()
    {
        VerticalLayoutGroup verticalLayoutGroup = ContentTransform.gameObject.AddComponent<VerticalLayoutGroup>();
        verticalLayoutGroup.childAlignment = TextAnchor.UpperCenter;
        verticalLayoutGroup.childControlHeight = false;
        verticalLayoutGroup.childControlWidth = false;
        verticalLayoutGroup.childForceExpandHeight = false;
        verticalLayoutGroup.childForceExpandWidth = false;
        verticalLayoutGroup.spacing = 5;
        verticalLayoutGroup.padding = new RectOffset(5, 5, 5, 5);
        
        GameObject auth_text_obj = new GameObject("AuthText", typeof(Text), typeof(LocalizedText));
        auth_text_obj.transform.SetParent(ContentTransform);
        auth_text_obj.transform.localScale = Vector3.one;
        auth_text_obj.GetComponent<RectTransform>().sizeDelta = new(190, 50);
        auth_text = auth_text_obj.GetComponent<Text>();
        OT.InitializeCommonText(auth_text);
        auth_text.alignment = TextAnchor.MiddleCenter;
        auth_text.resizeTextForBestFit = true;
        auth_text.resizeTextMinSize = 6;
        auth_text.resizeTextMaxSize = 14;
        auth_text.color = Color.white;
        localized_auth_text = auth_text_obj.GetComponent<LocalizedText>();
        localized_auth_text.setKeyAndUpdate("NML_AUTHENTICATION");
        LocalizedTextManager.addTextField(localized_auth_text);
        
        
        GameObject auth_grid_obj = new GameObject("AuthGrid", typeof(GridLayoutGroup));
        auth_grid_obj.transform.SetParent(ContentTransform);
        auth_grid_obj.transform.localScale = Vector3.one;
        auth_grid_obj.GetComponent<RectTransform>().sizeDelta = new(200, 100);
        auth_grid_transform = auth_grid_obj.transform;
        GridLayoutGroup layoutGroup = auth_grid_obj.GetComponent<GridLayoutGroup>();
        layoutGroup.cellSize = new Vector2(48, 48);
        layoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layoutGroup.constraintCount = 3;
        layoutGroup.spacing = new Vector2(5, 5);
        layoutGroup.padding = new RectOffset(5, 5, 5, 5);
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        
        GameObject auth_button_obj = new GameObject("AuthButton", typeof(Image), typeof(Button), typeof(TipButton));
        auth_button_obj.transform.SetParent(WorldBoxMod.Transform);
        prefab_auth_button = auth_button_obj.GetComponent<Button>();
        prefab_auth_button.image.sprite = SpriteTextureLoader.getSprite("ui/special/special_buttonred");
        prefab_auth_button.image.type = Image.Type.Sliced;
        GameObject auth_button_icon_obj = new GameObject("Icon", typeof(Image));
        auth_button_icon_obj.transform.SetParent(auth_button_obj.transform);
        auth_button_icon_obj.transform.localPosition = Vector3.zero;
        auth_button_icon_obj.transform.localScale = Vector3.one;
        auth_button_icon_obj.GetComponent<RectTransform>().sizeDelta = new Vector2(42, 42);
        

        CreateAuthButton("DiscordAuth", "ui/icons/iconDiscordWhite", DiscordRoleAuthViaUserLoginUtils.Authenticate, new (42, 30.7f));
        CreateAuthButton("GithubAuth", InternalResourcesGetter.GetGitHubIcon(), GithubOrgAuthUtils.Authenticate);
        CreateAuthButton("SkipAuth", "ui/icons/iconArrowBack", null);
    }
    internal static List<Func<bool>> all_auto_auth_funcs = new()
    {
        () =>
        {
            // It runs in a new thread, so it's safe to block the thread and unnecessary to catch exception.
            while (true)
            {
                if (!string.IsNullOrEmpty(Config.discordId))
                {
                    return DiscordAutomaticRoleAuthUtils.Authenticate();
                }
                if (DiscordTracker.userTries <= 0)
                {
                    return false;
                }
                Thread.Sleep(10000);
            }
        }
    };
    /// <summary>
    /// The function to be called when the button is clicked. Methods in it might throw an AuthenticationException if something goes wrong with the authentication process.
    /// </summary>
    internal Func<bool> AuthFunc;
    internal bool AuthFuncSelected = false;
    private Button CreateAuthButton(string pId, Sprite pIcon, Func<bool> pAuthFunc, Vector2 pIconSize = default)
    {
        Button button = Instantiate(prefab_auth_button, auth_grid_transform);
        button.transform.Find("Icon").GetComponent<Image>().sprite = pIcon;
        if (pIconSize != default)
        {
            button.transform.Find("Icon").GetComponent<RectTransform>().sizeDelta = pIconSize;
        }
        button.onClick.AddListener(() =>
        {
            if (pAuthFunc != null)
            {
                AuthFunc = pAuthFunc;
                AuthFuncSelected = true;
            }
            else
            {
                AuthSkipped = true;
            }
        });
        var tip_button = button.GetComponent<TipButton>();
        tip_button.textOnClick = pId + " Title";
        tip_button.text_description_2 = pId + " Description";
        return button;
    }
    private Button CreateAuthButton(string pId, string pIconPath, Func<bool> pAuthFunc, Vector2 pIconSize = default)
    {
        return CreateAuthButton(pId, SpriteTextureLoader.getSprite(pIconPath), pAuthFunc, pIconSize);
    }

    public static void SetState(bool pAuthState, string pTipText = null)
    {
        Instance.auth_text.color = pAuthState ? Color.green : Color.red;
        Instance.localized_auth_text.setKeyAndUpdate(pAuthState ? "NML_AUTHENTICATED" : "NML_AUTHENTICATION_FAILED");
        
        if(!string.IsNullOrEmpty(pTipText))
        {
            Instance.auth_text.text += $"\n{pTipText}";
        }
    }
    public bool Opened()
    {
        return this.IsOpened;
    }
    public override void OnNormalEnable()
    {
        base.OnNormalEnable();
        AuthSkipped = false;
        AuthFuncSelected = false;
        AuthFunc = null;
    }

    internal bool AuthSkipped;
    public override void OnNormalDisable()
    {
        base.OnNormalDisable();
    }
}