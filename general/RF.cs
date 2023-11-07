using System.Reflection;
using NeoModLoader.api.attributes;
using NeoModLoader.services;
using NeoModLoader.utils;
using Steamworks.Data;

namespace NeoModLoader.General;
/// <summary>
/// Reflection helper class.
/// </summary>
/// <remarks>
/// <list type="table">
/// <item>
/// <term> TI </term>
/// <description> Instance type </description>
/// </item>
/// <item>
/// <term> TO </term>
/// <description> Output type </description>
/// </item>
/// <item>
/// <term> TF </term>
/// <description> Field type </description>
/// </item>
/// </list>
/// </remarks>
[Experimental("This helper class is experimental. Maybe some errors will occur.")]
public static class RF
{
    static Dictionary<Type, Dictionary<string, Delegate>> _method_cache = new();
    static Dictionary<Type, Dictionary<string, Delegate>> _getter_cache = new();
    static Dictionary<Type, Dictionary<string, Delegate>> _setter_cache = new();

    public static TF GetField<TF, TI>(this TI obj, string name)
    {
        if (_getter_cache.TryGetValue(typeof(TI), out var getters))
        {
            if(getters.TryGetValue(name, out var getter))
            {
                return ((Func<TI, TF>)getter)(obj);
            }
            var newGetter1 = ReflectionHelper.CreateFieldGetter<TI, TF>(name);
            getters.Add(name, newGetter1);
            return newGetter1(obj);
        }
        var newGetter = ReflectionHelper.CreateFieldGetter<TI, TF>(name);
        _getter_cache.Add(typeof(TI), new Dictionary<string, Delegate> { { name, newGetter } });
        return newGetter(obj);
    }
    public static TF GetField<TF>(this Object obj, string name)
    {
        Type TI = obj.GetType();
        if (_getter_cache.TryGetValue(TI, out var getters))
        {
            if(getters.TryGetValue(name, out var getter))
            {
                return (TF)getter.DynamicInvoke(obj);
            }
            var newGetter1 = ReflectionHelper.CreateFieldGetter<TF>(name, TI);
            getters.Add(name, newGetter1);
            return (TF)newGetter1.DynamicInvoke(obj);
        }
        var newGetter = ReflectionHelper.CreateFieldGetter<TF>(name, TI);
        _getter_cache.Add(TI, new Dictionary<string, Delegate> { { name, newGetter } });
        return (TF)newGetter.DynamicInvoke(obj);
    }

    public static TF GetStaticField<TF, TI>(string name)
    {
        FieldInfo fieldInfo = typeof(TI).GetField(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        if (fieldInfo != null) return (TF)fieldInfo.GetValue(null);
        LogService.LogWarning($"Cannot find '{name}' in type {typeof(TI).FullName}. Return default value.");
        try
        {
            throw new Exception();
        }
        catch (Exception e)
        {
            LogService.LogWarning(e.StackTrace);
        }
        return default;
    }
    
    public static void SetField<TF, TI>(this TI obj, string name, TF value)
    {
        if (_setter_cache.TryGetValue(typeof(TI), out var setters))
        {
            if(setters.TryGetValue(name, out var setter))
            {
                ((Action<TI, TF>)setter)(obj, value);
                return;
            }
            var newSetter1 = ReflectionHelper.CreateFieldSetter<TI, TF>(name);
            setters.Add(name, newSetter1);
            newSetter1(obj, value);
            return;
        }
        var newSetter = ReflectionHelper.CreateFieldSetter<TI, TF>(name);
        _setter_cache.Add(typeof(TI), new Dictionary<string, Delegate> { { name, newSetter } });
        newSetter(obj, value);
    }
    public static void SetStaticField<TF, TI>(string name, TF value)
    {
        FieldInfo fieldInfo = typeof(TI).GetField(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        if (fieldInfo != null)
        {
            fieldInfo.SetValue(null, value);
            return;
        }
        LogService.LogWarning($"Cannot find '{name}' in type {typeof(TI).FullName}. No action taken.");
        try
        {
            throw new Exception();
        }
        catch (Exception e)
        {
            LogService.LogWarning(e.StackTrace);
        }
    }
    public static void SetStaticField<TF>(string name, TF value, Type TI)
    {
        FieldInfo fieldInfo = TI.GetField(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        if (fieldInfo != null)
        {
            fieldInfo.SetValue(null, value);
            return;
        }
        LogService.LogWarning($"Cannot find '{name}' in type {TI.FullName}. No action taken.");
        try
        {
            throw new Exception();
        }
        catch (Exception e)
        {
            LogService.LogWarning(e.StackTrace);
        }
    }
}