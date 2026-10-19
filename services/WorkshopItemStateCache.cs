namespace NeoModLoader.services;

/// <summary>
///     Short-lived Steam item snapshots shared by the manager, listing filters and rows.
///     Queue status and progress are resolved separately, so they stay live.
/// </summary>
internal sealed class WorkshopItemStateCache
{
    private static readonly TimeSpan lifetime = TimeSpan.FromSeconds(5);
    private readonly Func<ulong, WorkshopItemState> fetch;
    private readonly Func<DateTime> clock;
    private readonly Dictionary<ulong, (DateTime Updated, WorkshopItemState State)> items = new();

    public WorkshopItemStateCache(Func<ulong, WorkshopItemState> pFetch, Func<DateTime> pClock)
    {
        fetch = pFetch;
        clock = pClock;
    }

    public WorkshopItemState Get(ulong pFileId)
    {
        DateTime now = clock();
        if (items.TryGetValue(pFileId, out var cached) &&
            now >= cached.Updated && now - cached.Updated < lifetime)
        {
            return cached.State;
        }

        WorkshopItemState state = fetch(pFileId);
        items[pFileId] = (clock(), state);
        return state;
    }

    public void Invalidate(ulong pFileId) => items.Remove(pFileId);

    public void Clear() => items.Clear();
}
