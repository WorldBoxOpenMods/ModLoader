using System.Reflection;
using HarmonyLib;
using NeoModLoader.api;
using NeoModLoader.api.attributes;
using NeoModLoader.constants;
using NeoModLoader.General;
using NeoModLoader.utils;
using RSG;
using Steamworks;
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
    [HarmonyPrefix]
    [HarmonyPatch(typeof(WorkshopMaps), "uploadMap")]
    public static bool uploadMapPrefix(ref Promise __result)
    {
        LogService.LogWarning("uploadMapPrefix");
        __result = UploadMod(WorldBoxMod.LoadedMods.First((mod)=>!mod.GetDeclaration().Name.Contains("启源")));
        return false;
    }
    /// <summary>
    /// Try to Upload a mod as map to Steam Workshop
    /// </summary>
    /// <param name="path">Path to mod's dll</param>
    /// <returns>Success or not</returns>
    public static Promise UploadMod(IMod mod)
    {
        LogService.LogWarning("Enter UploadMod");
        
        ModDeclare mod_decl = mod.GetDeclaration();
        string name = mod_decl.Name;
        string description = $"{name} Uploaded by NeoModLoader\n\n" +
                             $"{mod_decl.Description}\n\n" +
                             $"NOTE: MOD CANNOT BE LOAD CURRENTLY. ATTENTIONTO: {CoreConstants.RepoURL}\n\n" +
                             $"{name} 模组由NeoModLoader上传\n\n" +
                             $"注意: 该模组目前无法加载. " +
                             $"加载器关注仓库动态: {CoreConstants.RepoURL}";
        string workshopPath = SaveManager.generateWorkshopPath(mod_decl.UUID);
        if (Directory.Exists(workshopPath))
        {
            Directory.Delete(workshopPath, true);
        }
        Directory.CreateDirectory(workshopPath);
        
        LogService.LogWarning("Create workshopPath");
        
        foreach (string file_full_path in Directory.GetFiles(mod_decl.FolderPath, "*.*", SearchOption.AllDirectories))
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
            FileStream icon_file = File.Create(Path.Combine(workshopPath, "preview.png"));
            icon_stream.Seek(0, SeekOrigin.Begin);
            icon_stream.CopyTo(icon_file);
            icon_file.Close();
            previewImagePath = Path.Combine(workshopPath, "preview.png");
        }
        else
        {
            previewImagePath = Path.Combine(workshopPath, mod_decl.IconPath);
        }
        //string mainPath = Path.Combine(workshopPath, Path.GetFileName(path));

        LogService.LogWarning($"To upload {name}, {description}, {workshopPath}, {previewImagePath}");
        
        Editor editor = Editor.NewCommunityFile.WithTag("Mod");
        editor = editor.WithTitle(name).WithDescription(description).WithPreviewFile(previewImagePath)
            .WithContent(workshopPath);
        Promise promise = new();
/*
        object progressTracker = Type.GetType("WorkshopUploadProgress")
            .GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[0], null).Invoke(null);
        ReflectionUtility.Reflection.SetStaticField(typeof(WorkshopMaps), "uploadProgressTracker", progressTracker);
*/

        
        editor.SubmitAsync(null).ContinueWith(delegate(Task<PublishResult> taskResult)
        {
            if (taskResult == null)
            {
                LogService.LogError("taskResult is null");
            }
            if (taskResult.Status != TaskStatus.RanToCompletion)
            {
                LogService.LogError("Status != TaskStatus.RanToCompletion");
                try
                {
                    promise.Reject(taskResult.Exception.GetBaseException());
                }
                catch (Exception e)
                {
                    LogService.LogError("Status != TaskStatus.RanToCompletion and taskResult is null");
                }
                return;
            }
            PublishResult result = taskResult.Result;
            LogService.LogInfo("Get taskresult.result");
            if (!result.Success)
            {
                LogService.LogError("!result.Success");
            }
            if (result.NeedsWorkshopAgreement)
            {
                LogService.LogError("w: Needs Workshop Agreement");
                WorkshopUploadingWorldWindow.needsWorkshopAgreement = true;
                WorkshopOpenSteamWorkshop.fileID = result.FileId.ToString();
            }
            if (result.Result != Result.OK)
            {
                LogService.LogError(result.Result.ToString());
                promise.Reject(new Exception("Something went wrong: " + result.Result.ToString()));
                return;
            }
            WorkshopMaps.uploaded_file_id = result.FileId;
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