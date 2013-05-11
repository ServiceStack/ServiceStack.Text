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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using ServiceStack.Text.Common;
using ServiceStack.Text.Support;
#if NETFX_CORE
using System.Threading.Tasks;
#endif

#if WINDOWS_PHONE
using System.IO.IsolatedStorage;
#if  !WP8
using ServiceStack.Text.WP;
#endif
#endif

namespace ServiceStack.Text
{
    public static class StringExtensions
    {
        public static T To<T>(this string value)
        {
            return TypeSerializer.DeserializeFromString<T>(value);
        }

        public static T To<T>(this string value, T defaultValue)
        {
            return String.IsNullOrEmpty(value) ? defaultValue : TypeSerializer.DeserializeFromString<T>(value);
        }

        public static T ToOrDefaultValue<T>(this string value)
        {
            return String.IsNullOrEmpty(value) ? default(T) : TypeSerializer.DeserializeFromString<T>(value);
        }

        public static object To(this string value, Type type)
        {
            return TypeSerializer.DeserializeFromString(value, type);
        }


        /// <summary>
        /// Converts from base: 0 - 62
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <returns></returns>
        public static string BaseConvert(this string source, int from, int to)
        {
            const string chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var result = "";
            var length = source.Length;
            var number = new int[length];

            for (var i = 0; i < length; i++)
            {
                number[i] = chars.IndexOf(source[i]);
            }

            int newlen;

            do
            {
                var divide = 0;
                newlen = 0;

                for (var i = 0; i < length; i++)
                {
                    divide = divide * @from + number[i];

                    if (divide >= to)
                    {
                        number[newlen++] = divide / to;
                        divide = divide % to;
                    }
                    else if (newlen > 0)
                    {
                        number[newlen++] = 0;
                    }
                }

                length = newlen;
                result = chars[divide] + result;
            }
            while (newlen != 0);

            return result;
        }

        public static string EncodeXml(this string value)
        {
            return value.Replace("<", "&lt;").Replace(">", "&gt;").Replace("&", "&amp;");
        }

        public static string EncodeJson(this string value)
        {
            return String.Concat
            ("\"",
                value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "\\n"),
                "\""
            );
        }

        public static string EncodeJsv(this string value)
        {
            if (JsState.QueryStringMode)
            {
                return UrlEncode(value);
            }
            return String.IsNullOrEmpty(value) || !JsWriter.HasAnyEscapeChars(value)
                ? value
                : String.Concat
                    (
                        JsWriter.QuoteString,
                        value.Replace(JsWriter.QuoteString, TypeSerializer.DoubleQuoteString),
                        JsWriter.QuoteString
                    );
        }

        public static string DecodeJsv(this string value)
        {
            const int startingQuotePos = 1;
            const int endingQuotePos = 2;
            return String.IsNullOrEmpty(value) || value[0] != JsWriter.QuoteChar
                    ? value
                    : value.Substring(startingQuotePos, value.Length - endingQuotePos)
                        .Replace(TypeSerializer.DoubleQuoteString, JsWriter.QuoteString);
        }

        public static string UrlEncode(this string text)
        {
            if (String.IsNullOrEmpty(text)) return text;

            var sb = new StringBuilder();

            foreach (var charCode in Encoding.UTF8.GetBytes(text))
            {

                if (
                    charCode >= 65 && charCode <= 90        // A-Z
                    || charCode >= 97 && charCode <= 122    // a-z
                    || charCode >= 48 && charCode <= 57     // 0-9
                    || charCode >= 44 && charCode <= 46     // ,-.
                    )
                {
                    sb.Append((char)charCode);
                }
                else
                {
                    sb.Append('%' + charCode.ToString("x2"));
                }
            }

            return sb.ToString();
        }

