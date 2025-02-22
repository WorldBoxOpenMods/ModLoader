using System.Globalization;
using FMOD;
using FMODUnity;
using HarmonyLib;
using NeoModLoader.api.exceptions;
using NeoModLoader.services;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.U2D;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace NeoModLoader.utils;

/// <summary>
///     This class is used to patch resources.
/// </summary>
public struct WavContainer
{
    public string Path;
    public bool _3D;
    public float Volume;

    public WavContainer(string Path, bool _3D, float Volume)
    {
        this.Path = Path;
        this._3D = _3D;
        this.Volume = Volume;
    }
}

public static class ResourcesPatch
{
    private static ResourceTree tree;
    public static Dictionary<string, WavContainer> AudioWavLibrary = new Dictionary<string, WavContainer>();
    public static FMOD.System fmodSystem;
    public static ChannelGroup masterChannelGroup;

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

    static void InitializeFMODSystem()
    {
        if (RuntimeManager.StudioSystem.getCoreSystem(out fmodSystem) != RESULT.OK)
        {
            LogService.LogError("Failed to initialize FMOD Core System!");
            return;
        }

        if (fmodSystem.getMasterChannelGroup(out masterChannelGroup) != RESULT.OK)
        {
            LogService.LogError("Failed to retrieve master channel group!");
        }
    }

    internal static void Initialize()
    {
        InitializeFMODSystem();
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

    internal static void PatchSomeResources()
    {
        Sprite[] items = Resources.LoadAll<Sprite>("actors/races/items");
        foreach (Sprite item in items)
        {
            if (ActorAnimationLoader.dictItems.ContainsKey(item.name)) continue;
            ActorAnimationLoader.dictItems[item.name] = item;
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
        if (pLowerPath.EndsWith(".wav"))
        {
            LoadWavFile(path);
        }

        return new Object[]
        {
            LoadTextAsset(path)
        };
    }

    //doesnt return anything, simply adds the wav to the wav library that contains all the custom sounds
    private static void LoadWavFile(string path)
    {
        string Name = Path.GetFileNameWithoutExtension(path);
        WavContainer container = default;
        try
        {
            container = JsonConvert.DeserializeObject<WavContainer>(
                File.ReadAllText(Path.GetDirectoryName(path) + "/" + Name + ".json"));
            container.Path = path;
        }
        catch (Exception e)
        {
            container = new WavContainer(path, true, 50f);
        }

        AudioWavLibrary.Add(Name, container);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MusicBox), nameof(MusicBox.playSound), typeof(string), typeof(float), typeof(float),
        typeof(bool), typeof(bool))]
    public static bool LoadCustomSound(string pSoundPath, float pX, float pY, bool pGameViewOnly)
    {
        if (!MusicBox.sounds_on)
        {
            return false;
        }

        if (pGameViewOnly && World.world.qualityChanger.lowRes)
        {
            return false;
        }

        if (!AudioWavLibrary.ContainsKey(pSoundPath)) return true;

        WavContainer WAV = AudioWavLibrary[pSoundPath];
        if (fmodSystem.createSound(WAV.Path, WAV._3D ? MODE._3D : MODE._2D, out var sound) != RESULT.OK)
        {
            Debug.Log($"Unable to play sound {pSoundPath}!");
        }

        var listenerPosition = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y,
            Camera.main.orthographicSize);
        Vector3 sourcePosition = new(pX, pY, 0);
        fmodSystem.playSound(sound, masterChannelGroup, false, out Channel channel);
        if (WAV._3D)
        {
            VECTOR vel = new VECTOR() { x = listenerPosition.x - pX, y = listenerPosition.y - pY, z = 0 };
            VECTOR pos = new VECTOR() { x = pX, y = pY, z = 0 };
            channel.set3DAttributes(ref pos, ref vel);
        }

        float normalizedDistance = Mathf.Clamp01(Vector3.Distance(sourcePosition, listenerPosition) / WAV.Volume);
        channel.setVolume(1f - normalizedDistance);
        return false;
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
        public void AddFromFile(string path, string absPath)
        {
            string lower_path = path.ToLower();
            if (lower_path.EndsWith(".meta") || lower_path.EndsWith("sprites.json")) return;

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