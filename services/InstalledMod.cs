using NeoModLoader.api;

namespace NeoModLoader.services;

/// <summary>
///     State of an installed mod as shown in Manage Mods.
/// </summary>
internal enum InstalledModStatus
{
    Enabled,
    Disabled,
    Failed,
    MissingDependency,

    /// <summary>
    ///     Steam has a newer version of this Workshop mod.
    /// </summary>
    UpdateAvailable,

    /// <summary>
    ///     What is loaded now differs from the saved choice or the files on disk, until the next start.
    /// </summary>
    RestartRequired
}

/// <summary>
///     Where a mod was found.
/// </summary>
internal enum ModSource
{
    Workshop,
    ModsFolder,
    NativeModsFolder,
    BepInEx,
    Other
}

/// <summary>
///     Snapshot of one recognized mod, taken when Manage Mods refreshes.
/// </summary>
internal sealed class InstalledMod
{
    public ModDeclare Declaration { get; set; }

    /// <summary>
    ///     Localized display name.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    ///     Localized display author.
    /// </summary>
    public string Author { get; set; } = "";

    public InstalledModStatus Status { get; set; }

    /// <summary>
    ///     The user's saved choice, which can differ from what is loaded until the next start.
    /// </summary>
    public bool Enabled { get; set; }

    public ModSource Source { get; set; }

    /// <summary>
    ///     When the mod's folder or mod.json was last written on this computer (installed, updated or edited).
    /// </summary>
    public DateTime LastChanged { get; set; }

    public bool HasProblem => Status is InstalledModStatus.Failed or InstalledModStatus.MissingDependency;

    /// <param name="pState">NML's load state of the mod in this session</param>
    /// <param name="pDisabled">Whether the mod is saved as disabled</param>
    /// <param name="pMissingDependency">Whether a required dependency is not installed</param>
    /// <param name="pNeedsRestart">Whether its files changed in a way only a restart applies</param>
    /// <param name="pUpdateAvailable">Whether Steam has a newer version</param>
    public static InstalledModStatus ResolveStatus(ModState pState, bool pDisabled, bool pMissingDependency,
        bool pNeedsRestart, bool pUpdateAvailable)
    {
        if (pNeedsRestart) return InstalledModStatus.RestartRequired;
        if (pState == ModState.LOADED)
        {
            // NML cannot unload a mod, so disabling a loaded one takes effect at the next start.
            if (pDisabled) return InstalledModStatus.RestartRequired;
            return pUpdateAvailable ? InstalledModStatus.UpdateAvailable : InstalledModStatus.Enabled;
        }

        if (pDisabled) return pUpdateAvailable ? InstalledModStatus.UpdateAvailable : InstalledModStatus.Disabled;

        // Saved as enabled but still DISABLED in this session: it was enabled for the next start (Enable All).
        if (pState == ModState.DISABLED) return InstalledModStatus.RestartRequired;

        return pMissingDependency ? InstalledModStatus.MissingDependency : InstalledModStatus.Failed;
    }

    /// <param name="pModType">Declared mod type</param>
    /// <param name="pIsWorkshopLoaded">Whether the mod was found in Steam's Workshop content folder</param>
    /// <param name="pFolderPath">The mod's folder</param>
    /// <param name="pModsPath">The game's Mods folder</param>
    /// <param name="pNativeModsPath">The game's StreamingAssets/mods folder</param>
    public static ModSource DetectSource(ModTypeEnum pModType, bool pIsWorkshopLoaded, string pFolderPath,
        string pModsPath, string pNativeModsPath)
    {
        if (pModType == ModTypeEnum.BEPINEX) return ModSource.BepInEx;
        if (pIsWorkshopLoaded) return ModSource.Workshop;
        if (IsInside(pFolderPath, pModsPath)) return ModSource.ModsFolder;
        if (IsInside(pFolderPath, pNativeModsPath)) return ModSource.NativeModsFolder;
        return ModSource.Other;
    }

    /// <summary>
    ///     Read the Workshop item id from a folder under "workshop/content/&lt;game id&gt;/&lt;item id&gt;".
    /// </summary>
    public static bool TryGetWorkshopId(string pFolderPath, ulong pGameId, out ulong pFileId)
    {
        pFileId = 0;
        if (string.IsNullOrEmpty(pFolderPath)) return false;

        string[] segments = pFolderPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string game_id = pGameId.ToString();
        for (int i = 2; i < segments.Length - 1; i++)
        {
            if (segments[i] == game_id && segments[i - 1] == "content" && segments[i - 2] == "workshop")
            {
                return ulong.TryParse(segments[i + 1], out pFileId);
            }
        }

        return false;
    }

    private static bool IsInside(string pPath, string pFolder)
    {
        if (string.IsNullOrEmpty(pPath) || string.IsNullOrEmpty(pFolder)) return false;

        string folder = Path.GetFullPath(pFolder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
                        Path.DirectorySeparatorChar;
        return Path.GetFullPath(pPath).StartsWith(folder, StringComparison.OrdinalIgnoreCase);
    }
}
