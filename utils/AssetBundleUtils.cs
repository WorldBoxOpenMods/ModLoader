using NeoModLoader.services;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NeoModLoader.utils;

/// <summary>
///     The class provides a wrapped asset bundle for easier use.
/// </summary>
public class WrappedAssetBundle
{
    private readonly AssetBundle assetBundle;
    private readonly Dictionary<string, AssetNode> direct_visit = new();
    private readonly AssetNode root = new();

    internal WrappedAssetBundle(AssetBundle ab)
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
                if (!node.children.TryGetValue(part, out var child))
                {
                    child = new AssetNode();
                    node.children[part] = child;
                }

                node = child;
            }

            node.resources_full_names.Add(name);
        }
    }

    /// <summary>
    ///     Name of the asset bundle.
    /// </summary>
    public string Name => assetBundle.name;

    /// <summary>
    ///     Same with <see cref="AssetBundle.GetAllAssetNames" />
    /// </summary>
    /// <returns></returns>
    public string[] GetAllAssetNames()
    {
        return assetBundle.GetAllAssetNames();
    }

    /// <summary>
    ///     Same with <see cref="AssetBundle.GetAllScenePaths" />
    /// </summary>
    /// <returns></returns>
    public string[] GetAllScenePaths()
    {
        return assetBundle.GetAllScenePaths();
    }

    /// <summary>
    ///     Same with <see cref="AssetBundle.LoadAsset(string)" />
    /// </summary>
    /// <returns></returns>
    public Object GetObject(string pName)
    {
        return assetBundle.LoadAsset(pName);
    }

    /// <summary>
    ///     Same with <see cref="AssetBundle.LoadAsset(string, Type)" />
    /// </summary>
    /// <returns></returns>
    public Object GetObject(string pName, Type type)
    {
        return assetBundle.LoadAsset(pName, type);
    }

    /// <summary>
    ///     Same with <see cref="AssetBundle.LoadAsset{T}(string)" />
    /// </summary>
    /// <returns></returns>
    public T GetObject<T>(string pName) where T : Object
    {
        return assetBundle.LoadAsset<T>(pName);
    }

    /// <summary>
    ///     Same with <see cref="AssetBundle.LoadAllAssets(Type)" />
    /// </summary>
    /// <returns></returns>
    public Object[] GetAllObjects(Type pType)
    {
        return assetBundle.LoadAllAssets(pType);
    }

    /// <summary>
    ///     Same with <see cref="AssetBundle.LoadAllAssets{T}" />
    /// </summary>
    /// <returns></returns>
    public T[] GetAllObjects<T>() where T : Object
    {
        return assetBundle.LoadAllAssets<T>();
    }

    /// <summary>
    ///     Get all objects typed <paramref name="pType" /> under path <paramref name="pPath" />
    /// </summary>
    /// <returns></returns>
    public Object[] GetAllObjects(string pPath, Type pType)
    {
        AssetNode node;
        pPath = pPath.ToLower();
        if (!direct_visit.TryGetValue(pPath, out node))
        {
            node = root;
            string[] parts = pPath.ToLower().Split('/');
            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                if (!node.children.ContainsKey(part))
                {
                    return null;
                }

                node = node.children[part];
            }

            direct_visit[pPath] = node;
        }

        if (node.resources_full_names.Count == 0)
        {
            return null;
        }

        List<Object> objects = new();
        foreach (string name in node.resources_full_names)
        {
            Object obj = assetBundle.LoadAsset(name, pType);
            if (obj != null)
            {
                objects.Add(obj);
            }
        }

        return objects.ToArray();
    }

    /// <summary>
    ///     Get all objects typed <typeparamref name="T" /> under path <paramref name="pPath" />
    /// </summary>
    /// <returns></returns>
    public T[] GetAllObjects<T>(string pPath) where T : Object
    {
        AssetNode node;
        pPath = pPath.ToLower();
        if (!direct_visit.TryGetValue(pPath, out node))
        {
            node = root;
            string[] parts = pPath.ToLower().Split('/');
            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                if (!node.children.ContainsKey(part))
                {
                    return null;
                }

                node = node.children[part];
            }

            direct_visit[pPath] = node;
        }

        if (node.resources_full_names.Count == 0)
        {
            return null;
        }

        List<T> objects = new();
        foreach (string name in node.resources_full_names)
        {
            T obj = assetBundle.LoadAsset<T>(name);
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
public static class AssetBundleUtils
{
    private static readonly Dictionary<string, WrappedAssetBundle> LoadedAssetBundles = new();
    private static readonly Dictionary<string, WrappedAssetBundle> LoadedAssetBundlesByPath = new();

    /// <summary>
    ///     Get asset bundle by name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static WrappedAssetBundle GetAssetBundle(string name)
    {
        return LoadedAssetBundles[name];
    }

    /// <summary>
    ///     Load asset bundle from file manually.
    /// </summary>
    /// <param name="pPath"></param>
    /// <param name="pForceReload"></param>
    /// <returns></returns>
    public static WrappedAssetBundle LoadFromFile(string pPath, bool pForceReload = false)
    {
        FileInfo file_info = new(pPath);
        if (LoadedAssetBundlesByPath.ContainsKey(file_info.FullName) && !pForceReload)
        {
            return LoadedAssetBundlesByPath[file_info.FullName];
        }

        using Stream file = file_info.OpenRead();

        var ab = new WrappedAssetBundle(AssetBundle.LoadFromStream(file));

        LoadedAssetBundlesByPath[file_info.FullName] = ab;
        LoadedAssetBundles[ab.Name] = ab;

        return ab;
    }

    /// <summary>
    ///     Load all asset bundles in a folder manually.
    /// </summary>
    /// <param name="pFolder"></param>
    /// <returns></returns>
    public static WrappedAssetBundle[] LoadFromFolder(string pFolder)
    {
        DirectoryInfo directory_info = new(pFolder);
        FileInfo[] files = directory_info.GetFiles();
        List<WrappedAssetBundle> asset_bundles = new();

        // TODO: Read from manifest file and load in order.
        foreach (FileInfo file_info in files)
        {
            if (file_info.Extension != ".manifest")
            {
                try
                {
                    asset_bundles.Add(LoadFromFile(file_info.FullName));
                }
                catch (Exception e)
                {
                    LogService.LogError($"Failed to load asset bundle {file_info.FullName}.\n{e}");
                }
            }
        }

        return asset_bundles.ToArray();
    }
}