using System;
using System.IO;
using System.Net;
using System.Web;
using System.Text;
using System.Net.Security;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

public class Web
{
    public static string HttpGet(string url)
    {
        HttpWebResponse resp = HttpWebResponseUtility.CreateGetHttpResponse(url,null,null,new CookieCollection());
        Stream respStream = resp.GetResponseStream();
        StreamReader respStreamReader = new StreamReader(respStream,Encoding.UTF8);
        string strBuff = "";
        char[] cbuffer = new char[256];
        int byteRead = 0;
        byteRead = respStreamReader.Read(cbuffer,0,256);
        while(byteRead != 0)
        {
            string strResp = new string(cbuffer,0,byteRead);
            strBuff = strBuff + strResp;
            byteRead = respStreamReader.Read(cbuffer,0,256);
        }
        respStream.Close();
        return strBuff;
    }

    public static Stream HttpGetStream(string url,out long content_length)
    {
        HttpWebResponse resp = HttpWebResponseUtility.CreateGetHttpResponse(url,null,null,new CookieCollection());
        content_length = resp.ContentLength;
        return resp.GetResponseStream();
    }

    public class HttpWebResponseUtility
    {
        public static WebProxy proxy = null;

        public static readonly string DefaultUserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)";

        public static HttpWebResponse CreateGetHttpResponse(string url,int? timeout,string userAgent,CookieCollection cookies)
        {
            if(string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "GET";
            request.UserAgent = DefaultUserAgent;
            request.Credentials = CredentialCache.DefaultCredentials;
            request.AllowAutoRedirect = false;
            if(proxy != null)
            {
                request.Proxy = proxy;
            }
            if(!string.IsNullOrEmpty(userAgent))
            {
                request.UserAgent = userAgent;
            }
            if(timeout.HasValue)
            {
                request.Timeout = timeout.Value;
            }
            if(cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }
            if(url.StartsWith("https://",StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            }
            return request.GetResponse() as HttpWebResponse;
        }
        
        private static bool CheckValidationResult(object sender,X509Certificate certificate,X509Chain chain,SslPolicyErrors errors)
        {
            return true; //总是接受  
        }
    }
}
