using System.Reflection;
using HarmonyLib;
using NeoModLoader.api.attributes;
using NeoModLoader.constants;
using RSG;
using Steamworks;
using Steamworks.Ugc;
using UnityEngine;

namespace NeoModLoader.services;

[Experimental]
public static class ModUploadService
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(WorkshopMaps), "uploadMap")]
    public static bool uploadMapPrefix(ref Promise __result)
    {
        LogService.LogWarning("uploadMapPrefix");
        __result = UploadMod(Directory.GetFiles(Paths.CompiledModsPath).FirstOrDefault((str) =>
        {
            return str.Contains("TESTMOD.zip");
        }));
        return false;
    }
    /// <summary>
    /// Try to Upload a mod as map to Steam Workshop
    /// </summary>
    /// <param name="path">Path to mod's dll</param>
    /// <returns>Success or not</returns>
    public static Promise UploadMod(string path)
    {
        LogService.LogWarning("Enter UploadMod");
        
        string name = "启源Mod";
        string description = "启源Mod Uploaded as Map by NeoModLoader";
        string workshopPath = SaveManager.generateWorkshopPath("CW_CORE");
        if (Directory.Exists(workshopPath))
        {
            Directory.Delete(workshopPath, true);
        }
        Directory.CreateDirectory(workshopPath);
        
        LogService.LogWarning("Create workshopPath");
        
        File.Copy(path, Path.Combine(workshopPath, Path.GetFileName(path)));
        using Stream icon_stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NeoModLoader.resources.logo.png");
        FileStream icon_file = File.Create(Path.Combine(workshopPath, "preview.png"));
        icon_stream.Seek(0, SeekOrigin.Begin);
        icon_stream.CopyTo(icon_file);
        icon_file.Close();
        
        string previewImagePath = Path.Combine(workshopPath, "preview.png");
        //string mainPath = Path.Combine(workshopPath, Path.GetFileName(path));

        LogService.LogWarning($"To upload {name}, {description}, {workshopPath}, {previewImagePath}");
        
        Editor editor = Editor.NewCommunityFile.WithTag("World");
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
}