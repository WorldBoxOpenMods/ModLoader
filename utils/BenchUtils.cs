#if DEBUG
#define BENCHENABLED
#endif
using UnityEngine;

namespace NeoModLoader.utils;

public static class BenchUtils
{
    static Dictionary<string, float> bench = new Dictionary<string, float>();
    public static void Start(string key)
    {
#if BENCHENABLED
        if (!bench.ContainsKey(key))
        {
            bench.Add(key, 0);
        }
        float current = Time.realtimeSinceStartup;
        bench[key] = current;
#endif
    }

    public static float End(string key)
    {
#if BENCHENABLED
        float current = Time.realtimeSinceStartup;
        return current - bench[key];
#else
        return -1;
#endif
    }
}