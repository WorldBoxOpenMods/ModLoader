using System.Globalization;
using HarmonyLib;
using NeoModLoader.api.exceptions;
using NeoModLoader.constants;
using NeoModLoader.General;
using NeoModLoader.services;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NeoModLoader.utils;

internal static class ResourcesPatch
{
    class ResourceTree
    {
        private ResourceTreeNode root = new();
        private Dictionary<string, UnityEngine.Object> direct_objects = new();
        public ResourceTreeNode Find(string path)
        {
            path = path.ToLower();

            string[] parts;
            if (path.EndsWith("/"))
            {
                parts = path.Substring(0, path.Length - 1).Split('/');
            }
            else
            {
                parts = path.Split('/');
            }

            var node = root;
            foreach (var part in parts)
            {
                if (!node.children.ContainsKey(part))
                {
                    return null;
                }

                node = node.children[part];
            }

            return node;
        }

        public UnityEngine.Object Get(string path)
        {
            return direct_objects.TryGetValue(path.ToLower(), out Object o) ? o : null;
        }

        public void Add(string path, string absPath)
        {
            string lower_path = path.ToLower();
            if (lower_path.EndsWith(".meta")) return;
            if (lower_path.EndsWith("sprites.json"))
            {
                return;
            }
            
            string parent_path = Path.GetDirectoryName(lower_path);
            UnityEngine.Object[] objs;
            try
            {
                string abs_lower_path = absPath.ToLower();
                objs = LoadResourceFile(ref absPath, ref abs_lower_path);

                foreach (var obj in objs)
                {
                    if (parent_path == null)
                    {
                        direct_objects[obj.name] = obj;
                    }
                    else
                    {
                        direct_objects[Path.Combine(parent_path, obj.name)] = obj;
                    }
                }
            }
            catch (UnrecognizableResourceFileException)
            {
                LogService.LogWarning($"Cannot recognize resource file {path}");
                return;
            }
            if(objs.Length == 0) return;

            string[] parts = lower_path.Split('/');
            var node = root;
            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (!node.children.ContainsKey(parts[i]))
                {
                    node.children[parts[i]] = new ResourceTreeNode();
                }

                node = node.children[parts[i]];
            }

            foreach (var obj in objs)
            {
                node.objects[obj.name] = obj;
            }
        }
    }

    class ResourceTreeNode
    {
        public readonly Dictionary<string, ResourceTreeNode> children = new();
        public readonly Dictionary<string, UnityEngine.Object> objects = new();
    }

    private static ResourceTree tree;
    internal static void Initialize()
    {
        tree = new ResourceTree();
    }

    public static UnityEngine.Object[] LoadResourceFile(ref string path, ref string pLowerPath)
    {
        if (pLowerPath.EndsWith(".png") || pLowerPath.EndsWith(".jpg") || pLowerPath.EndsWith(".jpeg"))
            return SpriteLoadUtils.LoadSprites(path);
        if (pLowerPath.EndsWith(".txt") || pLowerPath.EndsWith(".json") || pLowerPath.EndsWith(".yml"))
            return new Object[]{LoadTextAsset(path)};
        if (pLowerPath.EndsWith(".prefab"))
        {
            return Array.Empty<Object>();
            GameObject obj = PrefabLoadUtils.LoadPrefab(path);
            return obj == null ? Array.Empty<Object>() : new Object[] { obj };
        }
        throw new UnrecognizableResourceFileException();
    }

    private static Object LoadPrefab(string path)
    {
        throw new UnrecognizableResourceFileException();
    }

    private static TextAsset LoadTextAsset(string path)
    {
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        textAsset.name = Path.GetFileNameWithoutExtension(path);
        return textAsset;
    }

    internal static void LoadResourceFromMod(string pModFolder)
    {
        string path = Path.Combine(pModFolder, Paths.ModResourceFolderName);
        if (!Directory.Exists(path)) return;
        
        var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            tree.Add(file.Replace(path, ""), file);
        }
    }
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Resources), nameof(Resources.LoadAll), new Type[]
    {
        typeof(string),
        typeof(Type)
    })]
    private static UnityEngine.Object[] LoadAll_Postfix(UnityEngine.Object[] __result, string path,
        Type systemTypeInstance)
    {
        ResourceTreeNode node = tree.Find(path);
        if (node == null || node.objects.Count == 0) return __result;
        
        var list = new List<UnityEngine.Object>(__result);
        var names = new List<string>(__result.Length);
        foreach (var obj in list)
        {
            names.Add(obj.name);
        }
        
        foreach (var (key, value) in node.objects)
        {
            int idx = names.IndexOf(key);
            if (idx < 0)
            {
                list.Add(value);
            }
            else
            {
                list[idx] = value;
            }
        }

        return list.ToArray();
    }
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Resources), nameof(Resources.Load), new Type[]
    {
        typeof(string),
        typeof(Type)
    })]
    private static UnityEngine.Object Load_Postfix(UnityEngine.Object __result, string path,
        Type systemTypeInstance)
    {
        var new_result = tree.Get(path);
        if(new_result == null) return __result;
        return new_result;
    }
}