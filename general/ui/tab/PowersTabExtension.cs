using NeoModLoader.services;
using UnityEngine;

namespace NeoModLoader.General.UI.Tab;

public static class PowersTabExtension
{
    private static Dictionary<string, WrappedPowersTab> _wrapped_powers_tabs = new();
    public static void SetLayout(this PowersTab pTab, List<string> pGroupIds)
    {
        var wrapped_tab = _getWrappedPowersTab(pTab);
        if (!wrapped_tab.Modifiable)
        {
            LogService.LogWarning($"{pTab.name}'s layout cannot be changed");
            LogService.LogStackTraceAsWarning();
            return;
        }
        
        wrapped_tab.ResetGroups();
        foreach (string group_id in pGroupIds)
        {
            wrapped_tab.AddGroup(group_id);
        }
        wrapped_tab.Modifiable = false;
    }

    public static void AddPowerButton(this PowersTab pTab, string pGroupId, PowerButton pPowerButton)
    {
        var wrapped_tab = _getWrappedPowersTab(pTab);

        if (!wrapped_tab.HasGroup(pGroupId))
        {
            LogService.LogWarning($"{pTab.name}'s layout does not contain group \"{pGroupId}\"");
            LogService.LogStackTraceAsWarning();
            return;
        }
        
        wrapped_tab.AddPowerButton(pGroupId, pPowerButton);
    }

    public static void PutElement(this PowersTab pTab, string pGroupId, RectTransform pObjRect,
        Vector2 pPositionInGroup, bool pPlacehold = true)
    {
        var wrapped_tab = _getWrappedPowersTab(pTab);

        if (!wrapped_tab.HasGroup(pGroupId))
        {
            LogService.LogWarning($"{pTab.name}'s layout does not contain group \"{pGroupId}\"");
            LogService.LogStackTraceAsWarning();
            return;
        }
        
        wrapped_tab.AddCustomRect(pGroupId, pObjRect, pPositionInGroup, pPlacehold);
    }

    public static void UpdateLayout(this PowersTab pTab)
    {
        _getWrappedPowersTab(pTab).UpdateLayout();
    }
    private static WrappedPowersTab _getWrappedPowersTab(PowersTab pTab)
    {
        if (!_wrapped_powers_tabs.TryGetValue(pTab.name, out var wrapped_powers_tab))
        {
            wrapped_powers_tab = new WrappedPowersTab(pTab);
            _wrapped_powers_tabs.Add(pTab.name, wrapped_powers_tab);
        }

        return wrapped_powers_tab;
    }
}