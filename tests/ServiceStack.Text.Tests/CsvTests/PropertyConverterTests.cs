using System;
using NUnit.Framework;
using ServiceStack.Text.Csv;

namespace ServiceStack.Text.Tests.CsvTests
{
	[TestFixture]
	public class PropertyConverterTests
	{
		[Test]
		public void Can_deul_with_string_to_bool_transform()
		{
			var propertyInfo = PropertyConvertor.GetProperty<QueryRow>("Ignore");

			var queryRow = new QueryRow();
			PropertyConvertor.SetValue(queryRow, propertyInfo, "false");
			Assert.That(queryRow.Ignore, Is.False);

			PropertyConvertor.SetValue(queryRow, propertyInfo, "true");
			Assert.That(queryRow.Ignore);
		}

		[Test]
		public void Can_deal_with_numeric_transforms()
		{

			var testObject = new Test();
			var shortProperty = PropertyConvertor.GetProperty<Test>("Short");
			PropertyConvertor.SetValue(testObject, shortProperty, short.MaxValue.ToString());
			Assert.That(testObject.Short, Is.EqualTo(short.MaxValue));

			var intProperty = PropertyConvertor.GetProperty<Test>("Int");
			PropertyConvertor.SetValue(testObject, intProperty, int.MaxValue.ToString());
			Assert.That(testObject.Int, Is.EqualTo(int.MaxValue));

			var longProperty = PropertyConvertor.GetProperty<Test>("Long");
			PropertyConvertor.SetValue(testObject, longProperty, long.MaxValue.ToString());
			Assert.That(testObject.Long, Is.EqualTo(long.MaxValue));
		}

		[Test]
		public void Can_deal_with_datetime_transforms()
		{
			var testObject = new Test();
			var property = PropertyConvertor.GetProperty<Test>("TImestamp");
			PropertyConvertor.SetValue(testObject, property, DateTime.MinValue.ToString());
			Assert.That(testObject.TImestamp, Is.EqualTo(DateTime.MinValue));
		}

		internal class Test
		{
			public short Short { get; set; }
			public int Int { get; set; }
			public long Long { get; set; }
			public DateTime TImestamp { get; set; }
		}

	}
}