using System;
using NUnit.Framework;
#if !MONOTOUCH
using ServiceStack.Client;
#endif

namespace ServiceStack.Text.Tests.JsonTests
{
	public class JsonDateTimeTests
	{
	    private string _localTimezoneOffset;

        [SetUp]
        public void SetUp()
        {
            _localTimezoneOffset = TimeZoneInfo.Local.BaseUtcOffset.Hours.ToString("00") + TimeZoneInfo.Local.BaseUtcOffset.Minutes.ToString("00");

        }

		#region TimestampOffset Tests
        [Test]
        public void When_using_TimestampOffset_and_serializing_as_Utc_It_should_deserialize_as_Utc()
        {
            JsConfig.DateHandler = JsonDateHandler.TimestampOffset;
            var initialDate = new DateTime(2012, 7, 25, 16, 17, 00, DateTimeKind.Utc);
            var json = JsonSerializer.SerializeToString(initialDate); //"2012-07-25T16:17:00.0000000Z"

            var deserializedDate = JsonSerializer.DeserializeFromString<DateTime>(json);

            Assert.AreEqual(DateTimeKind.Utc, deserializedDate.Kind);
            Assert.AreEqual(initialDate, deserializedDate);
        }

		[Test]
		public void Can_serialize_json_date_timestampOffset_utc()
		{
			JsConfig.DateHandler = JsonDateHandler.TimestampOffset;

			var dateTime = new DateTime(1994, 11, 24, 0, 0, 0, DateTimeKind.Utc);
			var ssJson = JsonSerializer.SerializeToString(dateTime);

			Assert.That(ssJson, Is.EqualTo(@"""\/Date(785635200000)\/"""));
			JsConfig.Reset();
		}

		[Test]
		public void Can_serialize_json_date_timestampOffset_local()
		{
			JsConfig.DateHandler = JsonDateHandler.TimestampOffset;

			var dateTime = new DateTime(1994, 11, 24, 0, 0, 0, DateTimeKind.Local);
			var ssJson = JsonSerializer.SerializeToString(dateTime);

			var offsetSpan = TimeZoneInfo.Local.GetUtcOffset(dateTime);
			var ticks = 785635200000 - offsetSpan.TotalMilliseconds;
			var offset = offsetSpan.ToTimeOffsetString();

			Assert.That(ssJson, Is.EqualTo(@"""\/Date(" + ticks + offset + @")\/"""));
			JsConfig.Reset();
		}

		[Test]
		public void Can_serialize_json_date_timestampOffset_unspecified()
		{
			JsConfig.DateHandler = JsonDateHandler.TimestampOffset;

			// Unspecified time is assumed to be local, so just make sure they serialize the same way.

			var dateTime1 = new DateTime(1994, 11, 24, 0, 0, 0, DateTimeKind.Unspecified);
			var ssJson1 = JsonSerializer.SerializeToString(dateTime1);

			var dateTime2 = new DateTime(1994, 11, 24, 0, 0, 0, DateTimeKind.Local);
			var ssJson2 = JsonSerializer.SerializeToString(dateTime2);

			Assert.That(ssJson1, Is.EqualTo(ssJson2));
			JsConfig.Reset();
		}

		[Test]
		public void Can_deserialize_json_date_timestampOffset_withoutOffset_asUtc()
		{
			JsConfig.DateHandler = JsonDateHandler.TimestampOffset;

			const string json = @"""\/Date(785635200000)\/""";
			var fromJson = JsonSerializer.DeserializeFromString<DateTime>(json);

			var dateTime = new DateTime(1994, 11, 24, 0, 0, 0, DateTimeKind.Utc);
			Assert.That(fromJson, Is.EqualTo(dateTime));
			Assert.That(fromJson.Kind, Is.EqualTo(dateTime.Kind));
			JsConfig.Reset();
		}

