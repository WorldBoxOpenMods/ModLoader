namespace NCMS.Extensions;

public static class DictionaryRange
{
    // Token: 0x060000A1 RID: 161 RVA: 0x00008098 File Offset: 0x00006298
    public static void AddRangeOverride<TKey, TValue>(this IDictionary<TKey, TValue> dic,
        IDictionary<TKey, TValue> dicToAdd)
    {
        dicToAdd.ForEach(delegate(KeyValuePair<TKey, TValue> x) { dic[x.Key] = x.Value; });
    }

    // Token: 0x060000A2 RID: 162 RVA: 0x000080C8 File Offset: 0x000062C8
    public static void AddRangeNewOnly<TKey, TValue>(this IDictionary<TKey, TValue> dic,
        IDictionary<TKey, TValue> dicToAdd)
    {
        dicToAdd.ForEach(delegate(KeyValuePair<TKey, TValue> x)
        {
            if (!dic.ContainsKey(x.Key))
            {
                dic.Add(x.Key, x.Value);
            }
        });
    }

    // Token: 0x060000A3 RID: 163 RVA: 0x000080F8 File Offset: 0x000062F8
    public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dic, IDictionary<TKey, TValue> dicToAdd)
    {
        dicToAdd.ForEach(delegate(KeyValuePair<TKey, TValue> x) { dic.Add(x.Key, x.Value); });
    }

    // Token: 0x060000A4 RID: 164 RVA: 0x00008128 File Offset: 0x00006328
    public static bool ContainsKeys<TKey, TValue>(this IDictionary<TKey, TValue> dic, IEnumerable<TKey> keys)
    {
        bool result = false;
        keys.ForEachOrBreak(delegate(TKey x)
        {
            result = dic.ContainsKey(x);
            return result;
        });
        return result;
    }

    // Token: 0x060000A5 RID: 165 RVA: 0x00008168 File Offset: 0x00006368
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (T t in source)
        {
            action(t);
        }
    }

    // Token: 0x060000A6 RID: 166 RVA: 0x000081B8 File Offset: 0x000063B8
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