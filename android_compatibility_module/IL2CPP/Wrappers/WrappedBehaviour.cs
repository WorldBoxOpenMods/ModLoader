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

    public static T FindObjectOfType<T>() where T : UnityEngine.Object
    {
        return Il2CPPBehaviour.FindObjectOfType<T>();
    }
    public static T[] FindObjectsOfType<T>() where T : UnityEngine.Object
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