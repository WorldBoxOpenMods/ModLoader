using System.Reflection;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using NeoModLoader.services;
using UnityEngine;
using Object = Il2CppSystem.Object;

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
        onEnable?.Invoke(WrappedBehaviour);
    }

    public void Start()
    {
       start?.Invoke(WrappedBehaviour);
    }

    public void OnDisable()
    {
        onDisable?.Invoke(WrappedBehaviour);
    }

    private bool canawake;
    public void Awake()
    {
        if (!canawake) return;
        awake?.Invoke(WrappedBehaviour);
        canawake = false;
    }
    public void Update()
    {
        update?.Invoke(WrappedBehaviour);
    }

    public void OnGUI()
    {
        ongui?.Invoke(WrappedBehaviour);
    }
    [HideFromIl2Cpp]
    public WrappedAction GetWrappedMethod(string Method)
    {
        Type type = WrappedType;
        while (type != null)
        {
            var method = type.GetMethod(
                Method,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly, Type.EmptyTypes);
            if (method != null)
                return WrapperHelper.CreateWrappedAction(method);
            type = type.BaseType;
        }
        return null;
    }
    [HideFromIl2Cpp]
    internal B SetWrappedBehaviour<B>(B Behaviour) where B : WrappedBehaviour
    {
        WrappedBehaviour = Behaviour;
        WrappedType = Behaviour.GetType();
        Behaviour.Wrapper = this;
        update = GetWrappedMethod("Update");
        start = GetWrappedMethod("Start");
        awake = GetWrappedMethod("Awake");
        canawake = true;
        if (gameObject.activeInHierarchy)
        {
            Awake();
        }
        ongui = GetWrappedMethod("OnGUI");
        onEnable = GetWrappedMethod("OnEnable");
        onDisable = GetWrappedMethod("OnDisable");
        return Behaviour;
    }
    [HideFromIl2Cpp]
    internal WrappedBehaviour CreateWrapperIfNull(Type WrappedType)
    {
        return WrappedBehaviour ?? SetWrappedBehaviour((WrappedBehaviour)Activator.CreateInstance(WrappedType));
    }
    private WrappedAction update;
    private WrappedAction start;
    private WrappedAction awake;
    private WrappedAction ongui;
    private WrappedAction onEnable;
    private WrappedAction onDisable;
    public Type WrappedType { [HideFromIl2Cpp] get; [HideFromIl2Cpp] private set; }
    public WrappedBehaviour WrappedBehaviour {  [HideFromIl2Cpp]get;  [HideFromIl2Cpp]private set; }
}
public delegate void WrappedAction(WrappedBehaviour beh);