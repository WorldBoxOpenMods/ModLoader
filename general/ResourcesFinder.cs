using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using NeoModLoader.utils;

namespace NeoModLoader.General;

public static class ResourcesFinder
{
    private class CachedResource
    {
        public string name;
        public UnityEngine.Object[] resources;
        public long last_access;
    }
    private static HashSet<string> cached_names = new();
    private static Dictionary<string, CachedResource> cached_resources_set = new();
    private static PriorityQueue<CachedResource> cached_resources_queue = new(8, Comparer<CachedResource>.Create((a, b) => a.last_access.CompareTo(b.last_access)));
    
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
    
}