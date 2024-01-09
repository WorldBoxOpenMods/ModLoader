using System.Reflection;
using HarmonyLib;
using NeoModLoader.services;
using Newtonsoft.Json;

namespace NeoModLoader.api;

/// <summary>
/// Type of <see cref="ModConfigItem"/>
/// </summary>
public enum ConfigItemType
{
    /// <summary>
    /// A <see cref="ModConfigItem"/> with this will be displayed as a switch button. Only <see cref="ModConfigItem.BoolVal"/> is valid
    /// </summary>
    SWITCH,

    /// <summary>
    /// A <see cref="ModConfigItem"/> with this will be displayed as a slider. Only <see cref="ModConfigItem.FloatVal"/> is valid
    /// </summary>
    SLIDER,

    /// <summary>
    /// A <see cref="ModConfigItem"/> with this will be displayed as a text box. Only <see cref="ModConfigItem.TextVal"/> is valid
    /// </summary>
    TEXT,

    /// <summary>
    /// A <see cref="ModConfigItem"/> with this will be displayed as a select box. Only <see cref="ModConfigItem.IntVal"/> is valid
    /// </summary>
    SELECT
}

/// <summary>
/// The item of <see cref="ModConfig"/>
/// </summary>
public class ModConfigItem
{
    private MethodInfo callback;

    /// <summary>
    /// Type of this item
    /// </summary>
    [JsonProperty("Type")]
    public ConfigItemType Type { get; internal set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonProperty("Id")]
    public string Id { get; internal set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonProperty("IconPath")]
    public string IconPath { get; internal set; }

    /// <summary>
    /// It is valid only when <see cref="Type"/> is <see cref="ConfigItemType.SWITCH"/>
    /// </summary>
    [JsonProperty("BoolVal")]
    public bool BoolVal { get; internal set; }

    /// <summary>
    /// It is valid only when <see cref="Type"/> is <see cref="ConfigItemType.TEXT"/>
    /// </summary>
    [JsonProperty("TextVal")]
    public string TextVal { get; internal set; }

    /// <summary>
    /// It is valid only when <see cref="Type"/> is <see cref="ConfigItemType.SLIDER"/>
    /// </summary>
    [JsonProperty("FloatVal")]
    public float FloatVal { get; internal set; }

    /// <summary>
    /// It is valid only when <see cref="Type"/> is <see cref="ConfigItemType.SLIDER"/>
    /// </summary>
    [JsonProperty("MaxFloatVal")]
    public float MaxFloatVal { get; internal set; } = 1;

    /// <summary>
    /// It is valid only when <see cref="Type"/> is <see cref="ConfigItemType.SLIDER"/>
    /// </summary>
    [JsonProperty("MinFloatVal")]
    public float MinFloatVal { get; internal set; } = 0;

    /// <summary>
    /// It is not implemented.
    /// </summary>
    [JsonProperty("IntVal")]
    public int IntVal { get; internal set; }

    /// <summary>
    /// Callback "Type:Method", the method must be static and have only one parameter(see <see cref="Type"/>)
    /// </summary>
    [JsonProperty("Callback")]
    public string CallBack { get; internal set; }

    /// <summary>
    /// Set float range, it is valid only when <see cref="Type"/> is <see cref="ConfigItemType.SLIDER"/>
    /// </summary>
    /// <param name="pMin"></param>
    /// <param name="pMax"></param>
    /// <exception cref="ArgumentException"><paramref name="pMax"/> is smaller than <paramref name="pMin"/></exception>
    public void SetFloatRange(float pMin, float pMax)
    {
        if (pMax < pMin) throw new ArgumentException("Max value must be greater than min value!");
        MinFloatVal = pMin;
        MaxFloatVal = pMax;
    }

