using System.Globalization;
using HarmonyLib;
using NeoModLoader.api.exceptions;
using NeoModLoader.services;
using NeoModLoader.utils.Builders;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

namespace NeoModLoader.utils;

/// <summary>
///     This class is used to patch resources.
/// </summary>
public static class ResourcesPatch
{
    private static ResourceTree tree;

    /// <summary>
    ///     Get all patched resources.
    /// </summary>
    /// <remarks>
    ///     Not suggested to use this method.
    /// </remarks>
    /// <returns></returns>
    public static Dictionary<string, Object> GetAllPatchedResources()
    {
        return tree.direct_objects;
    }

    /// <summary>
    ///     Patch a resource to the tree at runtime.
    /// </summary>
    /// <param name="pPath"></param>
    /// <param name="pObject"></param>
    public static void PatchResource(string pPath, Object pObject)
    {
        tree.Add(pPath, pObject);
    }

    internal static void Initialize()
    {
        CustomAudioManager.Initialize();
        tree = new ResourceTree();
        SpriteAtlas atlas = Resources.FindObjectsOfTypeAll<SpriteAtlas>()
            .FirstOrDefault(x => x.name == "SpriteAtlasUI");

        Sprite[] sprites = new Sprite[atlas.spriteCount];
        atlas.GetSprites(sprites);
        foreach (var sprite in sprites)
        {
            sprite.name = sprite.name.Replace("(Clone)", "");
            tree.Add($"ui/special/{sprite.name}", sprite);
        }

        foreach (var method in typeof(InternalResourcesGetter).GetMethods())
        {
            if (method.ReturnType != typeof(Sprite)) continue;
            if (method.GetParameters().Length != 0) continue;
            method.Invoke(null, null);
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
    public static Object[] LoadResourceFile(ref string path, ref string pLowerPath)
    {
        if (pLowerPath.EndsWith(".png") || pLowerPath.EndsWith(".jpg") || pLowerPath.EndsWith(".jpeg"))
            return SpriteLoadUtils.LoadSprites(path);
        return new Object[]
        {
            LoadTextAsset(path)
        };
    }
    static Builder LoadAsset(string Path, string Extention) => Extention switch
    {
        //"actorasset" => new ActorAssetBuilder(Path, false),
        ".actortraitasset" => new ActorTraitBuilder(Path, false),
        ".subspeciestraitasset" => new SubspeciesTraitBuilder(Path, false),
        //".itemasset" => new ItemBuilder(Path, false), // had to comment out because whatever generics system melvin set up for this broke and I don't have time to look into it
        //".itemmodifierasset" => new ItemModifierBuilder(Path, false), // same issue as above
        ".clantraitasset" => new ClanTraitBuilder(Path, false),
        ".culturetraitasset" => new CultureTraitBuilder(Path, false),
        ".actortraitgroupasset" => new GroupAssetBuilder<ActorTraitGroupAsset>(Path, false),
        ".achievementgroupasset" => new GroupAssetBuilder<AchievementGroupAsset>(Path, false),
        ".clantraitgroupasset" => new GroupAssetBuilder<ClanTraitGroupAsset>(Path, false),
        ".culturetraitgroupasset" => new GroupAssetBuilder<CultureTraitGroupAsset>(Path, false),
        ".itemgroupasset" => new GroupAssetBuilder<ItemGroupAsset>(Path, false),
        ".kingdomtraitgroupasset" => new GroupAssetBuilder<KingdomTraitGroupAsset>(Path, false),
        ".languagetraitgroupasset" => new GroupAssetBuilder<LanguageTraitGroupAsset>(Path, false),
        ".plotcategoryasset" => new GroupAssetBuilder<PlotCategoryAsset>(Path, false),
        ".religiontraitgroupasset" => new GroupAssetBuilder<ReligionTraitGroupAsset>(Path, false),
        ".subspeciestraitgroupasset" => new GroupAssetBuilder<SubspeciesTraitGroupAsset>(Path, false),
        ".worldlawgroupasset" => new GroupAssetBuilder<WorldLawGroupAsset>(Path, false),
        _ => throw new NotSupportedException($"the asset {Extention} has not been supported yet!"),
    };
    /// <summary>
    /// doesnt return anything, simply adds the wav to the wav library that contains all the custom sounds
    /// </summary>
    private static void LoadWavFile(string path)
    {
        string Name = Path.GetFileNameWithoutExtension(path);
        if (CustomAudioManager.AudioWavLibrary.ContainsKey(Name))
        {
            LogService.LogError($"The Sound file {Name} has already been loaded!");
            return;
        }
        WavContainer container;
        try
        {
            container = JsonConvert.DeserializeObject<WavContainer>(
                File.ReadAllText(Path.GetDirectoryName(path) + "/" + Name + ".json"));
            container.Path = path;
        }
        catch (Exception)
        {
            container = new WavContainer(path, SoundMode.Stereo3D, 50f);
        }
        CustomAudioManager.AudioWavLibrary.Add(Name, container);
    }

    private static TextAsset LoadTextAsset(string path)
    {
        TextAsset textAsset = new TextAsset(File.ReadAllText(path));
        textAsset.name = Path.GetFileNameWithoutExtension(path);
        return textAsset;
    }

    internal static void LoadResourceFromFolder(string pFolder, out List<Builder> Builders)
    {
        Builders = null;
        if (!Directory.Exists(pFolder)) return;
        var files = SystemUtils.SearchFileRecursive(pFolder, filename => !filename.StartsWith("."),
            dirname => !dirname.StartsWith("."));
        foreach (var file in files)
        {
            tree.AddFromFile(file.Replace(pFolder, "").Replace('\\', '/').Substring(1), file, out Builder builder);
            if(builder != null)
            {
                Builders ??= new List<Builder>();
                Builders.Add(builder);
            }
        }
    }

    internal static void LoadAssetBundlesFromFolder(string pFolder)
    {
        if (!Directory.Exists(pFolder)) return;

        string platform_subfolder_name = Application.platform switch
        {
            RuntimePlatform.WindowsPlayer => "win",
            RuntimePlatform.WindowsEditor => "win",
            RuntimePlatform.OSXPlayer => "osx",
            RuntimePlatform.OSXEditor => "osx",
            RuntimePlatform.LinuxPlayer => "linux",
            RuntimePlatform.LinuxEditor => "linux",
            _ => "win"
        };
        string platform_folder = Path.Combine(pFolder, platform_subfolder_name);
        if (!Directory.Exists(platform_folder)) return;

        AssetBundleUtils.LoadFromFolder(platform_folder);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Resources), nameof(Resources.LoadAll), new Type[]
    {
        typeof(string), typeof(Type)
    })]
    private static void LoadAll_Prefix(ref string path)
    {
        if (!path.Contains("..")) return;
        string[] parts = path.Split('/');
        List<string> new_parts = new List<string>(parts.Length);
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i] == ".." && new_parts.Count > 0)
            {
                new_parts.RemoveAt(new_parts.Count - 1);
            }
            else
            {
                new_parts.Add(parts[i]);
            }
        }

        path = string.Join("/", new_parts);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Resources), nameof(Resources.LoadAll), new Type[]
    {
        typeof(string), typeof(Type)
    })]
    private static Object[] LoadAll_Postfix(Object[] __result, string path,
        Type systemTypeInstance)
    {
        if (tree == null) return __result;
        ResourceTreeNode node = tree.Find(path);
        if (node == null) return __result;
        List<Object> append_list = node.GetAllObjects(systemTypeInstance);

        if (append_list.Count == 0) return __result;

        var list = new List<Object>(__result);

        HashSet<string> names = new HashSet<string>(append_list.Select(x => x.name));
        list.RemoveAll(x => names.Contains(x.name));

        list.AddRange(append_list);
        return list.ToArray();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Resources), nameof(Resources.Load), new Type[]
    {
        typeof(string), typeof(Type)
    })]
    private static void Load_Prefix(ref string path)
    {
        if (!path.Contains("..")) return;
        string[] parts = path.Split('/');
        List<string> new_parts = new List<string>(parts.Length);
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i] == ".." && new_parts.Count > 0)
            {
                new_parts.RemoveAt(new_parts.Count - 1);
            }
            else
            {
                new_parts.Add(parts[i]);
            }
        }

        path = string.Join("/", new_parts);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Resources), nameof(Resources.Load), new Type[]
    {
        typeof(string), typeof(Type)
    })]
    private static Object Load_Postfix(Object __result, string path,
        Type systemTypeInstance)
    {
        if (tree == null) return __result;
        var new_result = tree.Get(path);
        if (new_result != null && systemTypeInstance.IsInstanceOfType(new_result))
            return new_result;

        return __result;
    }

    class ResourceTree
    {
        internal Dictionary<string, Object> direct_objects = new();
        private ResourceTreeNode root = new(null);

        public ResourceTree()
        {
            root.parent = root;
        }

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
                if (part == "..")
                {
                    node = node.parent;
                    continue;
                }

                if (part == ".") continue;

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

        public Object Get(string path)
        {
            if (direct_objects.TryGetValue(path.ToLower(), out Object o))
            {
                return o;
            }

            var node = Find(path, true, false);

            if (node == null) return null;
            if (node.objects.TryGetValue(Path.GetFileNameWithoutExtension(path.ToLower()), out o))
            {
                direct_objects[path] = o;
                return o;
            }

            return null;
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
        /// <param name="Builder">if your folder contains asset files (.actorasset, .itemasset) pass a master builder to load these assets, then make it buildall</param>
        public void AddFromFile(string path, string absPath, out Builder Builder)
        {
            Builder = null;
            string lower_path = path.ToLower();
            if (lower_path.EndsWith(".meta") || lower_path.EndsWith("sprites.json")) return;
            if (lower_path.EndsWith(".wav"))
            {
                LoadWavFile(absPath);
                return;
            }
            if (lower_path.EndsWith("asset"))
            {
                Builder = LoadAsset(absPath, Path.GetExtension(lower_path));
                return;
            }
            string parent_path = Path.GetDirectoryName(lower_path);
            Object[] objs;
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
                        direct_objects[Path.Combine(parent_path, obj.name).Replace('\\', '/').ToLower()] = obj;
                    }
                }
            }
            catch (UnrecognizableResourceFileException)
            {
                LogService.LogWarning($"Cannot recognize resource file {path}");
                return;
            }

            if (objs.Length == 0) return;

            var node = Find(path, true, false);

            foreach (var obj in objs)
            {
                node.objects[obj.name.ToLower()] = obj;
            }
        }
    }

    class ResourceTreeNode
    {
        public readonly Dictionary<string, ResourceTreeNode> children = new();
        public readonly Dictionary<string, Object> objects = new();

        public ResourceTreeNode(ResourceTreeNode parent)
        {
            this.parent = parent;
        }

        public ResourceTreeNode parent { get; internal set; }

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

                foreach (var child in node.children.Values)
                {
                    queue.Enqueue(child);
                }
            }


            return result;
        }
    }
}