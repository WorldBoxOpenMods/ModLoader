using NeoModLoader.api;
using NeoModLoader.utils;

namespace NeoModLoader.services;

internal static class ModDepenSolveService
{
    private static ModDependencyGraph graph = new();

    public static void InitializeGraph(IEnumerable<ModDeclare> pMods)
    {
        graph = new ModDependencyGraph();
        foreach (ModDeclare mod in pMods)
        {
            registerOrUpdateNode(mod);
        }

        graph.RebuildEdges();
    }

    public static ModEnablePlan BuildStartupEnablePlan()
    {
        ModEnablePlan startup_plan = new();
        foreach (ModDependencyNode node in graph.nodes)
        {
            startup_plan.DesiredEnabledSnapshot[node.mod_decl.UID] = node.DesiredEnabled;
        }

        List<ModDeclare> requested_roots = graph.nodes
            .Where(node => node.DesiredEnabled)
            .Select(node => node.mod_decl)
            .OrderBy(mod => mod.UID)
            .ToList();

        foreach (ModDeclare requested_root in requested_roots)
        {
            ModEnablePlan root_plan = buildEnablePlan(new[] { requested_root });
            foreach (var snapshot_entry in root_plan.DesiredEnabledSnapshot)
            {
                if (!startup_plan.DesiredEnabledSnapshot.ContainsKey(snapshot_entry.Key))
                {
                    startup_plan.DesiredEnabledSnapshot[snapshot_entry.Key] = snapshot_entry.Value;
                }
            }

            if (root_plan.HasFailure)
            {
                if (graph.TryGetNode(requested_root.UID, out ModDependencyNode failed_root_node))
                {
                    failed_root_node.mod_decl.FailReason.Clear();
                    failed_root_node.mod_decl.FailReason.AppendLine(root_plan.FailureReason);
                    WorldBoxMod.AllRecognizedMods[failed_root_node.mod_decl] = ModState.FAILED;
                }

                continue;
            }

            startup_plan.RequestedRoots.Add(requested_root.UID);
            startup_plan.AutoEnabledNodes.UnionWith(root_plan.AutoEnabledNodes);
            startup_plan.PlannedEnabledNodes.UnionWith(root_plan.PlannedEnabledNodes);
        }

        IEnumerable<ModDependencyNode> planned_nodes =
            startup_plan.PlannedEnabledNodes.Select(uid => graph.node_map[uid]);
        startup_plan.LoadOrder.AddRange(ModDependencyUtils.SortModsCompileOrderFromDependencyTopology(planned_nodes));
        return startup_plan;
    }

    public static ModEnablePlan BuildRuntimeEnablePlan(ModDeclare pTargetMod)
    {
        registerOrUpdateNode(pTargetMod);
        graph.RebuildEdges();
        ModEnablePlan plan = buildEnablePlan(new[] { pTargetMod });
        if (plan.HasFailure && graph.TryGetNode(pTargetMod.UID, out ModDependencyNode target_node))
        {
            target_node.mod_decl.FailReason.Clear();
            target_node.mod_decl.FailReason.AppendLine(plan.FailureReason);
            WorldBoxMod.AllRecognizedMods[target_node.mod_decl] = ModState.FAILED;
        }

        return plan;
    }

    public static void CommitEnablePlan(ModEnablePlan pPlan)
    {
        foreach (string mod_uid in pPlan.PlannedEnabledNodes)
        {
            if (!graph.TryGetNode(mod_uid, out ModDependencyNode node))
            {
                continue;
            }

            node.DesiredEnabled = true;
            ModInfoUtils.setModDisabled(mod_uid, false, false);
        }
    }

    public static void RollbackEnablePlan(ModEnablePlan pPlan)
    {
        foreach (var snapshot_entry in pPlan.DesiredEnabledSnapshot)
        {
            if (graph.TryGetNode(snapshot_entry.Key, out ModDependencyNode node))
            {
                node.DesiredEnabled = snapshot_entry.Value;
            }

            ModInfoUtils.setModDisabled(snapshot_entry.Key, !snapshot_entry.Value, false);
        }
    }

    public static void MarkModLoaded(ModDeclare pModDeclare)
    {
        ModDependencyNode node = registerOrUpdateNode(pModDeclare);
        node.Loaded = true;
        graph.RebuildEdges();
    }

    public static void SetModDesiredEnabled(ModDeclare pModDeclare, bool pDesiredEnabled, bool pSave = true)
    {
        ModDeclare recognized_mod = ModInfoUtils.EnsureRecognizedMod(pModDeclare);
        ModDependencyNode node = registerOrUpdateNode(recognized_mod);
        node.DesiredEnabled = pDesiredEnabled;
        graph.RebuildEdges();
        ModInfoUtils.setModDisabled(recognized_mod.UID, !pDesiredEnabled, false);

        if (!node.Loaded)
        {
            WorldBoxMod.AllRecognizedMods[recognized_mod] = pDesiredEnabled ? ModState.FAILED : ModState.DISABLED;
        }

        if (pSave)
        {
            ModInfoUtils.SaveModRecords();
        }
    }

    public static ModDependencyNode EnsureNode(ModDeclare pModDeclare)
    {
        ModDependencyNode node = registerOrUpdateNode(pModDeclare);
        graph.RebuildEdges();
        return node;
    }

