using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ServiceStack.Text.Csv;

namespace ServiceStack.Text.Tests.CsvTests
{
	[TestFixture]
	public class CsvDeserializationTests
	{
		[Test]
		public void Should_deserialize_correct_csv()
		{
			var queryRows = CsvDeserialize.DeSerialize<QueryRow>(TestData.TestCsv).ToList();
			Assert.That(queryRows.Count, Is.EqualTo(2));
			Assert.That(queryRows[0].Artist, Is.EqualTo("Elton John"));
			Assert.That(queryRows[0].Country, Is.EqualTo("US"));
			Assert.That(queryRows[0].Query, Is.EqualTo("Your Song"));
			Assert.That(queryRows[0].Title, Is.EqualTo("Your Song"));
		}

		[Test]
		public void Should_deserialize_csv_that_contains_blank_column()
		{
			var queryRows = CsvDeserialize.DeSerialize<QueryRow>(TestData.TestCsvBlankColumn).ToList();
			Assert.That(queryRows.Count, Is.EqualTo(1));
			Assert.That(queryRows[0].Artist, Is.EqualTo("Guns 'n Roses"));
			Assert.That(queryRows[0].Country, Is.EqualTo("US"));
			Assert.That(queryRows[0].Query, Is.EqualTo(""));
			Assert.That(queryRows[0].Title, Is.EqualTo("Patience"));
		}

		[Test]
		public void Should_deserialize_csv_that_contains_comma()
		{
			var queryRows = CsvDeserialize.DeSerialize<QueryRow>(TestData.TestCsvCommaColumn).ToList();
			Assert.That(queryRows.Count, Is.EqualTo(1));
			Assert.That(queryRows[0].Artist, Is.EqualTo("Oasis"));
			Assert.That(queryRows[0].Country, Is.EqualTo("UK"));
			Assert.That(queryRows[0].Query, Is.EqualTo("Definately, Maybe"));
			Assert.That(queryRows[0].Title, Is.EqualTo("Definately, Maybe"));
		}

		[Test]
		public void Should_throw_meaningful_execption_if_header_row_doesnt_match_type()
		{
			var testCsv = TestData.TestCsv.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Skip(1).ToArray();
			var csvDeserializationException = Assert.Throws<CsvDeserializationException>(() => CsvDeserialize.DeSerialize<QueryRow>(testCsv));

			Assert.That(csvDeserializationException.Message, Is.StringStarting("PropertyName \"US\" is not a property of type "));
		}

		[Test]
		public void Should_throw_meaningful_exception_if_row_columns_does_not_match_header_columns()
		{
			var csvDeserializationException = Assert.Throws<CsvDeserializationException>(() => CsvDeserialize.DeSerialize<QueryRow>(TestData.TestCsvNotMatching).ToList());

			Assert.That(csvDeserializationException.Message, Is.EqualTo("Row length does not match header row length"));
		}

		[Test]
		public void Can_deserialize_from_stream()
		{
			var testCsv = TestData.TestCsv.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToArray();
			var tempFileName = Path.GetTempFileName();
			File.WriteAllLines(tempFileName, testCsv);

			using (var fs = new FileStream(tempFileName, FileMode.Open, FileAccess.Read))
			{
				var queryRows = CsvDeserialize.DeSerialize<QueryRow>(fs).ToList();
				Assert.That(queryRows.Count, Is.EqualTo(testCsv.Length - 1));
				Assert.That(queryRows[0].Artist, Is.EqualTo("Elton John"));
				Assert.That(queryRows[0].Country, Is.EqualTo("US"));
				Assert.That(queryRows[0].Query, Is.EqualTo("Your Song"));
				Assert.That(queryRows[0].Title, Is.EqualTo("Your Song"));
			}

			File.Delete(tempFileName);
		}
	}
}
