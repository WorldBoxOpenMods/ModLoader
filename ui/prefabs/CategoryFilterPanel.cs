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

    /// <param name="pParent">Window background</param>
    /// <param name="pSelected">Selected categories, changed in place</param>
    /// <param name="pOnChanged">Called after every change</param>
    public static CategoryFilterPanel Create(Transform pParent, HashSet<string> pSelected, Action pOnChanged)
    {
        var obj = new GameObject(nameof(CategoryFilterPanel), typeof(Image));
        obj.transform.SetParent(pParent, false);
        obj.transform.localPosition = new Vector3(0, 22);
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(216, 126);
        Image background = obj.GetComponent<Image>();
        background.sprite = SpriteTextureLoader.getSprite("ui/special/darkInputFieldEmpty");
        background.type = Image.Type.Sliced;

        CategoryFilterPanel panel = obj.AddComponent<CategoryFilterPanel>();
        panel.selected = pSelected;
        panel.on_changed = pOnChanged;

        string[] categories = CoreConstants.ModCategories;
        for (int i = 0; i < categories.Length; i++)
        {
            string category = categories[i];
            SimpleButton toggle = SimpleButton.Instantiate(obj.transform, pName: category);
            // Category names are Workshop tags, shown as uploaders chose them.
            toggle.Setup(() => panel.Toggle(category), null, category, new Vector2(100, 16));
            toggle.transform.localPosition = new Vector3(i % 2 == 0 ? -52 : 52, 48 - i / 2 * 20);
            panel.toggles[category] = toggle;
        }

        ModHubUI.CreateTextButton(obj.transform, "Any", "mod_hub_category_any", new Vector2(100, 16),
            new Vector2(0, -50), panel.Clear);
        panel.ShowSelection();
        obj.SetActive(false);
        return panel;
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
            toggle.Value.Background.color = selected.Contains(toggle.Key) ? Color.white : Color.gray;
        }
    }
}
