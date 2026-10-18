using NeoModLoader.api;
using NeoModLoader.constants;
using NeoModLoader.utils;
using UnityEngine;

namespace NeoModLoader.services;

/// <summary>
///     Whether a dependency of a mod is available.
/// </summary>
internal enum DependencyState
{
    Loaded,

    /// <summary>
    ///     Recognized by NML but not loaded in this session.
    /// </summary>
    Installed,
    Missing
}

/// <summary>
///     Reads NML's recognized mods for Manage Mods and applies the user's choices through the existing
///     enable and disable paths.
/// </summary>
internal static class ModManagerService
{
    public static List<InstalledMod> GetInstalledMods()
    {
        return WorldBoxMod.AllRecognizedMods.Select(pair => Describe(pair.Key, pair.Value)).ToList();
    }

    public static InstalledMod Describe(ModDeclare pMod)
    {
        return Describe(pMod, WorldBoxMod.AllRecognizedMods[pMod]);
    }

    private static InstalledMod Describe(ModDeclare pMod, ModState pState)
    {
        bool disabled = ModInfoUtils.isModDisabled(pMod.UID);
        bool missing_dependency = pMod.Dependencies.Any(uid => GetDependencyState(uid) == DependencyState.Missing);
        return new InstalledMod
        {
            Declaration = pMod,
            Name = pMod.GetDisplayName() ?? "",
            Author = pMod.GetDisplayAuthor() ?? "",
            Enabled = !disabled,
            Status = InstalledMod.ResolveStatus(pState, disabled, missing_dependency,
                ModDownloadService.NeedsRestart(pMod.UID), ModDownloadService.HasUpdate(pMod)),
            Source = InstalledMod.DetectSource(pMod.ModType, pMod.IsWorkshopLoaded, pMod.FolderPath, Paths.ModsPath,
                Paths.NativeModsPath),
            LastChanged = GetLastChanged(pMod.FolderPath)
        };
    }

    public static DependencyState GetDependencyState(string pModUid)
    {
        if (!ModInfoUtils.TryGetRecognizedMod(pModUid, out ModDeclare dependency)) return DependencyState.Missing;
        return WorldBoxMod.AllRecognizedMods[dependency] == ModState.LOADED
            ? DependencyState.Loaded
            : DependencyState.Installed;
    }

    public static Sprite GetIcon(ModDeclare pMod)
    {
        if (!string.IsNullOrEmpty(pMod.IconPath))
        {
            string icon_path = Path.Combine(pMod.FolderPath, pMod.IconPath);
            Sprite icon = File.Exists(icon_path) ? SpriteLoadUtils.LoadSingleSprite(icon_path) : null;
            if (icon != null) return icon;
        }

        return InternalResourcesGetter.GetIcon();
    }

    /// <summary>
    ///     Enable a disabled mod (compiled and loaded now, as before) or disable an enabled one (a loaded mod stays
    ///     loaded until the next start).
    /// </summary>
    public static void Toggle(ModDeclare pMod)
    {
        if (ModInfoUtils.isModDisabled(pMod.UID))
        {
            ModCompileLoadService.TryEnableMod(pMod);
        }
        else
        {
            ModCompileLoadService.DisableMod(pMod);
        }
    }

    public static int CountDisabled()
    {
        return WorldBoxMod.AllRecognizedMods.Keys.Count(mod => ModInfoUtils.isModDisabled(mod.UID));
    }

    public static int CountEnabled()
    {
        return WorldBoxMod.AllRecognizedMods.Keys.Count(mod => !ModInfoUtils.isModDisabled(mod.UID));
    }

    /// <summary>
    ///     Save every disabled mod as enabled. Nothing is compiled or loaded now; they load at the next start.
    /// </summary>
    public static void EnableAll()
    {
        foreach (ModDeclare mod in WorldBoxMod.AllRecognizedMods.Keys.ToList())
        {
            if (ModInfoUtils.isModDisabled(mod.UID)) ModInfoUtils.setModDisabled(mod.UID, false, false);
        }

        ModInfoUtils.SaveModRecords();
    }

