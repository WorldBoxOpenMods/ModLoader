using NeoModLoader.api;
using NeoModLoader.utils;

namespace NeoModLoader.services;

public static class ModDepenSolveService
{
    public static List<ModDependencyNode> SolveModDependencies(List<api.ModDeclare> mods)
    {
        ModDependencyGraph graph = new ModDependencyGraph(mods);
        
        mods.Clear();
        // Remove circle dependencies, make sure more mods load.
        // and log error/pop up warning if there is any. 
        ModDependencyUtils.RemoveCircleDependencies(graph);

        // Remove mods without required dependencies. 
        // and log error/pop up warning if there is any.
        ModDependencyUtils.RemoveModsWithoutRequiredDependencies(graph);

        // Sort mods compile order from dependency topology.
        var ret = ModDependencyUtils.SortModsCompileOrderFromDependencyTopology(graph);
        ret.Reverse();
        return ret; 
    }
}