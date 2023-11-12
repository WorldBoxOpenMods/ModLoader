using NeoModLoader.services;

namespace NeoModLoader.api;

public enum ConfigItemType
{
    SWITCH,
    SLIDER,
    TEXT,
    SELECT
}
class ModConfigItem
{
    public ConfigItemType Type { get; internal set; }
    public string Id { get; internal set; }
    public string IconPath { get; internal set; }
    public bool BoolVal;
    public string TextVal;
    public float FloatVal;
    public int IntVal;

    public void SetValue(object val)
    {
        try
        {
            switch (Type)
            {
                case ConfigItemType.SWITCH:
                    BoolVal = (bool)val;
                    break;
                case ConfigItemType.SLIDER:
                    FloatVal = (float)val;
                    break;
                case ConfigItemType.TEXT:
                    TextVal = (string)val;
                    break;
                case ConfigItemType.SELECT:
                    IntVal = (int)val;
                    break;
            }
        }
        catch (Exception e)
        {
            LogService.LogError($"Error while setting value for config item {Type}! {e.Message}");
            LogService.LogError(e.StackTrace);
            LogService.LogError("Set default value instead.");
            switch (Type)
            {
                case ConfigItemType.SWITCH:
                    BoolVal = false;
                    break;
                case ConfigItemType.SLIDER:
                    FloatVal = 0;
                    break;
                case ConfigItemType.TEXT:
                    TextVal = "";
                    break;
                case ConfigItemType.SELECT:
                    IntVal = 0;
                    break;
            }
        }
    }
}
public class ModConfig
{
    internal Dictionary<string, Dictionary<string, ModConfigItem>> _config = new();

    public void CreateGroup(string pId)
    {
        if (_config.ContainsKey(pId))
        {
            LogService.LogWarning($"ModConfigGroup {pId} already exists!");
            LogService.LogStackTraceAsWarning();
            return;
        }

        _config[pId] = new Dictionary<string, ModConfigItem>();
    }

    public void AddConfigItem(string pGroupId, string pId, ConfigItemType pType, object pDefaultValue, string pIconPath = "")
    {
        if (!_config.TryGetValue(pGroupId, out var group))
        {
            group = new();
            _config[pGroupId] = group;
        }
        if (group.ContainsKey(pId))
        {
            LogService.LogWarning($"ModConfigItem {pId} already exists in group {pGroupId}! Overwriting...");
            LogService.LogStackTraceAsWarning();
            group[pId].Type = pType;
            group[pId].SetValue(pDefaultValue);
            return;
        }

        group[pId] = new ModConfigItem()
        {
            Id = pId,
            Type = pType
        };
        group[pId].SetValue(pDefaultValue);
        group[pId].IconPath = pIconPath;
    }
}