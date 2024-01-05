using NeoModLoader.General.UI.Window.Layout;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.General.UI.Window.Utils.Extensions;

/// <summary>
///     This class is used to extend AutoLayoutGroup with different given layout groups
/// </summary>
public static class AutoLayoutGroupExtension
{
    /// <summary>
    ///     Begin a horizontal layout group
    /// </summary>
    /// <param name="pThis"></param>
    /// <param name="pSize"></param>
    /// <param name="pAlignment"></param>
    /// <param name="pSpacing"></param>
    /// <param name="pPadding"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    /// <returns></returns>
    public static AutoHoriLayoutGroup BeginHoriGroup<T, TElement>(
        this AutoLayoutGroup<T, TElement> pThis,
        Vector2 pSize = default,
        TextAnchor pAlignment = TextAnchor.MiddleLeft,
        float pSpacing = 3,
        RectOffset pPadding = null
    )
        where T : LayoutGroup where TElement : AutoLayoutGroup<T, TElement>
    {
        var auto_layout_group = pThis.BeginSubGroup<AutoHoriLayoutGroup, HorizontalLayoutGroup>(pSize);

        if (pSize == default)
        {
            ContentSizeFitter fitter = auto_layout_group.gameObject.GetComponent<ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = auto_layout_group.gameObject.AddComponent<ContentSizeFitter>();
            }

            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        var layout_group = auto_layout_group.GetLayoutGroup();

        layout_group.childAlignment = pAlignment;
        layout_group.childControlHeight = false;
        layout_group.childControlWidth = false;
        layout_group.childForceExpandHeight = false;
        layout_group.childForceExpandWidth = false;
        layout_group.childScaleHeight = false;
        layout_group.childScaleWidth = false;
        layout_group.spacing = pSpacing;
        layout_group.padding = pPadding ?? new RectOffset(3, 3, 3, 3);

        return auto_layout_group;
    }

    /// <summary>
    ///     Begin a vertical layout group
    /// </summary>
    /// <param name="pThis"></param>
    /// <param name="pSize"></param>
    /// <param name="pAlignment"></param>
    /// <param name="pSpacing"></param>
    /// <param name="pPadding"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    /// <returns></returns>
    public static AutoVertLayoutGroup BeginVertGroup<T, TElement>(this AutoLayoutGroup<T, TElement> pThis,
        Vector2 pSize = default,
        TextAnchor pAlignment = TextAnchor.UpperCenter,
        float pSpacing = 3,
        RectOffset pPadding = null)
        where T : LayoutGroup where TElement : AutoLayoutGroup<T, TElement>
    {
        var auto_layout_group = pThis.BeginSubGroup<AutoVertLayoutGroup, VerticalLayoutGroup>(pSize);
        if (pSize == default)
        {
            ContentSizeFitter fitter = auto_layout_group.gameObject.GetComponent<ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = auto_layout_group.gameObject.AddComponent<ContentSizeFitter>();
            }

            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        var layout_group = auto_layout_group.GetLayoutGroup();

        layout_group.childAlignment = pAlignment;
        layout_group.childControlHeight = false;
        layout_group.childControlWidth = false;
        layout_group.childForceExpandHeight = false;
        layout_group.childForceExpandWidth = false;
        layout_group.childScaleHeight = false;
        layout_group.childScaleWidth = false;
        layout_group.spacing = pSpacing;
        layout_group.padding = pPadding ?? new RectOffset(3, 3, 3, 3);

        return auto_layout_group;
    }

    /// <summary>
    ///     Begin a grid layout group
    /// </summary>
    /// <param name="pThis"></param>
    /// <param name="pConstraintCount"></param>
    /// <param name="pConstraint"></param>
    /// <param name="pSize"></param>
    /// <param name="pCellSize"></param>
    /// <param name="pSpacing"></param>
    /// <param name="pStartAxis"></param>
    /// <param name="pStartCorner"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    /// <returns></returns>
    public static AutoGridLayoutGroup BeginGridGroup<T, TElement>(this AutoLayoutGroup<T, TElement> pThis,
        int pConstraintCount,
        GridLayoutGroup.Constraint pConstraint = GridLayoutGroup.Constraint.FixedColumnCount,
        Vector2 pSize = default,
        Vector2 pCellSize = default,
        Vector2 pSpacing = default,
        GridLayoutGroup.Axis pStartAxis = GridLayoutGroup.Axis.Horizontal,
        GridLayoutGroup.Corner pStartCorner = GridLayoutGroup.Corner.UpperLeft
    )
        where T : LayoutGroup where TElement : AutoLayoutGroup<T, TElement>
    {
        var auto_layout_group = pThis.BeginSubGroup<AutoGridLayoutGroup, GridLayoutGroup>(pSize);
        if (pSize == default)
        {
            ContentSizeFitter fitter = auto_layout_group.gameObject.GetComponent<ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = auto_layout_group.gameObject.AddComponent<ContentSizeFitter>();
            }

            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        var layout_group = auto_layout_group.GetLayoutGroup();

        layout_group.constraint = pConstraint;
        layout_group.constraintCount = pConstraintCount;

        layout_group.cellSize = pCellSize == default ? new Vector2(16, 16) : pCellSize;
        layout_group.spacing = pSpacing == default ? new Vector2(3, 3) : pSpacing;

        layout_group.startAxis = pStartAxis;
        layout_group.startCorner = pStartCorner;

        return auto_layout_group;
    }
}