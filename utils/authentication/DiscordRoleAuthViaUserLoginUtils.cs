using System.Net;
using NeoModLoader.constants;
namespace NeoModLoader.utils.authentication;

public class DiscordRoleAuthViaUserLoginUtils
{
    private const string client_id = Setting.discord_auth_client_id;

    public static bool Authenticate()
    {
        return false;
    }

    public static void Test()
    {
        GetAuthToken();
    }
    public static string GetAuthToken()
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:36549/");
        listener.Start();
        
        System.Diagnostics.Process.Start("https://discord.com/api/oauth2/authorize?client_id=" + client_id + "&redirect_uri=http%3A%2F%2Flocalhost%3A36549&response_type=code&scope=identify");
        new Task(() =>
        {
            HttpListener listener_ref = listener;
            int waitTime = 0;
            while(waitTime < 60000)
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
        const string response_text = "<html><head><title>NeoModLoader</title></head><body>You can close this page!</body></html>";
        response.OutputStream.Write(response_text.ToCharArray().Select((c => (byte) c)).ToArray(), 0, response_text.Length);
        response.Close();
        string code = request.QueryString["code"];
        System.Diagnostics.Debug.WriteLine(code);
        listener.Close();
        /*
        string res = HttpUtils.Post("https://github.com/login/oauth/access_token", new Dictionary<string, string>()
        {
            { "client_id", client_id },
            { "client_secret", client_secret },
            { "code", code }
        }, new Dictionary<string, string>()
        {
            { "Accept", "application/json" }
        });
        */
        return "";
    }
}