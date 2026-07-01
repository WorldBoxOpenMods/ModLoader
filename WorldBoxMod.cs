using System.Reflection;
using HarmonyLib;
using NeoModLoader.api;
using NeoModLoader.constants;
using NeoModLoader.General;
using NeoModLoader.General.UI.Tab;
using NeoModLoader.ncms_compatible_layer;
using NeoModLoader.services;
using NeoModLoader.ui;
using NeoModLoader.utils;
using UnityEngine;

namespace NeoModLoader;

/// <summary>
/// Main class
/// </summary>
public class WorldBoxMod : MonoBehaviour
{
    /// <summary>
    /// All successfully loaded mods.
    /// </summary>
    public static List<IMod> LoadedMods = new();

    /// <summary>
    /// Tries to get a loaded mod by declaration UID.
    /// </summary>
    /// <param name="pModDeclare">The target mod declaration.</param>
    /// <param name="pLoadedMod">The loaded mod instance when found.</param>
    /// <returns><see langword="true"/> when the mod is loaded; otherwise <see langword="false"/>.</returns>
    public static bool TryGetLoadedMod(ModDeclare pModDeclare, out IMod pLoadedMod)
    {
        if (pModDeclare != null)
        {
            foreach (var mod in LoadedMods)
            {
                if (mod.GetDeclaration().UID == pModDeclare.UID)
                {
                    pLoadedMod = mod;
                    return true;
                }
            }
        }

        pLoadedMod = null;
        return false;
    }

    internal static Dictionary<ModDeclare, ModState> AllRecognizedMods = new();
    internal static Transform Transform;
    internal static Transform InactiveTransform;
    internal static Assembly NeoModLoaderAssembly = Assembly.GetExecutingAssembly();
    private bool initialized = false;
    private bool initialized_successfully = false;

    private static void UnityExplorerFix() {
        Harmony harmony = new Harmony(Others.harmony_id);
        MethodInfo original = AccessTools.Method(typeof(Assembly), nameof(Assembly.LoadFrom), new[] { typeof(string) });
        MethodInfo standin = AccessTools.Method(typeof(WorldBoxMod), nameof(LoadFrom));
        ReversePatcher reversePatcher = harmony.CreateReversePatcher(original, new HarmonyMethod(standin));

        reversePatcher.Patch();
    }

    private static Assembly LoadFrom(string path) => Assembly.LoadFrom(path);

    private void Start()
    {
        Others.unity_player_enabled = true;
        Transform = transform;

        InactiveTransform = new GameObject("Inactive").transform;
        InactiveTransform.SetParent(Transform);
        InactiveTransform.gameObject.SetActive(false);

        LogService.Init();

        if (ReflectionHelper.IsAssemblyLoaded("0Harmony")) {
            UnityExplorerFix();
        }

        fileSystemInitialize();
        LogService.LogInfo($"NeoModLoader Version: {InternalResourcesGetter.GetCommit()}");
    }

