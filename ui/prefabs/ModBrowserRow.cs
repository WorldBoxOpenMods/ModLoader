using System.Globalization;
using NeoModLoader.api;
using NeoModLoader.General;
using NeoModLoader.General.UI.Prefabs;
using NeoModLoader.services;
using NeoModLoader.utils;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui.prefabs;

/// <summary>
///     One Workshop mod in <see cref="ModBrowserWindow" />.
/// </summary>
/// <remarks>
///     Like <see cref="ModManagerRow" />, the prefab holds the visuals and <see cref="Awake" /> adds the handlers.
/// </remarks>
internal class ModBrowserRow : AbstractListWindowItem<ModListing>
{
    [SerializeField] private Image thumbnail;
    [SerializeField] private Text title;
    [SerializeField] private Text details;
    [SerializeField] private Text status;
    [SerializeField] private SimpleButton about;
    [SerializeField] private SimpleButton info;
    [SerializeField] private SimpleButton download;

    private ModListing listing;

    private void Awake()
    {
        info.Button.onClick.AddListener(() => Application.OpenURL($"steam://url/CommunityFilePage/{listing.Id}"));
        download.Button.onClick.AddListener(OnDownloadClicked);
        ModHubUI.SetTextTooltip(about, () => listing.Title, DescribeListing);
    }

    public override void Setup(ModListing pListing)
    {
        listing = pListing;
        title.text = pListing.Title;
        thumbnail.sprite = InternalResourcesGetter.GetIcon();
        WorkshopThumbnails.Request(pListing.PreviewUrl, sprite =>
        {
            // The pooled row may show another listing by the time the image arrives.
            if (listing == pListing) thumbnail.sprite = sprite;
        });
        UpdateAuthor();
        UpdateStatus();
    }

    /// <summary>
    ///     Show the author's name once Steam has sent it.
    /// </summary>
    public void UpdateAuthor()
    {
        string author = ModDownloadService.Authors.Get(listing.AuthorId) ?? "...";
        details.text = $"{author}  -  {DescribeAge(DateTime.UtcNow - listing.Updated)}";
    }

    public void UpdateStatus()
    {
        ListingStatus listing_status = ModDownloadService.GetStatus(listing);
        DownloadEntry entry = ModDownloadService.Queue.Find(listing.Id);
        status.color = StatusColor(listing_status);
        status.text = listing_status switch
        {
            ListingStatus.NotInstalled => string.Format(LM.Get("mod_hub_subscribers"),
                listing.Subscriptions.ToString("N0", CultureInfo.InvariantCulture)),
            ListingStatus.Downloading => string.Format(LM.Get("mod_hub_listing_downloading"),
                Mathf.RoundToInt((entry?.Progress ?? 0) * 100)),
            ListingStatus.Failed => LM.Get(entry.Error),
            _ => LM.Get(ModHubUI.LocaleKey("mod_hub_listing", listing_status))
        };

        bool busy = listing_status is ListingStatus.Queued or ListingStatus.Downloading;
        download.gameObject.SetActive(listing_status != ListingStatus.Installed);
        download.Icon.sprite = ModHubUI.GameIcon(busy ? "iconClose" : "iconArrowDOWN");
        download.TipButton.textOnClick = busy ? "mod_hub_cancel_download" : "mod_hub_download";
    }

    private void OnDownloadClicked()
    {
        DownloadEntry entry = ModDownloadService.Queue.Find(listing.Id);
        if (entry != null && !entry.IsFinished)
        {
            ModDownloadService.Queue.Cancel(listing.Id);
            return;
        }

        ModBrowserWindow.Instance.Download(listing);
    }

    private string DescribeListing()
    {
        uint votes = listing.VotesUp + listing.VotesDown;
        string rating = votes == 0
            ? LM.Get("mod_hub_no_rating")
            : string.Format(LM.Get("mod_hub_rating"), Mathf.RoundToInt(listing.Score * 100), votes);
        string categories = string.Join(", ", listing.Categories);
        return string.Join("\n",
            string.Format(LM.Get("mod_hub_by_author"), ModDownloadService.Authors.Get(listing.AuthorId) ?? "..."),
            string.Format(LM.Get("mod_hub_subscribers"),
                listing.Subscriptions.ToString("N0", CultureInfo.InvariantCulture)),
            rating,
            string.Format(LM.Get("mod_hub_dates"), FormatDate(listing.Updated), FormatDate(listing.Created)),
            string.Format(LM.Get("mod_hub_categories"),
                categories.Length > 0 ? categories : LM.Get("mod_hub_categories_none")),
            string.Format(LM.Get("mod_hub_ai_content"), LM.Get(ModHubUI.LocaleKey("mod_hub_ai", listing.AIContent))));
    }

