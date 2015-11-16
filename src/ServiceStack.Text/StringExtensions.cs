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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ServiceStack.Text;
using ServiceStack.Text.Common;
using ServiceStack.Text.Support;

namespace ServiceStack
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
            var chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var len = source.Length;
            if (len == 0)
                throw new Exception(string.Format("Parameter: '{0}' is not valid integer (in base {1}).", source, from));
            var minus = source[0] == '-' ? "-" : "";
            var src = minus == "" ? source : source.Substring(1);
            len = src.Length;
            if (len == 0)
                throw new Exception(string.Format("Parameter: '{0}' is not valid integer (in base {1}).", source, from));

            var d = 0;
            for (int i = 0; i < len; i++) // Convert to decimal
            {
                int c = chars.IndexOf(src[i]);
                if (c >= from)
                    throw new Exception(string.Format("Parameter: '{0}' is not valid integer (in base {1}).", source, from));
                d = d * from + c;
            }
            if (to == 10 || d == 0)
                return minus + d;

            var result = "";
            while (d > 0)   // Convert to desired
            {
                result = chars[d % to] + result;
                d /= to;
            }
            return minus + result;
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

        public static string UrlEncode(this string text, bool upperCase=false)
        {
            if (String.IsNullOrEmpty(text)) return text;

            var sb = new StringBuilder();
            var fmt = upperCase ? "X2" : "x2";

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
                else if(charCode == 32)
                {
                    sb.Append('+');
                }
                else
                {
                    sb.Append('%' + charCode.ToString(fmt));
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

            byte[] byteArray = bytes.ToArray();
            return Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
        }

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

        public static string AppendUrlPathsRaw(this string uri, params string[] uriComponents)
        {
            var sb = new StringBuilder(uri.WithTrailingSlash());
            var i = 0;
            foreach (var uriComponent in uriComponents)
            {
                if (i++ > 0) sb.Append('/');
                sb.Append(uriComponent);
            }
            return sb.ToString();
        }

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

        public static byte[] ToUtf8Bytes(this ulong ulongVal)
        {
            return FastToUtf8Bytes(ulongVal.ToString());
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
                : new[] { strVal.Substring(0, pos), strVal.Substring(pos + needle.Length) };
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
                : new[] { strVal.Substring(0, pos), strVal.Substring(pos + needle.Length) };
        }

        public static string WithoutExtension(this string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) 
                return null;

            var extPos = filePath.LastIndexOf('.');
            if (extPos == -1) return filePath;

            var dirPos = filePath.LastIndexOfAny(DirSeps);
            return extPos > dirPos ? filePath.Substring(0, extPos) : filePath;
        }

        public static string GetExtension(this string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) 
                return null;

            var extPos = filePath.LastIndexOf('.');
            return extPos == -1 ? string.Empty : filePath.Substring(extPos);
        }

        static readonly char[] DirSeps = new[] { '\\', '/' };

        public static string ParentDirectory(this string filePath)
        {
            if (String.IsNullOrEmpty(filePath)) return null;

            var dirSep = filePath.IndexOf(PclExport.Instance.DirSep) != -1
                         ? PclExport.Instance.DirSep
                         : filePath.IndexOf(PclExport.Instance.AltDirSep) != -1
                            ? PclExport.Instance.AltDirSep 
                            : (char)0;

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

        public static string ToJson<T>(this T obj)
        {
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
            return text != null
                && text.StartsWith(startsWith, PclExport.Instance.InvariantComparisonIgnoreCase);
        }

        public static bool EndsWithIgnoreCase(this string text, string endsWith)
        {
            return text != null
                && text.EndsWith(endsWith, PclExport.Instance.InvariantComparisonIgnoreCase);
        }

        public static string ReadAllText(this string filePath)
        {
            return PclExport.Instance.ReadAllText(filePath);
        }

        public static bool FileExists(this string filePath)
        {
            return PclExport.Instance.FileExists(filePath);
        }

        public static bool DirectoryExists(this string dirPath)
        {
            return PclExport.Instance.DirectoryExists(dirPath);
        }

        public static void CreateDirectory(this string dirPath)
        {
            PclExport.Instance.CreateDirectory(dirPath);
        }

        public static int IndexOfAny(this string text, params string[] needles)
        {
            return IndexOfAny(text, 0, needles);
        }

        public static int IndexOfAny(this string text, int startIndex, params string[] needles)
        {
            var firstPos = -1;
            if (text != null)
            {
                foreach (var needle in needles)
                {
                    var pos = text.IndexOf(needle, startIndex);
                    if ((pos >= 0) && (firstPos == -1 || pos < firstPos))
                        firstPos = pos;
                }
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

        static readonly Regex StripHtmlRegEx = new Regex(@"<(.|\n)*?>", PclExport.Instance.RegexOptions);

        public static string StripHtml(this string html)
        {
            return String.IsNullOrEmpty(html) ? null : StripHtmlRegEx.Replace(html, "");
        }

        public static string StripQuotes(this string text)
        {
            return string.IsNullOrEmpty(text) || text.Length < 2
                ? text
                : text[0] == '"' && text[text.Length - 1] == '"'
                    ? text.Substring(1, text.Length - 2)
                    : text;
        }

        static readonly Regex StripBracketsRegEx = new Regex(@"\[(.|\n)*?\]", PclExport.Instance.RegexOptions);
        static readonly Regex StripBracesRegEx = new Regex(@"\((.|\n)*?\)", PclExport.Instance.RegexOptions);

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
            if (string.IsNullOrEmpty(value)) return value;

            var len = value.Length;
            var newValue = new char[len];
            var firstPart = true;

            for (var i = 0; i < len; ++i)
            {
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

        public static string ToPascalCase(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            if (value.IndexOf('_') >= 0)
            {
                var parts = value.Split('_');
                var sb = new StringBuilder();
                foreach (var part in parts)
                {
                    var str = part.ToCamelCase();
                    sb.Append(Char.ToUpper(str[0]) + str.SafeSubstring(1, str.Length));
                }
                return sb.ToString();
            }

            var camelCase = value.ToCamelCase();
            return Char.ToUpper(camelCase[0]) + camelCase.SafeSubstring(1, camelCase.Length);
        }

        public static string ToTitleCase(this string value)
        {
            return PclExport.Instance.ToTitleCase(value);
        }

        public static string ToLowercaseUnderscore(this string value)
        {
            if (String.IsNullOrEmpty(value)) return value;
            value = value.ToCamelCase();

            var sb = new StringBuilder(value.Length);
            foreach (char t in value)
            {
                if (Char.IsDigit(t) || (Char.IsLetter(t) && Char.IsLower(t)) || t == '_')
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

        public static string ToLowerSafe(this string value)
        {
            return value != null ? value.ToLower() : null;
        }

        public static string ToUpperSafe(this string value)
        {
            return value != null ? value.ToUpper() : null;
        }

        public static string SafeSubstring(this string value, int startIndex)
        {
            return SafeSubstring(value, startIndex, value.Length);
        }

        public static string SafeSubstring(this string value, int startIndex, int length)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            if (value.Length >= (startIndex + length))
                return value.Substring(startIndex, length);

            return value.Length > startIndex ? value.Substring(startIndex) : String.Empty;
        }

        public static bool IsAnonymousType(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return PclExport.Instance.IsAnonymousType(type);
        }

        public static int CompareIgnoreCase(this string strA, string strB)
        {
            return String.Compare(strA, strB, PclExport.Instance.InvariantComparisonIgnoreCase);
        }

        public static bool EndsWithInvariant(this string str, string endsWith)
        {
            return str.EndsWith(endsWith, PclExport.Instance.InvariantComparison);
        }

        private static readonly Regex InvalidVarCharsRegex = new Regex(@"[^A-Za-z0-9]", PclExport.Instance.RegexOptions);
        private static readonly Regex SplitCamelCaseRegex = new Regex("([A-Z]|[0-9]+)", PclExport.Instance.RegexOptions);
        private static readonly Regex HttpRegex = new Regex(@"^http://",
            PclExport.Instance.RegexOptions | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        public static T ToEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static T ToEnumOrDefault<T>(this string value, T defaultValue)
        {
            if (String.IsNullOrEmpty(value)) return defaultValue;
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static string SplitCamelCase(this string value)
        {
            return SplitCamelCaseRegex.Replace(value, " $1").TrimStart();
        }

        public static string ToInvariantUpper(this char value)
        {
            return PclExport.Instance.ToInvariantUpper(value);
        }

        public static string ToEnglish(this string camelCase)
        {
            var ucWords = camelCase.SplitCamelCase().ToLower();
            return ucWords[0].ToInvariantUpper() + ucWords.Substring(1);
        }

        public static string ToHttps(this string url)
        {
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }
            return HttpRegex.Replace(url.Trim(), "https://");
        }

        public static bool IsEmpty(this string value)
        {
            return String.IsNullOrEmpty(value);
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return String.IsNullOrEmpty(value);
        }

        public static bool EqualsIgnoreCase(this string value, string other)
        {
            return String.Equals(value, other, StringComparison.CurrentCultureIgnoreCase);
        }

        public static string ReplaceFirst(this string haystack, string needle, string replacement)
        {
            var pos = haystack.IndexOf(needle);
            if (pos < 0) return haystack;

            return haystack.Substring(0, pos) + replacement + haystack.Substring(pos + needle.Length);
        }

        public static string ReplaceAll(this string haystack, string needle, string replacement)
        {
            int pos;
            // Avoid a possible infinite loop
            if (needle == replacement) return haystack;
            while ((pos = haystack.IndexOf(needle)) > 0)
            {
                haystack = haystack.Substring(0, pos)
                    + replacement
                    + haystack.Substring(pos + needle.Length);
            }
            return haystack;
        }

        public static bool ContainsAny(this string text, params string[] testMatches)
        {
            foreach (var testMatch in testMatches)
            {
                if (text.Contains(testMatch)) return true;
            }
            return false;
        }

        public static string SafeVarName(this string text)
        {
            if (String.IsNullOrEmpty(text)) return null;
            return InvalidVarCharsRegex.Replace(text, "_");
        }

        public static string Join(this List<string> items)
        {
            return String.Join(JsWriter.ItemSeperatorString, items.ToArray());
        }

        public static string Join(this List<string> items, string delimeter)
        {
            return String.Join(delimeter, items.ToArray());
        }

        public static string ToParentPath(this string path)
        {
            var pos = path.LastIndexOf('/');
            if (pos == -1) return "/";

            var parentPath = path.Substring(0, pos);
            return parentPath;
        }

        public static string RemoveCharFlags(this string text, bool[] charFlags)
        {
            if (text == null) return null;

            var copy = text.ToCharArray();
            var nonWsPos = 0;

            for (var i = 0; i < text.Length; i++)
            {
                var @char = text[i];
                if (@char < charFlags.Length && charFlags[@char]) continue;
                copy[nonWsPos++] = @char;
            }

            return new String(copy, 0, nonWsPos);
        }

        public static string ToNullIfEmpty(this string text)
        {
            return String.IsNullOrEmpty(text) ? null : text;
        }
        
        private static char[] SystemTypeChars = new[] { '<', '>', '+' };

        public static bool IsUserType(this Type type)
        {
            return type.IsClass()
                && !type.IsSystemType();
        }

        public static bool IsUserEnum(this Type type)
        {
            return type.IsEnum()
                && !type.IsSystemType();
        }

        public static bool IsSystemType(this Type type)
        {
            return type.Namespace == null
                || type.Namespace.StartsWith("System")
                || type.Name.IndexOfAny(SystemTypeChars) >= 0;
        }

        public static bool IsInt(this string text)
        {
            if (String.IsNullOrEmpty(text)) return false;
            int ret;
            return Int32.TryParse(text, out ret);
        }

        public static int ToInt(this string text)
        {
            return text == null ? default(int) : Int32.Parse(text);
        }

        public static int ToInt(this string text, int defaultValue)
        {
            int ret;
            return Int32.TryParse(text, out ret) ? ret : defaultValue;
        }

        public static long ToInt64(this string text)
        {
            return Int64.Parse(text);
        }

        public static long ToInt64(this string text, long defaultValue)
        {
            long ret;
            return Int64.TryParse(text, out ret) ? ret : defaultValue;
        }

        public static float ToFloat(this string text)
        {
            return text == null ? default(float) : float.Parse(text);
        }

        public static float ToFloat(this string text, float defaultValue)
        {
            float ret;
            return float.TryParse(text, out ret) ? ret : defaultValue;
        }

        public static double ToDouble(this string text)
        {
            return text == null ? default(double) : double.Parse(text);
        }

        public static double ToDouble(this string text, double defaultValue)
        {
            double ret;
            return double.TryParse(text, out ret) ? ret : defaultValue;
        }

        public static decimal ToDecimal(this string text)
        {
            return text == null ? default(decimal) : decimal.Parse(text);
        }

        public static decimal ToDecimal(this string text, decimal defaultValue)
        {
            decimal ret;
            return decimal.TryParse(text, out ret) ? ret : defaultValue;
        }

        public static bool Matches(this string value, string pattern)
        {
            return value.Glob(pattern);
        }

        public static bool Glob(this string value, string pattern)
        {
            int pos;
            for (pos = 0; pattern.Length != pos; pos++)
            {
                switch (pattern[pos])
                {
                    case '?':
                        break;

                    case '*':
                        for (int i = value.Length; i >= pos; i--)
                        {
                            if (Glob(value.Substring(i), pattern.Substring(pos + 1)))
                                return true;
                        }
                        return false;

                    default:
                        if (value.Length == pos || Char.ToUpper(pattern[pos]) != Char.ToUpper(value[pos]))
                        {
                            return false;
                        }
                        break;
                }
            }

            return value.Length == pos;
        }

        public static string TrimPrefixes(this string fromString, params string[] prefixes)
        {
            if (string.IsNullOrEmpty(fromString))
                return fromString;

            foreach (var prefix in prefixes)
            {
                if (fromString.StartsWith(prefix))
                    return fromString.Substring(prefix.Length);
            }

            return fromString;
        }

        public static string FromAsciiBytes(this byte[] bytes)
        {
            return bytes == null ? null
                : PclExport.Instance.GetAsciiString(bytes);
        }

        public static byte[] ToAsciiBytes(this string value)
        {
            return PclExport.Instance.GetAsciiBytes(value);
        }

        public static Dictionary<string,string> ParseKeyValueText(this string text, string delimiter=" ")
        {
            var to = new Dictionary<string, string>();
            if (text == null) return to;

            foreach (var parts in text.ReadLines().Select(line => line.SplitOnFirst(delimiter)))
            {
                var key = parts[0].Trim();
                if (key.Length == 0 || key.StartsWith("#")) continue;
                to[key] = parts.Length == 2 ? parts[1].Trim() : null;
            }

            return to;
        }

        public static IEnumerable<string> ReadLines(this string text)
        {
            string line;
            var reader = new StringReader(text ?? "");
            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }

        public static int CountOccurrencesOf(this string text, char needle)
        {
            var chars = text.ToCharArray();
            var count = 0;
            var length = chars.Length;
            for (var n = length - 1; n >= 0; n--)
            {
                if (chars[n] == needle)
                    count++;
            }
            return count;
        }

        public static string NormalizeNewLines(this string text)
        {
            return text != null
                ? text.Replace("\r\n", "\n")
                : null;
        }

#if !LITE
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

        public static string ToXml<T>(this T obj)
        {
            return XmlSerializer.SerializeToString(obj);
        }

        public static T FromXml<T>(this string json)
        {
            return XmlSerializer.DeserializeFromString<T>(json);
        }
#endif

    }
}
