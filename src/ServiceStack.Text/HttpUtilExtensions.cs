//Copyright (c) Service Stack LLC. All Rights Reserved.
//License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;

namespace ServiceStack.Text
{
    public static class HttpUtilExtensions
    {
        public static string AddQueryParam(this string url, string key, object val, bool encode = true)
        {
            return url.AddQueryParam(key, val.ToString(), encode);
        }

        public static string AddQueryParam(this string url, object key, string val, bool encode = true)
        {
            return AddQueryParam(url, (key ?? "").ToString(), val, encode);
        }

        public static string AddQueryParam(this string url, string key, string val, bool encode = true)
        {
            if (string.IsNullOrEmpty(url)) return null;
            var prefix = url.IndexOf('?') == -1 ? "?" : "&";
            return url + prefix + key + "=" + (encode ? val.UrlEncode() : val);
        }

        public static string SetQueryParam(this string url, string key, string val)
        {
            if (string.IsNullOrEmpty(url)) return null;
            var qsPos = url.IndexOf('?');
            if (qsPos != -1)
            {
                var existingKeyPos = url.IndexOf(key, qsPos, StringComparison.InvariantCulture);
                if (existingKeyPos != -1)
                {
                    var endPos = url.IndexOf('&', existingKeyPos);
                    if (endPos == -1) endPos = url.Length;

                    var newUrl = url.Substring(0, existingKeyPos + key.Length + 1)
                        + val.UrlEncode()
                        + url.Substring(endPos);
                    return newUrl;
                }
            }
            var prefix = qsPos == -1 ? "?" : "&";
            return url + prefix + key + "=" + val.UrlEncode();
        }

        public static string AddHashParam(this string url, string key, object val)
        {
            return url.AddHashParam(key, val.ToString());
        }

        public static string AddHashParam(this string url, string key, string val)
        {
            if (string.IsNullOrEmpty(url)) return null;
            var prefix = url.IndexOf('#') == -1 ? "#" : "/";
            return url + prefix + key + "=" + val.UrlEncode();
        }

        public static string GetJsonFromUrl(this string url,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return url.GetStringFromUrl(MimeTypes.Json, requestFilter, responseFilter);
        }

        public static string GetXmlFromUrl(this string url,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return url.GetStringFromUrl(MimeTypes.Xml, requestFilter, responseFilter);
        }

