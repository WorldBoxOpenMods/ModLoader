using System.Net;
using NeoModLoader.constants;
using NeoModLoader.General;
using NeoModLoader.ui;
using UnityEngine;

namespace NeoModLoader.utils.authentication;

public static class GithubOrgAuthUtils
{
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
    private const string client_id = Setting.github_auth_client_id;
    public static bool Authenticate()
    {
        string token = GetTokenByDeviceFlow();

        if (string.IsNullOrEmpty(token)) return false;

        HttpResponseMessage res = HttpUtils.Get("https://api.github.com/user", new Dictionary<string, string>()
        {
            { "Authorization", "Bearer " + token },
            { "User-Agent", "NeoModLoader" }
        });
        UserInfo info = Newtonsoft.Json.JsonConvert.DeserializeObject<UserInfo>(res.Content.ReadAsStringAsync().Result);

        res = HttpUtils.Get($"https://api.github.com/orgs/{CoreConstants.OrgName}/members/{info.login}",
            new Dictionary<string, string>()
            {
                { "Authorization", "Bearer " + token },
                { "User-Agent", "NeoModLoader"},
                { "Accept", "application/vnd.github.v3+json" }
            });
        
        // https://docs.github.com/zh/rest/orgs/members?apiVersion=2022-11-28#check-organization-membership-for-a-user
        if (res.StatusCode == HttpStatusCode.NoContent)
        {
            return true;
        }
        
        
        return false;
    }
    struct DeviceFlow
    {
        public string device_code;
        public string user_code;
        public string verification_uri;
        public int interval;
        public int expires_in;
    }
    private static string GetTokenByDeviceFlow()
    { 
        string res = HttpUtils.Post("https://github.com/login/device/code", new Dictionary<string, string>()
        {
            { "client_id", client_id }
        }, new Dictionary<string, string>()
        {
            { "Accept", "application/json" }
        });
        DeviceFlow flow = Newtonsoft.Json.JsonConvert.DeserializeObject<DeviceFlow>(res);
        InformationWindow.ShowWindow(string.Format(LM.Get("GithubAuth Tip"), flow.user_code));
        Application.OpenURL(flow.verification_uri);
        
        int wait_time = 0;
        while (wait_time < flow.expires_in * 1000)
        {
            Thread.Sleep(flow.interval * 1000);
            wait_time += flow.interval * 1000;
            res = HttpUtils.Post("https://github.com/login/oauth/access_token", new Dictionary<string, string>()
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

        return Newtonsoft.Json.JsonConvert.DeserializeObject<TokenInfo>(res).access_token;
    }
    /*
    private static string GetToken()
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8000/");
        listener.Start();
        
        System.Diagnostics.Process.Start("https://github.com/login/oauth/authorize?client_id=" + client_id);
        new Task(() =>
        {
            HttpListener listener_ref = listener;
            int waitTime = 0;
            while(waitTime < 30000)
            {
                if (listener_ref.IsListening)
                {
                    waitTime += 100;
                    Thread.Sleep(100);
                }
                else
                {
                    return;
                }
            }
            listener_ref.Close();
        }).Start();
        HttpListenerContext context;
        try
        {
            context = listener.GetContext();
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;
        string code = request.QueryString["code"];
        System.Diagnostics.Debug.WriteLine(code);
        listener.Close();
        
        string res = HttpUtils.Post("https://github.com/login/oauth/access_token", new Dictionary<string, string>()
        {
            { "client_id", client_id },
            { "client_secret", client_secret },
            { "code", code }
        }, new Dictionary<string, string>()
        {
            { "Accept", "application/json" }
        });
        
        return Newtonsoft.Json.JsonConvert.DeserializeObject<TokenInfo>(res).access_token;
    }
    */
}