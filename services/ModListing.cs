using NeoModLoader.constants;

namespace NeoModLoader.services;

/// <summary>
///     AI content as declared by the uploader through NML's upload tags.
/// </summary>
internal enum ModAIContent
{
    /// <summary>
    ///     Uploaded before NML added AI tags, or by another tool.
    /// </summary>
    NotDeclared,
    HumanAuthored,
    AIAssisted,
    AIMade
}

/// <summary>
///     A mod offered for download, independent of where it is hosted. Built from Steam Workshop items today.
/// </summary>
internal sealed class ModListing
{
    // Tags written by ModDeclare.GetAIAttributionDisplay() at upload, lower-cased by Facepunch.
    private const string ai_made_tag = "ai-made";
    private const string ai_assisted_tag = "ai-assisted";
    private const string human_authored_tag = "human authored";

    public ulong Id { get; set; }
    public string Title { get; set; } = "";
    public ulong AuthorId { get; set; }

    /// <summary>
    ///     Lower-case tags.
    /// </summary>
    public string[] Tags { get; set; } = Array.Empty<string>();

    public string PreviewUrl { get; set; }
    public ulong Subscriptions { get; set; }
    public uint VotesUp { get; set; }
    public uint VotesDown { get; set; }

    /// <summary>
    ///     Rating from 0 to 1.
    /// </summary>
    public float Score { get; set; }

    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }

    public ModAIContent AIContent
    {
        get
        {
            if (HasTag(ai_made_tag)) return ModAIContent.AIMade;
            if (HasTag(ai_assisted_tag)) return ModAIContent.AIAssisted;
            return HasTag(human_authored_tag) ? ModAIContent.HumanAuthored : ModAIContent.NotDeclared;
        }
    }

    /// <summary>
    ///     The categories from <see cref="CoreConstants.ModCategories" /> this mod is tagged with.
    /// </summary>
    public IEnumerable<string> Categories =>
        CoreConstants.ModCategories.Where(category => HasTag(category));

    public bool HasTag(string pTag)
    {
        return Tags.Any(tag => string.Equals(tag, pTag, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
///     One page of listings and how many listings match in total.
/// </summary>
internal sealed class ModListingPage
{
    public int TotalCount { get; set; }
    public List<ModListing> Items { get; set; } = new();
}

/// <summary>
///     What Steam has on this computer for one Workshop item, read live on every call.
/// </summary>
internal struct WorkshopItemState
{
    public bool Subscribed;
    public bool Installed;
    public bool NeedsUpdate;
    public bool Downloading;
    public bool DownloadPending;

    /// <summary>
    ///     0 to 1 while <see cref="Downloading" />.
    /// </summary>
    public float Progress;

    /// <summary>
    ///     Install folder, null unless installed.
    /// </summary>
    public string Directory;

    public bool IsUpToDate => Installed && !NeedsUpdate && !Downloading && !DownloadPending;
    public bool IsBusy => Downloading || DownloadPending;
}
