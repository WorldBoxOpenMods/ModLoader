using System.Reflection;

namespace NeoModLoader.General;

public static class Reflection
{
    public static T GetField<T>(this Object obj, string name)
    {
        return (T) obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(obj);
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