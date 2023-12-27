using Object = UnityEngine.Object;

namespace NCMS.Utils;

#pragma warning disable CS1591 // No comment for NCMS compatible layer
public class ResourcesPatch
{
    internal static Dictionary<string, Object> modsResources;
    internal static Dictionary<string, Object> modsResourcesReplace = new();
}