    private void Update()
    {
        if (!Config.game_loaded) return;
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
        HarmonyUtils._init();
        Harmony.CreateAndPatchAll(typeof(LM), Others.harmony_id);
        Harmony.CreateAndPatchAll(typeof(ResourcesPatch), Others.harmony_id);
        Harmony.CreateAndPatchAll(typeof(CustomAudioManager), Others.harmony_id);
        if (!SmoothLoader.isLoading()) SmoothLoader.prepare();

        SmoothLoader.add(() =>
        {
            ResourcesPatch.Initialize();
            LoadLocales();
            LM.ApplyLocale();
            TabManager._init();
            WindowCreator.init();
            WrappedPowersTab._init();
            NCMSCompatibleLayer.PreInit();
            ModInfoUtils.InitializeModCompileCache();
        }, "Initialize NeoModLoader");

        List<ModDependencyNode> mod_nodes = new();
        HashSet<string> compiled_mod_uids = new();
        ModEnablePlan startup_enable_plan = null;
        SmoothLoader.add(() =>
        {
            ModCompileLoadService.loadInfoOfBepInExPlugins();

            ModInfoUtils.findAndPrepareMods();
            ModDepenSolveService.InitializeGraph(AllRecognizedMods.Keys);
            startup_enable_plan = ModDepenSolveService.BuildStartupEnablePlan();
            mod_nodes.AddRange(startup_enable_plan.LoadOrder);
        }, "Load Mods Info And Prepare Mods");
        SmoothLoader.add(() =>
        {
            compiled_mod_uids = ModCompileLoadService.CompileModNodes(mod_nodes);
            AssetLinker Linker = new();
            foreach (var mod in mod_nodes)
            {
                SmoothLoader.add(() =>
                {
                    if (!mod.Loaded && compiled_mod_uids.Contains(mod.mod_decl.UID))
                    {
                        ResourcesPatch.LoadResourceFromFolder(Path.Combine(mod.mod_decl.FolderPath,
                            Paths.ModResourceFolderName), Linker);
                        ResourcesPatch.LoadResourceFromFolder(Path.Combine(mod.mod_decl.FolderPath,
                            Paths.NCMSAdditionModResourceFolderName), Linker);
                        ResourcesPatch.LoadAssetBundlesFromFolder(Path.Combine(mod.mod_decl.FolderPath,
                            Paths.ModAssetBundleFolderName));
                    }
                }, "Load Resources From Mod " + mod.mod_decl.Name);
            }

            SmoothLoader.add(() =>
            {
                ModCompileLoadService.loadMods(mod_nodes
                    .Where(mod => !mod.Loaded && compiled_mod_uids.Contains(mod.mod_decl.UID))
                    .Select(mod => mod.mod_decl)
                    .ToList());
                Linker.AddAssets();
                if (startup_enable_plan != null)
                {
                    if (startup_enable_plan.RequestedRoots.All(ModCompileLoadService.IsModLoaded))
                    {
                        ModDepenSolveService.CommitEnablePlan(startup_enable_plan);
                    }
                    else
                    {
                        ModDepenSolveService.RollbackEnablePlan(startup_enable_plan);
                    }
                }

                ModInfoUtils.SaveModRecords();
                NCMSCompatibleLayer.Init();
                var successfulInit = new Dictionary<IMod, bool>();
                foreach (IMod mod in LoadedMods.Where(mod => mod is IStagedLoad))
                {
                    SmoothLoader.add(() =>
                    {
                        successfulInit.Add(mod, ModCompileLoadService.TryInitMod(mod));
                    }, "Init Mod " + mod.GetDeclaration().Name);
                }
                foreach (IMod mod in LoadedMods.Where(mod => mod is IStagedLoad))
                {
                    SmoothLoader.add(() =>
                    {
                        if (successfulInit.ContainsKey(mod) && successfulInit[mod])
                        {
                            ModCompileLoadService.PostInitMod(mod);
                        }
                    }, "Post-Init Mod " + mod.GetDeclaration().Name);
                }
            }, "Load Mods");

            SmoothLoader.add(() =>
            {
                ModWorkshopService.Init();

                UIManager.init();

                ModInfoUtils.DealWithBepInExModLinkRequests();

                LM.ApplyLocale();
                initialized_successfully = true;
            }, "NeoModLoader Post Initialize");
            SmoothLoader.add(ExternalModInstallService.CheckExternalModInstall, "Check External Mods to Install");
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

        void extractAssemblies()
        {
            var resources = NeoModLoaderAssembly.GetManifestResourceNames();
            foreach (var resource in resources)
            {
                if (resource.EndsWith(".dll"))
                {
                    if (resource.Contains("Assembly-CSharp-Publicized")) continue;
                    if (resource.Contains("AutoUpdate")) continue;
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
                    "NeoModLoader.resources.assemblies.Assembly-CSharp-Publicized.dll");
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
                    "NeoModLoader.resources.assemblies.Assembly-CSharp-Publicized.dll");
            using var file = new FileStream(Paths.PublicizedAssemblyPath, FileMode.CreateNew, FileAccess.Write);
            stream.CopyTo(file);
        }

        foreach (var file_full_path in Directory.GetFiles(Paths.NMLAssembliesPath, "*.dll"))
        {
            try
            {
                string assembly_name = AssemblyName.GetAssemblyName(file_full_path).Name;
                if (AppDomain.CurrentDomain.GetAssemblies()
                    .Any(assembly => string.Equals(
                        assembly.GetName().Name,
                        assembly_name,
                        StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                LoadFrom(file_full_path);
            }
            catch (BadImageFormatException)
            {
                LogService.LogError($"" +
                                    $"BadImageFormatException: " +
                                    $"The file {file_full_path} is not a valid assembly.");
            }
            catch (Exception e)
            {
                LogService.LogError($"Exception: " +
                                    $"Failed to load assembly {file_full_path}.");
                LogService.LogError(e.Message);
                LogService.LogError(e.StackTrace);
            }
        }

        File.WriteAllText(Paths.NMLCommitPath, InternalResourcesGetter.GetCommit());
        if (File.Exists(Paths.NMLAutoUpdateModulePath))
        {
            FileInfo file = new(Paths.NMLAutoUpdateModulePath);
            if (file.LastWriteTimeUtc.Ticks < InternalResourcesGetter.GetLastWriteTime())
                try
                {
                    file.Delete();
                    LogService.LogInfo($"NeoModLoader.dll is newer than AutoUpdate.dll, " +
                                       $"re-extract AutoUpdate.dll from NeoModLoader.dll");
                }
                catch (Exception e)
                {
                    // ignored
                }
        }

        if (!File.Exists(Paths.NMLAutoUpdateModulePath))
        {
            using Stream stream = NeoModLoaderAssembly.GetManifestResourceStream(
                "NeoModLoader.resources.assemblies.NeoModLoader.AutoUpdate.dll");
            using var file = new FileStream(Paths.NMLAutoUpdateModulePath, FileMode.CreateNew, FileAccess.Write);
            stream.CopyTo(file);
        }
    }
}
