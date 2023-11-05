using System.Reflection;
using UnityEngine;

namespace NeoModLoader.utils;

internal static class InternalResourcesGetter
{
    private static Sprite mod_icon;
    private static Sprite icon_frame;
    public static Sprite GetIcon()
    {
        if (mod_icon != null) return mod_icon;
        var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("NeoModLoader.resources.logo.png");
        byte[] buffer = new byte[s.Length];
        s.Read(buffer, 0, buffer.Length);
        SpriteTextureLoader.addSprite("ui/icons/neomodloader", buffer);
        mod_icon = SpriteTextureLoader.getSprite("ui/icons/neomodloader");
        return mod_icon;
    }

    public static Sprite GetIconFrame()
    {
        if (icon_frame != null) return icon_frame;
        var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("NeoModLoader.resources.square_frame_only.png");
        byte[] buffer = new byte[s.Length];
        s.Read(buffer, 0, buffer.Length);

        Texture2D texture = new(0, 0);
        texture.filterMode = FilterMode.Point;
        texture.LoadImage(buffer);

        icon_frame = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 1,
            0, SpriteMeshType.Tight, new Vector4(7, 7, 7, 7));
        return icon_frame;
    }
}