		[Test]
		public void Can_deserialize_json_date_timestampOffset_withOffset_asUnspecified()
		{
			JsConfig.DateHandler = JsonDateHandler.TimestampOffset;

			const string json = @"""\/Date(785660400000-0700)\/""";
			var fromJson = JsonSerializer.DeserializeFromString<DateTime>(json);

			var dateTime = new DateTime(1994, 11, 24, 0, 0, 0, DateTimeKind.Unspecified);
			Assert.That(fromJson, Is.EqualTo(dateTime));
			Assert.That(fromJson.Kind, Is.EqualTo(dateTime.Kind));
			JsConfig.Reset();
		}

		[Test]
		public void Can_deserialize_json_date_timestampOffset_withZeroOffset_asUnspecified()
		{
			JsConfig.DateHandler = JsonDateHandler.TimestampOffset;

			const string json = @"""\/Date(785635200000+0000)\/""";
			var fromJson = JsonSerializer.DeserializeFromString<DateTime>(json);

			var dateTime = new DateTime(1994, 11, 24, 0, 0, 0, DateTimeKind.Unspecified);
			Assert.That(fromJson, Is.EqualTo(dateTime));
			Assert.That(fromJson.Kind, Is.EqualTo(dateTime.Kind));
			JsConfig.Reset();
		}

		#endregion

        #region TimeSpan Tests
        [Test]
        public void JsonSerializerReturnsTimeSpanAsString()
        {
            Assert.AreEqual("\"PT0S\"", JsonSerializer.SerializeToString(new TimeSpan()));
            Assert.AreEqual("\"PT0.0000001S\"", JsonSerializer.SerializeToString(new TimeSpan(1)));
        }

        [Test]
        public void JsonDeserializerReturnsTimeSpanFromString()
        {
            Assert.AreEqual(TimeSpan.Zero, JsonSerializer.DeserializeFromString<TimeSpan>("\"PT0S\""));
            Assert.AreEqual(new TimeSpan(1), JsonSerializer.DeserializeFromString<TimeSpan>("\"PT0.0000001S\""));
        }
        #endregion

        #region DCJS Compatibility Tests
        [Test]
		public void Can_serialize_json_date_dcjsCompatible_utc()
		{
			JsConfig.DateHandler = JsonDateHandler.DCJSCompatible;

			var dateTime = new DateTime(1994, 11, 24, 0, 0, 0, DateTimeKind.Utc);
			var ssJson = JsonSerializer.SerializeToString(dateTime);
            var bclJson = @"""\/Date(785635200000)\/"""; //BclJsonDataContractSerializer.Instance.Parse(dateTime);

			Assert.That(ssJson, Is.EqualTo(bclJson));
			JsConfig.Reset();
		}

#if !__MonoCS__
		[Test]
		public void Can_serialize_json_date_dcjsCompatible_local()
		{
			JsConfig.DateHandler = JsonDateHandler.DCJSCompatible;

			var dateTime = new DateTime(1994, 11, 24, 0, 0, 0, DateTimeKind.Local);
			var ssJson = JsonSerializer.SerializeToString(dateTime);
            var bclJson = BclJsonDataContractSerializer.Instance.Parse(dateTime);

			Assert.That(ssJson, Is.EqualTo(bclJson));
			JsConfig.Reset();
		}

