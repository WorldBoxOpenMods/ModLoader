extern alias unixsteamwork;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using NeoModLoader.api;
using NeoModLoader.constants;
using NeoModLoader.General;
using NeoModLoader.services;
using NeoModLoader.ui;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using unixsteamwork::Steamworks;

namespace NeoModLoader.utils;

internal static class ModInfoUtils
{
    private static Queue<ModDeclare> link_request_mods = new();
    private static bool to_install_bepinex;

    private static Dictionary<string, ModCompilationCache> mod_compilation_caches;

    private static readonly Dictionary<string, long> mod_last_update_timestamps = new();

    public static void InitializeModCompileCache()
    {
        if (!File.Exists(Paths.ModCompileRecordPath)) File.WriteAllText(Paths.ModCompileRecordPath, "{}");
        var json = File.ReadAllText(Paths.ModCompileRecordPath);
        var json_settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.Indented
        };
        try
        {
            mod_compilation_caches =
                JsonConvert.DeserializeObject<Dictionary<string, ModCompilationCache>>(json, json_settings) ??
                new Dictionary<string, ModCompilationCache>();
        }
        catch (Exception)
        {
            mod_compilation_caches = new Dictionary<string, ModCompilationCache>();
        }
        finally
        {
            mod_compilation_caches ??= new Dictionary<string, ModCompilationCache>();
        }

