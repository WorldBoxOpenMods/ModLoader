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

    public static Sprite[] LoadSprites(string path)
    {
        TextureImporter textureImporter = loadMeta($"{path}.meta");
        if (textureImporter == null)
        {
            Sprite sprite = loadSpriteSimply(path);
            if (sprite == null)
            {
                return new Sprite[0];
            }

            sprite.name = Path.GetFileNameWithoutExtension(path);
            return new Sprite[]{sprite};
        }
        
        return loadSpriteWithMeta(path, textureImporter);
    }

    private static Sprite loadSpriteSimply(string path)
    {
        byte[] raw_data = File.ReadAllBytes(path);
        Texture2D texture = new(0, 0);
        texture.LoadImage(raw_data);
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 1);
    }

    private static Sprite[] loadSpriteWithMeta(string path, TextureImporter textureImporter)
    {
        Texture2D texture = new(0, 0);
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