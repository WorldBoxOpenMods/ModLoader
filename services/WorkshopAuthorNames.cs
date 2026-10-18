namespace NeoModLoader.services;

/// <summary>
///     Steam display names of Workshop authors, requested on demand and kept for the game session.
/// </summary>
/// <remarks>
///     Workshop queries only return an author's Steam id. Each name is a separate request to Steam, so names
///     arrive a little after the list. Use it from Unity's main thread.
/// </remarks>
internal sealed class WorkshopAuthorNames
{
    // Steam answers name requests one by one; a few at a time keeps it quick without flooding it.
    private const int batch_size = 8;

    private readonly Func<ulong, Task<string>> request_name;
    private readonly Dictionary<ulong, string> names = new();
    private readonly HashSet<ulong> requested = new();

    /// <param name="pRequestName">Asks Steam for a name; null when unknown</param>
    public WorkshopAuthorNames(Func<ulong, Task<string>> pRequestName)
    {
        request_name = pRequestName;
    }

    /// <summary>
    ///     Raised after each batch of names arrives.
    /// </summary>
    public event Action Changed;

    /// <summary>
    ///     The name of an author, or null while it is unknown.
    /// </summary>
    public string Get(ulong pSteamId)
    {
        return names.TryGetValue(pSteamId, out string name) ? name : null;
    }

    /// <summary>
    ///     Request the names not requested yet.
    /// </summary>
    public async Task RequestAsync(IEnumerable<ulong> pSteamIds)
    {
        List<ulong> missing = pSteamIds.Where(id => requested.Add(id)).ToList();
        for (int start = 0; start < missing.Count; start += batch_size)
        {
            await Task.WhenAll(missing.Skip(start).Take(batch_size).Select(RequestOneAsync));
            Changed?.Invoke();
        }
    }

    private async Task RequestOneAsync(ulong pSteamId)
    {
        try
        {
            string name = await request_name(pSteamId);
            if (!string.IsNullOrEmpty(name)) names[pSteamId] = name;
        }
        catch (Exception e)
        {
            LogService.LogWarning($"Cannot get the Steam name of user {pSteamId}: {e.Message}");
        }
    }
}
