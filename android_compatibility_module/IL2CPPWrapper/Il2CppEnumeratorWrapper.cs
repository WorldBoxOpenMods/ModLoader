using Il2CppInterop.Runtime;
using Il2CppSystem.Collections;
using NeoModLoader.services;
using IEnumerable = System.Collections.IEnumerable;

namespace NeoModLoader.AndroidCompatibilityModule.IL2CPPWrapper;

public class Il2CppEnumeratorWrapper<T> : IEnumerator<T>, IEnumerable<T> where T : Il2CppSystem.Object
{
    private IEnumerator _inner;

    public Il2CppEnumeratorWrapper(IEnumerator inner)
    {
        _inner = inner;
    }

    public T Current => (T)_inner.Current;

    object System.Collections.IEnumerator.Current => _inner.Current;

    public bool MoveNext() => _inner.MoveNext();

    public void Reset() => _inner.Reset();

    public void Dispose()
    {
        _inner = null;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return this;
    }

    System.Collections.IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}