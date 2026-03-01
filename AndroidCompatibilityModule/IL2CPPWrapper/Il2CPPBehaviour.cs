using System.Reflection;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using UnityEngine;

namespace NeoModLoader.AndroidCompatibilityModule;
[RegisterTypeInIl2Cpp]
public class Il2CPPBehaviour : MonoBehaviour
{
    public Il2CPPBehaviour(IntPtr ptr) : base(ptr)
    {
    }

    public Il2CPPBehaviour() : base(ClassInjector.DerivedConstructorPointer<Il2CPPBehaviour>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }

    public void OnEnable()
    {
        onEnable?.Invoke(WrappedBehaviour, null);
    }

    public void Start()
    {
       start?.Invoke(WrappedBehaviour, null);
    }

    public void OnDisable()
    {
        onDisable?.Invoke(WrappedBehaviour, null);
    }

    public void Awake()
    {
        awake?.Invoke(WrappedBehaviour, null);
    }
    public void Update()
    {
        update?.Invoke(WrappedBehaviour, null);
    }
    [HideFromIl2Cpp]
    public MethodInfo GetWrappedMethod(string Method)
    {
        return typeof(WrappedBehaviour).GetMethod(Method);
    }
    [HideFromIl2Cpp]
    internal WrappedBehaviour SetWrappedBehaviour(WrappedBehaviour Behaviour)
    {
        WrappedBehaviour = Behaviour;
        Behaviour.Wrapper = this;
        update = GetWrappedMethod("Update");
        start = GetWrappedMethod("Start");
        awake = GetWrappedMethod("Awake");
        onEnable = GetWrappedMethod("OnEnable");
        onDisable = GetWrappedMethod("OnDisable");
        return Behaviour;
    }
    private MethodInfo update;
    private MethodInfo start;
    private MethodInfo awake;
    private MethodInfo onEnable;
    private MethodInfo onDisable;
    public WrappedBehaviour WrappedBehaviour {  [HideFromIl2Cpp]get;  [HideFromIl2Cpp]private set; }
}