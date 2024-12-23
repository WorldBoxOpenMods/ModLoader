using System.Diagnostics.CodeAnalysis;
using NeoModLoader.constants;
using NeoModLoader.services;
using Newtonsoft.Json;
using UnityEngine;
using YamlDotNet.Serialization;

namespace NeoModLoader.utils;
#pragma warning disable CS0649 // They are assigned by deserializer
[Serializable]
class TextureImporter
{
    public SpriteSheet spriteSheet;
}

[Serializable]
class SpriteSheet
{
    public List<SingleSpriteMetaData> sprites;
}

[Serializable]
class SingleSpriteMetaData
{
    public string          name;
    public Rect            rect;
    public SpriteAlignment alignment;
    public Vector2         pivot;
    public Vector4         border;
}
#pragma warning restore CS0649 // They are assigned by deserializer

/// <summary>
/// A utility class for loading sprites.
/// </summary>
/// <remarks>
/// All parameter path should be path in actual file system.
/// </remarks>
public static class SpriteLoadUtils
{
    private static Dictionary<string, Sprite>              singleSpriteCache            = new();
    private static Dictionary<string, NCMSSpritesSettings> dirNCMSSettings              = new();
    private static HashSet<string>                         ignoreNCMSSettingsSearchPath = new();
    private static NCMSSpritesSettings.SpecificSetting     defaultNCMSSetting           = new();

    private static IDeserializer deserializer =
        new DeserializerBuilder().IgnoreUnmatchedProperties().Build();

    /// <summary>
    /// Load a single sprite from file path
    /// </summary>
    /// <remarks>It will not check sprite setting file. Load the raw file as a sprite</remarks>
    /// <param name="path">Path to the file</param>
    /// <returns></returns>
    public static Sprite LoadSingleSprite(string path)
    {
        if (singleSpriteCache.TryGetValue(path, out Sprite s))
        {
            return s;
        }

        Sprite sprite = loadSpriteSimply(path);
        singleSpriteCache[path] = sprite;
        return sprite;
    }

    /// <summary>
    /// Load a single/sheet sprites from a path.
    /// </summary>
    /// <remarks>If there is a file named "{path}.meta" and it describes a SpriteSheet, all sprites under the sprite sheet will be loaded together. Otherwise, the return array contains only one sprite</remarks>
    /// <param name="path"></param>
    /// <returns></returns>
    public static Sprite[] LoadSprites(string path)
    {
        TextureImporter textureImporter = loadMeta($"{path}.meta");
        if (textureImporter == null)
        {
            NCMSSpritesSettings.SpecificSetting NCMSSetting = searchUpNCMSSetting(path);
            if (NCMSSetting != null)
            {
                try
                {
                    Sprite sprite = NCMSSetting.loadFromPath(path);
                    if (sprite != null)
                    {
                        return new Sprite[] { sprite };
                    }
                }
                catch (Exception e)
                {
                    LogService.LogError(
                        $"Failed to load sprite from {path} with NCMSSetting {NCMSSetting.GetType().FullName}");
                    LogService.LogError(e.ToString());
                    return Array.Empty<Sprite>();
                }
            }
            else
            {
                Sprite sprite = loadSpriteSimply(path);
                if (sprite == null)
                {
                    return Array.Empty<Sprite>();
                }

                sprite.name = Path.GetFileNameWithoutExtension(path);
                return new Sprite[] { sprite };
            }
        }

        return loadSpriteWithMeta(path, textureImporter);
    }

