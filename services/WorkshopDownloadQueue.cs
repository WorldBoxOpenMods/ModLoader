namespace NeoModLoader.services;

/// <summary>
///     The Steam operations the download queue needs. Implemented by the platform backends.
/// </summary>
internal interface IWorkshopItemActions
{
    WorkshopItemState GetItemState(ulong pFileId);
    Task<bool> SubscribeAsync(ulong pFileId);
    Task<bool> UnsubscribeAsync(ulong pFileId);

    /// <summary>
    ///     Ask Steam to download a new item or update an installed one. False when Steam could not start it.
    /// </summary>
    bool StartDownload(ulong pFileId);
}

internal enum DownloadState
{
    Queued,
    Downloading,
    Installing,
    Completed,
    Failed,
    Cancelled
}

/// <summary>
///     One requested download.
/// </summary>
internal sealed class DownloadEntry
{
    public DownloadEntry(ModListing pListing)
    {
        Listing = pListing;
    }

    public ModListing Listing { get; }
    public DownloadState State { get; internal set; } = DownloadState.Queued;

    /// <summary>
    ///     0 to 1 while <see cref="DownloadState.Downloading" />.
    /// </summary>
    public float Progress { get; internal set; }

    /// <summary>
    ///     Locale key of the reason when <see cref="DownloadState.Failed" />.
    /// </summary>
    public string Error { get; internal set; }

    public bool IsFinished => State is DownloadState.Completed or DownloadState.Failed or DownloadState.Cancelled;

    /// <summary>
    ///     The queue subscribed the item, so cancelling or failing before it is installed unsubscribes it again.
    /// </summary>
    internal bool SubscribedByQueue { get; set; }

    internal bool CancelRequested { get; set; }
}

/// <summary>
///     Subscribes to, downloads and installs Workshop items one at a time.
/// </summary>
/// <remarks>
///     Use it from Unity's main thread only. Its awaits then resume on the main thread through Unity's
///     synchronization context, so no locking is needed and <see cref="Changed" /> can update the UI directly.
///     One item at a time keeps the progress accurate and a failing item never stops the others.
/// </remarks>
internal sealed class WorkshopDownloadQueue
{
    public const string ErrorSubscribe = "mod_hub_download_error_subscribe";
    public const string ErrorStart = "mod_hub_download_error_start";
    public const string ErrorNoProgress = "mod_hub_download_error_no_progress";
    public const string ErrorUnexpected = "mod_hub_download_error_unexpected";

    private readonly IWorkshopItemActions steam;
    private readonly Func<string, string> install;
    private readonly Func<TimeSpan, Task> delay;
    private readonly Func<DateTime> clock;
    private readonly Dictionary<ulong, DownloadEntry> entries = new();
    private readonly Queue<DownloadEntry> waiting = new();
    private bool running;

    /// <param name="pSteam">Steam operations</param>
    /// <param name="pInstall">
    ///     Called with the install folder once Steam has installed an item. Returns the locale key of an error, or
    ///     null on success.
    /// </param>
    /// <param name="pDelay">Waits between state checks, <see cref="Task.Delay(TimeSpan)" /> outside tests</param>
    /// <param name="pClock">Current time, <see cref="DateTime.UtcNow" /> outside tests</param>
    public WorkshopDownloadQueue(IWorkshopItemActions pSteam, Func<string, string> pInstall,
        Func<TimeSpan, Task> pDelay, Func<DateTime> pClock)
    {
        steam = pSteam;
        install = pInstall;
        delay = pDelay;
        clock = pClock;
    }

    public TimeSpan PollInterval { get; set; } = TimeSpan.FromMilliseconds(250);

    /// <summary>
    ///     Fail when a download makes no progress for this long, the usual sign of Steam being offline.
    /// </summary>
    public TimeSpan StallTimeout { get; set; } = TimeSpan.FromSeconds(90);

    /// <summary>
    ///     Fail when Steam neither downloads nor queues the item for this long after it was asked to.
    /// </summary>
    public TimeSpan IdleTimeout { get; set; } = TimeSpan.FromSeconds(15);

    /// <summary>
    ///     Raised whenever an entry changes state or progress.
    /// </summary>
    public event Action<DownloadEntry> Changed;

    /// <summary>
    ///     Downloads queued or in progress.
    /// </summary>
    public int UnfinishedCount => entries.Values.Count(entry => !entry.IsFinished);

    /// <summary>
    ///     The latest entry for an item, finished or not, or null.
    /// </summary>
    public DownloadEntry Find(ulong pFileId)
    {
        return entries.TryGetValue(pFileId, out DownloadEntry entry) ? entry : null;
    }

    /// <summary>
    ///     Queue a download. Returns the existing entry when the item is already queued or downloading.
    /// </summary>
    public DownloadEntry Enqueue(ModListing pListing)
    {
        DownloadEntry existing = Find(pListing.Id);
        if (existing != null && !existing.IsFinished) return existing;

        var entry = new DownloadEntry(pListing);
        entries[pListing.Id] = entry;
        waiting.Enqueue(entry);
        Changed?.Invoke(entry);
        if (!running) _ = RunAsync();
        return entry;
    }

