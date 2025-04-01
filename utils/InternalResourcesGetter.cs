using System.Reflection;
using NeoModLoader.constants;
using UnityEngine;

namespace NeoModLoader.utils;

internal static class InternalResourcesGetter
{
    private static Sprite mod_icon;
    private static Sprite icon_frame;
    private static Sprite icon_reload;
    private static Sprite github_icon;
    private static Sprite window_empty_frame;
    private static Sprite window_big_close;
    private static Sprite window_vert_name_plate;
    private static string commit = "";
    private static long   last_write_time;

    private static Texture2D LoadManifestTexture(string path_under_resources)
    {
        var s = Assembly.GetExecutingAssembly()
                        .GetManifestResourceStream($"NeoModLoader.resources.{path_under_resources}");
        byte[] buffer = new byte[s.Length];
        s.Read(buffer, 0, buffer.Length);

        Texture2D texture = new(0, 0);
        texture.filterMode = FilterMode.Point;
        texture.LoadImage(buffer);
        return texture;
    }

    private static byte[] LoadManifestBytes(string path_under_resources)
    {
        var s = Assembly.GetExecutingAssembly()
                        .GetManifestResourceStream($"NeoModLoader.resources.{path_under_resources}");
        byte[] buffer = new byte[s.Length];
        s.Read(buffer, 0, buffer.Length);

        return buffer;
    }

    public static long GetLastWriteTime()
    {
        if (last_write_time == 0)
        {
            var f = new FileInfo(Paths.NMLModPath);
            last_write_time = f.LastWriteTimeUtc.Ticks;
        }

        return last_write_time;
    }

    public static string GetCommit()
    {
        if (string.IsNullOrEmpty(commit))
        {
            var s = WorldBoxMod.NeoModLoaderAssembly.GetManifestResourceStream("NeoModLoader.resources.commit");

            commit = new StreamReader(s).ReadToEnd().Replace("\n", "").Replace("\r", "");

            s.Close();
        }

        return commit;
    }

    public static Sprite GetIcon()
    {
        if (mod_icon != null) return mod_icon;
        SpriteTextureLoader.addSprite("ui/icons/neomodloader", LoadManifestBytes("logo.png"));
        mod_icon = SpriteTextureLoader.getSprite("ui/icons/neomodloader");
        mod_icon.name = "NeoModLoader";
        ResourcesPatch.PatchResource("ui/icons/neomodloader", mod_icon);
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
        if (github_icon != null) return github_icon;
        SpriteTextureLoader.addSprite("ui/icons/iconGithub", LoadManifestBytes("github.png"));
        github_icon = SpriteTextureLoader.getSprite("ui/icons/iconGithub");
        github_icon.name = "iconGithub";

        return github_icon;
    }

    public static Sprite GetReloadIcon()
    {
        if (icon_reload != null) return icon_reload;

        Texture2D texture = LoadManifestTexture("reload.png");

        icon_reload = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 1,
                                    0, SpriteMeshType.Tight, new Vector4(0, 0, 0, 0));

        return icon_reload;
    }

    public static Sprite GetWindowEmptyFrame()
    {
        if (window_empty_frame != null) return window_empty_frame;

        var texture = LoadManifestTexture("window_empty_frame.png");

        window_empty_frame = Sprite.Create(texture, new Rect(0, 0, 216, 252), new Vector2(0.5f, 0.5f), 1, 1,
                                           SpriteMeshType.Tight, new Vector4(12, 12, 12, 12));
        window_empty_frame.name = "windowEmptyFrame";
        SpriteTextureLoader._cached_sprites[$"ui/special/{window_empty_frame.name}"] = window_empty_frame;

        return window_empty_frame;
    }

    public static Sprite GetWindowBigCloseSliced()
    {
        if (window_big_close != null) return window_big_close;

        var texture = LoadManifestTexture("windowBigCloseSliced.png");

        window_big_close = Sprite.Create(texture, new Rect(0, 0, 36, 35), new Vector2(0.5f, 0.5f), 1, 1,
                                         SpriteMeshType.Tight, new Vector4(8, 8, 8, 8));
        window_big_close.name = "windowBigCloseSliced";
        SpriteTextureLoader._cached_sprites[$"ui/special/{window_big_close.name}"] = window_big_close;

        return window_big_close;
    }

    public static Sprite GetWindowVertNamePlate()
    {
        if (window_vert_name_plate != null) return window_vert_name_plate;

        var texture = LoadManifestTexture("windowVertNamePlate.png");

        window_vert_name_plate = Sprite.Create(texture, new Rect(0, 0, 18, 43), new Vector2(0.5f, 0.5f), 1, 1,
                                               SpriteMeshType.Tight, new Vector4(2, 2, 2, 2));
        window_vert_name_plate.name = "windowVertNamePlate";
        SpriteTextureLoader._cached_sprites[$"ui/special/{window_vert_name_plate.name}"] = window_vert_name_plate;

        return window_vert_name_plate;
    }
}