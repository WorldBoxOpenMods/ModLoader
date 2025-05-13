using Newtonsoft.Json.Linq;
using System.Reflection;

namespace NeoModLoader.utils.SerializedAssets
{
    /// <summary>
    /// Because delegates like worldaction cannot be serialized, this is used so you can serialize them
    /// </summary>
    /// <remarks>
    /// the way it stores delegates is that it stores their name, the path to their class, and parameters and then searches the assembly for a matching delegate
    /// </remarks>
    [Serializable]
    public class SerializableAsset<A> where A : Asset, new()
    {
        /// <summary>
        /// the variables of the asset
        /// </summary>
        public Dictionary<string, object> Variables;
        /// <summary>
        /// the delegates of the asset
        /// </summary>
        public Dictionary<string, Tuple<string, Type, Type[]>> Delegates;
        /// <summary>
        /// takes delegates and variables from an asset and takes them to a serializable asset
        /// </summary>
        public static void Serialize(A Asset, SerializableAsset<A> asset)
        {
            foreach (FieldInfo field in typeof(A).GetFields())
            {
                object Value = field.GetValue(Asset);
                if (Value is Delegate)
                {
                    asset.Delegates.Add(field.Name, new Tuple<string, Type, Type[]>((Value as Delegate).ConvertToString(), Value.GetType(), (Value as Delegate).Method.GetParameters().ToTypes()));
                }
                else
                {
                    asset.Variables.Add(field.Name, Value);
                }
            }
        }
        /// <summary>
        /// Converts the augmentation asset to a serializable version
        /// </summary>
        public static SerializableAsset<A> FromAsset(A Asset)
        {
            SerializableAsset<A> asset = new()
            {
                Variables = new Dictionary<string, object>(),
                Delegates = new()
            };
            Serialize(Asset, asset);
            return asset;
        }
        /// <summary>
        /// takes delegates and variables from a serializable asset and takes them to a asset
        /// </summary>
        public static void Deserialize(SerializableAsset<A> Asset, A asset)
        {
            static object GetRealValueOfObject(object Value, string Name)
            {
                if (Value is long)
                {
                    return Convert.ToInt32(Value);
                }
                else if (Value is double)
                {
                    return Convert.ToSingle(Value);
                }
                else if (Value is JObject && (Name == "base_stats" || Name == "base_stats_meta"))
                {
                    return (Value as JObject).ToObject<BaseStats>();
                }
                return Value;
            }
            foreach (FieldInfo field in typeof(A).GetFields())
            {
                if (typeof(Delegate).IsAssignableFrom(field.FieldType))
                {
                    if (Asset.Delegates.TryGetValue(field.Name, out Tuple<string, Type, Type[]> Delegate))
                    {
                        field.SetValue(asset, Delegate.Item1.ConvertToDelegate(Delegate.Item2, Delegate.Item3));
                    }
                }
                else if (Asset.Variables.TryGetValue(field.Name, out object Value))
                {
                    field.SetValue(asset, GetRealValueOfObject(Value, field.Name));
                }
            }
        }
        /// <summary>
        /// converts the serializable version to its asset
        /// </summary>
        public static A ToAsset(SerializableAsset<A> Asset)
        {
            A asset = new();
            Deserialize(Asset, asset);
            return asset;
        }
    }
}