    private static NCMSSpritesSettings.SpecificSetting searchUpNCMSSetting(string path)
    {
        string dir = Path.GetDirectoryName(path);

        NCMSSpritesSettings.SpecificSetting getInternalSetting(string i_path, NCMSSpritesSettings settings)
        {
            if (settings.Specific == null) return settings.Default;
            foreach (var setting in settings.Specific)
            {
                if (setting == null) continue;
                if (setting.Path == Path.GetFileName(i_path))
                {
                    //LogService.LogInfo($"Specific NCMSSetting {setting.Path} found for {path}");
                    return setting;
                }
            }

            return settings.Default;
        }

        while (true)
        {
            if (!ignoreNCMSSettingsSearchPath.Contains(dir))
            {
                if (dirNCMSSettings.ContainsKey(dir))
                {
                    return getInternalSetting(path, dirNCMSSettings[dir]);
                }

                string settingPath = Path.Combine(dir, "sprites.json");
                //LogService.LogInfo(
                //    $"Searching for NCMSSetting in {dir}");
                if (File.Exists(settingPath))
                {
                    NCMSSpritesSettings settings =
                        JsonConvert.DeserializeObject<NCMSSpritesSettings>(File.ReadAllText(settingPath));
                    if (settings != null)
                    {
                        settings.Default ??= defaultNCMSSetting;
                        dirNCMSSettings.Add(dir, settings);
                        if (settings.Specific?.Contains(null) ?? false)
                            LogService.LogWarning($"Here is something wrong at {settingPath}");

                        //LogService.LogInfo(settings.ToString());

                        return getInternalSetting(path, settings);
                    }

                    LogService.LogWarning($"Wrong sprite settings file at {settingPath}");
                }

                ignoreNCMSSettingsSearchPath.Add(dir);
            }

            if (dir == Paths.ModsPath)
            {
                return defaultNCMSSetting;
            }

            dir = Path.GetDirectoryName(dir);
            if (string.IsNullOrEmpty(dir)) return defaultNCMSSetting;
        }
    }

    private static Sprite loadSpriteSimply(string path)
    {
        byte[] raw_data = File.ReadAllBytes(path);
        Texture2D texture = new(0, 0);
        texture.filterMode = FilterMode.Point;
        texture.LoadImage(raw_data);
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 1);
    }

    private static Sprite[] loadSpriteWithMeta(string path, TextureImporter textureImporter)
    {
        Texture2D texture = new(0, 0);
        texture.filterMode = FilterMode.Point;
        texture.LoadImage(File.ReadAllBytes(path));
        Sprite[] sprites = new Sprite[textureImporter.spriteSheet.sprites.Count];
        for (int i = 0; i < sprites.Length; i++)
        {
            var sprite = textureImporter.spriteSheet.sprites[i];
            sprites[i] = Sprite.Create(texture, sprite.rect, sprite.pivot, 1, 0, SpriteMeshType.FullRect,
                sprite.border);
            sprites[i].name = sprite.name;
        }

        return sprites;
    }

    private static TextureImporter loadMeta(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        MetaFile metaFile = deserializer.Deserialize<MetaFile>(File.ReadAllText(path));
        return metaFile?.TextureImporter;
    }

#pragma warning disable CS0649 // They are assigned by deserializer
    class MetaFile
    {
        public TextureImporter TextureImporter;
    }

    /// <remarks>
    ///     Prototype comes from [NCMS](https://denq04.github.io/ncms/)
    /// </remarks>
    class NCMSSpritesSettings
    {
        public SpecificSetting       Default;
        public List<SpecificSetting> Specific;
#pragma warning restore CS0649 // They are assigned by deserializer

        public override String ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <remarks>
        ///     Prototype comes from [NCMS](https://denq04.github.io/ncms/)
        /// </remarks>
        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
        public class SpecificSetting
        {
            public string Alias         = "";
            public float  BorderB       = 0.0f;
            public float  BorderL       = 0.0f;
            public float  BorderR       = 0.0f;
            public float  BorderT       = 0.0f;
            public string Path          = "\\";
            public float  PivotX        = 0.5f;
            public float  PivotY        = 0.0f;
            public float  PixelsPerUnit = 1f;
            public float  RectH         = -1;
            public float  RectW         = -1;
            public float  RectX         = 0.0f;
            public float  RectY         = 0.0f;

            public Sprite loadFromPath(string path)
            {
                Texture2D texture = new(0, 0);
                texture.filterMode = FilterMode.Point;
                texture.LoadImage(File.ReadAllBytes(path));
                Sprite sprite = Sprite.Create(texture,
                    new Rect(RectX, RectY, RectW < 0 ? texture.width : RectW,
                        RectH                    < 0 ? texture.height : RectH),
                    new Vector2(PivotX, PivotY), PixelsPerUnit, 1, SpriteMeshType.Tight,
                    new Vector4(BorderL, BorderB, BorderR, BorderT));
                sprite.name = string.IsNullOrEmpty(Alias) ? System.IO.Path.GetFileNameWithoutExtension(path) : Alias;
                return sprite;
            }
        }
    }
}