    private static ModEnablePlan buildEnablePlan(IEnumerable<ModDeclare> pRequestedRoots)
    {
        ModEnablePlan plan = new();
        foreach (ModDependencyNode node in graph.nodes)
        {
            plan.DesiredEnabledSnapshot[node.mod_decl.UID] = node.DesiredEnabled;
        }

        foreach (ModDeclare requested_root in pRequestedRoots)
        {
            ModDependencyNode root_node = registerOrUpdateNode(requested_root);
            plan.DesiredEnabledSnapshot[root_node.mod_decl.UID] = root_node.DesiredEnabled;
            plan.RequestedRoots.Add(root_node.mod_decl.UID);
        }

        Dictionary<string, int> visit_state = new();
        foreach (string requested_root_uid in plan.RequestedRoots.OrderBy(uid => uid))
        {
            if (!graph.TryGetNode(requested_root_uid, out ModDependencyNode root_node))
            {
                plan.SetFailure($"Mod {requested_root_uid} is not recognized.");
                break;
            }

            if (!visitNode(root_node, plan, visit_state))
            {
                break;
            }
        }

        if (plan.HasFailure)
        {
            return plan;
        }

        IEnumerable<ModDependencyNode> planned_nodes =
            plan.PlannedEnabledNodes.Select(uid => graph.node_map[uid]);
        plan.LoadOrder.AddRange(ModDependencyUtils.SortModsCompileOrderFromDependencyTopology(planned_nodes));
        return plan;
    }

    private static bool visitNode(ModDependencyNode pNode, ModEnablePlan pPlan, Dictionary<string, int> pVisitState)
    {
        if (pVisitState.TryGetValue(pNode.mod_decl.UID, out int state))
        {
            if (state == 2)
            {
                return true;
            }

            string cycle_reason = ModDependencyUtils.BuildCircularDependencyMessage(pNode.mod_decl);
            failPlan(pPlan, pNode, cycle_reason);
            return false;
        }

        pVisitState[pNode.mod_decl.UID] = 1;

        List<string> incompatible_mods = new();
        foreach (string incompatible_uid in pNode.mod_decl.IncompatibleWith)
        {
            if (!graph.TryGetNode(incompatible_uid, out ModDependencyNode incompatible_node))
            {
                continue;
            }

            if (incompatible_node.Loaded ||
                incompatible_node.DesiredEnabled ||
                pPlan.PlannedEnabledNodes.Contains(incompatible_uid) ||
                pPlan.RequestedRoots.Contains(incompatible_uid) ||
                isVisiting(incompatible_uid, pVisitState))
            {
                incompatible_mods.Add(incompatible_uid);
            }
        }

        if (incompatible_mods.Count > 0)
        {
            failPlan(pPlan, pNode, ModDependencyUtils.BuildIncompatibleModMessage(pNode.mod_decl, incompatible_mods));
            pVisitState.Remove(pNode.mod_decl.UID);
            return false;
        }

        List<string> missing_dependencies = new();
        foreach (string dependency_uid in pNode.mod_decl.Dependencies)
        {
            if (!tryGetOrRegisterDependency(dependency_uid, pPlan, out ModDependencyNode dependency_node))
            {
                missing_dependencies.Add(dependency_uid);
                continue;
            }

            if (!visitNode(dependency_node, pPlan, pVisitState))
            {
                pVisitState.Remove(pNode.mod_decl.UID);
                return false;
            }
        }

        if (missing_dependencies.Count > 0)
        {
            failPlan(pPlan, pNode, ModDependencyUtils.BuildMissingDependencyMessage(pNode.mod_decl, missing_dependencies));
            pVisitState.Remove(pNode.mod_decl.UID);
            return false;
        }

        pVisitState[pNode.mod_decl.UID] = 2;
        pPlan.PlannedEnabledNodes.Add(pNode.mod_decl.UID);
        if (!pPlan.RequestedRoots.Contains(pNode.mod_decl.UID) &&
            pPlan.DesiredEnabledSnapshot.TryGetValue(pNode.mod_decl.UID, out bool desired_enabled_before) &&
            !desired_enabled_before)
        {
            pPlan.AutoEnabledNodes.Add(pNode.mod_decl.UID);
        }

        return true;
    }

    private static bool tryGetOrRegisterDependency(string pDependencyUid, ModEnablePlan pPlan,
        out ModDependencyNode pDependencyNode)
    {
        if (graph.TryGetNode(pDependencyUid, out pDependencyNode))
        {
            return true;
        }

        if (!ModInfoUtils.TryFindMod(pDependencyUid, out ModDeclare dependency_mod))
        {
            pDependencyNode = null;
            return false;
        }

        pDependencyNode = registerOrUpdateNode(dependency_mod);
        graph.RebuildEdges();
        pPlan.DesiredEnabledSnapshot[pDependencyNode.mod_decl.UID] = pDependencyNode.DesiredEnabled;
        return true;
    }

    private static ModDependencyNode registerOrUpdateNode(ModDeclare pModDeclare)
    {
        ModDeclare recognized_mod = ModInfoUtils.EnsureRecognizedMod(pModDeclare);
        bool loaded = ModCompileLoadService.IsModLoaded(recognized_mod.UID);
        ModDependencyNode node = graph.RegisterMod(recognized_mod, !ModInfoUtils.isModDisabled(recognized_mod.UID), loaded);
        return node;
    }

    private static bool isVisiting(string pModUid, Dictionary<string, int> pVisitState)
    {
        return pVisitState.TryGetValue(pModUid, out int state) && state == 1;
    }

    private static void failPlan(ModEnablePlan pPlan, ModDependencyNode pNode, string pReason)
    {
        pPlan.SetFailure(pReason);
        pNode.mod_decl.FailReason.Clear();
        pNode.mod_decl.FailReason.AppendLine(pReason);
        WorldBoxMod.AllRecognizedMods[pNode.mod_decl] = ModState.FAILED;
        LogService.LogError(pReason);
    }
}
