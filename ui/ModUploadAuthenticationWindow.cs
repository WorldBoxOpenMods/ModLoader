using NeoModLoader.api;
using NeoModLoader.services;
using NeoModLoader.utils;
using NeoModLoader.utils.authentication;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui;

public class ModUploadAuthenticationWindow : AbstractWindow<ModUploadAuthenticationWindow>
{
    private static Button prefab_auth_button;
    protected override void Init()
    {
        GridLayoutGroup layoutGroup = ContentTransform.gameObject.AddComponent<GridLayoutGroup>();
        layoutGroup.cellSize = new Vector2(48, 48);
        layoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layoutGroup.constraintCount = 3;
        layoutGroup.spacing = new Vector2(5, 5);
        layoutGroup.padding = new RectOffset(5, 5, 5, 5);
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        
        GameObject auth_button_obj = new GameObject("AuthButton", typeof(Image), typeof(Button));
        auth_button_obj.transform.SetParent(WorldBoxMod.Transform);
        prefab_auth_button = auth_button_obj.GetComponent<Button>();
        prefab_auth_button.image.sprite = SpriteTextureLoader.getSprite("ui/special/special_buttonred");
        prefab_auth_button.image.type = Image.Type.Sliced;
        GameObject auth_button_icon_obj = new GameObject("Icon", typeof(Image));
        auth_button_icon_obj.transform.SetParent(auth_button_obj.transform);
        auth_button_icon_obj.transform.localPosition = Vector3.zero;
        auth_button_icon_obj.transform.localScale = Vector3.one;
        auth_button_icon_obj.GetComponent<RectTransform>().sizeDelta = new Vector2(42, 42);

        CreateAuthButton("ui/icons/iconDiscordWhite", () => false, new (42, 30.7f));
        CreateAuthButton(InternalResourcesGetter.GetGitHubIcon(), GithubOrgAuthUtils.Authenticate);
        CreateAuthButton("ui/icons/iconArrowBack", null);
    }
    internal Func<bool> AuthFunc;
    internal bool AuthFuncSelected = false;
    private Button CreateAuthButton(Sprite pIcon, Func<bool> pAuthFunc, Vector2 pIconSize = default)
    {
        Button button = Instantiate(prefab_auth_button, ContentTransform);
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
        return button;
    }
    private Button CreateAuthButton(string pIconPath, Func<bool> pAuthFunc, Vector2 pIconSize = default)
    {
        return CreateAuthButton(SpriteTextureLoader.getSprite(pIconPath), pAuthFunc, pIconSize);
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
        AuthSkipped = true;
    }
}