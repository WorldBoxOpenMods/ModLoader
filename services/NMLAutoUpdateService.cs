using System.Net;
using NeoModLoader.api;
using NeoModLoader.constants;
using NeoModLoader.General;
using NeoModLoader.ui;
using NeoModLoader.utils;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.services;

internal static class NMLAutoUpdateService
{
    private static readonly string RELEASE_APIURL =
        $"https://api.github.com/repos/{CoreConstants.OrgName}/{CoreConstants.RepoName}/releases/latest";

    private static readonly string COMMMIT_APIURL =
        $"https://api.github.com/repos/{CoreConstants.OrgName}/{CoreConstants.RepoName}/commits/tag_name";

    public static void CheckWorkshopUpdate()
    {
        string workshopPath = Path.Combine(Paths.CommonModsWorkshopPath, CoreConstants.WorkshopFileId.ToString());
        if (!Directory.Exists(workshopPath))
        {
            return;
        }

        string[] files = Directory.GetFiles(workshopPath);
        string dll_path = null;
        string pdb_path = null;
        foreach (string file in files)
        {
            if (file.EndsWith(".dll"))
            {
                dll_path = file;
            }

            if (file.EndsWith(".pdb"))
            {
                pdb_path = file;
            }
        }

        if (dll_path == null)
        {
            return;
        }

        bool updated = false;
        FileInfo dll_info = new FileInfo(dll_path);
        FileInfo last_info = new FileInfo(Paths.NMLModPath);

        void update_in_turn()
        {
            if (!last_info.Exists || dll_info.LastWriteTime <= last_info.LastWriteTime) return;
            try
            {
                last_info.Delete();
                File.Copy(dll_path, Paths.NMLModPath, true);
                updated = true;
                LogService.LogInfo($"{dll_info.LastWriteTime} : {last_info.LastWriteTime}");
            }
            catch (UnauthorizedAccessException)
            {
                if (Paths.NMLModPath.EndsWith("_memload.dll"))
                {
                    FileInfo another_one = new FileInfo(Paths.NMLModPath.Replace("_memload.dll", ".dll"));
                    if (another_one.Exists)
                    {
                        another_one.Delete();
                    }

                    File.Copy(dll_path, Paths.NMLModPath.Replace("_memload.dll", ".dll"), true);
                }
                else
                {
                    FileInfo another_one = new FileInfo(Paths.NMLModPath.Replace(".dll", "_memload.dll"));
                    if (another_one.Exists)
                    {
                        another_one.Delete();
                    }

                    File.Copy(dll_path, Paths.NMLModPath.Replace(".dll", "_memload.dll"), true);
                }
            }
        }

        update_in_turn();
        if (Paths.NMLModPath.Contains("_memload.dll"))
        {
            last_info = new FileInfo(Paths.NMLModPath.Replace("_memload.dll", ".dll"));
        }
        else
        {
            last_info = new FileInfo(Paths.NMLModPath.Replace(".dll", "_memload.dll"));
        }

        update_in_turn();

        FileInfo pdb_info = pdb_path == null ? null : new FileInfo(pdb_path);
        FileInfo last_pdb_info = new FileInfo(Paths.NMLModPath.Replace("_memload.dll", ".dll").Replace(".dll", ".pdb"));
        if (pdb_info != null && (!last_pdb_info.Exists || pdb_info.LastWriteTime > last_pdb_info.LastWriteTime))
        {
            try
            {
                if (last_pdb_info.Exists)
                    last_pdb_info.Delete();
                File.Copy(pdb_path, Paths.NMLModPath.Replace("_memload.dll", ".dll").Replace(".dll", ".pdb"), true);
                updated = true;
            }
            catch (UnauthorizedAccessException)
            {
                // ignored
            }
        }

        if (updated)
        {
            InformationWindow.ShowWindow(LM.Get("NeoModLoader Updated"));
        }
    }

    public static bool CheckUpdate()
    {
        VersionInfo info = JsonConvert.DeserializeObject<VersionInfo>(HttpUtils.Request(RELEASE_APIURL));
        if (info == null)
        {
            return false;
        }

        if (info.IsPrerelease) return false;
        RefInfo refInfo =
            JsonConvert.DeserializeObject<RefInfo>(HttpUtils.Request(COMMMIT_APIURL.Replace("tag_name", info.TagName)));

        var s = WorldBoxMod.NeoModLoaderAssembly.GetManifestResourceStream("NeoModLoader.resources.commit");

        string current_commit = new StreamReader(s).ReadToEnd();

        s.Close();
        current_commit = current_commit.Replace("\n", "").Replace("\r", "");

        if (current_commit == refInfo.sha)
        {
            return false;
        }

        PopupUpdateWindow(info, refInfo.sha);
        return true;
    }

    private static void PopupUpdateWindow(VersionInfo info, string commit)
    {
        UpdateWindow.CreateAndInit("NeoModLoader Update").Show(info, commit);
    }

