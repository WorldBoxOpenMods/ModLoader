using System.Reflection;
using HarmonyLib;
using NeoModLoader.api;
using NeoModLoader.constants;
using NeoModLoader.General;
using NeoModLoader.ncms_compatible_layer;
using NeoModLoader.services;
using NeoModLoader.utils;
using UnityEngine;

namespace NeoModLoader;

public class WorldBoxMod : MonoBehaviour
{
    private bool initialized = false;
    public static List<IMod> LoadedMods = new();
    private void Start()
    {
        Others.unity_player_enabled = true;
        fileSystemInitialize();
    }
    private void Update()
    {
        if (initialized || !Config.gameLoaded) return;
        initialized = true;
        
        Harmony.CreateAndPatchAll(typeof(LM), Others.harmony_id);
        Harmony.CreateAndPatchAll(typeof(ResourcesPatch), Others.harmony_id);
        
        ResourcesPatch.Initialize();

        var mods = ModInfoUtils.findMods();

        var mod_nodes = ModDepenSolveService.SolveModDependencies(mods);

        ModCompileLoadService.prepareCompile(mod_nodes);

        var mods_to_load = new List<api.ModDeclare>();
        foreach (var mod in mod_nodes)
        {
            if (ModCompileLoadService.compileMod(mod))
            {
                mods_to_load.Add(mod.mod_decl);
                ResourcesPatch.LoadResourceFromMod(mod.mod_decl.FolderPath);
                LogService.LogInfo($"Successfully compile mod {mod.mod_decl.Name}");
            }
            else
            {
                LogService.LogError($"Failed to compile mod {mod.mod_decl.Name}");
            }
        }

        ModCompileLoadService.loadMods(mods_to_load);
        NCMSCompatibleLayer.Init();
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
        
        if (!File.Exists(Paths.ModCompileRecordPath))
        {
            File.Create(Paths.ModCompileRecordPath).Close();
            LogService.LogInfo($"Create mod_compile_records.json at {Paths.ModCompileRecordPath}");
        }

        if (!Directory.Exists(Paths.NMLAssembliesPath))
        {
            Directory.CreateDirectory(Paths.NMLAssembliesPath);
            LogService.LogInfo($"Create NMLAssemblies folder at {Paths.NMLAssembliesPath}");
        }
        
        foreach (var file_full_path in Directory.GetFiles(
                     Paths.NMLAssembliesPath, "*.dll"))
        {
            try
            {
                Assembly.LoadFrom(file_full_path);
                // LogService.LogInfo($"Load assembly {file_full_path} successfully.");
            }
            catch (BadImageFormatException)
            {
                LogService.LogError($"" +
                                    $"BadImageFormatException: " +
                                    $"The file {file_full_path} is not a valid assembly.");
            }
        }
    }
}