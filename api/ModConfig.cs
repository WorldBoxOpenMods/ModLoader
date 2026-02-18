using System.Reflection;
using System.Globalization;
using HarmonyLib;
using NeoModLoader.services;
using Newtonsoft.Json;

namespace NeoModLoader.api;

/// <summary>
///     Type of <see cref="ModConfigItem" />
/// </summary>
public enum ConfigItemType
{
    /// <summary>
    ///     A <see cref="ModConfigItem" /> with this will be displayed as a switch button. Only
    ///     <see cref="ModConfigItem.BoolVal" /> is valid
    /// </summary>
    SWITCH,

    /// <summary>
    ///     A <see cref="ModConfigItem" /> with this will be displayed as a slider. Only <see cref="ModConfigItem.FloatVal" />
    ///     is valid
    /// </summary>
    SLIDER,

    /// <summary>
    ///     A <see cref="ModConfigItem" /> with this will be displayed as a text box. Only <see cref="ModConfigItem.TextVal" />
    ///     is valid
    /// </summary>
    TEXT,

    /// <summary>
    ///     A <see cref="ModConfigItem" /> with this will be displayed as a select box.
    ///     <see cref="ModConfigItem.IntVal" /> stores selected index and <see cref="ModConfigItem.MaxIntVal" />
    ///     stores option count.
    /// </summary>
    SELECT,

    /// <summary>
    ///     A <see cref="ModConfigItem" /> with this will be displayed as a int slider. Only
    ///     <see cref="ModConfigItem.IntVal" /> is valid
    /// </summary>
    INT_SLIDER
}

/// <summary>
///     The item of <see cref="ModConfig" />
/// </summary>
public class ModConfigItem
{
    private MethodInfo callback;

    /// <summary>
    ///     Type of this item
    /// </summary>
    [JsonProperty("Type")]
    public ConfigItemType Type { get; internal set; }

    /// <summary>
    /// </summary>
    [JsonProperty("Id")]
    public string Id { get; internal set; }

    /// <summary>
    /// </summary>
    [JsonProperty("IconPath")]
    public string IconPath { get; internal set; }

    /// <summary>
    ///     It is valid only when <see cref="Type" /> is <see cref="ConfigItemType.SWITCH" />
    /// </summary>
    [JsonProperty("BoolVal")]
    public bool BoolVal { get; internal set; }

    /// <summary>
    ///     It is valid only when <see cref="Type" /> is <see cref="ConfigItemType.TEXT" />
    /// </summary>
    [JsonProperty("TextVal")]
    public string TextVal { get; internal set; }

    /// <summary>
    ///     It is valid only when <see cref="Type" /> is <see cref="ConfigItemType.SLIDER" />
    /// </summary>
    [JsonProperty("FloatVal")]
    public float FloatVal { get; internal set; }

    /// <summary>
    ///     It is valid only when <see cref="Type" /> is <see cref="ConfigItemType.SLIDER" />
    /// </summary>
    [JsonProperty("MaxFloatVal")]
    public float MaxFloatVal { get; internal set; } = 1;

    /// <summary>
    ///     It is valid only when <see cref="Type" /> is <see cref="ConfigItemType.SLIDER" />
    /// </summary>
    [JsonProperty("MinFloatVal")]
    public float MinFloatVal { get; internal set; }

    /// <summary>
    ///     It is valid when <see cref="Type" /> is <see cref="ConfigItemType.INT_SLIDER" /> or
    ///     <see cref="ConfigItemType.SELECT" />
    /// </summary>
    [JsonProperty("IntVal")]
    public int IntVal { get; internal set; }

    /// <summary>
    ///     It is valid when <see cref="Type" /> is <see cref="ConfigItemType.INT_SLIDER" /> or
    ///     <see cref="ConfigItemType.SELECT" />.
    ///     For <see cref="ConfigItemType.SELECT" />, this stores option count.
    /// </summary>
    [JsonProperty("MaxIntVal")]
    public int MaxIntVal { get; internal set; } = 1;

