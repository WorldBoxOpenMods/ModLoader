using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
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
    public static T Instantiate<T>(T original, Transform parent, bool worldPositionStays = true) where T : WrappedBehaviour
    { 
        Il2CPPBehaviour il2cpp = UnityEngine.Object.Instantiate(original.Wrapper, parent, worldPositionStays);
        WrapperResolver.ResolveInstantiate(original.Wrapper.gameObject, il2cpp.gameObject);
        return (T)il2cpp.WrappedBehaviour;
    }
    public static T AddComponent<T>(this GameObject gameObject) where T : WrappedBehaviour
    {
        Il2CPPBehaviour behaviour = gameObject.AddComponent<Il2CPPBehaviour>();
        return behaviour.CreateWrapperIfNull(typeof(T)) as T;
    }
    public static WrappedBehaviour AddComponent(this GameObject gameObject, Type type)
    {
        Il2CPPBehaviour behaviour = gameObject.AddComponent<Il2CPPBehaviour>();
        return behaviour.CreateWrapperIfNull(type);
    }
}