        if (File.Exists(Paths.ModsDisabledRecordPath))
        {
            var old_disabled = new List<string>(File.ReadAllLines(Paths.ModsDisabledRecordPath));
            foreach (var disabled in old_disabled)
                if (!mod_compilation_caches.ContainsKey(disabled))
                {
                    mod_compilation_caches[disabled] = new ModCompilationCache(disabled);
                    mod_compilation_caches[disabled].disabled = true;
                }
                else
                {
                    mod_compilation_caches[disabled].disabled = true;
                }

            File.Delete(Paths.ModsDisabledRecordPath);
        }
    }

    public static string TryToUnzipModZip(string pZipFile)
    {
        var extract_path = Path.Combine(Application.temporaryCachePath,
            Path.GetFileNameWithoutExtension(pZipFile));
        if (Directory.Exists(extract_path)) Directory.Delete(extract_path, true);

        try
        {
            ZipFile.ExtractToDirectory(pZipFile, extract_path);
        }
        catch (Exception e)
        {
            if (Directory.Exists(extract_path)) Directory.Delete(extract_path, true);

            LogService.LogError($"Error occurs when extracting {pZipFile}");
            LogService.LogError(e.Message);
            LogService.LogError(e.StackTrace);
            return "";
        }

        var mod_json_files = SystemUtils.SearchFileRecursive(extract_path,
            filename => filename == Paths.ModDeclarationFileName,
            dirname => true);
        if (mod_json_files.Count == 0)
        {
            Directory.Delete(extract_path, true);
            return "";
        }

        if (mod_json_files.Count > 1)
            LogService.LogWarning($"More than one mod.json file in {pZipFile}, only load the first one");
        var target_folder_name = Path.GetFileNameWithoutExtension(pZipFile);
        try
        {
            ModDeclare mod_declare = new(mod_json_files[0]);
            target_folder_name = mod_declare.UID;
        }
        catch (Exception e)
        {
            return "";
        }

        try
        {
            SystemUtils.CopyDirectory(Path.GetDirectoryName(mod_json_files[0]),
                Path.Combine(Paths.ModsPath, target_folder_name));
            return Path.Combine(Paths.ModsPath, target_folder_name);
        }
        catch (UnauthorizedAccessException)
        {
            ZipFile.ExtractToDirectory(pZipFile,
                Path.Combine(Paths.ModsPath, Path.GetFileNameWithoutExtension(pZipFile)));
        }
        finally
        {
            try
            {
                File.Delete(pZipFile);
                if (Directory.Exists(extract_path))
                    Directory.Delete(extract_path, true);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        return "";
    }

    public static void CheckModsFolder(string pFolderPath, HashSet<string> pFindModsIDs, List<ModDeclare> pModsToFill,
        bool pLogModJsonNotFound = true)
    {
        if (!Directory.Exists(pFolderPath)) return;
        var zipped_mods = new HashSet<string>(Directory.GetFiles(pFolderPath, "*.zip"))
            .Union(Directory.GetFiles(pFolderPath, "*.7z"))
            .Union(Directory.GetFiles(pFolderPath, "*.rar"))
            .Union(Directory.GetFiles(pFolderPath, "*.tar"))
            .Union(Directory.GetFiles(pFolderPath, "*.tar.gz"))
            .Union(Directory.GetFiles(pFolderPath, "*.mod"));
        foreach (var zipped_mod in zipped_mods) TryToUnzipModZip(zipped_mod);

        var mod_folders = Directory.GetDirectories(pFolderPath);
        foreach (var mod_folder in mod_folders)
        {
            ModDeclare mod = recogMod(mod_folder, pLogModJsonNotFound);
            if (mod != null)
            {
                if (pFindModsIDs.Contains(mod.UID))
                {
                    LogService.LogWarning($"Repeat Mod with {mod.UID}, Only load one of them");
                    continue;
                }

                pModsToFill.Add(mod);
                pFindModsIDs.Add(mod.UID);
            }
        }
    }

    public static List<ModDeclare> findAndPrepareMods()
    {
        HashSet<string> findModsIDs = new();
        var mods = new List<ModDeclare>();
        if (!NCMSHere())
        {
            CheckModsFolder(Paths.ModsPath, findModsIDs, mods);
        }

        CheckModsFolder(Paths.NativeModsPath, findModsIDs, mods, false);

        bool NCMSHere()
        {
            return false;
            return Directory.GetFiles(Paths.NativeModsPath, "NCMS*.dll").Length > 0;
        }

        string[] workshop_mod_folders;
        if (Others.is_editor)
        {
            goto SKIP_WORKSHOP;
        }

        try
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.OSXPlayer:
                    workshop_mod_folders = Directory.GetDirectories(Paths.CommonModsWorkshopPath);
                    break;
                default:
                    LogService.LogWarning(
                        $"Your platform {Application.platform.ToString()} doesn't have defined behaviour, trying to handle it like Windows...");
                    workshop_mod_folders = Directory.GetDirectories(Paths.CommonModsWorkshopPath);
                    break;
            }
        }
        catch (DirectoryNotFoundException)
        {
            LogService.LogWarning("Workshop folder not found, skip loading workshop mods");
            goto SKIP_WORKSHOP;
        }

        foreach (var mod_folder in workshop_mod_folders)
        {
            var mod = recogMod(mod_folder, false);
            if (mod != null)
            {
                if (mod.ModType == ModTypeEnum.NEOMOD)
                {
                    if (findModsIDs.Contains(mod.UID))
                    {
                        LogService.LogWarning($"Repeat Mod with {mod.UID}, Only load one of them");
                        continue;
                    }

                    if (string.IsNullOrEmpty(mod.RepoUrl))
                    {
                        mod.SetRepoUrlToWorkshopPage(Path.GetFileName(mod_folder));
                    }

                    mods.Add(mod);
                    findModsIDs.Add(mod.UID);
                }
                else if (mod.ModType == ModTypeEnum.BEPINEX)
                {
                    LinkBepInExModToLocalRequest(mod);
                }
            }
        }

        SKIP_WORKSHOP:
        foreach (var mod in mods)
        {
            WorldBoxMod.AllRecognizedMods[mod] = ModState.FAILED;
        }

        return removeDisabledMods(mods);
    }

    private static List<ModDeclare> removeDisabledMods(List<ModDeclare> mods_to_process)
    {
        var result = new List<ModDeclare>();
        foreach (var mod in mods_to_process)
        {
            if (isModDisabled(mod.UID))
            {
                WorldBoxMod.AllRecognizedMods[mod] = ModState.DISABLED;
            }
            else
            {
                result.Add(mod);
            }
        }

        return result;
    }

    internal static void DealWithBepInExModLinkRequests()
    {
        if (link_request_mods.Count == 0) return;
        InformationWindow.ShowWindow(LM.Get("ModLinkRequest"), InstallBepInExMod);
    }

    private static void InstallBepInExMod()
    {
        if (to_install_bepinex)
        {
            try
            {
                InstallBepInEx();
            }
            catch (Exception e)
            {
                LogService.LogError(e.Message);
                LogService.LogError(e.StackTrace);
                return;
            }

            to_install_bepinex = false;
        }

        if (!Directory.Exists(Paths.BepInExPluginsPath)) Directory.CreateDirectory(Paths.BepInExPluginsPath);

        var parameters = new List<string>();
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
                parameters.Add("/c");
                while (link_request_mods.Count > 0)
                {
                    ModDeclare mod = link_request_mods.Dequeue();
                    if (parameters.Count != 1) parameters.Add("&&");

                    parameters.Add("mklink");
                    parameters.Add("/D");
                    parameters.Add($"\"{Path.Combine(Paths.BepInExPluginsPath, mod.Name)}\"");
                    parameters.Add($"\"{mod.FolderPath}\"");
                }

                SystemUtils.CmdRunAs(parameters.ToArray());
                break;
            case RuntimePlatform.LinuxPlayer:
            case RuntimePlatform.OSXPlayer:
                parameters.Add("-c");
                while (link_request_mods.Count > 0)
                {
                    ModDeclare mod = link_request_mods.Dequeue();
                    if (parameters.Count != 1) parameters.Add("&&");

                    parameters.Add("ln");
                    parameters.Add("-s");
                    parameters.Add($"\"{mod.FolderPath}\"");
                    parameters.Add($"\"{Path.Combine(Paths.BepInExPluginsPath, mod.Name)}\"");
                }

                SystemUtils.BashRun(parameters.ToArray());
                break;
        }
    }

    private static void InstallBepInEx()
    {
        WebClient client = new();
        string download_path = Path.Combine(Path.GetTempPath(), "bepinex.zip");
        string download_url = Application.platform switch
        {
            RuntimePlatform.WindowsPlayer =>
                "https://github.com/BepInEx/BepInEx/releases/download/v5.4.22/BepInEx_x64_5.4.22.0.zip",
            RuntimePlatform.LinuxPlayer =>
                "https://github.com/BepInEx/BepInEx/releases/download/v5.4.22/BepInEx_unix_5.4.22.0.zip",
            RuntimePlatform.OSXPlayer =>
                "https://github.com/BepInEx/BepInEx/releases/download/v5.4.22/BepInEx_unix_5.4.22.0.zip",
            _ => "https://github.com/BepInEx/BepInEx/releases/download/v5.4.22/BepInEx_x64_5.4.22.0.zip"
        };
        client.DownloadFile(download_url, download_path);
        try
        {
            ZipFile.ExtractToDirectory(download_path, Paths.GamePath);
        }
        catch (Exception)
        {
            // ignored. maybe file already exists
        }

        File.Delete(download_path);

        switch (Application.platform)
        {
            case RuntimePlatform.LinuxPlayer:
            case RuntimePlatform.OSXPlayer:

                string bepinex_run_sh_path = Path.Combine(Paths.GamePath, "run_bepinex.sh");
                string executable_name = "";
                foreach (string file in Directory.GetFiles(Paths.GamePath))
                {
                    FileInfo file_info = new FileInfo(file);
                    if (!file_info.Name.Contains("worldbox")) continue;
                    executable_name = file_info.Name;
                    break;
                }

                if (string.IsNullOrEmpty(executable_name))
                {
                    LogService.LogErrorConcurrent("Failed to find WorldBox executable file!");
                    LogService.LogWarningConcurrent("Set it as \"worldbox\" automatically");
                    executable_name = "worldbox";
                }

                string sh_text = File.ReadAllText(bepinex_run_sh_path);
                sh_text = sh_text.Replace("executable_name=\"\"", $"executable_name=\"{executable_name}\"");
                File.WriteAllText(bepinex_run_sh_path, sh_text);

                // Write launch script
                if (Application.platform == RuntimePlatform.LinuxPlayer)
                {
                    string launch_script_path = string.Format(Paths.LinuxSteamLocalConfigPath,
                        SteamClient.SteamId.AccountId.ToString());

                    var result = VdfConvert.Deserialize(File.ReadAllText(launch_script_path));
                    result.Value["Software"]["Valve"]["Steam"]["apps"][CoreConstants.GameId.ToString()]
                        ["LaunchOptions"] = new VValue($"{bepinex_run_sh_path} %command%");
                    File.WriteAllText(launch_script_path, VdfConvert.Serialize(result));
                }
                else
                {
                    LogService.LogWarningConcurrent("You are using macOS, please add launch script manually");
                }

                SystemUtils.BashRun(new string[] { "-c", "chmod", "u+x", bepinex_run_sh_path });
                break;
            default:
                break;
        }

        LogService.LogInfo($"Install BepInEx to {Paths.GamePath}");
    }

    internal static void LinkBepInExModToLocalRequest(ModDeclare mod)
    {
        if (!Directory.Exists(Paths.BepInExPluginsPath))
        {
            LogService.LogInfo(
                $"Find a BepInEx mod {mod.Name} but BepInEx not found, Add Install BepInEx Task into queue");
            to_install_bepinex = true;
        }

        bool already_loaded = false;
        foreach (var loaded_mod in WorldBoxMod.LoadedMods)
        {
            if (loaded_mod.GetDeclaration().UID == mod.UID)
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
        var mod_config_path = Path.Combine(pModFolderPath, Paths.ModDeclarationFileName);
        if (!File.Exists(mod_config_path))
        {
            var possible_mod_config_path = SystemUtils.SearchFileRecursive(pModFolderPath,
                file_name =>
                    file_name ==
                    Paths.ModDeclarationFileName,
                _ => true);
            if (possible_mod_config_path.Count == 0)
            {
                if (pLogModJsonNotFound)
                    LogService.LogWarning($"No mod.json file for folder {pModFolderPath} in Mods");
                return null;
            }

            if (possible_mod_config_path.Count > 1)
                LogService.LogWarning(
                    $"More than one mod.json file in mod folder, only load the first one at '{possible_mod_config_path[0]}'");
            mod_config_path = possible_mod_config_path[0];
        }

        try
        {
            var mod = new ModDeclare(mod_config_path);
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

    public static List<ModDeclare> recogBepInExMods()
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

            if (bepinex_plugin_files.Length == 0) continue;
            bepinex_plugin_file_locs.Add(bepinex_plugin_files[0].FullName);
        }

        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();


        foreach (var assembly in assemblies)
        {
            string assembly_location;
            try
            {
                assembly_location = assembly.Location;
            }
            catch (NotSupportedException)
            {
                continue;
            }

            if (bepinex_plugin_file_locs.Contains(assembly_location))
            {
                string folder_path = Path.GetDirectoryName(assembly_location);
                var mod = recogBepInExMod(folder_path, assembly);
                if (mod == null)
                {
                    continue;
                }

                if (File.Exists(Path.Combine(folder_path, "icon.png")))
                {
                    mod.SetIconPath(Path.Combine(folder_path, "icon.png"));
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
            if (assemblyName.FullName !=
                "Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null") continue;
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

    public static bool isModDisabled(string pModUID)
    {
        return mod_compilation_caches.TryGetValue(pModUID, out ModCompilationCache cache) && cache.disabled;
    }

    /// <summary>
    /// Toggle Mod Disabled Status
    /// </summary>
    /// <param name="pModUID"></param>
    /// <returns>Enable mod and return true; or disable mod and return false</returns>
    public static bool toggleMod(string pModUID, bool pSave = true)
    {
        if (!mod_compilation_caches.TryGetValue(pModUID, out ModCompilationCache cache))
        {
            cache = new ModCompilationCache(pModUID);
            cache.disabled = true;
            mod_compilation_caches[pModUID] = cache;

            return false;
        }

        var result = cache.disabled;
        cache.disabled = !cache.disabled;
        if (pSave)
            SaveModRecords();

        return result;
    }

    public static void SaveModRecords()
    {
        var json_settings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver(),
            Formatting = Formatting.Indented
        };
        var json = JsonConvert.SerializeObject(mod_compilation_caches, json_settings);
        File.WriteAllText(Paths.ModCompileRecordPath, json);
    }

    public static void RecordMod(ModDeclare pModDeclare, List<string> pDependencies, List<string> pOptionalDependencies,
        bool pDisabled = false, bool pSave = true)
    {
        if (!mod_compilation_caches.TryGetValue(pModDeclare.UID, out ModCompilationCache cache))
        {
            cache = new ModCompilationCache(pModDeclare, pDependencies, pOptionalDependencies);
        }
        else
        {
            cache.dependencies = new List<string>(pDependencies);
            cache.optional_dependencies = new List<string>(pOptionalDependencies);
        }

        cache.disabled = pDisabled;
        cache.timestamp = getModNewestUpdateTimestamp(pModDeclare.FolderPath);

        mod_compilation_caches[pModDeclare.UID] = cache;
        if (pSave)
            SaveModRecords();
    }

    // ReSharper disable once InconsistentNaming
    public static bool doesModNeedRecompile(ModDeclare pModDeclare, List<string> pDependencies,
        List<string> pOptionalDependencies)
    {
        if (!mod_compilation_caches.TryGetValue(pModDeclare.UID, out ModCompilationCache cache)) return true;
        if (!File.Exists(Path.Combine(Paths.CompiledModsPath, pModDeclare.UID))) return true;
        var curr = new HashSet<string>(pDependencies);
        var last = new HashSet<string>(cache.dependencies);

        if (!curr.SetEquals(last)) return true;
        curr = new HashSet<string>(pOptionalDependencies);
        last = new HashSet<string>(cache.optional_dependencies);
        if (!curr.SetEquals(last)) return true;

        var last_compile_time = cache.timestamp;
        bool need_recompile = last_compile_time <
                              Others.confirmed_compile_time + getModNewestUpdateTimestamp(pModDeclare.FolderPath);
        if (need_recompile) return true;

        foreach (var depen in pDependencies)
        {
            need_recompile |= last_compile_time < Others.confirmed_compile_time + getModLastCompileTimestamp(depen);
            if (need_recompile) return true;
        }

        foreach (var depen in pOptionalDependencies)
        {
            need_recompile |= last_compile_time < Others.confirmed_compile_time + getModLastCompileTimestamp(depen);
            if (need_recompile) return true;
        }

        return false;
    }

    public static void clearModCompileTimestamp(string pModUUID, bool pSave = true)
    {
        if (!mod_compilation_caches.TryGetValue(pModUUID, out ModCompilationCache cache))
        {
            cache = new ModCompilationCache(pModUUID);
            cache.disabled = false;
            cache.timestamp = 0;
            mod_compilation_caches[pModUUID] = cache;
            return;
        }

        cache.timestamp = 0;
        if (pSave)
            SaveModRecords();
    }

    // ReSharper disable once InconsistentNaming
    private static long getModLastCompileTimestamp(string pModUID)
    {
        return mod_compilation_caches.TryGetValue(pModUID, out ModCompilationCache cache) ? cache.timestamp : 0;
    }

    private static long getModNewestUpdateTimestamp(string pModFolderPath)
    {
        var dir = new DirectoryInfo(pModFolderPath);
        if (mod_last_update_timestamps.ContainsKey(dir.FullName)) return mod_last_update_timestamps[dir.FullName];
        var files = SystemUtils.SearchFileRecursive(dir.FullName, (filename) => !filename.StartsWith("."),
            dirname => !dirname.StartsWith(".") &&
                       !Paths.IgnoreSearchDirectories.Contains(dirname));
        var result = files.Select(filepath => new FileInfo(filepath))
            .Select(file_info =>
                Math.Max(file_info.CreationTimeUtc.Ticks, file_info.LastWriteTimeUtc.Ticks))
            .Prepend(Math.Max(dir.CreationTimeUtc.Ticks, dir.LastWriteTimeUtc.Ticks))
            .Prepend(InternalResourcesGetter.GetLastWriteTime())
            .Max();
        mod_last_update_timestamps[dir.FullName] = result;
        return result;
    }
}