using UnityEngine;
using Object = UnityEngine.Object;

namespace NeoModLoader.utils;

public class WrappedAssetBundle
{
    private readonly AssetNode root = new();
    private AssetBundle assetBundle;

    public WrappedAssetBundle(AssetBundle ab)
    {
        assetBundle = ab;
        string[] names = ab.GetAllAssetNames();
        foreach (string name in names)
        {
            string[] parts = name.Split('/');
            AssetNode node = root;
            for (int i = 0; i < parts.Length - 1; i++)
            {
                string part = parts[i];
                if (!node.children.ContainsKey(part))
                {
                    node.children[part] = new AssetNode();
                }

                node = node.children[part];
            }

            node.resources_full_names.Add(name);
        }
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

    public Object[] GetAllObjects(string path, Type systemTypeInstance)
    {
        AssetNode node = root;
        string[] parts = path.Split('/');
        for (int i = 0; i < parts.Length; i++)
        {
            string part = parts[i];
            if (!node.children.ContainsKey(part))
            {
                return null;
            }

            node = node.children[part];
        }

        if (node.resources_full_names.Count == 0)
        {
            return null;
        }

        List<Object> objects = new();
        foreach (string name in node.resources_full_names)
        {
            Object obj = assetBundle.LoadAsset(name, systemTypeInstance);
            if (obj != null)
            {
                objects.Add(obj);
            }
        }

        return objects.ToArray();
    }

    private class AssetNode
    {
        public readonly Dictionary<string, AssetNode> children = new();
        public readonly List<string> resources_full_names = new();
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