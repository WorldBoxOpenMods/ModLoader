extern alias winsteamwork;
using System.Reflection;
using System.Threading.Tasks;
using NeoModLoader.api;
using NeoModLoader.constants;
using NeoModLoader.ui;
using NeoModLoader.utils;
using RSG;
using UnityEngine;
using winsteamwork::Steamworks;
using winsteamwork::Steamworks.Ugc;
using WorkshopItem = winsteamwork::Steamworks.Ugc.Item;

namespace NeoModLoader.services;

extern alias winsteamwork;

internal class ModWorkshopServiceWindows : IPlatformSpecificModWorkshopService
{
    static List<winsteamwork::Steamworks.Ugc.Item> subscribedItems = new();
    static Queue<winsteamwork::Steamworks.Ugc.Item> subscribedModsQueue = new();

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
        string changelog, string aiTag = null, List<string> extraTags = null)
    {
        // Create Upload Files Descriptor
        Editor editor = Editor.NewCommunityFile.WithTag(WorkshopTags.Mod)
            .WithTitle(name).WithDescription(description).WithPreviewFile(previewImagePath)
            .WithContent(workshopPath).WithChangeLog(changelog);
        if (!string.IsNullOrEmpty(aiTag))
        {
            editor = editor.WithTag(aiTag);
        }
        if (extraTags != null)
        {
            foreach (string tag in extraTags)
            {
                if (!string.IsNullOrEmpty(tag))
                {
                    editor = editor.WithTag(tag);
                }
            }
        }

        Promise promise = new();
        ModUploadingProgressWindow.UploadProgress uploadProgress = ModUploadingProgressWindow.ShowWindow();
        ExecuteUpload(editor, promise, uploadProgress);
        return promise;
    }

    private async void ExecuteUpload(Editor pEditor, Promise pPromise,
        ModUploadingProgressWindow.UploadProgress pProgress)
    {
        try
        {
            // Resume on Unity's context before updating the upload window or resolving UI callbacks.
            PublishResult result = await pEditor.SubmitAsync(pProgress);
            if (result.NeedsWorkshopAgreement)
            {
                Application.OpenURL("steam://url/CommunityFilePage/" + result.FileId);
            }

            if (result.Result != Result.OK)
            {
                pPromise.Reject(new Exception(result.Result.ToString()));
                return;
            }

            ModUploadingProgressWindow.Instance.fileId = result.FileId;
            pPromise.Resolve();
        }
        catch (Exception exception)
        {
            pPromise.Reject(exception);
        }
    }

    public Promise EditMod(ulong fileID, string previewImagePath, string workshopPath, string changelog, ModDeclare mod_decl)
    {
        Promise promise = new();
        var progress = ModUploadingProgressWindow.ShowWindow();
        ExecuteEditMod(fileID, previewImagePath, workshopPath, changelog, mod_decl, promise, progress);
        return promise;
    }

    private async Task<string> FetchUpdatedDescriptionAsync(ulong fileID, ModDeclare mod_decl)
    {
        try
        {
            Query q = Query.Items.WithFileId(fileID).WithLongDescription(true);
            ResultPage? resultPage = await q.GetPageAsync(1);
            if (resultPage.HasValue)
            {
                foreach (var entry in resultPage.Value.Entries)
                {
                    if (!string.IsNullOrEmpty(entry.Description))
                    {
                        return ModWorkshopService.UpdateDescriptionDisclaimer(entry.Description, mod_decl);
                    }
                }
            }
        }
        catch (Exception e)
        {
            LogService.LogWarning($"Failed to get existing description: {e.Message}");
        }

        return ModWorkshopService.BuildModDescription(mod_decl);
    }

    private Editor ConfigureEditor(ulong fileID, string previewImagePath, string workshopPath, string changelog,
        string desc, ModDeclare mod_decl)
    {
        Editor editor = new Editor(fileID)
            .WithPreviewFile(previewImagePath)
            .WithContent(workshopPath)
            .WithChangeLog(changelog)
            .WithTag(WorkshopTags.Mod);

        if (!string.IsNullOrEmpty(desc))
        {
            editor = editor.WithDescription(desc);
        }

        string aiTag = ModWorkshopService.GetAISteamTag(mod_decl);
        if (!string.IsNullOrEmpty(aiTag))
        {
            editor = editor.WithTag(aiTag);
        }

        if (mod_decl.Tags != null)
        {
            foreach (string tag in mod_decl.Tags)
            {
                if (!string.IsNullOrEmpty(tag))
                {
                    editor = editor.WithTag(tag);
                }
            }
        }

        return editor;
    }

    private async void ExecuteEditMod(ulong fileID, string previewImagePath, string workshopPath, string changelog,
        ModDeclare mod_decl, Promise promise, ModUploadingProgressWindow.UploadProgress progress)
    {
        try
        {
            string desc = await FetchUpdatedDescriptionAsync(fileID, mod_decl);
            Editor editor = ConfigureEditor(fileID, previewImagePath, workshopPath, changelog, desc, mod_decl);

            PublishResult result = await editor.SubmitAsync(progress);
            if (result.NeedsWorkshopAgreement)
            {
                Application.OpenURL("steam://url/CommunityFilePage/" + result.FileId);
            }

            if (result.Result != Result.OK)
            {
                promise.Reject(new Exception(result.Result.ToString()));
                return;
            }

            ModUploadingProgressWindow.Instance.fileId = fileID;
            promise.Resolve();
        }
        catch (Exception ex)
        {
            promise.Reject(ex);
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

    public async void FindSubscribedMods()
    {
        var items = await GetSubscribedItems();
        foreach (var item in items)
        {
            subscribedModsQueue.Enqueue(item);
        }
    }

    static async Task<List<winsteamwork::Steamworks.Ugc.Item>> GetSubscribedItems()
    {
        Query q = Query.ItemsReadyToUse.WhereUserSubscribed()
            .WithTag(WorkshopTags.Mod).WithTag(WorkshopTags.UnverifiedMod)
            .MatchAnyTag().WithoutTag(WorkshopTags.ModLoader);
        q = q.SortByCreationDateAsc();
        subscribedItems.Clear();
        int count = 1;
        int curr = 0;
        int page = 1;

        while (count > curr)
        {
            ResultPage? resultPage = await q.GetPageAsync(page++);
            if (!resultPage.HasValue) break;

            using ResultPage result = resultPage.Value;
            count = result.TotalCount;
            curr += result.ResultCount;
            if (result.ResultCount == 0) break;

            foreach (var entry in result.Entries)
            {
                if (entry.IsInstalled && !entry.IsDownloadPending && !entry.IsDownloading)
                {
                    subscribedItems.Add(entry);
                }
            }
        }

        return subscribedItems;
    }

    public bool IsSteamOnline()
    {
        return SteamClient.IsValid && SteamClient.IsLoggedOn;
    }

    public async Task<ModListingPage> QueryModsAsync(int pPage, bool pRankedByTrend)
    {
        // Query is a struct: every builder method returns a changed copy.
        Query query = Query.ItemsReadyToUse.WithTag(WorkshopTags.Mod).WithTag(WorkshopTags.UnverifiedMod)
            .MatchAnyTag().WithoutTag(WorkshopTags.ModLoader);
        query = pRankedByTrend ? query.RankedByTrend().WithTrendDays(7) : query.RankedByPublicationDate();

        ResultPage? result = await query.GetPageAsync(pPage);
        if (!result.HasValue) return null;

        // The page holds a Steam query handle until it is disposed.
        using ResultPage page = result.Value;
        var listings = new ModListingPage { TotalCount = page.TotalCount };
        foreach (WorkshopItem item in page.Entries)
        {
            listings.Items.Add(new ModListing
            {
                Id = item.Id.Value,
                Title = item.Title ?? "",
                AuthorId = item.Owner.Id.Value,
                Tags = item.Tags ?? Array.Empty<string>(),
                PreviewUrl = item.PreviewImageUrl,
                Subscriptions = item.NumSubscriptions,
                VotesUp = item.VotesUp,
                VotesDown = item.VotesDown,
                Score = item.Score,
                Created = item.Created,
                Updated = item.Updated
            });
        }

        return listings;
    }

    public WorkshopItemState GetItemState(ulong pFileId)
    {
        var item = new WorkshopItem(pFileId);
        bool installed = item.IsInstalled;
        bool downloading = item.IsDownloading;
        return new WorkshopItemState
        {
            Subscribed = item.IsSubscribed,
            Installed = installed,
            NeedsUpdate = item.NeedsUpdate,
            Downloading = downloading,
            DownloadPending = item.IsDownloadPending,
            // DownloadBytesDownloaded reads 0 during a first download; DownloadAmount asks Steam directly.
            Progress = downloading ? item.DownloadAmount : 0,
            Directory = installed ? item.Directory : null
        };
    }

    public Task<bool> SubscribeAsync(ulong pFileId)
    {
        return new WorkshopItem(pFileId).Subscribe();
    }

    public Task<bool> UnsubscribeAsync(ulong pFileId)
    {
        return new WorkshopItem(pFileId).Unsubscribe();
    }

    public bool StartDownload(ulong pFileId)
    {
        return SteamUGC.Download(pFileId, true);
    }

    public async Task<string> GetPersonaNameAsync(ulong pSteamId)
    {
        var friend = new Friend(pSteamId);
        Task request = friend.RequestInfoAsync();
        if (await Task.WhenAny(request, Task.Delay(5000)) == request) await request;

        string name = friend.Name;
        return string.IsNullOrEmpty(name) || name == "[unknown]" ? null : name;
    }

    public async Task<string> GetDescriptionAsync(ulong pFileId)
    {
        ResultPage? result = await Query.Items.WithFileId(pFileId).WithLongDescription(true).GetPageAsync(1);
        if (!result.HasValue) return null;
        using ResultPage page = result.Value;
        return page.Entries.Select(item => item.Description).FirstOrDefault();
    }
}
