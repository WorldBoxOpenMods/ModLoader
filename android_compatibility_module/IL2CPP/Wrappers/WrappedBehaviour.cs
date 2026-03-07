using System.Collections;
using Il2CppInterop.Runtime;
using UnityEngine;

namespace NeoModLoader.AndroidCompatibilityModule;

public class WrappedBehaviour
{
    public Transform transform => Wrapper.transform;

    public GameObject gameObject => Wrapper.gameObject;
    public string name
    {
        get => Wrapper.name;
        set => Wrapper.name = value;
    }

    public static void DontDestroyOnLoad(GameObject gameObject)
    {
        Il2CPPBehaviour.DontDestroyOnLoad(gameObject);
    }

    public static UnityEngine.Object FindObjectOfType(Type type)
    {
        return Il2CPPBehaviour.FindObjectOfType(Il2CppType.From(type));
    }
    public static T FindObjectOfType<T>() where T : WrappedBehaviour
    {
        T[] arr = FindObjectsOfType<T>();
        return arr.Length != 0 ? arr[0] : null;
    }
    public static T[] FindObjectsOfType<T>() where T : WrappedBehaviour
    {
        List<T> list = new List<T>();
        Il2CPPBehaviour[] il2cpp = FindObjectsOfType(typeof(Il2CPPBehaviour)).Cast<Il2CPPBehaviour>().ToArray();
        Type type = typeof(T);
        foreach (var beh in il2cpp)
        {
            if (beh.WrappedBehaviour.GetType().IsAssignableTo(type))
            {
                list.Add((T)beh.WrappedBehaviour);
            }
        }

        return list.ToArray();
    }
    public static UnityEngine.Object[] FindObjectsOfType(Type type)
    {
        return Il2CPPBehaviour.FindObjectsOfType(Il2CppType.From(type));
    }
    public Il2CPPBehaviour Wrapper { get; internal set; }
    public C GetComponent<C>() where C : Component
    {
        return Wrapper.GetComponent<C>();
    }
    public static T Instantiate<T>(T original, Transform parent, bool worldPositionStays = true) where T : WrappedBehaviour
    {
        return Extentions.Instantiate(original, parent, worldPositionStays);
    }
    public Coroutine StartCoroutine(IEnumerator enumerator)
    {
        return Wrapper.StartCoroutine(enumerator.ToIL2CPP());
    }
    public void StopAllCoroutines(){
        Wrapper.StopAllCoroutines();
    }
    public static GameObject Instantiate(GameObject obj, Transform parent)
    {
        return GameObject.Instantiate(obj, parent);
    }
    public static void Destroy(UnityEngine.Object Object)
    {
        GameObject.Destroy(Object);
    }
    public static implicit operator Il2CPPBehaviour(WrappedBehaviour beh)
    {
        return beh.Wrapper;
    }
    public static implicit operator WrappedBehaviour(Il2CPPBehaviour beh)
    {
        return beh.WrappedBehaviour;
    }
    public WrappedBehaviour()
    {
       
    }
}