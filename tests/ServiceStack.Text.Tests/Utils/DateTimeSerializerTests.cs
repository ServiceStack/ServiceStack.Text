using System;
using NUnit.Framework;
using ServiceStack.Text.Common;

namespace ServiceStack.Text.Tests.Utils
{
	[TestFixture]
	public class DateTimeSerializerTests
		: TestBase
	{
		public void PrintFormats(DateTime dateTime)
		{
			Log("dateTime.ToShortDateString(): " + dateTime.ToShortDateString());
			Log("dateTime.ToShortTimeString(): " + dateTime.ToShortTimeString());
			Log("dateTime.ToLongTimeString(): " + dateTime.ToLongTimeString());
			Log("dateTime.ToShortTimeString(): " + dateTime.ToShortTimeString());
			Log("dateTime.ToString(): " + dateTime.ToString());
			Log("DateTimeSerializer.ToShortestXsdDateTimeString(dateTime): " + DateTimeSerializer.ToShortestXsdDateTimeString(dateTime));
			Log("DateTimeSerializer.ToDateTimeString(dateTime): " + DateTimeSerializer.ToDateTimeString(dateTime));
			Log("DateTimeSerializer.ToXsdDateTimeString(dateTime): " + DateTimeSerializer.ToXsdDateTimeString(dateTime));
			Log("\n");
		}

        public void PrintFormats(TimeSpan timeSpan)
        {
            Log("DateTimeSerializer.ToXsdTimeSpanString(timeSpan): " + DateTimeSerializer.ToXsdTimeSpanString(timeSpan));
            Log("\n");
        }

		[Test]
		public void PrintDate()
		{
			PrintFormats(DateTime.Now);
			PrintFormats(DateTime.UtcNow);
			PrintFormats(new DateTime(1979, 5, 9));
			PrintFormats(new DateTime(1979, 5, 9, 0, 0, 1));
			PrintFormats(new DateTime(1979, 5, 9, 0, 0, 0, 1));
			PrintFormats(new DateTime(2010, 10, 20, 10, 10, 10, 1));
			PrintFormats(new DateTime(2010, 11, 22, 11, 11, 11, 1));
		}

        [Test]
        public void PrintTimeSpan()
        {
            PrintFormats(new TimeSpan());
            PrintFormats(new TimeSpan(1));
            PrintFormats(new TimeSpan(1, 2, 3));
            PrintFormats(new TimeSpan(1, 2, 3, 4));
        }

		[Test]
		public void ToShortestXsdDateTimeString_works()
		{
			var shortDate = new DateTime(1979, 5, 9);
			const string shortDateString = "1979-05-09";

			var shortDateTime = new DateTime(1979, 5, 9, 0, 0, 1, DateTimeKind.Utc);
			var shortDateTimeString = shortDateTime.Equals(shortDateTime.ToStableUniversalTime())
              	? "1979-05-09T00:00:01Z"
              	: "1979-05-08T23:00:01Z";

			var longDateTime = new DateTime(1979, 5, 9, 0, 0, 0, 1, DateTimeKind.Utc);
			var longDateTimeString = longDateTime.Equals(longDateTime.ToStableUniversalTime())
         		? "1979-05-09T00:00:00.001Z"
         		: "1979-05-08T23:00:00.001Z";

			Assert.That(shortDateString, Is.EqualTo(DateTimeSerializer.ToShortestXsdDateTimeString(shortDate)));
			Assert.That(shortDateTimeString, Is.EqualTo(DateTimeSerializer.ToShortestXsdDateTimeString(shortDateTime)));
			Assert.That(longDateTimeString, Is.EqualTo(DateTimeSerializer.ToShortestXsdDateTimeString(longDateTime)));
		}

        [Test]
        public void CanDeserializeDateTimeOffsetWithTimeSpanIsZero()
        {
            var expectedValue = new DateTimeOffset(2012, 6, 27, 11, 26, 04, 524, TimeSpan.Zero);

            var s = DateTimeSerializer.ToWcfJsonDateTimeOffset(expectedValue);

            Assert.AreEqual("\\/Date(1340796364524)\\/", s);

            var afterValue = DateTimeSerializer.ParseWcfJsonDateOffset(s);

            Assert.AreEqual(expectedValue, afterValue);
        }

		[Test][Ignore]
		public void Utc_Local_Equals()
		{
			var now = DateTime.Now;
			var utcNow = now.ToStableUniversalTime();

			Assert.That(now.Ticks, Is.EqualTo(utcNow.Ticks), "Ticks are different");
			Assert.That(now, Is.EqualTo(utcNow), "DateTimes are different");
		}

		[Test]
		public void ParseShortestXsdDateTime_works()
		{
			DateTime shortDate = DateTimeSerializer.ParseShortestXsdDateTime("2011-8-4");
			Assert.That (shortDate, Is.EqualTo(new DateTime (2011, 8, 4)), "Month and day without leading 0");
			shortDate = DateTimeSerializer.ParseShortestXsdDateTime("2011-8-05");
			Assert.That (shortDate, Is.EqualTo(new DateTime (2011, 8, 5)), "Month without leading 0");
			shortDate = DateTimeSerializer.ParseShortestXsdDateTime("2011-09-4");
			Assert.That (shortDate, Is.EqualTo(new DateTime (2011, 9, 4)), "Day without leading 0");
		}

		[Test]
		public void TestSqlServerDateTime()
		{
			var result = TypeSerializer.DeserializeFromString<DateTime>("2010-06-01 21:52:59.280");
			Assert.That(result, Is.Not.Null);
		}

		[Test, Ignore("Don't pre-serialize into Utc")]
		public void UtcDateTime_Is_Deserialized_As_Kind_Utc()
		{
			//Serializing UTC
			var utcNow = new DateTime(2012, 1, 8, 12, 17, 1, 538, DateTimeKind.Utc);
			Assert.That(utcNow.Kind, Is.EqualTo(DateTimeKind.Utc));
			var serialized = JsonSerializer.SerializeToString(utcNow);
			//Deserializing UTC?
			var deserialized = JsonSerializer.DeserializeFromString<DateTime>(serialized);
			Assert.That(deserialized.Kind, Is.EqualTo(DateTimeKind.Utc)); //fails -> is DateTimeKind.Local
		}

        private static DateTime[] _dateTimeTests = new[] {
			DateTime.Now,
			DateTime.UtcNow,
			new DateTime(1979, 5, 9),
			new DateTime(1972,3,24),
			new DateTime(1979, 5, 9, 0, 0, 1),
			new DateTime(1979, 5, 9, 0, 0, 0, 1),
			new DateTime(2010, 10, 20, 10, 10, 10, 1),
			new DateTime(2010, 11, 22, 11, 11, 11, 1),
            new DateTime(622119282055250000)
        };

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
		[TestCase(7)]
		[TestCase(8)]
        public void AssertDateIsEqual(int whichDate)
		{
            DateTime dateTime = _dateTimeTests[whichDate];

			//Don't test short dates without time to UTC as you lose precision
			var shortDateStr = dateTime.ToString(DateTimeSerializer.ShortDateTimeFormat);
			var shortDateTimeStr = dateTime.ToStableUniversalTime().ToString(DateTimeSerializer.XsdDateTimeFormatSeconds);
			var longDateTimeStr = DateTimeSerializer.ToXsdDateTimeString(dateTime);
			var shortestDateStr = DateTimeSerializer.ToShortestXsdDateTimeString(dateTime);

			Log("{0} | {1} | {2}  [{3}]",
			    shortDateStr, shortDateTimeStr, longDateTimeStr, shortestDateStr);

			var shortDate = DateTimeSerializer.ParseShortestXsdDateTime(shortDateStr);
			var shortDateTime = DateTimeSerializer.ParseShortestXsdDateTime(shortDateTimeStr);
			var longDateTime = DateTimeSerializer.ParseShortestXsdDateTime(longDateTimeStr);

			Assert.That(shortDate, Is.EqualTo(dateTime.Date));

			var shortDateTimeUtc = shortDateTime.ToStableUniversalTime();
			Assert.That(shortDateTimeUtc, Is.EqualTo(
				new DateTime(
					shortDateTimeUtc.Year, shortDateTimeUtc.Month, shortDateTimeUtc.Day,
					shortDateTimeUtc.Hour, shortDateTimeUtc.Minute, shortDateTimeUtc.Second,
					shortDateTimeUtc.Millisecond, DateTimeKind.Utc)));

			Assert.That(longDateTime.ToStableUniversalTime(), Is.EqualTo(dateTime.ToStableUniversalTime()));

			var toDateTime = DateTimeSerializer.ParseShortestXsdDateTime(shortestDateStr);
			AssertDatesAreEqual(toDateTime, dateTime, "shortestDate");

			var unixTime = dateTime.ToUnixTimeMs();
			var fromUnixTime = DateTimeExtensions.FromUnixTimeMs(unixTime);
			AssertDatesAreEqual(fromUnixTime, dateTime, "unixTimeMs");

            var wcfDateString = DateTimeSerializer.ToWcfJsonDate(dateTime);
            var wcfDate = DateTimeSerializer.ParseWcfJsonDate(wcfDateString);
            AssertDatesAreEqual(wcfDate, dateTime, "wcf date");
		}

        private void AssertDatesAreEqual(DateTime toDateTime, DateTime dateTime, string which)
        {
			Assert.That(toDateTime.ToStableUniversalTime().RoundToMs(), Is.EqualTo(dateTime.ToStableUniversalTime().RoundToMs()), which);
        }
	}
}