using System.Reflection;
using UnityEngine;

namespace NeoModLoader.utils;

internal static class InternalResourcesGetter
{
    private static Sprite mod_icon;
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
}