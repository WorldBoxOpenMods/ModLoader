namespace NeoModLoader.utils;

/// <summary>
/// A simple PriorityQueue implementation with binary heap, not thread-safe
/// </summary>
/// <typeparam name="T"></typeparam>
public class PriorityQueue<T>
{
    private IComparer<T> comparer;
    private T[] heap;
    private int size;
    /// <summary>
    /// Current size of the PriorityQueue
    /// </summary>
    public int Count => size;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="capacity"></param>
    /// <param name="comparer"></param>
    public PriorityQueue(int capacity, IComparer<T> comparer)
    {
        this.comparer = comparer;
        this.heap = new T[capacity > 0 ? capacity : 8];
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
    /// View the top element of the PriorityQueue
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public T Peek()
    {
        if (size == 0)
        {
            throw new InvalidOperationException("PriorityQueue is empty");
        }
        return heap[0];
    }
    /// <summary>
    /// Enqueue an element into the PriorityQueue
    /// </summary>
    /// <param name="x"></param>
    public void Enqueue(T x)
    {
        if (size == heap.Length)
        {
            Array.Resize(ref heap, size << 1);
        }
        size++;
        heap[size - 1] = x;
        SiftUp(size - 1);
    }
    private void SiftUp(int i)
    {
        T x = heap[i];
        while (i > 0)
        {
            int p = Parent(i);
            if (comparer.Compare(x, heap[p]) >= 0)
            {
                break;
            }
            heap[i] = heap[p];
            i = p;
        }
        heap[i] = x;
    }
    /// <summary>
    /// Dequeue an element from the PriorityQueue
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public T Dequeue()
    {
        if (size == 0)
        {
            throw new InvalidOperationException("PriorityQueue is empty");
        }
        T x = heap[0];
        T y = heap[size - 1];
        size--;
        if (size != 0)
        {
            SiftDown(0, y);
        }
        return x;
    }
    
    private void SiftDown(int i, T x)
    {
        while (true)
        {
            int l = Left(i);
            if (l > size - 1)
            {
                break;
            }
            int r = l + 1;
            int c = (r > size - 1 || comparer.Compare(heap[l], heap[r]) <= 0) ? l : r;
            if (comparer.Compare(x, heap[c]) <= 0)
            {
                break;
            }
            heap[i] = heap[c];
            i = c;
        }
        heap[i] = x;
    }
}