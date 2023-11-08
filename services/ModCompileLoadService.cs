#define COMPILE_METHOD_ROSLYN

#if COMPILE_METHOD_ROSLYN
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Emit;
    using Microsoft.CodeAnalysis.Text;
    using NeoModLoader.ncms_compatible_layer;
#else
    using System.CodeDom.Compiler;
#endif
using System.Reflection;
using System.Text;
using ModDeclaration;
using NeoModLoader.api;
using NeoModLoader.constants;
using NeoModLoader.utils;
using UnityEngine;

namespace NeoModLoader.services;

public static class ModCompileLoadService
{
    private static string[] _default_ref_path = null!;
    private static readonly Dictionary<string, string> mod_inc_path = new();


#if COMPILE_METHOD_ROSLYN
    private static MetadataReference[] _default_ref = null!;
    private static readonly Dictionary<string, MetadataReference> mod_ref = new();
    private static bool compileMod(api.ModDeclare pModDecl, IEnumerable<MetadataReference> pDefaultInc,
        string[] pAddInc, Dictionary<string, MetadataReference> pModInc)
    {
        if (!ModInfoUtils.isModNeedRecompile(pModDecl.UUID, pModDecl.FolderPath))
        {
            return true;
        }
        BenchUtils.Start($"{pModDecl.UUID}");
        var syntaxTrees = new List<SyntaxTree>();
        var code_files = SystemUtils.SearchFileRecursive(pModDecl.FolderPath,
            file_name => file_name.EndsWith(".cs") && !file_name.StartsWith("."),
            dir_name => !dir_name.StartsWith(".") && !Paths.IgnoreSearchDirectories.Contains(dir_name));
        var embeded_resources = new List<ResourceDescription>();
        
        bool is_ncms_mod = false;
        var parse_option = new CSharpParseOptions(LanguageVersion.Latest);
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
                    var resource_name = $"{pModDecl.Name}.Resources.{relative_path.Replace('\\', '.').Replace('/', '.')}";
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

        List<MetadataReference> list = pDefaultInc.ToList();
        foreach(var inc in pAddInc)
        {
            MetadataReference reference = MetadataReference.CreateFromFile(inc);
            list.Add(reference);
            if (reference == null)
            {
                LogService.LogError($"{pModDecl.UUID}'s additional ref of {inc} instance is null");
                return false;
            }
        }

        foreach (var depen in pModDecl.Dependencies)
        {
            if (!pModInc.ContainsKey(depen))
            {
                LogService.LogError($"{pModDecl.UUID} miss dependency {depen}");
                return false;
            }
            list.Add(pModInc[depen]);
            if (pModInc[depen] == null)
            {
                LogService.LogError($"{pModDecl.UUID}'s optional ref of {depen} instance is null");
                return false;
            }
        }
    

        foreach (var option_depen in pModDecl.OptionalDependencies)
        {
            if (pModInc.TryGetValue(option_depen, out var value))
            {
                list.Add(value);
                if (value == null)
                { 
                    LogService.LogError($"{pModDecl.UUID}'s optional ref of {option_depen} instance is null");
                    return false;
                }
            }
        }

        var compilation = CSharpCompilation.Create(
            $"{pModDecl.UUID}",
            syntaxTrees,
            list,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true)
        );
        
        using MemoryStream dllms = new MemoryStream();
        using MemoryStream pdbms = new MemoryStream();

        string dll_path = Path.Combine(Paths.CompiledModsPath, $"{pModDecl.UUID}.dll");
        string pdb_path = Path.Combine(Paths.CompiledModsPath, $"{pModDecl.UUID}.pdb");

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
                if(diagnostic.Severity != DiagnosticSeverity.Error) continue;
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

        float time = BenchUtils.End($"{pModDecl.UUID}");
        if (time > 0)
        {
            LogService.LogInfo($"Time used to compile {pModDecl.UUID}: {time}");
        }
        return true;
    }
