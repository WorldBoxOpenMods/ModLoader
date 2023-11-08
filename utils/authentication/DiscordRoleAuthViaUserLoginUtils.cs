using System.Net;
using System.Text;
using NeoModLoader.constants;
using Newtonsoft.Json.Linq;
using UnityEngine;
namespace NeoModLoader.utils.authentication;

public class DiscordRoleAuthViaUserLoginUtils
{
    private struct TokenInfo
    {
        public string access_token;
        public string token_type;
        public string expires_in;
        public string refresh_token;
        public string scope;
    }
    private const string client_id = Setting.discord_auth_client_id;

    public static bool Authenticate()
    {
        return false;
    }

    public static void Test()
    {
        TokenInfo tokenInfo = GetAuthToken();
        System.Diagnostics.Debug.WriteLine(tokenInfo.access_token);
    }
    private static TokenInfo GetAuthToken()
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:36549/");
        listener.Start();

        System.Diagnostics.Process.Start("https://discord.com/api/oauth2/authorize?client_id=" + client_id + "&redirect_uri=http%3A%2F%2Flocalhost%3A36549&response_type=code&scope=identify");
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
        const string response_text = "<html><head><title>NeoModLoader</title><style>body {background-color: black; color: white;}</style></head><body>You can close this page!</body></html>";
        response.OutputStream.Write(response_text.ToCharArray().Select((c => (byte)c)).ToArray(), 0, response_text.Length);
        response.Close();
        string code = request.QueryString["code"];
        System.Diagnostics.Debug.WriteLine(code);
        listener.Close();
        string res = HttpUtils.Post("https://discord.com/api/oauth2/token", new Dictionary<string, string>()
        {
            { "code", code },
            { "grant_type", "authorization_code" },
            { "redirect_uri", "http://localhost:36549" },
            { "scope", "identify" }
        }, new Dictionary<string, string>()
        {
            { "Accept", "application/json" },
            { "Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(client_id + ":" + "NOT_COMMITING_THIS_THING :3")) }, // TODO: MY GOD PLEASE STORE THIS SECRET MORE SAFELY BEFORE COMMITING
        });
        System.Diagnostics.Debug.WriteLine(res);
        Console.WriteLine(res);
        string[] resSegments = res.Split(',');
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
}