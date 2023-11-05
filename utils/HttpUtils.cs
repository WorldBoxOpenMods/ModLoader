using System.Net;
using System.Text;
using NeoModLoader.services;

namespace NeoModLoader.utils;

public static class HttpUtils
{
    public static string Request(string url, string param = "", string method = "get")
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;//TLS1.2=3702

            string result = "";
            HttpWebRequest req = WebRequest.Create(url) as HttpWebRequest;
            HttpWebResponse res = null;
            if (req == null) return result;
            req.Method = method;
            req.ContentType = @"application/octet-stream";
            req.UserAgent = @"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.133 Safari/537.36";
            byte[] postData = Encoding.GetEncoding("UTF-8").GetBytes(param);
            if (postData.Length > 0)
            {
                req.ContentLength = postData.Length;
                req.Timeout = 15000;
                Stream outputStream = req.GetRequestStream();
                outputStream.Write(postData, 0, postData.Length);
                outputStream.Flush();
                outputStream.Close();
                try
                {
                    res = (HttpWebResponse)req.GetResponse();
                    System.IO.Stream InputStream = res.GetResponseStream();
                    Encoding encoding = Encoding.GetEncoding("UTF-8");
                    StreamReader sr = new StreamReader(InputStream, encoding);
                    result = sr.ReadToEnd();
                }
                catch (Exception ex)
                {
                    LogService.LogError(ex.Message);
                    return result;
                }
            }
            else
            {
                try
                {
                    res = (HttpWebResponse)req.GetResponse();
                    System.IO.Stream InputStream = res.GetResponseStream();
                    Encoding encoding = Encoding.GetEncoding("UTF-8");
                    StreamReader sr = new StreamReader(InputStream, encoding);
                    result = sr.ReadToEnd();
                    sr.Close();
                }
                catch (Exception ex)
                {
                    LogService.LogError(ex.Message);
                    return result;
                }
            }
            return result;
    }
}