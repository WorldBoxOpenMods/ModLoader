using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using ModDeclaration;
using NCMS;
using NeoModLoader.api;
using NeoModLoader.constants;
using NeoModLoader.General;
using NeoModLoader.ncms_compatible_layer;
using NeoModLoader.utils;
using NeoModLoader.utils.Builders;
using UnityEngine;

namespace NeoModLoader.services;

/// <summary>
/// Service of mod compiling and loading. 
/// </summary>
public static class ModCompileLoadService
{
    private sealed class CompileSessionContext
    {
        public CompileSessionContext(MetadataReference[] pDefaultReferences, MetadataReference pPublicizedAssemblyReference)
        {
            DefaultReferences = pDefaultReferences;
            PublicizedAssemblyReference = pPublicizedAssemblyReference;
        }

        public MetadataReference[] DefaultReferences { get; }
        public MetadataReference PublicizedAssemblyReference { get; }
        public ConcurrentDictionary<string, MetadataReference> ModReferences { get; } = new();
        public ConcurrentDictionary<string, byte> LoadedAdditionalReferences { get; } = new();
    }

    private sealed class CompileNodeResult
    {
        public ModDependencyNode Node { get; set; } = null!;
        public bool Success { get; set; }
        public string FailureReason { get; set; } = string.Empty;
        public string CompileErrors { get; set; } = string.Empty;
        public string OptionalDependencyCompileErrors { get; set; } = string.Empty;
        public bool UsedOptionalDependencyFallback { get; set; }
        public bool ShouldRecordCompileCache { get; set; }
        public MetadataReference ProducedReference { get; set; }
        public IReadOnlyList<string> AvailableDependencies { get; set; } = Array.Empty<string>();
        public IReadOnlyList<string> AvailableOptionalDependencies { get; set; } = Array.Empty<string>();
    }


    private static string CollectCompileErrors(IEnumerable<Diagnostic> pDiagnostics)
    {
        StringBuilder diags = new StringBuilder();
        foreach (var diagnostic in pDiagnostics)
        {
            if (diagnostic.Severity != DiagnosticSeverity.Error) continue;
            diags.AppendLine(diagnostic.ToString());
        }

        return diags.ToString().TrimEnd();
    }

    private static void LogCompileFailure(string pModUid, string pCompileErrors)
    {
        if (string.IsNullOrWhiteSpace(pCompileErrors))
        {
            LogService.LogError($"Failed to compile mod {pModUid}");
            return;
        }

        LogService.LogError($"Failed to compile mod {pModUid}:\n{pCompileErrors}");
    }

    private static void LogCompileFailureWithOptionalDependencies(string pModUid, string pCompileErrors)
    {
        if (string.IsNullOrWhiteSpace(pCompileErrors))
        {
            LogService.LogWarning($"Failed to compile mod {pModUid} with optional dependencies, but succeeded after disabling them");
            return;
        }

        LogService.LogWarning(
            $"Failed to compile mod {pModUid} with optional dependencies, but succeeded after disabling them:\n{pCompileErrors}");
    }

    private static string BuildFailedDependencyCompileMessage(ModDeclare pModDeclare,
        IEnumerable<string> pFailedDependencies)
    {
        StringBuilder sb = new();
        sb.AppendLine($"Compile skipped for mod {pModDeclare.UID} because required dependencies failed to compile:");
        foreach (string dependency_uid in pFailedDependencies.Distinct())
        {
            sb.AppendLine($"    {dependency_uid}");
        }

        return sb.ToString().TrimEnd();
    }

    internal static void LoadLocales(object pModComponent, ModDeclare pModDeclare, bool pUpdateTexts = true,
        bool pLogLoadedFiles = false)
    {
        if (pModComponent is not ILocalizable localizable_mod)
            return;

        string locale_path = localizable_mod.GetLocaleFilesDirectory(pModDeclare);
        if (!Directory.Exists(locale_path)) return;

        char csv_separator = ',';
        if (pModComponent is ICsvSepCustomized sep_customized)
            csv_separator = sep_customized.GetCsvSeparator();

        var files = Directory.GetFiles(locale_path, "*", SearchOption.AllDirectories);
        foreach (var locale_file in files)
        {
            if (pLogLoadedFiles)
            {
                LogService.LogInfo(
                    $"Reload {locale_file} as {Path.GetFileNameWithoutExtension(locale_file)}");
            }

            try
            {
                if (locale_file.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    LM.LoadLocale(Path.GetFileNameWithoutExtension(locale_file), locale_file);
                }
                else if (locale_file.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    LM.LoadLocales(locale_file, csv_separator);
                }
            }
            catch (FormatException e)
            {
                LogService.LogWarning(e.Message);
            }
        }

        LM.ApplyLocale(pUpdateTexts);
    }

