using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace NeoModLoader.General.Game.extensions;

public static class DataExtension
{
    public static bool TryGet<TCustomData>(this BaseSystemData data, string key, out TCustomData result) where TCustomData : ICustomData, new()
    {
        result = new TCustomData();
        data.get(key, out string json);
        if (json == null)
        {
            return false;
        }
        JObject json_obj;
        try
        {
            json_obj = JObject.Parse(json);
        }
        catch (JsonReaderException)
        {
            return false;
        }
        var serialized_data = json_obj.ToObject<SerializedCustomData>();
        if (serialized_data == null)
        {
            return false;
        }
        result.Deserialize(serialized_data);
        return true;
    }

    public static void Set<TCustomData>(this BaseSystemData data, string key, TCustomData value) where TCustomData : ICustomData
    {
        data.set(key, JsonConvert.SerializeObject(value.Serialize()));
    }
}

public class SerializedCustomData
{
    public string ModId;
    public string DataVersion;
    public JObject Data;
}

public interface ICustomData
{
    public SerializedCustomData Serialize();
    public void Deserialize(SerializedCustomData data);
}