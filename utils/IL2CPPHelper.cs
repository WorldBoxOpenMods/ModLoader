namespace NeoModLoader.utils;
#if !IL2CPP
using System = System;
#else
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System = Il2CppSystem;
using Il2CppInterop.Runtime;
#endif
/// <summary>
/// collection of tools to allow mods to work on il2cpp and mono on the same code
/// </summary>
public static class IL2CPPHelper
{
    #if IL2CPP
    public static D Convert<D>(this Delegate func) where D : System.Delegate
    {
        return DelegateSupport.ConvertDelegate<D>(func);
    }
    public static A[] Convert<A>(this Il2CppArrayBase<A> arr)
    {
        return arr;
    }
    public static Il2CppReferenceArray<A> Convert<A>(this A[] arr) where A : Il2CppObjectBase
    {
        return arr;
    }
    public static Il2CppStructArray<A> ConvertStruct<A>(this A[] arr) where A : unmanaged
    {
        return arr;
    }
    public static Il2CppStringArray ConvertString<A>(this string[] arr)
    {
        return arr;
    }
    public static System.Type Convert(this Type type)
    {
        return Il2CppType.From(type);
    }
    public static System.Exception Convert(this Exception e)
    {
        return new System.Exception(e.Message);
    }

    public static System.Collections.Generic.HashSet<E> Convert<E>(this HashSet<E> set)
    {
        System.Collections.Generic.HashSet<E> hash = new();
        foreach (var VARIABLE in set)
        {
            hash.Add(VARIABLE);
        }
        return hash;
    }
    public static System.Collections.Generic.List<E> Convert<E>(this List<E> e)
    {
        System.Collections.Generic.List<E> list = new System.Collections.Generic.List<E>();
        foreach (var item in e)
        {
            list.Add(item);
        }
        return list;
    }
    public static List<E> Convert<E>(this System.Collections.Generic.List<E> e)
    {
        List<E> list = new List<E>();
        foreach (var item in e)
        {
            list.Add(item);
        }
        return list;
    }
    public static Dictionary<key, value> Convert<key, value>(this System.Collections.Generic.Dictionary<key, value> e) 
    {
        Dictionary<key, value> dictionary = new Dictionary<key, value>();
        foreach (var item in e)
        {
            dictionary.Add(item.Key, item.Value);
        }
        return dictionary;
    }
    #else
    public static D Convert<D>(this Delegate func) where D : System.Delegate
    {
        return (D)func;
    }
    public static System.Type Convert(this Type type)
    {
        return type;
    }
        public static E Convert<E>(this E e) where E : Exception
    {
        return e;
    }
    public static A[] Convert<A>(this A[] array)
    {
        return array;
    }
    public static List<E> Convert<E>(this List<E> e)
    {
        return e;
    }
    public static Dictionary<key, value> Convert<key, value>(this Dictionary<key, value> e) 
    {
        return e;
    }
    public static HashSet<E> Convert<E>(this HashSet<E> set){
    return set;
    }
    #endif
}