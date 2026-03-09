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

    public static T FindObjectOfType<T>(bool stub = true) where T : UnityEngine.Object
    {
        T[] arr = FindObjectsOfType<T>();
        return arr.Length != 0 ? arr[0] : null;
    }
    public static T FindObjectOfType<T>() where T : WrappedBehaviour
    {
        T[] arr = FindObjectsOfType<T>();
        return arr.Length != 0 ? arr[0] : null;
    }
    public static T[] FindObjectsOfType<T>() where T : WrappedBehaviour
    {
        List<T> list = new List<T>();
        Il2CPPBehaviour[] il2cpp = FindObjectsOfType<Il2CPPBehaviour>();
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
    public static T[] FindObjectsOfType<T>(bool stub = true) where T : UnityEngine.Object
    {
        return ((UnityEngine.Object[])Il2CPPBehaviour.FindObjectsOfType(Il2CppType.From(typeof(T)))).Cast<T>().ToArray();
    }
    public Il2CPPBehaviour Wrapper { get; internal set; }
    public C GetComponent<C>() where C : Component
    {
        return Wrapper.GetComponent<C>();
    }
    public static T Instantiate<T>(T original, Transform parent, bool worldPositionStays = true) where T : WrappedBehaviour
    {
        return WrapperHelper.Instantiate(original, parent, worldPositionStays);
    }
    public Coroutine StartCoroutine(IEnumerator enumerator)
    {
        return Wrapper.StartCoroutine(enumerator.ToIL2CPP());
    }
    public void StopAllCoroutines(){
        Wrapper.StopAllCoroutines();
    }
    public static GameObject Instantiate(GameObject obj, Transform parent = null, bool positionstays = false)
    {
        return WrapperHelper.Instantiate(obj, parent, positionstays);
    }
    public static void Destroy(UnityEngine.Object Object)
    {
        GameObject.Destroy(Object);
    }

    public T AddComponent<T>() where T : Component
    {
        return Wrapper.AddComponent<T>();
    }
    public T AddComponent<T>(bool stub = true) where T : WrappedBehaviour
    {
        return gameObject.AddComponent<T>();
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