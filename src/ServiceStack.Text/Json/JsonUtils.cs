using System;
using System.IO;

namespace ServiceStack.Text.Json
{
	public static class JsonUtils
	{
		public const char EscapeChar = '\\';
		public const char QuoteChar = '"';
		public const string Null = "null";
		public const string True = "true";
		public const string False = "false";

		static readonly char[] EscapeChars = new[]
			{
				QuoteChar, '\n', '\r', '\t', '"', '\\', '\f', '\b',
			};

		private const int LengthFromLargestChar = '\\' + 1;
		private static readonly bool[] EscapeCharFlags = new bool[LengthFromLargestChar];

		static JsonUtils()
		{
			foreach (var escapeChar in EscapeChars)
			{
				EscapeCharFlags[escapeChar] = true;
			}
		}
		
		public static void WriteString(TextWriter writer, string value)
		{
			if (value == null)
			{
                writer.Write(JsonUtils.Null);
				return;
			}
			if (!HasAnyEscapeChars(value))
			{
				writer.Write(QuoteChar);
				writer.Write(value);
				writer.Write(QuoteChar);
				return;
			}

			var hexSeqBuffer = new char[4];
			writer.Write(QuoteChar);

			var len = value.Length;
			for (var i = 0; i < len; i++)
			{
				switch (value[i])
				{
					case '\n':
						writer.Write("\\n");
						continue;

					case '\r':
						writer.Write("\\r");
						continue;

					case '\t':
						writer.Write("\\t");
						continue;

					case '"':
					case '\\':
						writer.Write('\\');
						writer.Write(value[i]);
						continue;

					case '\f':
						writer.Write("\\f");
						continue;

					case '\b':
						writer.Write("\\b");
						continue;
				}

				//Is printable char?
				if (value[i] >= 32 && value[i] <= 126)
				{
					writer.Write(value[i]);
					continue;
				}

				var isValidSequence = value[i] < 0xD800 || value[i] > 0xDFFF;
				if (isValidSequence)
				{
					// Default, turn into a \uXXXX sequence
					IntToHex(value[i], hexSeqBuffer);
					writer.Write("\\u");
					writer.Write(hexSeqBuffer);
				}
			}

			writer.Write(QuoteChar);
		}

		/// <summary>
		/// micro optimizations: using flags instead of value.IndexOfAny(EscapeChars)
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		private static bool HasAnyEscapeChars(string value)
		{
			var len = value.Length;
			for (var i = 0; i < len; i++)
			{
				var c = value[i];
				if (c >= LengthFromLargestChar || !EscapeCharFlags[c]) continue;
				return true;
			}
			return false;
		}

		public static void IntToHex(int intValue, char[] hex)
		{
			for (var i = 0; i < 4; i++)
			{
				var num = intValue % 16;

				if (num < 10)
					hex[3 - i] = (char)('0' + num);
				else
					hex[3 - i] = (char)('A' + (num - 10));

				intValue >>= 4;
			}
		}

		public static bool IsJsString(string value)
		{
			return !string.IsNullOrEmpty(value)
				&& value[0] == '{'
				&& value[value.Length - 1] == '}';
		}
		
		private static readonly int LowerCaseOffset = 'a' - 'A';
		public static string MemberNameToCamelCase(string value)
		{
			if(string.IsNullOrEmpty(value)) return value;
			var sb = new System.Text.StringBuilder();
			var firstChar = value[0];
			if(firstChar >= 'A' && firstChar <= 'Z')
			{
				firstChar = (char)((int)firstChar + LowerCaseOffset);
			}
			else
			{
				return value;
			}
			sb.Append(firstChar);
			sb.Append(value.Substring(1, value.Length - 1));
			return sb.ToString();
		}
	}

}