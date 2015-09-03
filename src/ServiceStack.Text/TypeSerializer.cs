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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using ServiceStack.Text.Common;
using ServiceStack.Text.Jsv;

namespace ServiceStack.Text
{
	/// <summary>
	/// Creates an instance of a Type from a string value
	/// </summary>
	public static class TypeSerializer
	{
        public static UTF8Encoding UTF8Encoding = new UTF8Encoding(false); //Don't emit UTF8 BOM by default

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
                JsvWriter<T>.WriteRootObject(sb.Writer, value);

                return sb.ToString();
            }
		}

		public static string SerializeToString(object value, Type type)
		{
			if (value == null) return null;
			if (type == typeof(string)) 
                return value as string;

            using (var sb = StringBuilderWriter.Create())
            {
                JsvWriter.GetWriteFn(type)(sb.Writer, value);

                return sb.ToString();
            }
		}

		public static void SerializeToWriter<T>(T value, TextWriter writer)
		{
			if (value == null) return;
			if (typeof(T) == typeof(string))
			{
				writer.Write(value);
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
                JsvWriter<T>.WriteRootObject(writer, value);
            }
		}

		public static void SerializeToWriter(object value, Type type, TextWriter writer)
		{
			if (value == null) return;
			if (type == typeof(string))
			{
				writer.Write(value);
				return;
			}

			JsvWriter.GetWriteFn(type)(writer, value);
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
                JsvWriter<T>.WriteRootObject(writer, value);
                writer.Flush();
            }
		}

		public static void SerializeToStream(object value, Type type, Stream stream)
		{
			var writer = new StreamWriter(stream, UTF8Encoding);
			JsvWriter.GetWriteFn(type)(writer, value);
			writer.Flush();
		}

		public static T Clone<T>(T value)
		{
			var serializedValue = SerializeToString(value);
			var cloneObj = DeserializeFromString<T>(serializedValue);
			return cloneObj;
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

		/// <summary>
		/// Useful extension method to get the Dictionary[string,string] representation of any POCO type.
		/// </summary>
		/// <returns></returns>
		public static Dictionary<string, string> ToStringDictionary<T>(this T obj)
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

        /// <summary>
        /// Print Dump to Console.WriteLine
        /// </summary>
        public static void PrintDump<T>(this T instance)
        {
            PclExport.Instance.WriteLine(SerializeAndFormat(instance));
        }

        /// <summary>
        /// Print string.Format to Console.WriteLine
        /// </summary>
        public static void Print(this string text, params object[] args)
        {
            if (args.Length > 0)
                PclExport.Instance.WriteLine(text, args);
            else
                PclExport.Instance.WriteLine(text);
        }

        public static void Print(this int intValue)
        {
            PclExport.Instance.WriteLine(intValue.ToString(CultureInfo.InvariantCulture));
        }

        public static void Print(this long longValue)
        {
            PclExport.Instance.WriteLine(longValue.ToString(CultureInfo.InvariantCulture));
        }

        public static string SerializeAndFormat<T>(this T instance)
		{
		    var fn = instance as Delegate;
		    if (fn != null)
                return Dump(fn);

			var dtoStr = SerializeToString(instance);
			var formatStr = JsvFormatter.Format(dtoStr);
			return formatStr;
		}

        public static string Dump(this Delegate fn)
        {
            var method = fn.GetType().GetMethod("Invoke");
            var sb = new StringBuilder();
            foreach (var param in method.GetParameters())
            {
                if (sb.Length > 0)
                    sb.Append(", ");

                sb.AppendFormat("{0} {1}", param.ParameterType.Name, param.Name);
            }

            var methodName = fn.Method().Name;
            var info = "{0} {1}({2})".Fmt(method.ReturnType.Name, methodName, sb);
            return info;
        }
	}

    public class JsvStringSerializer : IStringSerializer
    {
        public To DeserializeFromString<To>(string serializedText)
        {
            return TypeSerializer.DeserializeFromString<To>(serializedText);
        }

        public object DeserializeFromString(string serializedText, Type type)
        {
            return TypeSerializer.DeserializeFromString(serializedText, type);
        }

        public string SerializeToString<TFrom>(TFrom @from)
        {
            return TypeSerializer.SerializeToString(@from);
        }
    }
}