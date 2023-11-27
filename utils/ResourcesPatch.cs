using System.Globalization;
using HarmonyLib;
using NeoModLoader.api.exceptions;
using NeoModLoader.constants;
using NeoModLoader.General;
using NeoModLoader.services;
using UnityEngine;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

namespace NeoModLoader.utils;

internal static class ResourcesPatch
{
    class ResourceTree
    {
        private ResourceTreeNode root = new(null);
        internal Dictionary<string, UnityEngine.Object> direct_objects = new();
        /// <summary>
        /// Find a ResourceTreeNode by path.
        /// </summary>
        /// <param name="path">The path of node path</param>
        /// <param name="createNodeAlong">Whether create node along the path if node does not exist</param>
        /// <param name="visitLast">Whether check the last node</param>
        /// <returns></returns>
        public ResourceTreeNode Find(string path, bool createNodeAlong = false, bool visitLast = true)
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
            for (int i = 0; i < parts.Length - (visitLast ? 0 : 1); i++)
            {
                var part = parts[i];
                if (!node.children.ContainsKey(part))
                {
                    if (!createNodeAlong)
                        return null;
                    node.children[part] = new ResourceTreeNode(node);
                }

                node = node.children[part];
            }

            return node;
        }

        public UnityEngine.Object Get(string path)
        {
            return direct_objects.TryGetValue(path.ToLower(), out Object o) ? o : null;
        }

        public void Add(string path, Object obj)
        {
            string lower_path = path.ToLower();
            direct_objects[lower_path] = obj;
            var node = Find(path, true, false);
            node.objects[Path.GetFileNameWithoutExtension(lower_path)] = obj;

        }
        /// <summary>
        /// Load resources under absPath, and patch them to the tree under the folder of path.
        /// </summary>
        /// <param name="path">Path to resource in tree</param>
        /// <param name="absPath">Path to resource in actual filesystem</param>
        public void AddFromFile(string path, string absPath)
        {
            string lower_path = path.ToLower();
            if (lower_path.EndsWith(".meta") || lower_path.EndsWith("sprites.json")) return;
            if (lower_path.EndsWith(".ab"))
            {
                patchAssetBundleToTree(path, absPath);
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
                        direct_objects[Path.Combine(parent_path, obj.name).Replace('\\','/').ToLower()] = obj;
                    }
                }
            }
            catch (UnrecognizableResourceFileException)
            {
                LogService.LogWarning($"Cannot recognize resource file {path}");
                return;
            }
            if(objs.Length == 0) return;

            var node = Find(path, true, false);

            foreach (var obj in objs)
            {
                node.objects[obj.name.ToLower()] = obj;
            }
        }

        private void patchAssetBundleToTree(string path, string absPath)
        {
            WrappedAssetBundle ab = AssetBundleUtils.LoadFromFile(absPath);
            if (ab == null)
            {
                LogService.LogError($"Cannot load asset bundle {path}");
                LogService.LogStackTraceAsError();
                return;
            }
            var node = Find(path, true, false);
            node.assetBundles.Add(ab);
        }
    }

    class ResourceTreeNode
    {
        public readonly Dictionary<string, ResourceTreeNode> children = new();
        public readonly Dictionary<string, UnityEngine.Object> objects = new();
        public readonly List<WrappedAssetBundle> assetBundles = new();
        public readonly ResourceTreeNode parent;

        public ResourceTreeNode(ResourceTreeNode parent)
        {
            this.parent = parent;
        }

        public List<Object> GetAllObjects(Type systemTypeInstance)
        {
            var result = new List<Object>(objects.Count);
            
            Queue<ResourceTreeNode> queue = new Queue<ResourceTreeNode>(children.Count);
            queue.Enqueue(this);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                foreach (var obj in node.objects.Values)
                {
                    if (systemTypeInstance.IsInstanceOfType(obj))
                    {
                        result.Add(obj);
                    }
                }
                foreach(WrappedAssetBundle ab in node.assetBundles)
                {
                    result.AddRange(ab.GetAllObjects(systemTypeInstance));
                }
                foreach(var child in node.children.Values)
                {
                    queue.Enqueue(child);
                }
            }


            return result;
        }
    }

    private static ResourceTree tree;

    public static Dictionary<string, UnityEngine.Object> GetAllPatchedResources()
    {
        return tree.direct_objects;
    }
    internal static void Initialize()
    {
        tree = new ResourceTree();
        SpriteAtlas atlas = Resources.FindObjectsOfTypeAll<SpriteAtlas>().FirstOrDefault(x => x.name == "SpriteAtlasUI");

        Sprite[] sprites = new Sprite[atlas.spriteCount];
        atlas.GetSprites(sprites);
        foreach (var sprite in sprites)
        {
            tree.Add($"ui/special/{sprite.name.Replace("(Clone)", "")}", sprite);
        }
    }
    /// <summary>
    /// Load a resource file from path, and named by pLowerPath.
    /// </summary>
    /// <remarks>
    /// It can recognize jpg, png, jpeg by postfix now.
    /// <para>All others will be loaded as text</para>
    /// </remarks>
    /// <param name="path">the path to the resource file to load</param>
    /// <param name="pLowerPath">the lower of path with <see cref="CultureInfo.CurrentCulture"/></param>
    /// <returns>The Objects loaded, if single Object, an array with single one; if no Objects, an empty array</returns>
    /// It can recognize jpg, png, jpeg by postfix now
    public static UnityEngine.Object[] LoadResourceFile(ref string path, ref string pLowerPath)
    {
        if (pLowerPath.EndsWith(".png") || pLowerPath.EndsWith(".jpg") || pLowerPath.EndsWith(".jpeg"))
            return SpriteLoadUtils.LoadSprites(path);
        return new Object[]{LoadTextAsset(path)};
    }

    private static TextAsset LoadTextAsset(string path)
    {
        TextAsset textAsset = new TextAsset(File.ReadAllText(path));
        textAsset.name = Path.GetFileNameWithoutExtension(path);
        return textAsset;
    }

    internal static void LoadResourceFromFolder(string pFolder)
    {
        if (!Directory.Exists(pFolder)) return;

        var files = SystemUtils.SearchFileRecursive(pFolder, filename => !filename.StartsWith("."),
            dirname => !dirname.StartsWith("."));
        foreach (var file in files)
        {
            tree.AddFromFile(file.Replace(pFolder, "").Replace('\\', '/').Substring(1), file);
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
        if (node == null) return __result;
        
        var append_list = node.GetAllObjects(systemTypeInstance);
        
        if(append_list.Count == 0) return __result;
        
        var list = new List<UnityEngine.Object>(__result);
        
        HashSet<string> names = new HashSet<string>(append_list.Select(x => x.name));
        list.RemoveAll(x => names.Contains(x.name));
        
        list.AddRange(append_list);
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
        if (new_result != null && systemTypeInstance.IsInstanceOfType(new_result))
            return new_result;
        var node = tree.Find(path, false, false);
        if (node == null)
            return __result;
        foreach (var ab in node.assetBundles)
        {
            new_result = ab.GetObject(Path.GetFileName(path), systemTypeInstance);
            if (new_result != null)
                return new_result;
        }
        return __result;
    }
}