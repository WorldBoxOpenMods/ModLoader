using NeoModLoader.General;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.api;
/// <summary>
/// An abstract window class that should be only one instance.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class AbstractWindow<T> : MonoBehaviour where T : AbstractWindow<T>
{
    /// <summary>
    /// The only instance of this class.
    /// </summary>
    public static T Instance { get; protected set; }
    /// <summary>
    /// Transform of Background/Scroll View/Viewport/Content of Instance
    /// </summary>
    protected Transform ContentTransform { get; set; }
    /// <summary>
    /// Transform of Background of Instance
    /// </summary>
    protected Transform BackgroundTransform { get; set; }
    /// <summary>
    /// It will be set to true after <see cref="Init"/> is called.
    /// </summary>
    protected bool Initialized;
    /// <summary>
    /// It will be set to true after <see cref="OnFirstEnable"/> and <see cref="OnNormalEnable"/> called.
    /// </summary>
    protected bool IsOpened;
    /// <summary>
    /// It will be set to false after <see cref="OnFirstEnable"/> called.
    /// </summary>
    protected bool IsFirstOpen = true;
    /// <summary>
    /// WindowId of <see cref="Instance"/>
    /// </summary>
    public static string WindowId { get; protected set; }

    public static T CreateAndInit(string pWindowId)
    {
        WindowId = pWindowId;
        ScrollWindow scroll_window = WindowCreator.CreateEmptyWindow(pWindowId, pWindowId + " Title");
        
        GameObject window_object = scroll_window.gameObject;
        Instance = window_object.AddComponent<T>();
        Instance.gameObject.SetActive(false);

        Instance.BackgroundTransform = scroll_window.transform.Find("Background");
        Instance.BackgroundTransform.Find("Scroll View").gameObject.SetActive(true);

        Instance.ContentTransform = Instance.BackgroundTransform.Find("Scroll View/Viewport/Content");

        Instance.Init();

        Instance.Initialized = true;
        
        return Instance;
    }

    protected abstract void Init();

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