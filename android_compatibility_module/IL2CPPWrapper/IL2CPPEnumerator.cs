using System.Collections;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.Injection;
using Object = Il2CppSystem.Object;

namespace NeoModLoader.AndroidCompatibilityModule.IL2CPPWrapper;

public class IL2CPPEnumerator : Il2CppSystem.Collections.IEnumerator
{
    internal IEnumerator _enumerator;
    public IL2CPPEnumerator(IntPtr pointer) : base(pointer)
    {
    }
    public IL2CPPEnumerator() : base(ClassInjector.DerivedConstructorPointer<IL2CPPEnumerator>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }
    [HideFromIl2Cpp]
    public IL2CPPEnumerator(IEnumerator inner) : this()
    {
        _enumerator = inner;
    }
    public override bool MoveNext()
    {
        return _enumerator.MoveNext();
    }

    public override Object Current => (Object)_enumerator.Current;
    public override void Reset()
    {
        _enumerator.Reset();
    }
}