    /// <summary>
    /// Load a list of mods
    /// </summary>
    /// <param name="mods_to_load"></param>
    public static void loadMods(List<ModDeclare> mods_to_load)
    {
        // It can be sure that all mods are compiled successfully.
        foreach (var mod in mods_to_load)
        {
            try
            {
                LoadMod(mod);
            }
            catch (ReflectionTypeLoadException e)
            {
                LogService.LogError(
                    $"Compiled mod {mod.UID} out of date, if it happens again after restarting game, please update, delete or unsubscribe it");
                LogService.LogException(e);

                string dll_path = Path.Combine(Paths.CompiledModsPath, $"{mod.UID}.dll");
                string pdb_path = Path.Combine(Paths.CompiledModsPath, $"{mod.UID}.pdb");
                try
                {
                    if (File.Exists(dll_path)) File.Delete(dll_path);

                    if (File.Exists(pdb_path)) File.Delete(pdb_path);
                }
                catch (Exception)
                {
                    // ignored
                }

                ModInfoUtils.clearModCompileTimestamp(mod.UID);
            }
        }
    }

    /// <summary>
    /// Load a single mod
    /// </summary>
    /// <param name="pMod"></param>
    public static void LoadMod(ModDeclare pMod)
    {
        Assembly[] mod_assemblies;
        switch (pMod.ModType)
        {
            case ModTypeEnum.NEOMOD:
                mod_assemblies = new[]
                {
                    Assembly.Load(
                        File.ReadAllBytes(Path.Combine(Paths.CompiledModsPath,
                            $"{pMod.UID}.dll")),
                        File.ReadAllBytes(Path.Combine(Paths.CompiledModsPath, $"{pMod.UID}.pdb"))
                    )
                };
                break;
            case ModTypeEnum.COMPILED_NEOMOD:
                var dll_files = Directory.GetFiles(pMod.FolderPath, "*.dll");
                List<string> pdb_files = Directory.GetFiles(pMod.FolderPath, "*.pdb").ToList();
                mod_assemblies = new Assembly[dll_files.Length];
                for (int i = 0; i < dll_files.Length; i++)
                {
                    string dll_file_name = Path.GetFileName(dll_files[i]).Replace(".dll", "");
                    int index = pdb_files.IndexOf(Path.Combine(pMod.FolderPath, $"{dll_file_name}.pdb"));
                    if (index != -1)
                    {
                        mod_assemblies[i] = Assembly.Load(
                            File.ReadAllBytes(dll_files[i]),
                            File.ReadAllBytes(pdb_files[index])
                        );
                        pdb_files.RemoveAt(index);
                    }
                    else
                    {
                        mod_assemblies[i] = Assembly.Load(File.ReadAllBytes(dll_files[i]));
                    }
                }

                break;
            case ModTypeEnum.BEPINEX:
            case ModTypeEnum.RESOURCE_PACK:
            default:
                throw new ArgumentException("Cannot load mod of type " + pMod.ModType + " with NML!");
        }
        bool all_success = true;
        foreach (var mod_assembly in mod_assemblies)
        {
            GameObject mod_instance;
            bool any_loaded = false;
            foreach (var type in mod_assembly.GetTypes())
            {
                var mod_entry = Attribute.GetCustomAttribute(type, typeof(ModEntry));
                if (!type.IsSubclassOf(typeof(MonoBehaviour)) ||
                    (type.GetInterface(nameof(IMod)) == null && mod_entry == null) || type.IsAbstract) continue;


                mod_instance = new GameObject(pMod.Name)
                {
                    transform =
                    {
                        parent = GameObject.Find("Services/ModLoader").transform
                    }
                };
                mod_instance.SetActive(false);

                if (mod_entry != null)
                {
                    pMod.IsNCMSMod = true;
                    Type ncmsGlobalObjectType = mod_assembly.GetType("Mod");
                    ncmsGlobalObjectType.GetField("Info")
                        ?.SetValue(null, new Info(NCMSCompatibleLayer.GenerateNCMSMod(pMod)));
                    ncmsGlobalObjectType.GetField("GameObject")?.SetValue(null, mod_instance);
                }

                IMod mod_interface = null;
                try
                {
                    MonoBehaviour main_component = null;
                    if (type.GetInterface(nameof(IMod)) == null)
                    {
                        mod_interface = mod_instance.AddComponent<AttachedModComponent>();
                        main_component = (MonoBehaviour)mod_instance.AddComponent(type);
                    }
                    else
                    {
                        mod_interface = (IMod)mod_instance.AddComponent(type);
                        main_component = (MonoBehaviour)mod_interface;
                    }
                    LoadLocales(main_component, pMod, false);

                    mod_interface.OnLoad(pMod, mod_instance);
                    mod_instance.SetActive(true);
                    WorldBoxMod.LoadedMods.Add(mod_instance.GetComponent<IMod>());
                    any_loaded = true;
                    break;
                }
                catch (Exception e)
                {
                    LogService.LogError(e.Message);
                    if (e.StackTrace != null) LogService.LogError(e.StackTrace);

                    mod_instance.SetActive(false);
                    LogService.LogError(
                        $"{pMod.Name} has been disabled due to an error. Please check the log for details.");

                    continue;
                }
            }
            if (!any_loaded)
            {
                all_success = false;
                LogService.LogError(
                    $"No valid mod component found in assembly {mod_assembly.FullName} for mod {pMod.UID}");
            }
        }
        if (all_success)
        {
            WorldBoxMod.AllRecognizedMods[pMod] = ModState.LOADED;
            ModDepenSolveService.MarkModLoaded(pMod);
        }
        else
        {
            pMod.FailReason.AppendLine("All mod assemblies failed to load.");
            ModInfoUtils.clearModCompileTimestamp(pMod.UID);
        }
    }

