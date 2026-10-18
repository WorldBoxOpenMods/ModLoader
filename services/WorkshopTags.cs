namespace NeoModLoader.services;

/// <summary>
///     Steam Workshop tags that NML's uploads use to mark what an item is.
/// </summary>
internal static class WorkshopTags
{
    /// <summary>
    ///     Every upload since the upload check was removed (ModUploadAuthenticationService.Authed is always true).
    /// </summary>
    public const string Mod = "Mod";

    /// <summary>
    ///     Uploads that skipped the old check. Treated like <see cref="Mod" />: the check never reviewed code.
    /// </summary>
    public const string UnverifiedMod = "Unverified Mod";

    /// <summary>
    ///     NeoModLoader itself.
    /// </summary>
    public const string ModLoader = "Mod Loader";
}
