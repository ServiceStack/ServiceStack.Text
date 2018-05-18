﻿//Copyright (c) ServiceStack, Inc. All Rights Reserved.
//License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ServiceStack
{
    public static class HttpUtils
    {
        public static string UserAgent = "ServiceStack.Text";

        public static Encoding UseEncoding { get; set; } = PclExport.Instance.GetUTF8Encoding(false);

        [ThreadStatic]
        public static IHttpResultsFilter ResultsFilter;

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
            var prefix = string.Empty;
            if (!url.EndsWith("?") && !url.EndsWith("&"))
            {
                prefix = url.IndexOf('?') == -1 ? "?" : "&";
            }
            return url + prefix + key + "=" + (encode ? val.UrlEncode() : val);
        }

        public static string SetQueryParam(this string url, string key, string val)
        {
            if (string.IsNullOrEmpty(url)) return null;
            var qsPos = url.IndexOf('?');
            if (qsPos != -1)
            {
                var existingKeyPos = qsPos + 1 == url.IndexOf(key + "=", qsPos, PclExport.Instance.InvariantComparison)
                    ? qsPos
                    : url.IndexOf("&" + key, qsPos, PclExport.Instance.InvariantComparison);

                if (existingKeyPos != -1)
                {
                    var endPos = url.IndexOf('&', existingKeyPos + 1);
                    if (endPos == -1)
                        endPos = url.Length;

                    var newUrl = url.Substring(0, existingKeyPos + key.Length + 1)
                        + "="
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

        public static string SetHashParam(this string url, string key, string val)
        {
            if (string.IsNullOrEmpty(url)) return null;
            var hPos = url.IndexOf('#');
            if (hPos != -1)
            {
                var existingKeyPos = hPos + 1 == url.IndexOf(key + "=", hPos, PclExport.Instance.InvariantComparison)
                    ? hPos
                    : url.IndexOf("/" + key, hPos, PclExport.Instance.InvariantComparison);

                if (existingKeyPos != -1)
                {
                    var endPos = url.IndexOf('/', existingKeyPos + 1);
                    if (endPos == -1)
                        endPos = url.Length;

                    var newUrl = url.Substring(0, existingKeyPos + key.Length + 1)
                        + "="
                        + val.UrlEncode()
                        + url.Substring(endPos);
                    return newUrl;
                }
            }
            var prefix = url.IndexOf('#') == -1 ? "#" : "/";
            return url + prefix + key + "=" + val.UrlEncode();
        }

        public static bool HasRequestBody(string httpMethod)
        {
            switch (httpMethod)
            {
                case HttpMethods.Get:
                case HttpMethods.Delete:
                case HttpMethods.Head:
                case HttpMethods.Options:
                    return false;
            }

            return true;
        }

        public static string GetJsonFromUrl(this string url,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return url.GetStringFromUrl(MimeTypes.Json, requestFilter, responseFilter);
        }

        public static Task<string> GetJsonFromUrlAsync(this string url,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return url.GetStringFromUrlAsync(MimeTypes.Json, requestFilter, responseFilter);
        }

        public static string GetXmlFromUrl(this string url,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return url.GetStringFromUrl(MimeTypes.Xml, requestFilter, responseFilter);
        }

        public static Task<string> GetXmlFromUrlAsync(this string url,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return url.GetStringFromUrlAsync(MimeTypes.Xml, requestFilter, responseFilter);
        }

        public static string GetCsvFromUrl(this string url,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return url.GetStringFromUrl(MimeTypes.Csv, requestFilter, responseFilter);
        }

        public static Task<string> GetCsvFromUrlAsync(this string url,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return url.GetStringFromUrlAsync(MimeTypes.Csv, requestFilter, responseFilter);
        }

        public static string GetStringFromUrl(this string url, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> GetStringFromUrlAsync(this string url, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PostStringToUrl(this string url, string requestBody = null,
            string contentType = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "POST",
                requestBody: requestBody, contentType: contentType,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> PostStringToUrlAsync(this string url, string requestBody = null,
            string contentType = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "POST",
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

        public static Task<string> PostToUrlAsync(this string url, string formData = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "POST",
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

        public static Task<string> PostToUrlAsync(this string url, object formData = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            string postFormData = formData != null ? QueryStringSerializer.SerializeToString(formData) : null;

            return SendStringToUrlAsync(url, method: "POST",
                contentType: MimeTypes.FormUrlEncoded, requestBody: postFormData,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PostJsonToUrl(this string url, string json,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "POST", requestBody: json, contentType: MimeTypes.Json, accept: MimeTypes.Json,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> PostJsonToUrlAsync(this string url, string json,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "POST", requestBody: json, contentType: MimeTypes.Json, accept: MimeTypes.Json,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PostJsonToUrl(this string url, object data,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "POST", requestBody: data.ToJson(), contentType: MimeTypes.Json, accept: MimeTypes.Json,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> PostJsonToUrlAsync(this string url, object data,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "POST", requestBody: data.ToJson(), contentType: MimeTypes.Json, accept: MimeTypes.Json,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PostXmlToUrl(this string url, string xml,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "POST", requestBody: xml, contentType: MimeTypes.Xml, accept: MimeTypes.Xml,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> PostXmlToUrlAsync(this string url, string xml,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "POST", requestBody: xml, contentType: MimeTypes.Xml, accept: MimeTypes.Xml,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PostCsvToUrl(this string url, string csv,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "POST", requestBody: csv, contentType: MimeTypes.Csv, accept: MimeTypes.Csv,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> PostCsvToUrlAsync(this string url, string csv,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "POST", requestBody: csv, contentType: MimeTypes.Csv, accept: MimeTypes.Csv,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PutStringToUrl(this string url, string requestBody = null,
            string contentType = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "PUT",
                requestBody: requestBody, contentType: contentType,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> PutStringToUrlAsync(this string url, string requestBody = null,
            string contentType = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "PUT",
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

        public static Task<string> PutToUrlAsync(this string url, string formData = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "PUT",
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

        public static Task<string> PutToUrlAsync(this string url, object formData = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            string postFormData = formData != null ? QueryStringSerializer.SerializeToString(formData) : null;

            return SendStringToUrlAsync(url, method: "PUT",
                contentType: MimeTypes.FormUrlEncoded, requestBody: postFormData,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PutJsonToUrl(this string url, string json,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "PUT", requestBody: json, contentType: MimeTypes.Json, accept: MimeTypes.Json,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> PutJsonToUrlAsync(this string url, string json,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "PUT", requestBody: json, contentType: MimeTypes.Json, accept: MimeTypes.Json,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PutJsonToUrl(this string url, object data,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "PUT", requestBody: data.ToJson(), contentType: MimeTypes.Json, accept: MimeTypes.Json,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> PutJsonToUrlAsync(this string url, object data,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "PUT", requestBody: data.ToJson(), contentType: MimeTypes.Json, accept: MimeTypes.Json,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PutXmlToUrl(this string url, string xml,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "PUT", requestBody: xml, contentType: MimeTypes.Xml, accept: MimeTypes.Xml,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> PutXmlToUrlAsync(this string url, string xml,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "PUT", requestBody: xml, contentType: MimeTypes.Xml, accept: MimeTypes.Xml,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PutCsvToUrl(this string url, string csv,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "PUT", requestBody: csv, contentType: MimeTypes.Csv, accept: MimeTypes.Csv,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> PutCsvToUrlAsync(this string url, string csv,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "PUT", requestBody: csv, contentType: MimeTypes.Csv, accept: MimeTypes.Csv,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PatchStringToUrl(this string url, string requestBody = null,
            string contentType = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "PATCH",
                requestBody: requestBody, contentType: contentType,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> PatchStringToUrlAsync(this string url, string requestBody = null,
            string contentType = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "PATCH",
                requestBody: requestBody, contentType: contentType,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PatchToUrl(this string url, string formData = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "PATCH",
                contentType: MimeTypes.FormUrlEncoded, requestBody: formData,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> PatchToUrlAsync(this string url, string formData = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "PATCH",
                contentType: MimeTypes.FormUrlEncoded, requestBody: formData,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PatchToUrl(this string url, object formData = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            string postFormData = formData != null ? QueryStringSerializer.SerializeToString(formData) : null;

            return SendStringToUrl(url, method: "PATCH",
                contentType: MimeTypes.FormUrlEncoded, requestBody: postFormData,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> PatchToUrlAsync(this string url, object formData = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            string postFormData = formData != null ? QueryStringSerializer.SerializeToString(formData) : null;

            return SendStringToUrlAsync(url, method: "PATCH",
                contentType: MimeTypes.FormUrlEncoded, requestBody: postFormData,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PatchJsonToUrl(this string url, string json,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "PATCH", requestBody: json, contentType: MimeTypes.Json, accept: MimeTypes.Json,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> PatchJsonToUrlAsync(this string url, string json,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "PATCH", requestBody: json, contentType: MimeTypes.Json, accept: MimeTypes.Json,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PatchJsonToUrl(this string url, object data,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "PATCH", requestBody: data.ToJson(), contentType: MimeTypes.Json, accept: MimeTypes.Json,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> PatchJsonToUrlAsync(this string url, object data,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "PATCH", requestBody: data.ToJson(), contentType: MimeTypes.Json, accept: MimeTypes.Json,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string DeleteFromUrl(this string url, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "DELETE", accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> DeleteFromUrlAsync(this string url, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "DELETE", accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string OptionsFromUrl(this string url, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "OPTIONS", accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> OptionsFromUrlAsync(this string url, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "OPTIONS", accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string HeadFromUrl(this string url, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "HEAD", accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<string> HeadFromUrlAsync(this string url, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrlAsync(url, method: "HEAD", accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
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
            PclExport.Instance.AddCompression(webReq);

            requestFilter?.Invoke(webReq);

            if (ResultsFilter != null)
            {
                return ResultsFilter.GetString(webReq, requestBody);
            }

            if (requestBody != null)
            {
                using (var reqStream = PclExport.Instance.GetRequestStream(webReq))
                using (var writer = new StreamWriter(reqStream, UseEncoding))
                {
                    writer.Write(requestBody);
                }
            }
            else if (method != null && HasRequestBody(method))
            {
                webReq.ContentLength = 0;
            }

            using (var webRes = PclExport.Instance.GetResponse(webReq))
            using (var stream = webRes.GetResponseStream())
            using (var reader = new StreamReader(stream, UseEncoding))
            {
                responseFilter?.Invoke((HttpWebResponse)webRes);

                return reader.ReadToEnd();
            }
        }

        public static Task<string> SendStringToUrlAsync(this string url, string method = null, string requestBody = null,
            string contentType = null, string accept = "*/*", Action<HttpWebRequest> requestFilter = null,
            Action<HttpWebResponse> responseFilter = null)
        {
            var webReq = (HttpWebRequest)WebRequest.Create(url);
            if (method != null)
                webReq.Method = method;
            if (contentType != null)
                webReq.ContentType = contentType;

            webReq.Accept = accept;
            PclExport.Instance.AddCompression(webReq);

            requestFilter?.Invoke(webReq);

            if (ResultsFilter != null)
            {
                var result = ResultsFilter.GetString(webReq, requestBody);
                var tcsResult = new TaskCompletionSource<string>();
                tcsResult.SetResult(result);
                return tcsResult.Task;
            }

            if (requestBody != null)
            {
                using (var reqStream = PclExport.Instance.GetRequestStream(webReq))
                using (var writer = new StreamWriter(reqStream, UseEncoding))
                {
                    writer.Write(requestBody);
                }
            }

            var taskWebRes = webReq.GetResponseAsync();
            var tcs = new TaskCompletionSource<string>();

            taskWebRes.ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    tcs.SetException(task.Exception);
                    return;
                }
                if (task.IsCanceled)
                {
                    tcs.SetCanceled();
                    return;
                }

                var webRes = task.Result;
                responseFilter?.Invoke((HttpWebResponse)webRes);

                using (var stream = webRes.GetResponseStream())
                using (var reader = new StreamReader(stream, UseEncoding))
                {
                    tcs.SetResult(reader.ReadToEnd());
                }
            });

            return tcs.Task;
        }

        public static byte[] GetBytesFromUrl(this string url, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return url.SendBytesToUrl(accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<byte[]> GetBytesFromUrlAsync(this string url, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return url.SendBytesToUrlAsync(accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static byte[] PostBytesToUrl(this string url, byte[] requestBody = null, string contentType = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendBytesToUrl(url, method: "POST",
                contentType: contentType, requestBody: requestBody,
                accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static Task<byte[]> PostBytesToUrlAsync(this string url, byte[] requestBody = null, string contentType = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendBytesToUrlAsync(url, method: "POST",
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

        public static Task<byte[]> PutBytesToUrlAsync(this string url, byte[] requestBody = null, string contentType = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendBytesToUrlAsync(url, method: "PUT",
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
            PclExport.Instance.AddCompression(webReq);

            requestFilter?.Invoke(webReq);

            if (ResultsFilter != null)
            {
                return ResultsFilter.GetBytes(webReq, requestBody);
            }

            if (requestBody != null)
            {
                using (var req = PclExport.Instance.GetRequestStream(webReq))
                {
                    req.Write(requestBody, 0, requestBody.Length);
                }
            }

            using (var webRes = PclExport.Instance.GetResponse(webReq))
            {
                responseFilter?.Invoke((HttpWebResponse)webRes);

                using (var stream = webRes.GetResponseStream())
                {
                    return stream.ReadFully();
                }
            }
        }

        public static Task<byte[]> SendBytesToUrlAsync(this string url, string method = null,
            byte[] requestBody = null, string contentType = null, string accept = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            var webReq = (HttpWebRequest)WebRequest.Create(url);
            if (method != null)
                webReq.Method = method;
            if (contentType != null)
                webReq.ContentType = contentType;

            webReq.Accept = accept;
            PclExport.Instance.AddCompression(webReq);

            requestFilter?.Invoke(webReq);

            if (ResultsFilter != null)
            {
                var result = ResultsFilter.GetBytes(webReq, requestBody);
                var tcsResult = new TaskCompletionSource<byte[]>();
                tcsResult.SetResult(result);
                return tcsResult.Task;
            }

            if (requestBody != null)
            {
                using (var req = PclExport.Instance.GetRequestStream(webReq))
                {
                    req.Write(requestBody, 0, requestBody.Length);
                }
            }

            var taskWebRes = webReq.GetResponseAsync();
            var tcs = new TaskCompletionSource<byte[]>();

            taskWebRes.ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    tcs.SetException(task.Exception);
                    return;
                }
                if (task.IsCanceled)
                {
                    tcs.SetCanceled();
                    return;
                }

                var webRes = task.Result;
                responseFilter?.Invoke((HttpWebResponse)webRes);

                using (var stream = webRes.GetResponseStream())
                {
                    tcs.SetResult(stream.ReadFully());
                }
            });

            return tcs.Task;
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

        public static bool IsNotModified(this Exception ex)
        {
            return GetStatus(ex) == HttpStatusCode.NotModified;
        }

        public static bool IsBadRequest(this Exception ex)
        {
            return GetStatus(ex) == HttpStatusCode.BadRequest;
        }

        public static bool IsNotFound(this Exception ex)
        {
            return GetStatus(ex) == HttpStatusCode.NotFound;
        }

        public static bool IsUnauthorized(this Exception ex)
        {
            return GetStatus(ex) == HttpStatusCode.Unauthorized;
        }

        public static bool IsForbidden(this Exception ex)
        {
            return GetStatus(ex) == HttpStatusCode.Forbidden;
        }

        public static bool IsInternalServerError(this Exception ex)
        {
            return GetStatus(ex) == HttpStatusCode.InternalServerError;
        }

        public static HttpStatusCode? GetResponseStatus(this string url)
        {
            try
            {
                var webReq = (HttpWebRequest)WebRequest.Create(url);
                using (var webRes = PclExport.Instance.GetResponse(webReq))
                {
                    var httpRes = webRes as HttpWebResponse;
                    return httpRes?.StatusCode;
                }
            }
            catch (Exception ex)
            {
                return ex.GetStatus();
            }
        }

        public static HttpStatusCode? GetStatus(this Exception ex)
        {
            if (ex == null)
                return null;

            if (ex is WebException webEx)
                return GetStatus(webEx);

            if (ex is IHasStatusCode hasStatus)
                return (HttpStatusCode)hasStatus.StatusCode;

            return null;
        }

        public static HttpStatusCode? GetStatus(this WebException webEx)
        {
            var httpRes = webEx?.Response as HttpWebResponse;
            return httpRes?.StatusCode;
        }

        public static bool HasStatus(this Exception ex, HttpStatusCode statusCode)
        {
            return GetStatus(ex) == statusCode;
        }

        public static string GetResponseBody(this Exception ex)
        {
            if (!(ex is WebException webEx) || webEx.Response == null || webEx.Status != WebExceptionStatus.ProtocolError) 
                return null;

            var errorResponse = (HttpWebResponse)webEx.Response;
            using (var reader = new StreamReader(errorResponse.GetResponseStream(), UseEncoding))
            {
                return reader.ReadToEnd();
            }
        }

        public static string ReadToEnd(this WebResponse webRes)
        {
            using (var stream = webRes.GetResponseStream())
            using (var reader = new StreamReader(stream, UseEncoding))
            {
                return reader.ReadToEnd();
            }
        }

        public static IEnumerable<string> ReadLines(this WebResponse webRes)
        {
            using (var stream = webRes.GetResponseStream())
            using (var reader = new StreamReader(stream, UseEncoding))
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
                using (var webRes = PclExport.Instance.GetResponse(webReq))
                {
                    webRes.ReadToEnd();
                    return null;
                }
            }
            catch (WebException webEx)
            {
                return (HttpWebResponse)webEx.Response;
            }
        }

        public static Task<Stream> GetRequestStreamAsync(this WebRequest request)
        {
            return GetRequestStreamAsync((HttpWebRequest)request);
        }

        public static Task<Stream> GetRequestStreamAsync(this HttpWebRequest request)
        {
            var tcs = new TaskCompletionSource<Stream>();

            try
            {
                request.BeginGetRequestStream(iar =>
                {
                    try
                    {
                        var response = request.EndGetRequestStream(iar);
                        tcs.SetResult(response);
                    }
                    catch (Exception exc)
                    {
                        tcs.SetException(exc);
                    }
                }, null);
            }
            catch (Exception exc)
            {
                tcs.SetException(exc);
            }

            return tcs.Task;
        }

        public static Task<TBase> ConvertTo<TDerived, TBase>(this Task<TDerived> task) where TDerived : TBase
        {
            var tcs = new TaskCompletionSource<TBase>();
            task.ContinueWith(t => tcs.SetResult(t.Result), TaskContinuationOptions.OnlyOnRanToCompletion);
            task.ContinueWith(t => tcs.SetException(t.Exception.InnerExceptions), TaskContinuationOptions.OnlyOnFaulted);
            task.ContinueWith(t => tcs.SetCanceled(), TaskContinuationOptions.OnlyOnCanceled);
            return tcs.Task;
        }

        public static Task<WebResponse> GetResponseAsync(this WebRequest request)
        {
            return GetResponseAsync((HttpWebRequest)request).ConvertTo<HttpWebResponse, WebResponse>();
        }

        public static Task<HttpWebResponse> GetResponseAsync(this HttpWebRequest request)
        {
            var tcs = new TaskCompletionSource<HttpWebResponse>();

            try
            {
                request.BeginGetResponse(iar =>
                {
                    try
                    {
                        var response = (HttpWebResponse)request.EndGetResponse(iar);
                        tcs.SetResult(response);
                    }
                    catch (Exception exc)
                    {
                        tcs.SetException(exc);
                    }
                }, null);
            }
            catch (Exception exc)
            {
                tcs.SetException(exc);
            }

            return tcs.Task;
        }

        public static void UploadFile(this WebRequest webRequest, Stream fileStream, string fileName, string mimeType,
            string accept = null, Action<HttpWebRequest> requestFilter = null, string method = "POST", string field = "file")
        {
            var httpReq = (HttpWebRequest)webRequest;
            httpReq.Method = method;

            if (accept != null)
                httpReq.Accept = accept;

            requestFilter?.Invoke(httpReq);

            var boundary = Guid.NewGuid().ToString("N");

            httpReq.ContentType = "multipart/form-data; boundary=\"" + boundary + "\"";

            var boundarybytes = ("\r\n--" + boundary + "--\r\n").ToAsciiBytes();

            var header = "\r\n--" + boundary +
                         $"\r\nContent-Disposition: form-data; name=\"{field}\"; filename=\"{fileName}\"\r\nContent-Type: {mimeType}\r\n\r\n";

            var headerbytes = header.ToAsciiBytes();

            var contentLength = fileStream.Length + headerbytes.Length + boundarybytes.Length;
            PclExport.Instance.InitHttpWebRequest(httpReq,
                contentLength: contentLength, allowAutoRedirect: false, keepAlive: false);

            if (ResultsFilter != null)
            {
                ResultsFilter.UploadStream(httpReq, fileStream, fileName);
                return;
            }

            using (var outputStream = PclExport.Instance.GetRequestStream(httpReq))
            {
                outputStream.Write(headerbytes, 0, headerbytes.Length);

                fileStream.CopyTo(outputStream, 4096);

                outputStream.Write(boundarybytes, 0, boundarybytes.Length);

                PclExport.Instance.CloseStream(outputStream);
            }
        }

        public static void UploadFile(this WebRequest webRequest, Stream fileStream, string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));
            var mimeType = MimeTypes.GetMimeType(fileName);
            if (mimeType == null)
                throw new ArgumentException("Mime-type not found for file: " + fileName);

            UploadFile(webRequest, fileStream, fileName, mimeType);
        }

        public static string PostXmlToUrl(this string url, object data,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "POST", requestBody: data.ToXml(), contentType: MimeTypes.Xml, accept: MimeTypes.Xml,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PostCsvToUrl(this string url, object data,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "POST", requestBody: data.ToCsv(), contentType: MimeTypes.Csv, accept: MimeTypes.Csv,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PutXmlToUrl(this string url, object data,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "PUT", requestBody: data.ToXml(), contentType: MimeTypes.Xml, accept: MimeTypes.Xml,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PutCsvToUrl(this string url, object data,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "PUT", requestBody: data.ToCsv(), contentType: MimeTypes.Csv, accept: MimeTypes.Csv,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }
    }

    //Allow Exceptions to Customize HTTP StatusCode and StatusDescription returned
    public interface IHasStatusCode
    {
        int StatusCode { get; }
    }

    public interface IHasStatusDescription
    {
        string StatusDescription { get; }
    }

    public interface IHttpResultsFilter : IDisposable
    {
        string GetString(HttpWebRequest webReq, string reqBody);
        byte[] GetBytes(HttpWebRequest webReq, byte[] reqBody);
        void UploadStream(HttpWebRequest webRequest, Stream fileStream, string fileName);
    }

    public class HttpResultsFilter : IHttpResultsFilter
    {
        private readonly IHttpResultsFilter previousFilter;

        public string StringResult { get; set; }
        public byte[] BytesResult { get; set; }

        public Func<HttpWebRequest, string, string> StringResultFn { get; set; }
        public Func<HttpWebRequest, byte[], byte[]> BytesResultFn { get; set; }
        public Action<HttpWebRequest, Stream, string> UploadFileFn { get; set; }

        public HttpResultsFilter(string stringResult = null, byte[] bytesResult = null)
        {
            StringResult = stringResult;
            BytesResult = bytesResult;

            previousFilter = HttpUtils.ResultsFilter;
            HttpUtils.ResultsFilter = this;
        }

        public void Dispose()
        {
            HttpUtils.ResultsFilter = previousFilter;
        }

        public string GetString(HttpWebRequest webReq, string reqBody)
        {
            return StringResultFn != null
                ? StringResultFn(webReq, reqBody)
                : StringResult;
        }

        public byte[] GetBytes(HttpWebRequest webReq, byte[] reqBody)
        {
            return BytesResultFn != null
                ? BytesResultFn(webReq, reqBody)
                : BytesResult;
        }

        public void UploadStream(HttpWebRequest webRequest, Stream fileStream, string fileName)
        {
            UploadFileFn?.Invoke(webRequest, fileStream, fileName);
        }
    }
}

namespace ServiceStack
{
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
        public const string Soap11 = "text/xml; charset=utf-8";
        public const string Soap12 = "application/soap+xml";
        public const string Yaml = "application/yaml";
        public const string YamlText = "text/yaml";
        public const string PlainText = "text/plain";
        public const string MarkdownText = "text/markdown";
        public const string MsgPack = "application/x-msgpack";
        public const string Wire = "application/x-wire";
        public const string NetSerializer = "application/x-netserializer";

        public const string ImagePng = "image/png";
        public const string ImageGif = "image/gif";
        public const string ImageJpg = "image/jpeg";
        public const string ImageSvg = "image/svg+xml";

        public const string Bson = "application/bson";
        public const string Binary = "application/octet-stream";
        public const string ServerSentEvents = "text/event-stream";

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

                case "ts":
                    return "text/x.typescript";

                case "jsx":
                    return "text/jsx";

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

                case "ogv":
                    return "video/ogg";

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
                case "woff2":
                    return "application/font-woff2";

                default:
                    return "application/" + fileExt;
            }
        }
    }

    public static class HttpHeaders
    {
        public const string XParamOverridePrefix = "X-Param-Override-";

        public const string XHttpMethodOverride = "X-Http-Method-Override";

        public const string XAutoBatchCompleted = "X-AutoBatch-Completed"; // How many requests were completed before first failure

        public const string XTag = "X-Tag";

        public const string XUserAuthId = "X-UAId";

        public const string XTrigger = "X-Trigger"; // Trigger Events on UserAgent

        public const string XForwardedFor = "X-Forwarded-For"; // IP Address

        public const string XForwardedPort = "X-Forwarded-Port";  // 80

        public const string XForwardedProtocol = "X-Forwarded-Proto"; // http or https

        public const string XRealIp = "X-Real-IP";

        public const string XLocation = "X-Location";

        public const string XStatus = "X-Status";

        public const string Referer = "Referer";

        public const string CacheControl = "Cache-Control";

        public const string IfModifiedSince = "If-Modified-Since";

        public const string IfUnmodifiedSince = "If-Unmodified-Since";

        public const string IfNoneMatch = "If-None-Match";

        public const string IfMatch = "If-Match";

        public const string LastModified = "Last-Modified";

        public const string Accept = "Accept";

        public const string AcceptEncoding = "Accept-Encoding";

        public const string ContentType = "Content-Type";

        public const string ContentEncoding = "Content-Encoding";

        public const string ContentLength = "Content-Length";

        public const string ContentDisposition = "Content-Disposition";

        public const string Location = "Location";

        public const string SetCookie = "Set-Cookie";

        public const string ETag = "ETag";

        public const string Age = "Age";

        public const string Expires = "Expires";

        public const string Vary = "Vary";

        public const string Authorization = "Authorization";

        public const string WwwAuthenticate = "WWW-Authenticate";

        public const string AllowOrigin = "Access-Control-Allow-Origin";

        public const string AllowMethods = "Access-Control-Allow-Methods";

        public const string AllowHeaders = "Access-Control-Allow-Headers";

        public const string AllowCredentials = "Access-Control-Allow-Credentials";

        public const string ExposeHeaders = "Access-Control-Expose-Headers";

        public const string AccessControlMaxAge = "Access-Control-Max-Age";

        public const string Origin = "Origin";

        public const string RequestMethod = "Access-Control-Request-Method";

        public const string RequestHeaders = "Access-Control-Request-Headers";

        public const string AcceptRanges = "Accept-Ranges";

        public const string ContentRange = "Content-Range";

        public const string Range = "Range";

        public const string SOAPAction = "SOAPAction";

        public const string Allow = "Allow";

        public const string AcceptCharset = "Accept-Charset";

        public const string AcceptLanguage = "Accept-Language";

        public const string Connection = "Connection";

        public const string Cookie = "Cookie";

        public const string ContentLanguage = "Content-Language";

        public const string Expect = "Expect";

        public const string Pragma = "Pragma";
        
        public const string ProxyAuthenticate = "Proxy-Authenticate";

        public const string ProxyAuthorization = "Proxy-Authorization";

        public const string ProxyConnection = "Proxy-Connection";

        public const string SetCookie2 = "Set-Cookie2";

        public const string TE = "TE";

        public const string Trailer = "Trailer";

        public const string TransferEncoding = "Transfer-Encoding";

        public const string Upgrade = "Upgrade";

        public const string Via = "Via";

        public const string Warning = "Warning";

        public const string Date = "Date";
        public const string Host = "Host";
        public const string UserAgent = "User-Agent";

        public static HashSet<string> RestrictedHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Accept,
            Connection,
            ContentLength,
            ContentType,
            Date,
            Expect,
            Host,
            IfModifiedSince,
            Range,
            Referer,
            TransferEncoding,
            UserAgent,
            ProxyConnection,
        };
    }

    public static class HttpMethods
    {
        static readonly string[] allVerbs = {
            "OPTIONS", "GET", "HEAD", "POST", "PUT", "DELETE", "TRACE", "CONNECT", // RFC 2616
            "PROPFIND", "PROPPATCH", "MKCOL", "COPY", "MOVE", "LOCK", "UNLOCK",    // RFC 2518
            "VERSION-CONTROL", "REPORT", "CHECKOUT", "CHECKIN", "UNCHECKOUT",
            "MKWORKSPACE", "UPDATE", "LABEL", "MERGE", "BASELINE-CONTROL", "MKACTIVITY",  // RFC 3253
            "ORDERPATCH", // RFC 3648
            "ACL",        // RFC 3744
            "PATCH",      // https://datatracker.ietf.org/doc/draft-dusseault-http-patch/
            "SEARCH",     // https://datatracker.ietf.org/doc/draft-reschke-webdav-search/
            "BCOPY", "BDELETE", "BMOVE", "BPROPFIND", "BPROPPATCH", "NOTIFY",
            "POLL",  "SUBSCRIBE", "UNSUBSCRIBE" //MS Exchange WebDav: http://msdn.microsoft.com/en-us/library/aa142917.aspx
        };

        public static HashSet<string> AllVerbs = new HashSet<string>(allVerbs);

        public static bool Exists(string httpMethod) => AllVerbs.Contains(httpMethod.ToUpper());
        public static bool HasVerb(string httpVerb) => Exists(httpVerb);

        public const string Get = "GET";
        public const string Put = "PUT";
        public const string Post = "POST";
        public const string Delete = "DELETE";
        public const string Options = "OPTIONS";
        public const string Head = "HEAD";
        public const string Patch = "PATCH";
    }

    public static class CompressionTypes
    {
        public static readonly string[] AllCompressionTypes = new[] { Deflate, GZip };

        public const string Default = Deflate;
        public const string Deflate = "deflate";
        public const string GZip = "gzip";

        public static bool IsValid(string compressionType)
        {
            return compressionType == Deflate || compressionType == GZip;
        }

        public static void AssertIsValid(string compressionType)
        {
            if (!IsValid(compressionType))
            {
                throw new NotSupportedException(compressionType
                    + " is not a supported compression type. Valid types: gzip, deflate.");
            }
        }

        public static string GetExtension(string compressionType)
        {
            switch (compressionType)
            {
                case Deflate:
                case GZip:
                    return "." + compressionType;
                default:
                    throw new NotSupportedException(
                        "Unknown compressionType: " + compressionType);
            }
        }
    }

    public static class HttpStatus
    {
        public static string GetStatusDescription(int statusCode)
        {
            if (statusCode >= 100 && statusCode < 600)
            {
                int i = statusCode / 100;
                int j = statusCode % 100;

                if (j < Descriptions[i].Length)
                    return Descriptions[i][j];
            }

            return string.Empty;
        }

        private static readonly string[][] Descriptions = new string[][]
        {
            null,
            new[]
            { 
                /* 100 */ "Continue",
                /* 101 */ "Switching Protocols",
                /* 102 */ "Processing"
            },
            new[]
            { 
                /* 200 */ "OK",
                /* 201 */ "Created",
                /* 202 */ "Accepted",
                /* 203 */ "Non-Authoritative Information",
                /* 204 */ "No Content",
                /* 205 */ "Reset Content",
                /* 206 */ "Partial Content",
                /* 207 */ "Multi-Status"
            },
            new[]
            { 
                /* 300 */ "Multiple Choices",
                /* 301 */ "Moved Permanently",
                /* 302 */ "Found",
                /* 303 */ "See Other",
                /* 304 */ "Not Modified",
                /* 305 */ "Use Proxy",
                /* 306 */ string.Empty,
                /* 307 */ "Temporary Redirect"
            },
            new[]
            { 
                /* 400 */ "Bad Request",
                /* 401 */ "Unauthorized",
                /* 402 */ "Payment Required",
                /* 403 */ "Forbidden",
                /* 404 */ "Not Found",
                /* 405 */ "Method Not Allowed",
                /* 406 */ "Not Acceptable",
                /* 407 */ "Proxy Authentication Required",
                /* 408 */ "Request Timeout",
                /* 409 */ "Conflict",
                /* 410 */ "Gone",
                /* 411 */ "Length Required",
                /* 412 */ "Precondition Failed",
                /* 413 */ "Request Entity Too Large",
                /* 414 */ "Request-Uri Too Long",
                /* 415 */ "Unsupported Media Type",
                /* 416 */ "Requested Range Not Satisfiable",
                /* 417 */ "Expectation Failed",
                /* 418 */ string.Empty,
                /* 419 */ string.Empty,
                /* 420 */ string.Empty,
                /* 421 */ string.Empty,
                /* 422 */ "Unprocessable Entity",
                /* 423 */ "Locked",
                /* 424 */ "Failed Dependency"
            },
            new[]
            { 
                /* 500 */ "Internal Server Error",
                /* 501 */ "Not Implemented",
                /* 502 */ "Bad Gateway",
                /* 503 */ "Service Unavailable",
                /* 504 */ "Gateway Timeout",
                /* 505 */ "Http Version Not Supported",
                /* 506 */ string.Empty,
                /* 507 */ "Insufficient Storage"
            }
        };
    }
}
