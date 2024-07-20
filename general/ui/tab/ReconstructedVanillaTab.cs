using UnityEngine;

namespace NeoModLoader.General.UI.Tab;

/// <summary>
///     Reconstructed vanilla tab, used to add button to vanilla tabs and make them looks like a vanilla button
/// </summary>
public abstract class ReconstructedVanillaTab
{
    internal WrappedPowersTab tab;
    protected abstract string[] Groups { get; }
    protected class TabElement
    {
        public Vector2 pos_in_group;
        public RectTransform element;
    }
    /// <summary>
    ///     Reconstruct a vanilla tab and group its buttons with groups
    /// </summary>
    internal void Init()
    {
        InitTab();
        return;
        var elements_groups = TrackElements();

        for (int i = 0; i < Groups.Length; i++)
        {
            var group = Groups[i];

            tab.AddGroup(group);

            var elements = elements_groups[i];
            foreach (var element in elements)
            {
                var button = element.element.GetComponent<PowerButton>();
                if (button != null)
                {
                    tab.AddPowerButton(group, button);
                }
                else
                {
                    tab.AddCustomRect(group, element.element, element.pos_in_group, true);
                }
            }
        }
        tab.UpdateLayout();
    }
    protected abstract void InitTab();
    public void AddPowerButton(string pGroupId, PowerButton pPowerButton)
    {
        tab.AddPowerButton(pGroupId, pPowerButton);
    }
    public void AddCustomRect(string pGroupId, RectTransform pCustomRect, Vector2 pPosInGroup, bool pPlaceholder)
    {
        tab.AddCustomRect(pGroupId, pCustomRect, pPosInGroup, pPlaceholder);
    }
    protected List<List<TabElement>> TrackElements()
    {
        var tab_container = tab.Tab.transform;
        int count = tab_container.childCount;

        List<Transform> all_elements = new List<Transform>();
        List<Vector2> line_positions = new List<Vector2>();

        for (int i = 0; i < count; i++)
        {
            var child = tab_container.GetChild(i);
            if (_is_line(child))
            {
                tab.RecordLine(child.gameObject);
                line_positions.Add(child.position);
            }
            else
            {
                all_elements.Add(child);
            }
        }

        all_elements.Sort((a, b) => a.position.x.CompareTo(b.position.x));
        line_positions.Sort((a, b) => a.x.CompareTo(b.x));


        List<List<TabElement>> groups = new List<List<TabElement>>();
        foreach (var line in line_positions)
        {
            List<TabElement> group = new List<TabElement>();
            foreach (var element in all_elements)
            {
                if (element.position.x < line.x)
                {
                    group.Add(new TabElement { pos_in_group = element.localPosition - new Vector3(line.x, 0), element = element.GetComponent<RectTransform>() });
                }
            }
            _sort_group(group);
            groups.Add(group);
        }
        return groups;
    }
    
    private bool _is_line(Transform pTransform)
    {
        return pTransform.name.ToLower().Contains("line");
    }
    private void _sort_group(List<TabElement> group)
    {
        throw new NotImplementedException();
    }
}