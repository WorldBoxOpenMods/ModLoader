using System.Reflection;
using MonoMod.Utils;
using NeoModLoader.api;
using NeoModLoader.constants;
using NeoModLoader.services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Steamworks.Data;
using UnityEngine;

namespace NeoModLoader.utils;

internal static class ModInfoUtils
{
    public static List<api.ModDeclare> findMods()
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

        var workshop_mod_folders = Directory.GetDirectories(Paths.ModsWorkshopPath);
        foreach (var mod_folder in workshop_mod_folders)
        {
            var mod = recogMod(mod_folder, false);
            if (mod != null)
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
        }
        return mods;
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
        FileInfo[] bepinex_plugin_files = bepinex_plugin_folder.GetFiles("*.dll", SearchOption.AllDirectories);
        HashSet<string> bepinex_plugin_file_locs = new HashSet<string>();
        foreach (var file in bepinex_plugin_files)
        {
            bepinex_plugin_file_locs.Add(file.FullName);
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

        mod_compile_timestamps.TryAdd(pModUUID, 0);

        return mod_compile_timestamps[pModUUID];
    }

    private static long getModNewestUpdateTimestamp(string pModFolderPath)
    {
        var dir = new DirectoryInfo(pModFolderPath);
        var files = dir.GetFiles("*", SearchOption.AllDirectories);

        long newest_timestamp = 0;
        
        foreach (var file_info in files)
        {
            newest_timestamp = Math.Max(newest_timestamp, file_info.LastWriteTimeUtc.Ticks);
        }
        return newest_timestamp;
    }
}