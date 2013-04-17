using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;

namespace ServiceStack.Text
{
    public static class WebRequestExtensions
    {
        public const string Json = "application/json";
        public const string Xml = "application/xml";
        public const string FormUrlEncoded = "application/x-www-form-urlencoded";
        public const string MultiPartFormData = "multipart/form-data";
        
        public static string GetJsonFromUrl(this string url, 
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return url.GetStringFromUrl(Json, requestFilter, responseFilter);
        }

        public static string GetXmlFromUrl(this string url,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return url.GetStringFromUrl(Xml, requestFilter, responseFilter);
        }

        public static string GetStringFromUrl(this string url, string acceptContentType = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, acceptContentType: acceptContentType, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PostStringToUrl(this string url, string requestBody = null,
            string contentType = null, string acceptContentType = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "POST",
                requestBody: requestBody, contentType: contentType,
                acceptContentType: acceptContentType, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PostToUrl(this string url, string formData = null, string acceptContentType = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "POST",
                contentType: FormUrlEncoded, requestBody: formData,
                acceptContentType: acceptContentType, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PostToUrl(this string url, object formData = null, string acceptContentType = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            string postFormData = formData != null ? QueryStringSerializer.SerializeToString(formData) : null;

            return SendStringToUrl(url, method: "POST",
                contentType: FormUrlEncoded, requestBody: postFormData,
                acceptContentType: acceptContentType, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PostJsonToUrl(this string url, string json,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "POST", requestBody: json, contentType: Json, acceptContentType: Json,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PostJsonToUrl(this string url, object data,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "POST", requestBody: data.ToJson(), contentType: Json, acceptContentType: Json,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PostXmlToUrl(this string url, string xml,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "POST", requestBody: xml, contentType: Xml, acceptContentType: Xml,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }
#if !XBOX && !SILVERLIGHT && !MONOTOUCH
        public static string PostXmlToUrl(this string url, object data,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "POST", requestBody: data.ToXml(), contentType: Xml, acceptContentType: Xml,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }
#endif

        public static string PutStringToUrl(this string url, string requestBody = null,
            string contentType = null, string acceptContentType = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "PUT",
                requestBody: requestBody, contentType: contentType,
                acceptContentType: acceptContentType, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PutToUrl(this string url, string formData = null, string acceptContentType = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "PUT",
                contentType: FormUrlEncoded, requestBody: formData,
                acceptContentType: acceptContentType, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PutToUrl(this string url, object formData = null, string acceptContentType = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            string postFormData = formData != null ? QueryStringSerializer.SerializeToString(formData) : null;

            return SendStringToUrl(url, method: "PUT",
                contentType: FormUrlEncoded, requestBody: postFormData,
                acceptContentType: acceptContentType, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PutJsonToUrl(this string url, string json,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "PUT", requestBody: json, contentType: Json, acceptContentType: Json,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PutJsonToUrl(this string url, object data,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "PUT", requestBody: data.ToJson(), contentType: Json, acceptContentType: Json,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PutXmlToUrl(this string url, string xml,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "PUT", requestBody: xml, contentType: Xml, acceptContentType: Xml,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

#if !XBOX && !SILVERLIGHT && !MONOTOUCH
        public static string PutXmlToUrl(this string url, object data,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "PUT", requestBody: data.ToXml(), contentType: Xml, acceptContentType: Xml,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }
#endif

        public static string DeleteFromUrl(this string url, string acceptContentType = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "DELETE", acceptContentType: acceptContentType, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string OptionsFromUrl(this string url, string acceptContentType = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "OPTIONS", acceptContentType: acceptContentType, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string HeadFromUrl(this string url, string acceptContentType = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "HEAD", acceptContentType: acceptContentType, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string SendStringToUrl(this string url, string method = null,
            string requestBody = null, string contentType = null, string acceptContentType = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            var webReq = (HttpWebRequest)WebRequest.Create(url);
            if (method != null)
                webReq.Method = method;
            if (contentType != null)
                webReq.ContentType = contentType;

            webReq.Accept = acceptContentType;
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

        public static byte[] GetBytesFromUrl(this string url, string acceptContentType = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return url.SendBytesToUrl(acceptContentType:acceptContentType, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static byte[] PostBytesToUrl(this string url, byte[] requestBody = null, string contentType = null, string acceptContentType = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendBytesToUrl(url, method: "POST",
                contentType: contentType, requestBody: requestBody,
                acceptContentType: acceptContentType, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static byte[] PutBytesToUrl(this string url, byte[] requestBody = null, string contentType = null, string acceptContentType = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendBytesToUrl(url, method: "PUT",
                contentType: contentType, requestBody: requestBody,
                acceptContentType: acceptContentType, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static byte[] SendBytesToUrl(this string url, string method = null,
            byte[] requestBody = null, string contentType = null, string acceptContentType = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            var webReq = (HttpWebRequest)WebRequest.Create(url);
            if (method != null)
                webReq.Method = method;

            if (contentType != null)
                webReq.ContentType = contentType;

            webReq.Accept = acceptContentType;
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
    }
}