        public static string UrlDecode(this string text)
        {
            if (String.IsNullOrEmpty(text)) return null;

            var bytes = new List<byte>();

            var textLength = text.Length;
            for (var i = 0; i < textLength; i++)
            {
                var c = text[i];
                if (c == '+')
                {
                    bytes.Add(32);
                }
                else if (c == '%')
                {
                    var hexNo = Convert.ToByte(text.Substring(i + 1, 2), 16);
                    bytes.Add(hexNo);
                    i += 2;
                }
                else
                {
                    bytes.Add((byte)c);
                }
            }
#if SILVERLIGHT
            byte[] byteArray = bytes.ToArray();
            return Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
#else
            return Encoding.UTF8.GetString(bytes.ToArray());
#endif
        }

#if !XBOX
        public static string HexEscape(this string text, params char[] anyCharOf)
        {
            if (String.IsNullOrEmpty(text)) return text;
            if (anyCharOf == null || anyCharOf.Length == 0) return text;

            var encodeCharMap = new HashSet<char>(anyCharOf);

            var sb = new StringBuilder();
            var textLength = text.Length;
            for (var i = 0; i < textLength; i++)
            {
                var c = text[i];
                if (encodeCharMap.Contains(c))
                {
                    sb.Append('%' + ((int)c).ToString("x"));
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
#endif
        public static string HexUnescape(this string text, params char[] anyCharOf)
        {
            if (String.IsNullOrEmpty(text)) return null;
            if (anyCharOf == null || anyCharOf.Length == 0) return text;

            var sb = new StringBuilder();

            var textLength = text.Length;
            for (var i = 0; i < textLength; i++)
            {
                var c = text.Substring(i, 1);
                if (c == "%")
                {
                    var hexNo = Convert.ToInt32(text.Substring(i + 1, 2), 16);
                    sb.Append((char)hexNo);
                    i += 2;
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        public static string UrlFormat(this string url, params string[] urlComponents)
        {
            var encodedUrlComponents = new string[urlComponents.Length];
            for (var i = 0; i < urlComponents.Length; i++)
            {
                var x = urlComponents[i];
                encodedUrlComponents[i] = x.UrlEncode();
            }

            return String.Format(url, encodedUrlComponents);
        }

        public static string ToRot13(this string value)
        {
            var array = value.ToCharArray();
            for (var i = 0; i < array.Length; i++)
            {
                var number = (int)array[i];

                if (number >= 'a' && number <= 'z')
                    number += (number > 'm') ? -13 : 13;

                else if (number >= 'A' && number <= 'Z')
                    number += (number > 'M') ? -13 : 13;

                array[i] = (char)number;
            }
            return new string(array);
        }

        public static string WithTrailingSlash(this string path)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            if (path[path.Length - 1] != '/')
            {
                return path + "/";
            }
            return path;
        }

        public static string AppendPath(this string uri, params string[] uriComponents)
        {
            return AppendUrlPaths(uri, uriComponents);
        }

        public static string AppendUrlPaths(this string uri, params string[] uriComponents)
        {
            var sb = new StringBuilder(uri.WithTrailingSlash());
            var i = 0;
            foreach (var uriComponent in uriComponents)
            {
                if (i++ > 0) sb.Append('/');
                sb.Append(uriComponent.UrlEncode());
            }
            return sb.ToString();
        }
#if !SILVERLIGHT
        public static string FromAsciiBytes(this byte[] bytes)
        {
            return bytes == null ? null
                : Encoding.ASCII.GetString(bytes, 0, bytes.Length);
        }

        public static byte[] ToAsciiBytes(this string value)
        {
            return Encoding.ASCII.GetBytes(value);
        }
#endif
        public static string FromUtf8Bytes(this byte[] bytes)
        {
            return bytes == null ? null
                : Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        public static byte[] ToUtf8Bytes(this string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }

        public static byte[] ToUtf8Bytes(this int intVal)
        {
            return FastToUtf8Bytes(intVal.ToString());
        }

        public static byte[] ToUtf8Bytes(this long longVal)
        {
            return FastToUtf8Bytes(longVal.ToString());
        }

        public static byte[] ToUtf8Bytes(this double doubleVal)
        {
            var doubleStr = doubleVal.ToString(CultureInfo.InvariantCulture.NumberFormat);

            if (doubleStr.IndexOf('E') != -1 || doubleStr.IndexOf('e') != -1)
                doubleStr = DoubleConverter.ToExactString(doubleVal);

            return FastToUtf8Bytes(doubleStr);
        }

        /// <summary>
        /// Skip the encoding process for 'safe strings' 
        /// </summary>
        /// <param name="strVal"></param>
        /// <returns></returns>
        private static byte[] FastToUtf8Bytes(string strVal)
        {
            var bytes = new byte[strVal.Length];
            for (var i = 0; i < strVal.Length; i++)
                bytes[i] = (byte)strVal[i];

            return bytes;
        }

        public static string[] SplitOnFirst(this string strVal, char needle)
        {
            if (strVal == null) return new string[0];
            var pos = strVal.IndexOf(needle);
            return pos == -1
                ? new[] { strVal }
                : new[] { strVal.Substring(0, pos), strVal.Substring(pos + 1) };
        }

        public static string[] SplitOnFirst(this string strVal, string needle)
        {
            if (strVal == null) return new string[0];
            var pos = strVal.IndexOf(needle);
            return pos == -1
                ? new[] { strVal }
                : new[] { strVal.Substring(0, pos), strVal.Substring(pos + 1) };
        }

        public static string[] SplitOnLast(this string strVal, char needle)
        {
            if (strVal == null) return new string[0];
            var pos = strVal.LastIndexOf(needle);
            return pos == -1
                ? new[] { strVal }
                : new[] { strVal.Substring(0, pos), strVal.Substring(pos + 1) };
        }

        public static string[] SplitOnLast(this string strVal, string needle)
        {
            if (strVal == null) return new string[0];
            var pos = strVal.LastIndexOf(needle);
            return pos == -1
                ? new[] { strVal }
                : new[] { strVal.Substring(0, pos), strVal.Substring(pos + 1) };
        }

        public static string WithoutExtension(this string filePath)
        {
            if (String.IsNullOrEmpty(filePath)) return null;

            var extPos = filePath.LastIndexOf('.');
            if (extPos == -1) return filePath;

            var dirPos = filePath.LastIndexOfAny(DirSeps);
            return extPos > dirPos ? filePath.Substring(0, extPos) : filePath;
        }

#if NETFX_CORE
        private static readonly char DirSep = '\\';//Path.DirectorySeparatorChar;
        private static readonly char AltDirSep = '/';//Path.DirectorySeparatorChar == '/' ? '\\' : '/';
#else
        private static readonly char DirSep = Path.DirectorySeparatorChar;
        private static readonly char AltDirSep = Path.DirectorySeparatorChar == '/' ? '\\' : '/';
#endif
        static readonly char[] DirSeps = new[] { '\\', '/' };

        public static string ParentDirectory(this string filePath)
        {
            if (String.IsNullOrEmpty(filePath)) return null;

            var dirSep = filePath.IndexOf(DirSep) != -1
                         ? DirSep
                         : filePath.IndexOf(AltDirSep) != -1 ? AltDirSep : (char)0;

            return dirSep == 0 ? null : filePath.TrimEnd(dirSep).SplitOnLast(dirSep)[0];
        }

        public static string ToJsv<T>(this T obj)
        {
            return TypeSerializer.SerializeToString(obj);
        }

        public static T FromJsv<T>(this string jsv)
        {
            return TypeSerializer.DeserializeFromString<T>(jsv);
        }

        public static string ToJson<T>(this T obj) {
            return JsConfig.PreferInterfaces
                ? JsonSerializer.SerializeToString(obj, AssemblyUtils.MainInterface<T>())
                : JsonSerializer.SerializeToString(obj);
        }

        public static T FromJson<T>(this string json)
        {
            return JsonSerializer.DeserializeFromString<T>(json);
        }

        public static string ToCsv<T>(this T obj)
        {
            return CsvSerializer.SerializeToString(obj);
        }

#if !XBOX && !SILVERLIGHT && !MONOTOUCH
        public static string ToXml<T>(this T obj)
        {
            return XmlSerializer.SerializeToString(obj);
        }
#endif

#if !XBOX && !SILVERLIGHT && !MONOTOUCH
        public static T FromXml<T>(this string json)
        {
            return XmlSerializer.DeserializeFromString<T>(json);
        }
#endif
        public static string FormatWith(this string text, params object[] args)
        {
            return String.Format(text, args);
        }

        public static string Fmt(this string text, params object[] args)
        {
            return String.Format(text, args);
        }

        public static bool StartsWithIgnoreCase(this string text, string startsWith)
        {
#if NETFX_CORE
            return text != null
                && text.StartsWith(startsWith, StringComparison.CurrentCultureIgnoreCase);
#else
            return text != null
                && text.StartsWith(startsWith, StringComparison.InvariantCultureIgnoreCase);
#endif
        }

        public static bool EndsWithIgnoreCase(this string text, string endsWith)
        {
#if NETFX_CORE
            return text != null
                && text.EndsWith(endsWith, StringComparison.CurrentCultureIgnoreCase);
#else
            return text != null
                && text.EndsWith(endsWith, StringComparison.InvariantCultureIgnoreCase);
#endif
        }

        public static string ReadAllText(this string filePath)
        {
#if XBOX && !SILVERLIGHT
            using( var fileStream = new FileStream( filePath, FileMode.Open, FileAccess.Read ) )
            {
                return new StreamReader( fileStream ).ReadToEnd( ) ;
            }
#elif NETFX_CORE
            var task = Windows.Storage.StorageFile.GetFileFromPathAsync(filePath);
            task.AsTask().Wait();

            var file = task.GetResults();
            
            var streamTask = file.OpenStreamForReadAsync();
            streamTask.Wait();

            var fileStream = streamTask.Result;

            return new StreamReader( fileStream ).ReadToEnd( ) ;
#elif WINDOWS_PHONE
            using (var isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var fileStream = isoStore.OpenFile(filePath, FileMode.Open))
                {
                    return new StreamReader(fileStream).ReadToEnd();
                }
            }
#else
            return File.ReadAllText(filePath);
#endif
        }


        public static int IndexOfAny(this string text, params string[] needles)
        {
            return IndexOfAny(text, 0, needles);
        }

        public static int IndexOfAny(this string text, int startIndex, params string[] needles)
        {
            if (text == null) return -1;

            var firstPos = -1;
            foreach (var needle in needles)
            {
                var pos = text.IndexOf(needle);
                if (firstPos == -1 || pos < firstPos) firstPos = pos;
            }
            return firstPos;
        }

        public static string ExtractContents(this string fromText, string startAfter, string endAt)
        {
            return ExtractContents(fromText, startAfter, startAfter, endAt);
        }

        public static string ExtractContents(this string fromText, string uniqueMarker, string startAfter, string endAt)
        {
            if (String.IsNullOrEmpty(uniqueMarker))
                throw new ArgumentNullException("uniqueMarker");
            if (String.IsNullOrEmpty(startAfter))
                throw new ArgumentNullException("startAfter");
            if (String.IsNullOrEmpty(endAt))
                throw new ArgumentNullException("endAt");

            if (String.IsNullOrEmpty(fromText)) return null;

            var markerPos = fromText.IndexOf(uniqueMarker);
            if (markerPos == -1) return null;

            var startPos = fromText.IndexOf(startAfter, markerPos);
            if (startPos == -1) return null;
            startPos += startAfter.Length;

            var endPos = fromText.IndexOf(endAt, startPos);
            if (endPos == -1) endPos = fromText.Length;

            return fromText.Substring(startPos, endPos - startPos);
        }

#if XBOX && !SILVERLIGHT
        static readonly Regex StripHtmlRegEx = new Regex(@"<(.|\n)*?>", RegexOptions.Compiled);
#else
        static readonly Regex StripHtmlRegEx = new Regex(@"<(.|\n)*?>");
#endif
        public static string StripHtml(this string html)
        {
            return String.IsNullOrEmpty(html) ? null : StripHtmlRegEx.Replace(html, "");
        }

#if XBOX && !SILVERLIGHT
        static readonly Regex StripBracketsRegEx = new Regex(@"\[(.|\n)*?\]", RegexOptions.Compiled);
        static readonly Regex StripBracesRegEx = new Regex(@"\((.|\n)*?\)", RegexOptions.Compiled);
#else
        static readonly Regex StripBracketsRegEx = new Regex(@"\[(.|\n)*?\]");
        static readonly Regex StripBracesRegEx = new Regex(@"\((.|\n)*?\)");
#endif
        public static string StripMarkdownMarkup(this string markdown)
        {
            if (String.IsNullOrEmpty(markdown)) return null;
            markdown = StripBracketsRegEx.Replace(markdown, "");
            markdown = StripBracesRegEx.Replace(markdown, "");
            markdown = markdown
                .Replace("*", "")
                .Replace("!", "")
                .Replace("\r", "")
                .Replace("\n", "")
                .Replace("#", "");

            return markdown;
        }

        private const int LowerCaseOffset = 'a' - 'A';
        public static string ToCamelCase(this string value)
        {
            if (String.IsNullOrEmpty(value)) return value;

            var len = value.Length;
            var newValue = new char[len];
            var firstPart = true;

            for (var i = 0; i < len; ++i) {
                var c0 = value[i];
                var c1 = i < len - 1 ? value[i + 1] : 'A';
                var c0isUpper = c0 >= 'A' && c0 <= 'Z';
                var c1isUpper = c1 >= 'A' && c1 <= 'Z';

                if (firstPart && c0isUpper && (c1isUpper || i == 0))
                    c0 = (char)(c0 + LowerCaseOffset);
                else
                    firstPart = false;

                newValue[i] = c0;
            }

            return new string(newValue);
        }

        private static readonly TextInfo TextInfo = CultureInfo.InvariantCulture.TextInfo;
        public static string ToTitleCase(this string value)
        {
#if SILVERLIGHT || __MonoCS__
            string[] words = value.Split('_');

            for (int i = 0; i <= words.Length - 1; i++)
            {
                if ((!object.ReferenceEquals(words[i], string.Empty)))
                {
                    string firstLetter = words[i].Substring(0, 1);
                    string rest = words[i].Substring(1);
                    string result = firstLetter.ToUpper() + rest.ToLower();
                    words[i] = result;
                }
            }
            return String.Join("", words);
#else
            return TextInfo.ToTitleCase(value).Replace("_", String.Empty);
#endif
        }

        public static string ToLowercaseUnderscore(this string value)
        {
            if (String.IsNullOrEmpty(value)) return value;
            value = value.ToCamelCase();
            
            var sb = new StringBuilder(value.Length);
            foreach (var t in value)
            {
                if (Char.IsLower(t) || t == '_')
                {
                    sb.Append(t);
                }
                else
                {
                    sb.Append("_");
                    sb.Append(Char.ToLowerInvariant(t));
                }
            }
            return sb.ToString();
        }

        public static string SafeSubstring(this string value, int length)
        {
            return String.IsNullOrEmpty(value)
                ? String.Empty
                : value.Substring(Math.Min(length, value.Length));
        }

        public static string SafeSubstring(this string value, int startIndex, int length)
        {
            if (String.IsNullOrEmpty(value)) return String.Empty;
            if (value.Length >= (startIndex + length))
                return value.Substring(startIndex, length);

            return value.Length > startIndex ? value.Substring(startIndex) : String.Empty;
        }

        public static bool IsAnonymousType(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            // HACK: The only way to detect anonymous types right now.
#if NETFX_CORE
            return type.IsGeneric() && type.Name.Contains("AnonymousType")
                && (type.Name.StartsWith("<>", StringComparison.Ordinal) || type.Name.StartsWith("VB$", StringComparison.Ordinal));
#else
            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                && type.IsGeneric() && type.Name.Contains("AnonymousType")
                && (type.Name.StartsWith("<>", StringComparison.Ordinal) || type.Name.StartsWith("VB$", StringComparison.Ordinal))
                && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
#endif
        }
        
        public static int CompareIgnoreCase(this string strA, string strB)
        {
            return String.Compare(strA, strB, InvariantComparisonIgnoreCase());
        }

        public static bool EndsWithInvariant(this string str, string endsWith)
        {
            return str.EndsWith(endsWith, InvariantComparison());
        }

#if NETFX_CORE
        public static char DirSeparatorChar = '\\';
        public static StringComparison InvariantComparison()
        {
            return StringComparison.CurrentCulture;
        }
        public static StringComparison InvariantComparisonIgnoreCase()
        {
            return StringComparison.CurrentCultureIgnoreCase;
        }
        public static StringComparer InvariantComparer()
        {
            return StringComparer.CurrentCulture;
        }
        public static StringComparer InvariantComparerIgnoreCase()
        {
            return StringComparer.CurrentCultureIgnoreCase;
        }
#else
        public static char DirSeparatorChar = Path.DirectorySeparatorChar;
        public static StringComparison InvariantComparison()
        {
            return StringComparison.InvariantCulture;
        }
        public static StringComparison InvariantComparisonIgnoreCase()
        {
            return StringComparison.InvariantCultureIgnoreCase;
        }
        public static StringComparer InvariantComparer()
        {
            return StringComparer.InvariantCulture;
        }
        public static StringComparer InvariantComparerIgnoreCase()
        {
            return StringComparer.InvariantCultureIgnoreCase;
        }
#endif

    }
}