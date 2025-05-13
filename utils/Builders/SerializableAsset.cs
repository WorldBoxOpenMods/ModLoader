using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using UnityEngine;

namespace NeoModLoader.utils.Builders
{
    /// <summary>
    /// Because delegates like worldaction cannot be serialized, this is used so you can serialize them
    /// </summary>
    /// <remarks>
    /// the way it stores delegates is that it stores their name, the path to their class, and parameters and then searches the assembly for a matching delegate
    /// </remarks>
    [Serializable]
    public class SerializableAsset<As> where As : Asset
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
        /// Converts the augmentation asset to a serializable version
        /// </summary>
        public static SerializableAsset<A> FromAsset<A>(A Asset) where A : Asset
        {
            SerializableAsset<A> asset = new()
            {
                Variables = new Dictionary<string, object>(),
                Delegates = new()
            };
            foreach (FieldInfo field in typeof(A).GetFields()) { 
                object Value = field.GetValue(Asset);
                if(Value is Delegate){
                    asset.Delegates.Add(field.Name, new Tuple<string,Type, Type[]>(DelegateExtentions.ConvertToString(Value as Delegate), Value.GetType(), (Value as Delegate).Method.GetParameters().ToTypes()));
                }
                else
                {
                    asset.Variables.Add(field.Name, Value);
                }
            }
            return asset;
        }
        /// <summary>
        /// converts the serializable version to its asset
        /// </summary>
        public static A ToAsset<A>(SerializableAsset<A> Asset) where A : Asset, new()
        {
            static object GetRealValueOfObject(object Value, string Name)
            {
                if(Value is long)
                {
                    return Convert.ToInt32(Value);
                }
                else if(Value is double)
                {
                    return Convert.ToSingle(Value);
                }
                else if(Value is JObject && (Name == "base_stats" || Name == "base_stats_meta"))
                {
                    return (Value as JObject).ToObject<BaseStats>();
                }
                return Value;
            }
            A asset = new();
            foreach(FieldInfo field in typeof(A).GetFields())
            {
                if (typeof(Delegate).IsAssignableFrom(field.FieldType))
                {
                    if (Asset.Delegates.TryGetValue(field.Name, out Tuple<string, Type, Type[]> Delegate))
                    {
                        field.SetValue(asset, Delegate.Item1.ToDelegate(Delegate.Item2, Delegate.Item3));
                    }
                }
                else if (Asset.Variables.TryGetValue(field.Name, out object Value))
                {
                    field.SetValue(asset, GetRealValueOfObject(Value, field.Name));
                }
            }
            return asset;
        }
    }
}
