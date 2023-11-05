using NeoModLoader.constants;
using NeoModLoader.services;
using UnityEngine;
using YamlDotNet.Serialization;
namespace NeoModLoader.utils;

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
    public string name;
    public Rect rect;
    public SpriteAlignment alignment;
    public Vector2 pivot;
    public Vector4 border;
}
public static class SpriteLoadUtils
{
    class MetaFile
    {
        public TextureImporter TextureImporter;
    }

    class NCMSSpritesSettings
    {
        public class SpecificSetting
        {
            public string Path = "\\";
            public float PivotX = 0.5f;
            public float PivotY = 0.0f;
            public float PixelsPerUnit = 1f;
            public float RectX = 0.0f;
            public float RectY = 0.0f;
            public Sprite loadFromPath(string path)
            {
                Texture2D texture = new(0, 0);
                texture.filterMode = FilterMode.Point;
                texture.LoadImage(File.ReadAllBytes(path));
                Sprite sprite = Sprite.Create(texture, new Rect(RectX, RectY, texture.width, texture.height), new Vector2(PivotX, PivotY), PixelsPerUnit);
                sprite.name = System.IO.Path.GetFileNameWithoutExtension(path);
                return sprite;
            }
        }
        public SpecificSetting Default;
        public List<SpecificSetting> Specific;

        public override String ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }

    private static Dictionary<string, NCMSSpritesSettings> dirNCMSSettings = new();
    private static HashSet<string> ignoreNCMSSettingsSearchPath = new();
    private static NCMSSpritesSettings.SpecificSetting defaultNCMSSetting = new();

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
                return new Sprite[]{sprite};
            }
        }
        
        return loadSpriteWithMeta(path, textureImporter);
    }

    private static NCMSSpritesSettings.SpecificSetting searchUpNCMSSetting(string path)
    {
        string dir = Path.GetDirectoryName(path);

        NCMSSpritesSettings.SpecificSetting getInternalSetting(string path, NCMSSpritesSettings settings)
        {
            if (settings.Specific == null) return settings.Default;
            foreach (var setting in settings.Specific)
            {
                if (setting.Path == Path.GetFileName(path))
                {
                    //LogService.LogInfo($"Specific NCMSSetting {setting.Path} found for {path}");
                    return setting;
                }
            }
            return dirNCMSSettings[dir].Default;
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
                        Newtonsoft.Json.JsonConvert.DeserializeObject<NCMSSpritesSettings>(File.ReadAllText(settingPath));
                    settings.Default ??= defaultNCMSSetting;
                    dirNCMSSettings.Add(dir, settings);
                    
                    //LogService.LogInfo(settings.ToString());
                    
                    return getInternalSetting(path, settings);
                }
                ignoreNCMSSettingsSearchPath.Add(dir);
            }

            if (dir == Paths.ModsPath)
            {
                return null;
            }

            dir = Path.GetDirectoryName(dir);
            if(string.IsNullOrEmpty(dir)) return null;
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
        for(int i = 0; i < sprites.Length; i++)
        {
            var sprite = textureImporter.spriteSheet.sprites[i];
            sprites[i] = Sprite.Create(texture, sprite.rect, sprite.pivot, 1, 0, SpriteMeshType.FullRect, sprite.border);
            sprites[i].name = sprite.name;
        }
        return sprites;
    }

    private static IDeserializer deserializer =
        new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
    private static TextureImporter loadMeta(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }
        MetaFile metaFile = deserializer.Deserialize<MetaFile>(File.ReadAllText(path));
        return metaFile?.TextureImporter;
    }
}