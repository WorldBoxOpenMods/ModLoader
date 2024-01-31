using System.Net;
using System.Net.Http;
using System.Text;
using NeoModLoader.services;

namespace NeoModLoader.utils;

/// <summary>
/// This class is made as utility to make http request easier. Maybe not, just for myself --inmny.
/// </summary>
public static class HttpUtils
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <param name="headers"></param>
    /// <returns></returns>
    public static HttpResponseMessage Get(string url, Dictionary<string, string> headers)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Clear();
        foreach (var header in headers)
        {
            client.DefaultRequestHeaders.Add(header.Key, header.Value);
        }

        return client.GetAsync(url).Result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <param name="params"></param>
    /// <param name="headers"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public static string Post(string                     url,            Dictionary<string, string> @params,
                              Dictionary<string, string> headers = null, double                     timeout = 30)
    {
        using var client = new HttpClient();
        var content = new FormUrlEncodedContent(@params);
        if (headers != null)
        {
            client.DefaultRequestHeaders.Clear();
            foreach (var header in headers)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }

        client.Timeout = TimeSpan.FromSeconds(timeout);
        try
        {
            HttpResponseMessage response = client.PostAsync(url, content).Result;
            return response.StatusCode == HttpStatusCode.OK ? response.Content.ReadAsStringAsync().Result : "";
        }
        catch (Exception e)
        {
            LogService.LogErrorConcurrent(e.Message);
            LogService.LogErrorConcurrent(e.StackTrace);
        }

        return "";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <param name="param"></param>
    /// <param name="method"></param>
    /// <returns></returns>
    public static string Request(string url, string param = "", string method = "get")
    {
        ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; //TLS1.2=3702

        string result = "";
        HttpWebRequest req = WebRequest.Create(url) as HttpWebRequest;
        HttpWebResponse res = null;
        if (req == null) return result;
        req.Method = method;
        req.ContentType = @"application/octet-stream";
        req.UserAgent =
            @"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.133 Safari/537.36";
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
                Stream InputStream = res.GetResponseStream();
                Encoding encoding = Encoding.GetEncoding("UTF-8");
                StreamReader sr = new StreamReader(InputStream, encoding);
                result = sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                LogService.LogErrorConcurrent(ex.Message);
                return result;
            }
        }
        else
        {
            try
            {
                res = (HttpWebResponse)req.GetResponse();
                Stream InputStream = res.GetResponseStream();
                Encoding encoding = Encoding.GetEncoding("UTF-8");
                StreamReader sr = new StreamReader(InputStream, encoding);
                result = sr.ReadToEnd();
                sr.Close();
            }
            catch (Exception ex)
            {
                LogService.LogErrorConcurrent(ex.Message);
                return result;
            }
        }

        return result;
    }
}