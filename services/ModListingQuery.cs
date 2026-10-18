namespace NeoModLoader.services;

internal enum ListingSort
{
    RecentlyUpdated,
    MostSubscribed,

    /// <summary>
    ///     Steam's trending order. Items Steam does not rank follow, by subscribers.
    /// </summary>
    Trending,
    TopRated,
    Newest,
    Name
}

internal enum UpdatedWithin
{
    Anytime,
    Week,
    Month,
    ThreeMonths,
    Year
}

internal enum InstallFilter
{
    All,
    NotInstalled,
    Installed,
    UpdateAvailable
}

internal enum AIContentFilter
{
    Any,
    HideAIMade,

    /// <summary>
    ///     Hide AI-made and AI-assisted mods.
    /// </summary>
    HideAI,

    /// <summary>
    ///     Only mods tagged Human Authored. Mods uploaded before NML added AI tags have no such tag.
    /// </summary>
    OnlyHumanAuthored
}

/// <summary>
///     What a listing means for this computer, shown on its row.
/// </summary>
internal enum ListingStatus
{
    NotInstalled,
    Queued,
    Downloading,
    Installed,
    UpdateAvailable,
    Failed
}

/// <summary>
///     Filter, search and order of the Download Mods list.
/// </summary>
internal sealed class ModListingQuery
{
    public ListingSort Sort { get; set; } = ListingSort.RecentlyUpdated;
    public UpdatedWithin UpdatedWithin { get; set; } = UpdatedWithin.Anytime;
    public InstallFilter Install { get; set; } = InstallFilter.All;
    public AIContentFilter AIContent { get; set; } = AIContentFilter.Any;

    /// <summary>
    ///     Categories from CoreConstants.ModCategories. A listing matches when it has any of them; empty matches all.
    /// </summary>
    public HashSet<string> Categories { get; } = new(StringComparer.OrdinalIgnoreCase);

    public ModSearchMode SearchMode { get; set; } = ModSearchMode.Name;

    /// <summary>
    ///     Case-insensitive part of the title, or of the author's Steam name. Empty matches everything.
    /// </summary>
    public string SearchText { get; set; } = "";

    /// <param name="pListings">All listings</param>
    /// <param name="pNow">Current time, for <see cref="UpdatedWithin" /></param>
    /// <param name="pStatus">Install state of a listing</param>
    /// <param name="pAuthorName">Steam name of an author, or null when not known yet</param>
    /// <param name="pTrendingRank">Position of each listing in Steam's trending order</param>
    public List<ModListing> Apply(IEnumerable<ModListing> pListings, DateTime pNow,
        Func<ModListing, ListingStatus> pStatus, Func<ulong, string> pAuthorName,
        IReadOnlyDictionary<ulong, int> pTrendingRank)
    {
        IEnumerable<ModListing> matches = pListings.Where(listing =>
            IsRecentEnough(listing, pNow) && MatchesInstall(pStatus(listing)) && MatchesAIContent(listing) &&
            MatchesCategories(listing) && MatchesSearch(listing, pAuthorName));
        return Order(matches, pTrendingRank).ToList();
    }

    /// <summary>
    ///     Status of a listing from its download entry (if any) and what Steam has on disk.
    /// </summary>
    public static ListingStatus ResolveStatus(DownloadEntry pEntry, WorkshopItemState pState)
    {
        if (pEntry != null)
        {
            switch (pEntry.State)
            {
                case DownloadState.Queued:
                    return ListingStatus.Queued;
                case DownloadState.Downloading:
                case DownloadState.Installing:
                    return ListingStatus.Downloading;
                case DownloadState.Failed:
                    return ListingStatus.Failed;
            }
        }

        // Steam keeps an unsubscribed item installed (and may finish its download) until the game closes.
        if (!pState.Subscribed) return ListingStatus.NotInstalled;
        if (pState.Installed) return pState.NeedsUpdate ? ListingStatus.UpdateAvailable : ListingStatus.Installed;
        return pState.IsBusy ? ListingStatus.Downloading : ListingStatus.NotInstalled;
    }

    private bool IsRecentEnough(ModListing pListing, DateTime pNow)
    {
        int days = UpdatedWithin switch
        {
            UpdatedWithin.Week => 7,
            UpdatedWithin.Month => 30,
            UpdatedWithin.ThreeMonths => 90,
            UpdatedWithin.Year => 365,
            _ => 0
        };
        return days == 0 || pNow - pListing.Updated <= TimeSpan.FromDays(days);
    }

    private bool MatchesInstall(ListingStatus pStatus)
    {
        bool installed = pStatus is ListingStatus.Installed or ListingStatus.UpdateAvailable;
        return Install switch
        {
            InstallFilter.NotInstalled => !installed,
            InstallFilter.Installed => installed,
            InstallFilter.UpdateAvailable => pStatus == ListingStatus.UpdateAvailable,
            _ => true
        };
    }

    private bool MatchesAIContent(ModListing pListing)
    {
        ModAIContent content = pListing.AIContent;
        return AIContent switch
        {
            AIContentFilter.HideAIMade => content != ModAIContent.AIMade,
            AIContentFilter.HideAI => content is not (ModAIContent.AIMade or ModAIContent.AIAssisted),
            AIContentFilter.OnlyHumanAuthored => content == ModAIContent.HumanAuthored,
            _ => true
        };
    }

    private bool MatchesCategories(ModListing pListing)
    {
        return Categories.Count == 0 || Categories.Any(pListing.HasTag);
    }

    private bool MatchesSearch(ModListing pListing, Func<ulong, string> pAuthorName)
    {
        string search_text = SearchText?.Trim();
        if (string.IsNullOrEmpty(search_text)) return true;

        string field = SearchMode == ModSearchMode.Author ? pAuthorName(pListing.AuthorId) : pListing.Title;
        return field != null && field.IndexOf(search_text, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private IEnumerable<ModListing> Order(IEnumerable<ModListing> pListings,
        IReadOnlyDictionary<ulong, int> pTrendingRank)
    {
        return Sort switch
        {
            ListingSort.MostSubscribed => pListings.OrderByDescending(listing => listing.Subscriptions),
            ListingSort.Trending => pListings
                .OrderBy(listing => pTrendingRank.TryGetValue(listing.Id, out int rank) ? rank : int.MaxValue)
                .ThenByDescending(listing => listing.Subscriptions),
            ListingSort.TopRated => pListings.OrderByDescending(listing => listing.Score)
                .ThenByDescending(listing => listing.VotesUp),
            ListingSort.Newest => pListings.OrderByDescending(listing => listing.Created),
            ListingSort.Name => pListings.OrderBy(listing => listing.Title, StringComparer.OrdinalIgnoreCase),
            _ => pListings.OrderByDescending(listing => listing.Updated)
        };
    }
}
