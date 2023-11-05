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
    public static T Instance { get; private set; }
    protected static Transform ContentTransform { get; private set; }
    protected static Transform BackgroundTransform { get; private set; }
    protected bool Initialized;
    protected bool IsOpened;
    protected bool IsFirstOpen = true;

    public static T CreateAndInit(string pWindowId)
    {
        ScrollWindow scroll_window = WindowCreator.CreateEmptyWindow(pWindowId, pWindowId + " Title");
        
        GameObject window_object = scroll_window.gameObject;
        Instance = window_object.AddComponent<T>();
        Instance.gameObject.SetActive(false);

        BackgroundTransform = scroll_window.transform.Find("Background");
        BackgroundTransform.Find("Scroll View").gameObject.SetActive(true);

        LocalizedText localized_text = BackgroundTransform.Find("Title").gameObject.GetComponent<LocalizedText>();
        if(localized_text == null){
            localized_text = BackgroundTransform.Find("Title").gameObject.AddComponent<LocalizedText>();
        }
        localized_text.key = pWindowId + " Title";

        ContentTransform = BackgroundTransform.Find("Scroll View/Viewport/Content");

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