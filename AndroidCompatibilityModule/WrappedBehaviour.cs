
using UnityEngine;
#if IL2CPP
using Il2CppSystem.Collections;
#endif
namespace NeoModLoader.AndroidCompatibilityModule;
#if IL2CPP
public class WrappedBehaviour
{
    public Transform transform => Wrapper.transform;

    public GameObject gameObject => Wrapper.gameObject;
    public string name
    {
        get => Wrapper.name;
        set => Wrapper.name = value;
    }

    public Il2CPPBehaviour Wrapper { get; internal set; }
    public C GetComponent<C>() where C : Component
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

    public WrappedBehaviour()
    {
       
    }
}
#else
public class WrappedBehaviour : MonoBehaviour{
}
#endif