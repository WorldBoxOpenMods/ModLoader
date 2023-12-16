extern alias unixsteamwork;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using MonoMod.Utils;
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

    private static HashSet<string> mods_disabled = null;

    private static readonly Dictionary<string, long> mod_compile_timestamps = new();

    private static readonly JsonSerializerSettings mod_compile_timestamps_serializer_settings = new()
    {
        ContractResolver = new DefaultContractResolver()
    };

    public static List<ModDeclare> findAndPrepareMods()
    {
        HashSet<string> findModsIDs = new();
        var mods = new List<ModDeclare>();
        if (!NCMSHere())
        {
            var zipped_mods = new HashSet<string>(Directory.GetFiles(Paths.ModsPath, "*.zip"))
                .Union(Directory.GetFiles(Paths.ModsPath, "*.7z"))
                .Union(Directory.GetFiles(Paths.ModsPath, "*.rar"))
                .Union(Directory.GetFiles(Paths.ModsPath, "*.tar"))
                .Union(Directory.GetFiles(Paths.ModsPath, "*.tar.gz"))
                .Union(Directory.GetFiles(Paths.ModsPath, "*.mod"));
            foreach (var zipped_mod in zipped_mods)
            {
                string extract_path = Path.Combine(Application.temporaryCachePath,
                    Path.GetFileNameWithoutExtension(zipped_mod));
                if (Directory.Exists(extract_path))
                {
                    Directory.Delete(extract_path, true);
                }

                ZipFile.ExtractToDirectory(zipped_mod, extract_path);
                var mod_json_files = SystemUtils.SearchFileRecursive(extract_path,
                    (filename) => filename == Paths.ModDeclarationFileName, (dirname) => true);
                if (mod_json_files.Count == 0)
                {
                    Directory.Delete(extract_path, true);
                    continue;
                }

                if (mod_json_files.Count > 1)
                {
                    LogService.LogWarning($"More than one mod.json file in {zipped_mod}, only load the first one");
                }

                try
                {
                    Directory.Move(Path.GetDirectoryName(mod_json_files[0]),
                        Path.Combine(Paths.ModsPath, Path.GetFileNameWithoutExtension(zipped_mod)));
                }
                catch (UnauthorizedAccessException)
                {
                    ZipFile.ExtractToDirectory(zipped_mod,
                        Path.Combine(Paths.ModsPath, Path.GetFileNameWithoutExtension(zipped_mod)));
                }
                finally
                {
                    try
                    {
                        File.Delete(zipped_mod);
                        if (Directory.Exists(extract_path))
                            Directory.Delete(extract_path, true);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }

            var mod_folders = Directory.GetDirectories(Paths.ModsPath);
            foreach (var mod_folder in mod_folders)
            {
                var mod = recogMod(mod_folder);
                if (mod != null)
                {
                    if (findModsIDs.Contains(mod.UID))
                    {
                        LogService.LogWarning($"Repeat Mod with {mod.UID}, Only load one of them");
                        continue;
                    }

                    mods.Add(mod);
                    findModsIDs.Add(mod.UID);
                }
            }
        }

        bool NCMSHere()
        {
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
                if (mod.ModType == ModTypeEnum.NORMAL)
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
            WorldBoxMod.AllRecognizedMods.Add(mod, ModState.FAILED);
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
        InformationWindow.ShowWindow(LM.Get("ModLinkRequest"));
        new Task(() =>
        {
            Task.Delay(15000);
            if (to_install_bepinex)
            {
                try
                {
                    InstallBepInEx();
                }
                catch (Exception e)
                {
                    LogService.LogErrorConcurrent(e.Message);
                    LogService.LogErrorConcurrent(e.StackTrace);
                    return;
                }

                to_install_bepinex = false;
            }

            if (!Directory.Exists(Paths.BepInExPluginsPath))
            {
                Directory.CreateDirectory(Paths.BepInExPluginsPath);
            }

            List<string> parameters = new List<string>();
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
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
                    break;
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.OSXPlayer:
                    parameters.Add("-c");
                    while (link_request_mods.Count > 0)
                    {
                        var mod = link_request_mods.Dequeue();
                        if (parameters.Count != 1)
                        {
                            parameters.Add("&&");
                        }

                        parameters.Add("ln");
                        parameters.Add("-s");
                        parameters.Add($"\"{mod.FolderPath}\"");
                        parameters.Add($"\"{Path.Combine(Paths.BepInExPluginsPath, mod.Name)}\"");
                    }

                    SystemUtils.BashRun(parameters.ToArray());
                    break;
            }
        }).Start();
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
        catch (Exception e)
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
            LogService.LogWarning($"Failed to load mod {mod.Name} which is a BepInEx mod, but BepInEx not found");
            LogService.LogInfo($"Add Install BepInEx Task into queue");
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
            if (pLogModJsonNotFound)
                LogService.LogWarning($"No mod.json file for folder {pModFolderPath} in Mods");
            return null;
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
            if (bepinex_plugin_file_locs.Contains(assembly.Location))
            {
                string folder_path = Path.GetDirectoryName(assembly.Location);
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
        if (mods_disabled == null)
        {
            mods_disabled = new(File.ReadAllLines(Paths.ModsDisabledRecordPath));
        }

        return mods_disabled.Contains(pModUID);
    }

    /// <summary>
    /// Toggle Mod Disabled Status
    /// </summary>
    /// <param name="pModUID"></param>
    /// <returns>Enable mod and return true; or disable mod and return false</returns>
    public static bool toggleMod(string pModUID)
    {
        bool result = mods_disabled.Contains(pModUID);
        if (result)
        {
            mods_disabled.Remove(pModUID);
        }
        else
        {
            mods_disabled.Add(pModUID);
        }

        File.WriteAllLines(Paths.ModsDisabledRecordPath, mods_disabled.ToArray());
        return result;
    }

    // ReSharper disable once InconsistentNaming
    public static bool isModNeedRecompile(string pModUUID, string pModFolderPath)
    {
        return getModLastCompileTimestamp(pModUUID) <
               Others.confirmed_compile_time + getModNewestUpdateTimestamp(pModFolderPath);
    }

    public static void updateModCompileTimestamp(string pModUUID)
    {
        mod_compile_timestamps[pModUUID] = DateTime.UtcNow.Ticks;

        File.WriteAllText(Paths.ModCompileRecordPath,
            JsonConvert.SerializeObject(mod_compile_timestamps, mod_compile_timestamps_serializer_settings));
    }

    public static void clearModCompileTimestamp(string pModUUID)
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