using System.Net;
using NeoModLoader.constants;
using NeoModLoader.utils;
using Newtonsoft.Json;

namespace NeoModLoader.services;

internal static class NMLAutoUpdateService
{
    private static readonly string API_URL = $"https://api.github.com/repos/{CoreConstants.OrgName}/{CoreConstants.RepoName}/releases/latest";

    class VersionInfo
    {
        [JsonProperty("name")]
        public string Name { get; private set; }
        [JsonProperty("prerelease")]
        public bool IsPrerelease { get; private set; }
        [JsonProperty("assets")]
        public ReleaseAsset[] Assets { get; private set; }
    }

    class ReleaseAsset
    {
        [JsonProperty("name")]
        public string Name { get; private set; }
        [JsonProperty("size")]
        public string Size { get; private set; }
        [JsonProperty("browser_download_url")]
        public string DownloadUrl { get; private set; }
    }
    public static bool CheckUpdate()
    {
        VersionInfo info = JsonConvert.DeserializeObject<VersionInfo>(HttpUtils.Request(API_URL));
        return false;
    }

    public static void DownloadNewest()
    {
        
    }
}