    /// <summary>
    ///     Save every mod as disabled. Loaded mods stay loaded until the next start.
    /// </summary>
    public static void DisableAll()
    {
        foreach (ModDeclare mod in WorldBoxMod.AllRecognizedMods.Keys.ToList())
        {
            if (!ModInfoUtils.isModDisabled(mod.UID)) ModDepenSolveService.SetModDesiredEnabled(mod, false, false);
        }

        ModInfoUtils.SaveModRecords();
    }

    /// <summary>
    ///     The config of a loaded mod, or null when it has none. Every <see cref="BasicMod{T}" /> is
    ///     <see cref="IConfigurable" />, so a config without any item counts as none.
    /// </summary>
    public static ModConfig GetConfig(ModDeclare pMod)
    {
        if (!WorldBoxMod.TryGetLoadedMod(pMod, out IMod mod) || mod is not IConfigurable configurable) return null;

        ModConfig config = configurable.GetConfig();
        return config != null && config._config.Values.Any(group => group.Count > 0) ? config : null;
    }

    /// <summary>
    ///     Hot reload is a developer tool: offered only in the game's developer mode, as before.
    /// </summary>
    public static bool CanReload(ModDeclare pMod)
    {
        return Config.isEditor && ModReloadService.CanReload(pMod);
    }

    /// <summary>
    ///     Whether Manage Mods offers to delete a mod: it is not loaded, and either sits in the Mods folder or
    ///     comes from the Steam Workshop. BepInEx plugins and mods in other folders are left alone.
    /// </summary>
    public static bool CanDelete(InstalledMod pMod)
    {
        if (pMod.Status is not (InstalledModStatus.Disabled or InstalledModStatus.Failed)) return false;
        return pMod.Source switch
        {
            ModSource.ModsFolder => IsModsFolderEntry(pMod.Declaration.FolderPath),
            ModSource.Workshop => ModDownloadService.CheckSteam() == CatalogStatus.Ready,
            _ => false
        };
    }

    /// <summary>
    ///     Delete a mod's folder from the Mods folder, or unsubscribe from a Workshop mod so Steam deletes it.
    /// </summary>
    /// <returns>False when the files could not be deleted or Steam refused</returns>
    public static async Task<bool> DeleteAsync(InstalledMod pMod)
    {
        ModDeclare declaration = pMod.Declaration;
        if (pMod.Source == ModSource.Workshop) return await ModDownloadService.UnsubscribeAsync(declaration);
        if (!IsModsFolderEntry(declaration.FolderPath)) return false;

        try
        {
            Directory.Delete(declaration.FolderPath, true);
        }
        catch (Exception e)
        {
            LogService.LogWarning($"Failed to delete mod {declaration.Name} at {declaration.FolderPath}: {e.Message}");
            return false;
        }

        WorldBoxMod.AllRecognizedMods.Remove(declaration);
        ModDepenSolveService.InitializeGraph(WorldBoxMod.AllRecognizedMods.Keys);
        return true;
    }

    /// <summary>
    ///     Only a folder directly inside the Mods folder is ever deleted.
    /// </summary>
    private static bool IsModsFolderEntry(string pFolderPath)
    {
        if (string.IsNullOrEmpty(pFolderPath) || !Directory.Exists(pFolderPath)) return false;
        char[] separators = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        string parent = Path.GetDirectoryName(Path.GetFullPath(pFolderPath).TrimEnd(separators));
        return string.Equals(parent, Path.GetFullPath(Paths.ModsPath).TrimEnd(separators),
            StringComparison.OrdinalIgnoreCase);
    }

    public static void OpenFolder(string pFolderPath)
    {
        if (!Directory.Exists(pFolderPath))
        {
            LogService.LogWarning($"Cannot open missing folder {pFolderPath}");
            return;
        }

        Application.OpenURL(pFolderPath);
    }

    private static DateTime GetLastChanged(string pFolderPath)
    {
        try
        {
            DateTime folder = Directory.GetLastWriteTimeUtc(pFolderPath);
            DateTime mod_json = File.GetLastWriteTimeUtc(Path.Combine(pFolderPath, Paths.ModDeclarationFileName));
            return folder > mod_json ? folder : mod_json;
        }
        catch (Exception e)
        {
            LogService.LogWarning($"Cannot read the last write time of {pFolderPath}: {e.Message}");
            return DateTime.MinValue;
        }
    }
}
