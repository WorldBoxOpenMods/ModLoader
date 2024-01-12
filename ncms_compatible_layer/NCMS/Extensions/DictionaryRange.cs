namespace NCMS.Extensions;

// Several stupid NCMS mods use this class, so we have to implement it.
// Idk why they do not use a loop directly.
#pragma warning disable CS1591 // No comment for NCMS compatible layer
/// <remarks>
///     From [NCMS](https://denq04.github.io/ncms/)
/// </remarks>
public static class DictionaryRange
{
    public static void AddRangeOverride<TKey, TValue>(this IDictionary<TKey, TValue> dic,
        IDictionary<TKey, TValue> dicToAdd)
    {
        foreach (var key in dicToAdd.Keys) dic[key] = dicToAdd[key];
    }

    public static void AddRangeNewOnly<TKey, TValue>(this IDictionary<TKey, TValue> dic,
        IDictionary<TKey, TValue> dicToAdd)
    {
        foreach (var key in dicToAdd.Keys)
            if (!dic.ContainsKey(key))
            {
                dic[key] = dicToAdd[key];
            }
    }

    public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dic, IDictionary<TKey, TValue> dicToAdd)
    {
        MonoMod.Utils.Extensions.AddRange(dic, dicToAdd);
    }

    public static bool ContainsKeys<TKey, TValue>(this IDictionary<TKey, TValue> dic, IEnumerable<TKey> keys)
    {
        foreach (var key in keys)
            if (!dic.ContainsKey(key))
                return false;
        return true;
    }

    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (T t in source)
        {
            action(t);
        }
    }

    public static void ForEachOrBreak<T>(this IEnumerable<T> source, Func<T, bool> func)
    {
        foreach (T t in source)
        {
            if (func(t))
            {
                break;
            }
        }
    }
}