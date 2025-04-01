using NeoModLoader.General.UI.Window.Layout;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.General.UI.Window;

public abstract class AutoLayoutWindow<T> : AutoVertLayoutGroup where T : AutoLayoutWindow<T>
{
    /// <summary>
    /// It will be set to true after <see cref="Init"/> is called.
    /// </summary>
    protected new bool Initialized;

    /// <summary>
    /// It will be set to false after <see cref="OnFirstEnable"/> called.
    /// </summary>
    protected bool IsFirstOpen = true;

    /// <summary>
    /// It will be set to true after <see cref="OnFirstEnable"/> and <see cref="OnNormalEnable"/> called.
    /// </summary>
    protected bool IsOpened;

    /// <summary>
    /// Component ScrollWindow of Instance for easy click show/hide/back
    /// </summary>
    protected ScrollWindow ScrollWindowComponent { get; set; }

    /// <summary>
    /// Transform of Background/Scroll View/Viewport/Content of Instance
    /// </summary>
    protected Transform ContentTransform { get; set; }

    /// <summary>
    /// Transform of Background of Instance
    /// </summary>
    protected Transform BackgroundTransform { get; set; }

    /// <summary>
    /// WindowID of Instance
    /// </summary>
    protected internal string WindowID { get; set; }

    private void OnEnable()
    {
        if (!Initialized) return;
        if (IsFirstOpen)
        {
            IsFirstOpen = false;
            OnFirstEnable();
        }

        OnNormalEnable();
        IsOpened = true;
    }

    private void OnDisable()
    {
        if (!Initialized) return;
        IsOpened = false;
        OnNormalDisable();
    }

    public static T CreateWindow(string pWindowID, string pWindowTitleKey)
    {
        ScrollWindow window = WindowCreator.CreateEmptyWindow(pWindowID, pWindowTitleKey);

        window.gameObject.SetActive(false);

        window.transform_content.gameObject.AddComponent<VerticalLayoutGroup>();
        T auto_layout_window = window.transform_content.gameObject.AddComponent<T>();

        auto_layout_window.BackgroundTransform = window.transform.Find("Background");
        window.transform_scrollRect.gameObject.SetActive(true);

        auto_layout_window.ContentTransform = window.transform_content;
        auto_layout_window.ScrollWindowComponent = window;

        var layout_group = auto_layout_window.GetLayoutGroup();

        layout_group.childAlignment = TextAnchor.UpperCenter;
        layout_group.childControlHeight = false;
        layout_group.childControlWidth = false;
        layout_group.childForceExpandHeight = false;
        layout_group.childForceExpandWidth = false;
        layout_group.childScaleHeight = false;
        layout_group.childScaleWidth = false;
        layout_group.spacing = 10;
        layout_group.padding = new RectOffset(3, 3, 10, 10);

        ContentSizeFitter fitter = window.transform_content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        auto_layout_window.WindowID = pWindowID;
        auto_layout_window.Init();

        auto_layout_window.Initialized = true;

        return auto_layout_window;
    }

    protected abstract new void Init();

    public static void Reconstruct(ref T pWindow)
    {
        pWindow.ScrollWindowComponent.clickHide();
        ScrollWindow._all_windows.Remove(pWindow.WindowID);

        string pWindowID = pWindow.WindowID;
        string pWindowTitleKey = pWindow.ScrollWindowComponent.titleText.GetComponent<LocalizedText>().key;

        Destroy(pWindow.ScrollWindowComponent.gameObject);
        pWindow = CreateWindow(pWindowID, pWindowTitleKey);
    }

    public virtual void OnNormalDisable()
    {
    }

    public virtual void OnFirstEnable()
    {
    }

    public virtual void OnNormalEnable()
    {
    }
}