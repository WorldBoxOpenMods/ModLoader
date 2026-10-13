using System.Collections;

namespace NeoModLoader.utils.Collections;
/// <summary>
/// simple implementation of a list x hashset
/// </summary>
/// <typeparam name="T"></typeparam>
public class HashList<T> : IList<T>, ISet<T>
{
    private List<T> list = new();
    private HashSet<T> set = new();
    public IEnumerator<T> GetEnumerator()
    {
        return list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Sort(IComparer<T> comparer = null)
    {
        list.Sort(comparer);
    }
    public void Add(T item)
    {
        if (set.Add(item))
        {
            list.Add(item);
        }
    }

    public void ExceptWith(IEnumerable<T> other)
    {
        foreach (var item in other)
        {
            Remove(item);
        }
    }

    public void IntersectWith(IEnumerable<T> other)
    {
        HashSet<T> temp = new(other);
        for (int i = 0; i < Count; i++)
        {
            var item = this[i];
            if (temp.Contains(item)) continue;
            RemoveAt(i, item);
            i--;
        }
    }
    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        foreach (var item in other)
        {
            var index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index, item);
            }
            else
            {
                Add(item);
            }
        }
    }

    public void UnionWith(IEnumerable<T> other)
    {
        foreach (var item in other)
        {
            Add(item);
        }
    }
    public bool IsProperSubsetOf(IEnumerable<T> other)
    {
        return set.IsProperSubsetOf(other);
    }

    public bool IsProperSupersetOf(IEnumerable<T> other)
    {
        return set.IsProperSupersetOf(other);
    }

    public bool IsSubsetOf(IEnumerable<T> other)
    {
        return set.IsSubsetOf(other);
    }

    public bool IsSupersetOf(IEnumerable<T> other)
    {
        return set.IsSupersetOf(other);
    }

    public bool Overlaps(IEnumerable<T> other)
    {
        return set.Overlaps(other);
    }

    public bool SetEquals(IEnumerable<T> other)
    {
        return set.SetEquals(other);
    }

    bool ISet<T>.Add(T item)
    {
        if (set.Add(item))
        {
            list.Add(item);
            return true;
        }
        return false;
    }
    public void Clear()
    {
       list.Clear();
       set.Clear();
    }

    public bool Contains(T item)
    {
        return set.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
         list.CopyTo(array, arrayIndex);
    }

    public bool Remove(T item)
    {
        if (!set.Remove(item)) return false;
        list.Remove(item);
        return true;
    }

    public int Count
    {
        get => list.Count;
    }
    public bool IsReadOnly
    {
        get => false;
    }
    public int IndexOf(T item)
    {
        return list.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        if (index < 0 || index > Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        if (set.Add(item))
        {
            list.Insert(index, item);
        }
    }

    public void RemoveAt(int index)
    {
        if (index < 0 || index >= Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        RemoveAt(index, list[index]);
    }
    void RemoveAt(int index, T item)
    {
        set.Remove(item);
        list.RemoveAt(index);
    }

    public void Set(int index, T value)
    {
        var old = list[index];
        if (Equals(old, value))
            return;
        if (set.Contains(value))
            return;
        list[index] = value;
        set.Remove(old);
        set.Add(value);
    }
    public T this[int index]
    {
        get => list[index];
        set => Set(index, value);
    }
}