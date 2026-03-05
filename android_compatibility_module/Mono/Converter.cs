using System.Collections;
using UnityEngine;

namespace NeoModLoader.AndroidCompatibilityModule;

public static class Converter
{
    public static A C<A>(this A a)
    {
        return a;
    }
    public static A[] A<A>(params A[] a)
    {
        return a;
    }
    public static A? Nullify<A>(this A a) where A : struct
    {
        return a;
    }
    public static T Cast<T>( object obj)
    {
        return (T)obj;
    }
     public static D C<D>(Delegate func) where D : System.Delegate
     {
         return (D)func;
     }
    public static IEnumerator<T> Enumerate<T>(this IEnumerable<T> Object)
    {
        return Object.GetEnumerator();
    }
    public static IEnumerator ToIL2CPP(this IEnumerator enumerator)
    {
        return enumerator;
    }
    public static GameObject CreateGameObject(string name, params Type[] types)
    {
        return new GameObject(name, types);
    }
   
}