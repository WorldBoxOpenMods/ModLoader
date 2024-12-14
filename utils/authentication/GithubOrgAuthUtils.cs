using System.Net;
using System.Net.Http;
using NeoModLoader.constants;
using NeoModLoader.General;
using NeoModLoader.ui;
using Newtonsoft.Json;
using UnityEngine;

namespace NeoModLoader.utils.authentication;

/// <summary>
///     This class is used to authenticate user by Github organization.
/// </summary>
public static class GithubOrgAuthUtils
{
    private const  string client_id = Setting.github_auth_client_id;
    private static string domain = "github.com";

    private static readonly string[] _alter_domains =
    {
        "github.com"
    };

    /// <summary>
    ///     Get authentication result.
    /// </summary>
    /// <remarks>It will block thread</remarks>
    public static bool Authenticate()
    {
        string token = GetTokenByDeviceFlow();

        if (string.IsNullOrEmpty(token)) return false;

        HttpResponseMessage res = HttpUtils.Get($"https://api.{domain}/user", new Dictionary<string, string>
        {
            { "Authorization", "Bearer " + token },
            { "User-Agent", "NeoModLoader" }
        });
        UserInfo info = JsonConvert.DeserializeObject<UserInfo>(res.Content.ReadAsStringAsync().Result);

        res = HttpUtils.Get($"https://api.{domain}/orgs/{CoreConstants.OrgName}/members/{info.login}",
            new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + token },
                { "User-Agent", "NeoModLoader" },
                { "Accept", "application/vnd.github.v3+json" }
            });

        // https://docs.github.com/zh/rest/orgs/members?apiVersion=2022-11-28#check-organization-membership-for-a-user
        if (res.StatusCode == HttpStatusCode.NoContent)
        {
            return true;
        }


        return false;
    }


    private static string GetTokenByDeviceFlow()
    {
        var res = "";
        foreach (var alter_domain in _alter_domains)
            try
            {
                res = HttpUtils.Post($"https://{alter_domain}/login/device/code", new Dictionary<string, string>
                {
                    { "client_id", client_id }
                }, new Dictionary<string, string>
                {
                    { "Accept", "application/json" }
                }, 5);
                if (!string.IsNullOrEmpty(res))
                {
                    domain = alter_domain;
                    break;
                }
            }
            catch (Exception)
            {
                // ignored
            }

        if (string.IsNullOrEmpty(res)) throw new AuthenticaticationException("Failed to get device code.");
        DeviceFlow flow = JsonConvert.DeserializeObject<DeviceFlow>(res);
        InformationWindow.ShowWindow(string.Format(LM.Get("GithubAuth Tip"), flow.user_code));
        Application.OpenURL(flow.verification_uri);

        int wait_time = 0;
        while (wait_time < flow.expires_in * 1000)
        {
            Thread.Sleep(flow.interval * 1000);
            wait_time += flow.interval * 1000;
            res = HttpUtils.Post($"https://{domain}/login/oauth/access_token", new Dictionary<string, string>
            {
                { "client_id", client_id },
                { "device_code", flow.device_code },
                { "grant_type", "urn:ietf:params:oauth:grant-type:device_code" }
            }, new Dictionary<string, string>()
            {
                { "Accept", "application/json" }
            });
            if (res.Contains("access_token"))
            {
                break;
            }
        }

        InformationWindow.Back();

        return JsonConvert.DeserializeObject<TokenInfo>(res).access_token;
    }
#pragma warning disable CS0649 // They are assigned by Newtonsoft.Json.JsonConvert.DeserializeObject<T>
    struct TokenInfo
    {
        public string access_token;
        public string token_type;
        public string scope;
    }

    struct UserInfo
    {
        public string login;
    }

    private struct DeviceFlow
    {
        public string device_code;
        public string user_code;
        public string verification_uri;
        public int    interval;
        public int    expires_in;
    }
#pragma warning restore CS0649 // They are assigned by Newtonsoft.Json.JsonConvert.DeserializeObject<T>
}