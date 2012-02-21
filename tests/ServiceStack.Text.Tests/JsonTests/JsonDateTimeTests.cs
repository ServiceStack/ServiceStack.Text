using System;
using NUnit.Framework;
using ServiceStack.Client;

namespace ServiceStack.Text.Tests.JsonTests
{
	public class JsonDateTimeTests
	{
		#region TimestampOffset Tests
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

		#region DCJS Compatibility Tests
		[Test]
		public void Can_serialize_json_date_dcjsCompatible_utc()
		{
			JsConfig.DateHandler = JsonDateHandler.DCJSCompatible;

			var dateTime = new DateTime(1994, 11, 24, 0, 0, 0, DateTimeKind.Utc);
			var ssJson = JsonSerializer.SerializeToString(dateTime);
			var bclJson = BclJsonDataContractSerializer.Instance.Parse(dateTime);

			Assert.That(ssJson, Is.EqualTo(bclJson));
			JsConfig.Reset();
		}

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

		[Test]
		public void Can_deserialize_json_date_dcjsCompatible_utc()
		{
			JsConfig.DateHandler = JsonDateHandler.DCJSCompatible;

			var dateTime = new DateTime(1994, 11, 24, 0, 0, 0, DateTimeKind.Utc);
			var ssJson = JsonSerializer.SerializeToString(dateTime);
			var fromJson = JsonSerializer.DeserializeFromString<DateTime>(ssJson);
			var fromBclJson = BclJsonDataContractDeserializer.Instance.Parse<DateTime>(ssJson);

			Assert.That(fromJson, Is.EqualTo(fromBclJson));
			Assert.That(fromJson.Kind, Is.EqualTo(fromBclJson.Kind));
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
			Assert.That(fromJson.Kind, Is.EqualTo(fromBclJson.Kind));
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
			Assert.That(fromJson.Kind, Is.EqualTo(fromBclJson.Kind));
			JsConfig.Reset();
		}
		#endregion

		#region ISO-8601 Tests
		[Test]
		public void Can_serialize_json_date_iso8601_utc()
		{
			JsConfig.DateHandler = JsonDateHandler.ISO8601;

			var dateTime = new DateTime(1994, 11, 24, 12, 34, 56, DateTimeKind.Utc);
			var ssJson = JsonSerializer.SerializeToString(dateTime);

			Assert.That(ssJson, Is.EqualTo(@"""\/Date(1994-11-24T12:34:56.0000000Z)\/"""));
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

			Assert.That(ssJson, Is.EqualTo(@"""\/Date(1994-11-24T12:34:56.0000000" + offset + @")\/"""));
			JsConfig.Reset();
		}

		[Test]
		public void Can_serialize_json_date_iso8601_unspecified()
		{
			JsConfig.DateHandler = JsonDateHandler.ISO8601;

			var dateTime = new DateTime(1994, 11, 24, 12, 34, 56, DateTimeKind.Unspecified);
			var ssJson = JsonSerializer.SerializeToString(dateTime);

			Assert.That(ssJson, Is.EqualTo(@"""\/Date(1994-11-24T12:34:56.0000000)\/"""));
			JsConfig.Reset();
		}

		[Test]
		public void Can_deserialize_json_date_iso8601_withZOffset_asUtc()
		{
			JsConfig.DateHandler = JsonDateHandler.ISO8601;

			const string json = @"""\/Date(1994-11-24T12:34:56Z)\/""";
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

			const string json = @"""\/Date(1994-11-24T12:34:56)\/""";
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

			var json = @"""\/Date(1994-11-24T12:34:56" + offset + @")\/""";
			var fromJson = JsonSerializer.DeserializeFromString<DateTime>(json);


			Assert.That(fromJson, Is.EqualTo(dateTime));
			Assert.That(fromJson.Kind, Is.EqualTo(dateTime.Kind));
			JsConfig.Reset();
		}

		#endregion
	}
}
