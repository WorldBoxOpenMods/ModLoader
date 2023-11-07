using System.Reflection;
using UnityEngine;

namespace NeoModLoader.utils;

internal static class InternalResourcesGetter
{
    private static Sprite mod_icon;
    private static Sprite icon_frame;
    private static Sprite github_icon;

    private static Texture2D LoadManifestTexture(string path_under_resources)
    {
        var s = Assembly.GetExecutingAssembly().GetManifestResourceStream($"NeoModLoader.resources.{path_under_resources}");
        byte[] buffer = new byte[s.Length];
        s.Read(buffer, 0, buffer.Length);
        
        Texture2D texture = new(0, 0);
        texture.filterMode = FilterMode.Point;
        texture.LoadImage(buffer);
        return texture;
    }
    private static byte[] LoadManifestBytes(string path_under_resources)
    {
        var s = Assembly.GetExecutingAssembly().GetManifestResourceStream($"NeoModLoader.resources.{path_under_resources}");
        byte[] buffer = new byte[s.Length];
        s.Read(buffer, 0, buffer.Length);
        
        return buffer;
    }
    public static Sprite GetIcon()
    {
        if (mod_icon != null) return mod_icon;
        SpriteTextureLoader.addSprite("ui/icons/neomodloader", LoadManifestBytes("logo.png"));
        mod_icon = SpriteTextureLoader.getSprite("ui/icons/neomodloader");
        mod_icon.name = "NeoModLoader";
        return mod_icon;
    }

    public static Sprite GetIconFrame()
    {
        if (icon_frame != null) return icon_frame;

        Texture2D texture = LoadManifestTexture("square_frame_only.png");

        icon_frame = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 1,
            0, SpriteMeshType.Tight, new Vector4(7, 7, 7, 7));
        return icon_frame;
    }

    public static Sprite GetGitHubIcon()
    {
        if(github_icon != null) return github_icon;
        SpriteTextureLoader.addSprite("ui/icons/iconGithub", LoadManifestBytes("github.png"));
        github_icon = SpriteTextureLoader.getSprite("ui/icons/iconGithub");
        github_icon.name = "iconGithub";
        
        return github_icon;
    }
}