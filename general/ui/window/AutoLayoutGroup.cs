using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.General.UI.Window;

public abstract class AutoLayoutGroup<T> : AutoLayoutElement<AutoLayoutGroup<T>> where T : LayoutGroup
{
    public virtual void AddChild(GameObject pChild, int pIndex = -1)
    {
        Transform child_transform;
        (child_transform = pChild.transform).SetParent(transform);
        child_transform.localScale = Vector3.one;
        var child_count = transform.childCount;
        child_transform.SetSiblingIndex((pIndex + child_count) % child_count);
    }
    public virtual T GetLayoutGroup()
    {
        T layout_group = gameObject.GetComponent<T>();
        
        return layout_group != null ? layout_group : gameObject.AddComponent<T>();
    }
    public TSub BeginSubGroup<TSub, TSubGroup>(Vector2 pSize = default) 
        where TSub : AutoLayoutGroup<TSubGroup>
        where TSubGroup : LayoutGroup
    {
        GameObject game_object =
            new (nameof(TSubGroup), typeof(TSub), typeof(TSubGroup));

        TSub sub_group = game_object.GetComponent<TSub>();

        if (pSize != default)
        {
            sub_group.SetSize(pSize);
        }

        AddChild(game_object);

        return sub_group;
    }
    public virtual void SetSize(Vector2 pSize)
    {
        GetComponent<RectTransform>().sizeDelta = pSize;
    }
}