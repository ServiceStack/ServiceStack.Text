using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ServiceStack.Text.Csv
{
	public static class CsvDeserialize
	{
		private const char DELIMETER = ',';

		/// <exception cref="CsvDeserializationException"></exception>
		public static IEnumerable<TEntity> DeSerialize<TEntity>(string testCsv)
		{
			var rows = testCsv.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			return DeSerialize<TEntity>(rows);
		}

		/// <exception cref="CsvDeserializationException"></exception>
		public static IEnumerable<TEntity> DeSerialize<TEntity>(string[] rows)
		{
			const int toSkip = 1;

			var headings = rows.ElementAt(0).Split(new[] { DELIMETER }, StringSplitOptions.None);
			var values = rows.Skip(toSkip);

			var properties = headings.Select(PropertyConvertor.GetProperty<TEntity>).ToArray();

			return values.Select(row => BuildEntity<TEntity>(properties, row));
		}

		/// <exception cref="CsvDeserializationException"></exception>
		public static IEnumerable<TEntity> DeSerialize<TEntity>(Stream stream)
		{
			using (var streamReader = new StreamReader(stream, Encoding.Default))
			{
				var properties = new PropertyInfo[] { };
				string row;
				var isFirstRow = true;
				while ((row = streamReader.ReadLine()) != null)
				{
					if (isFirstRow)
					{
						var headings = row.Split(new[] { DELIMETER }, StringSplitOptions.None);
						properties = headings.Select(PropertyConvertor.GetProperty<TEntity>).ToArray();
						isFirstRow = false;
					}
					else
					{
						yield return BuildEntity<TEntity>(properties, row);
					}
				}
			}
		}

		private static TEntity BuildEntity<TEntity>(IList<PropertyInfo> entitySchema, string row)
		{
			var entity = Activator.CreateInstance<TEntity>();

			var strings = row.SplitCsvRowHandlingQuotes(DELIMETER).ToArray();

			if (strings.Length != entitySchema.Count)
				throw new CsvDeserializationException("Row length does not match header row length");

			For(0, strings.Length, i => PropertyConvertor.SetValue(entity, entitySchema[i], strings[i]));

			return entity;
		}

		private static void For(int fromInclusive, int toExclusive, Action<int> loopAction)
		{
			for (var i = fromInclusive; i < toExclusive; i++)
			{
				loopAction(i);
			}
		}
	}
}