#else
    private static bool compileMod(ModDeclare pModDecl, string[] pDefaultInc, string[] pAddIncPath, Dictionary<string, string> pModIncPath)
    {
        if (!ModInfoUtils.isModNeedRecompile(pModDecl.UUID, pModDecl.FolderPath))
        {
            return true;
        }

        CodeDomProvider provider = CodeDomProvider.CreateProvider("cs");
        
        CompilerParameters parameters = new CompilerParameters();
        parameters.GenerateExecutable = false;
        parameters.GenerateInMemory = false;
        parameters.OutputAssembly = Path.Combine(Paths.CompiledModsPath, $"{pModDecl.UUID}.dll");
        
        /* Load referenced assemblies */
        parameters.ReferencedAssemblies.AddRange(pDefaultInc);
        parameters.ReferencedAssemblies.AddRange(pAddIncPath);
        foreach (var depen in pModDecl.Dependencies)
        {
            // It is ensured that all dependencies are compiled before compiling
            // Consider depend mod compile failed.
            if (!pModIncPath.ContainsKey(depen))
            {
                return false;
            }
            parameters.ReferencedAssemblies.Add(pModIncPath[depen]);
        }

        foreach (var opt_depen in pModDecl.OptionalDependencies)
        {
            if (!pModIncPath.ContainsKey(opt_depen))
            {
                continue;
            }
            parameters.ReferencedAssemblies.Add(pModIncPath[opt_depen]);
        }
        
        /* Find code files to compile */
        var code_files = Directory.GetFiles(pModDecl.FolderPath, "*.cs", SearchOption.AllDirectories);
        
        CompilerResults results = provider.CompileAssemblyFromFile(parameters, code_files);
        
        if(results.Errors.HasErrors)
        {
            foreach (CompilerError error in results.Errors)
            {
                Console.WriteLine(error.ErrorText);
            }
            return false;
        }

        return true;
    }