        public static string GetStringFromUrl(this string url, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PostStringToUrl(this string url, string requestBody = null,
            string contentType = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "POST",
                requestBody: requestBody, contentType: contentType,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PostToUrl(this string url, string formData = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "POST",
                contentType: MimeTypes.FormUrlEncoded, requestBody: formData,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PostToUrl(this string url, object formData = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            string postFormData = formData != null ? QueryStringSerializer.SerializeToString(formData) : null;

            return SendStringToUrl(url, method: "POST",
                contentType: MimeTypes.FormUrlEncoded, requestBody: postFormData,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PostJsonToUrl(this string url, string json,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "POST", requestBody: json, contentType: MimeTypes.Json, accept: MimeTypes.Json,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PostJsonToUrl(this string url, object data,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "POST", requestBody: data.ToJson(), contentType: MimeTypes.Json, accept: MimeTypes.Json,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PostXmlToUrl(this string url, string xml,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "POST", requestBody: xml, contentType: MimeTypes.Xml, accept: MimeTypes.Xml,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

#if !XBOX && !SILVERLIGHT && !MONOTOUCH
        public static string PostXmlToUrl(this string url, object data,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "POST", requestBody: data.ToXml(), contentType: MimeTypes.Xml, accept: MimeTypes.Xml,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }
#endif

        public static string PutStringToUrl(this string url, string requestBody = null,
            string contentType = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "PUT",
                requestBody: requestBody, contentType: contentType,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PutToUrl(this string url, string formData = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "PUT",
                contentType: MimeTypes.FormUrlEncoded, requestBody: formData,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PutToUrl(this string url, object formData = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            string postFormData = formData != null ? QueryStringSerializer.SerializeToString(formData) : null;

            return SendStringToUrl(url, method: "PUT",
                contentType: MimeTypes.FormUrlEncoded, requestBody: postFormData,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PutJsonToUrl(this string url, string json,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "PUT", requestBody: json, contentType: MimeTypes.Json, accept: MimeTypes.Json,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PutJsonToUrl(this string url, object data,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "PUT", requestBody: data.ToJson(), contentType: MimeTypes.Json, accept: MimeTypes.Json,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PutXmlToUrl(this string url, string xml,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "PUT", requestBody: xml, contentType: MimeTypes.Xml, accept: MimeTypes.Xml,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

#if !XBOX && !SILVERLIGHT && !MONOTOUCH
        public static string PutXmlToUrl(this string url, object data,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "PUT", requestBody: data.ToXml(), contentType: MimeTypes.Xml, accept: MimeTypes.Xml,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }
#endif

        public static string DeleteFromUrl(this string url, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "DELETE", accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string OptionsFromUrl(this string url, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "OPTIONS", accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string HeadFromUrl(this string url, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "HEAD", accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string SendStringToUrl(this string url, string method = null,
            string requestBody = null, string contentType = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            var webReq = (HttpWebRequest)WebRequest.Create(url);
            if (method != null)
                webReq.Method = method;
            if (contentType != null)
                webReq.ContentType = contentType;

            webReq.Accept = accept;
            webReq.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
            webReq.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            if (requestFilter != null)
            {
                requestFilter(webReq);
            }

            if (requestBody != null)
            {
                using (var reqStream = webReq.GetRequestStream())
                using (var writer = new StreamWriter(reqStream))
                {
                    writer.Write(requestBody);
                }
            }

            using (var webRes = webReq.GetResponse())
            using (var stream = webRes.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                if (responseFilter != null)
                {
                    responseFilter((HttpWebResponse)webRes);
                }
                return reader.ReadToEnd();
            }
        }

        public static byte[] GetBytesFromUrl(this string url, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return url.SendBytesToUrl(accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static byte[] PostBytesToUrl(this string url, byte[] requestBody = null, string contentType = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendBytesToUrl(url, method: "POST",
                contentType: contentType, requestBody: requestBody,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static byte[] PutBytesToUrl(this string url, byte[] requestBody = null, string contentType = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendBytesToUrl(url, method: "PUT",
                contentType: contentType, requestBody: requestBody,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static byte[] SendBytesToUrl(this string url, string method = null,
            byte[] requestBody = null, string contentType = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            var webReq = (HttpWebRequest)WebRequest.Create(url);
            if (method != null)
                webReq.Method = method;

            if (contentType != null)
                webReq.ContentType = contentType;

            webReq.Accept = accept;
            webReq.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
            webReq.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            if (requestFilter != null)
            {
                requestFilter(webReq);
            }

            if (requestBody != null)
            {
                using (var req = webReq.GetRequestStream())
                {
                    req.Write(requestBody, 0, requestBody.Length);
                }
            }

            using (var webRes = webReq.GetResponse())
            {
                if (responseFilter != null)
                    responseFilter((HttpWebResponse)webRes);

                using (var stream = webRes.GetResponseStream())
                {
                    return stream.ReadFully();
                }
            }
        }

        public static bool IsAny300(this Exception ex)
        {
            var status = ex.GetStatus();
            return status >= HttpStatusCode.MultipleChoices && status < HttpStatusCode.BadRequest;
        }

        public static bool IsAny400(this Exception ex)
        {
            var status = ex.GetStatus();
            return status >= HttpStatusCode.BadRequest && status < HttpStatusCode.InternalServerError;
        }

        public static bool IsAny500(this Exception ex)
        {
            var status = ex.GetStatus();
            return status >= HttpStatusCode.InternalServerError && (int)status < 600;
        }

        public static bool IsBadRequest(this Exception ex)
        {
            return HasStatus(ex as WebException, HttpStatusCode.BadRequest);
        }

        public static bool IsNotFound(this Exception ex)
        {
            return HasStatus(ex as WebException, HttpStatusCode.NotFound);
        }

        public static bool IsUnauthorized(this Exception ex)
        {
            return HasStatus(ex as WebException, HttpStatusCode.Unauthorized);
        }

        public static bool IsForbidden(this Exception ex)
        {
            return HasStatus(ex as WebException, HttpStatusCode.Forbidden);
        }

        public static bool IsInternalServerError(this Exception ex)
        {
            return HasStatus(ex as WebException, HttpStatusCode.InternalServerError);
        }

        public static HttpStatusCode? GetResponseStatus(this string url)
        {
            try
            {
                var webReq = (HttpWebRequest)WebRequest.Create(url);
                using (var webRes = webReq.GetResponse())
                {
                    var httpRes = webRes as HttpWebResponse;
                    return httpRes != null ? httpRes.StatusCode : (HttpStatusCode?)null;
                }
            }
            catch (Exception ex)
            {
                return ex.GetStatus();
            }
        }

        public static HttpStatusCode? GetStatus(this Exception ex)
        {
            return GetStatus(ex as WebException);
        }

        public static HttpStatusCode? GetStatus(this WebException webEx)
        {
            if (webEx == null) return null;
            var httpRes = webEx.Response as HttpWebResponse;
            return httpRes != null ? httpRes.StatusCode : (HttpStatusCode?)null;
        }

        public static bool HasStatus(this WebException webEx, HttpStatusCode statusCode)
        {
            return GetStatus(webEx) == statusCode;
        }

        public static string GetResponseBody(this Exception ex)
        {
            var webEx = ex as WebException;
            if (webEx == null || webEx.Status != WebExceptionStatus.ProtocolError) return null;

            var errorResponse = ((HttpWebResponse)webEx.Response);
            using (var reader = new StreamReader(errorResponse.GetResponseStream()))
            {
                return reader.ReadToEnd();
            }
        }

        public static string ToFormUrlEncoded(this NameValueCollection queryParams)
        {
            var sb = new StringBuilder();
            foreach (string key in queryParams)
            {
                var values = queryParams.GetValues(key);
                if (values == null) continue;

                foreach (var value in values)
                {
                    if (sb.Length > 0)
                        sb.Append('&');

                    sb.AppendFormat("{0}={1}", key.UrlEncode(), value.UrlEncode());
                }
            }

            return sb.ToString();
        }

        public static string ReadToEnd(this WebResponse webRes)
        {
            using (var stream = webRes.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static IEnumerable<string> ReadLines(this WebResponse webRes)
        {
            using (var stream = webRes.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        public static HttpWebResponse GetErrorResponse(this string url)
        {
            try
            {
                var webReq = WebRequest.Create(url);
                var webRes = webReq.GetResponse();
                var strRes = webRes.ReadToEnd();
                return null;
            }
            catch (WebException webEx)
            {
                return (HttpWebResponse)webEx.Response;
            }
        }

        public static WebResponse PostFileToUrl(this string url,
            FileInfo uploadFileInfo, string uploadFileMimeType,
            string accept = null,
            Action<HttpWebRequest> requestFilter = null)
        {
            var webReq = (HttpWebRequest)WebRequest.Create(url);
            using (var fileStream = uploadFileInfo.OpenRead())
            {
                var fileName = uploadFileInfo.Name;

                webReq.UploadFile(fileStream, fileName, uploadFileMimeType, accept: accept, requestFilter: requestFilter, method: "POST");
            }

            return webReq.GetResponse();
        }

        public static WebResponse PutFileToUrl(this string url,
            FileInfo uploadFileInfo, string uploadFileMimeType,
            string accept = null,
            Action<HttpWebRequest> requestFilter = null)
        {
            var webReq = (HttpWebRequest)WebRequest.Create(url);
            using (var fileStream = uploadFileInfo.OpenRead())
            {
                var fileName = uploadFileInfo.Name;

                webReq.UploadFile(fileStream, fileName, uploadFileMimeType, accept: accept, requestFilter: requestFilter, method: "PUT");
            }

            return webReq.GetResponse();
        }

        public static WebResponse UploadFile(this WebRequest webRequest,
            FileInfo uploadFileInfo, string uploadFileMimeType)
        {
            using (var fileStream = uploadFileInfo.OpenRead())
            {
                var fileName = uploadFileInfo.Name;

                webRequest.UploadFile(fileStream, fileName, uploadFileMimeType);
            }

            return webRequest.GetResponse();
        }

        public static void UploadFile(this WebRequest webRequest, Stream fileStream, string fileName, string mimeType,
            string accept = null, Action<HttpWebRequest> requestFilter = null, string method = "POST")
        {
            var httpReq = (HttpWebRequest)webRequest;
            httpReq.UserAgent = Env.ServerUserAgent;
            httpReq.Method = method;
            httpReq.AllowAutoRedirect = false;
            httpReq.KeepAlive = false;

            if (accept != null)
                httpReq.Accept = accept;

            if (requestFilter != null)
                requestFilter(httpReq);

            var boundary = "----------------------------" + DateTime.UtcNow.Ticks.ToString("x");

            httpReq.ContentType = "multipart/form-data; boundary=" + boundary;

            var boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            var headerTemplate = "\r\n--" + boundary +
                                 "\r\nContent-Disposition: form-data; name=\"file\"; filename=\"{0}\"\r\nContent-Type: {1}\r\n\r\n";

            var header = string.Format(headerTemplate, fileName, mimeType);

            var headerbytes = System.Text.Encoding.ASCII.GetBytes(header);

            httpReq.ContentLength = fileStream.Length + headerbytes.Length + boundarybytes.Length;

            using (Stream outputStream = httpReq.GetRequestStream())
            {
                outputStream.Write(headerbytes, 0, headerbytes.Length);

                byte[] buffer = new byte[4096];
                int byteCount;

                while ((byteCount = fileStream.Read(buffer, 0, 4096)) > 0)
                {
                    outputStream.Write(buffer, 0, byteCount);
                }

                outputStream.Write(boundarybytes, 0, boundarybytes.Length);

                outputStream.Close();
            }
        }


        public static void UploadFile(this WebRequest webRequest, Stream fileStream, string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException("fileName");
            var mimeType = MimeTypes.GetMimeType(fileName);
            if (mimeType == null)
                throw new ArgumentException("Mime-type not found for file: " + fileName);

            UploadFile(webRequest, fileStream, fileName, mimeType);
        }
    }

    public static class MimeTypes
    {
        public static Dictionary<string, string> ExtensionMimeTypes = new Dictionary<string, string>();

        public const string Html = "text/html";
        public const string Xml = "application/xml";
        public const string XmlText = "text/xml";
        public const string Json = "application/json";
        public const string JsonText = "text/json";
        public const string Jsv = "application/jsv";
        public const string JsvText = "text/jsv";
        public const string Csv = "text/csv";
        public const string ProtoBuf = "application/x-protobuf";
        public const string JavaScript = "text/javascript";

        public const string FormUrlEncoded = "application/x-www-form-urlencoded";
        public const string MultiPartFormData = "multipart/form-data";
        public const string JsonReport = "text/jsonreport";
        public const string Soap11 = " text/xml; charset=utf-8";
        public const string Soap12 = " application/soap+xml";
        public const string Yaml = "application/yaml";
        public const string YamlText = "text/yaml";
        public const string PlainText = "text/plain";
        public const string MarkdownText = "text/markdown";
        public const string MsgPack = "application/x-msgpack";
        public const string Bson = "application/bson";
        public const string Binary = "application/octet-stream";

        public static string GetExtension(string mimeType)
        {
            switch (mimeType)
            {
                case ProtoBuf:
                    return ".pbuf";
            }

            var parts = mimeType.Split('/');
            if (parts.Length == 1) return "." + parts[0];
            if (parts.Length == 2) return "." + parts[1];

            throw new NotSupportedException("Unknown mimeType: " + mimeType);
        }

        public static string GetMimeType(string fileNameOrExt)
        {
            if (string.IsNullOrEmpty(fileNameOrExt))
                throw new ArgumentNullException("fileNameOrExt");

            var parts = fileNameOrExt.Split('.');
            var fileExt = parts[parts.Length - 1];

            string mimeType;
            if (ExtensionMimeTypes.TryGetValue(fileExt, out mimeType))
            {
                return mimeType;
            }

            switch (fileExt)
            {
                case "jpeg":
                case "gif":
                case "png":
                case "tiff":
                case "bmp":
                case "webp":
                    return "image/" + fileExt;

                case "jpg":
                    return "image/jpeg";

                case "tif":
                    return "image/tiff";

                case "svg":
                    return "image/svg+xml";

                case "htm":
                case "html":
                case "shtml":
                    return "text/html";

                case "js":
                    return "text/javascript";

                case "csv":
                case "css":
                case "sgml":
                    return "text/" + fileExt;

                case "txt":
                    return "text/plain";

                case "wav":
                    return "audio/wav";

                case "mp3":
                    return "audio/mpeg3";

                case "mid":
                    return "audio/midi";

                case "qt":
                case "mov":
                    return "video/quicktime";

                case "mpg":
                    return "video/mpeg";

                case "avi":
                case "mp4":
                case "ogg":
                case "webm":
                    return "video/" + fileExt;

                case "rtf":
                    return "application/" + fileExt;

                case "xls":
                    return "application/x-excel";

                case "doc":
                    return "application/msword";

                case "ppt":
                    return "application/powerpoint";

                case "gz":
                case "tgz":
                    return "application/x-compressed";

                case "eot":
                    return "application/vnd.ms-fontobject";

                case "ttf":
                    return "application/octet-stream";

                case "woff":
                    return "application/font-woff";

                default:
                    return "application/" + fileExt;
            }
        }
    }
}