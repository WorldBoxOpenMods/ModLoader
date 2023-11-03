using System.Reflection;

namespace NeoModLoader.General;

public static class Reflection
{
    public static T GetField<T>(this Object obj, string name)
    {
        return (T) obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(obj);
    }

    public static TO GetStaticField<TO, TI>(string name)
    {
        return (TO) typeof(TI).GetField(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null);
    }
}