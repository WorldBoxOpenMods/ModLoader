using System.Text;
using NeoModLoader.api;

namespace NeoModLoader.utils;

/// <summary>
///     Dependency node for mod dependency graph.
/// </summary>
public class ModDependencyNode
{
    /// <summary>
    ///     Mods that depend on this mod.
    /// </summary>
    public HashSet<ModDependencyNode> depend_by;

    /// <summary>
    ///     Mods that this mod depends on.
    /// </summary>
    public HashSet<ModDependencyNode> depend_on;

    /// <summary>
    ///     Necessary mods that this mod depends on.
    /// </summary>
    public HashSet<ModDependencyNode> necessary_depend_on;

    /// <summary>
    ///     Whether this mod should be enabled in future sessions.
    /// </summary>
    public bool DesiredEnabled { get; set; }

    /// <summary>
    ///     Whether this mod is loaded in the current session.
    /// </summary>
    public bool Loaded { get; set; }

    /// <summary>
    ///     Create a new mod dependency node for a mod declaration
    /// </summary>
    /// <param name="pModDecl"></param>
    public ModDependencyNode(ModDeclare pModDecl)
    {
        mod_decl = pModDecl;
        necessary_depend_on = new HashSet<ModDependencyNode>();
        depend_on = new HashSet<ModDependencyNode>();
        depend_by = new HashSet<ModDependencyNode>();
    }

    /// <summary>
    ///     Related mod declaration
    /// </summary>
    public ModDeclare mod_decl { get; private set; }

    public void UpdateDeclaration(ModDeclare pModDecl)
    {
        mod_decl = pModDecl;
    }

    /// <summary>
    ///     Get all additional assembly references that this mod depends on.
    /// </summary>
    /// <returns></returns>
    public List<string> GetAdditionReferences(bool recursive = true)
    {
        List<string> references = new();
        HashSet<string> visited = new();
        collectAdditionReferences(this, recursive, references, visited);
        return references;
    }

    private static void collectAdditionReferences(ModDependencyNode pNode, bool pRecursive, List<string> pReferences,
        HashSet<string> pVisited)
    {
        if (!pVisited.Add(pNode.mod_decl.UID))
        {
            return;
        }

        var assemblies_path = Path.Combine(pNode.mod_decl.FolderPath, "Assemblies");
        if (Directory.Exists(assemblies_path))
        {
            pReferences.AddRange(Directory.GetFiles(assemblies_path, "*.dll"));
        }

        if (!pRecursive)
        {
            return;
        }

        foreach (ModDependencyNode dependency in pNode.depend_on)
        {
            collectAdditionReferences(dependency, true, pReferences, pVisited);
        }
    }
}

/// <summary>
///     This class is used to solve mod dependency. And generate mod loading order.
/// </summary>
public class ModDependencyGraph
{
    /// <summary>
    ///     All nodes in the graph.
    /// </summary>
    public HashSet<ModDependencyNode> nodes;

    /// <summary>
    ///     Fast lookup by mod UID.
    /// </summary>
    public Dictionary<string, ModDependencyNode> node_map;

    public ModDependencyGraph()
    {
        nodes = new();
        node_map = new();
    }

    /// <summary>
    ///     Create a new mod dependency graph from a collection of mod declarations.
    /// </summary>
    /// <param name="mods"></param>
    public ModDependencyGraph(ICollection<ModDeclare> mods) : this()
    {
        foreach (ModDeclare mod in mods)
        {
            RegisterMod(mod);
        }

        RebuildEdges();
    }

    public ModDependencyNode RegisterMod(ModDeclare pModDecl, bool pDesiredEnabled = false, bool pLoaded = false)
    {
        if (node_map.TryGetValue(pModDecl.UID, out ModDependencyNode node))
        {
            node.UpdateDeclaration(pModDecl);
            node.DesiredEnabled = pDesiredEnabled;
            node.Loaded = pLoaded;
            return node;
        }

        node = new ModDependencyNode(pModDecl)
        {
            DesiredEnabled = pDesiredEnabled,
            Loaded = pLoaded
        };
        node_map.Add(pModDecl.UID, node);
        nodes.Add(node);
        return node;
    }

