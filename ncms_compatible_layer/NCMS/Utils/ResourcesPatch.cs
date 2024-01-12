using Object = UnityEngine.Object;

namespace NCMS.Utils;

#pragma warning disable CS1591 // No comment for NCMS compatible layer

/// <summary>
///     There are mods use reflection to patch resources manually. So keep it.
/// </summary>
public class ResourcesPatch
{
    internal static Dictionary<string, Object> modsResources;
    internal static Dictionary<string, Object> modsResourcesReplace = new();
}