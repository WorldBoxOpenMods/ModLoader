using NeoModLoader.services;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace NeoModLoader.utils
{
    /// <summary>
    /// Because delegates like worldaction cannot be serialized, this is used so you can serialize them
    /// </summary>
    [Serializable]
    public class SerializableAsset
    {
        /// <summary>
        /// the variables of the asset
        /// </summary>
        public Dictionary<string, object> Variables = new();
        /// <summary>
        /// takes delegates and variables from an asset and takes them to a serializable asset
        /// </summary>
        public static void Serialize(Asset Asset, SerializableAsset asset)
        {
            foreach (FieldInfo field in Asset.GetType().GetFields())
            {
                object Value = field.GetValue(Asset);
                if (Value is Delegate value)
                {
                    asset.Variables.Add(field.Name, value.AsString(false));
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
        public static SerializableAsset FromAsset(Asset Asset)
        {
            SerializableAsset asset = new();
            Serialize(Asset, asset);
            return asset;
        }
        /// <summary>
        /// takes delegates and variables from a serializable asset and takes them to a asset
        /// </summary>
        public static void Deserialize(SerializableAsset Asset, Asset asset)
        {
            static object GetRealValueOfObject(object Value, Type Type)
            {
                if(Value == null)
                {
                    return null;
                }
                if (typeof(Delegate).IsAssignableFrom(Type))
                {
                    return (Value as string).AsDelegate(Type);
                }
                if (Type == typeof(int))
                {
                    return Convert.ToInt32(Value);
                }
                else if (Type == typeof(float))
                {
                    return Convert.ToSingle(Value);
                }
                else if (typeof(Enum).IsAssignableFrom(Type))
                {
                    return Enum.ToObject(Type, Convert.ToInt32(Value));
                }
                else if (Value is JObject JObject)
                {
                    try
                    {
                        return JObject.ToObject(Type);
                    }
                    catch(Exception e)
                    {
                        LogService.LogWarning($"Warning: the field {Type.Name} of Asset {Type.DeclaringType} is invalid.");
                        return null;
                    }
                }
                return Value;
            }
            foreach (FieldInfo field in asset.GetType().GetFields())
            {
                if (Asset.Variables.TryGetValue(field.Name, out object Value))
                {
                    field.SetValue(asset, GetRealValueOfObject(Value, field.FieldType));
                }
            }
        }
        /// <summary>
        /// converts the serializable version to its asset
        /// </summary>
        public static Asset ToAsset(SerializableAsset Asset, Type AssetType)
        {
            Asset asset = Activator.CreateInstance(AssetType) as Asset;
            Deserialize(Asset, asset);
            return asset;
        }
    }
}