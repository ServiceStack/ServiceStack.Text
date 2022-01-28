//Copyright (c) ServiceStack, Inc. All Rights Reserved.
//License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ServiceStack;

public static partial class HttpUtils
{
    public static string UserAgent = "ServiceStack.Text" + Text.Env.PlatformModifier;

    public static Encoding UseEncoding { get; set; } = PclExport.Instance.GetUTF8Encoding(false);

    public static string AddQueryParam(this string url, string key, object val, bool encode = true)
    {
        return url.AddQueryParam(key, val?.ToString(), encode);
    }

    public static string AddQueryParam(this string url, object key, string val, bool encode = true)
    {
        return AddQueryParam(url, key?.ToString(), val, encode);
    }

    public static string AddQueryParam(this string url, string key, string val, bool encode = true)
    {
        if (url == null)
            url = "";

        if (key == null || val == null)
            return url;
            
        var prefix = string.Empty;
        if (!url.EndsWith("?") && !url.EndsWith("&"))
        {
            prefix = url.IndexOf('?') == -1 ? "?" : "&";
        }
        return url + prefix + key + "=" + (encode ? val.UrlEncode() : val);
    }

    public static string SetQueryParam(this string url, string key, string val)
    {
        if (url == null)
            url = "";
            
        if (key == null || val == null)
            return url;
            
        var qsPos = url.IndexOf('?');
        if (qsPos != -1)
        {
            var existingKeyPos = qsPos + 1 == url.IndexOf(key + "=", qsPos, StringComparison.Ordinal)
                ? qsPos
                : url.IndexOf("&" + key, qsPos, StringComparison.Ordinal);

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
        return url.AddHashParam(key, val?.ToString());
    }

    public static string AddHashParam(this string url, string key, string val)
    {
        if (url == null)
            url = "";
            
        if (key == null || val == null)
            return url;
            
        var prefix = url.IndexOf('#') == -1 ? "#" : "/";
        return url + prefix + key + "=" + val.UrlEncode();
    }

    public static string SetHashParam(this string url, string key, string val)
    {
        if (url == null)
            url = "";
            
        if (key == null || val == null)
            return url;
            
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