    /// <summary>
    ///     It is valid only when <see cref="Type" /> is <see cref="ConfigItemType.INT_SLIDER" />
    /// </summary>
    [JsonProperty("MinIntVal")]
    public int MinIntVal { get; internal set; }

    /// <summary>
    ///     Callback "Type:Method", the method must be static and have only one parameter(see <see cref="Type" />)
    /// </summary>
    [JsonProperty("Callback")]
    public string CallBack { get; internal set; }

    /// <summary>
    ///     Set float range, it is valid only when <see cref="Type" /> is <see cref="ConfigItemType.SLIDER" />
    /// </summary>
    /// <param name="pMin"></param>
    /// <param name="pMax"></param>
    /// <exception cref="ArgumentException"><paramref name="pMax" /> is smaller than <paramref name="pMin" /></exception>
    public void SetFloatRange(float pMin, float pMax)
    {
        if (pMax < pMin) throw new ArgumentException("Max value must be greater than min value!");
        MinFloatVal = pMin;
        MaxFloatVal = pMax;
    }

    /// <summary>
    ///     Set int range, it is valid only when <see cref="Type" /> is <see cref="ConfigItemType.INT_SLIDER" />
    /// </summary>
    /// <param name="pMin"></param>
    /// <param name="pMax"></param>
    /// <exception cref="ArgumentException"><paramref name="pMax" /> is smaller than <paramref name="pMin" /></exception>
    public void SetIntRange(int pMin, int pMax)
    {
        if (pMax < pMin) throw new ArgumentException("Max value must be greater than min value!");
        MinIntVal = pMin;
        MaxIntVal = pMax;
    }