    private static string FormatDate(DateTime pDate)
    {
        return pDate.ToLocalTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     "3 days ago" and the like, in the largest whole unit.
    /// </summary>
    private static string DescribeAge(TimeSpan pAge)
    {
        (string unit, int count) = pAge.TotalDays switch
        {
            >= 365 => ("year", (int)(pAge.TotalDays / 365)),
            >= 30 => ("month", (int)(pAge.TotalDays / 30)),
            >= 1 => ("day", (int)pAge.TotalDays),
            _ when pAge.TotalHours >= 1 => ("hour", (int)pAge.TotalHours),
            _ => ("minute", Math.Max(1, (int)pAge.TotalMinutes))
        };
        // Separate keys for one and many, e.g. "mod_hub_age_day" and "mod_hub_age_days".
        return string.Format(LM.Get($"mod_hub_age_{unit}{(count == 1 ? "" : "s")}"), count);
    }

    private static Color StatusColor(ListingStatus pStatus)
    {
        return pStatus switch
        {
            ListingStatus.Installed => new Color(0.45f, 0.9f, 0.45f),
            ListingStatus.UpdateAvailable => new Color(1f, 0.65f, 0.2f),
            ListingStatus.Queued or ListingStatus.Downloading => new Color(0.35f, 0.85f, 1f),
            ListingStatus.Failed => new Color(1f, 0.4f, 0.35f),
            _ => ModHubUI.MutedTextColor
        };
    }

    /// <summary>
    ///     Build the row prefab that the pool of <see cref="AbstractListWindow{T,TItem}" /> clones.
    /// </summary>
    internal static ModBrowserRow CreatePrefab()
    {
        var obj = new GameObject(nameof(ModBrowserRow), typeof(Image));
        obj.SetActive(false);
        obj.transform.SetParent(WorldBoxMod.Transform);
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 50);
        Image background = obj.GetComponent<Image>();
        background.sprite = SpriteTextureLoader.getSprite("ui/special/windowInnerSliced");
        background.type = Image.Type.Sliced;

        ModBrowserRow row = obj.AddComponent<ModBrowserRow>();
        var picture = new GameObject("Thumbnail", typeof(Image));
        picture.transform.SetParent(obj.transform, false);
        picture.transform.localPosition = new Vector3(-75, 0);
        picture.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 40);
        row.thumbnail = picture.GetComponent<Image>();
        row.thumbnail.preserveAspect = true;

        // An invisible button over the texts, for the details tooltip.
        row.about = ModHubUI.CreateIconButton(obj.transform, "About", null, new Vector2(130, 24),
            new Vector2(12, 10), null, null);
        row.about.Background.color = Color.clear;
        row.about.Icon.enabled = false;

        row.title = ModHubUI.CreateText(obj.transform, "Title", new Vector2(130, 12), new Vector2(12, 15), 7,
            TextAnchor.MiddleLeft);
        row.details = ModHubUI.CreateText(obj.transform, "Details", new Vector2(130, 10), new Vector2(12, 4), 6,
            TextAnchor.MiddleLeft);
        row.details.color = ModHubUI.MutedTextColor;
        row.status = ModHubUI.CreateText(obj.transform, "Status", new Vector2(100, 12), new Vector2(-3, -12), 7,
            TextAnchor.MiddleLeft);

        Vector2 button_size = new(19, 19);
        row.info = ModHubUI.CreateIconButton(obj.transform, "Info", ModHubUI.GameIcon("iconQuestionMark"),
            button_size, new Vector2(88, -11), null, "mod_hub_workshop_page");
        row.download = ModHubUI.CreateIconButton(obj.transform, "Download", ModHubUI.GameIcon("iconArrowDOWN"),
            button_size, new Vector2(66, -11), null, "mod_hub_download");
        return row;
    }
}