    /// <summary>
    ///     Cancel a download. A waiting one stops at once; the active one stops at its next state check.
    /// </summary>
    public void Cancel(ulong pFileId)
    {
        DownloadEntry entry = Find(pFileId);
        if (entry == null || entry.IsFinished) return;

        entry.CancelRequested = true;
        if (entry.State == DownloadState.Queued) SetState(entry, DownloadState.Cancelled);
    }

    private async Task RunAsync()
    {
        running = true;
        try
        {
            while (waiting.Count > 0)
            {
                DownloadEntry entry = waiting.Dequeue();
                if (entry.IsFinished) continue;
                try
                {
                    await ProcessAsync(entry);
                }
                catch (Exception e)
                {
                    // ProcessAsync handles its own errors; this only runs when handling them failed too.
                    LogService.LogException(e);
                    entry.Error = ErrorUnexpected;
                    SetState(entry, DownloadState.Failed);
                }
            }
        }
        finally
        {
            running = false;
        }
    }

    private async Task ProcessAsync(DownloadEntry pEntry)
    {
        ulong id = pEntry.Listing.Id;
        try
        {
            SetState(pEntry, DownloadState.Downloading);
            if (!steam.GetItemState(id).Subscribed)
            {
                if (!await steam.SubscribeAsync(id))
                {
                    await FailAsync(pEntry, ErrorSubscribe);
                    return;
                }

                pEntry.SubscribedByQueue = true;
            }

            if (!steam.StartDownload(id) && !steam.GetItemState(id).IsUpToDate)
            {
                await FailAsync(pEntry, ErrorStart);
                return;
            }
            string directory = await WaitUntilInstalledAsync(pEntry);
            if (directory == null) return;

            SetState(pEntry, DownloadState.Installing);
            string install_error = install(directory);
            if (install_error != null)
            {
                await FailAsync(pEntry, install_error);
                return;
            }

            SetState(pEntry, DownloadState.Completed);
            LogService.LogInfo($"Workshop item {id} ({pEntry.Listing.Title}) installed at {directory}");
        }
        catch (Exception e)
        {
            LogService.LogError($"Workshop item {id} ({pEntry.Listing.Title}) failed: {e.Message}");
            LogService.LogException(e);
            await FailAsync(pEntry, ErrorUnexpected);
        }
    }

    /// <summary>
    ///     Poll Steam until the item is installed and up to date. Returns its folder, or null when the entry was
    ///     cancelled or failed.
    /// </summary>
    private async Task<string> WaitUntilInstalledAsync(DownloadEntry pEntry)
    {
        ulong id = pEntry.Listing.Id;
        string last_signature = null;
        DateTime last_change = clock();
        while (true)
        {
            if (pEntry.CancelRequested)
            {
                await RollbackSubscriptionAsync(pEntry);
                SetState(pEntry, DownloadState.Cancelled);
                return null;
            }

            WorkshopItemState state = steam.GetItemState(id);
            if (state.IsUpToDate) return state.Directory;

            SetProgress(pEntry, state.Progress);
            string signature = $"{state.Downloading}|{state.DownloadPending}|{state.Installed}|{state.Progress:F3}";
            if (signature != last_signature)
            {
                last_signature = signature;
                last_change = clock();
            }
            else if (clock() - last_change > (state.IsBusy ? StallTimeout : IdleTimeout))
            {
                await FailAsync(pEntry, ErrorNoProgress);
                return null;
            }

            await delay(PollInterval);
        }
    }

    private async Task FailAsync(DownloadEntry pEntry, string pErrorKey)
    {
        LogService.LogWarning($"Workshop item {pEntry.Listing.Id} ({pEntry.Listing.Title}) failed: {pErrorKey}");
        // Leave no subscription behind for an item the queue could not install.
        if (!steam.GetItemState(pEntry.Listing.Id).Installed) await RollbackSubscriptionAsync(pEntry);
        pEntry.Error = pErrorKey;
        SetState(pEntry, DownloadState.Failed);
    }

    private async Task RollbackSubscriptionAsync(DownloadEntry pEntry)
    {
        if (!pEntry.SubscribedByQueue) return;
        pEntry.SubscribedByQueue = false;
        if (!await steam.UnsubscribeAsync(pEntry.Listing.Id))
        {
            LogService.LogWarning($"Steam refused to unsubscribe Workshop item {pEntry.Listing.Id}");
        }
    }

    private void SetProgress(DownloadEntry pEntry, float pProgress)
    {
        // Notify on whole-percent steps only.
        if ((int)(pEntry.Progress * 100) == (int)(pProgress * 100)) return;
        pEntry.Progress = pProgress;
        Changed?.Invoke(pEntry);
    }

    private void SetState(DownloadEntry pEntry, DownloadState pState)
    {
        if (pState == DownloadState.Completed) pEntry.Progress = 1;
        pEntry.State = pState;
        Changed?.Invoke(pEntry);
    }
}
