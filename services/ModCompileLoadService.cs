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
using UnityEngine;

namespace NeoModLoader.services;

/// <summary>
/// Service of mod compiling and loading. 
/// </summary>
public static class ModCompileLoadService
{
    private static string[] _default_ref_path = null!;
    private static readonly Dictionary<string, string> mod_inc_path = new();
    private static readonly HashSet<string> _loaded_ref = new();

    private static MetadataReference[] _default_ref = null!;
    private static MetadataReference _publicized_assembly_ref = null!;
    private static readonly Dictionary<string, MetadataReference> mod_ref = new();

    private static bool compileMod(ModDeclare pModDecl, IEnumerable<MetadataReference> pDefaultInc,
        string[] pAddInc, Dictionary<string, MetadataReference> pModInc, bool pForce = false,
        bool pDisableOptionalDepen = false)
    {
        if (!pForce && !ModInfoUtils.isModNeedRecompile(pModDecl))
        {
            LoadAddInc();
            return true;
        }

        var preprocessor_symbols = new List<string>();

        List<MetadataReference> list = pDefaultInc.ToList();
        list.AddRange(pAddInc.Select(inc => MetadataReference.CreateFromFile(inc)));
        LoadAddInc();
        if (pModDecl.UsePublicizedAssembly)
        {
            list.Add(_publicized_assembly_ref);
        }

        foreach (var depen in pModDecl.Dependencies)
        {
            if (!pModInc.ContainsKey(depen))
            {
                LogService.LogError($"{pModDecl.UID} miss dependency {depen}");
                return false;
            }

            list.Add(pModInc[depen]);
            if (pModInc[depen] == null)
            {
                LogService.LogError($"{pModDecl.UID}'s optional ref of {depen} instance is null");
                return false;
            }
        }

        if (!pDisableOptionalDepen)
        {
            foreach (var option_depen in pModDecl.OptionalDependencies)
            {
                if (pModInc.TryGetValue(option_depen, out var value))
                {
                    list.Add(value);
                    preprocessor_symbols.Add(ModDependencyUtils.ParseDepenNameToPreprocessSymbol(option_depen));
                    if (value == null)
                    {
                        LogService.LogError($"{pModDecl.UID}'s optional ref of {option_depen} instance is null");
                        return false;
                    }
                }
            }
        }

        var syntaxTrees = new List<SyntaxTree>();
        var code_files = SystemUtils.SearchFileRecursive(pModDecl.FolderPath,
            file_name => file_name.EndsWith(".cs") && !file_name.StartsWith("."),
            dir_name => !dir_name.StartsWith(".") && !Paths.IgnoreSearchDirectories.Contains(dir_name));
        var embeded_resources = new List<ResourceDescription>();

        bool is_ncms_mod = false;
        var parse_option = new CSharpParseOptions(LanguageVersion.Latest, preprocessorSymbols: preprocessor_symbols);

        foreach (var code_file in code_files)
        {
            SourceText sourceText = SourceText.From(File.ReadAllText(code_file), Encoding.UTF8);
            SyntaxTree syntaxTree =
                CSharpSyntaxTree.ParseText(
                    sourceText,
                    parse_option,
                    code_file.Substring(pModDecl.FolderPath.Length + 1)
                );
            syntaxTrees.Add(syntaxTree);
            if (!is_ncms_mod)
            {
                is_ncms_mod = NCMSCompatibleLayer.IsNCMSMod(syntaxTree);
            }
        }


        if (is_ncms_mod)
        {
            // Load Manifest Files
            string embeded_resource_folder = Path.Combine(pModDecl.FolderPath, Paths.NCMSModEmbededResourceFolderName);
            if (Directory.Exists(embeded_resource_folder))
            {
                var embeded_resource_files = Directory.GetFiles(
                    embeded_resource_folder, "*", SearchOption.AllDirectories);
                foreach (var file in embeded_resource_files)
                {
                    var relative_path = file.Substring(embeded_resource_folder.Length + 1);
                    var resource_name =
                        $"{pModDecl.Name}.Resources.{relative_path.Replace('\\', '.').Replace('/', '.')}";
                    var resource_desc = new ResourceDescription(
                        resource_name,
                        () => File.OpenRead(file),
                        true
                    );
                    embeded_resources.Add(resource_desc);
                }
            }

            // Load Global Object
            SourceText global_object_sourceText = SourceText.From(NCMSCompatibleLayer.modGlobalObject, Encoding.UTF8);
            SyntaxTree global_object_syntaxTree =
                CSharpSyntaxTree.ParseText(
                    global_object_sourceText,
                    parse_option,
                    $"{pModDecl.Name}.GlobalObject.cs"
                );
            syntaxTrees.Add(global_object_syntaxTree);
        }

        pModDecl.IsNCMSMod = is_ncms_mod;

        void LoadAddInc()
        {
            foreach (var inc in pAddInc)
            {
                string file_name = Path.GetFileName(inc);
                if (file_name == "Assembly-CSharp.dll")
                {
                    continue;
                }

                if (_loaded_ref.Contains(file_name)) continue;
                _loaded_ref.Add(file_name);
                try
                {
                    var loaded_inc = Assembly.LoadFrom(inc);
                    LogService.LogInfo($"Load {loaded_inc.FullName}");
                }
                catch (Exception e)
                {
                    LogService.LogWarning($"Failed to load Assembly {file_name} for mod {pModDecl.UID}");
                    LogService.LogWarning(e.Message);
                    LogService.LogWarning(e.StackTrace);
                }
            }
        }


        var compilation = CSharpCompilation.Create(
            $"{pModDecl.UID}",
            syntaxTrees,
            list,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true)
        );

