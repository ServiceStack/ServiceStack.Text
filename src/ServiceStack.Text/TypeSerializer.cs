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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ServiceStack.Text.Common;
using ServiceStack.Text.Jsv;
using ServiceStack.Text.Pools;

namespace ServiceStack.Text
{
    /// <summary>
    /// Creates an instance of a Type from a string value
    /// </summary>
    public static class TypeSerializer
    {
        static TypeSerializer()
        {
            JsConfig.InitStatics();
        }

        public static Encoding UTF8Encoding = PclExport.Instance.GetUTF8Encoding(false);

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

            var writer = StringWriterThreadStatic.Allocate();
            JsvWriter<T>.WriteRootObject(writer, value);
            return StringWriterThreadStatic.ReturnAndFree(writer);
        }

        public static string SerializeToString(object value, Type type)
        {
            if (value == null) return null;
            if (type == typeof(string))
                return value as string;

            var writer = StringWriterThreadStatic.Allocate();
            JsvWriter.GetWriteFn(type)(writer, value);
            return StringWriterThreadStatic.ReturnAndFree(writer);
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

            var dtoStr = !HasCircularReferences(instance) 
                ? SerializeToString(instance)
                : SerializeToString(instance.ToSafePartialObjectDictionary());
            var formatStr = JsvFormatter.Format(dtoStr);
            return formatStr;
        }

        public static string Dump(this Delegate fn)
        {
            var method = fn.GetType().GetMethod("Invoke");
            var sb = StringBuilderThreadStatic.Allocate();
            foreach (var param in method.GetParameters())
            {
                if (sb.Length > 0)
                    sb.Append(", ");

                sb.AppendFormat("{0} {1}", param.ParameterType.Name, param.Name);
            }

            var methodName = fn.Method().Name;
            var info = "{0} {1}({2})".Fmt(method.ReturnType.Name, methodName, 
                StringBuilderThreadStatic.ReturnAndFree(sb));
            return info;
        }

        public static bool HasCircularReferences(object value)
        {
            return HasCircularReferences(value, null);
        }

        private static bool HasCircularReferences(object value, Stack<object> parentValues)
        {
            var type = value != null ? value.GetType() : null;

            if (type == null || !type.IsClass() || value is string)
                return false;

            if (parentValues == null)
            {
                parentValues = new Stack<object>();
                parentValues.Push(value);
            }

            var valueEnumerable = value as IEnumerable;
            if (valueEnumerable != null)
            {
                foreach (var item in valueEnumerable)
                {
                    if (HasCircularReferences(item, parentValues))
                        return true;
                }
            }
            else
            {
                var props = type.GetSerializableProperties();

                foreach (var pi in props)
                {
                    if (pi.GetIndexParameters().Length > 0)
                        continue;

                    var mi = pi.PropertyGetMethod();
                    var pValue = mi != null ? mi.Invoke(value, null) : null;
                    if (pValue == null)
                        continue;

                    if (parentValues.Contains(pValue))
                        return true;

                    parentValues.Push(pValue);

                    if (HasCircularReferences(pValue, parentValues))
                        return true;

                    parentValues.Pop();
                }
            }

            return false;
        }

        private static void times(int count, Action fn)
        {
            for (var i = 0; i < count; i++) fn();
        }

        private const string Indent = "    ";
        public static string IndentJson(this string json)
        {
            var indent = 0;
            var quoted = false;
            var sb = StringBuilderThreadStatic.Allocate();

            for (var i = 0; i < json.Length; i++)
            {
                var ch = json[i];
                switch (ch)
                {
                    case '{':
                    case '[':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            times(++indent, () => sb.Append(Indent));
                        }
                        break;
                    case '}':
                    case ']':
                        if (!quoted)
                        {
                            sb.AppendLine();
                            times(--indent, () => sb.Append(Indent));
                        }
                        sb.Append(ch);
                        break;
                    case '"':
                        sb.Append(ch);
                        var escaped = false;
                        var index = i;
                        while (index > 0 && json[--index] == '\\')
                            escaped = !escaped;
                        if (!escaped)
                            quoted = !quoted;
                        break;
                    case ',':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            times(indent, () => sb.Append(Indent));
                        }
                        break;
                    case ':':
                        sb.Append(ch);
                        if (!quoted)
                            sb.Append(" ");
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }
            return StringBuilderThreadStatic.ReturnAndFree(sb);
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