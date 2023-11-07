using UnityEngine;

namespace NCMS.Utils;

public class Sprites
{
    public static Sprite LoadSprite(string path, float offsetX = 0f, float offsetY = 0f)
    {
        byte[] array = File.ReadAllBytes(path);
        Texture2D texture2D = new Texture2D(1, 1);
        texture2D.anisoLevel = 0;
        texture2D.LoadImage(array);
        texture2D.filterMode = FilterMode.Point;
        texture2D.name = Path.GetFileNameWithoutExtension(path);
        Sprite sprite = Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), new Vector2(offsetX, offsetY), 1f);
        sprite.texture.GetRawTextureData();
        return sprite;
    }
}