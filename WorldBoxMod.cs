using System.Reflection;
using HarmonyLib;
using NeoModLoader.api;
using NeoModLoader.constants;
using NeoModLoader.General;
using NeoModLoader.General.Event;
using NeoModLoader.General.UI.Prefabs;
using NeoModLoader.General.UI.Tab;
using NeoModLoader.ncms_compatible_layer;
using NeoModLoader.services;
using NeoModLoader.ui;
using NeoModLoader.utils;
using UnityEngine;

namespace NeoModLoader;

public class WorldBoxMod : MonoBehaviour
{
    public static List<IMod> LoadedMods = new();
    internal static Dictionary<ModDeclare, ModState> AllRecognizedMods = new();
    internal static Transform Transform;
    internal static Transform InactiveTransform;
    internal static Assembly NeoModLoaderAssembly = Assembly.GetExecutingAssembly();
    private bool initialized = false;
    private bool initialized_successfully = false;

    private void Start()
    {
        Others.unity_player_enabled = true;
        Transform = transform;

        InactiveTransform = new GameObject("Inactive").transform;
        InactiveTransform.SetParent(Transform);
        InactiveTransform.gameObject.SetActive(false);

        LogService.Init();
        fileSystemInitialize();
        LogService.LogInfo($"NeoModLoader Version: {InternalResourcesGetter.GetCommit()}");
    }