#endif
    public static void prepareCompile(List<ModDependencyNode> pModNodes)
    {
        foreach (var mod_node in pModNodes)
        {
            mod_inc_path.Add(mod_node.mod_decl.UUID,
                Path.Combine(Paths.CompiledModsPath, $"{mod_node.mod_decl.UUID}.dll"));
        }

        var default_ref_path_list = new List<string>();
        default_ref_path_list.AddRange(Directory.GetFiles(Paths.ManagedPath, "*.dll"));
        default_ref_path_list.AddRange(Directory.GetFiles(Paths.NMLAssembliesPath, "*.dll"));
        default_ref_path_list.Add(Paths.NMLModPath);
        _default_ref_path = default_ref_path_list.ToArray();

        #if COMPILE_METHOD_ROSLYN
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
        #endif
    }

    public static void prepareCompileRuntime(ModDependencyNode pModNode)
    {
        mod_inc_path.Add(pModNode.mod_decl.UUID,
            Path.Combine(Paths.CompiledModsPath, $"{pModNode.mod_decl.UUID}.dll"));
    }
    public static bool compileMod(ModDependencyNode pModNode)
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
        #if COMPILE_METHOD_ROSLYN
        compile_result = 
            compileMod(pModNode.mod_decl, _default_ref, 
                add_inc_path, mod_ref
            );
        if (compile_result)
        {
            mod_ref.Add(pModNode.mod_decl.UUID,
                MetadataReference.CreateFromFile(Path.Combine(Paths.CompiledModsPath,
                    $"{pModNode.mod_decl.UUID}.dll")));
        }
        #else
        compile_result = 
            compileMod(pModNode.mod_decl, _default_ref_path, 
                add_inc_path, mod_inc_path
            );
        #endif

        if (!compile_result)
        {
            mod_inc_path.Remove(pModNode.mod_decl.UUID);
        }
        else
        {
            ModInfoUtils.updateModCompileTimestamp(pModNode.mod_decl.UUID);
        }

        return compile_result;
    }

    public static void loadMods(List<api.ModDeclare> mods_to_load)
    {
        // It can be sure that all mods are compiled successfully.
        foreach (var mod in mods_to_load)
        {
            LoadMod(mod);
        }
    }

    public static void LoadMod(ModDeclare pMod)
    {
        Assembly mod_assembly = Assembly.Load(
                File.ReadAllBytes(Path.Combine(Paths.CompiledModsPath, $"{pMod.UUID}.dll")), 
                File.ReadAllBytes(Path.Combine(Paths.CompiledModsPath, $"{pMod.UUID}.pdb"))
                );
        bool type_found = false;
        foreach(var type in mod_assembly.GetTypes())
        {
            if (type.GetInterface(nameof(IMod)) == null)
            {
                // Check if it is a NCMS Mod
                if (Attribute.GetCustomAttribute(type, typeof(NCMS.ModEntry)) != null && type.IsSubclassOf(typeof(MonoBehaviour)))
                {
                    var mod_instance = new GameObject(pMod.Name)
                    {
                        transform =
                        {
                            parent = GameObject.Find("Services/ModLoader").transform
                        }
                    };
                    mod_instance.SetActive(false);
                    Type ncmsGlobalObjectType = mod_assembly.GetType("Mod");
                    ncmsGlobalObjectType.GetField("Info")?.SetValue(null, new Info(NCMSCompatibleLayer.GenerateNCMSMod(pMod)));
                    ncmsGlobalObjectType.GetField("GameObject")?.SetValue(null, mod_instance);
                    mod_instance.AddComponent(type);
                    try
                    {
                        AttachedModComponent mod_interface = mod_instance.AddComponent<AttachedModComponent>();
                        mod_interface.OnLoad(pMod, mod_instance);
                        mod_instance.SetActive(true);
                    }
                    catch (Exception e)
                    {
                        LogService.LogError(e.Message);
                        if (e.StackTrace != null) LogService.LogError(e.StackTrace);
                        
                        mod_instance.SetActive(false);
                        LogService.LogError($"{pMod.Name} has been disabled due to an error. Please check the log for details.");
                        
                        continue;
                    }
                    WorldBoxMod.LoadedMods.Add(mod_instance.GetComponent<AttachedModComponent>());
                }
                continue;
            }
            if (type.IsSubclassOf(typeof(MonoBehaviour)))
            {
                type_found = true;
                var mod_instance = new GameObject(pMod.Name)
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

                    mod_interface.OnLoad(pMod, mod_instance);
                    mod_instance.SetActive(true);
                }
                catch (Exception e)
                {
                    LogService.LogError(e.Message);
                    if (e.StackTrace != null) LogService.LogError(e.StackTrace);
                    
                    mod_instance.SetActive(false);
                    LogService.LogError($"{pMod.Name} has been disabled due to an error. Please check the log for details.");
                    
                    continue;
                }
                WorldBoxMod.LoadedMods.Add(mod_instance.GetComponent<IMod>());
                break;
            }
        }
        
        if (!type_found)
        {
            LogService.LogWarning($"Cannot find Implement of IMod in {pMod.UUID}, this mod will be executed as a lib mod?");
        }
    }
    public static bool IsModLoaded(string uuid)
    {
        foreach (var mod in WorldBoxMod.LoadedMods)
        {
            if (mod.GetDeclaration().UUID == uuid)
            {
                return true;
            }
        }

        return false;
    }

    public static void loadInfoOfBepInExPlugins()
    {
        List<ModDeclare> bepInExMods = ModInfoUtils.recogBepInExMods();
        
        foreach (var mod in bepInExMods)
        {
            if (IsModLoaded(mod.UUID))
            {
                LogService.LogWarning($"Repeat Mod with {mod.UUID}, Only load one of them");
                continue;
            }
            VirtualMod virtualMod = new();
            virtualMod.OnLoad(mod, null);
            WorldBoxMod.LoadedMods.Add(virtualMod);
        }
        
        //AppDomain.Unload(inspect_domain);
    }
}