		[Test]
		public void Can_serialize_json_date_dcjsCompatible_unspecified()
		{
			JsConfig.DateHandler = JsonDateHandler.DCJSCompatible;

			var dateTime = new DateTime(1994, 11, 24, 0, 0, 0, DateTimeKind.Unspecified);
			var ssJson = JsonSerializer.SerializeToString(dateTime);
            var bclJson = BclJsonDataContractSerializer.Instance.Parse(dateTime);

            Assert.That(ssJson, Is.EqualTo(bclJson));
			JsConfig.Reset();
		}
#endif

#if !MONOTOUCH
		[Test]
		public void Can_deserialize_json_date_dcjsCompatible_utc()
		{
			JsConfig.DateHandler = JsonDateHandler.DCJSCompatible;

			var dateTime = new DateTime(1994, 11, 24, 0, 0, 0, DateTimeKind.Utc);
			var ssJson = JsonSerializer.SerializeToString(dateTime);
			var fromJson = JsonSerializer.DeserializeFromString<DateTime>(ssJson);
			var fromBclJson = BclJsonDataContractDeserializer.Instance.Parse<DateTime>(ssJson);

			Assert.That(fromJson, Is.EqualTo(fromBclJson));
            Assert.That(fromJson.Kind, Is.EqualTo(DateTimeKind.Utc)); // fromBclJson.Kind
			JsConfig.Reset();
		}

		[Test]
		public void Can_deserialize_json_date_dcjsCompatible_local()
		{
			JsConfig.DateHandler = JsonDateHandler.DCJSCompatible;

			var dateTime = new DateTime(1994, 11, 24, 0, 0, 0, DateTimeKind.Local);
			var ssJson = JsonSerializer.SerializeToString(dateTime);
			var fromJson = JsonSerializer.DeserializeFromString<DateTime>(ssJson);
			var fromBclJson = BclJsonDataContractDeserializer.Instance.Parse<DateTime>(ssJson);

			Assert.That(fromJson, Is.EqualTo(fromBclJson));
            Assert.That(fromJson.Kind, Is.EqualTo(DateTimeKind.Local)); // fromBclJson.Kind
			JsConfig.Reset();
		}

		[Test]
		public void Can_deserialize_json_date_dcjsCompatible_unspecified()
		{
			JsConfig.DateHandler = JsonDateHandler.DCJSCompatible;

			var dateTime = new DateTime(1994, 11, 24, 0, 0, 0, DateTimeKind.Unspecified);
			var ssJson = JsonSerializer.SerializeToString(dateTime);
			var fromJson = JsonSerializer.DeserializeFromString<DateTime>(ssJson);
			var fromBclJson = BclJsonDataContractDeserializer.Instance.Parse<DateTime>(ssJson);

			Assert.That(fromJson, Is.EqualTo(fromBclJson));
            Assert.That(fromJson.Kind, Is.EqualTo(DateTimeKind.Local)); // fromBclJson.Kind
			JsConfig.Reset();
		}
#endif
		#endregion

		#region ISO-8601 Tests
        [Test]
        public void When_using_ISO8601_and_serializing_as_Utc_It_should_deserialize_as_Utc()
        {
            JsConfig.DateHandler = JsonDateHandler.ISO8601;
            var initialDate = new DateTime(2012, 7, 25, 16, 17, 00, DateTimeKind.Utc);
            var json = JsonSerializer.SerializeToString(initialDate); //"2012-07-25T16:17:00.0000000Z"

            var deserializedDate = JsonSerializer.DeserializeFromString<DateTime>(json);

            Assert.AreEqual(DateTimeKind.Utc, deserializedDate.Kind);
            Assert.AreEqual(initialDate, deserializedDate);
        }

		[Test]
		public void Can_serialize_json_date_iso8601_utc()
		{
			JsConfig.DateHandler = JsonDateHandler.ISO8601;

			var dateTime = new DateTime(1994, 11, 24, 12, 34, 56, DateTimeKind.Utc);
			var ssJson = JsonSerializer.SerializeToString(dateTime);

			Assert.That(ssJson, Is.EqualTo(@"""1994-11-24T12:34:56.0000000Z"""));
			JsConfig.Reset();
		}

