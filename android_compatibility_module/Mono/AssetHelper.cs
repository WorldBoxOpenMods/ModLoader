namespace NeoModLoader.AndroidCompatibilityModule;

public static class AssetHelper<A> where A : Asset
{
    public static string[] a(params string[] pArgs)
    {
        return AssetLibrary<A>.a(pArgs);
    }
}