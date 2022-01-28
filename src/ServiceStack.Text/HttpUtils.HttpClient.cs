#if NET6_0_OR_GREATER
#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.Text;

namespace ServiceStack;

public static partial class HttpUtils
{
    public static Func<HttpMessageHandler> HandlerFactory { get; set; } =
        () => new HttpClientHandler {
            UseDefaultCredentials = true,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
        };

    public static Func<string, HttpClient> ClientFactory { get; set; } =
        url => new HttpClient(HandlerFactory(), disposeHandler:true);

    public static string GetJsonFromUrl(this string url,
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        return url.GetStringFromUrl(MimeTypes.Json, requestFilter, responseFilter);
    }

    public static Task<string> GetJsonFromUrlAsync(this string url,
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null,
        CancellationToken token = default)
    {
        return url.GetStringFromUrlAsync(MimeTypes.Json, requestFilter, responseFilter, token: token);
    }

    public static string GetXmlFromUrl(this string url,
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        return url.GetStringFromUrl(MimeTypes.Xml, requestFilter, responseFilter);
    }

    public static Task<string> GetXmlFromUrlAsync(this string url,
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null,
        CancellationToken token = default)
    {
        return url.GetStringFromUrlAsync(MimeTypes.Xml, requestFilter, responseFilter, token: token);
    }

    public static string GetCsvFromUrl(this string url,
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        return url.GetStringFromUrl(MimeTypes.Csv, requestFilter, responseFilter);
    }

    public static Task<string> GetCsvFromUrlAsync(this string url,
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null,
        CancellationToken token = default)
    {
        return url.GetStringFromUrlAsync(MimeTypes.Csv, requestFilter, responseFilter, token: token);
    }