    private void Update()
    {
        if (!Config.gameLoaded) return;
        if (initialized_successfully)
        {
            TabManager._checkNewTabs();
        }

        if (initialized)
        {
            return;
        }

        initialized = true;
        ModUploadAuthenticationService.AutoAuth();
        Harmony.CreateAndPatchAll(typeof(LM), Others.harmony_id);
        Harmony.CreateAndPatchAll(typeof(ResourcesPatch), Others.harmony_id);
        float time = 0;

        SmoothLoader.add(() =>
        {
            ResourcesPatch.Initialize();
            LoadLocales();
            LM.ApplyLocale();
            PrefabManager._init();
            TabManager._init();
            WindowCreator.init();
            ListenerManager._init();
            WrappedPowersTab._init();
            NCMSCompatibleLayer.PreInit();
        }, "Initialize NeoModLoader");

        List<ModDependencyNode> mod_nodes = new();
        SmoothLoader.add(() =>
        {
            ModCompileLoadService.loadInfoOfBepInExPlugins();

            var mods = ModInfoUtils.findAndPrepareMods();

            mod_nodes.AddRange(ModDepenSolveService.SolveModDependencies(mods));

            ModCompileLoadService.prepareCompile(mod_nodes);
        }, "Load Mods Info And Prepare Mods");


        SmoothLoader.add(() =>
        {
            var mods_to_load = new List<ModDeclare>();
            foreach (var mod in mod_nodes)
            {
                SmoothLoader.add(() =>
                {
                    if (ModCompileLoadService.compileMod(mod))
                    {
                        mods_to_load.Add(mod.mod_decl);
                    }
                    else
                    {
                        LogService.LogError($"Failed to compile mod {mod.mod_decl.Name}");
                    }
                }, "Compile Mod " + mod.mod_decl.Name);
            }

            foreach (var mod in mod_nodes)
            {
                SmoothLoader.add(() =>
                {
                    if (mods_to_load.Contains(mod.mod_decl))
                    {
                        ResourcesPatch.LoadResourceFromFolder(Path.Combine(mod.mod_decl.FolderPath,
                            Paths.ModResourceFolderName));
                        ResourcesPatch.LoadResourceFromFolder(Path.Combine(mod.mod_decl.FolderPath,
                            Paths.NCMSAdditionModResourceFolderName));
                        ResourcesPatch.LoadAssetBundlesFromFolder(Path.Combine(mod.mod_decl.FolderPath,
                            Paths.ModAssetBundleFolderName));
                    }
                }, "Load Resources From Mod " + mod.mod_decl.Name);
            }

            SmoothLoader.add(() =>
            {
                ModCompileLoadService.loadMods(mods_to_load);
                NCMSCompatibleLayer.Init();
            }, "Load Mods");

            SmoothLoader.add(ResourcesPatch.PatchSomeResources, "Patch part of Resources into game");
            SmoothLoader.add(() =>
            {
                ModWorkshopService.Init();

                UIManager.init();

                ModInfoUtils.DealWithBepInExModLinkRequests();

                initialized_successfully = true;

                try
                {
                    if (!SteamSDK.shouldQuit)
                    {
                        NMLAutoUpdateService.CheckWorkshopUpdate();
                    }
                    else
                    {
                        NMLAutoUpdateService.CheckUpdate();
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    ErrorWindow.errorMessage = LM.Get("FailedAutoUpdate");
                    ScrollWindow.get("error_with_reason").clickShow();
                }
            }, "NeoModLoader Post Initialize");
        }, "Compile Mods And Load resources");
    }

    private void LoadLocales()
    {
        string[] resources = NeoModLoaderAssembly.GetManifestResourceNames();
        string locale_path = "NeoModLoader.resources.locales.";
        foreach (string resource_path in resources)
        {
            if (!resource_path.StartsWith(locale_path)) continue;

            LM.LoadLocale(resource_path.Replace(locale_path, "").Replace(".json", ""),
                NeoModLoaderAssembly.GetManifestResourceStream(resource_path));
        }
    }

    private void fileSystemInitialize()
    {
        if (!Directory.Exists(Paths.ModsPath))
        {
            Directory.CreateDirectory(Paths.ModsPath);
            LogService.LogInfo($"Create Mods folder at {Paths.ModsPath}");
        }

        if (!Directory.Exists(Paths.CompiledModsPath))
        {
            Directory.CreateDirectory(Paths.CompiledModsPath);
            LogService.LogInfo($"Create CompiledMods folder at {Paths.CompiledModsPath}");
        }

        if (!Directory.Exists(Paths.ModsConfigPath))
        {
            Directory.CreateDirectory(Paths.ModsConfigPath);
            LogService.LogInfo($"Create mods_config folder at {Paths.ModsConfigPath}");
        }

        if (!File.Exists(Paths.ModCompileRecordPath))
        {
            File.Create(Paths.ModCompileRecordPath).Close();
            LogService.LogInfo($"Create mod_compile_records.json at {Paths.ModCompileRecordPath}");
        }

        if (!File.Exists(Paths.ModsDisabledRecordPath))
        {
            File.Create(Paths.ModsDisabledRecordPath).Close();
            LogService.LogInfo($"Create mod_compile_records.json at {Paths.ModsDisabledRecordPath}");
        }

        void extractAssemblies()
        {
            var resources = NeoModLoaderAssembly.GetManifestResourceNames();
            foreach (var resource in resources)
            {
                if (resource.EndsWith(".dll"))
                {
                    var file_name = resource.Replace("NeoModLoader.resources.assemblies.", "");
                    var file_path = Path.Combine(Paths.NMLAssembliesPath, file_name).Replace("-renamed", "");

                    using var stream = NeoModLoaderAssembly.GetManifestResourceStream(resource);
                    using var file = new FileStream(file_path, FileMode.Create, FileAccess.Write);
                    stream.CopyTo(file);
                    // LogService.LogInfo($"Extract {file_name} to {file_path}");
                }
            }
        }

        if (!Directory.Exists(Paths.NMLAssembliesPath))
        {
            Directory.CreateDirectory(Paths.NMLAssembliesPath);
            LogService.LogInfo($"Create NMLAssemblies folder at {Paths.NMLAssembliesPath}");
            extractAssemblies();
        }
        else
        {
            var modupdate_time = new FileInfo(Paths.NMLModPath).LastWriteTime;
            var assemblyupdate_time = new DirectoryInfo(Paths.NMLAssembliesPath).CreationTime;
            if (modupdate_time > assemblyupdate_time)
            {
                LogService.LogInfo($"NeoModLoader.dll is newer than assemblies in NMLAssemblies folder, " +
                                   $"re-extract assemblies from NeoModLoader.dll");
                Debug.Log(Paths.NMLAssembliesPath);
                Directory.Delete(Paths.NMLAssembliesPath, true);
                Directory.CreateDirectory(Paths.NMLAssembliesPath);
                LogService.LogInfo($"Create new NMLAssemblies folder at {Paths.NMLAssembliesPath}");
                extractAssemblies();
            }
        }

        try
        {
            using var stream =
                NeoModLoaderAssembly.GetManifestResourceStream(
                    "NeoModLoader.resources.assemblies.Assembly-CSharp-Publicized");
            if (File.Exists(Paths.PublicizedAssemblyPath))
            {
                var modupdate_time = new FileInfo(Paths.NMLModPath).LastWriteTime;
                var assemblyupdate_time = new FileInfo(Paths.PublicizedAssemblyPath).CreationTime;
                if (modupdate_time > assemblyupdate_time)
                {
                    LogService.LogInfo($"NeoModLoader.dll is newer than Assembly-CSharp-Publicized.dll, " +
                                       $"re-extract Assembly-CSharp-Publicized.dll from NeoModLoader.dll");
                    File.Delete(Paths.PublicizedAssemblyPath);
                    using var file = new FileStream(Paths.PublicizedAssemblyPath, FileMode.Create, FileAccess.Write);
                    stream.CopyTo(file);
                }
            }
            else
            {
                using var file = new FileStream(Paths.PublicizedAssemblyPath, FileMode.CreateNew, FileAccess.Write);
                stream.CopyTo(file);
            }
        }
        catch (UnauthorizedAccessException) // If the file is hidden, delete it and try again
        {
            File.Delete(Paths.PublicizedAssemblyPath);
            using var stream =
                NeoModLoaderAssembly.GetManifestResourceStream(
                    "NeoModLoader.resources.assemblies.Assembly-CSharp-Publicized");
            using var file = new FileStream(Paths.PublicizedAssemblyPath, FileMode.CreateNew, FileAccess.Write);
            stream.CopyTo(file);
        }

        foreach (var file_full_path in Directory.GetFiles(Paths.NMLAssembliesPath, "*.dll"))
        {
            try
            {
                Assembly.LoadFrom(file_full_path);
                // LogService.LogInfo($"Load assembly {file_full_path} successfully.");
            }
            catch (BadImageFormatException)
            {
                switch (Path.GetFileName(file_full_path))
                {
                    case "System.IO.Compression.FileSystem.dll":
                        // Just because BepInEx not installed
                        continue;
                }

                LogService.LogError($"" +
                                    $"BadImageFormatException: " +
                                    $"The file {file_full_path} is not a valid assembly.");
            }
            catch (FileNotFoundException e)
            {
                LogService.LogError($"FileNotFoundException: " +
                                    $"The file {file_full_path} is not found.");
                LogService.LogError(e.Message);
                LogService.LogError(e.StackTrace);
            }
            catch (Exception e)
            {
                LogService.LogError($"Exception: " +
                                    $"Failed to load assembly {file_full_path}.");
                LogService.LogError(e.Message);
                LogService.LogError(e.StackTrace);
            }
        }
    }
}