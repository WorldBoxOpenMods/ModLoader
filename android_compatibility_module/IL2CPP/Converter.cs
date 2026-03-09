using System.Collections.Concurrent;
using System.Reflection;
using Il2CppSystem.Linq;

namespace NeoModLoader.AndroidCompatibilityModule;
using Il2CppSystem.Collections;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System = Il2CppSystem;
using Il2CppInterop.Runtime;
using UnityEngine;

/// <summary>
/// collection of tools to allow mods to work on il2cpp and mono on the same code
/// </summary>
public static class Converter
{
  
    public static D C<D>(Delegate func) where D : System.Delegate
    {
        return DelegateSupport.ConvertDelegate<D>(func);
    }

    public static System.ValueTuple<X, Y, Z> C<X, Y, Z>(ValueTuple<X, Y, Z> tuple)
    {
        return new System.ValueTuple<X, Y, Z>(tuple.Item1, tuple.Item2, tuple.Item3);
    }
    public static System.ValueTuple<X, Y> C<X, Y>(ValueTuple<X, Y> tuple)
    {
        return new System.ValueTuple<X, Y>(tuple.Item1, tuple.Item2);
    }
    public static System.Type C (this Type type)
    {
        return Il2CppType.From(type);
    }

    public static Type C(this System.Type type)
    {
        return Type.GetType(type.AssemblyQualifiedName);
    }

    #region  Arrays
    public static A[] C<A>(this Il2CppArrayBase<A> arr)
    {
        return arr;
    }
    //List.Of crashes game for some reason
    public static System.Collections.Generic.List<T> CreateList<T>(params T[] arr)
    {
        System.Collections.Generic.List<T> list = new System.Collections.Generic.List<T>();
        foreach (var t in arr)
        {
            list.Add(t);
        }

        return list;
    }
    public static System.Nullable<A> Nullify<A>(this A a) where A : new()
    {
        return new System.Nullable<A>(a);
    }
    public static Il2CppReferenceArray<A> A<A>(params A[] arr) where A : Il2CppObjectBase
    {
        return arr;
    }
    public static Il2CppStringArray A(params string[] arr)
    {
        return arr;
    }
    public static T Cast<T>( Il2CppObjectBase obj) where T : Il2CppObjectBase
    {
        return obj.Cast<T>();
    }
    public static Il2CppReferenceArray<A> C<A>(this A[] arr) where A : Il2CppObjectBase
    {
        return arr;
    }
    public static Il2CppStringArray C(this string[] arr)
    {
        return arr;
    }
    #endregion
    public static System.Exception C(this Exception e)
    {
        return new System.Exception(e.Message);
    }
    
    public static System.Collections.Generic.HashSet<E> C<E>(this HashSet<E> set)
    {
        System.Collections.Generic.HashSet<E> hash = new();
        foreach (var VARIABLE in set)
        {
            hash.Add(VARIABLE);
        }
        return hash;
    }
    public static HashSet<E> C<E>(this System.Collections.Generic.HashSet<E> set)
    {
        HashSet<E> hash = new();
        foreach (var VARIABLE in set)
        {
            hash.Add(VARIABLE);
        }
        return hash;
    }
    public static System.Collections.Generic.List<E> C<E>(this List<E> e)
    {
        System.Collections.Generic.List<E> list = new System.Collections.Generic.List<E>();
        foreach (var item in e)
        {
            list.Add(item);
        }
        return list;
    }
    public static List<E> C<E>(this System.Collections.Generic.List<E> e)
    {
        List<E> list = new List<E>();
        foreach (var item in e)
        {
            list.Add(item);
        }
        return list;
    }
    public static Dictionary<key, value> C<key, value>(this System.Collections.Generic.Dictionary<key, value> e) 
    {
        Dictionary<key, value> dictionary = new Dictionary<key, value>();
        foreach (var item in e)
        {
            dictionary.Add(item.Key, item.Value);
        }
        return dictionary;
    }
    public static System.Collections.Generic.Dictionary<key, value> C<key, value>(this Dictionary<key, value> e) 
    {
        System.Collections.Generic.Dictionary<key, value> dictionary = new System.Collections.Generic.Dictionary<key, value>();
        foreach (var item in e)
        {
            dictionary.Add(item.Key, item.Value);
        }
        return dictionary;
    }
    public static GameObject CreateGameObject(string name, params Type[] types)
    {
        Il2CppSystem.Type[] Types = new Il2CppSystem.Type[types.Length];
        List<Type> WrappedTypes = new List<Type>();
        for(int i = 0; i< types.Length; i++)
        {
            if(typeof(WrappedBehaviour).IsAssignableFrom(types[i]))
            {
                WrappedTypes.Add(types[i]);
                Types[i] = typeof(Il2CPPBehaviour).C();
            }
            else
            {
                Types[i] = types[i].C();
            }
        }
        GameObject obj = new GameObject(name, Types);
        if (WrappedTypes.Count <= 0) return obj;
        {
            var behs = obj.GetComponents<Il2CPPBehaviour>();
            for (int i = 0; i < WrappedTypes.Count; i++)
            {
                behs[i].CreateWrapperIfNull(WrappedTypes[i]);
            }
        }
        return obj;
    }

    public static System.Collections.Generic.List<T> L<T>(params T[] arr)
    {
        System.Collections.Generic.List<T> list = new();
        foreach (var t in arr)
        {
            list.Add(t);
        }

        return list;
    }
    public static System.Collections.Generic.HashSet<T> H<T>(params T[] arr)
    {
        System.Collections.Generic.HashSet<T> list = new();
        foreach (var t in arr)
        {
            list.Add(t);
        }

        return list;
    }
}