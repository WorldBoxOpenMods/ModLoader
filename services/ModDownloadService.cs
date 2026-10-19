using NeoModLoader.api;
using NeoModLoader.constants;
using NeoModLoader.utils;
using System.Text.RegularExpressions;
using RSG;

namespace NeoModLoader.services;

/// <summary>
///     Connects the Download Mods window to the Steam Workshop: the list of mods, the download queue, author names,
///     and what happens when a download finishes.
/// </summary>
/// <remarks>
///     Nothing here compiles or loads a mod. A new download is saved as disabled; the player enables it in Manage
///     Mods through the existing enable path.
/// </remarks>
internal static class ModDownloadService
{
    public const string ErrorNotAMod = "mod_hub_download_error_not_mod";

    // Mods whose files changed on disk this session in a way only a restart applies.
    private static readonly HashSet<string> restart_pending = new();
    private static readonly Dictionary<ulong, string> descriptions = new();
    private static readonly WorkshopItemStateCache item_states = new(file_id => Steam.GetItemState(file_id),
        () => DateTime.UtcNow);
    private static CatalogStatus cached_steam_status = CatalogStatus.Empty;
    private static WorkshopCatalog catalog;
    private static WorkshopDownloadQueue queue;
    private static WorkshopAuthorNames authors;

    public static WorkshopCatalog Catalog => catalog ??=
        new WorkshopCatalog((page, ranked_by_trend) => Steam.QueryModsAsync(page, ranked_by_trend));

    public static WorkshopDownloadQueue Queue => queue ??= CreateQueue();

    public static WorkshopAuthorNames Authors => authors ??=
        new WorkshopAuthorNames(steam_id => Steam.GetPersonaNameAsync(steam_id));

    private static IPlatformSpecificModWorkshopService Steam => ModWorkshopService.Backend;

    /// <summary>
    ///     Whether the Workshop can be used now: <see cref="CatalogStatus.Ready" />, or why not.
    /// </summary>
    public static CatalogStatus CheckSteam()
    {
        Promise steam_initialized = ModWorkshopService.steamWorkshopPromise;
        if (Others.is_editor || steam_initialized?.CurState != PromiseState.Resolved || Steam == null)
        {
            return CatalogStatus.SteamUnavailable;
        }

        return Steam.IsSteamOnline() ? CatalogStatus.Ready : CatalogStatus.Offline;
    }

    /// <summary>
    ///     Download a mod, or update it when it is installed already.
    /// </summary>
    public static void Download(ModListing pListing)
    {
        CatalogStatus status = CheckSteam();
        if (status != CatalogStatus.Ready)
        {
            Catalog.SetUnavailable(status);
            return;
        }
        WorkshopItemState state = Steam.GetItemState(pListing.Id);
        if (!state.Subscribed || !state.Installed) SetPending(pListing.Id, true);
        Queue.Enqueue(pListing);
    }

    public static ListingStatus GetStatus(ModListing pListing)
    {
        DownloadEntry entry = Queue.Find(pListing.Id);
        if (entry?.State is DownloadState.Queued or DownloadState.Downloading or DownloadState.Installing
            or DownloadState.Failed)
        {
            return ModListingQuery.ResolveStatus(entry, default);
        }

        return ModListingQuery.ResolveStatus(entry, GetItemState(pListing.Id));
    }

    private static WorkshopItemState GetItemState(ulong pFileId)
    {
        CatalogStatus steam = CheckSteam();
        if (steam != cached_steam_status)
        {
            ClearItemStates();
            cached_steam_status = steam;
        }

        return steam == CatalogStatus.Ready ? item_states.Get(pFileId) : default;
    }

    internal static void ClearItemStates() => item_states.Clear();

    /// <summary>
    ///     Queue every listed mod that Steam has an update for. Returns how many were queued.
    /// </summary>
    public static int UpdateAll()
    {
        ClearItemStates();
        List<ModListing> outdated = Catalog.Items.Where(listing => GetStatus(listing) == ListingStatus.UpdateAvailable)
            .ToList();
        foreach (ModListing listing in outdated) Queue.Enqueue(listing);
        return outdated.Count;
    }

    /// <summary>
    ///     Whether Steam has a newer version of an installed Workshop mod.
    /// </summary>
    public static bool HasUpdate(ModDeclare pMod)
    {
        return pMod.IsWorkshopLoaded &&
               InstalledMod.TryGetWorkshopId(pMod.FolderPath, CoreConstants.GameId, out ulong file_id) &&
               GetItemState(file_id).NeedsUpdate;
    }

