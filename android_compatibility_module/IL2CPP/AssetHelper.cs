using System.Runtime.InteropServices;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace NeoModLoader.AndroidCompatibilityModule;
public static class AssetHelper<A> where A : Asset
{
    public static unsafe Il2CppStringArray a(params string[] pArgs)
    {
        return new Il2CppStringArray(AssetLibrary<A>.a(pArgs.C()));
    }
}