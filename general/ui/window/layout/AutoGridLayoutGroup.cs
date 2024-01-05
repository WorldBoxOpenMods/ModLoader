using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.General.UI.Window.Layout;

public class AutoGridLayoutGroup : AutoLayoutGroup<GridLayoutGroup, AutoGridLayoutGroup>
{
    public void Setup(
        int pConstraintCount,
        GridLayoutGroup.Constraint pConstraint = GridLayoutGroup.Constraint.FixedColumnCount,
        Vector2 pSize = default,
        Vector2 pCellSize = default,
        Vector2 pSpacing = default,
        GridLayoutGroup.Axis pStartAxis = GridLayoutGroup.Axis.Horizontal,
        GridLayoutGroup.Corner pStartCorner = GridLayoutGroup.Corner.UpperLeft)
    {
        Init();
        if (pSize == default)
        {
            fitter.enabled = true;
        }
        else
        {
            fitter.enabled = false;
            GetComponent<RectTransform>().sizeDelta = pSize;
        }

        layout.constraint = pConstraint;
        layout.constraintCount = pConstraintCount;

        layout.cellSize = pCellSize == default ? new Vector2(16, 16) : pCellSize;
        layout.spacing = pSpacing == default ? new Vector2(3, 3) : pSpacing;

        layout.startAxis = pStartAxis;
        layout.startCorner = pStartCorner;
    }

    internal static void _init()
    {
        GameObject game_object =
            new(nameof(AutoGridLayoutGroup), typeof(GridLayoutGroup), typeof(AutoGridLayoutGroup),
                typeof(ContentSizeFitter));

        ContentSizeFitter fitter = game_object.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        var layout_group = game_object.GetComponent<GridLayoutGroup>();

        layout_group.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout_group.constraintCount = 3;

        layout_group.cellSize = new Vector2(16, 16);
        layout_group.spacing = new Vector2(3, 3);

        layout_group.startAxis = GridLayoutGroup.Axis.Horizontal;
        layout_group.startCorner = GridLayoutGroup.Corner.UpperLeft;


        Prefab = game_object.GetComponent<AutoGridLayoutGroup>();
    }
}