    /// <summary>
    /// Initializes a single mod if the mod implements IStagedLoad
    /// </summary>
    /// <param name="mod">The mod to init</param>
    public static bool TryInitMod(IMod mod)
    {
        if (mod is IStagedLoad staged_load_mod)
        {
            try
            {
                staged_load_mod.Init();
            }
            catch (Exception e)
            {
                LogService.LogError(e.Message);
                if (e.StackTrace != null) LogService.LogError(e.StackTrace);
                mod.GetGameObject().SetActive(false);
                LogService.LogError(
                    $"{mod.GetDeclaration().Name} has been disabled due to an init error. Please check the log for details.");
                return false;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Post initializes a single mod if the mod implements IStagedLoad
    /// </summary>
    /// <param name="mod">The mod to post-init</param>
    public static void PostInitMod(IMod mod)
    {
        if (mod is IStagedLoad staged_load_mod)
        {
            try
            {
                staged_load_mod.PostInit();
            }
            catch (Exception e)
            {
                LogService.LogError(e.Message);
                if (e.StackTrace != null) LogService.LogError(e.StackTrace);
                mod.GetGameObject().SetActive(false);
                LogService.LogError(
                    $"{mod.GetDeclaration().Name} has been disabled due to a post init error. Please check the log for details.");
            }
        }
    }

    /// <summary>
    /// Check whether a mod loaded with mod's UID
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    public static bool IsModLoaded(string uid)
    {
        foreach (var mod in WorldBoxMod.LoadedMods)
        {
            if (mod.GetDeclaration().UID == uid)
            {
                return true;
            }
        }

        return false;
    }

    private static string BuildRuntimeLoadErrorMessage(ModDeclare pModDeclare, string pReason)
    {
        return $"Failed to load mod {pModDeclare.Name}:\n{pReason.Trim()}";
    }

    private static void ShowRuntimeLoadError(ModDeclare pModDeclare, string pFallbackReason)
    {
        string reason = pModDeclare.FailReason.ToString().Trim();
        if (string.IsNullOrWhiteSpace(reason))
        {
            reason = pFallbackReason;
        }

        ErrorWindow.errorMessage = BuildRuntimeLoadErrorMessage(pModDeclare, reason);
        ScrollWindow.get("error_with_reason").clickShow();
    }

    private static bool TryLoadCompiledModAtRuntime(ModDeclare pModDeclare)
    {
        MasterBuilder builder = new MasterBuilder();
        ResourcesPatch.LoadResourceFromFolder(Path.Combine(pModDeclare.FolderPath, Paths.ModResourceFolderName),
            out List<Builder> builders);
        ResourcesPatch.LoadResourceFromFolder(Path.Combine(pModDeclare.FolderPath,
            Paths.NCMSAdditionModResourceFolderName), out List<Builder> builders2);
        ResourcesPatch.LoadAssetBundlesFromFolder(Path.Combine(pModDeclare.FolderPath, Paths.ModAssetBundleFolderName));

        LoadMod(pModDeclare);
        builder.AddBuilders(builders);
        builder.AddBuilders(builders2);
        builder.BuildAll();

        if (IsModLoaded(pModDeclare.UID))
        {
            return true;
        }

        WorldBoxMod.AllRecognizedMods[pModDeclare] = ModState.FAILED;
        return false;
    }

    /// <summary>
    /// Compile mod at runtime.
    /// </summary>
    /// <param name="pModDeclare">Info of to be compiled mod</param>
    /// <param name="pForce">Wheather recompile mod if the mod has been compiled</param>
    /// <returns></returns>
    public static bool TryCompileModAtRuntime(ModDeclare pModDeclare, bool pForce = false)
    {
        pModDeclare = ModInfoUtils.EnsureRecognizedMod(pModDeclare);
        if (pModDeclare.ModType == ModTypeEnum.BEPINEX)
        {
            ModInfoUtils.LinkBepInExModToLocalRequest(pModDeclare);
            ModInfoUtils.DealWithBepInExModLinkRequests();
            return false;
        }

        ModDependencyNode node = ModDepenSolveService.EnsureNode(pModDeclare);
        bool success = CompileModNodes(new[] { node }, pForce).Contains(node.mod_decl.UID);
        if (!success)
        {
            ShowRuntimeLoadError(pModDeclare,
                "Failed to compile mod. Check incompatible mods and dependencies, then try again.");
            return false;
        }

        ModInfoUtils.SaveModRecords();
        return true;
    }

    /// <summary>
    /// Compile and load mod at runtime
    /// </summary>
    /// <param name="mod_declare">Info of to be compiled mo</param>
    /// <returns></returns>
    public static bool TryCompileAndLoadModAtRuntime(ModDeclare mod_declare)
    {
        mod_declare = ModInfoUtils.EnsureRecognizedMod(mod_declare);
        bool actually_loaded = IsModLoaded(mod_declare.UID);

        if (actually_loaded) return false;

        if (mod_declare.ModType == ModTypeEnum.BEPINEX)
        {
            ModInfoUtils.LinkBepInExModToLocalRequest(mod_declare);
            ModInfoUtils.DealWithBepInExModLinkRequests();
            return false;
        }

        ModEnablePlan plan = ModDepenSolveService.BuildRuntimeEnablePlan(mod_declare);
        if (plan.HasFailure)
        {
            ShowRuntimeLoadError(mod_declare, plan.FailureReason);
            return false;
        }

        HashSet<string> compiled_nodes = CompileModNodes(plan.LoadOrder);
        foreach (ModDependencyNode node in plan.LoadOrder)
        {
            if (node.Loaded || IsModLoaded(node.mod_decl.UID))
            {
                ModDepenSolveService.MarkModLoaded(node.mod_decl);
                continue;
            }

            if (!compiled_nodes.Contains(node.mod_decl.UID))
            {
                ModDepenSolveService.RollbackEnablePlan(plan);
                mod_declare.FailReason.Clear();
                mod_declare.FailReason.Append(node.mod_decl.FailReason);
                WorldBoxMod.AllRecognizedMods[mod_declare] = ModState.FAILED;
                ShowRuntimeLoadError(node.mod_decl,
                    "Failed to compile mod. Check incompatible mods and dependencies, then try again.");
                return false;
            }
        }

        foreach (ModDependencyNode node in plan.LoadOrder)
        {
            if (node.Loaded || IsModLoaded(node.mod_decl.UID))
            {
                ModDepenSolveService.MarkModLoaded(node.mod_decl);
                continue;
            }

            if (!TryLoadCompiledModAtRuntime(node.mod_decl))
            {
                ModDepenSolveService.RollbackEnablePlan(plan);
                mod_declare.FailReason.Clear();
                mod_declare.FailReason.Append(node.mod_decl.FailReason);
                WorldBoxMod.AllRecognizedMods[mod_declare] = ModState.FAILED;
                ShowRuntimeLoadError(node.mod_decl, "Failed to load mod. Check the log for details.");
                return false;
            }
        }

        if (!plan.RequestedRoots.All(IsModLoaded))
        {
            ModDepenSolveService.RollbackEnablePlan(plan);
            ShowRuntimeLoadError(mod_declare, "Failed to load mod. Check the log for details.");
            return false;
        }

        ModDepenSolveService.CommitEnablePlan(plan);
        ModInfoUtils.SaveModRecords();
        return true;
    }

    public static bool TryEnableMod(ModDeclare pModDeclare)
    {
        pModDeclare = ModInfoUtils.EnsureRecognizedMod(pModDeclare);
        if (IsModLoaded(pModDeclare.UID))
        {
            ModEnablePlan plan = ModDepenSolveService.BuildRuntimeEnablePlan(pModDeclare);
            if (plan.HasFailure)
            {
                ShowRuntimeLoadError(pModDeclare, plan.FailureReason);
                return false;
            }

            ModDepenSolveService.CommitEnablePlan(plan);
            ModInfoUtils.SaveModRecords();
            return true;
        }

        return TryCompileAndLoadModAtRuntime(pModDeclare);
    }

    public static void DisableMod(ModDeclare pModDeclare)
    {
        ModDepenSolveService.SetModDesiredEnabled(pModDeclare, false);
    }

    /// <summary>
    /// Load information of all BepInEx plugins which is made only for Worldbox
    /// </summary>
    public static void loadInfoOfBepInExPlugins()
    {
        List<ModDeclare> bepInExMods = ModInfoUtils.recogBepInExMods();

        GameObject bepinexManager = GameObject.Find("BepInEx_Manager");
        foreach (var mod in bepInExMods)
        {
            ModDeclare recognized_mod = ModInfoUtils.EnsureRecognizedMod(mod);
            if (IsModLoaded(mod.UID))
            {
                LogService.LogWarning($"Repeat Mod with {mod.UID}, Only load one of them");
                continue;
            }

            BepinexMod virtualMod = new();
            MonoBehaviour virtualModComponent = null;

            // try to find the GameObject of the mod
            if (bepinexManager != null)
            {
                var bepinexComponents = bepinexManager.GetComponents<MonoBehaviour>();
                foreach (MonoBehaviour component in bepinexComponents.Where(component =>
                             (component.GetType().FullName ?? "").Contains(mod.Name)))
                {
                    virtualModComponent = component;
                    break;
                }
            }

            virtualMod.OnLoad(recognized_mod, virtualModComponent);
            WorldBoxMod.LoadedMods.Add(virtualMod);
            WorldBoxMod.AllRecognizedMods[recognized_mod] = ModState.LOADED;
            ModDepenSolveService.MarkModLoaded(recognized_mod);
        }
    }
    internal static HashSet<string> CompileModNodes(IEnumerable<ModDependencyNode> pModNodes, bool pForce = false)
    {
        List<ModDependencyNode> nodes = pModNodes.Distinct().ToList();
        HashSet<string> successful_nodes = new();
        if (nodes.Count == 0)
        {
            return successful_nodes;
        }

        CompileSessionContext context = CreateCompileSessionContext(nodes);
        List<List<ModDependencyNode>> stages =
            ModDependencyUtils.PartitionModsCompileStagesFromDependencyTopology(nodes);
        Dictionary<string, CompileNodeResult> completed_results = new();
        bool should_clear_compile_records = false;

        foreach (List<ModDependencyNode> stage in stages)
        {
            Dictionary<string, MetadataReference> available_mod_references =
                context.ModReferences.ToDictionary(entry => entry.Key, entry => entry.Value);
            ConcurrentDictionary<string, CompileNodeResult> stage_results = new();
            int max_degree_of_parallelism = Math.Min(stage.Count, Math.Max(1, Environment.ProcessorCount - 1));

            Parallel.ForEach(stage, new ParallelOptions { MaxDegreeOfParallelism = max_degree_of_parallelism }, node =>
            {
                stage_results[node.mod_decl.UID] =
                    CompileNodeInSession(node, context, available_mod_references, completed_results, pForce);
            });

            foreach (ModDependencyNode node in stage)
            {
                CompileNodeResult result = stage_results[node.mod_decl.UID];
                completed_results[node.mod_decl.UID] = result;

                if (result.Success)
                {
                    successful_nodes.Add(node.mod_decl.UID);
                    node.mod_decl.FailReason.Clear();

                    if (result.ProducedReference != null)
                    {
                        context.ModReferences[node.mod_decl.UID] = result.ProducedReference;
                    }

                    if (result.ShouldRecordCompileCache)
                    {
                        ModInfoUtils.RecordMod(node.mod_decl, result.AvailableDependencies.ToList(),
                            result.AvailableOptionalDependencies.ToList(), false, false);
                    }

                    if (result.UsedOptionalDependencyFallback)
                    {
                        LogCompileFailureWithOptionalDependencies(node.mod_decl.UID,
                            result.OptionalDependencyCompileErrors);
                    }

                    continue;
                }

                should_clear_compile_records = true;
                ApplyCompileFailure(result);
            }
        }

        if (should_clear_compile_records)
        {
            File.WriteAllText(Paths.ModCompileRecordPath, "");
        }

        LogService.PullAllConcurrentLogToCurrentThread();
        return successful_nodes;
    }

    private static CompileSessionContext CreateCompileSessionContext(IReadOnlyCollection<ModDependencyNode> pModNodes)
    {
        var default_ref_paths = new List<string>();
        default_ref_paths.AddRange(Directory.GetFiles(Paths.ManagedPath, "*.dll"));
        default_ref_paths.AddRange(Directory.GetFiles(Paths.NMLAssembliesPath, "*.dll"));
        default_ref_paths.Add(Paths.NMLModPath);

        List<MetadataReference> default_references = new();
        foreach (string default_ref_path in default_ref_paths)
        {
            try
            {
                default_references.Add(MetadataReference.CreateFromFile(default_ref_path));
            }
            catch (Exception e)
            {
                LogService.LogError($"Error when load default reference {default_ref_path}: {e.Message}");
            }
        }

        CompileSessionContext context = new(default_references.ToArray(),
            MetadataReference.CreateFromFile(Paths.PublicizedAssemblyPath));
        SeedAvailableModReferences(context, pModNodes);
        return context;
    }

    private static void SeedAvailableModReferences(CompileSessionContext pContext, IReadOnlyCollection<ModDependencyNode> pModNodes)
    {
        HashSet<string> selected_node_uids = pModNodes.Select(node => node.mod_decl.UID).ToHashSet();
        HashSet<string> visited = new();

        foreach (ModDependencyNode node in pModNodes)
        {
            SeedAvailableModReferencesRecursive(node, pContext, selected_node_uids, visited);
        }
    }

    private static void SeedAvailableModReferencesRecursive(ModDependencyNode pNode, CompileSessionContext pContext,
        ISet<string> pSelectedNodeUids, ISet<string> pVisited)
    {
        if (!pVisited.Add(pNode.mod_decl.UID))
        {
            return;
        }

        bool should_seed_current_reference = pNode.Loaded || !pSelectedNodeUids.Contains(pNode.mod_decl.UID);
        if (should_seed_current_reference && TryCreateMetadataReference(pNode.mod_decl, out MetadataReference reference))
        {
            pContext.ModReferences[pNode.mod_decl.UID] = reference;
        }

        foreach (ModDependencyNode dependency in pNode.depend_on)
        {
            SeedAvailableModReferencesRecursive(dependency, pContext, pSelectedNodeUids, pVisited);
        }
    }

    private static CompileNodeResult CompileNodeInSession(ModDependencyNode pNode, CompileSessionContext pContext,
        IReadOnlyDictionary<string, MetadataReference> pAvailableModReferences,
        IReadOnlyDictionary<string, CompileNodeResult> pCompletedResults, bool pForce)
    {
        List<string> failed_dependencies = pNode.mod_decl.Dependencies
            .Where(dependency_uid =>
                pCompletedResults.TryGetValue(dependency_uid, out CompileNodeResult dependency_result) &&
                !dependency_result.Success)
            .Distinct()
            .ToList();
        if (failed_dependencies.Count > 0)
        {
            return CreateFailedResult(pNode, BuildFailedDependencyCompileMessage(pNode.mod_decl, failed_dependencies));
        }

        if (pNode.Loaded)
        {
            if (TryCreateMetadataReference(pNode.mod_decl, out MetadataReference reference))
            {
                return CreateSuccessResult(pNode, reference);
            }

            if (pNode.mod_decl.ModType is ModTypeEnum.BEPINEX or ModTypeEnum.RESOURCE_PACK)
            {
                return new CompileNodeResult
                {
                    Node = pNode,
                    Success = true
                };
            }

            return CreateFailedResult(pNode,
                $"Mod {pNode.mod_decl.UID} is already loaded, but its compiled assembly could not be found.");
        }

        string[] precompiled_dll_files = Directory.GetFiles(pNode.mod_decl.FolderPath, "*.dll");
        if (precompiled_dll_files.Length > 0)
        {
            LogService.LogInfoConcurrent(
                $"{pNode.mod_decl.UID} detected as precompiled, compilation phase will be skipped on it!");
            pNode.mod_decl.SetModType(ModTypeEnum.COMPILED_NEOMOD);

            string main_dll = precompiled_dll_files.FirstOrDefault(file =>
                                  Path.GetFileNameWithoutExtension(file) == pNode.mod_decl.UID) ??
                              precompiled_dll_files[0];
            return CreateSuccessResult(pNode, MetadataReference.CreateFromFile(main_dll));
        }

        bool has_available_optional_depen =
            pNode.mod_decl.OptionalDependencies.Any(pAvailableModReferences.ContainsKey);
        CompileNodeResult compile_result =
            CompileModFromSource(pNode, pNode.GetAdditionReferences().ToArray(), pAvailableModReferences, pContext,
                pForce);
        if (compile_result.Success)
        {
            return compile_result;
        }

        if (!has_available_optional_depen)
        {
            return compile_result;
        }

        LogService.LogWarningConcurrent(
            $"Cannot compile mod {pNode.mod_decl.UID} with Optional Dependencies, try to disable them");
        CompileNodeResult fallback_result =
            CompileModFromSource(pNode, pNode.GetAdditionReferences(false).ToArray(), pAvailableModReferences,
                pContext, pForce, true);
        if (!fallback_result.Success)
        {
            return fallback_result;
        }

        return new CompileNodeResult
        {
            Node = pNode,
            Success = true,
            ProducedReference = fallback_result.ProducedReference,
            AvailableDependencies = fallback_result.AvailableDependencies,
            AvailableOptionalDependencies = fallback_result.AvailableOptionalDependencies,
            ShouldRecordCompileCache = fallback_result.ShouldRecordCompileCache,
            UsedOptionalDependencyFallback = true,
            OptionalDependencyCompileErrors = compile_result.CompileErrors
        };
    }

    private static CompileNodeResult CompileModFromSource(ModDependencyNode pNode, string[] pAddInc,
        IReadOnlyDictionary<string, MetadataReference> pAvailableModReferences, CompileSessionContext pContext,
        bool pForce = false, bool pDisableOptionalDepen = false)
    {
        ModDeclare pModDecl = pNode.mod_decl;
        List<string> available_optional_depens = pDisableOptionalDepen
            ? new List<string>()
            : pModDecl.OptionalDependencies.Where(pAvailableModReferences.ContainsKey).ToList();
        List<string> available_depens = pModDecl.Dependencies.Where(pAvailableModReferences.ContainsKey).ToList();

        if (!pForce && !ModInfoUtils.doesModNeedRecompile(pModDecl, available_depens, available_optional_depens))
        {
            LoadAdditionalReferences(pContext, pAddInc, pModDecl.UID);
            if (!TryCreateMetadataReference(pModDecl, out MetadataReference reference))
            {
                return CreateFailedResult(pNode, $"Compiled assembly for mod {pModDecl.UID} was not found.");
            }

            return new CompileNodeResult
            {
                Node = pNode,
                Success = true,
                ProducedReference = reference,
                AvailableDependencies = available_depens,
                AvailableOptionalDependencies = available_optional_depens
            };
        }

        var preprocessor_symbols = new List<string>();
        List<MetadataReference> references = pContext.DefaultReferences.ToList();
        references.AddRange(pAddInc.Select(inc => MetadataReference.CreateFromFile(inc)));
        LoadAdditionalReferences(pContext, pAddInc, pModDecl.UID);
        if (pModDecl.UsePublicizedAssembly)
        {
            references.Add(pContext.PublicizedAssemblyReference);
        }

        foreach (string dependency_uid in available_depens)
        {
            if (!pAvailableModReferences.TryGetValue(dependency_uid, out MetadataReference dependency_reference))
            {
                return CreateFailedResult(pNode,
                    $"Required dependency {dependency_uid} is missing a compiled assembly reference.");
            }

            references.Add(dependency_reference);
        }

        foreach (string optional_dependency_uid in available_optional_depens)
        {
            if (!pAvailableModReferences.TryGetValue(optional_dependency_uid,
                    out MetadataReference optional_dependency_reference))
            {
                return CreateFailedResult(pNode,
                    $"Optional dependency {optional_dependency_uid} is missing a compiled assembly reference.");
            }

            references.Add(optional_dependency_reference);
            preprocessor_symbols.Add(ModDependencyUtils.ParseDepenNameToPreprocessSymbol(optional_dependency_uid));
        }

        var syntax_trees = new List<SyntaxTree>();
        var code_files = SystemUtils.SearchFileRecursive(pModDecl.FolderPath,
            file_name => file_name.EndsWith(".cs") && !file_name.StartsWith("."),
            dir_name => !dir_name.StartsWith(".") &&
                        !Paths.CompileIgnoreSearchDirectories.Contains(dir_name));
        var embeded_resources = new List<ResourceDescription>();

        bool is_ncms_mod = false;
        var parse_option = new CSharpParseOptions(LanguageVersion.Latest, preprocessorSymbols: preprocessor_symbols);

        foreach (string code_file in code_files)
        {
            SourceText source_text = SourceText.From(File.ReadAllText(code_file), Encoding.UTF8);
            SyntaxTree syntax_tree =
                CSharpSyntaxTree.ParseText(
                    source_text,
                    parse_option,
                    code_file.Substring(pModDecl.FolderPath.Length + 1)
                );
            syntax_trees.Add(syntax_tree);
            if (!is_ncms_mod)
            {
                is_ncms_mod = NCMSCompatibleLayer.IsNCMSMod(syntax_tree);
            }
        }

        if (is_ncms_mod)
        {
            string embeded_resource_folder = Path.Combine(pModDecl.FolderPath, Paths.NCMSModEmbededResourceFolderName);
            if (Directory.Exists(embeded_resource_folder))
            {
                string[] embeded_resource_files = Directory.GetFiles(embeded_resource_folder, "*",
                    SearchOption.AllDirectories);
                foreach (string file in embeded_resource_files)
                {
                    string relative_path = file.Substring(embeded_resource_folder.Length + 1);
                    string resource_name =
                        $"{pModDecl.Name}.Resources.{relative_path.Replace('\\', '.').Replace('/', '.')}";
                    embeded_resources.Add(new ResourceDescription(
                        resource_name,
                        () => File.OpenRead(file),
                        true
                    ));
                }
            }

            SourceText global_object_source_text = SourceText.From(NCMSCompatibleLayer.modGlobalObject, Encoding.UTF8);
            SyntaxTree global_object_syntax_tree =
                CSharpSyntaxTree.ParseText(
                    global_object_source_text,
                    parse_option,
                    $"{pModDecl.Name}.GlobalObject.cs"
                );
            syntax_trees.Add(global_object_syntax_tree);
        }

        pModDecl.IsNCMSMod = is_ncms_mod;

        var compilation = CSharpCompilation.Create(
            pModDecl.UID,
            syntax_trees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                allowUnsafe: true, deterministic: true, assemblyIdentityComparer: AssemblyIdentityComparer.Default)
        );

        using MemoryStream dllms = new();
        using MemoryStream pdbms = new();

        string dll_path = Path.Combine(Paths.CompiledModsPath, $"{pModDecl.UID}.dll");
        string pdb_path = Path.Combine(Paths.CompiledModsPath, $"{pModDecl.UID}.pdb");

        EmitResult emit_result = compilation.Emit(dllms, pdbms,
            manifestResources: embeded_resources,
            options: new EmitOptions(
                debugInformationFormat: DebugInformationFormat.PortablePdb,
                pdbFilePath: pdb_path
            )
        );

        if (!emit_result.Success)
        {
            return new CompileNodeResult
            {
                Node = pNode,
                Success = false,
                FailureReason = "Compile Failed\n Check Log for details\n All mods compiled before it will be recompiled next time",
                CompileErrors = CollectCompileErrors(emit_result.Diagnostics),
                AvailableDependencies = available_depens,
                AvailableOptionalDependencies = available_optional_depens
            };
        }

        using (var dll_fs = new FileStream(dll_path, FileMode.Create, FileAccess.Write))
        {
            dllms.Seek(0, SeekOrigin.Begin);
            dllms.WriteTo(dll_fs);
        }

        using (var pdb_fs = new FileStream(pdb_path, FileMode.Create, FileAccess.Write))
        {
            pdbms.Seek(0, SeekOrigin.Begin);
            pdbms.WriteTo(pdb_fs);
        }

        return new CompileNodeResult
        {
            Node = pNode,
            Success = true,
            ProducedReference = MetadataReference.CreateFromFile(dll_path),
            AvailableDependencies = available_depens,
            AvailableOptionalDependencies = available_optional_depens,
            ShouldRecordCompileCache = true
        };
    }

    private static void LoadAdditionalReferences(CompileSessionContext pContext, IEnumerable<string> pAdditionalReferences,
        string pModUid)
    {
        foreach (string inc in pAdditionalReferences)
        {
            string file_name = Path.GetFileName(inc);
            if (file_name == "Assembly-CSharp.dll")
            {
                continue;
            }

            if (!pContext.LoadedAdditionalReferences.TryAdd(file_name, 0))
            {
                continue;
            }

            try
            {
                Assembly loaded_inc = Assembly.LoadFrom(inc);
                LogService.LogInfoConcurrent($"Load {loaded_inc.FullName}");
            }
            catch (Exception e)
            {
                LogService.LogWarningConcurrent($"Failed to load Assembly {file_name} for mod {pModUid}");
                LogService.LogWarningConcurrent(e.Message);
                if (e.StackTrace != null)
                {
                    LogService.LogWarningConcurrent(e.StackTrace);
                }
            }
        }
    }

    private static bool TryCreateMetadataReference(ModDeclare pModDeclare, out MetadataReference pReference)
    {
        pReference = null;
        if (!TryGetCompiledAssemblyPath(pModDeclare, out string assembly_path))
        {
            return false;
        }

        pReference = MetadataReference.CreateFromFile(assembly_path);
        return true;
    }

    private static bool TryGetCompiledAssemblyPath(ModDeclare pModDeclare, out string pAssemblyPath)
    {
        if (pModDeclare.ModType == ModTypeEnum.COMPILED_NEOMOD)
        {
            string[] dll_files = Directory.GetFiles(pModDeclare.FolderPath, "*.dll");
            if (dll_files.Length == 0)
            {
                pAssemblyPath = string.Empty;
                return false;
            }

            pAssemblyPath = dll_files.FirstOrDefault(file =>
                                Path.GetFileNameWithoutExtension(file) == pModDeclare.UID) ??
                            dll_files[0];
            return true;
        }

        pAssemblyPath = Path.Combine(Paths.CompiledModsPath, $"{pModDeclare.UID}.dll");
        return File.Exists(pAssemblyPath);
    }

    private static CompileNodeResult CreateSuccessResult(ModDependencyNode pNode, MetadataReference pProducedReference)
    {
        return new CompileNodeResult
        {
            Node = pNode,
            Success = true,
            ProducedReference = pProducedReference
        };
    }

    private static CompileNodeResult CreateFailedResult(ModDependencyNode pNode, string pFailureReason,
        string pCompileErrors = "")
    {
        return new CompileNodeResult
        {
            Node = pNode,
            Success = false,
            FailureReason = pFailureReason,
            CompileErrors = pCompileErrors
        };
    }

    private static void ApplyCompileFailure(CompileNodeResult pResult)
    {
        if (!string.IsNullOrWhiteSpace(pResult.CompileErrors))
        {
            LogCompileFailure(pResult.Node.mod_decl.UID, pResult.CompileErrors);
        }
        else if (!string.IsNullOrWhiteSpace(pResult.FailureReason))
        {
            LogService.LogError(pResult.FailureReason);
        }

        pResult.Node.mod_decl.FailReason.Clear();
        pResult.Node.mod_decl.FailReason.AppendLine(pResult.FailureReason);
        WorldBoxMod.AllRecognizedMods[pResult.Node.mod_decl] = ModState.FAILED;
    }
}
