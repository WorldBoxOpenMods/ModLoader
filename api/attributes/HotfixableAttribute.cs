namespace NeoModLoader.api.attributes;
/// <summary>
/// If a method has this attribute and its mod main class implements <see cref="IReloadable"/>, the method will be hotfixed when the mod is reloaded.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class HotfixableAttribute : Attribute
{
    
}