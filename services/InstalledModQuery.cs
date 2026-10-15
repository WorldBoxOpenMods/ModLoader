namespace NeoModLoader.services;

/// <summary>
///     Which mods Manage Mods shows.
/// </summary>
internal enum InstalledModFilter
{
    All,
    Enabled,
    Disabled,

    /// <summary>
    ///     Failed to load or missing a dependency.
    /// </summary>
    Problems,
    Updates
}

internal enum InstalledModSort
{
    NameAscending,
    NameDescending,

    /// <summary>
    ///     Newest <see cref="InstalledMod.LastChanged" /> first.
    /// </summary>
    RecentlyChanged
}

/// <summary>
///     What the search text is matched against.
/// </summary>
internal enum ModSearchMode
{
    Name,
    Author
}

/// <summary>
///     Filter, search and order of the Manage Mods list.
/// </summary>
internal sealed class InstalledModQuery
{
    public InstalledModFilter Filter { get; set; } = InstalledModFilter.All;
    public InstalledModSort Sort { get; set; } = InstalledModSort.NameAscending;
    public ModSearchMode SearchMode { get; set; } = ModSearchMode.Name;

    /// <summary>
    ///     Case-insensitive part of a name or author. Empty matches everything.
    /// </summary>
    public string SearchText { get; set; } = "";

    public List<InstalledMod> Apply(IEnumerable<InstalledMod> pMods)
    {
        IEnumerable<InstalledMod> matches = pMods.Where(mod => MatchesFilter(mod) && MatchesSearch(mod));
        return Order(matches).ToList();
    }

    private bool MatchesFilter(InstalledMod pMod)
    {
        return Filter switch
        {
            InstalledModFilter.Enabled => pMod.Enabled,
            InstalledModFilter.Disabled => !pMod.Enabled,
            InstalledModFilter.Problems => pMod.HasProblem,
            InstalledModFilter.Updates => pMod.Status == InstalledModStatus.UpdateAvailable,
            _ => true
        };
    }

    private bool MatchesSearch(InstalledMod pMod)
    {
        string search_text = SearchText?.Trim();
        if (string.IsNullOrEmpty(search_text)) return true;

        string field = SearchMode == ModSearchMode.Author ? pMod.Author : pMod.Name;
        return field != null && field.IndexOf(search_text, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private IEnumerable<InstalledMod> Order(IEnumerable<InstalledMod> pMods)
    {
        return Sort switch
        {
            InstalledModSort.NameDescending =>
                pMods.OrderByDescending(mod => mod.Name, StringComparer.OrdinalIgnoreCase),
            InstalledModSort.RecentlyChanged => pMods.OrderByDescending(mod => mod.LastChanged)
                .ThenBy(mod => mod.Name, StringComparer.OrdinalIgnoreCase),
            _ => pMods.OrderBy(mod => mod.Name, StringComparer.OrdinalIgnoreCase)
        };
    }
}
