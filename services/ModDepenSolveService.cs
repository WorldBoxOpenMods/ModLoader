using NeoModLoader.api;
using NeoModLoader.utils;

namespace NeoModLoader.services;

internal static class ModDepenSolveService
{
    private static ModDependencyGraph graph;

    public static List<ModDependencyNode> SolveModDependencies(List<api.ModDeclare> mods)
    {
        graph = new ModDependencyGraph(mods);

        mods.Clear();
        // Remove circle dependencies, make sure more mods load.
        // and log error/pop up warning if there is any. 
        ModDependencyUtils.RemoveCircleDependencies(graph);

        // Remove mods without required dependencies. 
        // and log error/pop up warning if there is any.
        ModDependencyUtils.RemoveModsWithoutRequiredDependencies(graph);

        // Sort mods compile order from dependency topology.
        var ret = ModDependencyUtils.SortModsCompileOrderFromDependencyTopology(graph);
        return ret;
    }

    /// <summary>
    /// Get a mod's dependency node at runtime.
    /// </summary>
    /// <param name="mod"></param>
    /// <returns>The dependency node</returns>
    public static ModDependencyNode SolveModDependencyRuntime(ModDeclare mod)
    {
        return ModDependencyUtils.TryToAppendMod(graph, mod);
    }
}