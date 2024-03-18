using System.Net;
using System.Net.Http;
using AuthenticationException = NeoModLoader.utils.authentication.AuthenticaticationException;
using NeoModLoader.constants;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace NeoModLoader.utils.authentication;

public class DiscordRoleAuthViaUserLoginUtils
{
    private const string client_id = Setting.discord_auth_client_id;

    public static bool Authenticate() =>
        DiscordCommonAuthLogic.ModderIsInRolesList(DiscordCommonAuthLogic.GetRolesOfUser(GetUserID(GetAuthToken())));

    public static void Test()
    {
        TokenInfo token_info = GetAuthToken();
        Debug.WriteLine(token_info.access_token);
        string user_id = GetUserID(token_info);
        Debug.WriteLine(user_id);
        var roles = DiscordCommonAuthLogic.GetRolesOfUser(user_id);
        bool result = DiscordCommonAuthLogic.ModderIsInRolesList(roles);
        Debug.WriteLine(result);
        if (result) Console.WriteLine("You are a modder!");
        else Console.WriteLine("You are not a modder!");
        Console.WriteLine("Tests:");
        roles = DiscordCommonAuthLogic.GetRolesOfUser("1171719697557880892");
        roles.ToList().ForEach(Console.WriteLine);
        roles = DiscordCommonAuthLogic.GetRolesOfUser("0000000000000000000");
        roles.ToList().ForEach(Console.WriteLine);
    }

    private static string GetUserID(TokenInfo token_info)
    {
        HttpResponseMessage response = HttpUtils.Get("https://discordapp.com/api/users/@me",
            new Dictionary<string, string>()
            {
                { "Authorization", token_info.token_type + " " + token_info.access_token }
            });
        string res_json = response.Content.ReadAsStringAsync().Result;
        string[] res_segments = res_json.Trim(' ', 'd', 'a', 't', 'a', ':', '{', '}').Split(',');
        foreach (string[] pair in res_segments.Select(segment => segment.Split(':'))
                     .Where(pair => pair[0].Trim('"', ' ') == "id"))
        {
            return pair[1].Trim('"', ' ');
        }

        return "";
    }

    private static TokenInfo GetAuthToken()
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:36549/");
        listener.Start();

        Application.OpenURL("https://discord.com/api/oauth2/authorize?client_id=" + client_id + "&redirect_uri=http%3A%2F%2Flocalhost%3A36549&response_type=code&scope=identify");
        new Task(() =>
        {
            HttpListener listener_ref = listener;
            int waitTime = 0;
            while (waitTime < 60000)
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
        catch (InvalidOperationException e)
        {
            throw new Exception("Failed to get context", e);
        }
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;
        
        string code;
        string response_text;
        try
        {
            code = request.QueryString["code"];
            response_text = "<html><head><title>NeoModLoader</title><style>body {background-color: black; color: white;}</style></head><body>Success!<br>You can close this page!</body></html>";
            response.OutputStream.Write(response_text.ToCharArray().Select((c => (byte)c)).ToArray(), 0, response_text.Length);
        }
        catch (Exception)
        {
            response_text = "<html><head><title>NeoModLoader</title><style>body {background-color: black; color: white;}</style></head><body>Error!<br>Authentication declined!</body></html>";
            UnityEngine.Debug.LogWarning("Manual Discord Authentication declined!");
            response.OutputStream.Write(response_text.ToCharArray().Select((c => (byte)c)).ToArray(), 0, response_text.Length);
            throw new AuthenticationException("Discord user authentication declined.");
        }
        response.Close();
        Debug.WriteLine(code);
        listener.Close();
        HttpResponseMessage res;
        using (HttpClient client = new HttpClient())
        {
            res = client.GetAsync("https://keymasterer.uk/nml/api/get-discord-access-token/" + code).Result;
        }

        string resJson = res.Content.ReadAsStringAsync().Result;
        Debug.WriteLine(resJson);
        Console.WriteLine(resJson);
        string[] resSegments = resJson.Split(',');
        TokenInfo result = new TokenInfo
        {
            token_type = resSegments[0].Split(':')[1].Trim('"', ' '),
            access_token = resSegments[1].Split(':')[1].Trim('"', ' '),
            expires_in = resSegments[2].Split(':')[1].Trim('"', ' '),
            refresh_token = resSegments[3].Split(':')[1].Trim('"', ' '),
            scope = resSegments[4].Split(':')[1].Trim('"', ' ', '}')
        };
        return result;
    }

    private struct TokenInfo
    {
        public string access_token;
        public string token_type;
        public string expires_in;
        public string refresh_token;
        public string scope;
    }
}