#nullable enable

using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceStack;

public class HttpRequestConfig 
{
    public string? Accept { get; set; } 
    public string? UserAgent { get; set; } 
    public string? ContentType { get; set; }
    public HttpHeader? Authorization { get; set; }
    public RangeHeader? Range { get; set; }
    public List<HttpHeader> Headers { get; set; } = new();

    public string AuthBearer
    {
        set => Authorization = new("Bearer", value);
    }
    
    public HttpHeader AuthBasic
    {
        set => Authorization = new("Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes(value.Name + ":" + value.Value)));
    }
}

public record HttpHeader
{
    public HttpHeader(string name, string value)
    {
        this.Name = name;
        this.Value = value;
    }

    public string Name { get; }
    public string Value { get; }

    public void Deconstruct(out string name, out string value)
    {
        name = this.Name;
        value = this.Value;
    }
}

public record RangeHeader
{
    public RangeHeader(long from, long? to = null)
    {
        this.From = from;
        this.To = to;
    }

    public long From { get; }
    public long? To { get; }

    public void Deconstruct(out long from, out long? to)
    {
        from = this.From;
        to = this.To;
    }
}
