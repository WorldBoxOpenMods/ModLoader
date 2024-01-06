extern alias unixsteamwork;
using System.Reflection;
using NeoModLoader.api;
using NeoModLoader.constants;
using NeoModLoader.ui;
using NeoModLoader.utils;
using RSG;
using UnityEngine;
using unixsteamwork::Steamworks;
using unixsteamwork::Steamworks.Ugc;

namespace NeoModLoader.services;

extern alias unixsteamwork;

internal class ModWorkshopServiceUnix : IPlatformSpecificModWorkshopService
{
    static List<unixsteamwork::Steamworks.Ugc.Item> subscribedItems = new();
    static Queue<unixsteamwork::Steamworks.Ugc.Item> subscribedModsQueue = new();

    public void UploadModLoader(string changelog)
    {
        string workshopPath = SaveManager.generateWorkshopPath(CoreConstants.ModName);

        string previewImagePath = Path.Combine(workshopPath, "preview.png");
        if (Directory.Exists(workshopPath))
        {
            Directory.Delete(workshopPath, true);
        }

        Directory.CreateDirectory(workshopPath);
        // Prepare files to upload
        File.Copy(Paths.NMLModPath, Path.Combine(workshopPath, "NeoModLoader.dll"));
        File.Copy(Paths.NMLModPath.Replace(".dll", ".pdb"), Path.Combine(workshopPath, "NeoModLoader.pdb"));

        using Stream icon_stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("NeoModLoader.resources.logo.png");
        using FileStream icon_file = File.Create(previewImagePath);
        icon_stream.Seek(0, SeekOrigin.Begin);
        icon_stream.CopyTo(icon_file);
        icon_file.Close();

        Editor editor = new Editor(CoreConstants.WorkshopFileId).WithContent(workshopPath).WithTag("Mod Loader")
            .WithPreviewFile(previewImagePath)
            .WithChangeLog(changelog);

        editor.SubmitAsync(null).ContinueWith(delegate(Task<PublishResult> taskResult)
        {
            if (taskResult.Status != TaskStatus.RanToCompletion)
            {
                LogService.LogErrorConcurrent("!RanToCompletion");
                return;
            }

            PublishResult result = taskResult.Result;
            // Result process refer to: https://partner.steamgames.com/doc/api/steam_api#EResult
            if (!result.Success)
            {
                LogService.LogErrorConcurrent("!result.Success");
            }

            if (result.NeedsWorkshopAgreement)
            {
                Application.OpenURL("steam://url/CommunityFilePage/" + result.FileId);
            }

            if (result.Result != Result.OK)
            {
                LogService.LogErrorConcurrent(result.Result.ToString());
            }
        }, TaskScheduler.Default);
    }

    public Promise UploadMod(string name, string description, string previewImagePath, string workshopPath,
        string changelog, bool verified)
    {
        // Create Upload Files Descriptor
        Editor editor = Editor.NewCommunityFile.WithTag(verified ? "Mod" : "Unverified Mod")
            .WithTitle(name).WithDescription(description).WithPreviewFile(previewImagePath)
            .WithContent(workshopPath).WithChangeLog(changelog);

        Promise promise = new();
        ModUploadingProgressWindow.UploadProgress uploadProgress = ModUploadingProgressWindow.ShowWindow();
        editor.SubmitAsync(uploadProgress).ContinueWith(delegate(Task<PublishResult> taskResult)
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
                Application.OpenURL("steam://url/CommunityFilePage/" + result.FileId);
            }

            if (result.Result != Result.OK)
            {
                promise.Reject(new Exception("Something went wrong: " + result.Result.ToString()));
                return;
            }

            ModUploadingProgressWindow.Instance.fileId = result.FileId;
            promise.Resolve();
        }, TaskScheduler.Default);

        return promise;
    }

    public Promise EditMod(ulong fileID, string previewImagePath, string workshopPath, string changelog)
    {
        Promise promise = new();
        // Create Upload Files Descriptor
        Editor editor = new Editor(fileID)
            .WithPreviewFile(previewImagePath)
            .WithContent(workshopPath).WithChangeLog(changelog);

        editor.SubmitAsync(ModUploadingProgressWindow.ShowWindow()).ContinueWith(
            delegate(Task<PublishResult> taskResult)
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

    public async void FindSubscribedMods()
    {
        var items = await GetSubscribedItems();
        foreach (var item in items)
        {
            subscribedModsQueue.Enqueue(item);
        }
    }

    public ModDeclare GetNextModFromWorkshopItem()
    {
        if (subscribedModsQueue.Count == 0)
        {
            return null;
        }

        var item = subscribedModsQueue.Dequeue();
        ModDeclare modDeclare = ModInfoUtils.recogMod(item.Directory);
        if (string.IsNullOrEmpty(modDeclare.RepoUrl))
        {
            string id = Path.GetFileName(item.Directory);
            modDeclare.SetRepoUrlToWorkshopPage(id);
        }

        return modDeclare;
    }

    static async Task<List<unixsteamwork::Steamworks.Ugc.Item>> GetSubscribedItems()
    {
        bool available(unixsteamwork::Steamworks.Ugc.Item item)
        {
            return true;
        }

        Query q = Query.ItemsReadyToUse.WhereUserSubscribed().WithTag("Mod");
        q = q.SortByCreationDateAsc();
        subscribedItems.Clear();
        int count = 1;
        int curr = 0;
        int page = 1;

        while (count > curr)
        {
            ResultPage? resultPage = await q.GetPageAsync(page++);
            if (!resultPage.HasValue) break;

            count = resultPage.Value.TotalCount;
            curr += resultPage.Value.ResultCount;

            foreach (var entry in resultPage.Value.Entries)
            {
                if (entry.IsInstalled && !entry.IsDownloadPending && !entry.IsDownloading)
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