        using MemoryStream dllms = new MemoryStream();
        using MemoryStream pdbms = new MemoryStream();

        string dll_path = Path.Combine(Paths.CompiledModsPath, $"{pModDecl.UID}.dll");
        string pdb_path = Path.Combine(Paths.CompiledModsPath, $"{pModDecl.UID}.pdb");

        var result = compilation.Emit(dllms, pdbms,
            manifestResources: embeded_resources,
            options: new EmitOptions(
                debugInformationFormat: DebugInformationFormat.PortablePdb,
                pdbFilePath: pdb_path
            )
        );

        if (!result.Success)
        {
            StringBuilder diags = new StringBuilder();
            foreach (var diagnostic in result.Diagnostics)
            {
                if (diagnostic.Severity != DiagnosticSeverity.Error) continue;
                diags.AppendLine(diagnostic.ToString());
            }

            LogService.LogError(diags.ToString());
            return false;
        }

        using var dll_fs = new FileStream(dll_path, FileMode.Create, FileAccess.Write);
        dllms.Seek(0, SeekOrigin.Begin);
        dllms.WriteTo(dll_fs);

        using var pdb_fs = new FileStream(pdb_path, FileMode.Create, FileAccess.Write);
        pdbms.Seek(0, SeekOrigin.Begin);
        pdbms.WriteTo(pdb_fs);

