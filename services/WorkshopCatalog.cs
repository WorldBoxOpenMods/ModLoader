using NeoModLoader.constants;

namespace NeoModLoader.services;

internal enum CatalogStatus
{
    /// <summary>
    ///     Nothing loaded yet.
    /// </summary>
    Empty,
    Loading,
    Ready,

    /// <summary>
    ///     The player stopped loading. <see cref="WorkshopCatalog.Items" /> holds the pages loaded so far.
    /// </summary>
    Cancelled,
    SteamUnavailable,
    Offline,
    Failed
}

/// <summary>
///     Every mod offered for NML on the Steam Workshop, loaded once per game session.
/// </summary>
/// <remarks>
///     Steam can only order a few queries itself, and the filters (updated within, install state, categories, AI
///     content) are only correct on the full list. So every page is fetched (about 4 pages of 50 today, capped at
///     <see cref="MaxPages" />), and the list is filtered and sorted locally by <see cref="ModListingQuery" />.
///     Use it from Unity's main thread, like <see cref="WorkshopDownloadQueue" />.
/// </remarks>
internal sealed class WorkshopCatalog
{
    public const int MaxPages = 40;

    private readonly Func<int, bool, Task<ModListingPage>> fetch_page;
    private readonly HashSet<ulong> known_ids = new();

    // Incremented by every load and cancel, so a page that arrives for an older request is dropped.
    private int generation;

    /// <param name="pFetchPage">Fetches one 1-based page, ranked by trend when the flag is set; null on failure.</param>
    public WorkshopCatalog(Func<int, bool, Task<ModListingPage>> pFetchPage)
    {
        fetch_page = pFetchPage;
    }

    public List<ModListing> Items { get; } = new();

    /// <summary>
    ///     Position of each item in Steam's trending order, empty until <see cref="LoadTrendingAsync" /> finished.
    /// </summary>
    public Dictionary<ulong, int> TrendingRank { get; } = new();

    public CatalogStatus Status { get; private set; } = CatalogStatus.Empty;

    /// <summary>
    ///     Items Steam reports for the query, -1 before the first page arrives.
    /// </summary>
    public int TotalCount { get; private set; } = -1;

    public bool ReachedPageCap { get; private set; }
    public bool IsLoadingTrending { get; private set; }
    public bool IsLoading => Status == CatalogStatus.Loading || IsLoadingTrending;

    public event Action Changed;

    /// <summary>
    ///     Show that loading cannot start, e.g. because Steam is offline.
    /// </summary>
    public void SetUnavailable(CatalogStatus pStatus)
    {
        generation++;
        IsLoadingTrending = false;
        SetStatus(pStatus);
    }

    /// <summary>
    ///     Load every page again. Pages appear in <see cref="Items" /> as they arrive.
    /// </summary>
    public async Task LoadAsync()
    {
        int request = ++generation;
        IsLoadingTrending = false;
        Items.Clear();
        known_ids.Clear();
        TrendingRank.Clear();
        TotalCount = -1;
        ReachedPageCap = false;
        SetStatus(CatalogStatus.Loading);

        List<ModListing> loaded = await FetchAllAsync(request, false, page => AddItems(page.Items));
        if (request != generation) return;
        SetStatus(loaded == null ? CatalogStatus.Failed : CatalogStatus.Ready);
    }

    /// <summary>
    ///     Load Steam's trending order, needed only for the Trending sort. On failure the order stays empty and
    ///     <see cref="ModListingQuery" /> falls back to subscribers.
    /// </summary>
    public async Task LoadTrendingAsync()
    {
        if (TrendingRank.Count > 0 || IsLoading) return;

        int request = ++generation;
        IsLoadingTrending = true;
        Changed?.Invoke();
        List<ModListing> ranked = await FetchAllAsync(request, true, _ => { });
        if (request != generation) return;

        IsLoadingTrending = false;
        for (int i = 0; i < (ranked?.Count ?? 0); i++)
        {
            if (!TrendingRank.ContainsKey(ranked[i].Id)) TrendingRank[ranked[i].Id] = i;
        }

        Changed?.Invoke();
    }

    /// <summary>
    ///     Stop loading. Pages that arrive later are dropped.
    /// </summary>
    public void Cancel()
    {
        if (!IsLoading) return;
        generation++;
        IsLoadingTrending = false;
        SetStatus(Status == CatalogStatus.Loading ? CatalogStatus.Cancelled : Status);
    }

    /// <summary>
    ///     Fetch pages until Steam has no more. Returns null when a page failed or the request was replaced.
    /// </summary>
    private async Task<List<ModListing>> FetchAllAsync(int pRequest, bool pRankedByTrend,
        Action<ModListingPage> pOnPage)
    {
        var all = new List<ModListing>();
        try
        {
            for (int page_number = 1; ; page_number++)
            {
                if (page_number > MaxPages)
                {
                    ReachedPageCap = true;
                    return all;
                }

                ModListingPage page = await fetch_page(page_number, pRankedByTrend);
                if (pRequest != generation) return null;
                if (page == null)
                {
                    LogService.LogWarning($"Steam returned no result for Workshop page {page_number}.");
                    return null;
                }

                if (!pRankedByTrend) TotalCount = page.TotalCount;
                all.AddRange(page.Items);
                pOnPage(page);
                Changed?.Invoke();
                if (page.Items.Count == 0 || all.Count >= page.TotalCount) return all;
            }
        }
        catch (Exception e)
        {
            LogService.LogError($"Workshop query failed: {e.Message}");
            LogService.LogException(e);
            return null;
        }
    }

    private void AddItems(IEnumerable<ModListing> pItems)
    {
        foreach (ModListing item in pItems)
        {
            // Rankings can shift between page requests, so an item may arrive twice.
            if (item.Id == CoreConstants.WorkshopFileId || !known_ids.Add(item.Id)) continue;
            Items.Add(item);
        }
    }

    private void SetStatus(CatalogStatus pStatus)
    {
        Status = pStatus;
        Changed?.Invoke();
    }
}
