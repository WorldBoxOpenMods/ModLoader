using System.Reflection;
using HarmonyLib;
using NeoModLoader.api;
using NeoModLoader.api.attributes;
using NeoModLoader.constants;
using NeoModLoader.General;
using NeoModLoader.ui;
using NeoModLoader.utils;
using RSG;
using Steamworks;
using Steamworks.Data;
using Steamworks.Ugc;
using UnityEngine;
using Item = Steamworks.Ugc.Item;

namespace NeoModLoader.services;

[Experimental]
internal static class ModWorkshopService
{
    internal static Promise steamWorkshopPromise;

    public static void Init()
    {
        steamWorkshopPromise = Reflection.GetStaticField<Promise, SteamSDK>("steamInitialized");
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
        string workshopPath = SaveManager.generateWorkshopPath(mod_decl.UUID);
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
        
        // Create Upload Files Descriptor
        Editor editor = Editor.NewCommunityFile.WithTag(verified ? "Mod" : "Unverified Mod")
            .WithTitle(name).WithDescription(description).WithPreviewFile(previewImagePath)
            .WithContent(workshopPath).WithChangeLog(changelog);
        
        Promise promise = new();
        editor.SubmitAsync(ModUploadingProgressWindow.ShowWindow()).ContinueWith(delegate(Task<PublishResult> taskResult)
        {
            if (taskResult.Status != TaskStatus.RanToCompletion)
            {
                promise.Reject(taskResult.Exception.GetBaseException());
                return;
            }
            PublishResult result = taskResult.Result;
            // Result process refer to: https://partner.steamgames.com/doc/api/steam_api#EResult
            if (!result.Success)
            {
                LogService.LogError("!result.Success");
            }
            if (result.NeedsWorkshopAgreement)
            {
                LogService.LogError("w: Needs Workshop Agreement");
                // TODO: Open Workshop Agreement
                Application.OpenURL("steam://url/CommunityFilePage/" + result.FileId);
            }
            if (result.Result != Result.OK)
            {
                LogService.LogError(result.Result.ToString());
                promise.Reject(new Exception("Something went wrong: " + result.Result.ToString()));
                return;
            }

            // result.FileId;
            promise.Resolve();
        }, TaskScheduler.FromCurrentSynchronizationContext());

        return promise;
    }
    public static Promise TryEditMod(ulong fileID, IMod mod, string changelog)
    {
        ModDeclare mod_decl = mod.GetDeclaration();
        string workshopPath = SaveManager.generateWorkshopPath(mod_decl.UUID);
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
        
        // Create Upload Files Descriptor
        Editor editor = new Editor(fileID)
            .WithPreviewFile(previewImagePath)
            .WithContent(workshopPath).WithChangeLog(changelog);
        
        Promise promise = new();
        editor.SubmitAsync(ModUploadingProgressWindow.ShowWindow()).ContinueWith(delegate(Task<PublishResult> taskResult)
        {
            if (taskResult.Status != TaskStatus.RanToCompletion)
            {
                promise.Reject(taskResult.Exception.GetBaseException());
                return;
            }
            PublishResult result = taskResult.Result;
            // Result process refer to: https://partner.steamgames.com/doc/api/steam_api#EResult
            if (result.NeedsWorkshopAgreement)
            {
                LogService.LogWarning("Needs Workshop Agreement");
                // TODO: Open Workshop Agreement
                Application.OpenURL("steam://url/CommunityFilePage/" + result.FileId);
            }
            if (result.Result != Result.OK)
            {
                promise.Reject(new Exception(result.Result.ToString()));
                return;
            }

            // result.FileId;
            promise.Resolve();
        }, TaskScheduler.FromCurrentSynchronizationContext());

        return promise;
    }
    public static ModDeclare GetModFromWorkshopItem(Steamworks.Ugc.Item item)
    {
        ModDeclare modDeclare = ModInfoUtils.recogMod(item.Directory);
        if (string.IsNullOrEmpty(modDeclare.RepoUrl))
        {
            string id = Path.GetFileName(item.Directory);
            modDeclare.SetRepoUrlToWorkshopPage(id);
        }

        return modDeclare;
    }
    static List<Steamworks.Ugc.Item> subscribedItems = new();
    public static async Task<List<Steamworks.Ugc.Item>> GetSubscribedItems()
    {
        Query q = Query.ItemsReadyToUse.WhereUserSubscribed().WithTag("Mod");
        q = q.SortByCreationDateAsc();
        subscribedItems.Clear();
        int count = 1;
        int curr = 0;
        int page = 1;
        
        bool available(Steamworks.Ugc.Item item)
        {
            return true;
        }
        
        while (count > curr)
        {
            ResultPage? resultPage = await q.GetPageAsync(page++);
            if (!resultPage.HasValue) break;

            count = resultPage.Value.TotalCount;
            curr += resultPage.Value.ResultCount;
            
            foreach(var entry in resultPage.Value.Entries)
            {
                if(entry.IsInstalled && ! entry.IsDownloadPending && ! entry.IsDownloading)
                {
                    if (!available(entry))
                    {
                        LogService.LogWarning($"Incomplete mod {entry.Title} found, skip");
                    }
                    else
                    {
                        subscribedItems.Add(entry);
                    }
                }
            }
        }

        return subscribedItems;
    }
}