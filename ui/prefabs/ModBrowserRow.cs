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
///     One Workshop mod in <see cref="ModBrowserWindow" />: thumbnail, title with details on hover, status,
///     when it was updated, and its buttons.
/// </summary>
/// <remarks>
///     Like <see cref="ModManagerRow" />, the prefab holds the visuals and <see cref="Awake" /> adds the handlers.
/// </remarks>
internal class ModBrowserRow : AbstractListWindowItem<ModListing>
{
    private const float height = 44;

    [SerializeField] private Image thumbnail;
    [SerializeField] private Text title;
    [SerializeField] private Text status;
    [SerializeField] private Text age;
    [SerializeField] private SimpleButton info;
    [SerializeField] private SimpleButton download;

    private ModListing listing;

    private void Awake()
    {
        info.Button.onClick.AddListener(() => Application.OpenURL($"steam://url/CommunityFilePage/{listing.Id}"));
        download.Button.onClick.AddListener(OnDownloadClicked);
        ModHubUI.SetTextTooltip(title.GetComponent<TipButton>(), () => listing.Title, DescribeListing);
    }

    public override void Setup(ModListing pListing)
    {
        listing = pListing;
        title.text = pListing.Title;
        age.text = DescribeAge(DateTime.UtcNow - pListing.Updated);
        thumbnail.sprite = InternalResourcesGetter.GetIcon();
        WorkshopThumbnails.Request(pListing.PreviewUrl, this, sprite =>
        {
            // The pooled row may show another listing by the time the image arrives.
            if (listing == pListing) thumbnail.sprite = sprite;
        });
        UpdateStatus();
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
            ListingStatus.Installed => ModHubStyle.Positive,
            ListingStatus.UpdateAvailable => ModHubStyle.Warning,
            ListingStatus.Queued or ListingStatus.Downloading => new Color(0.35f, 0.85f, 1f),
            ListingStatus.Failed => ModHubStyle.Negative,
            _ => ModHubStyle.TextMuted
        };
    }

    /// <summary>
    ///     Build the row prefab that the pool of <see cref="AbstractListWindow{T,TItem}" /> clones.
    /// </summary>
    internal static ModBrowserRow CreatePrefab()
    {
        GameObject obj = ModHubStyle.Box(WorldBoxMod.Transform, nameof(ModBrowserRow), ModHubStyle.Card);
        obj.SetActive(false);
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(0, height);
        ModHubStyle.Horizontal(obj, ModHubStyle.SpaceM, ModHubStyle.SpaceS).padding.left = ModHubStyle.SpaceM;

        ModBrowserRow row = obj.AddComponent<ModBrowserRow>();
        row.thumbnail = ModHubStyle.Icon(obj.transform, "Thumbnail", null, 32);

        var body = new GameObject("Body", typeof(RectTransform));
        body.transform.SetParent(obj.transform, false);
        ModHubStyle.Vertical(body, 2).childForceExpandWidth = true;
        ModHubStyle.Size(body, pFlexibleWidth: 1);

        row.title = ModHubStyle.Label(body.transform, "Title", ModHubStyle.FontBody, ModHubStyle.TextBright);
        // Hovering the title shows the author and the item's details.
        row.title.raycastTarget = true;
        row.title.gameObject.AddComponent<Button>();
        ModHubStyle.HoverTip(row.title.gameObject);
        ModHubStyle.PassScroll(row.title.gameObject);
        ModHubStyle.Size(row.title.gameObject, pHeight: 12);
        ModHubStyle.Divider(body.transform);
        CreateControlLine(row, body.transform);
        return row;
    }

    private static void CreateControlLine(ModBrowserRow pRow, Transform pBody)
    {
        var line = new GameObject("ControlLine", typeof(RectTransform));
        line.transform.SetParent(pBody, false);
        ModHubStyle.Horizontal(line, ModHubStyle.SpaceS);
        ModHubStyle.Size(line, pHeight: ModHubStyle.IconButtonSize);

        pRow.status = ModHubStyle.Label(line.transform, "Status", ModHubStyle.FontSmall, ModHubStyle.TextMuted);
        ModHubStyle.Size(pRow.status.gameObject, 0, pFlexibleWidth: 1);
        pRow.age = ModHubStyle.PillLabel(line.transform, "Age", 48);
        pRow.download = ModHubStyle.IconButton(line.transform, "Download", ModHubUI.GameIcon("iconArrowDOWN"), null,
            "mod_hub_download");
        pRow.info = ModHubStyle.IconButton(line.transform, "Info", ModHubUI.GameIcon("iconQuestionMark"), null,
            "mod_hub_workshop_page");
        ModHubStyle.PassScroll(pRow.download.gameObject);
        ModHubStyle.PassScroll(pRow.info.gameObject);
    }
}