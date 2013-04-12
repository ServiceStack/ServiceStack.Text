using System.Collections.Generic;
using System.Text;

namespace ServiceStack.Text.Csv
{
	public static class CsvExtensions
	{
		public static string UppercaseFirst(this string s)
		{
			if (string.IsNullOrEmpty(s))
				return string.Empty;

			return char.ToUpper(s[0]) + s.Substring(1);
		}

		public static IEnumerable<string> SplitCsvRowHandlingQuotes(this string csvRow, char delimeter)
		{
			var sb = new StringBuilder();
			var isQuoted = false;

			foreach (var character in csvRow)
			{
				if (isQuoted)
				{
					if (character == '"')
						isQuoted = false;
					else
						sb.Append(character);
				}
				else
				{
					if (character == '"')
					{
						isQuoted = true;
					}
					else if (character == delimeter)
					{
						yield return sb.ToString();
						sb = new StringBuilder();
					}
					else
					{
						sb.Append(character);
					}
				}
			}

			if (isQuoted)
				throw new CsvDeserializationException("Csv contained unterminated quotation mark.");

			yield return sb.ToString();
		}
	}
}