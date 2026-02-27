
using UnityEngine;
#if IL2CPP
using Il2CppSystem.Collections;
#endif
namespace NeoModLoader.AndroidCompatibilityModule;
#if IL2CPP
public class WrappedBehaviour
{
    public Transform transform
    {
        get =>  Wrapper.transform;
        
    }

    public GameObject gameObject
    {
        get => Wrapper.gameObject;
    }

    public string name
    {
        get => Wrapper.name;
        set => Wrapper.name = value;
    }

    public readonly Il2CPPBehaviour Wrapper;
    public C GetComponent<C>()
    {
        return Wrapper.GetComponent<C>();
    }

    public static T Instantiate<T>(T original, Transform parent, bool worldPositionStays = true) where T : WrappedBehaviour
    {
        return IL2CPPHelper.Instantiate(original, parent, worldPositionStays);
    }

    public static GameObject Instantiate(GameObject obj, Transform parent)
    {
        return GameObject.Instantiate(obj, parent);
    }
    public static void Destroy(GameObject Object)
    {
        GameObject.Destroy(Object);
    }
    public WrappedBehaviour(Il2CPPBehaviour Wrapper)
    {
        this.Wrapper = Wrapper;
    }

    public WrappedBehaviour()
    {
        Wrapper = new Il2CPPBehaviour();
    }
}
#else
public class WrappedBehaviour : MonoBehaviour{
}
#endif