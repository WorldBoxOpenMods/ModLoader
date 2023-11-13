using NeoModLoader.services;
using Newtonsoft.Json;

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
    [JsonProperty("Type")]
    public ConfigItemType Type { get; internal set; }
    [JsonProperty("Id")]
    public string Id { get; internal set; }
    [JsonProperty("IconPath")]
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
                    BoolVal = Convert.ToBoolean(val);
                    break;
                case ConfigItemType.SLIDER:
                    FloatVal = Convert.ToSingle(val);
                    break;
                case ConfigItemType.TEXT:
                    TextVal = Convert.ToString(val);
                    break;
                case ConfigItemType.SELECT:
                    IntVal = Convert.ToInt32(val);
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

    public object GetValue()
    {
        return Type switch
        {
            ConfigItemType.SWITCH => BoolVal,
            ConfigItemType.SLIDER => FloatVal,
            ConfigItemType.TEXT => TextVal,
            ConfigItemType.SELECT => IntVal,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
public class ModConfig
{
    private string _path;
    internal Dictionary<string, Dictionary<string, ModConfigItem>> _config = new();
    public ModConfig(string path, bool pPossibleNew = false)
    {
        if (!File.Exists(path))
        {
            if (!pPossibleNew) LogService.LogWarning($"ModConfig file {path} does not exist, suggest to create one");
            else
            {
                _path = path;
            }
            return;
        }
        string json_text = File.ReadAllText(path);
        var raw_config = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, List<ModConfigItem>>>(json_text);
        if (raw_config == null)
        {
            if (!pPossibleNew) LogService.LogWarning($"ModConfig file {path} is empty or in invalid format!");
            else
            {
                _path = path;
            }
            return;
        }
        _path = path;
        foreach (var key in raw_config.Keys)
        {
            CreateGroup(key);
            var value = raw_config[key];
            foreach (var item in value)
            {
                _config[key][item.Id] = item;
            }
        }
    }

    public void MergeWith(ModConfig pDefaultConfig)
    {
        foreach (var key in pDefaultConfig._config.Keys)
        {
            if (!_config.ContainsKey(key))
            {
                _config[key] = new();
            }
            var group = _config[key];
            var default_group = pDefaultConfig._config[key];
            foreach (string item in default_group.Keys.Where(item => !group.ContainsKey(item)))
            {
                AddConfigItem(key, item, default_group[item].Type, default_group[item].GetValue(), default_group[item].IconPath);
            }
        }
    }
    public void Save(string path = null)
    {
        path ??= _path;
        var raw_config = new Dictionary<string, List<ModConfigItem>>();
        foreach (var key in _config.Keys)
        {
            var value = _config[key];
            raw_config[key] = new List<ModConfigItem>();
            foreach (var item in value)
            {
                raw_config[key].Add(item.Value);
            }
        }
        string json_text = Newtonsoft.Json.JsonConvert.SerializeObject(raw_config);
        File.WriteAllText(path, json_text);
    }
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