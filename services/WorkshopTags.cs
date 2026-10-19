namespace NeoModLoader.services;

/// <summary>
///     Steam Workshop tags that NML's uploads use to mark what an item is.
/// </summary>
internal static class WorkshopTags
{
    /// <summary>
    ///     All new mod uploads.
    /// </summary>
    public const string Mod = "Mod";

    /// <summary>
    ///     Legacy tag, kept only to discover existing Workshop items. New uploads use <see cref="Mod" />.
    /// </summary>
    public const string UnverifiedMod = "Unverified Mod";

    /// <summary>
    ///     NeoModLoader itself.
    /// </summary>
    public const string ModLoader = "Mod Loader";
}
