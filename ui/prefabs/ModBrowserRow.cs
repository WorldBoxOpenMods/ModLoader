using System.Globalization;
using NeoModLoader.api;
using NeoModLoader.General;
using NeoModLoader.General.UI;
using NeoModLoader.General.UI.Prefabs;
using NeoModLoader.services;
using NeoModLoader.utils;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.ui.prefabs;

/// <summary>
///     One Workshop mod in <see cref="ModBrowserPage" />: thumbnail, title with details on hover, status,
///     when it was updated, and its buttons.
/// </summary>
/// <remarks>
///     Like <see cref="ModManagerRow" />, the prefab holds the visuals and <see cref="Awake" /> adds the handlers.
/// </remarks>
internal class ModBrowserRow : ModHubRow<ModListing>
{
    [SerializeField] private Text status;
    [SerializeField] private Text age;
    [SerializeField] private SimpleButton info;
    [SerializeField] private SimpleButton download;

    private ModListing listing;

    private void Awake()
    {
        info.Button.onClick.AddListener(() => Application.OpenURL($"steam://url/CommunityFilePage/{listing.Id}"));
        download.Button.onClick.AddListener(OnDownloadClicked);
        Title.GetComponent<Button>().onClick.AddListener(() => info.Button.onClick.Invoke());
        Title.GetComponent<TipButton>().SetTextTooltip(() => listing.Title, DescribeListing);
    }

    public override void Setup(ModListing pListing)
    {
        listing = pListing;
        Title.text = pListing.Title;
        age.text = DescribeAge(DateTime.UtcNow - pListing.Updated);
        Icon.sprite = InternalResourcesGetter.GetIcon();
        RequestPreview();
        UpdateStatus();
    }

    private void OnEnable() => RequestPreview();

    private void OnDisable() => WorkshopThumbnails.Release(this);

    private void OnDestroy() => WorkshopThumbnails.Release(this);

    private void RequestPreview()
    {
        if (listing == null || !isActiveAndEnabled) return;
        ModListing current = listing;
        Icon.sprite = InternalResourcesGetter.GetIcon();
        WorkshopThumbnails.Request(current.PreviewUrl, this, sprite =>
        {
            // The pooled row may show another listing by the time the image arrives.
            if (this != null && isActiveAndEnabled && listing == current) Icon.sprite = sprite;
        });
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
            _ => LM.Get("mod_hub_listing_" + listing_status.ToString().ToLowerInvariant())
        };

        bool busy = listing_status is ListingStatus.Queued or ListingStatus.Downloading;
        download.gameObject.SetActive(listing_status != ListingStatus.Installed);
        download.Icon.sprite = SpriteTextureLoader.getSprite(busy ? "ui/icons/iconClose" : "ui/icons/iconArrowDOWN");
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

        ModBrowserPage.Instance.Download(listing);
    }

    private string DescribeListing()
    {
        uint votes = listing.VotesUp + listing.VotesDown;
        string rating = votes == 0
            ? LM.Get("mod_hub_no_rating")
            : string.Format(LM.Get("mod_hub_rating"), Mathf.RoundToInt(listing.Score * 100), votes);
        string categories = string.Join(", ", listing.Categories.Select(category => LM.Get("ModTag_" + category)));
        return string.Join("\n",
            string.Format(LM.Get("mod_hub_by_author"), ModDownloadService.Authors.Get(listing.AuthorId) ?? "..."),
            string.Format(LM.Get("mod_hub_subscribers"),
                listing.Subscriptions.ToString("N0", CultureInfo.InvariantCulture)),
            rating,
            string.Format(LM.Get("mod_hub_dates"), FormatDate(listing.Updated), FormatDate(listing.Created)),
            string.Format(LM.Get("mod_hub_categories"),
                categories.Length > 0 ? categories : LM.Get("mod_hub_categories_none")),
            string.Format(LM.Get("mod_hub_ai_content"), LM.Get("mod_hub_ai_" + listing.AIContent.ToString().ToLowerInvariant())));
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
            ListingStatus.UpdateAvailable => new Color(1f, 0.72f, 0.25f),
            ListingStatus.Queued or ListingStatus.Downloading => new Color(0.35f, 0.85f, 1f),
            ListingStatus.Failed => new Color(1f, 0.42f, 0.36f),
            _ => Color.gray
        };
    }

    /// <summary>
    ///     Build the row prefab that the pool of <see cref="AbstractListWindow{T,TItem}" /> clones.
    /// </summary>
    internal static ModBrowserRow CreatePrefab()
    {
        ModBrowserRow row = CreateRow<ModBrowserRow>();
        CreateControlLine(row, row.Controls);
        return row;
    }

    private static void CreateControlLine(ModBrowserRow pRow, Transform pBody)
    {
        pRow.status = OT.CreateText(pBody, "Status", 7, pColor: Color.gray);
        pRow.status.gameObject.SetLayoutSize(pFlexibleWidth: 1);
        var age = new GameObject("Age", typeof(Image));
        age.transform.SetParent(pBody, false);
        age.GetComponent<Image>().sprite = SpriteTextureLoader.getSprite("ui/special/darkInputFieldEmpty");
        age.GetComponent<Image>().type = Image.Type.Sliced;
        age.GetComponent<Image>().raycastTarget = false;
        age.SetLayoutSize(48, 16);
        pRow.age = OT.CreateText(age.transform, "Text", 7, TextAnchor.MiddleCenter, Color.gray);
        pRow.age.rectTransform.Stretch();
        pRow.download = pRow.AddAction("Download", SpriteTextureLoader.getSprite("ui/icons/iconArrowDOWN"),
            "mod_hub_download");
        pRow.info = pRow.AddAction("Info", SpriteTextureLoader.getSprite("ui/icons/iconQuestionMark"),
            "mod_hub_workshop_page");
    }
}
