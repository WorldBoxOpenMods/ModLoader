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
using NeoModLoader.utils;
using UnityEngine;

namespace NeoModLoader;

public class WorldBoxMod : MonoBehaviour
{
    private bool initialized = false;
    private bool initialized_successfully = false;
    public static List<IMod> LoadedMods = new();
    internal static Transform Transform;
    internal static Assembly NeoModLoaderAssembly = Assembly.GetExecutingAssembly();
    private void Start()
    {
        Others.unity_player_enabled = true;
        Transform = transform;
        LogService.Init();
        fileSystemInitialize();
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
            ListenerManager._init();
            WrappedPowersTab._init();
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
            var mods_to_load = new List<api.ModDeclare>();
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
            foreach(var mod in mod_nodes)
            {
                SmoothLoader.add(() =>
                {
                    if (mods_to_load.Contains(mod.mod_decl))
                    {
                        ResourcesPatch.LoadResourceFromMod(mod.mod_decl.FolderPath);
                    }
                }, "Load Resources From Mod " + mod.mod_decl.Name);
            }
            SmoothLoader.add(() =>
            {
                ModCompileLoadService.loadMods(mods_to_load);
                NCMSCompatibleLayer.Init();
            }, "Load Mods");
        
            SmoothLoader.add(() =>
            {
                ModWorkshopService.Init();
        
                ui.UIManager.init();

                NMLAutoUpdateService.CheckWorkshopUpdate();
                ModInfoUtils.DealWithBepInExModLinkRequests();

                initialized_successfully = true;
            }, "NeoModLoader Post Initialize");
        }, "Compile Mods And Load resources");

    }

    private void LoadLocales()
    {
        string[] resources = NeoModLoaderAssembly.GetManifestResourceNames();
        string locale_path = "NeoModLoader.resources.locales.";
        foreach (string resource_path in resources)
        {
            if(!resource_path.StartsWith(locale_path)) continue;
            
            LM.LoadLocale(resource_path.Replace(locale_path, "").Replace(".json", ""), NeoModLoaderAssembly.GetManifestResourceStream(resource_path));
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

        void extractAssemblies()
        {
            var resources = NeoModLoaderAssembly.GetManifestResourceNames();
            foreach (var resource in resources)
            {
                if (resource.EndsWith(".dll"))
                {
                    var file_name = resource.Replace("NeoModLoader.resources.assemblies.", "");
                    var file_path = Path.Combine(Paths.NMLAssembliesPath, file_name);
                
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
            if(modupdate_time > assemblyupdate_time)
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

        foreach (var file_full_path in Directory.GetFiles(Paths.NMLAssembliesPath, "*.dll"))
        {
            try {
                Assembly.LoadFrom(file_full_path);
                // LogService.LogInfo($"Load assembly {file_full_path} successfully.");
            } catch (BadImageFormatException) {
                LogService.LogError($"" +
                                    $"BadImageFormatException: " +
                                    $"The file {file_full_path} is not a valid assembly.");
            } catch (FileNotFoundException e) {
                LogService.LogError($"FileNotFoundException: " +
                                    $"The file {file_full_path} is not found.");
                LogService.LogError(e.Message);
                LogService.LogError(e.StackTrace);
            } catch (Exception e) {
                LogService.LogError($"Exception: " +
                                    $"Failed to load assembly {file_full_path}.");
                LogService.LogError(e.Message);
                LogService.LogError(e.StackTrace);
            }
        }
    }
}