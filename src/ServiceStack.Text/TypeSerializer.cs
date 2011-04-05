//
// http://code.google.com/p/servicestack/wiki/TypeSerializer
// ServiceStack.Text: .NET C# POCO Type Text Serializer.
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2011 Liquidbit Ltd.
//
// Licensed under the same terms of ServiceStack: new BSD license.
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using ServiceStack.Text.Jsv;

namespace ServiceStack.Text
{
	/// <summary>
	/// Creates an instance of a Type from a string value
	/// </summary>
	public static class TypeSerializer
	{
		private static readonly UTF8Encoding UTF8EncodingWithoutBom = new UTF8Encoding(false);

		public const string DoubleQuoteString = "\"\"";

		/// <summary>
		/// Determines whether the specified type is convertible from string.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>
		/// 	<c>true</c> if the specified type is convertible from string; otherwise, <c>false</c>.
		/// </returns>
		public static bool CanCreateFromString(Type type)
		{
			return JsvReader.GetParseFn(type) != null;
		}

		/// <summary>
		/// Parses the specified value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static T DeserializeFromString<T>(string value)
		{
			if (string.IsNullOrEmpty(value)) return default(T);
			return (T)JsvReader<T>.Parse(value);
		}

		public static T DeserializeFromReader<T>(TextReader reader)
		{
			return DeserializeFromString<T>(reader.ReadToEnd());
		}

		/// <summary>
		/// Parses the specified type.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static object DeserializeFromString(string value, Type type)
		{
			return value == null 
			       	? null 
			       	: JsvReader.GetParseFn(type)(value);
		}

		public static object DeserializeFromReader(TextReader reader, Type type)
		{
			return DeserializeFromString(reader.ReadToEnd(), type);
		}

		public static string SerializeToString<T>(T value)
		{
			if (value == null) return null;
			if (typeof(T) == typeof(string)) return value as string;

			var sb = new StringBuilder(4096);
			using (var writer = new StringWriter(sb, CultureInfo.InvariantCulture))
			{
				JsvWriter<T>.WriteObject(writer, value);
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

			JsvWriter<T>.WriteObject(writer, value);
		}

		public static T Clone<T>(T value)
		{
			var serializedValue = SerializeToString(value);
			var cloneObj = DeserializeFromString<T>(serializedValue);
			return cloneObj;
		}

		public static void SerializeToStream<T>(T value, Stream stream)
		{
			using (var writer = new StreamWriter(stream, UTF8EncodingWithoutBom))
			{
				JsvWriter<T>.WriteObject(writer, value);
			}
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

		/// <summary>
		/// Useful extension method to get the Dictionary[string,string] representation of any POCO type.
		/// </summary>
		/// <returns></returns>
		public static Dictionary<string, string> ToStringDictionary<T>(this T obj)
			where T : class
		{
			var jsv = SerializeToString(obj);
			var map = DeserializeFromString<Dictionary<string, string>>(jsv);
			return map;
		}
		
		/// <summary>
		/// Recursively prints the contents of any POCO object in a human-friendly, readable format
		/// </summary>
		/// <returns></returns>
		public static string Dump<T>(this T instance)
		{
			return SerializeAndFormat(instance);
		}

		public static string SerializeAndFormat<T>(this T instance)
		{
			var dtoStr = SerializeToString(instance);
			var formatStr = JsvFormatter.Format(dtoStr);
			return formatStr;
		}
	}
}