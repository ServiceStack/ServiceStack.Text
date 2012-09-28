
//
// https://github.com/ServiceStack/ServiceStack.Text
// ServiceStack.Text: .NET C# POCO JSON, JSV and CSV Text Serializers.
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2012 ServiceStack Ltd.
//
// Licensed under the same terms of ServiceStack: new BSD license.
//

using System;
using System.Globalization;
using System.IO;
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
		private static readonly UTF8Encoding UTF8EncodingWithoutBom = new UTF8Encoding(false);

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
			return value == null
					? null
					: JsonReader.GetParseFn(type)(value);
		}

		public static object DeserializeFromReader(TextReader reader, Type type)
		{
			return DeserializeFromString(reader.ReadToEnd(), type);
		}

		public static string SerializeToString<T>(T value)
		{
			if (value == null) return null;
            if (typeof(T) == typeof(object) || typeof(T).IsAbstract || typeof(T).IsInterface)
            {
                if (typeof(T).IsAbstract || typeof(T).IsInterface) JsState.IsWritingDynamic = true;
                var result = SerializeToString(value, value.GetType());
                if (typeof(T).IsAbstract || typeof(T).IsInterface) JsState.IsWritingDynamic = false;
                return result;
            }

			var sb = new StringBuilder();
			using (var writer = new StringWriter(sb, CultureInfo.InvariantCulture))
			{
				if (typeof(T) == typeof(string))
				{
					JsonUtils.WriteString(writer, value as string);
				}
				else
				{
					JsonWriter<T>.WriteObject(writer, value);
				}
			}
			return sb.ToString();
		}

		public static string SerializeToString(object value, Type type)
		{
			if (value == null) return null;

			var sb = new StringBuilder();
			using (var writer = new StringWriter(sb, CultureInfo.InvariantCulture))
			{
				if (type == typeof(string))
				{
					JsonUtils.WriteString(writer, value as string);
				}
				else
				{
					JsonWriter.GetWriteFn(type)(writer, value);
				}
			}
			return sb.ToString();
		}

		public static void SerializeToWriter<T>(T value, TextWriter writer)
		{
			if (value == null) return;
			if (typeof(T) == typeof(string))
			{
				writer.Write(value);
				return;
			}
            if (typeof(T) == typeof(object) || typeof(T).IsAbstract || typeof(T).IsInterface)
			{
                if (typeof(T).IsAbstract || typeof(T).IsInterface) JsState.IsWritingDynamic = true;
                SerializeToWriter(value, value.GetType(), writer);
                if (typeof(T).IsAbstract || typeof(T).IsInterface) JsState.IsWritingDynamic = false;
                return;
			}

			JsonWriter<T>.WriteObject(writer, value);
		}

		public static void SerializeToWriter(object value, Type type, TextWriter writer)
		{
			if (value == null) return;
			if (type == typeof(string))
			{
				writer.Write(value);
				return;
			}

			JsonWriter.GetWriteFn(type)(writer, value);
		}

		public static void SerializeToStream<T>(T value, Stream stream)
		{
			if (value == null) return;
			if (typeof(T) == typeof(object) || typeof(T).IsAbstract || typeof(T).IsInterface)
			{
                if (typeof(T).IsAbstract || typeof(T).IsInterface) JsState.IsWritingDynamic = true;
                SerializeToStream(value, value.GetType(), stream);
                if (typeof(T).IsAbstract || typeof(T).IsInterface) JsState.IsWritingDynamic = false;
				return;
			}

			var writer = new StreamWriter(stream, UTF8EncodingWithoutBom);
			JsonWriter<T>.WriteObject(writer, value);
			writer.Flush();
		}

		public static void SerializeToStream(object value, Type type, Stream stream)
		{
			var writer = new StreamWriter(stream, UTF8EncodingWithoutBom);
			JsonWriter.GetWriteFn(type)(writer, value);
			writer.Flush();
		}

		public static T DeserializeFromStream<T>(Stream stream)
		{
			using (var reader = new StreamReader(stream, UTF8EncodingWithoutBom))
			{
				return DeserializeFromString<T>(reader.ReadToEnd());
			}
		}

		public static object DeserializeFromStream(Type type, Stream stream)
		{
			using (var reader = new StreamReader(stream, UTF8EncodingWithoutBom))
			{
				return DeserializeFromString(reader.ReadToEnd(), type);
			}
		}
	}
}