//
// https://github.com/ServiceStack/ServiceStack.Text
// ServiceStack.Text: .NET C# POCO JSON, JSV and CSV Text Serializers.
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2012 Service Stack LLC. All Rights Reserved.
//
// Licensed under the same terms of ServiceStack.
//

using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using ServiceStack.Text.Common;
using ServiceStack.Text.Json;

namespace ServiceStack.Text
{
	/// <summary>
	/// Creates an instance of a Type from a string value
	/// </summary>
	public static class JsonSerializer
	{
	    public static UTF8Encoding UTF8Encoding = new UTF8Encoding(false); //Don't emit UTF8 BOM by default

		public static T DeserializeFromString<T>(string value)
		{
            if (string.IsNullOrEmpty(value)) return default(T);
            return (T)JsonReader<T>.Parse(value);
        }

		public static T DeserializeFromReader<T>(TextReader reader)
		{
			return DeserializeFromString<T>(reader.ReadToEnd());
		}

		public static object DeserializeFromString(string value, Type type)
		{
            return string.IsNullOrEmpty(value)
                ? null
                : JsonReader.GetParseFn(type)(value);
        }

		public static object DeserializeFromReader(TextReader reader, Type type)
		{
			return DeserializeFromString(reader.ReadToEnd(), type);
		}

        [ThreadStatic] //Reuse the thread static StringBuilder when serializing to strings
        private static StringBuilderWriter LastWriter; 

	    internal class StringBuilderWriter : IDisposable
        {
	        protected StringBuilder sb;
	        protected StringWriter writer;

	        public StringWriter Writer
	        {
	            get { return writer; }
	        }

	        public StringBuilderWriter()
	        {
	            this.sb = new StringBuilder();
                this.writer = new StringWriter(sb, CultureInfo.InvariantCulture);
	        }

	        public static StringBuilderWriter Create()
            {
                var ret = LastWriter;
                if (JsConfig.ReuseStringBuffer && ret != null)
                {
                    LastWriter = null;
                    ret.sb.Clear();
                    return ret;
                }

                return new StringBuilderWriter();
            }

            public override string ToString()
            {
                return sb.ToString();
            }

            public void Dispose()
            {
                if (JsConfig.ReuseStringBuffer)
                {
                    LastWriter = this;
                }
                else
                {
                    Writer.Dispose();
                }
            }
        }

	    public static string SerializeToString<T>(T value)
		{
            if (value == null || value is Delegate) return null;
            if (typeof(T) == typeof(object))
            {
                return SerializeToString(value, value.GetType());
            }
            if (typeof(T).IsAbstract() || typeof(T).IsInterface())
            {
                JsState.IsWritingDynamic = true;
                var result = SerializeToString(value, value.GetType());
                JsState.IsWritingDynamic = false;
                return result;
            }

            using (var sb = StringBuilderWriter.Create())
            {
                if (typeof(T) == typeof(string))
                {
                    JsonUtils.WriteString(sb.Writer, value as string);
                }
                else
                {
                    JsonWriter<T>.WriteRootObject(sb.Writer, value);
                }
                return sb.ToString();
            }
        }

		public static string SerializeToString(object value, Type type)
		{
			if (value == null) return null;

            using (var sb = StringBuilderWriter.Create())
            {
				if (type == typeof(string))
				{
					JsonUtils.WriteString(sb.Writer, value as string);
				}
				else
				{
                    JsonWriter.GetWriteFn(type)(sb.Writer, value);
				}
                return sb.ToString();
            }
		}

		public static void SerializeToWriter<T>(T value, TextWriter writer)
		{
			if (value == null) return;
			if (typeof(T) == typeof(string))
			{
                JsonUtils.WriteString(writer, value as string);
			}
            else if (typeof(T) == typeof(object))
            {
                SerializeToWriter(value, value.GetType(), writer);
            }
		    else if (typeof(T).IsAbstract() || typeof(T).IsInterface())
		    {
		        JsState.IsWritingDynamic = false;
		        SerializeToWriter(value, value.GetType(), writer);
		        JsState.IsWritingDynamic = true;
		    }
		    else
		    {
                JsonWriter<T>.WriteRootObject(writer, value);
            }
		}

		public static void SerializeToWriter(object value, Type type, TextWriter writer)
		{
			if (value == null) return;
			if (type == typeof(string))
			{
                JsonUtils.WriteString(writer, value as string);
				return;
			}

			JsonWriter.GetWriteFn(type)(writer, value);
		}

		public static void SerializeToStream<T>(T value, Stream stream)
		{
			if (value == null) return;
            if (typeof(T) == typeof(object))
            {
                SerializeToStream(value, value.GetType(), stream);
            }
            else if (typeof(T).IsAbstract() || typeof(T).IsInterface())
            {
                JsState.IsWritingDynamic = false;
                SerializeToStream(value, value.GetType(), stream);
                JsState.IsWritingDynamic = true;
            }
            else
            {
                var writer = new StreamWriter(stream, UTF8Encoding);
                JsonWriter<T>.WriteRootObject(writer, value);
                writer.Flush();
            }
		}

		public static void SerializeToStream(object value, Type type, Stream stream)
		{
			var writer = new StreamWriter(stream, UTF8Encoding);
			JsonWriter.GetWriteFn(type)(writer, value);
			writer.Flush();
		}

		public static T DeserializeFromStream<T>(Stream stream)
		{
			using (var reader = new StreamReader(stream, UTF8Encoding))
			{
				return DeserializeFromString<T>(reader.ReadToEnd());
			}
		}

		public static object DeserializeFromStream(Type type, Stream stream)
		{
			using (var reader = new StreamReader(stream, UTF8Encoding))
			{
				return DeserializeFromString(reader.ReadToEnd(), type);
			}
		}

        public static T DeserializeResponse<T>(WebRequest webRequest)
        {
            using (var webRes = PclExport.Instance.GetResponse(webRequest))
            {
                using (var stream = webRes.GetResponseStream())
                {
                    return DeserializeFromStream<T>(stream);
                }
            }
        }

        public static object DeserializeResponse<T>(Type type, WebRequest webRequest)
        {
            using (var webRes = PclExport.Instance.GetResponse(webRequest))
            {
                using (var stream = webRes.GetResponseStream())
                {
                    return DeserializeFromStream(type, stream);
                }
            }
        }

        public static T DeserializeRequest<T>(WebRequest webRequest)
        {
            using (var webRes = PclExport.Instance.GetResponse(webRequest))
            {
                return DeserializeResponse<T>(webRes);
            }
        }

        public static object DeserializeRequest(Type type, WebRequest webRequest)
        {
            using (var webRes = PclExport.Instance.GetResponse(webRequest))
            {
                return DeserializeResponse(type, webRes);
            }
        }
        
        public static T DeserializeResponse<T>(WebResponse webResponse)
		{
			using (var stream = webResponse.GetResponseStream())
			{
				return DeserializeFromStream<T>(stream);
			}
		}

		public static object DeserializeResponse(Type type, WebResponse webResponse)
		{
			using (var stream = webResponse.GetResponseStream())
			{
				return DeserializeFromStream(type, stream);
			}
		}
	}

    public class JsonStringSerializer : IStringSerializer
    {
        public To DeserializeFromString<To>(string serializedText)
        {
            return JsonSerializer.DeserializeFromString<To>(serializedText);
        }

        public object DeserializeFromString(string serializedText, Type type)
        {
            return JsonSerializer.DeserializeFromString(serializedText, type);
        }

        public string SerializeToString<TFrom>(TFrom @from)
        {
            return JsonSerializer.SerializeToString(@from);
        }
    }
}