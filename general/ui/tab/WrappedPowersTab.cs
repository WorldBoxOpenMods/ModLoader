using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.General.UI.Tab;

internal class WrappedPowersTab
{
    private const float space = 4;
    private const float tab_start_x = 87.4f;
    private const float assumed_button_size = 32;

    private static readonly RectTransform _empty_button_placehold =
        new GameObject("Empty Button Placehold", typeof(RectTransform)).GetComponent<RectTransform>();

    private static readonly float[] available_y = new float[2] { 18, -18 };
    private Queue<GameObject> _active_lines;
    private Queue<GameObject> _inactive_lines;
    private Dictionary<string, List<PowerButton>> ButtonGroups;
    private Dictionary<string, List<WrappedRectTransform>> CustomRectGroups;
    public bool Modifiable;
    public PowersTab Tab;

    public WrappedPowersTab(PowersTab pPowersTab)
    {
        Tab = pPowersTab;

        Modifiable = !PowerTabNames.GetNames().Contains(Tab.name);

        ButtonGroups = new();
        CustomRectGroups = new();

        AddGroup("Default");

        _inactive_lines = new();
        _active_lines = new();
    }

    internal void RecordLine(GameObject line)
    {
        _active_lines.Enqueue(line);
    }

    public static void _init()
    {
        _empty_button_placehold.SetParent(WorldBoxMod.Transform);
    }

    public bool HasGroup(string pGroupId)
    {
        return ButtonGroups.ContainsKey(pGroupId);
    }

    public void AddPowerButton(string pGroupId, PowerButton pPowerButton)
    {
        var group = ButtonGroups[pGroupId];

        if (pPowerButton != null)
        {
            Transform transform;
            (transform = pPowerButton.transform).SetParent(Tab.transform);
            transform.localScale = Vector3.one;
        }

        group.Add(pPowerButton);
    }

    public void AddCustomRect(string pGroupId, RectTransform pRect, Vector2 pPositionInGroup, bool pPlacehold)
    {
        var group = CustomRectGroups[pGroupId];

        pRect.SetParent(Tab.transform);
        pRect.localScale = Vector3.one;

        group.Add(new(pRect, pPositionInGroup, pPlacehold));
    }

    public void AddGroup(string pGroupId)
    {
        ButtonGroups.Add(pGroupId, new());
        CustomRectGroups.Add(pGroupId, new());
    }

    public void ResetGroups()
    {
        ButtonGroups.Clear();
    }

    public void UpdateLayout()
    {
        foreach (var active_line in _active_lines)
        {
            active_line.SetActive(false);
            _inactive_lines.Enqueue(active_line);
        }

        float group_start_x = tab_start_x;
        bool first_line = true;
        foreach (string group_id in ButtonGroups.Keys)
        {
            var button_group = ButtonGroups[group_id];
            var rect_group = CustomRectGroups[group_id];

            if (button_group.Count > 0 || rect_group.Count > 0)
            {
                group_start_x += space * 2;
                if (!first_line)
                    _add_line(group_start_x);
                else
                {
                    first_line = false;
                }

                group_start_x += space * 2;
            }
            else
            {
                continue;
            }

            PlaceholdRegions placehold_regions = new();
            foreach (var rect in rect_group)
            {
                rect.Rect.localPosition = rect.PositionInGroup + new Vector2(group_start_x, 0);
                if (rect.Placehold)
                {
                    placehold_regions.AddRegion(rect.Rect);
                }
            }

            bool check_up_y = true;
            foreach (var button in button_group)
            {
                RectTransform button_rect =
                    button == null ? _empty_button_placehold : button.GetComponent<RectTransform>();
                if (button == null)
                {
                    button_rect.sizeDelta = new Vector2(assumed_button_size, assumed_button_size);
                    button_rect.pivot = new(0.5f, 0.5f);
                }

                bool found = false;
                while (!found)
                {
                    if (check_up_y)
                    {
                        group_start_x += assumed_button_size / 2;
                        button_rect.localPosition = new(group_start_x, available_y[0]);
                        check_up_y = false;
                        if (!placehold_regions.Overlap(button_rect))
                        {
                            found = true;
                        }
                    }
                    else
                    {
                        button_rect.localPosition = new(group_start_x, available_y[1]);
                        check_up_y = true;
                        group_start_x += space + assumed_button_size / 2;
                        if (!placehold_regions.Overlap(button_rect))
                        {
                            found = true;
                        }
                    }
                }
            }

            if (check_up_y)
            {
                group_start_x += space;
            }
            else
            {
                group_start_x += assumed_button_size / 2;
            }
        }

        Tab._power_buttons.Clear();
        foreach (PowerButton power_button in Tab.GetComponentsInChildren<PowerButton>())
        {
            if (!(power_button == null) && !(power_button.rect_transform == null))
            {
                Tab._power_buttons.Add(power_button);
            }
        }

        foreach (PowerButton power_button in Tab._power_buttons)
        {
            power_button.findNeighbours(Tab._power_buttons);
        }
    }

    private void _add_line(float pX)
    {
        GameObject line;
        if (_inactive_lines.Count > 0)
        {
            line = _inactive_lines.Dequeue();
        }
        else
        {
            line = GameObject.Instantiate(ResourcesFinder.FindResource<GameObject>("_line"), Tab.transform);
            line.GetComponent<Image>().enabled = true;
            line.transform.localScale = new(1, 48.3f, 1);
        }

        line.SetActive(true);
        line.transform.localPosition = new(pX, 37.2f);
        _active_lines.Enqueue(line);
    }

    private class WrappedRectTransform
    {
        public readonly bool Placehold;
        public readonly Vector2 PositionInGroup;
        public readonly RectTransform Rect;

        public WrappedRectTransform(RectTransform pRect, Vector2 pPositionInGroup, bool pPlacehold)
        {
            Rect = pRect;
            PositionInGroup = pPositionInGroup;
            Placehold = pPlacehold;
        }
    }

    private class PlaceholdRegions
    {
        private HashSet<SimpleRegion> _regions = new();

        public void AddRegion(RectTransform pRect)
        {
            _regions.Add(new SimpleRegion(pRect));
        }

        public bool Overlap(RectTransform pRect)
        {
            var rect = pRect.rect;
            foreach (SimpleRegion region in _regions)
            {
                if (region.Contains(rect.xMin, rect.yMin)
                    || region.Contains(rect.xMin, rect.yMax)
                    || region.Contains(rect.xMax, rect.yMin)
                    || region.Contains(rect.xMax, rect.yMax))
                {
                    return true;
                }
            }

            return false;
        }

        private class SimpleRegion
        {
            public readonly Vector2 LeftUpCorner;
            public readonly Vector2 RightDownCorner;

            public SimpleRegion(RectTransform pRect)
            {
                var rect = pRect.rect;
                LeftUpCorner = new Vector2(rect.xMin, rect.yMax);
                RightDownCorner = new Vector2(rect.xMax, rect.yMin);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Contains(float pX, float pY)
            {
                return ContainsX(pX) && ContainsY(pY);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool ContainsX(float pX)
            {
                return pX >= LeftUpCorner.x && pX <= RightDownCorner.x;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool ContainsY(float pY)
            {
                return pY >= RightDownCorner.y && pY <= RightDownCorner.y;
            }
        }
    }
}