    /// <summary>
    ///     Whether only a restart applies a change to the mod's files: it was updated while loaded (NML cannot
    ///     reload it) or unsubscribed (Steam deletes its files).
    /// </summary>
    public static bool NeedsRestart(string pModUid)
    {
        return restart_pending.Contains(pModUid);
    }

    /// <summary>
    ///     Unsubscribe from an installed Workshop mod, so Steam deletes it. Returns false when Steam refused.
    /// </summary>
    public static async Task<bool> UnsubscribeAsync(ModDeclare pMod)
    {
        if (CheckSteam() != CatalogStatus.Ready) return false;
        if (!InstalledMod.TryGetWorkshopId(pMod.FolderPath, CoreConstants.GameId, out ulong file_id)) return false;

        bool unsubscribed = await Steam.UnsubscribeAsync(file_id);
        item_states.Invalidate(file_id);
        if (unsubscribed) restart_pending.Add(pMod.UID);
        else LogService.LogWarning($"Steam refused to unsubscribe Workshop item {file_id} ({pMod.Name})");
        return unsubscribed;
    }

    /// <summary>
    ///     Workshop description of an item as plain text, or null when Steam is unavailable. Kept for the game
    ///     session.
    /// </summary>
    public static async Task<string> GetDescriptionAsync(ulong pFileId)
    {
        if (descriptions.TryGetValue(pFileId, out string cached)) return cached;
        if (CheckSteam() != CatalogStatus.Ready) return null;

        string text;
        try
        {
            text = await Steam.GetDescriptionAsync(pFileId);
        }
        catch (Exception e)
        {
            LogService.LogWarning($"Failed to get the Workshop description of {pFileId}: {e.Message}");
            return null;
        }

        if (text == null) return null;
        return descriptions[pFileId] = StripBBCode(text);
    }

    /// <summary>
    ///     Plain text from Steam's BBCode: list items become dashes, images are dropped and other tags removed.
    /// </summary>
    internal static string StripBBCode(string pText)
    {
        string text = Regex.Replace(pText, @"\[img\].*?\[/img\]", "",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        text = text.Replace("[*]", "- ");
        text = Regex.Replace(text, @"\[/?[a-z0-9*]+(=[^\]]*)?\]", "", RegexOptions.IgnoreCase);
        return Regex.Replace(text, @"(\r?\n){3,}", "\n\n");
    }

    private static WorkshopDownloadQueue CreateQueue()
    {
        var download_queue = new WorkshopDownloadQueue(Steam, Install, Task.Delay, () => DateTime.UtcNow);
        download_queue.Changed += entry =>
        {
            item_states.Invalidate(entry.Listing.Id);
            if (entry.State is DownloadState.Cancelled or DownloadState.Failed) SetPending(entry.Listing.Id, false);
        };
        return download_queue;
    }

    private static void SetPending(ulong pFileId, bool pPending)
    {
        List<ulong> pending = ModHubSettings.Instance.PendingDownloads;
        bool changed = pPending ? !pending.Contains(pFileId) : pending.Contains(pFileId);
        if (!changed) return;

        if (pPending) pending.Add(pFileId);
        else pending.Remove(pFileId);
        ModHubSettings.Instance.Save();
    }

    /// <summary>
    ///     Called by the queue once Steam has installed an item. Returns the locale key of an error, or null.
    /// </summary>
    private static string Install(string pDirectory)
    {
        if (ulong.TryParse(Path.GetFileName(pDirectory), out ulong file_id)) SetPending(file_id, false);
        ModDeclare mod = ModInfoUtils.recogMod(pDirectory, false);
        if (mod == null) return ErrorNotAMod;

        if (ModInfoUtils.TryGetRecognizedMod(mod.UID, out _))
        {
            if (ModCompileLoadService.IsModLoaded(mod.UID)) restart_pending.Add(mod.UID);
            return null;
        }

        // New to this computer: saved as disabled, so neither this session nor the next start loads it.
        ModInfoUtils.setModDisabled(mod.UID, true);

        // The startup scan only lists NeoModLoader-type Workshop mods; do the same here.
        if (mod.ModType != ModTypeEnum.NEOMOD) return null;
        if (string.IsNullOrEmpty(mod.RepoUrl)) mod.SetRepoUrlToWorkshopPage(Path.GetFileName(pDirectory));
        ModInfoUtils.EnsureRecognizedMod(mod);
        return null;
    }
}
