using NeoModLoader.api;
using RSG;

namespace NeoModLoader.services;

internal interface IPlatformSpecificModWorkshopService : IWorkshopItemActions
{
    void UploadModLoader(string changelog);

    Promise UploadMod(string name, string description, string previewImagePath, string workshopPath, string changelog,
        bool verified, string aiTag = null, List<string> extraTags = null);

    Promise EditMod(ulong fileID, string previewImagePath, string workshopPath, string changelog, ModDeclare mod_decl);
    void FindSubscribedMods();
    ModDeclare GetNextModFromWorkshopItem();

    /// <summary>
    ///     Whether Steam is running and connected.
    /// </summary>
    bool IsSteamOnline();

    /// <summary>
    ///     One page (1-based, up to 50 items) of NML mods, or null when Steam returns nothing.
    /// </summary>
    /// <param name="pPage">Page number, starting at 1</param>
    /// <param name="pRankedByTrend">Order by recent popularity instead of publication date</param>
    Task<ModListingPage> QueryModsAsync(int pPage, bool pRankedByTrend);

    /// <summary>
    ///     Steam display name of a user, or null when Steam does not answer in time.
    /// </summary>
    Task<string> GetPersonaNameAsync(ulong pSteamId);

    /// <summary>
    ///     Workshop description of an item as its author wrote it, with BBCode, or null when Steam returns nothing.
    /// </summary>
    Task<string> GetDescriptionAsync(ulong pFileId);
}