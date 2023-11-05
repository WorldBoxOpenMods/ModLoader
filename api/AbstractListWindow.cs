using NeoModLoader.General;
using RSG;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.api;

public abstract class AbstractListWindowItem<TItem> : MonoBehaviour
{
    public abstract void Setup(TItem pObject);
}

public abstract class AbstractListWindow<T, TItem> : AbstractWindow<T> 
    where T : AbstractListWindow<T, TItem>
{
    protected static AbstractListWindowItem<TItem> ItemPrefab;
    protected virtual void AddItemToList(TItem item)
    {
        AbstractListWindowItem<TItem> itemobj = Instantiate(ItemPrefab, ContentTransform);
        itemobj.transform.localScale = Vector3.one;
        itemobj.Setup(item);
        itemobj.gameObject.SetActive(true);
    }
    public static T CreateAndInit(string pWindowId)
    {
        ScrollWindow scroll_window = WindowCreator.CreateEmptyWindow(pWindowId, pWindowId + " Title");
        
        GameObject window_object = scroll_window.gameObject;
        Instance = window_object.AddComponent<T>();
        Instance.gameObject.SetActive(false);

        Instance.BackgroundTransform = scroll_window.transform.Find("Background");
        Instance.BackgroundTransform.Find("Scroll View").gameObject.SetActive(true);

        LocalizedText localized_text = Instance.BackgroundTransform.Find("Title").gameObject.GetComponent<LocalizedText>();
        if(localized_text == null){
            localized_text = Instance.BackgroundTransform.Find("Title").gameObject.AddComponent<LocalizedText>();
        }
        localized_text.key = pWindowId + " Title";

        Instance.ContentTransform = Instance.BackgroundTransform.Find("Scroll View/Viewport/Content");

        
        VerticalLayoutGroup layoutGroup = Instance.ContentTransform.gameObject.AddComponent<VerticalLayoutGroup>();
        ContentSizeFitter sizeFitter = Instance.ContentTransform.gameObject.AddComponent<ContentSizeFitter>();
        
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.spacing = 10;
        layoutGroup.padding = new(30, 30, 10, 10);

        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        ItemPrefab = Instance.CreateItemPrefab();
        Instance.Init();

        Instance.Initialized = true;
        
        return Instance;
    }
    protected abstract AbstractListWindowItem<TItem> CreateItemPrefab();
}