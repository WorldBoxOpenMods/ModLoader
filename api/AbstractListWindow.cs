using NeoModLoader.General;
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
    /// <summary>
    /// Prefab of list item
    /// </summary>
    protected static AbstractListWindowItem<TItem> ItemPrefab;

    private ObjectPoolGenericMono<AbstractListWindowItem<TItem>> _pool;

    /// <summary>
    /// A map of item to its corresponding <see cref="AbstractListWindowItem{TItem}"/>
    /// </summary>
    protected Dictionary<TItem, AbstractListWindowItem<TItem>> ItemMap = new();

    /// <summary>
    /// Add an item to the list
    /// </summary>
    /// <param name="item"></param>
    protected virtual void AddItemToList(TItem item)
    {
        if (_pool == null)
        {
            _pool = new ObjectPoolGenericMono<AbstractListWindowItem<TItem>>(ItemPrefab, ContentTransform);
        }

        if (!ItemMap.TryGetValue(item, out var item_obj))
        {
            item_obj = _pool.getNext();
            ItemMap[item] = item_obj;
        }

        item_obj.transform.localScale = Vector3.one;
        item_obj.Setup(item);
    }

    /// <summary>
    /// Remove an item from the list
    /// </summary>
    /// <param name="item"></param>
    protected virtual void RemoveItemFromList(TItem item)
    {
        if (ItemMap.TryGetValue(item, out var obj))
        {
            if (obj.gameObject.activeSelf)
            {
                obj.gameObject.SetActive(false);
            }

            _pool._elements_inactive.Enqueue(obj);
            ItemMap.Remove(item);
        }
    }

    /// <summary>
    /// Clear all items in list
    /// </summary>
    protected virtual void ClearList()
    {
        _pool?.clear();
        ItemMap.Clear();
    }

    /// <summary>
    /// Create and initilize a window instance of your subclass window
    /// </summary>
    /// <param name="pWindowId"></param>
    /// <returns></returns>
    public static new T CreateAndInit(string pWindowId)
    {
        ScrollWindow scroll_window = WindowCreator.CreateEmptyWindow(pWindowId, pWindowId + " Title");

        GameObject window_object = scroll_window.gameObject;
        Instance = window_object.AddComponent<T>();
        Instance.gameObject.SetActive(false);

        Instance.BackgroundTransform = scroll_window.transform.Find("Background");
        Instance.BackgroundTransform.Find("Scroll View").gameObject.SetActive(true);
        Instance.BackgroundTransform.Find("Scroll View").GetComponent<RectTransform>().sizeDelta =
            new Vector2(232, 270);
        Instance.BackgroundTransform.Find("Scroll View").localPosition = new Vector3(0, -6);
        Instance.BackgroundTransform.Find("Scroll View/Viewport").GetComponent<RectTransform>().sizeDelta =
            new Vector2(30, 0);
        Instance.BackgroundTransform.Find("Scroll View/Viewport").localPosition = new Vector3(-131, 135);
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

    /// <summary>
    /// You should override this to make or load your own item prefab.
    /// </summary>
    /// <returns></returns>
    protected abstract AbstractListWindowItem<TItem> CreateItemPrefab();
}