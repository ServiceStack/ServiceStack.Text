using System.IO;
using System.Net;

namespace ServiceStack.Text
{
    public static class WebRequestExtensions
    {
        public static string DownloadJsonFromUrl(this string url)
        {
            return url.DownloadUrl("application/json");
        }

        public static string DownloadUrl(this string url, string acceptContentType)
        {
            var webReq = (HttpWebRequest)WebRequest.Create(url);
            webReq.Accept = acceptContentType;
            using (var webRes = webReq.GetResponse())
            using (var stream = webRes.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}