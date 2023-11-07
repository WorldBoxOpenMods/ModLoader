using System.Net;
using NeoModLoader.constants;

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
    private const string client_secret = Setting.github_auth_client_secret;
    public static bool Authenticate()
    {
        string token = GetToken();

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

    private static string GetToken()
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8000/");
        listener.Start();
        
        System.Diagnostics.Process.Start("https://github.com/login/oauth/authorize?client_id=" + client_id);
        
        HttpListenerContext context = listener.GetContext();
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
}