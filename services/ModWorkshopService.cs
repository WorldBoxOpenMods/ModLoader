using System.Linq;
using System.Reflection;
using NeoModLoader.api;
using NeoModLoader.api.attributes;
using NeoModLoader.constants;
using NeoModLoader.General;
using NeoModLoader.utils;
using Newtonsoft.Json;
using RSG;
using UnityEngine;

namespace NeoModLoader.services;

[Experimental]
internal static class ModWorkshopService
{
    internal static Promise steamWorkshopPromise;
    private static IPlatformSpecificModWorkshopService workshopServiceBackend;

    public static void Init()
    {
        steamWorkshopPromise = RF.GetStaticField<Promise, SteamSDK>("steamInitialized");
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            workshopServiceBackend = new ModWorkshopServiceWindows();
        }
        else
        {
            workshopServiceBackend = new ModWorkshopServiceUnix();
        }
    }

    private static void UploadModLoader(string changelog)
    {
        workshopServiceBackend.UploadModLoader(changelog);
    }

    /// <summary>
    /// Try to Upload a mod to Steam Workshop
    /// </summary>
    public static Promise UploadMod(IMod mod, string changelog, bool verified = false)
    {
        ModDeclare mod_decl = mod.GetDeclaration();
        string name = mod_decl.Name;
        string description = BuildModDescription(mod_decl);
        string workshopPath = Path.Combine(SaveManager.generateMainPath("workshop_upload_mod") + mod_decl.UID);

        PrepareWorkshopDirectory(workshopPath);
        CopyModFiles(mod_decl.FolderPath, workshopPath);

        string previewImagePath = PreparePreviewImage(mod_decl, workshopPath);

        // This works for BepInEx mods
        File.WriteAllText(Path.Combine(workshopPath, "mod.json"), JsonConvert.SerializeObject(mod_decl, Formatting.Indented));

        return workshopServiceBackend.UploadMod(name, description, previewImagePath, workshopPath, changelog, verified, mod_decl.GetAIAttributionDisplay(), mod_decl.Tags);
    }

    public static Promise TryEditMod(ulong fileID, IMod mod, string changelog)
    {
        ModDeclare mod_decl = mod.GetDeclaration();
        string workshopPath = Path.Combine(SaveManager.generateMainPath("workshop_upload_mod") + mod_decl.UID);

        PrepareWorkshopDirectory(workshopPath);
        CopyModFiles(mod_decl.FolderPath, workshopPath);

        string previewImagePath = PreparePreviewImage(mod_decl, workshopPath);

        File.WriteAllText(Path.Combine(workshopPath, "mod.json"), JsonConvert.SerializeObject(mod_decl, Formatting.Indented));

        return workshopServiceBackend.EditMod(fileID, previewImagePath, workshopPath, changelog, mod_decl.GetAIAttributionDisplay(), mod_decl.Tags);
    }

    private static string BuildModDescription(ModDeclare mod_decl)
    {
        string name = mod_decl.Name;
        string aiDisclaimer = BuildAIDisclaimer(mod_decl);

        return $"{name} Uploaded by NeoModLoader\n" +
               $"{name} 由NeoModLoader上传\n\n" +
               $"{mod_decl.Description}\n\n" +
               $"{aiDisclaimer}\n\n" +
               $"ModLoader: {CoreConstants.RepoURL}\n\n" +
               $"模组加载器: {CoreConstants.RepoURL}";
    }

    private static string BuildAIDisclaimer(ModDeclare mod_decl)
    {
        if (mod_decl.AIAttribution == "ai_made" || mod_decl.AIAttribution == "ai_assisted")
        {
            var items = mod_decl.GetAIChecklistItems();
            string itemsFormatted = items.Count > 0
                ? string.Join("\n", items.Select(x => $"[b]•[/b] {x}"))
                : "[b]•[/b] General Content";

            return $"[hr][/hr]" +
                   $"[b]AI Content Disclosure ({mod_decl.GetAIAttributionDisplay()}):[/b]\n" +
                   $"This mod utilizes generative AI for:\n" +
                   $"{itemsFormatted}\n" +
                   $"(in compliance with Steam Workshop guidelines)\n" +
                   $"[hr][/hr]";
        }

        return "[hr][/hr][b]Authorship:[/b] Handcrafted / Human Authored (No generative AI content)\n[hr][/hr]";
    }

    private static void PrepareWorkshopDirectory(string workshopPath)
    {
        if (Directory.Exists(workshopPath))
        {
            Directory.Delete(workshopPath, true);
        }

        string basePath = SaveManager.generateMainPath("workshop_upload_mod");
        if (!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
        }

        Directory.CreateDirectory(workshopPath);
    }

    private static void CopyModFiles(string sourceFolder, string targetFolder)
    {
        List<string> files_to_upload = SystemUtils.SearchFileRecursive(sourceFolder,
            filename => !filename.StartsWith("."),
            dirname => !dirname.StartsWith(".") && !Paths.IgnoreSearchDirectories.Contains(dirname));

        foreach (string file_full_path in files_to_upload)
        {
            string path = Path.Combine(targetFolder,
                file_full_path.Replace(sourceFolder, "").Replace("\\", "/").Substring(1));

            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.Copy(file_full_path, path);
        }
    }

    private static string PreparePreviewImage(ModDeclare mod_decl, string workshopPath)
    {
        string previewImagePath;
        if (string.IsNullOrEmpty(mod_decl.IconPath))
        {
            using Stream icon_stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("NeoModLoader.resources.logo.png");
            using FileStream icon_file = File.Create(Path.Combine(workshopPath, "preview.png"));
            icon_stream.Seek(0, SeekOrigin.Begin);
            icon_stream.CopyTo(icon_file);
            previewImagePath = Path.Combine(workshopPath, "preview.png");
        }
        else
        {
            previewImagePath = Path.Combine(workshopPath, mod_decl.IconPath);
        }

        return ApplyAIBadgeToThumbnail(previewImagePath, mod_decl.AIAttribution, mod_decl.AIBadgeCorner, mod_decl.AIBadgeStyle, workshopPath);
    }

    private static string ApplyAIBadgeToThumbnail(string originalPreviewPath, string attribution, string corner, string style, string workshopPath)
    {
        if (!CoreConstants.EnableBadges || string.IsNullOrEmpty(originalPreviewPath) || !File.Exists(originalPreviewPath))
        {
            return originalPreviewPath;
        }

        bool isIconOnly = string.Equals(style, "icon_only", StringComparison.OrdinalIgnoreCase);
        bool isWatermark = string.Equals(style, "watermark", StringComparison.OrdinalIgnoreCase);
        string resourceName = GetBadgeResourceName(attribution, isIconOnly);

        try
        {
            byte[] badgeBytes = LoadEmbeddedBadgeBytes(resourceName);
            if (badgeBytes == null || badgeBytes.Length == 0) return originalPreviewPath;

            Texture2D baseTex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!baseTex.LoadImage(File.ReadAllBytes(originalPreviewPath))) return originalPreviewPath;

            Texture2D badgeTex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!badgeTex.LoadImage(badgeBytes)) return originalPreviewPath;

            Texture2D resultTex = CreateReadableTexture(baseTex);

            int badgeWidth = isIconOnly
                ? Mathf.Clamp(resultTex.width / 6, 28, 64)
                : Mathf.Clamp(resultTex.width / 3, 60, 240);
            int badgeHeight = Mathf.RoundToInt((float)badgeWidth / badgeTex.width * badgeTex.height);
            Texture2D scaledBadge = scaleTexture(badgeTex, badgeWidth, badgeHeight);

            var (startX, startY) = CalculateBadgePosition(corner, resultTex.width, resultTex.height, scaledBadge.width, scaledBadge.height);
            BlendBadgePixels(resultTex, scaledBadge, startX, startY, isWatermark);

            string outPath = Path.Combine(workshopPath, "preview_tagged.png");
            File.WriteAllBytes(outPath, resultTex.EncodeToPNG());
            return outPath;
        }
        catch (Exception e)
        {
            LogService.LogWarning($"Failed to composite AI badge on thumbnail: {e.Message}");
            return originalPreviewPath;
        }
    }

    private static string GetBadgeResourceName(string attribution, bool isIconOnly)
    {
        string prefix = isIconOnly ? "i_" : "b_";
        string suffix = attribution switch
        {
            "ai_made" => "made.png",
            "ai_assisted" => "ast.png",
            _ => "na.png"
        };
        return $"NeoModLoader.resources.{prefix}{suffix}";
    }

    private static byte[] LoadEmbeddedBadgeBytes(string resourceName)
    {
        using Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        if (s == null) return null;
        byte[] bytes = new byte[s.Length];
        s.Read(bytes, 0, bytes.Length);
        return bytes;
    }

    private static Texture2D CreateReadableTexture(Texture2D baseTex)
    {
        RenderTexture rt = RenderTexture.GetTemporary(baseTex.width, baseTex.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(baseTex, rt);
        RenderTexture previousRT = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D resultTex = new Texture2D(baseTex.width, baseTex.height, TextureFormat.RGBA32, false);
        resultTex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        resultTex.Apply();

        RenderTexture.active = previousRT;
        RenderTexture.ReleaseTemporary(rt);
        return resultTex;
    }

    private static (int startX, int startY) CalculateBadgePosition(string corner, int targetWidth, int targetHeight, int badgeWidth, int badgeHeight)
    {
        return corner?.ToLower() switch
        {
            "top_left" => (5, Mathf.Max(0, targetHeight - badgeHeight - 5)),
            "top_right" => (Mathf.Max(0, targetWidth - badgeWidth - 5), Mathf.Max(0, targetHeight - badgeHeight - 5)),
            "bottom_left" => (5, 5),
            _ => (Mathf.Max(0, targetWidth - badgeWidth - 5), 5)
        };
    }

    private static void BlendBadgePixels(Texture2D resultTex, Texture2D scaledBadge, int startX, int startY, bool isWatermark)
    {
        for (int x = 0; x < scaledBadge.width; x++)
        {
            for (int y = 0; y < scaledBadge.height; y++)
            {
                int targetX = startX + x;
                int targetY = startY + y;
                if (targetX >= resultTex.width || targetY >= resultTex.height) continue;

                Color badgeColor = scaledBadge.GetPixel(x, y);
                if (badgeColor.a <= 0.01f) continue;

                Color baseColor = resultTex.GetPixel(targetX, targetY);
                float effectiveAlpha = isWatermark ? (badgeColor.a * 0.35f) : badgeColor.a;
                Color blendedColor = Color.Lerp(baseColor, badgeColor, effectiveAlpha);
                resultTex.SetPixel(targetX, targetY, blendedColor);
            }
        }
        resultTex.Apply();
    }

    private static Texture2D scaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(source, rt);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
        result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        result.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);
        return result;
    }

    public static void FindSubscribedMods()
    {
        workshopServiceBackend.FindSubscribedMods();
    }

    public static ModDeclare GetNextModFromWorkshopItem()
    {
        return workshopServiceBackend.GetNextModFromWorkshopItem();
    }
}