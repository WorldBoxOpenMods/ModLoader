using NeoModLoader.constants;
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
        GameObject obj = ModHubStyle.Box(pParent, nameof(CategoryFilterPanel), ModHubStyle.Card);
        var rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 1);
        rect.sizeDelta = Vector2.zero;
        ModHubStyle.Vertical(obj, ModHubStyle.SpaceS, ModHubStyle.SpaceM).childForceExpandWidth = true;
        obj.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        CategoryFilterPanel panel = obj.AddComponent<CategoryFilterPanel>();
        panel.selected = pSelected;
        panel.on_changed = pOnChanged;

        var grid = new GameObject("Grid", typeof(GridLayoutGroup));
        grid.transform.SetParent(obj.transform, false);
        panel.grid = grid.GetComponent<GridLayoutGroup>();
        panel.grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        panel.grid.constraintCount = 2;
        panel.grid.spacing = new Vector2(ModHubStyle.SpaceS, ModHubStyle.SpaceS);
        panel.grid.cellSize = new Vector2(0, ModHubStyle.ButtonHeight);

        foreach (string category in CoreConstants.ModCategories)
        {
            SimpleButton toggle = SimpleButton.Instantiate(grid.transform, pName: category);
            // Category names are Workshop tags, shown as uploaders chose them.
            toggle.Setup(() => panel.Toggle(category), null, category);
            toggle.Background.sprite = ModHubStyle.Button;
            toggle.Text.resizeTextForBestFit = false;
            toggle.Text.fontSize = ModHubStyle.FontBody;
            // The grid sizes the button; the text keeps the size Setup gave it unless stretched.
            RectTransform text_rect = toggle.Text.rectTransform;
            text_rect.anchorMin = Vector2.zero;
            text_rect.anchorMax = Vector2.one;
            text_rect.offsetMin = new Vector2(ModHubStyle.SpaceS, 0);
            text_rect.offsetMax = new Vector2(-ModHubStyle.SpaceS, 0);
            panel.toggles[category] = toggle;
        }

        ModHubStyle.TextButton(obj.transform, "Any", "mod_hub_category_any", panel.Clear);
        panel.ShowSelection();
        obj.SetActive(false);
        return panel;
    }

    private void OnEnable()
    {
        // Two columns across the panel. Its width is only known once the window's layout has run.
        if (grid == null) return;
        float width = ((RectTransform)transform).rect.width - 2 * ModHubStyle.SpaceM;
        grid.cellSize = new Vector2((width - ModHubStyle.SpaceS) / 2, ModHubStyle.ButtonHeight);
    }

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
            toggle.Value.Background.color = on ? Color.white : Color.gray;
            toggle.Value.Text.color = on ? ModHubStyle.Positive : ModHubStyle.TextMuted;
        }
    }
}
