using System.Reflection;
using NeoModLoader.utils;
using Steamworks.Data;

namespace NeoModLoader.General;

public static class Reflection
{
    static Dictionary<Type, Dictionary<string, Delegate>> _method_cache = new();
    static Dictionary<Type, Dictionary<string, Delegate>> _getter_cache = new();
    static Dictionary<Type, Dictionary<string, Delegate>> _setter_cache = new();

    public static TF GetField<TF, TO>(this TO obj, string name)
    {
        if (_getter_cache.TryGetValue(typeof(TO), out var getters))
        {
            if(getters.TryGetValue(name, out var getter))
            {
                return ((Func<TO, TF>)getter)(obj);
            }
            else
            {
                var newGetter = ReflectionHelper.CreateFieldGetter<TO, TF>(name);
                getters.Add(name, newGetter);
                return newGetter(obj);
            }
        }
        else
        {
            var newGetter = ReflectionHelper.CreateFieldGetter<TO, TF>(name);
            _getter_cache.Add(typeof(TO), new Dictionary<string, Delegate> { { name, newGetter } });
            return newGetter(obj);
        }
    }
    public static TF GetField<TF>(this Object obj, string name)
    {
        Type TO = obj.GetType();
        if (_getter_cache.TryGetValue(TO, out var getters))
        {
            if(getters.TryGetValue(name, out var getter))
            {
                return (TF)getter.DynamicInvoke(obj);
            }
            else
            {
                var newGetter = ReflectionHelper.CreateFieldGetter<TF>(name, TO);
                getters.Add(name, newGetter);
                return (TF)newGetter.DynamicInvoke(obj);
            }
        }
        else
        {
            var newGetter = ReflectionHelper.CreateFieldGetter<TF>(name, TO);
            _getter_cache.Add(TO, new Dictionary<string, Delegate> { { name, newGetter } });
            return (TF)newGetter.DynamicInvoke(obj);
        }
    }

    public static TF GetStaticField<TF, TO>(string name)
    {
        return (TF) typeof(TO).GetField(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null);
    }

    public static void SetField<TO, TV>(this TO obj, string name, TV value)
    {
        typeof(TO).GetField(name, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(obj, value);
    }
    public static void SetStaticField<TV>(Type TO, string name, TV value)
    {
        TO.GetField(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(null, value);
    }
}