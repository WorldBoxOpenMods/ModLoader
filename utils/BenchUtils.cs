#if DEBUG
#define BENCHENABLED
#endif
using UnityEngine;

namespace NeoModLoader.utils;
/// <summary>
/// It is a simple benchmark tool only enabled when compiled in Debug mode
/// </summary>
internal static class BenchUtils
{
    static Dictionary<string, float> bench = new Dictionary<string, float>();
    /// <summary>
    /// Start a benchmark with <see cref="key"/> tag
    /// </summary>
    public static void Start(string key)
    {
#if BENCHENABLED
        if (!bench.ContainsKey(key)) {
            bench.Add(key, 0);
        }
        float current = Time.realtimeSinceStartup;
        bench[key] = current;
#endif
    }
    /// <summary>
    /// End a benchmark with <see cref="key"/> tag and return the time elapsed
    /// </summary>
    /// <returns>-1 when key not found or compiled not in DEBUG mode. Otherwise, return the time elapsed</returns>
    public static float End(string key)
    {
#if BENCHENABLED
        float current = Time.realtimeSinceStartup;
        if (bench.TryGetValue(key, out float last))
        {
            return current - last;
        }

        return -1;
#else
        return -1;
#endif
    }
}