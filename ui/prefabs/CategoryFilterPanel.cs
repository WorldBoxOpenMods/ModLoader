using NeoModLoader.constants;
using NeoModLoader.General;
using NeoModLoader.General.UI;
using NeoModLoader.General.UI.Prefabs;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui.prefabs;

/// <summary>
///     Toggles for the Workshop categories of <see cref="CoreConstants.ModCategories" />. Several can be selected;
///     none selected means any category.
/// </summary>
internal class CategoryFilterPanel : MonoBehaviour
{
    private readonly Dictionary<string, SimpleButton> toggles = new();
    private Action on_changed;
    private HashSet<string> selected;
    private GridLayoutGroup grid;

    /// <param name="pParent">The list's scroll view; the panel covers the top of it</param>
    /// <param name="pSelected">Selected categories, changed in place</param>
    /// <param name="pOnChanged">Called after every change</param>
    public static CategoryFilterPanel Create(Transform pParent, HashSet<string> pSelected, Action pOnChanged)
    {
        var obj = new GameObject(nameof(CategoryFilterPanel), typeof(Image), typeof(VerticalLayoutGroup));
        obj.transform.SetParent(pParent, false);
        obj.GetComponent<Image>().sprite = SpriteTextureLoader.getSprite("ui/special/windowInnerSlicedRound");
        obj.GetComponent<Image>().type = Image.Type.Sliced;
        var rect = obj.GetComponent<RectTransform>();
        rect.Stretch();
        var layout = obj.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 4;
        layout.padding = new RectOffset(8, 8, 8, 8);
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        CategoryFilterPanel panel = obj.AddComponent<CategoryFilterPanel>();
        panel.selected = pSelected;
        panel.on_changed = pOnChanged;

        // Keep every category reachable even when the list viewport is shorter than the panel's contents.
        var viewport = new GameObject("Categories", typeof(RectTransform), typeof(Image), typeof(Mask),
            typeof(ScrollRect));
        viewport.transform.SetParent(obj.transform, false);
        viewport.GetComponent<Image>().color = Color.white;
        viewport.GetComponent<Mask>().showMaskGraphic = false;
        viewport.SetLayoutSize(pFlexibleHeight: 1);

        var grid = new GameObject("Grid", typeof(GridLayoutGroup), typeof(ContentSizeFitter));
        grid.transform.SetParent(viewport.transform, false);
        var grid_rect = grid.GetComponent<RectTransform>();
        grid_rect.anchorMin = new Vector2(0, 1);
        grid_rect.anchorMax = Vector2.one;
        grid_rect.pivot = new Vector2(0.5f, 1);
        grid_rect.sizeDelta = Vector2.zero;
        grid.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        ScrollRect scroll = viewport.GetComponent<ScrollRect>();
        scroll.viewport = (RectTransform)viewport.transform;
        scroll.content = grid_rect;
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 15;
        panel.grid = grid.GetComponent<GridLayoutGroup>();
        panel.grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        panel.grid.constraintCount = 2;
        panel.grid.spacing = new Vector2(4, 4);
        panel.grid.cellSize = new Vector2(0, 20);

        foreach (string category in CoreConstants.ModCategories)
        {
            SimpleButton toggle = SimpleButton.Instantiate(grid.transform, pName: category);
            // Keep the Workshop tag for filtering and use its ModTag locale key for display.
            string locale_key = "ModTag_" + category;
            toggle.Setup(() => panel.Toggle(category), null, LM.Get(locale_key));
            toggle.Text.gameObject.AddComponent<LocalizedText>().setKeyAndUpdate(locale_key);
            toggle.Background.sprite = SpriteTextureLoader.getSprite("ui/special/special_buttonGray");
            toggle.Text.resizeTextForBestFit = false;
            toggle.Text.fontSize = 8;
            // The grid sizes the button; the text keeps the size Setup gave it unless stretched.
            RectTransform text_rect = toggle.Text.rectTransform;
            text_rect.Stretch(4);
            panel.toggles[category] = toggle;
            toggle.gameObject.AddComponent<ScrollableButton>();
        }

        ModHubToolbar footer = ModHubToolbar.Create(obj.transform, "Actions");
        Sprite action_background = ResourcesFinder.FindResource<PowerButton>("world_laws").GetComponent<Image>().sprite;
        footer.AddText("Any", "mod_hub_category_any", panel.Clear).Background.sprite = action_background;
        footer.AddText("Done", "mod_hub_done", () => obj.SetActive(false)).Background.sprite = action_background;
        panel.ShowSelection();
        obj.SetActive(false);
        return panel;
    }

    private void OnEnable()
    {
        // Two columns across the panel. Its width is only known once the window's layout has run.
        if (grid == null) return;
        float width = ((RectTransform)transform).rect.width - 16;
        grid.cellSize = new Vector2(Mathf.Max(0, (width - 4) / 2), 20);
    }

    private void OnRectTransformDimensionsChange() => OnEnable();

    private void Toggle(string pCategory)
    {
        if (!selected.Remove(pCategory)) selected.Add(pCategory);
        ShowSelection();
        on_changed();
    }

    private void Clear()
    {
        selected.Clear();
        ShowSelection();
        on_changed();
    }

    private void ShowSelection()
    {
        foreach (KeyValuePair<string, SimpleButton> toggle in toggles)
        {
            bool on = selected.Contains(toggle.Key);
            toggle.Value.Background.sprite = SpriteTextureLoader.getSprite(on
                ? "ui/special/special_buttonRed" : "ui/special/special_buttonGray");
            toggle.Value.Text.color = Color.white;
        }
    }
}
