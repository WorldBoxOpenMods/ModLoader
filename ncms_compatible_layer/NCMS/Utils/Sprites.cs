using UnityEngine;

namespace NCMS.Utils;

#pragma warning disable CS1591 // No comment for NCMS compatible layer
public class Sprites
{
    /// <remarks>
    ///     From [NCMS](https://denq04.github.io/ncms/)
    /// </remarks>
    public static Sprite LoadSprite(string path, float offsetX = 0f, float offsetY = 0f)
    {
        // Maybe a NCMS mod will use exception to do something, so we do not catch it.
        /*
        if (string.IsNullOrEmpty(path))
            return (Sprite) null;
        if (!File.Exists(path))
            return (Sprite) null;
        */
        var texture2D = new Texture2D(0, 0);
        texture2D.anisoLevel = 0;
        texture2D.filterMode = FilterMode.Point;
        texture2D.LoadImage(File.ReadAllBytes(path));
        return Sprite.Create(texture2D, new Rect(0.0f, 0.0f, texture2D.width, texture2D.height),
                             new Vector2(offsetX, offsetY), 1);
    }
}