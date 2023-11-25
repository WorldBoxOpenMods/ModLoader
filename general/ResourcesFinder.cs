using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using NeoModLoader.utils;
using UnityEngine.SearchService;

namespace NeoModLoader.General;
/// <summary>
/// This class is used to find resources and inactive GameObject easily.
/// </summary>
public static class ResourcesFinder
{
    /// <summary>
    /// Find all UnityEngine.Object named <see cref="name"/> as type <see cref="T"/>
    /// </summary>
    public static T[] FindResources<T>(string name) where T : UnityEngine.Object
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

    private static Dictionary<Type, Dictionary<string, UnityEngine.Object>> objects_cache = new();
    
    /// <summary>
    /// Find a UnityEngine.Object named <see cref="name"/> as type <see cref="T"/>
    /// </summary>
    /// <returns>null if not find</returns>
    public static T FindResource<T>(string name) where T : UnityEngine.Object
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
                dict.Add(lower_name, obj);
                return obj;
            }
        }

        return null;
    }
}