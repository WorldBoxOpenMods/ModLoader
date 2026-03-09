using System.Collections;
using System.Reflection;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Linq;
using UnityEngine;
using UnityEngine.Events;
using IEnumerator = Il2CppSystem.Collections.IEnumerator;

namespace NeoModLoader.AndroidCompatibilityModule;
public static class Extentions
{
    public static bool IsValid(this Il2CppArrayBase arr)
    {
        return arr is { Length: > 0 };
    }
    //il2cpp array's indexof() is not good, better to just check pointers
    public static int GetIndex<T>(this Il2CppReferenceArray<T> arr, T obj) where T : Il2CppObjectBase
    {
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i].Pointer == obj.Pointer)
            {
                return i;
            }
        }

        return -1;
    }
    public static Il2CppSystem.Collections.Generic.List<T>  ToList<T>(this Il2CppObjectBase Object) where T : Il2CppSystem.Object
    {
        var enumerable = Object.Cast<Il2CppSystem.Collections.Generic.IEnumerable<T>>();
        if (enumerable == null)
        {
            throw new ArgumentException($"IL2CPP Object of {Object.GetType()} cannot be enumerated!");
        }
        return enumerable.ToList();
    }

    public static IEnumerator ToIL2CPP(this global::System.Collections.IEnumerator enumerator)
    {
        return new IL2CPPEnumerator(enumerator).Cast<IEnumerator>();
    }
    public static nint GetPointer<T>(this T obj) where T : Il2CppObjectBase
    {
        return obj.Pointer;
    }
    public static Il2CppObjectBase Cast(this Il2CppObjectBase obj, Type type)
    {
        var method = typeof(Il2CppObjectBase)
            .GetMethod("Cast")
            .MakeGenericMethod(type);
        return (Il2CppObjectBase)method.Invoke(obj, null);
    }
    public static Component GetComponent(this GameObject obj, Type type, int index)
    {
        var arr = obj.GetComponents(type.C());
        if(!arr.IsValid()) return null;
        return (Component)arr[index].Cast(type);
    }
    public static T AddComponent<T>(this GameObject gameObject) where T : WrappedBehaviour
    {
        Il2CPPBehaviour behaviour = gameObject.AddComponent<Il2CPPBehaviour>();
        return behaviour.CreateWrapperIfNull(typeof(T)) as T;
    }
    public static List<Transform> GetChildren(this Transform transform)
    {
        List<Transform> list = new List<Transform>();
        for (int i = 0; i < transform.GetChildCount(); i++)
        {
            Transform child = transform.GetChild(i);
            list.Add(child);
        }
        return list;
    }
    public static T GetWrappedComponent<T>(this GameObject obj)
    {
        return (T) WrapperHelper.GetWrappedComponent(obj, typeof(T));
    }
    public static void AddListener(this UnityEvent action, Delegate func){
        action.AddListener(Converter.C<UnityAction>(func));
    }
    public static void AddListener<T>(this UnityEvent<T> action, Delegate func){
        action.AddListener(Converter.C<UnityAction<T>>(func));
    }
    public static WrappedBehaviour AddComponent(this GameObject gameObject, Type type)
    {
        Il2CPPBehaviour behaviour = gameObject.AddComponent<Il2CPPBehaviour>();
        return behaviour.CreateWrapperIfNull(type);
    }
}