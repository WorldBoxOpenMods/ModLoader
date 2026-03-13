using NeoModLoader.api;
using NeoModLoader.constants;
using NeoModLoader.General;
using NeoModLoader.utils;
using NeoModLoader.utils.Builders;

namespace NeoModLoader.services;

/// <summary>
/// Coordinates mod hot-reload workflows.
/// </summary>
public static class ModReloadService
{
    /// <summary>
    /// Checks whether a loaded mod supports hot-reload.
    /// </summary>
    public static bool CanReload(ModDeclare pModDeclare)
    {
        return WorldBoxMod.TryGetLoadedMod(pModDeclare, out var mod) && mod is IReloadable;
    }

    /// <summary>
    /// Recompiles and patches a reloadable mod.
    /// </summary>
    public static bool HotfixMethods(IReloadable pMod, ModDeclare pModDeclare)
    {
        if (!ModReloadUtils.Prepare(pMod, pModDeclare)) return false;
        if (!ModReloadUtils.CompileNew()) return false;
        if (!ModReloadUtils.PatchHotfixMethodsNT()) return false;
        return true;
    }

    /// <summary>
    /// Recompiles, patches and invokes the reload callback of a mod.
    /// </summary>
    public static bool ReloadMod(ModDeclare pModDeclare)
    {
        return WorldBoxMod.TryGetLoadedMod(pModDeclare, out var mod) &&
               mod is IReloadable reloadable &&
               ReloadMod(reloadable, pModDeclare);
    }

    /// <summary>
    /// Recompiles, patches and invokes the reload callback of a mod.
    /// </summary>
    public static bool ReloadMod(IReloadable pMod, ModDeclare pModDeclare)
    {
        if (!HotfixMethods(pMod, pModDeclare)) return false;
        if (pMod is IMod mod)
        {
            if (!ReloadResources(mod)) return false;
            ReloadLocales(mod);
        }
        return ModReloadUtils.Reload();
    }

    /// <summary>
    /// Rebuilds mod resources from disk.
    /// </summary>
    public static bool ReloadResources(IMod pMod)
    {
        MasterBuilder Builder = new();
        ResourcesPatch.LoadResourceFromFolder(Path.Combine(pMod.GetDeclaration().FolderPath,
            Paths.ModResourceFolderName), out List<Builder> builders);
        ResourcesPatch.LoadResourceFromFolder(Path.Combine(pMod.GetDeclaration().FolderPath,
            Paths.NCMSAdditionModResourceFolderName), out List<Builder> builders2);
        Builder.AddBuilders(builders);
        Builder.AddBuilders(builders2);
        Builder.BuildAll();
        return true;
    }

    /// <summary>
    /// Reloads locale files from disk and applies them.
    /// </summary>
    public static void ReloadLocales(IMod pMod)
    {
        ModCompileLoadService.LoadLocales(pMod, pMod.GetDeclaration(), true, true);
    }
}
