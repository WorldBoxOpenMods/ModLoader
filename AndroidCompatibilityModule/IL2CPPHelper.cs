namespace NeoModLoader.AndroidCompatibilityModule;
#if !IL2CPP
using System = System;
#else
using Il2CppSystem.Collections;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System = Il2CppSystem;
using Il2CppInterop.Runtime;
#endif
using UnityEngine;
/// <summary>
/// collection of tools to allow mods to work on il2cpp and mono on the same code
/// </summary>
public static class IL2CPPHelper
{
    #if IL2CPP
    public static D C<D>(Delegate func) where D : System.Delegate
    {
        return DelegateSupport.ConvertDelegate<D>(func);
    }
    public static System.Type C (this Type type)
    {
        return Il2CppType.From(type);
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
    public static HashSet<E> Convert<E>(this System.Collections.Generic.HashSet<E> set)
    {
        HashSet<E> hash = new();
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
    public static System.Collections.Generic.Dictionary<key, value> Convert<key, value>(this Dictionary<key, value> e) 
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
        for(int i = 0; i< types.Length; i++)
        {
            Types[i] = types[i].C();
        }
        return new GameObject(name, Types);
    }
    public static T Instantiate<T>(T original, Transform parent, bool worldPositionStays = true) where T : WrappedBehaviour
    {
        return (T) UnityEngine.Object.Instantiate(original.Wrapper, parent, worldPositionStays).WrappedBehaviour;
    }
    public static T AddComponent<T>(this GameObject gameObject) where T : WrappedBehaviour
    {
        Il2CPPBehaviour behaviour = gameObject.AddComponent<Il2CPPBehaviour>();
        return (T)behaviour.SetWrappedBehaviour((T)Activator.CreateInstance(typeof(T)));
    }
#else
    public static D C<D>(Delegate func) where D : Delegate
    {
        return (D)func;
    }
    public static System.Type C (this Type type)
    {
        return type;
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
     public static GameObject CreateGameObject(string name, params Type[] Types)
    {
        return new GameObject(name, Types);
    }
#endif
}