        return true;
    }

    /// <summary>
    /// Prepare references for mod nodes
    /// </summary>
    /// <param name="pModNodes"></param>
    public static void prepareCompile(List<ModDependencyNode> pModNodes)
    {
        foreach (var mod_node in pModNodes)
        {
            mod_inc_path.Add(mod_node.mod_decl.UID,
                Path.Combine(Paths.CompiledModsPath, $"{mod_node.mod_decl.UID}.dll"));
        }

        var default_ref_path_list = new List<string>();
        default_ref_path_list.AddRange(Directory.GetFiles(Paths.ManagedPath, "*.dll"));
        default_ref_path_list.AddRange(Directory.GetFiles(Paths.NMLAssembliesPath, "*.dll"));
        default_ref_path_list.Add(Paths.NMLModPath);
        _default_ref_path = default_ref_path_list.ToArray();

        _default_ref = new MetadataReference[_default_ref_path.Length];
        for (int i = 0; i < _default_ref_path.Length; i++)
        {
            try
            {
                _default_ref[i] = MetadataReference.CreateFromFile(_default_ref_path[i]);
                if (_default_ref[i] == null) throw new Exception("Ref created is null");
            }
            catch (Exception e)
            {
                LogService.LogError($"Error when load default reference {_default_ref_path[i]}: {e.Message}");
            }
        }

        _publicized_assembly_ref = MetadataReference.CreateFromFile(Paths.PublicizedAssemblyPath);
    }

    /// <summary>
    /// Prepare references for a single mod node
    /// </summary>
    /// <param name="pModNode"></param>
    public static void prepareCompileRuntime(ModDependencyNode pModNode)
    {
        mod_inc_path.Add(pModNode.mod_decl.UID,
            Path.Combine(Paths.CompiledModsPath, $"{pModNode.mod_decl.UID}.dll"));
    }

    /// <summary>
    /// Public mod compiling method
    /// </summary>
    /// <param name="pModNode">The mod to compile</param>
    /// <param name="pForce">Wheather recompile when the mod does not need to recompile</param>
    /// <returns></returns>
    public static bool compileMod(ModDependencyNode pModNode, bool pForce = false)
    {
        bool compile_result = false;

        string[] add_inc_path;
        try
        {
            add_inc_path = Directory.GetFiles(
                Path.Combine(pModNode.mod_decl.FolderPath, "Assemblies"), "*.dll");
        }
        catch (DirectoryNotFoundException)
        {
            add_inc_path = new string[0];
        }

        bool disable_optional_depen = false;
        RECOMPILE:
        compile_result =
            compileMod(pModNode.mod_decl, _default_ref,
                add_inc_path, mod_ref, pForce, disable_optional_depen
            );
        if (compile_result)
        {
            mod_ref[pModNode.mod_decl.UID] =
                MetadataReference.CreateFromFile(Path.Combine(Paths.CompiledModsPath,
                    $"{pModNode.mod_decl.UID}.dll"));
        }
        else if (!disable_optional_depen && pModNode.mod_decl.OptionalDependencies.Length > 0)
        {
            LogService.LogWarning(
                $"Cannot compile mod {pModNode.mod_decl.UID} with Optional Dependencies, try to disable them");
            disable_optional_depen = true;
            goto RECOMPILE;
        }

        if (!compile_result)
        {
            mod_inc_path.Remove(pModNode.mod_decl.UID);
            pModNode.mod_decl.FailReason.AppendLine(
                "Compile Failed\n Check Log for details\n All mods compiled before it will be recompiled next time");
            File.WriteAllText(Paths.ModCompileRecordPath, "");
        }
        else
        {
            ModInfoUtils.updateModCompileTimestamp(pModNode.mod_decl.UID);
        }

        return compile_result;
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
            catch (ReflectionTypeLoadException)
            {
                LogService.LogError(
                    $"Compiled mod {mod.UID} out of date, if it happens again after restarting game, please update, delete or unorder it");
                string dll_path = Path.Combine(Paths.CompiledModsPath, $"{mod.UID}.dll");
                string pdb_path = Path.Combine(Paths.CompiledModsPath, $"{mod.UID}.pdb");
                if (File.Exists(dll_path))
                {
                    File.Delete(dll_path);
                }

                if (File.Exists(pdb_path))
                {
                    File.Delete(pdb_path);
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
        Assembly mod_assembly = Assembly.Load(
            File.ReadAllBytes(Path.Combine(Paths.CompiledModsPath, $"{pMod.UID}.dll")),
            File.ReadAllBytes(Path.Combine(Paths.CompiledModsPath, $"{pMod.UID}.pdb"))
        );
        GameObject mod_instance;
        foreach (var type in mod_assembly.GetTypes())
        {
            if (type.GetInterface(nameof(IMod)) == null)
            {
                // Check if it is a NCMS Mod
#pragma warning disable CS0618 // It is a NCMS mod
                if (Attribute.GetCustomAttribute(type, typeof(ModEntry)) != null &&
                    type.IsSubclassOf(typeof(MonoBehaviour)))
                {
                    pMod.IsNCMSMod = true;
                    mod_instance = new GameObject(pMod.Name)
                    {
                        transform =
                        {
                            parent = GameObject.Find("Services/ModLoader").transform
                        }
                    };
                    mod_instance.SetActive(false);
                    Type ncmsGlobalObjectType = mod_assembly.GetType("Mod");
                    ncmsGlobalObjectType.GetField("Info")
                        ?.SetValue(null, new Info(NCMSCompatibleLayer.GenerateNCMSMod(pMod)));
                    ncmsGlobalObjectType.GetField("GameObject")?.SetValue(null, mod_instance);
                    try
                    {
                        var mod_component = mod_instance.AddComponent(type);

                        auto_localize(mod_component);

                        AttachedModComponent mod_interface = mod_instance.AddComponent<AttachedModComponent>();
                        mod_interface.OnLoad(pMod, mod_instance);
                        mod_instance.SetActive(true);
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

                    WorldBoxMod.LoadedMods.Add(mod_instance.GetComponent<AttachedModComponent>());
                    WorldBoxMod.AllRecognizedMods[pMod] = ModState.LOADED;
                    return;
                }
#pragma warning restore CS0618

                continue;
            }

            if (type.IsSubclassOf(typeof(MonoBehaviour)))
            {
                mod_instance = new GameObject(pMod.Name)
                {
                    transform =
                    {
                        parent = GameObject.Find("Services/ModLoader").transform
                    }
                };
                mod_instance.SetActive(false);
                try
                {
                    IMod mod_interface = (IMod)mod_instance.AddComponent(type);

                    if (mod_interface == null)
                    {
                        LogService.LogError($"Some Errors happen, Mod component cannot be attached");
                        break;
                    }

                    auto_localize(mod_interface);

                    mod_interface.OnLoad(pMod, mod_instance);
                    mod_instance.SetActive(true);
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

                WorldBoxMod.LoadedMods.Add(mod_instance.GetComponent<IMod>());
                WorldBoxMod.AllRecognizedMods[pMod] = ModState.LOADED;
                return;
            }
        }

        pMod.FailReason.AppendLine("No Valid Mod Component Found");

        void auto_localize(object mod_component)
        {
            if (mod_component is ILocalizable localizable)
            {
                string locales_dir = localizable.GetLocaleFilesDirectory(pMod);
                if (Directory.Exists(locales_dir))
                {
                    var files = Directory.GetFiles(locales_dir);
                    foreach (var locale_file in files)
                    {
                        try
                        {
                            if (locale_file.EndsWith(".json"))
                            {
                                LM.LoadLocale(Path.GetFileNameWithoutExtension(locale_file), locale_file);
                            }
                            else if (locale_file.EndsWith(".csv"))
                            {
                                LM.LoadLocales(locale_file);
                            }
                        }
                        catch (FormatException e)
                        {
                            LogService.LogWarning(e.Message);
                        }
                    }

                    LM.ApplyLocale();
                }
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

    /// <summary>
    /// Compile mod at runtime.
    /// </summary>
    /// <param name="pModDeclare">Info of to be compiled mod</param>
    /// <param name="pForce">Wheather recompile mod if the mod has been compiled</param>
    /// <returns></returns>
    public static bool TryCompileModAtRuntime(ModDeclare pModDeclare, bool pForce = false)
    {
        if (pModDeclare.ModType == ModTypeEnum.BEPINEX)
        {
            ModInfoUtils.LinkBepInExModToLocalRequest(pModDeclare);
            ModInfoUtils.DealWithBepInExModLinkRequests();
            return false;
        }

        ModDependencyNode node = ModDepenSolveService.SolveModDependencyRuntime(pModDeclare);
        if (node == null)
        {
            ErrorWindow.errorMessage = $"Failed to load mod {pModDeclare.Name}:\n" +
                                       $"Failed to solve mod dependency." +
                                       $"Check Incompatible mods and dependencies, then try again.";
            ScrollWindow.get("error_with_reason").clickShow();
            return false;
        }

        bool success = compileMod(node, pForce);
        if (!success)
        {
            ErrorWindow.errorMessage = $"Failed to load mod {pModDeclare.Name}:\n" +
                                       $"Failed to compile mod." +
                                       $"Check Incompatible mods and dependencies, then try again.";
            ScrollWindow.get("error_with_reason").clickShow();
            return false;
        }

        return true;
    }

    /// <summary>
    /// Compile and load mod at runtime
    /// </summary>
    /// <param name="mod_declare">Info of to be compiled mo</param>
    /// <returns></returns>
    public static bool TryCompileAndLoadModAtRuntime(ModDeclare mod_declare)
    {
        bool actually_loaded = IsModLoaded(mod_declare.UID);

        if (actually_loaded) return false;

        bool compile_success = TryCompileModAtRuntime(mod_declare);

        if (!compile_success) return false;

        ResourcesPatch.LoadResourceFromFolder(Path.Combine(mod_declare.FolderPath, Paths.ModResourceFolderName));
        ResourcesPatch.LoadResourceFromFolder(Path.Combine(mod_declare.FolderPath,
            Paths.NCMSAdditionModResourceFolderName));

        LoadMod(mod_declare);
        return true;
    }

    /// <summary>
    /// Load information of all BepInEx plugins which is made only for Worldbox
    /// </summary>
    public static void loadInfoOfBepInExPlugins()
    {
        List<ModDeclare> bepInExMods = ModInfoUtils.recogBepInExMods();

        foreach (var mod in bepInExMods)
        {
            if (IsModLoaded(mod.UID))
            {
                LogService.LogWarning($"Repeat Mod with {mod.UID}, Only load one of them");
                continue;
            }

            VirtualMod virtualMod = new();
            virtualMod.OnLoad(mod, null);
            WorldBoxMod.LoadedMods.Add(virtualMod);
            WorldBoxMod.AllRecognizedMods[mod] = ModState.LOADED;
        }
    }
}