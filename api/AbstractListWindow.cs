using NeoModLoader.General;
using RSG;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.api;
/// <summary>
/// Abstract List Window Item
/// </summary>
/// <typeparam name="TItem">The type of the parameter passed into to setup the item</typeparam>
public abstract class AbstractListWindowItem<TItem> : MonoBehaviour
{
    /// <summary>
    /// Configure the item with the given object before added to the list
    /// </summary>
    public abstract void Setup(TItem pObject);
}
/// <summary>
/// An abstract window that contains a list of items.
/// <para> Items are layout automatically </para>
/// <para> Scroll View size fits to Items automatically </para>
/// </summary>
/// <remarks>
/// You should create a subclass of this class, and call CreateAndInit to create a window.
/// <para>In addition, you need to create a subclass of <see cref="AbstractListWindowItem{TItem}"/> for setup each item of the list </para>
/// </remarks>
/// <example><see cref="ui.ModListWindow"/></example>
/// <typeparam name="T">The type of the class which inherits this class </typeparam>
/// <typeparam name="TItem">The type of object passed into AbstractListWindowItem.Setup as parameter </typeparam>
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