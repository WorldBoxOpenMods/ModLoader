using UnityEngine;
using Object = UnityEngine.Object;

namespace NeoModLoader.utils;

public class WrappedAssetBundle
{
    private AssetBundle assetBundle;

    public WrappedAssetBundle(AssetBundle ab)
    {
        assetBundle = ab;
    }
    public Object GetObject(string name)
    {
        return assetBundle.LoadAsset(name);
    }
    public Object GetObject(string name, Type type)
    {
        return assetBundle.LoadAsset(name, type);
    }
    public Object[] GetAllObjects(Type systemTypeInstance)
    {
        return assetBundle.LoadAllAssets(systemTypeInstance);
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