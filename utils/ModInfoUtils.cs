using System.Reflection;
using MonoMod.Utils;
using NeoModLoader.api;
using NeoModLoader.constants;
using NeoModLoader.General;
using NeoModLoader.services;
using NeoModLoader.ui;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Steamworks.Data;
using UnityEngine;

namespace NeoModLoader.utils;

internal static class ModInfoUtils
{
    public static List<api.ModDeclare> findAndPrepareMods()
    {
        HashSet<string> findModsIDs = new();
        var mods = new List<api.ModDeclare>();
        if (!NCMSHere())
        {
            var mod_folders = Directory.GetDirectories(Paths.ModsPath);
            foreach (var mod_folder in mod_folders)
            {
                var mod = recogMod(mod_folder);
                if (mod != null)
                {
                    if (findModsIDs.Contains(mod.UUID))
                    {
                        LogService.LogWarning($"Repeat Mod with {mod.UUID}, Only load one of them");
                        continue;
                    }
                    mods.Add(mod);
                    findModsIDs.Add(mod.UUID);
                }
            }
        }

        bool NCMSHere()
        {
            return Directory.GetFiles(Path.Combine(Paths.NMLModPath, ".."), "NCMS*.dll").Length > 0;
        }

        string[] workshop_mod_folders;
        switch (Application.platform) {
            case RuntimePlatform.WindowsPlayer:
                workshop_mod_folders = Directory.GetDirectories(Paths.WindowsModsWorkshopPath);
                break;
            case RuntimePlatform.OSXPlayer:
                workshop_mod_folders = Directory.GetDirectories(Paths.OsxModsWorkshopPath);
                break;
            default:
                Debug.LogWarning("Your platform doesn't have defined behaviour, trying to handle it like Windows...");
                workshop_mod_folders = Directory.GetDirectories(Paths.WindowsModsWorkshopPath);
                break;
        }
        foreach (var mod_folder in workshop_mod_folders)
        {
            var mod = recogMod(mod_folder, false);
            if (mod != null)
            {
                if (mod.ModType == ModTypeEnum.NORMAL)
                {
                    if (findModsIDs.Contains(mod.UUID))
                    {
                        LogService.LogWarning($"Repeat Mod with {mod.UUID}, Only load one of them");
                        continue;
                    }
                    if (string.IsNullOrEmpty(mod.RepoUrl))
                    {
                        mod.SetRepoUrlToWorkshopPage(Path.GetFileName(mod_folder));
                    }
                    mods.Add(mod);
                    findModsIDs.Add(mod.UUID);
                }
                else if (mod.ModType == ModTypeEnum.BEPINEX)
                {
                    LinkBepInExModToLocalRequest(mod);
                }
            }
        }
        return mods;
    }
    private static Queue<ModDeclare> link_request_mods = new();
    
    internal static void DealWithBepInExModLinkRequests()
    {
        if (link_request_mods.Count == 0) return;
        InformationWindow.ShowWindow(LM.Get("ModLinkRequest"));
        new Task(() =>
        {
            Task.Delay(15000);
            List<string> parameters = new List<string>();
            parameters.Add("/c");
            while (link_request_mods.Count > 0)
            {
                var mod = link_request_mods.Dequeue();
                if (parameters.Count != 1)
                {
                    parameters.Add("&&");
                }
                parameters.Add("mklink");
                parameters.Add("/D");
                parameters.Add($"\"{Path.Combine(Paths.BepInExPluginsPath, mod.Name)}\"");
                parameters.Add($"\"{mod.FolderPath}\"");
            }
            SystemUtils.CmdRunAs(parameters.ToArray());
        }).Start();
    }
    internal static void LinkBepInExModToLocalRequest(ModDeclare mod)
    {
        if (!Directory.Exists(Paths.BepInExPluginsPath))
        {
            LogService.LogWarning($"Failed to load mod {mod.Name} which is a BepInEx mod, but BepInEx not found");
            return;
        }
        
        bool already_loaded = false;
        foreach (var loaded_mod in WorldBoxMod.LoadedMods)
        {
            if (loaded_mod.GetDeclaration().UUID == mod.UUID)
            {
                // Just because this mod's folder linked to workshop and already loaded from local folder link.
                //LogService.LogWarning($"Repeat Mod with {mod.UUID}, Only load one of them");
                already_loaded = true;
                break;
            }
        }
        if (already_loaded) return;
        link_request_mods.Enqueue(mod);
    }

    public static ModDeclare recogMod(string pModFolderPath, bool pLogModJsonNotFound = true)
    {
        var mod_config_path = Path.Combine(pModFolderPath, Paths.ModConfigFileName);
        if(!File.Exists(mod_config_path))
        {
            if(pLogModJsonNotFound) 
                LogService.LogWarning($"No mod.json file for folder {pModFolderPath} in Mods");
            return null;
        }
        try
        {
            var mod = new api.ModDeclare(mod_config_path);
            return mod;
        }
        catch (Exception e)
        {
            LogService.LogError($"Error occurs when loading mod config file {mod_config_path}");
            LogService.LogError(e.Message);
            LogService.LogError(e.StackTrace);
            return null;
        }
    }

