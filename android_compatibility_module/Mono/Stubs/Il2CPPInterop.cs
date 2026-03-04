namespace  Il2CppInterop.Runtime.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property |
                AttributeTargets.Event)]
public class HideFromIl2CppAttribute : Attribute
{
}