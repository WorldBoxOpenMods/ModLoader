using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.General.UI.Window.Layout;

public class AutoVertLayoutGroup : AutoLayoutGroup<VerticalLayoutGroup, AutoVertLayoutGroup>
{
    public void Setup(Vector2 pSize = default, TextAnchor pAlignment = TextAnchor.UpperCenter, float pSpacing = 3,
        RectOffset pPadding = null)
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

        layout.childAlignment = pAlignment;
        layout.spacing = pSpacing;
        layout.padding = pPadding ?? new RectOffset(3, 3, 3, 3);
    }

    internal static void _init()
    {
        GameObject game_object =
            new(nameof(AutoVertLayoutGroup), typeof(VerticalLayoutGroup), typeof(AutoVertLayoutGroup),
                typeof(ContentSizeFitter));
        game_object.transform.SetParent(WorldBoxMod.Transform);

        ContentSizeFitter fitter = game_object.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        var layout_group = game_object.GetComponent<VerticalLayoutGroup>();
        layout_group.childAlignment = TextAnchor.UpperCenter;
        layout_group.childControlHeight = false;
        layout_group.childControlWidth = false;
        layout_group.childForceExpandHeight = false;
        layout_group.childForceExpandWidth = false;
        layout_group.childScaleHeight = false;
        layout_group.childScaleWidth = false;
        layout_group.spacing = 3;
        layout_group.padding = new RectOffset(3, 3, 3, 3);

        Prefab = game_object.GetComponent<AutoVertLayoutGroup>();
    }
}