    public static List<ModDeclare> recogBepInExMods(AppDomain pInspectDomain)
    {
        // TODO: Check repeat or not? Does BepInEx check it?
        var mods = new List<ModDeclare>();
        if (!Directory.Exists(Paths.BepInExPluginsPath))
        {
            return mods;
        }
        
        DirectoryInfo bepinex_plugin_folder = new DirectoryInfo(Paths.BepInExPluginsPath);
        FileInfo[] bepinex_plugin_files;
        
        DirectoryInfo[] bepinex_plugin_sub_folders = bepinex_plugin_folder.GetDirectories();
        
        HashSet<string> bepinex_plugin_file_locs = new HashSet<string>();
        foreach (var folder in bepinex_plugin_sub_folders)
        {
            try
            {
                bepinex_plugin_files = folder.GetFiles("*.dll");
            }
            catch (DirectoryNotFoundException)
            {
                // Just because this directory linked to workshop and workshop mod not downloaded yet or unordered.
                continue;
            }
            if(bepinex_plugin_files.Length == 0) continue;
            bepinex_plugin_file_locs.Add(bepinex_plugin_files[0].FullName);
        }
        
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        
        
        foreach (var assembly in assemblies)
        {
            if (bepinex_plugin_file_locs.Contains(assembly.Location))
            {
                var mod = recogBepInExMod(Path.GetDirectoryName(assembly.Location), assembly);
                if (mod == null)
                {
                    continue;
                }
                mods.Add(mod);
            }
        }


        return mods;
    }
    public static ModDeclare recogBepInExMod(string folder, Assembly pAssembly)
    {
        AssemblyName[] referenced_assemblies = pAssembly.GetReferencedAssemblies();
        bool is_mod = false;
        LogService.LogWarning($"Checking {pAssembly.FullName}");
        foreach (AssemblyName assemblyName in referenced_assemblies)
        {
            if(assemblyName.FullName!= "Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null") continue;
            is_mod = true;
            break;
        }

        if (!is_mod) return null;

        string mod_name = pAssembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title;
        string mod_author = pAssembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;
        string mod_version = pAssembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version;
        if (string.IsNullOrEmpty(mod_version))
        {
            // TODO: I dont know why this happens for an assembly with AssemblyVersionAttribute, but it happens.
            mod_version = pAssembly.GetName().Version.ToString();
        }
        string mod_description = pAssembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;

        var mod = new ModDeclare(mod_name, mod_author, null, mod_version, mod_description, folder, null, null, null);
        mod.SetModType(ModTypeEnum.BEPINEX);
        return mod;
    }
    // ReSharper disable once InconsistentNaming
    public static bool isModNeedRecompile(string pModUUID, string pModFolderPath)
    {
        return getModLastCompileTimestamp(pModUUID) < Others.confirmed_compile_time + getModNewestUpdateTimestamp(pModFolderPath);
    }

    private static readonly Dictionary<string, long> mod_compile_timestamps = new();

    private static readonly JsonSerializerSettings mod_compile_timestamps_serializer_settings = new()
    {
        ContractResolver = new DefaultContractResolver()
    };
    public static void updateModCompileTimestamp(string pModUUID)
    {
        mod_compile_timestamps[pModUUID] = DateTime.UtcNow.Ticks;

        File.WriteAllText(Paths.ModCompileRecordPath,
            JsonConvert.SerializeObject(mod_compile_timestamps, mod_compile_timestamps_serializer_settings));
    }
    // ReSharper disable once InconsistentNaming
    private static long getModLastCompileTimestamp(string pModUUID)
    {
        if (mod_compile_timestamps.Count > 0 && !mod_compile_timestamps.ContainsKey(pModUUID))
        {
            return 0;
        }

        if (mod_compile_timestamps.Count == 0)
        {
            try
            {
                mod_compile_timestamps.AddRange(
                    JsonConvert.DeserializeObject<Dictionary<string, long>>(
                        File.ReadAllText(Paths.ModCompileRecordPath),
                        mod_compile_timestamps_serializer_settings));
            }
            catch (Exception)
            {
                mod_compile_timestamps.Add(pModUUID, 0);
            }
        }
        if (!mod_compile_timestamps.ContainsKey(pModUUID))
        {
            mod_compile_timestamps.Add(pModUUID, 0);
        }

        return mod_compile_timestamps[pModUUID];
    }

    private static long getModNewestUpdateTimestamp(string pModFolderPath)
    {
        var dir = new DirectoryInfo(pModFolderPath);
        var files = SystemUtils.SearchFileRecursive(dir.FullName, (filename) => !filename.StartsWith("."),
            dirname => !dirname.StartsWith(".") && !Paths.IgnoreSearchDirectories.Contains(dirname));

        long newest_timestamp = 0;
        
        foreach (var filepath in files)
        {
            var file_info = new FileInfo(filepath);
            newest_timestamp = Math.Max(newest_timestamp, file_info.LastWriteTimeUtc.Ticks);
        }
        return newest_timestamp;
    }
}