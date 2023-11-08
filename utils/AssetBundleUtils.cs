using UnityEngine;

namespace NeoModLoader.utils;

public class WrappedAssetBundle
{
    private AssetBundle assetBundle;

    public WrappedAssetBundle(AssetBundle ab)
    {
        assetBundle = ab;
    }
}
/// <summary>
/// The class provides some useful methods for loading non-hidden AssetBundles to ResourceTree.
/// </summary>
internal static class AssetBundleUtils
{
    public static WrappedAssetBundle LoadFromFile(string path)
    {
        return new WrappedAssetBundle(AssetBundle.LoadFromFile(path));
    }
}