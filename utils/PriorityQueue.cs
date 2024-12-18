using System.Collections;

namespace NeoModLoader.utils;

/// <summary>
///     A simple PriorityQueue implementation with binary heap, not thread-safe
/// </summary>
/// <typeparam name="T"></typeparam>
public class PriorityQueue<T> : IEnumerable<T>
{
    private readonly IComparer<T> comparer;
    private          T[]          heap;

    /// <summary>
    /// </summary>
    /// <param name="capacity"></param>
    /// <param name="comparer"></param>
    public PriorityQueue(int capacity, IComparer<T> comparer)
    {
        this.comparer = comparer;
        heap = new T[capacity > 0 ? capacity : 8];
    }

    /// <summary>
    ///     Current size of the PriorityQueue
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    ///     Get value
    /// </summary>
    /// <param name="index"></param>
    /// <exception cref="IndexOutOfRangeException"></exception>
    public T this[int index]
    {
        get
        {
            if (index > Count || index < 0) throw new IndexOutOfRangeException($"{index} / {Count}");

            return heap[index];
        }
    }

    /// <summary>
    ///     Get the enumerator of the PriorityQueue
    /// </summary>
    /// <returns></returns>
    public IEnumerator<T> GetEnumerator()
    {
        return (IEnumerator<T>)(heap.GetEnumerator() as IEnumerator<T> ?? Array.Empty<T>().GetEnumerator());
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private static int Parent(int i)
    {
        return (i - 1) >> 1;
    }

    private static int Left(int i)
    {
        return (i << 1) + 1;
    }

    /// <summary>
    ///     View the top element of the PriorityQueue
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public T Peek()
    {
        if (Count == 0) throw new InvalidOperationException("PriorityQueue is empty");

        return heap[0];
    }

    /// <summary>
    ///     Enqueue an element into the PriorityQueue
    /// </summary>
    /// <param name="x"></param>
    public int Enqueue(T x)
    {
        if (Count == heap.Length) Array.Resize(ref heap, Count << 1);

        Count++;
        heap[Count - 1] = x;
        return SiftUp(Count - 1);
    }

    private int SiftUp(int i)
    {
        T x = heap[i];
        while (i > 0)
        {
            var p = Parent(i);
            if (comparer.Compare(x, heap[p]) >= 0) break;

            heap[i] = heap[p];
            i = p;
        }

        heap[i] = x;
        return i;
    }

    /// <summary>
    ///     Dequeue an element from the PriorityQueue
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public T Dequeue()
    {
        if (Count == 0) throw new InvalidOperationException("PriorityQueue is empty");

        T x = heap[0];
        T y = heap[Count - 1];
        Count--;
        if (Count != 0) SiftDown(0, y);

        return x;
    }

    private void SiftDown(int i, T x)
    {
        while (true)
        {
            var l = Left(i);
            if (l > Count - 1) break;

            var r = l + 1;
            var c = r > Count - 1 || comparer.Compare(heap[l], heap[r]) <= 0 ? l : r;
            if (comparer.Compare(x, heap[c]) <= 0) break;

            heap[i] = heap[c];
            i = c;
        }

        heap[i] = x;
    }
}