using NeoModLoader.General.UI.Prefabs;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.General.UI.Window;

public abstract class AutoLayoutGroup<T, TElement> : AutoLayoutElement<TElement>
    where T : LayoutGroup where TElement : AutoLayoutGroup<T, TElement>
{
    protected ContentSizeFitter m_fitter;
    protected T m_layout;

    public ContentSizeFitter fitter
    {
        get
        {
            if (m_fitter == null)
            {
                m_fitter = gameObject.GetComponent<ContentSizeFitter>();
            }

            return m_fitter;
        }
    }

    public T layout
    {
        get
        {
            if (m_layout == null)
            {
                m_layout = GetLayoutGroup();
            }

            return m_layout;
        }
    }

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
        where TSub : AutoLayoutGroup<TSubGroup, TSub>
        where TSubGroup : LayoutGroup
    {
        GameObject game_object =
            new(nameof(TSubGroup), typeof(TSub), typeof(TSubGroup));

        TSub sub_group = game_object.GetComponent<TSub>();

        if (pSize != default)
        {
            sub_group.SetSize(pSize);
        }

        AddChild(game_object);

        return sub_group;
    }

    /// <inheritdoc cref="APrefab{T}.SetSize" />
    public override void SetSize(Vector2 pSize)
    {
        GetComponent<RectTransform>().sizeDelta = pSize;
    }
}