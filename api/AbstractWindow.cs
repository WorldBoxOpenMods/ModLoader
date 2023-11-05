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
    public static T Instance { get; protected set; }
    protected Transform ContentTransform { get; set; }
    protected Transform BackgroundTransform { get; set; }
    protected bool Initialized;
    protected bool IsOpened;
    protected bool IsFirstOpen = true;

    public static T CreateAndInit(string pWindowId)
    {
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