		[Test]
		public void Can_serialize_json_date_iso8601_local()
		{
			JsConfig.DateHandler = JsonDateHandler.ISO8601;

			var dateTime = new DateTime(1994, 11, 24, 12, 34, 56, DateTimeKind.Local);
			var ssJson = JsonSerializer.SerializeToString(dateTime);

			var offsetSpan = TimeZoneInfo.Local.GetUtcOffset(dateTime);
			var offset = offsetSpan.ToTimeOffsetString(true);

			Assert.That(ssJson, Is.EqualTo(@"""1994-11-24T12:34:56.0000000" + offset + @""""));
			JsConfig.Reset();
		}

		[Test]
		public void Can_serialize_json_date_iso8601_unspecified()
		{
			JsConfig.DateHandler = JsonDateHandler.ISO8601;

			var dateTime = new DateTime(1994, 11, 24, 12, 34, 56, DateTimeKind.Unspecified);
			var ssJson = JsonSerializer.SerializeToString(dateTime);

			Assert.That(ssJson, Is.EqualTo(@"""1994-11-24T12:34:56.0000000"""));
			JsConfig.Reset();
		}

		[Test]
		public void Can_deserialize_json_date_iso8601_withZOffset_asUtc()
		{
			JsConfig.DateHandler = JsonDateHandler.ISO8601;

			const string json = @"""1994-11-24T12:34:56Z""";
			var fromJson = JsonSerializer.DeserializeFromString<DateTime>(json);

			var dateTime = new DateTime(1994, 11, 24, 12, 34, 56, DateTimeKind.Utc);
			Assert.That(fromJson, Is.EqualTo(dateTime));
			Assert.That(fromJson.Kind, Is.EqualTo(dateTime.Kind));
			JsConfig.Reset();
		}

		[Test]
		public void Can_deserialize_json_date_iso8601_withoutOffset_asUnspecified()
		{
			JsConfig.DateHandler = JsonDateHandler.ISO8601;

			const string json = @"""1994-11-24T12:34:56""";
			var fromJson = JsonSerializer.DeserializeFromString<DateTime>(json);

			var dateTime = new DateTime(1994, 11, 24, 12, 34, 56, DateTimeKind.Unspecified);
			Assert.That(fromJson, Is.EqualTo(dateTime));
			Assert.That(fromJson.Kind, Is.EqualTo(dateTime.Kind));
			JsConfig.Reset();
		}

		[Test]
		public void Can_deserialize_json_date_iso8601_withOffset_asLocal()
		{
			JsConfig.DateHandler = JsonDateHandler.ISO8601;

			var dateTime = new DateTime(1994, 11, 24, 12, 34, 56, DateTimeKind.Local);
			var offset = TimeZoneInfo.Local.GetUtcOffset(dateTime).ToTimeOffsetString(true);

			var json = @"""1994-11-24T12:34:56" + offset + @"""";
			var fromJson = JsonSerializer.DeserializeFromString<DateTime>(json);


			Assert.That(fromJson, Is.EqualTo(dateTime));
			Assert.That(fromJson.Kind, Is.EqualTo(dateTime.Kind));
			JsConfig.Reset();
		}

		#endregion

		#region ISO-8601 TimeStampOffset Tests
		[Test]
		public void Can_serialize_json_datetimeoffset_iso8601_utc()
		{
			JsConfig.DateHandler = JsonDateHandler.ISO8601;

			var dateTimeOffset = new DateTimeOffset(1994, 11, 24, 12, 34, 56, TimeSpan.Zero);
			var ssJson = JsonSerializer.SerializeToString(dateTimeOffset);

			Assert.That(ssJson, Is.EqualTo(@"""1994-11-24T12:34:56.0000000+00:00"""));
			JsConfig.Reset();
		}

		[Test]
		public void Can_serialize_json_datetimeoffset_iso8601_specified()
		{
			JsConfig.DateHandler = JsonDateHandler.ISO8601;

			var dateTimeOffset = new DateTimeOffset(1994, 11, 24, 12, 34, 56, TimeSpan.FromHours(-7));
			var ssJson = JsonSerializer.SerializeToString(dateTimeOffset);

			Assert.That(ssJson, Is.EqualTo(@"""1994-11-24T12:34:56.0000000-07:00"""));
			JsConfig.Reset();
		}

		[Test]
		public void Can_deserialize_json_datetimeoffset_iso8601_withZOffset_asUtc()
		{
			JsConfig.DateHandler = JsonDateHandler.ISO8601;

			const string json = @"""1994-11-24T12:34:56Z""";
			var fromJson = JsonSerializer.DeserializeFromString<DateTimeOffset>(json);

			var dateTimeOffset = new DateTimeOffset(1994, 11, 24, 12, 34, 56, TimeSpan.Zero);
			Assert.That(fromJson, Is.EqualTo(dateTimeOffset));
			JsConfig.Reset();
		}

		[Test]
		public void Can_deserialize_json_datetimeoffset_iso8601_withoutOffset_asUtc()
		{
			JsConfig.DateHandler = JsonDateHandler.ISO8601;

			const string json = @"""1994-11-24T12:34:56""";
			var fromJson = JsonSerializer.DeserializeFromString<DateTimeOffset>(json);

			var dateTimeOffset = new DateTimeOffset(1994, 11, 24, 12, 34, 56, TimeSpan.Zero);
			Assert.That(fromJson, Is.EqualTo(dateTimeOffset));
			JsConfig.Reset();
		}

		[Test]
		public void Can_deserialize_json_datetimeoffset_iso8601_withOffset_asSpecified()
		{
			JsConfig.DateHandler = JsonDateHandler.ISO8601;

			var dateTimeOffset = new DateTimeOffset(1994, 11, 24, 12, 34, 56, TimeSpan.FromHours(-7));

			const string json = @"""1994-11-24T12:34:56-07:00""";
			var fromJson = JsonSerializer.DeserializeFromString<DateTimeOffset>(json);

			Assert.That(fromJson, Is.EqualTo(dateTimeOffset));
			JsConfig.Reset();
		}
		#endregion

        #region InteropTests

        [Test]
        public void Can_serialize_TimestampOffset_deserialize_ISO8601()
        {
            var dateTimeOffset = new DateTimeOffset(1997, 11, 24, 12, 34, 56, TimeSpan.FromHours(-10));

            JsConfig.DateHandler = JsonDateHandler.TimestampOffset;
            var json = ServiceStack.Text.Common.DateTimeSerializer.ToWcfJsonDateTimeOffset(dateTimeOffset);

            JsConfig.DateHandler = JsonDateHandler.ISO8601;
            var fromJson = ServiceStack.Text.Common.DateTimeSerializer.ParseDateTimeOffset(json);

            Assert.That(fromJson, Is.EqualTo(dateTimeOffset));
            JsConfig.Reset();
        }

        [Test]
        public void Can_serialize_ISO8601_deserialize_DCJSCompatible()
        {
            var dateTimeOffset = new DateTimeOffset(1994, 11, 24, 12, 34, 56, TimeSpan.FromHours(-10));

            JsConfig.DateHandler = JsonDateHandler.ISO8601;
            var json = ServiceStack.Text.Common.DateTimeSerializer.ToWcfJsonDateTimeOffset(dateTimeOffset);

            JsConfig.DateHandler = JsonDateHandler.DCJSCompatible;
            var fromJson = ServiceStack.Text.Common.DateTimeSerializer.ParseDateTimeOffset(json);

            // NOTE: DJCS goes to local, so botches offset
            Assert.That(fromJson, Is.EqualTo(dateTimeOffset));
            JsConfig.Reset();
        }

        [Test]
        public void Can_deserialize_null()
        {
            const string json = (string)null;
            var expected = default(DateTimeOffset);
            var fromJson = ServiceStack.Text.Common.DateTimeSerializer.ParseDateTimeOffset(json);
            Assert.That(fromJson, Is.EqualTo(expected));
        }

        #endregion
    }
}
