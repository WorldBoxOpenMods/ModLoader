namespace NeoModLoader.api;

enum ConfigItemType
{
    SWITCH,
    SLIDER,
    TEXT,
    SELECT
}
class ModConfigItem
{
    public ConfigItemType Type { get; set; }
    public bool BoolVal;
    public string TextVal;
    public float FloatVal;
    public int IntVal;
}
public class ModConfig
{
    private Dictionary<string, Dictionary<string, ModConfigItem>> _config = new();
}