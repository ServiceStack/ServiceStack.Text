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
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using ServiceStack.Text.Support;

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
			return string.IsNullOrEmpty(value) ? defaultValue : TypeSerializer.DeserializeFromString<T>(value);
		}

		public static T ToOrDefaultValue<T>(this string value)
		{
			return string.IsNullOrEmpty(value) ? default(T) : TypeSerializer.DeserializeFromString<T>(value);
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
					divide = divide * from + number[i];

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
			return string.Concat
			("\"",
				value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "\\n"),
				"\""
			);
		}

		public static string EncodeJsv(this string value)
		{
			return value.ToCsvField();
		}

		public static string UrlEncode(this string text)
		{
			if (string.IsNullOrEmpty(text)) return text;

			var sb = new StringBuilder();

			var textLength = text.Length;
			for (var i = 0; i < textLength; i++)
			{
				var c = text.Substring(i, 1);
				int charCode = text[i];

				if (
					charCode >= 65 && charCode <= 90		// A-Z
					|| charCode >= 97 && charCode <= 122    // a-z
					|| charCode >= 48 && charCode <= 57		// 0-9
					|| charCode >= 44 && charCode <= 46		// ,-.
					)
				{
					sb.Append(c);
				}
				else
				{
					sb.Append('%' + charCode.ToString("x"));
				}
			}
			return sb.ToString();
		}

		public static string UrlDecode(this string text)
		{
			if (string.IsNullOrEmpty(text)) return null;

			var sb = new StringBuilder();

			var textLength = text.Length;
			for (var i = 0; i < textLength; i++)
			{
				var c = text.Substring(i, 1);
				if (c == "+")
				{
					sb.Append(" ");
				}
				else if (c == "%")
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

#if !XBOX
		public static string HexEscape(this string text, params char[] anyCharOf)
		{
			if (string.IsNullOrEmpty(text)) return text;
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
			if (string.IsNullOrEmpty(text)) return null;
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

			return string.Format(url, encodedUrlComponents);
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
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException("path");

			if (path[path.Length - 1] != '/')
			{
				return path + "/";
			}
			return path;
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
			if (string.IsNullOrEmpty(filePath)) return null;

			var extPos = filePath.LastIndexOf('.');
			if (extPos == -1) return filePath;

			var dirPos = filePath.LastIndexOfAny(DirSeps);
			return extPos > dirPos ? filePath.Substring(0, extPos) : filePath;
		}

		private static readonly char DirSep = Path.DirectorySeparatorChar;
		private static readonly char AltDirSep = Path.DirectorySeparatorChar == '/' ? '\\' : '/';
		static readonly char[] DirSeps = new[] { '\\', '/' };

		public static string ParentDirectory(this string filePath)
		{
			if (string.IsNullOrEmpty(filePath)) return null;

			var dirSep = filePath.IndexOf(DirSep) != -1
			             ? DirSep
			             : filePath.IndexOf(AltDirSep) != -1 ? AltDirSep : (char)0;

			return dirSep == 0 ? null : filePath.TrimEnd(dirSep).SplitOnLast(dirSep)[0];
		}

		public static string ToJsv<T>(this T obj)
		{
			return TypeSerializer.SerializeToString<T>(obj);
		}

		public static T FromJsv<T>(this string jsv)
		{
			return TypeSerializer.DeserializeFromString<T>(jsv);
		}

		public static string ToJson<T>(this T obj)
		{
			return JsonSerializer.SerializeToString<T>(obj);
		}

		public static T FromJson<T>(this string json)
		{
			return JsonSerializer.DeserializeFromString<T>(json);
		}

#if !XBOX
		public static string ToXml<T>(this T obj)
		{
			return XmlSerializer.SerializeToString<T>(obj);
		}
#endif

#if !XBOX
		public static T FromXml<T>(this string json)
		{
			return XmlSerializer.DeserializeFromString<T>(json);
		}
#endif
		public static string FormatWith(this string text, params object[] args)
		{
			return string.Format(text, args);
		}

		public static string Fmt(this string text, params object[] args)
		{
			return string.Format(text, args);
		}

		public static bool StartsWithIgnoreCase(this string text, string startsWith)
		{
			return text != null 
				&& text.StartsWith(startsWith, StringComparison.InvariantCultureIgnoreCase);
		}

		public static string ReadAllText(this string filePath)
		{
#if XBOX
			using( var fileStream = new FileStream( filePath, FileMode.Open, FileAccess.Read ) )
			{
				return new StreamReader( fileStream ).ReadToEnd( ) ;
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
			if (string.IsNullOrEmpty(uniqueMarker))
				throw new ArgumentNullException("uniqueMarker");
			if (string.IsNullOrEmpty(startAfter))
				throw new ArgumentNullException("startAfter");
			if (string.IsNullOrEmpty(endAt))
				throw new ArgumentNullException("endAt");

			if (string.IsNullOrEmpty(fromText)) return null;

			var markerPos = fromText.IndexOf(uniqueMarker);
			if (markerPos == -1) return null;

			var startPos = fromText.IndexOf(startAfter, markerPos);
			if (startPos == -1) return null;
			startPos += startAfter.Length;

			var endPos = fromText.IndexOf(endAt, startPos);
			if (endPos == -1) endPos = fromText.Length;

			return fromText.Substring(startPos, endPos - startPos);
		}

		static readonly Regex StripHtmlRegEx = new Regex(@"<(.|\n)*?>", RegexOptions.Compiled);
		public static string StripHtml(this string html)
		{
			return string.IsNullOrEmpty(html) ? null : StripHtmlRegEx.Replace(html, "");
		}

		static readonly Regex StripBracketsRegEx = new Regex(@"\[(.|\n)*?\]", RegexOptions.Compiled);
		static readonly Regex StripBracesRegEx = new Regex(@"\((.|\n)*?\)", RegexOptions.Compiled);
		public static string StripMarkdownMarkup(this string markdown)
		{
			if (string.IsNullOrEmpty(markdown)) return null;
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

			var firstChar = value[0];
			if (firstChar < 'A' || firstChar > 'Z')
				return value;

			return (char)(firstChar + LowerCaseOffset) + value.Substring(1);
		}

	}
}