using System.Reflection;
using HarmonyLib;
using NeoModLoader.api;
using NeoModLoader.api.attributes;
using NeoModLoader.constants;
using NeoModLoader.General;
using NeoModLoader.utils;
using RSG;
using UnityEngine;

namespace NeoModLoader.services;

[Experimental]
internal static class ModWorkshopService
{
    internal static Promise steamWorkshopPromise;

    public static void Init()
    {
        steamWorkshopPromise = RF.GetStaticField<Promise, SteamSDK>("steamInitialized");
    }

    private static void UploadModLoader(string changelog)
    {
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            ModWorkshopServiceWindows.UploadModLoader(changelog);
        }
        else
        {
            ModWorkshopServiceUnix.UploadModLoader(changelog);
        }
    }
    /// <summary>
    /// Try to Upload a mod to Steam Workshop
    /// </summary>
    public static Promise UploadMod(IMod mod, string changelog, bool verified = false)
    {
        ModDeclare mod_decl = mod.GetDeclaration();
        string name = mod_decl.Name;
        string description = $"{name} Uploaded by NeoModLoader\n" +
                             $"{name} 由NeoModLoader上传\n\n" +
                             $"{mod_decl.Description}\n\n" +
                             $"ModLoader: {CoreConstants.RepoURL}\n\n" +
                             $"模组加载器: {CoreConstants.RepoURL}";
        string workshopPath = SaveManager.generateWorkshopPath(mod_decl.UID);
        if (Directory.Exists(workshopPath))
        {
            Directory.Delete(workshopPath, true);
        }
        Directory.CreateDirectory(workshopPath);
        // Prepare files to upload
        List<string> files_to_upload = SystemUtils.SearchFileRecursive(mod_decl.FolderPath,
            (filename) =>
            {   // To ignore .git and .vscode and so on files
                return !filename.StartsWith(".");
            },
            (dirname) =>
            {   // To ignore .git and .vscode and so on files
                return !dirname.StartsWith(".");
            });
        foreach (string file_full_path in files_to_upload)
        {
            string path = Path.Combine(workshopPath,
                file_full_path.Replace(mod_decl.FolderPath, "").Replace("\\", "/").Substring(1));

            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            File.Copy(file_full_path, path);
        }
        string previewImagePath;
        if (string.IsNullOrEmpty(mod_decl.IconPath))
        {
            using Stream icon_stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NeoModLoader.resources.logo.png");
            using FileStream icon_file = File.Create(Path.Combine(workshopPath, "preview.png"));
            icon_stream.Seek(0, SeekOrigin.Begin);
            icon_stream.CopyTo(icon_file);
            previewImagePath = Path.Combine(workshopPath, "preview.png");
        }
        else
        {
            previewImagePath = Path.Combine(workshopPath, mod_decl.IconPath);
        }
        // This works for BepInEx mods
        if(!File.Exists(Path.Combine(workshopPath, "mod.json")))
        {
            File.WriteAllText(Path.Combine(workshopPath, "mod.json"), Newtonsoft.Json.JsonConvert.SerializeObject(mod_decl));
        }

        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            return UploadModWin(name, description, previewImagePath, workshopPath, changelog, verified);
        }
        else
        {
            return UploadModUnix(name, description, previewImagePath, workshopPath, changelog, verified);
        }
    }
    private static Promise UploadModWin(string name, string description, string previewImagePath, string workshopPath, string changelog, bool verified)
    {
        return ModWorkshopServiceWindows.UploadMod(name, description, previewImagePath, workshopPath, changelog, verified);
    }
    private static Promise UploadModUnix(string name, string description, string previewImagePath, string workshopPath, string changelog, bool verified)
    {
        return ModWorkshopServiceUnix.UploadMod(name, description, previewImagePath, workshopPath, changelog, verified);
    }
    public static Promise TryEditMod(ulong fileID, IMod mod, string changelog)
    {
        ModDeclare mod_decl = mod.GetDeclaration();
        string workshopPath = SaveManager.generateWorkshopPath(mod_decl.UID);
        if (Directory.Exists(workshopPath))
        {
            Directory.Delete(workshopPath, true);
        }
        Directory.CreateDirectory(workshopPath);
        // Prepare files to upload
        List<string> files_to_upload = SystemUtils.SearchFileRecursive(mod_decl.FolderPath,
            (filename) =>
            {   // To ignore .git and .vscode and so on files
                return !filename.StartsWith(".");
            },
            (dirname) =>
            {   // To ignore .git and .vscode and so on files
                return !dirname.StartsWith(".");
            });
        foreach (string file_full_path in files_to_upload)
        {
            string path = Path.Combine(workshopPath,
                file_full_path.Replace(mod_decl.FolderPath, "").Replace("\\", "/").Substring(1));

            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            File.Copy(file_full_path, path);
        }
        string previewImagePath;
        if (string.IsNullOrEmpty(mod_decl.IconPath))
        {
            using Stream icon_stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NeoModLoader.resources.logo.png");
            using FileStream icon_file = File.Create(Path.Combine(workshopPath, "preview.png"));
            icon_stream.Seek(0, SeekOrigin.Begin);
            icon_stream.CopyTo(icon_file);
            previewImagePath = Path.Combine(workshopPath, "preview.png");
        }
        else
        {
            previewImagePath = Path.Combine(workshopPath, mod_decl.IconPath);
        }
        // This works for BepInEx mods
        if(!File.Exists(Path.Combine(workshopPath, "mod.json")))
        {
            File.WriteAllText(Path.Combine(workshopPath, "mod.json"), Newtonsoft.Json.JsonConvert.SerializeObject(mod_decl));
        }

        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            return EditModWin(fileID, previewImagePath, workshopPath, changelog);
        }
        else
        {
           return EditModUnix(fileID, previewImagePath, workshopPath, changelog);
        }
    }
    private static Promise EditModWin(ulong fileID, string previewImagePath, string workshopPath, string changelog)
    {
        return ModWorkshopServiceWindows.EditMod(fileID, previewImagePath, workshopPath, changelog);
    }
    private static Promise EditModUnix(ulong fileID, string previewImagePath, string workshopPath, string changelog)
    {
        return ModWorkshopServiceUnix.EditMod(fileID, previewImagePath, workshopPath, changelog);
    }

    public static void FindSubscribedMods()
    {
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            FindSubscribedModsWin();
        }
        else
        {
            FindSubscribedModsUnix();
        }
    }

    private static void FindSubscribedModsWin()
    {
        ModWorkshopServiceWindows.FindSubscribedMods();
    }
    private static void FindSubscribedModsUnix()
    {
        ModWorkshopServiceUnix.FindSubscribedMods();
    }
    public static ModDeclare GetNextModFromWorkshopItem()
    {
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            return GetNextModFromWorkshopItemWin();
        }
        else
        {
            return GetNextModFromWorkshopItemUnix();
        }
    }
    private static ModDeclare GetNextModFromWorkshopItemWin()
    {
        return ModWorkshopServiceWindows.GetNextModFromWorkshopItem();
    }
    private static ModDeclare GetNextModFromWorkshopItemUnix()
    {
        return ModWorkshopServiceUnix.GetNextModFromWorkshopItem();
    }
}