    /// <summary>
    /// Set value of this item
    /// </summary>
    /// <param name="val">It's type should match this item's <see cref="Type"/></param>
    /// <param name="pSkipCallback">Wheather skip calling callback when value updated</param>
    public void SetValue(object val, bool pSkipCallback = false)
    {
        try
        {
            switch (Type)
            {
                case ConfigItemType.SWITCH:
                    bool old_bool_value = BoolVal;
                    BoolVal = Convert.ToBoolean(val);
                    if (!string.IsNullOrEmpty(CallBack) && !pSkipCallback)
                    {
                        if (callback == null)
                        {
                            callback = AccessTools.Method(CallBack, new Type[1] { typeof(bool) });
                        }

                        if (callback == null)
                        {
                            LogService.LogWarning($"No found callback({typeof(bool)}) {CallBack}");
                        }
                        else
                        {
                            try
                            {
                                callback.Invoke(null, new object[] { BoolVal });
                            }
                            catch (Exception e)
                            {
                                LogService.LogError(
                                    $"Failed to set value '{BoolVal}'({typeof(bool)}) for config item '{Id}'");
                                LogService.LogError(e.Message);
                                LogService.LogError(e.StackTrace);
                                BoolVal = old_bool_value;
                            }
                        }
                    }

                    break;
                case ConfigItemType.SLIDER:
                    float old_float_value = FloatVal;
                    FloatVal = Convert.ToSingle(val);
                    FloatVal = Math.Max(MinFloatVal, Math.Min(MaxFloatVal, FloatVal));
                    if (!string.IsNullOrEmpty(CallBack) && !pSkipCallback)
                    {
                        var callback = AccessTools.Method(CallBack, new Type[1] { typeof(float) });
                        if (callback == null)
                        {
                            LogService.LogWarning($"No found callback({typeof(float)}) {CallBack}");
                        }
                        else
                        {
                            try
                            {
                                callback.Invoke(null, new object[] { FloatVal });
                            }
                            catch (Exception e)
                            {
                                LogService.LogError(
                                    $"Failed to set value '{FloatVal}'({typeof(float)}) for config item '{Id}'");
                                LogService.LogError(e.Message);
                                LogService.LogError(e.StackTrace);
                                FloatVal = old_float_value;
                            }
                        }
                    }

                    break;
                case ConfigItemType.TEXT:
                    string old_text_value = TextVal;
                    TextVal = Convert.ToString(val);
                    if (!string.IsNullOrEmpty(CallBack) && !pSkipCallback)
                    {
                        var callback = AccessTools.Method(CallBack, new Type[1] { typeof(string) });
                        if (callback == null)
                        {
                            LogService.LogWarning($"No found callback({typeof(string)}) {CallBack}");
                        }
                        else
                        {
                            try
                            {
                                callback.Invoke(null, new object[] { TextVal });
                            }
                            catch (Exception e)
                            {
                                LogService.LogError(
                                    $"Failed to set value '{TextVal}'({typeof(string)}) for config item '{Id}'");
                                LogService.LogError(e.Message);
                                LogService.LogError(e.StackTrace);
                                TextVal = old_text_value;
                            }
                        }
                    }

                    break;
                case ConfigItemType.SELECT:
                    int old_int_value = IntVal;
                    IntVal = Convert.ToInt32(val);
                    if (!string.IsNullOrEmpty(CallBack) && !pSkipCallback)
                    {
                        var callback = AccessTools.Method(CallBack, new Type[1] { typeof(int) });
                        if (callback == null)
                        {
                            LogService.LogWarning($"No found callback({typeof(int)}) {CallBack}");
                        }
                        else
                        {
                            try
                            {
                                callback.Invoke(null, new object[] { IntVal });
                            }
                            catch (Exception e)
                            {
                                LogService.LogError(
                                    $"Failed to set value '{IntVal}'({typeof(int)}) for config item '{Id}'");
                                LogService.LogError(e.Message);
                                LogService.LogError(e.StackTrace);
                                IntVal = old_int_value;
                            }
                        }
                    }

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

    /// <summary>
    /// Get value of this item
    /// </summary>
    /// <returns>The actual type of return value matches <see cref="Type"/></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
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

/// <summary>
/// This class is used to represent a mod's config.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>In fact, it can be used to represent any config and displayed by <see cref="NeoModLoader.ui.ModConfigureWindow.ShowWindow"/></item>
/// </list>
/// </remarks>
public class ModConfig
{
    internal Dictionary<string, Dictionary<string, ModConfigItem>> _config = new();
    private string _path;

    /// <summary>
    /// Create a new <see cref="ModConfig"/> instance from <paramref name="path"/>
    /// </summary>
    /// <param name="path">Path to read/save</param>
    /// <param name="pIsPersistent">Whether to skip callback of all items</param>
    public ModConfig(string path, bool pIsPersistent = false)
    {
        if (!File.Exists(path))
        {
            if (!pIsPersistent) LogService.LogWarning($"ModConfig file {path} does not exist, suggest to create one");
            else
            {
                _path = path;
            }

            return;
        }

        string json_text = File.ReadAllText(path);
        var raw_config = JsonConvert.DeserializeObject<Dictionary<string, List<ModConfigItem>>>(json_text);
        if (raw_config == null)
        {
            if (!pIsPersistent) LogService.LogWarning($"ModConfig file {path} is empty or in invalid format!");
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
                if (item.Type == ConfigItemType.SLIDER)
                {
                    if (item.MaxFloatVal < item.MinFloatVal) item.SetFloatRange(item.MinFloatVal, item.MinFloatVal);
                }

                item.SetValue(item.GetValue(), !pIsPersistent);
            }
        }
    }

    /// <summary>
    /// Get a <see cref="Dictionary{TKey,TValue}"/> of <see cref="ModConfigItem"/> by <paramref name="pGroupId"/>
    /// </summary>
    /// <param name="pGroupId"></param>
    /// <returns></returns>
    public Dictionary<string, ModConfigItem> this[string pGroupId]
    {
        get => _config[pGroupId];
    }

    /// <summary>
    ///     Merge with default config. Keep all items in default config, and remove items not in default config.
    /// </summary>
    public void MergeWith(ModConfig pDefaultConfig)
    {
        HashSet<string> group_to_remove = new();
        foreach (var key in _config.Keys)
        {
            if (!pDefaultConfig._config.ContainsKey(key))
            {
                group_to_remove.Add(key);
                continue;
            }

            var group = _config[key];
            var default_group = pDefaultConfig._config[key];
            HashSet<string> item_to_remove = new();
            foreach (string item in group.Keys.Where(item => !default_group.ContainsKey(item)))
            {
                item_to_remove.Add(item);
            }

            foreach (string item in item_to_remove)
            {
                group.Remove(item);
            }
        }

        foreach (var group in group_to_remove)
        {
            _config.Remove(group);
        }

        foreach (var group_id in pDefaultConfig._config.Keys)
        {
            if (!_config.ContainsKey(group_id))
            {
                _config[group_id] = new Dictionary<string, ModConfigItem>();
            }

            var group = _config[group_id];
            var default_group = pDefaultConfig._config[group_id];
            foreach (string item_id in default_group.Keys.Where(item => group.ContainsKey(item)))
            {
                group[item_id].CallBack = default_group[item_id].CallBack;
                if (group[item_id].Type != default_group[item_id].Type)
                {
                    AddConfigItem(group_id, item_id, default_group[item_id].Type, default_group[item_id].GetValue(),
                        default_group[item_id].IconPath, default_group[item_id].CallBack);
                }
                else if (group[item_id].Type == ConfigItemType.SLIDER)
                {
                    group[item_id].SetFloatRange(default_group[item_id].MinFloatVal,
                        default_group[item_id].MaxFloatVal);
                    float current_value = group[item_id].GetValue() is float ? (float)group[item_id].GetValue() : 0;
                    if (current_value < default_group[item_id].MinFloatVal ||
                        current_value > default_group[item_id].MaxFloatVal)
                        group[item_id].SetValue(default_group[item_id].GetValue());
                }
            }

            foreach (string item in default_group.Keys.Where(item => !group.ContainsKey(item)))
            {
                if (default_group[item].Type != ConfigItemType.SLIDER)
                    AddConfigItem(group_id, item, default_group[item].Type, default_group[item].GetValue(),
                        default_group[item].IconPath, default_group[item].CallBack);
                else
                    AddConfigSliderItemWithRange(group_id, item, (float)default_group[item].GetValue(),
                        default_group[item].MinFloatVal, default_group[item].MaxFloatVal,
                        default_group[item].IconPath, default_group[item].CallBack);
            }
        }
    }

    /// <summary>
    /// Save config
    /// </summary>
    /// <param name="path">Overwrite save path</param>
    public void Save(string path = null)
    {
        path ??= _path;
        if (string.IsNullOrEmpty(path)) return;
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

        string json_text = JsonConvert.SerializeObject(raw_config);
        File.WriteAllText(path, json_text);
    }

    /// <summary>
    /// Create a item group with id <paramref name="pId"/>
    /// </summary>
    /// <param name="pId"></param>
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

    /// <summary>
    /// Add a new Config item to <paramref name="pGroupId"/>.
    /// </summary>
    /// <param name="pGroupId"></param>
    /// <param name="pId"></param>
    /// <param name="pType"></param>
    /// <param name="pDefaultValue"></param>
    /// <param name="pIconPath"></param>
    /// <param name="pCallback"></param>
    /// <returns></returns>
    public ModConfigItem AddConfigItem(string pGroupId, string pId, ConfigItemType pType, object pDefaultValue,
        string pIconPath = "", string pCallback = "")
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
        }
        else
        {
            group[pId] = new ModConfigItem()
            {
                Id = pId
            };
        }

        group[pId].Type = pType;
        group[pId].CallBack = pCallback;
        group[pId].SetValue(pDefaultValue);
        group[pId].IconPath = pIconPath;
        return group[pId];
    }

    /// <summary>
    /// Add a new Config item typed <see cref="ConfigItemType.SLIDER"/> to <paramref name="pGroupId"/>
    /// </summary>
    /// <param name="pGroupId"></param>
    /// <param name="pId"></param>
    /// <param name="pDefaultValue"></param>
    /// <param name="pMinValue"></param>
    /// <param name="pMaxValue"></param>
    /// <param name="pIconPath"></param>
    /// <param name="pCallback"></param>
    /// <returns></returns>
    public ModConfigItem AddConfigSliderItemWithRange(string pGroupId, string pId, float pDefaultValue, float pMinValue,
        float pMaxValue, string pIconPath = "", string pCallback = "")
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
        }
        else
        {
            group[pId] = new ModConfigItem()
            {
                Id = pId
            };
        }

        group[pId].Type = ConfigItemType.SLIDER;
        group[pId].CallBack = pCallback;
        group[pId].SetFloatRange(pMinValue, pMaxValue);
        group[pId].SetValue(pDefaultValue);
        group[pId].IconPath = pIconPath;
        return group[pId];
    }
}