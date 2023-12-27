using UnityEngine;
using Object = UnityEngine.Object;

namespace NeoModLoader.General;

/// <summary>
/// This class is used to find resources and inactive GameObject easily.
/// </summary>
public static class ResourcesFinder
{
    private static Dictionary<Type, Dictionary<string, Object>> objects_cache = new();

    /// <summary>
    /// Find all UnityEngine.Object named <paramref name="name"/> as type <typeparamref name="T"/>
    /// </summary>
    public static T[] FindResources<T>(string name) where T : Object
    {
        T[] first_search = Resources.FindObjectsOfTypeAll<T>();
        List<T> result = new List<T>(first_search.Length / 16);

        string lower_name = name.ToLower();
        foreach (var obj in first_search)
        {
            if (obj.name.ToLower() == lower_name)
            {
                result.Add(obj);
            }
        }

        return result.ToArray();
    }

    /// <summary>
    /// Find a UnityEngine.Object named <paramref name="name"/> as type <typeparamref name="T"/>
    /// </summary>
    /// <returns>null if not find</returns>
    public static T FindResource<T>(string name) where T : Object
    {
        string lower_name = name.ToLower();
        if (objects_cache.TryGetValue(typeof(T), out var dict))
        {
            if (dict.TryGetValue(lower_name, out var result))
            {
                return (T)result;
            }

            goto FINDANDADD;
        }

        dict = new();
        objects_cache.Add(typeof(T), dict);

        FINDANDADD:
        T[] first_search = Resources.FindObjectsOfTypeAll<T>();
        foreach (var obj in first_search)
        {
            if (obj.name.ToLower() == lower_name)
            {
                T result = Object.Instantiate(obj, WorldBoxMod.InactiveTransform);
                result.name = obj.name;
                dict.Add(lower_name, result);
                return obj;
            }
        }

        return null;
    }
}