    /// <summary>
    ///     Set value of this item
    /// </summary>
    /// <param name="val">It's type should match this item's <see cref="Type" /></param>
    /// <param name="pSkipCallback">Wheather skip calling callback when value updated</param>
    public void SetValue(object val, bool pSkipCallback = false)
    {
        try
        {
            switch (Type)
            {
                case ConfigItemType.SWITCH:
                    var old_bool_value = BoolVal;
                    BoolVal = Convert.ToBoolean(val);
                    if (!string.IsNullOrEmpty(CallBack) && !pSkipCallback)
                    {
                        if (callback == null) callback = AccessTools.Method(CallBack, new Type[1] { typeof(bool) });

                        if (callback == null)
                            LogService.LogWarning($"No found callback({typeof(bool)}) {CallBack}");
                        else
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

                    break;
                case ConfigItemType.SLIDER:
                    var old_float_value = FloatVal;
                    FloatVal = Convert.ToSingle(val);
                    FloatVal = Math.Max(MinFloatVal, Math.Min(MaxFloatVal, FloatVal));
                    if (!string.IsNullOrEmpty(CallBack) && !pSkipCallback)
                    {
                        MethodInfo callback = AccessTools.Method(CallBack, new Type[1] { typeof(float) });
                        if (callback == null)
                            LogService.LogWarning($"No found callback({typeof(float)}) {CallBack}");
                        else
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

                    break;
                case ConfigItemType.INT_SLIDER:
                    var old_int_slider_value = IntVal;
                    IntVal = Convert.ToInt32(val);
                    IntVal = Math.Max(MinIntVal, Math.Min(MaxIntVal, IntVal));
                    if (!string.IsNullOrEmpty(CallBack) && !pSkipCallback)
                    {
                        MethodInfo callback = AccessTools.Method(CallBack, new Type[1] { typeof(int) });
                        if (callback == null)
                            LogService.LogWarning($"No found callback({typeof(int)}) {CallBack}");
                        else
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
                                IntVal = old_int_slider_value;
                            }
                    }

                    break;
                case ConfigItemType.TEXT:
                    var old_text_value = TextVal;
                    TextVal = Convert.ToString(val);
                    if (!string.IsNullOrEmpty(CallBack) && !pSkipCallback)
                    {
                        MethodInfo callback = AccessTools.Method(CallBack, new Type[1] { typeof(string) });
                        if (callback == null)
                            LogService.LogWarning($"No found callback({typeof(string)}) {CallBack}");
                        else
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

                    break;
                case ConfigItemType.SELECT:
                    var old_int_value = IntVal;
                    IntVal = Convert.ToInt32(val);
                    IntVal = ModConfigSelectOptionCodec.ClampIndex(IntVal, Math.Max(0, MaxIntVal));
                    if (!string.IsNullOrEmpty(CallBack) && !pSkipCallback)
                    {
                        MethodInfo callback = AccessTools.Method(CallBack, new Type[1] { typeof(int) });
                        if (callback == null)
                            LogService.LogWarning($"No found callback({typeof(int)}) {CallBack}");
                        else
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
                case ConfigItemType.INT_SLIDER:
                    IntVal = 0;
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
    ///     Get value of this item
    /// </summary>
    /// <returns>The actual type of return value matches <see cref="Type" /></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public object GetValue()
    {
        return Type switch
        {
            ConfigItemType.SWITCH => BoolVal,
            ConfigItemType.SLIDER => FloatVal,
            ConfigItemType.INT_SLIDER => IntVal,
            ConfigItemType.TEXT => TextVal,
            ConfigItemType.SELECT => IntVal,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

/// <summary>
///     This class is used to represent a mod's config.
/// </summary>
/// <remarks>
///     <list type="bullet">
///         <item>
///             In fact, it can be used to represent any config and displayed by
///             <see cref="NeoModLoader.ui.ModConfigureWindow.ShowWindow" />
///         </item>
///     </list>
/// </remarks>
public partial class ModConfig
{
    private readonly string _path;
    internal Dictionary<string, Dictionary<string, ModConfigItem>> _config = new();

    /// <summary>
    ///     Create a new <see cref="ModConfig" /> instance from <paramref name="path" />
    /// </summary>
    /// <param name="path">Path to read/save</param>
    /// <param name="pIsPersistent">Whether this is a persistent config file generated at runtime</param>
    public ModConfig(string path, bool pIsPersistent = false)
    {
        _path = path;
        if (!File.Exists(path))
        {
            if (!pIsPersistent) LogService.LogWarning($"ModConfig file {path} does not exist, suggest to create one");
            return;
        }

        var json_text = File.ReadAllText(path);
        if (string.IsNullOrWhiteSpace(json_text))
        {
            if (!pIsPersistent) LogService.LogWarning($"ModConfig file {path} is empty or in invalid format!");
            return;
        }

        // Legacy format keeps full schema and values.
        if (TryLoadLegacyConfig(json_text)) return;

        if (!pIsPersistent) LogService.LogWarning($"ModConfig file {path} is empty or in invalid format!");
    }

    private bool TryLoadLegacyConfig(string pJsonText)
    {
        Dictionary<string, List<ModConfigItem>> raw_config;
        try
        {
            raw_config = JsonConvert.DeserializeObject<Dictionary<string, List<ModConfigItem>>>(pJsonText);
        }
        catch
        {
            return false;
        }

        if (raw_config == null) return false;

        foreach (var key in raw_config.Keys)
        {
            CreateGroup(key);
            var value = raw_config[key];
            foreach (ModConfigItem item in value)
            {
                _config[key][item.Id] = item;
                if (item.Type == ConfigItemType.SLIDER)
                    if (item.MaxFloatVal < item.MinFloatVal)
                        item.SetFloatRange(item.MinFloatVal, item.MinFloatVal);

                if (item.Type == ConfigItemType.INT_SLIDER)
                    if (item.MaxIntVal < item.MinIntVal)
                        item.SetIntRange(item.MinIntVal, item.MinIntVal);

                item.SetValue(item.GetValue(), true);
            }
        }

        return true;
    }

    /// <summary>
    ///     Get a <see cref="Dictionary{TKey,TValue}" /> of <see cref="ModConfigItem" /> by <paramref name="pGroupId" />
    /// </summary>
    /// <param name="pGroupId"></param>
    /// <returns></returns>
    public Dictionary<string, ModConfigItem> this[string pGroupId] => _config[pGroupId];

    /// <summary>
    ///     Merge with default config. Keep all items in default config, and remove items not in default config.
    /// </summary>
    public void MergeWith(ModConfig pDefaultConfig)
    {
        var schema = ExtractSchema(pDefaultConfig);
        var state = ExtractState(this);
        _config = ComposeConfig(schema, state);
    }

    private static Dictionary<string, Dictionary<string, ConfigSchemaEntry>> ExtractSchema(ModConfig pDefaultConfig)
    {
        var schema = new Dictionary<string, Dictionary<string, ConfigSchemaEntry>>();
        foreach (var group in pDefaultConfig._config)
        {
            schema[group.Key] = new Dictionary<string, ConfigSchemaEntry>();
            foreach (var item in group.Value)
                schema[group.Key][item.Key] = CreateSchemaFromItem(item.Value);
        }

        return schema;
    }

    private static Dictionary<string, Dictionary<string, ConfigStateEntry>> ExtractState(ModConfig pPersistentConfig)
    {
        var state = new Dictionary<string, Dictionary<string, ConfigStateEntry>>();
        foreach (var group in pPersistentConfig._config)
        {
            state[group.Key] = new Dictionary<string, ConfigStateEntry>();
            foreach (var item in group.Value)
                state[group.Key][item.Key] = CreateStateFromItem(item.Value);
        }

        return state;
    }

    private static Dictionary<string, Dictionary<string, ModConfigItem>> ComposeConfig(
        Dictionary<string, Dictionary<string, ConfigSchemaEntry>> pSchema,
        Dictionary<string, Dictionary<string, ConfigStateEntry>> pState)
    {
        var result = new Dictionary<string, Dictionary<string, ModConfigItem>>();
        foreach (var schema_group in pSchema)
        {
            result[schema_group.Key] = new Dictionary<string, ModConfigItem>();
            pState.TryGetValue(schema_group.Key, out var state_group);
            foreach (var schema_item in schema_group.Value)
            {
                ConfigStateEntry state_entry = null;
                if (state_group != null) state_group.TryGetValue(schema_item.Key, out state_entry);
                result[schema_group.Key][schema_item.Key] = ComposeItem(schema_item.Value, state_entry);
            }
        }

        return result;
    }

    private static ModConfigItem ComposeItem(ConfigSchemaEntry pSchema, ConfigStateEntry pStateEntry)
    {
        ConfigStateEntry resolved_state = ResolveState(pSchema, pStateEntry);
        var new_item = new ModConfigItem
        {
            Id = pSchema.Id,
            Type = pSchema.Type,
            IconPath = pSchema.IconPath,
            CallBack = pSchema.CallBack
        };

        pSchema.ApplyMeta(new_item);
        ApplyStateToItem(new_item, resolved_state);
        return new_item;
    }

    private static ConfigStateEntry ResolveState(ConfigSchemaEntry pSchema, ConfigStateEntry pStateEntry)
    {
        if (pStateEntry == null) return pSchema.CreateDefaultState();
        if (pStateEntry.Type == pSchema.Type) return pSchema.NormalizeState(pStateEntry);
        if (TryConvertStateValue(pStateEntry, pSchema, out var converted)) return pSchema.NormalizeState(converted);
        return pSchema.CreateDefaultState();
    }

    private static bool TryConvertStateValue(ConfigStateEntry pSourceState, ConfigSchemaEntry pTargetSchema,
        out ConfigStateEntry pConvertedState)
    {
        pConvertedState = null;
        switch (pTargetSchema.Type)
        {
            case ConfigItemType.TEXT:
                pConvertedState = new TextStateEntry
                {
                    Value = pSourceState.BoxedValue?.ToString() ?? ""
                };
                return true;
            case ConfigItemType.SWITCH:
                if (TryConvertToBool(pSourceState.BoxedValue, out var bool_value))
                {
                    pConvertedState = new BoolStateEntry { Value = bool_value };
                    return true;
                }

                return false;
            case ConfigItemType.SLIDER:
                if (TryConvertToFloat(pSourceState.BoxedValue, out var float_value))
                {
                    pConvertedState = new FloatStateEntry { Value = float_value };
                    return true;
                }

                return false;
            case ConfigItemType.INT_SLIDER:
                if (TryConvertToInt(pSourceState.BoxedValue, out var int_value))
                {
                    pConvertedState = new IntStateEntry { Value = int_value };
                    return true;
                }

                return false;
            case ConfigItemType.SELECT:
                if (TryConvertToInt(pSourceState.BoxedValue, out var select_value))
                {
                    pConvertedState = new SelectStateEntry
                    {
                        Value = select_value
                    };
                    return true;
                }

                return false;
            default:
                return false;
        }
    }

    private static ConfigSchemaEntry CreateSchemaFromItem(ModConfigItem pItem)
    {
        switch (pItem.Type)
        {
            case ConfigItemType.SWITCH:
                return new SwitchSchemaEntry
                {
                    Id = pItem.Id,
                    IconPath = pItem.IconPath,
                    CallBack = pItem.CallBack,
                    DefaultValue = pItem.BoolVal
                };
            case ConfigItemType.SLIDER:
                return new FloatSliderSchemaEntry
                {
                    Id = pItem.Id,
                    IconPath = pItem.IconPath,
                    CallBack = pItem.CallBack,
                    DefaultValue = pItem.FloatVal,
                    MinValue = pItem.MinFloatVal,
                    MaxValue = pItem.MaxFloatVal
                };
            case ConfigItemType.INT_SLIDER:
                return new IntSliderSchemaEntry
                {
                    Id = pItem.Id,
                    IconPath = pItem.IconPath,
                    CallBack = pItem.CallBack,
                    DefaultValue = pItem.IntVal,
                    MinValue = pItem.MinIntVal,
                    MaxValue = pItem.MaxIntVal
                };
            case ConfigItemType.TEXT:
                return new TextSchemaEntry
                {
                    Id = pItem.Id,
                    IconPath = pItem.IconPath,
                    CallBack = pItem.CallBack,
                    DefaultValue = pItem.TextVal ?? ""
                };
            case ConfigItemType.SELECT:
                int option_count = Math.Max(0, pItem.MaxIntVal);
                return new SelectSchemaEntry
                {
                    Id = pItem.Id,
                    IconPath = pItem.IconPath,
                    CallBack = pItem.CallBack,
                    DefaultValue = pItem.IntVal,
                    OptionCount = option_count
                };
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static ConfigStateEntry CreateStateFromItem(ModConfigItem pItem)
    {
        switch (pItem.Type)
        {
            case ConfigItemType.SWITCH:
                return new BoolStateEntry { Value = pItem.BoolVal };
            case ConfigItemType.SLIDER:
                return new FloatStateEntry { Value = pItem.FloatVal };
            case ConfigItemType.INT_SLIDER:
                return new IntStateEntry { Value = pItem.IntVal };
            case ConfigItemType.TEXT:
                return new TextStateEntry { Value = pItem.TextVal ?? "" };
            case ConfigItemType.SELECT:
                return new SelectStateEntry { Value = pItem.IntVal };
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static void ApplyStateToItem(ModConfigItem pItem, ConfigStateEntry pStateEntry)
    {
        switch (pStateEntry)
        {
            case BoolStateEntry bool_state:
                pItem.SetValue(bool_state.Value, true);
                break;
            case FloatStateEntry float_state:
                pItem.SetValue(float_state.Value, true);
                break;
            case IntStateEntry int_state:
                pItem.SetValue(int_state.Value, true);
                break;
            case TextStateEntry text_state:
                pItem.SetValue(text_state.Value, true);
                break;
            case SelectStateEntry select_state:
                pItem.SetValue(select_state.Value, true);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(pStateEntry));
        }
    }

    private static int ClampSelectIndex(int pIndex, int pOptionCount)
    {
        return ModConfigSelectOptionCodec.ClampIndex(pIndex, pOptionCount);
    }

    private static bool TryConvertToFloat(object pValue, out float pResult)
    {
        pResult = 0;
        if (pValue == null) return false;
        switch (pValue)
        {
            case float float_value:
                pResult = float_value;
                return true;
            case bool bool_value:
                pResult = bool_value ? 1 : 0;
                return true;
            case string string_value:
                if (float.TryParse(string_value, NumberStyles.Float | NumberStyles.AllowThousands,
                        CultureInfo.InvariantCulture, out pResult))
                    return true;
                return float.TryParse(string_value, out pResult);
        }

        try
        {
            pResult = Convert.ToSingle(pValue, CultureInfo.InvariantCulture);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryConvertToInt(object pValue, out int pResult)
    {
        pResult = 0;
        if (pValue == null) return false;
        switch (pValue)
        {
            case int int_value:
                pResult = int_value;
                return true;
            case bool bool_value:
                pResult = bool_value ? 1 : 0;
                return true;
            case string string_value:
                if (int.TryParse(string_value, NumberStyles.Integer, CultureInfo.InvariantCulture, out pResult))
                    return true;
                return int.TryParse(string_value, out pResult);
        }

        try
        {
            pResult = Convert.ToInt32(pValue, CultureInfo.InvariantCulture);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryConvertToBool(object pValue, out bool pResult)
    {
        pResult = false;
        if (pValue == null) return false;
        switch (pValue)
        {
            case bool bool_value:
                pResult = bool_value;
                return true;
            case string string_value:
                if (bool.TryParse(string_value, out pResult)) return true;
                if (int.TryParse(string_value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var int_result))
                {
                    pResult = int_result != 0;
                    return true;
                }

                return false;
        }

        try
        {
            pResult = Convert.ToDouble(pValue, CultureInfo.InvariantCulture) != 0;
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     Apply callbacks of all items with current values.
    /// </summary>
    public void ApplyCallbacks()
    {
        foreach (var group in _config.Values)
        foreach (var item in group.Values)
            item.SetValue(item.GetValue());
    }

    /// <summary>
    ///     Save config
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
            foreach (var item in value) raw_config[key].Add(item.Value);
        }

        var json_text = JsonConvert.SerializeObject(raw_config);
        string temp_path = $"{path}.tmp";
        File.WriteAllText(temp_path, json_text);
        try
        {
            if (File.Exists(path))
                File.Replace(temp_path, path, null);
            else
                File.Move(temp_path, path);
        }
        catch
        {
            File.Copy(temp_path, path, true);
            File.Delete(temp_path);
        }
    }

    /// <summary>
    ///     Create a item group with id <paramref name="pId" />
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
    ///     Add a new Config item to <paramref name="pGroupId" />.
    /// </summary>
    /// <param name="pGroupId"></param>
    /// <param name="pId"></param>
    /// <param name="pType"></param>
    /// <param name="pDefaultValue"></param>
    /// <param name="pIconPath"></param>
    /// <param name="pCallback"></param>
    /// <param name="pSkipCallback">Whether skip callback while setting initial value</param>
    /// <returns></returns>
    public ModConfigItem AddConfigItem(string pGroupId, string pId, ConfigItemType pType, object pDefaultValue,
        string pIconPath = "", string pCallback = "", bool pSkipCallback = false)
    {
        ModConfigItem item = GetOrCreateItem(pGroupId, pId);
        item.Type = pType;
        item.CallBack = pCallback;
        item.SetValue(pDefaultValue, pSkipCallback);
        item.IconPath = pIconPath;
        return item;
    }

    private ModConfigItem GetOrCreateItem(string pGroupId, string pId)
    {
        if (!_config.TryGetValue(pGroupId, out var group))
        {
            group = new Dictionary<string, ModConfigItem>();
            _config[pGroupId] = group;
        }

        if (group.TryGetValue(pId, out var existing_item))
        {
            LogService.LogWarning($"ModConfigItem {pId} already exists in group {pGroupId}! Overwriting...");
            LogService.LogStackTraceAsWarning();
            return existing_item;
        }

        var new_item = new ModConfigItem
        {
            Id = pId
        };
        group[pId] = new_item;
        return new_item;
    }

    /// <summary>
    ///     Add a new Config item typed <see cref="ConfigItemType.SLIDER" /> to <paramref name="pGroupId" />
    /// </summary>
    /// <param name="pGroupId"></param>
    /// <param name="pId"></param>
    /// <param name="pDefaultValue"></param>
    /// <param name="pMinValue"></param>
    /// <param name="pMaxValue"></param>
    /// <param name="pIconPath"></param>
    /// <param name="pCallback"></param>
    /// <param name="pSkipCallback">Whether skip callback while setting initial value</param>
    /// <returns></returns>
    public ModConfigItem AddConfigSliderItemWithRange(string pGroupId, string pId, float pDefaultValue, float pMinValue,
        float pMaxValue, string pIconPath = "", string pCallback = "", bool pSkipCallback = false)
    {
        ModConfigItem item = GetOrCreateItem(pGroupId, pId);
        item.Type = ConfigItemType.SLIDER;
        item.CallBack = pCallback;
        item.SetFloatRange(pMinValue, pMaxValue);
        item.SetValue(pDefaultValue, pSkipCallback);
        item.IconPath = pIconPath;
        return item;
    }

    /// <summary>
    ///     Add a new Config item typed <see cref="ConfigItemType.INT_SLIDER" /> to <paramref name="pGroupId" />
    /// </summary>
    /// <param name="pGroupId"></param>
    /// <param name="pId"></param>
    /// <param name="pDefaultValue"></param>
    /// <param name="pMinValue"></param>
    /// <param name="pMaxValue"></param>
    /// <param name="pIconPath"></param>
    /// <param name="pCallback"></param>
    /// <param name="pSkipCallback">Whether skip callback while setting initial value</param>
    /// <returns></returns>
    public ModConfigItem AddConfigSliderItemWithIntRange(string pGroupId, string pId, int pDefaultValue, int pMinValue,
        int pMaxValue, string pIconPath = "", string pCallback = "", bool pSkipCallback = false)
    {
        ModConfigItem item = GetOrCreateItem(pGroupId, pId);
        item.Type = ConfigItemType.INT_SLIDER;
        item.CallBack = pCallback;
        item.SetIntRange(pMinValue, pMaxValue);
        item.SetValue(pDefaultValue, pSkipCallback);
        item.IconPath = pIconPath;
        return item;
    }

    /// <summary>
    ///     Add a new Config item typed <see cref="ConfigItemType.SELECT" /> to <paramref name="pGroupId" />
    /// </summary>
    /// <param name="pGroupId"></param>
    /// <param name="pId"></param>
    /// <param name="pDefaultIndex"></param>
    /// <param name="pOptionCount">
    ///     Total option count. Option texts are resolved by locale keys: "{pId}_0", "{pId}_1", ...
    /// </param>
    /// <param name="pIconPath"></param>
    /// <param name="pCallback"></param>
    /// <param name="pSkipCallback">Whether skip callback while setting initial value</param>
    /// <returns></returns>
    public ModConfigItem AddConfigSelectItem(string pGroupId, string pId, int pDefaultIndex, int pOptionCount,
        string pIconPath = "", string pCallback = "", bool pSkipCallback = false)
    {
        ModConfigItem item = GetOrCreateItem(pGroupId, pId);
        item.Type = ConfigItemType.SELECT;
        item.CallBack = pCallback;
        item.MinIntVal = 0;
        item.MaxIntVal = Math.Max(0, pOptionCount);
        item.TextVal = "";
        int normalized_index = ModConfigSelectOptionCodec.ClampIndex(pDefaultIndex, item.MaxIntVal);
        item.SetValue(normalized_index, pSkipCallback);
        item.IconPath = pIconPath;
        return item;
    }

    /// <summary>
    ///     Compatibility overload: option texts are ignored and only their count is used.
    /// </summary>
    public ModConfigItem AddConfigSelectItem(string pGroupId, string pId, int pDefaultIndex, IEnumerable<string> pOptions,
        string pIconPath = "", string pCallback = "", bool pSkipCallback = false)
    {
        int option_count = ModConfigSelectOptionCodec.CountOptions(pOptions);
        return AddConfigSelectItem(pGroupId, pId, pDefaultIndex, option_count, pIconPath, pCallback, pSkipCallback);
    }
}