    public bool TryGetNode(string pModUID, out ModDependencyNode pNode)
    {
        return node_map.TryGetValue(pModUID, out pNode);
    }

    public void RebuildEdges()
    {
        foreach (ModDependencyNode node in nodes)
        {
            node.necessary_depend_on.Clear();
            node.depend_on.Clear();
            node.depend_by.Clear();
        }

        foreach (ModDependencyNode node in nodes)
        {
            foreach (string dependency in node.mod_decl.Dependencies)
            {
                if (!node_map.TryGetValue(dependency, out ModDependencyNode dependency_node))
                {
                    continue;
                }

                dependency_node.depend_by.Add(node);
                node.necessary_depend_on.Add(dependency_node);
            }

            node.depend_on.UnionWith(node.necessary_depend_on);

            foreach (string optional_dependency in node.mod_decl.OptionalDependencies)
            {
                if (!node_map.TryGetValue(optional_dependency, out ModDependencyNode dependency_node))
                {
                    continue;
                }

                dependency_node.depend_by.Add(node);
                node.depend_on.Add(dependency_node);
            }
        }
    }
}

internal static class ModDependencyUtils
{
    public static string ParseDepenNameToPreprocessSymbol(string pDepenName)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var ch in pDepenName)
        {
            sb.Append((!char.IsLetterOrDigit(ch) && (int)ch <= 256) ? '_' : char.ToUpper(ch));
        }

        return sb.ToString();
    }

    public static string BuildMissingDependencyMessage(ModDeclare pModDeclare, IEnumerable<string> pMissingDependencies)
    {
        StringBuilder sb = new();
        sb.AppendLine($"Mod {pModDeclare.UID} has missing dependencies:");
        foreach (string dependency in pMissingDependencies.Distinct())
        {
            sb.AppendLine($"    {dependency}");
        }

        sb.Append("Grab the missing dependency mod and try again.");
        return sb.ToString();
    }

    public static string BuildIncompatibleModMessage(ModDeclare pModDeclare, IEnumerable<string> pIncompatibleMods)
    {
        StringBuilder sb = new();
        sb.AppendLine($"Mod {pModDeclare.UID} is incompatible with mods:");
        foreach (string incompatible_mod in pIncompatibleMods.Distinct())
        {
            sb.AppendLine($"    {incompatible_mod}");
        }

        return sb.ToString().TrimEnd();
    }

    public static string BuildCircularDependencyMessage(ModDeclare pModDeclare)
    {
        return $"Mod {pModDeclare.UID} has circular dependencies.";
    }

    public static List<ModDependencyNode> SortModsCompileOrderFromDependencyTopology(IEnumerable<ModDependencyNode> pNodes)
    {
        HashSet<ModDependencyNode> selected_nodes = new(pNodes);
        Dictionary<ModDependencyNode, int> node_in_degree = new();
        Queue<ModDependencyNode> queue = new();
        foreach (ModDependencyNode node in selected_nodes.OrderBy(node => node.mod_decl.UID))
        {
            int in_degree = node.depend_on.Count(dependency => selected_nodes.Contains(dependency));
            node_in_degree.Add(node, in_degree);
            if (in_degree == 0)
            {
                queue.Enqueue(node);
            }
        }

        List<ModDependencyNode> mods = new();
        while (queue.Count > 0)
        {
            ModDependencyNode curr_node = queue.Dequeue();
            mods.Add(curr_node);

            foreach (ModDependencyNode depend_on_node in curr_node.depend_by.OrderBy(node => node.mod_decl.UID))
            {
                if (!selected_nodes.Contains(depend_on_node))
                {
                    continue;
                }

                if (!node_in_degree.ContainsKey(depend_on_node))
                {
                    continue;
                }

                node_in_degree[depend_on_node]--;
                if (node_in_degree[depend_on_node] == 0)
                {
                    queue.Enqueue(depend_on_node);
                }
            }
        }

        if (mods.Count == selected_nodes.Count)
        {
            return mods;
        }

        foreach (ModDependencyNode remaining_node in selected_nodes
                     .Where(node => !mods.Contains(node))
                     .OrderBy(node => node.mod_decl.UID))
        {
            mods.Add(remaining_node);
        }

        return mods;
    }
}