    public static string GetStringFromUrl(this string url, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        return SendStringToUrl(url, accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
    }

    public static Task<string> GetStringFromUrlAsync(this string url, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null,
        CancellationToken token = default)
    {
        return SendStringToUrlAsync(url, accept: accept, requestFilter: requestFilter, responseFilter: responseFilter,
            token: token);
    }

    public static string PostStringToUrl(this string url, string? requestBody = null,
        string? contentType = null, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        return SendStringToUrl(url, method: "POST",
            requestBody: requestBody, contentType: contentType,
            accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
    }

    public static Task<string> PostStringToUrlAsync(this string url, string? requestBody = null,
        string? contentType = null, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null,
        CancellationToken token = default)
    {
        return SendStringToUrlAsync(url, method: "POST",
            requestBody: requestBody, contentType: contentType,
            accept: accept, requestFilter: requestFilter, responseFilter: responseFilter, token: token);
    }

    public static string PostToUrl(this string url, string? formData = null, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        return SendStringToUrl(url, method: "POST",
            contentType: MimeTypes.FormUrlEncoded, requestBody: formData,
            accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
    }

    public static Task<string> PostToUrlAsync(this string url, string? formData = null, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null,
        CancellationToken token = default)
    {
        return SendStringToUrlAsync(url, method: "POST",
            contentType: MimeTypes.FormUrlEncoded, requestBody: formData,
            accept: accept, requestFilter: requestFilter, responseFilter: responseFilter, token: token);
    }

    public static string PostToUrl(this string url, object? formData = null, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        string? postFormData = formData != null ? QueryStringSerializer.SerializeToString(formData) : null;

        return SendStringToUrl(url, method: "POST",
            contentType: MimeTypes.FormUrlEncoded, requestBody: postFormData,
            accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
    }

    public static Task<string> PostToUrlAsync(this string url, object? formData = null, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null,
        CancellationToken token = default)
    {
        string? postFormData = formData != null ? QueryStringSerializer.SerializeToString(formData) : null;

        return SendStringToUrlAsync(url, method: "POST",
            contentType: MimeTypes.FormUrlEncoded, requestBody: postFormData,
            accept: accept, requestFilter: requestFilter, responseFilter: responseFilter, token: token);
    }

    public static string PostJsonToUrl(this string url, string json,
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        return SendStringToUrl(url, method: "POST", requestBody: json, contentType: MimeTypes.Json,
            accept: MimeTypes.Json,
            requestFilter: requestFilter, responseFilter: responseFilter);
    }

    public static Task<string> PostJsonToUrlAsync(this string url, string json,
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null,
        CancellationToken token = default)
    {
        return SendStringToUrlAsync(url, method: "POST", requestBody: json, contentType: MimeTypes.Json,
            accept: MimeTypes.Json,
            requestFilter: requestFilter, responseFilter: responseFilter, token: token);
    }

    public static string PostJsonToUrl(this string url, object data,
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        return SendStringToUrl(url, method: "POST", requestBody: data.ToJson(), contentType: MimeTypes.Json,
            accept: MimeTypes.Json,
            requestFilter: requestFilter, responseFilter: responseFilter);
    }

    public static Task<string> PostJsonToUrlAsync(this string url, object data,
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null,
        CancellationToken token = default)
    {
        return SendStringToUrlAsync(url, method: "POST", requestBody: data.ToJson(), contentType: MimeTypes.Json,
            accept: MimeTypes.Json,
            requestFilter: requestFilter, responseFilter: responseFilter, token: token);
    }

    public static string PostXmlToUrl(this string url, string xml,
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        return SendStringToUrl(url, method: "POST", requestBody: xml, contentType: MimeTypes.Xml, accept: MimeTypes.Xml,
            requestFilter: requestFilter, responseFilter: responseFilter);
    }

    public static Task<string> PostXmlToUrlAsync(this string url, string xml,
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null,
        CancellationToken token = default)
    {
        return SendStringToUrlAsync(url, method: "POST", requestBody: xml, contentType: MimeTypes.Xml,
            accept: MimeTypes.Xml,
            requestFilter: requestFilter, responseFilter: responseFilter, token: token);
    }

    public static string PostCsvToUrl(this string url, string csv,
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        return SendStringToUrl(url, method: "POST", requestBody: csv, contentType: MimeTypes.Csv, accept: MimeTypes.Csv,
            requestFilter: requestFilter, responseFilter: responseFilter);
    }

    public static Task<string> PostCsvToUrlAsync(this string url, string csv,
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null,
        CancellationToken token = default)
    {
        return SendStringToUrlAsync(url, method: "POST", requestBody: csv, contentType: MimeTypes.Csv,
            accept: MimeTypes.Csv,
            requestFilter: requestFilter, responseFilter: responseFilter, token: token);
    }

    public static string PutStringToUrl(this string url, string? requestBody = null,
        string? contentType = null, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        return SendStringToUrl(url, method: "PUT",
            requestBody: requestBody, contentType: contentType,
            accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
    }

    public static Task<string> PutStringToUrlAsync(this string url, string? requestBody = null,
        string? contentType = null, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null,
        CancellationToken token = default)
    {
        return SendStringToUrlAsync(url, method: "PUT",
            requestBody: requestBody, contentType: contentType,
            accept: accept, requestFilter: requestFilter, responseFilter: responseFilter, token: token);
    }

    public static string PutToUrl(this string url, string? formData = null, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        return SendStringToUrl(url, method: "PUT",
            contentType: MimeTypes.FormUrlEncoded, requestBody: formData,
            accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
    }

    public static Task<string> PutToUrlAsync(this string url, string? formData = null, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null,
        CancellationToken token = default)
    {
        return SendStringToUrlAsync(url, method: "PUT",
            contentType: MimeTypes.FormUrlEncoded, requestBody: formData,
            accept: accept, requestFilter: requestFilter, responseFilter: responseFilter, token: token);
    }

    public static string PutToUrl(this string url, object? formData = null, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        string? postFormData = formData != null ? QueryStringSerializer.SerializeToString(formData) : null;

        return SendStringToUrl(url, method: "PUT",
            contentType: MimeTypes.FormUrlEncoded, requestBody: postFormData,
            accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
    }

    public static Task<string> PutToUrlAsync(this string url, object? formData = null, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null,
        CancellationToken token = default)
    {
        string? postFormData = formData != null ? QueryStringSerializer.SerializeToString(formData) : null;

        return SendStringToUrlAsync(url, method: "PUT",
            contentType: MimeTypes.FormUrlEncoded, requestBody: postFormData,
            accept: accept, requestFilter: requestFilter, responseFilter: responseFilter, token: token);
    }

    public static string PutJsonToUrl(this string url, string json,
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        return SendStringToUrl(url, method: "PUT", requestBody: json, contentType: MimeTypes.Json,
            accept: MimeTypes.Json,
            requestFilter: requestFilter, responseFilter: responseFilter);
    }

    public static Task<string> PutJsonToUrlAsync(this string url, string json,
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null,
        CancellationToken token = default)
    {
        return SendStringToUrlAsync(url, method: "PUT", requestBody: json, contentType: MimeTypes.Json,
            accept: MimeTypes.Json,
            requestFilter: requestFilter, responseFilter: responseFilter, token: token);
    }

    public static string PutJsonToUrl(this string url, object data,
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        return SendStringToUrl(url, method: "PUT", requestBody: data.ToJson(), contentType: MimeTypes.Json,
            accept: MimeTypes.Json,
            requestFilter: requestFilter, responseFilter: responseFilter);
    }

    public static Task<string> PutJsonToUrlAsync(this string url, object data,
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null,
        CancellationToken token = default)
    {
        return SendStringToUrlAsync(url, method: "PUT", requestBody: data.ToJson(), contentType: MimeTypes.Json,
            accept: MimeTypes.Json,
            requestFilter: requestFilter, responseFilter: responseFilter, token: token);
    }

    public static string PutXmlToUrl(this string url, string xml,
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        return SendStringToUrl(url, method: "PUT", requestBody: xml, contentType: MimeTypes.Xml, accept: MimeTypes.Xml,
            requestFilter: requestFilter, responseFilter: responseFilter);
    }

    public static Task<string> PutXmlToUrlAsync(this string url, string xml,
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null,
        CancellationToken token = default)
    {
        return SendStringToUrlAsync(url, method: "PUT", requestBody: xml, contentType: MimeTypes.Xml,
            accept: MimeTypes.Xml,
            requestFilter: requestFilter, responseFilter: responseFilter, token: token);
    }

    public static string PutCsvToUrl(this string url, string csv,
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        return SendStringToUrl(url, method: "PUT", requestBody: csv, contentType: MimeTypes.Csv, accept: MimeTypes.Csv,
            requestFilter: requestFilter, responseFilter: responseFilter);
    }

    public static Task<string> PutCsvToUrlAsync(this string url, string csv,
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null,
        CancellationToken token = default)
    {
        return SendStringToUrlAsync(url, method: "PUT", requestBody: csv, contentType: MimeTypes.Csv,
            accept: MimeTypes.Csv,
            requestFilter: requestFilter, responseFilter: responseFilter, token: token);
    }

    public static string PatchStringToUrl(this string url, string? requestBody = null,
        string? contentType = null, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        return SendStringToUrl(url, method: "PATCH",
            requestBody: requestBody, contentType: contentType,
            accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
    }

    public static Task<string> PatchStringToUrlAsync(this string url, string? requestBody = null,
        string? contentType = null, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null,
        CancellationToken token = default)
    {
        return SendStringToUrlAsync(url, method: "PATCH",
            requestBody: requestBody, contentType: contentType,
            accept: accept, requestFilter: requestFilter, responseFilter: responseFilter, token: token);
    }

    public static string PatchToUrl(this string url, string? formData = null, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        return SendStringToUrl(url, method: "PATCH",
            contentType: MimeTypes.FormUrlEncoded, requestBody: formData,
            accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
    }

    public static Task<string> PatchToUrlAsync(this string url, string? formData = null, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null,
        CancellationToken token = default)
    {
        return SendStringToUrlAsync(url, method: "PATCH",
            contentType: MimeTypes.FormUrlEncoded, requestBody: formData,
            accept: accept, requestFilter: requestFilter, responseFilter: responseFilter, token: token);
    }

    public static string PatchToUrl(this string url, object? formData = null, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        string? postFormData = formData != null ? QueryStringSerializer.SerializeToString(formData) : null;

        return SendStringToUrl(url, method: "PATCH",
            contentType: MimeTypes.FormUrlEncoded, requestBody: postFormData,
            accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
    }

    public static Task<string> PatchToUrlAsync(this string url, object? formData = null, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null,
        CancellationToken token = default)
    {
        string? postFormData = formData != null ? QueryStringSerializer.SerializeToString(formData) : null;

        return SendStringToUrlAsync(url, method: "PATCH",
            contentType: MimeTypes.FormUrlEncoded, requestBody: postFormData,
            accept: accept, requestFilter: requestFilter, responseFilter: responseFilter, token: token);
    }

    public static string PatchJsonToUrl(this string url, string json,
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        return SendStringToUrl(url, method: "PATCH", requestBody: json, contentType: MimeTypes.Json,
            accept: MimeTypes.Json,
            requestFilter: requestFilter, responseFilter: responseFilter);
    }

    public static Task<string> PatchJsonToUrlAsync(this string url, string json,
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null,
        CancellationToken token = default)
    {
        return SendStringToUrlAsync(url, method: "PATCH", requestBody: json, contentType: MimeTypes.Json,
            accept: MimeTypes.Json,
            requestFilter: requestFilter, responseFilter: responseFilter, token: token);
    }

    public static string PatchJsonToUrl(this string url, object data,
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        return SendStringToUrl(url, method: "PATCH", requestBody: data.ToJson(), contentType: MimeTypes.Json,
            accept: MimeTypes.Json,
            requestFilter: requestFilter, responseFilter: responseFilter);
    }

    public static Task<string> PatchJsonToUrlAsync(this string url, object data,
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null,
        CancellationToken token = default)
    {
        return SendStringToUrlAsync(url, method: "PATCH", requestBody: data.ToJson(), contentType: MimeTypes.Json,
            accept: MimeTypes.Json,
            requestFilter: requestFilter, responseFilter: responseFilter, token: token);
    }

    public static string DeleteFromUrl(this string url, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        return SendStringToUrl(url, method: "DELETE", accept: accept, requestFilter: requestFilter,
            responseFilter: responseFilter);
    }

    public static Task<string> DeleteFromUrlAsync(this string url, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null,
        CancellationToken token = default)
    {
        return SendStringToUrlAsync(url, method: "DELETE", accept: accept, requestFilter: requestFilter,
            responseFilter: responseFilter, token: token);
    }

    public static string OptionsFromUrl(this string url, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        return SendStringToUrl(url, method: "OPTIONS", accept: accept, requestFilter: requestFilter,
            responseFilter: responseFilter);
    }

    public static Task<string> OptionsFromUrlAsync(this string url, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null,
        CancellationToken token = default)
    {
        return SendStringToUrlAsync(url, method: "OPTIONS", accept: accept, requestFilter: requestFilter,
            responseFilter: responseFilter, token: token);
    }

    public static string HeadFromUrl(this string url, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        return SendStringToUrl(url, method: "HEAD", accept: accept, requestFilter: requestFilter,
            responseFilter: responseFilter);
    }

    public static Task<string> HeadFromUrlAsync(this string url, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null,
        CancellationToken token = default)
    {
        return SendStringToUrlAsync(url, method: "HEAD", accept: accept, requestFilter: requestFilter,
            responseFilter: responseFilter, token: token);
    }
    
    public static string SendStringToUrl(this string url, string method = HttpMethods.Post,
        string? requestBody = null, string? contentType = null, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        var client = ClientFactory(url);
        var httpReq = new HttpRequestMessage(new HttpMethod(method), url);
        httpReq.Headers.Add(HttpHeaders.Accept, accept);

        if (requestBody != null)
        {
            httpReq.Content = new StringContent(requestBody, UseEncoding);
            if (contentType != null)
                httpReq.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        }
        requestFilter?.Invoke(httpReq);

        var httpRes = client.Send(httpReq);
        responseFilter?.Invoke(httpRes);
        httpRes.EnsureSuccessStatusCode();
        return httpRes.Content.ReadAsStream().ReadToEnd(UseEncoding);
    }

    public static async Task<string> SendStringToUrlAsync(this string url, string method = HttpMethods.Post,
        string? requestBody = null,
        string? contentType = null, string accept = "*/*", Action<HttpRequestMessage>? requestFilter = null,
        Action<HttpResponseMessage>? responseFilter = null, CancellationToken token = default)
    {
        var client = ClientFactory(url);
        var httpReq = new HttpRequestMessage(new HttpMethod(method), url);
        httpReq.Headers.Add(HttpHeaders.Accept, accept);

        if (requestBody != null)
        {
            httpReq.Content = new StringContent(requestBody, UseEncoding);
            if (contentType != null)
                httpReq.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        }
        requestFilter?.Invoke(httpReq);

        var httpRes = await client.SendAsync(httpReq, token).ConfigAwait();
        responseFilter?.Invoke(httpRes);
        httpRes.EnsureSuccessStatusCode();
        return await httpRes.Content.ReadAsStringAsync(token).ConfigAwait();
    }

    public static byte[] GetBytesFromUrl(this string url, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        return url.SendBytesToUrl(accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
    }

    public static Task<byte[]> GetBytesFromUrlAsync(this string url, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null,
        CancellationToken token = default)
    {
        return url.SendBytesToUrlAsync(accept: accept, requestFilter: requestFilter, responseFilter: responseFilter,
            token: token);
    }

    public static byte[] PostBytesToUrl(this string url, byte[]? requestBody = null, string? contentType = null,
        string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        return SendBytesToUrl(url, method: "POST",
            contentType: contentType, requestBody: requestBody,
            accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
    }

    public static Task<byte[]> PostBytesToUrlAsync(this string url, byte[]? requestBody = null,
        string? contentType = null, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null,
        CancellationToken token = default)
    {
        return SendBytesToUrlAsync(url, method: "POST",
            contentType: contentType, requestBody: requestBody,
            accept: accept, requestFilter: requestFilter, responseFilter: responseFilter, token: token);
    }

    public static byte[] PutBytesToUrl(this string url, byte[]? requestBody = null, string? contentType = null,
        string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        return SendBytesToUrl(url, method: "PUT",
            contentType: contentType, requestBody: requestBody,
            accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
    }

    public static Task<byte[]> PutBytesToUrlAsync(this string url, byte[]? requestBody = null, string? contentType = null,
        string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null,
        CancellationToken token = default)
    {
        return SendBytesToUrlAsync(url, method: "PUT",
            contentType: contentType, requestBody: requestBody,
            accept: accept, requestFilter: requestFilter, responseFilter: responseFilter, token: token);
    }

    public static byte[] SendBytesToUrl(this string url, string method = HttpMethods.Post,
        byte[]? requestBody = null, string? contentType = null, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        var client = ClientFactory(url);
        var httpReq = new HttpRequestMessage(new HttpMethod(method), url);
        httpReq.Headers.Add(HttpHeaders.Accept, accept);

        if (requestBody != null)
        {
            httpReq.Content = new ReadOnlyMemoryContent(requestBody);
            if (contentType != null)
                httpReq.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        }
        requestFilter?.Invoke(httpReq);

        var httpRes = client.Send(httpReq);
        responseFilter?.Invoke(httpRes);
        httpRes.EnsureSuccessStatusCode();
        return httpRes.Content.ReadAsStream().ReadFully();
    }

    public static async Task<byte[]> SendBytesToUrlAsync(this string url, string method = HttpMethods.Post,
        byte[]? requestBody = null, string? contentType = null, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null,
        CancellationToken token = default)
    {
        var client = ClientFactory(url);
        var httpReq = new HttpRequestMessage(new HttpMethod(method), url);
        httpReq.Headers.Add(HttpHeaders.Accept, accept);

        if (requestBody != null)
        {
            httpReq.Content = new ReadOnlyMemoryContent(requestBody);
            if (contentType != null)
                httpReq.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        }
        requestFilter?.Invoke(httpReq);

        var httpRes = await client.SendAsync(httpReq, token).ConfigAwait();
        responseFilter?.Invoke(httpRes);
        httpRes.EnsureSuccessStatusCode();
        return await (await httpRes.Content.ReadAsStreamAsync(token).ConfigAwait()).ReadFullyAsync(token).ConfigAwait();
    }

    public static Stream GetStreamFromUrl(this string url, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        return url.SendStreamToUrl(accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
    }

    public static Task<Stream> GetStreamFromUrlAsync(this string url, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null,
        CancellationToken token = default)
    {
        return url.SendStreamToUrlAsync(accept: accept, requestFilter: requestFilter, responseFilter: responseFilter,
            token: token);
    }

    public static Stream PostStreamToUrl(this string url, Stream? requestBody = null, string? contentType = null,
        string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        return SendStreamToUrl(url, method: "POST",
            contentType: contentType, requestBody: requestBody,
            accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
    }

    public static Task<Stream> PostStreamToUrlAsync(this string url, Stream? requestBody = null,
        string? contentType = null, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null,
        CancellationToken token = default)
    {
        return SendStreamToUrlAsync(url, method: "POST",
            contentType: contentType, requestBody: requestBody,
            accept: accept, requestFilter: requestFilter, responseFilter: responseFilter, token: token);
    }

    public static Stream PutStreamToUrl(this string url, Stream? requestBody = null, string? contentType = null,
        string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        return SendStreamToUrl(url, method: "PUT",
            contentType: contentType, requestBody: requestBody,
            accept: accept, requestFilter: requestFilter, responseFilter: responseFilter);
    }

    public static Task<Stream> PutStreamToUrlAsync(this string url, Stream? requestBody = null,
        string? contentType = null, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null,
        CancellationToken token = default)
    {
        return SendStreamToUrlAsync(url, method: "PUT",
            contentType: contentType, requestBody: requestBody,
            accept: accept, requestFilter: requestFilter, responseFilter: responseFilter, token: token);
    }

    /// <summary>
    /// Returns HttpWebResponse Stream which must be disposed
    /// </summary>
    public static Stream SendStreamToUrl(this string url, string method = HttpMethods.Post,
        Stream? requestBody = null, string? contentType = null, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        var client = ClientFactory(url);
        var httpReq = new HttpRequestMessage(new HttpMethod(method), url);
        httpReq.Headers.Add(HttpHeaders.Accept, accept);

        if (requestBody != null)
        {
            httpReq.Content = new StreamContent(requestBody);
            if (contentType != null)
                httpReq.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        }
        requestFilter?.Invoke(httpReq);

        var httpRes = client.Send(httpReq);
        responseFilter?.Invoke(httpRes);
        httpRes.EnsureSuccessStatusCode();
        return httpRes.Content.ReadAsStream();
    }

    /// <summary>
    /// Returns HttpWebResponse Stream which must be disposed
    /// </summary>
    public static async Task<Stream> SendStreamToUrlAsync(this string url, string method = HttpMethods.Post,
        Stream? requestBody = null, string? contentType = null, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null,
        CancellationToken token = default)
    {
        var client = ClientFactory(url);
        var httpReq = new HttpRequestMessage(new HttpMethod(method), url);
        httpReq.Headers.Add(HttpHeaders.Accept, accept);

        if (requestBody != null)
        {
            httpReq.Content = new StreamContent(requestBody);
            if (contentType != null)
                httpReq.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        }
        requestFilter?.Invoke(httpReq);

        var httpRes = await client.SendAsync(httpReq, token).ConfigAwait();
        responseFilter?.Invoke(httpRes);
        httpRes.EnsureSuccessStatusCode();
        return await httpRes.Content.ReadAsStreamAsync(token).ConfigAwait();
    }

    public static HttpStatusCode? GetResponseStatus(this string url)
    {
        try
        {
            var client = ClientFactory(url);
            var httpReq = new HttpRequestMessage(new HttpMethod(HttpMethods.Get), url);
            httpReq.Headers.Add(HttpHeaders.Accept, "*/*");
            var httpRes = client.Send(httpReq);
            return httpRes.StatusCode;
        }
        catch (Exception ex)
        {
            return ex.GetStatus();
        }
    }

    public static HttpResponseMessage? GetErrorResponse(this string url)
    {
        var client = ClientFactory(url);
        var httpReq = new HttpRequestMessage(new HttpMethod(HttpMethods.Get), url);
        httpReq.Headers.Add(HttpHeaders.Accept, "*/*");
        var httpRes = client.Send(httpReq);
        return httpRes.IsSuccessStatusCode 
            ? null
            : httpRes;
    }

    public static async Task<HttpResponseMessage?> GetErrorResponseAsync(this string url, CancellationToken token=default)
    {
        var client = ClientFactory(url);
        var httpReq = new HttpRequestMessage(new HttpMethod(HttpMethods.Get), url);
        httpReq.Headers.Add(HttpHeaders.Accept, "*/*");
        var httpRes = await client.SendAsync(httpReq, token).ConfigAwait();
        return httpRes.IsSuccessStatusCode 
            ? null
            : httpRes;
    }

    public static string ReadToEnd(this HttpResponseMessage webRes)
    {
        using var stream = webRes.Content.ReadAsStream();
        return stream.ReadToEnd(UseEncoding);
    }

    public static Task<string> ReadToEndAsync(this HttpResponseMessage webRes)
    {
        using var stream = webRes.Content.ReadAsStream();
        return stream.ReadToEndAsync(UseEncoding);
    }

    public static IEnumerable<string> ReadLines(this HttpResponseMessage webRes)
    {
        using var stream = webRes.Content.ReadAsStream();
        using var reader = new StreamReader(stream, UseEncoding, true, 1024, leaveOpen: true);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            yield return line;
        }
    }

    public static HttpResponseMessage UploadFile(this HttpRequestMessage httpReq, Stream fileStream, string fileName, 
        string? mimeType = null, string accept = "*/*", string method = HttpMethods.Post, string field = "file",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        if (httpReq.RequestUri == null)
            throw new ArgumentException(nameof(httpReq.RequestUri));
        
        httpReq.Method = new HttpMethod(method);
        httpReq.Headers.Add(HttpHeaders.Accept, accept);
        requestFilter?.Invoke(httpReq);

        using var content = new MultipartFormDataContent();
        var fileBytes = fileStream.ReadFully();
        using var fileContent = new ByteArrayContent(fileBytes, 0, fileBytes.Length);
        fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = "file",
            FileName = fileName
        };
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(mimeType ?? MimeTypes.GetMimeType(fileName));
        content.Add(fileContent, "file", fileName);

        var client = ClientFactory(httpReq.RequestUri!.ToString());
        var httpRes = client.Send(httpReq);
        responseFilter?.Invoke(httpRes);
        httpRes.EnsureSuccessStatusCode();
        return httpRes;
    }

    public static async Task<HttpResponseMessage> UploadFileAsync(this HttpRequestMessage httpReq, Stream fileStream, string fileName,
        string? mimeType = null, string accept = "*/*", string method = HttpMethods.Post, string field = "file",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null, 
        CancellationToken token = default)
    {
        if (httpReq.RequestUri == null)
            throw new ArgumentException(nameof(httpReq.RequestUri));
        
        httpReq.Method = new HttpMethod(method);
        httpReq.Headers.Add(HttpHeaders.Accept, accept);
        requestFilter?.Invoke(httpReq);

        using var content = new MultipartFormDataContent();
        var fileBytes = await fileStream.ReadFullyAsync(token).ConfigAwait();
        using var fileContent = new ByteArrayContent(fileBytes, 0, fileBytes.Length);
        fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = "file",
            FileName = fileName
        };
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(mimeType ?? MimeTypes.GetMimeType(fileName));
        content.Add(fileContent, "file", fileName);

        var client = ClientFactory(httpReq.RequestUri!.ToString());
        var httpRes = await client.SendAsync(httpReq, token).ConfigAwait();
        responseFilter?.Invoke(httpRes);
        httpRes.EnsureSuccessStatusCode();
        return httpRes;
    }

    public static void UploadFile(this HttpRequestMessage httpReq, Stream fileStream, string fileName)
    {
        if (fileName == null)
            throw new ArgumentNullException(nameof(fileName));
        var mimeType = MimeTypes.GetMimeType(fileName);
        if (mimeType == null)
            throw new ArgumentException("Mime-type not found for file: " + fileName);

        UploadFile(httpReq, fileStream, fileName, mimeType);
    }

    public static async Task UploadFileAsync(this HttpRequestMessage webRequest, Stream fileStream, string fileName,
        CancellationToken token = default)
    {
        if (fileName == null)
            throw new ArgumentNullException(nameof(fileName));
        var mimeType = MimeTypes.GetMimeType(fileName);
        if (mimeType == null)
            throw new ArgumentException("Mime-type not found for file: " + fileName);

        await UploadFileAsync(webRequest, fileStream, fileName, mimeType, token: token).ConfigAwait();
    }

    public static string PostXmlToUrl(this string url, object data,
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        return SendStringToUrl(url, method: "POST", requestBody: data.ToXml(), contentType: MimeTypes.Xml,
            accept: MimeTypes.Xml,
            requestFilter: requestFilter, responseFilter: responseFilter);
    }

    public static string PostCsvToUrl(this string url, object data,
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        return SendStringToUrl(url, method: "POST", requestBody: data.ToCsv(), contentType: MimeTypes.Csv,
            accept: MimeTypes.Csv,
            requestFilter: requestFilter, responseFilter: responseFilter);
    }

    public static string PutXmlToUrl(this string url, object data,
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        return SendStringToUrl(url, method: "PUT", requestBody: data.ToXml(), contentType: MimeTypes.Xml,
            accept: MimeTypes.Xml,
            requestFilter: requestFilter, responseFilter: responseFilter);
    }

    public static string PutCsvToUrl(this string url, object data,
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        return SendStringToUrl(url, method: "PUT", requestBody: data.ToCsv(), contentType: MimeTypes.Csv,
            accept: MimeTypes.Csv,
            requestFilter: requestFilter, responseFilter: responseFilter);
    }

    public static HttpResponseMessage PostFileToUrl(this string url,
        FileInfo uploadFileInfo, string uploadFileMimeType, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        var webReq = new HttpRequestMessage(HttpMethod.Post, url);
        using var fileStream = uploadFileInfo.OpenRead();
        var fileName = uploadFileInfo.Name;

        return webReq.UploadFile(fileStream, fileName, uploadFileMimeType, accept: accept, 
            method: HttpMethods.Post, requestFilter: requestFilter, responseFilter: responseFilter);
    }

    public static async Task<HttpResponseMessage> PostFileToUrlAsync(this string url,
        FileInfo uploadFileInfo, string uploadFileMimeType, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null, 
        CancellationToken token = default)
    {
        var webReq = new HttpRequestMessage(HttpMethod.Post, url);
        await using var fileStream = uploadFileInfo.OpenRead();
        var fileName = uploadFileInfo.Name;

        return await webReq.UploadFileAsync(fileStream, fileName, uploadFileMimeType, accept: accept, 
            method: HttpMethods.Post, requestFilter: requestFilter, responseFilter: responseFilter, token: token).ConfigAwait();
    }

    public static HttpResponseMessage PutFileToUrl(this string url,
        FileInfo uploadFileInfo, string uploadFileMimeType, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null)
    {
        var webReq = new HttpRequestMessage(HttpMethod.Put, url);
        using var fileStream = uploadFileInfo.OpenRead();
        var fileName = uploadFileInfo.Name;

        return webReq.UploadFile(fileStream, fileName, uploadFileMimeType, accept: accept, 
            method: HttpMethods.Post, requestFilter: requestFilter, responseFilter: responseFilter);
    }

    public static async Task<HttpResponseMessage> PutFileToUrlAsync(this string url,
        FileInfo uploadFileInfo, string uploadFileMimeType, string accept = "*/*",
        Action<HttpRequestMessage>? requestFilter = null, Action<HttpResponseMessage>? responseFilter = null, 
        CancellationToken token = default)
    {
        var webReq = new HttpRequestMessage(HttpMethod.Put, url);
        await using var fileStream = uploadFileInfo.OpenRead();
        var fileName = uploadFileInfo.Name;

        return await webReq.UploadFileAsync(fileStream, fileName, uploadFileMimeType, accept: accept, 
            method: HttpMethods.Post, requestFilter: requestFilter, responseFilter: responseFilter, token: token).ConfigAwait();
    }

    public static HttpRequestMessage With(this HttpRequestMessage httpReq,
        string? accept = null,
        string? userAgent = null,
        Dictionary<string,string>? headers = null)
    {
        if (accept != null)
            httpReq.Headers.Add(HttpHeaders.Accept, accept);
            
        if (userAgent != null)
            httpReq.Headers.UserAgent.Add(new ProductInfoHeaderValue(userAgent));

        if (headers != null)
        {
            foreach (var entry in headers)
            {
                httpReq.Headers.Add(entry.Key, entry.Value);
            }
        }
        
        return httpReq;
    }
    
}

#endif