    class VersionInfo
    {
        [JsonProperty("name")] public string Name { get; private set; }

        [JsonProperty("tag_name")] public string TagName { get; private set; }

        [JsonProperty("body")] public string Content { get; private set; }

        [JsonProperty("prerelease")] public bool IsPrerelease { get; private set; }

        [JsonProperty("assets")] public ReleaseAsset[] Assets { get; private set; }
    }

    class RefInfo
    {
        [JsonProperty("sha")] public string sha { get; private set; }
    }

    class ReleaseAsset
    {
        [JsonProperty("name")] public string Name { get; private set; }

        [JsonProperty("size")] public string Size { get; private set; }

        [JsonProperty("browser_download_url")] public string DownloadUrl { get; private set; }
    }

    class UpdateWindow : AbstractWindow<UpdateWindow>
    {
        private Text content_text;
        private string dll_download_path;
        private bool downloaded;
        private string downloaded_version_name;
        private string pdb_download_path;
        private Text title_text;
        private Button update_button;

        private void Update()
        {
            if (downloaded)
            {
                WorldTip.showNowTop(string.Format(LM.Get("NeoModLoader Update Complete"), downloaded_version_name));
                update_button.transform.Find("Icon").GetComponent<Image>().sprite =
                    SpriteTextureLoader.getSprite("ui/icons/iconOn");
                downloaded = false;
            }
        }

        protected override void Init()
        {
            ContentSizeFitter fitter = ContentTransform.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            VerticalLayoutGroup layoutGroup = ContentTransform.gameObject.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.spacing = 3;
            layoutGroup.padding = new(30, 30, 10, 10);


            GameObject update_title = new GameObject("Update Title", typeof(Text));
            update_title.transform.SetParent(ContentTransform);
            title_text = update_title.GetComponent<Text>();
            OT.InitializeCommonText(title_text);
            title_text.alignment = TextAnchor.MiddleCenter;
            title_text.transform.localScale = Vector3.one;
            update_title.GetComponent<RectTransform>().sizeDelta = new(0, 24);

            GameObject update_content = new GameObject("Update Content", typeof(Text));
            update_content.transform.SetParent(ContentTransform);
            content_text = update_content.GetComponent<Text>();
            OT.InitializeCommonText(content_text);
            content_text.resizeTextForBestFit = true;
            content_text.alignment = TextAnchor.UpperCenter;
            content_text.transform.localScale = Vector3.one;
            content_text.GetComponent<RectTransform>().sizeDelta = new(0, 150);

            GameObject update_button_obj = new GameObject("Update Button", typeof(Image), typeof(Button));
            update_button_obj.transform.SetParent(ContentTransform);
            update_button_obj.transform.localScale = Vector3.one;
            update_button = update_button_obj.GetComponent<Button>();
            Vector2 button_size = new Vector2(30, 30);
            update_button.GetComponent<RectTransform>().sizeDelta = button_size;
            update_button.GetComponent<Image>().sprite = SpriteTextureLoader.getSprite("ui/special/button2");
            update_button.GetComponent<Image>().type = Image.Type.Sliced;
            GameObject update_button_icon_obj = new GameObject("Icon", typeof(Image));
            update_button_icon_obj.transform.SetParent(update_button_obj.transform);
            update_button_icon_obj.GetComponent<RectTransform>().sizeDelta = button_size;
            update_button_icon_obj.GetComponent<Image>().sprite = Resources.Load<Sprite>("ui/icons/iconSteam");
        }

        public void Show(VersionInfo info, string commit)
        {
            title_text.text = info.Name;
            content_text.text = info.Content;
            update_button.onClick.AddListener(() =>
            {
                update_button.onClick.RemoveAllListeners();
                string download_path = Path.GetTempPath();
                dll_download_path = Path.Combine(download_path, $"NML_{commit}.dll");
                pdb_download_path = Path.Combine(download_path, $"NML_{commit}.pdb");
                new Task(() =>
                {
                    using WebClient client = new WebClient();
                    foreach (ReleaseAsset asset in info.Assets)
                    {
                        if (asset.Name.EndsWith(".dll"))
                        {
                            client.DownloadFile(new Uri(asset.DownloadUrl), dll_download_path);
                        }

                        if (asset.Name.EndsWith(".pdb"))
                        {
                            client.DownloadFile(new Uri(asset.DownloadUrl), pdb_download_path);
                        }
                    }

                    if (File.Exists(dll_download_path))
                    {
                        File.Delete(Paths.NMLModPath);
                        File.Copy(dll_download_path, Paths.NMLModPath);
                    }

                    if (File.Exists(pdb_download_path))
                    {
                        File.Delete(Paths.NMLModPath.Replace(".dll", ".pdb"));
                        File.Copy(pdb_download_path, Paths.NMLModPath.Replace(".dll", ".pdb"));
                    }

                    downloaded_version_name = info.Name;
                    downloaded = true;
                }).Start();
            });
            ScrollWindow.showWindow("NeoModLoader Update");
        }
    }
}