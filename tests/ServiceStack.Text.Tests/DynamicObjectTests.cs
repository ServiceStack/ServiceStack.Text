using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;

namespace ServiceStack.Text.Tests
{
	[TestFixture]
	public class DynamicObjectTests
		: TestBase
	{
		public class UrlStatus
		{
			public int Status { get; set; }
			public string Url { get; set; }
		}

        [TearDown]
        public void TearDown()
        {
            JsConfig.Reset();
        }

		[Test]
		public void Dictionary_Object_UrlStatus()
		{
			var urlStatus = new UrlStatus {
				Status = 301,
				Url = "http://www.ehow.com/how_5615409_create-pdfs-using-bean.html",
			};

			var map = new Dictionary<string, object>
          	{
          		{"Status","OK"},
          		{"Url","http://www.ehow.com/m/how_5615409_create-pdfs-using-bean.html"},
          		{"Parent Url","http://www.ehow.com/mobilearticle35.xml"},
          		{"Redirect Chai", urlStatus},
          	};

			var json = JsonSerializer.SerializeToString(map);
			var fromJson = JsonSerializer.DeserializeFromString<Dictionary<string, object>>(json);

			Assert.That(fromJson["Status"], Is.EqualTo(map["Status"]));
			Assert.That(fromJson["Url"], Is.EqualTo(map["Url"]));
			Assert.That(fromJson["Parent Url"], Is.EqualTo(map["Parent Url"]));

			var actualStatus = (UrlStatus)fromJson["Redirect Chai"];
			Assert.That(actualStatus.Status, Is.EqualTo(urlStatus.Status));
			Assert.That(actualStatus.Url, Is.EqualTo(urlStatus.Url));

			Console.WriteLine("JSON: " + json);
		}

		public class PocoWithKvp
		{
			public KeyValuePair<string, string>[] Values { get; set; }
		}

		[Test]
		public void Can_Serailize_KVP_array()
		{
			var kvpArray = new[] {
				new KeyValuePair<string, string>("Key", "Foo"),
				new KeyValuePair<string, string>("Value", "Bar"),
			};
			var dto = new PocoWithKvp {
				Values = kvpArray
			};

			Console.WriteLine(dto.ToJson());

			Serialize(dto, includeXml: false);
		}

        [Test]
        public void Can_deserialize_object_string()
        {
            JsConfig.TryToParsePrimitiveTypeValues = true;
            JsConfig.ConvertObjectTypesIntoStringDictionary = true;

            var json = "12345";
            var deserialized = JsonSerializer.DeserializeFromString<object>(json);
            Assert.That(deserialized, Is.EqualTo(json));
        }

        [Test]
        public void Can_deserialize_object_array()
        {
            JsConfig.TryToParsePrimitiveTypeValues = true;
            JsConfig.ConvertObjectTypesIntoStringDictionary = true;

            var json = "[1,2,3]";
            var deserialized = JsonSerializer.DeserializeFromString<object>(json);
            Assert.That(deserialized, Is.InstanceOf<List<object>>());
            Assert.That(((List<object>)deserialized)[0], Is.EqualTo(1));
            Assert.That(((List<object>)deserialized)[1], Is.EqualTo(2));
            Assert.That(((List<object>)deserialized)[2], Is.EqualTo(3));
        }

        [Test]
        public void Can_deserialize_object_epoch_datetime()
        {
            JsConfig.TryToParsePrimitiveTypeValues = true;
            JsConfig.ConvertObjectTypesIntoStringDictionary = true;

            var json = "{\"foo\":\"\\/Date(1353438089156)\\/\"}";
            var deserialized = JsonSerializer.DeserializeFromString<object>(json);
            Assert.That(deserialized, Is.InstanceOf<Dictionary<string, object>>());
            Assert.That(((Dictionary<string, object>)deserialized)["foo"], Is.InstanceOf<DateTime>());
        }

        [Test]
        public void Can_deserialize_object_iso8601_datetime()
        {
            JsConfig.DateHandler = JsonDateHandler.ISO8601;
            JsConfig.TryToParsePrimitiveTypeValues = true;
            JsConfig.ConvertObjectTypesIntoStringDictionary = true;

            var json = "{\"foo\":\"2012-11-20T21:37:32.0876543Z\"}";
            var deserialized = JsonSerializer.DeserializeFromString<object>(json);
            Assert.That(deserialized, Is.InstanceOf<Dictionary<string, object>>());
            Assert.That(((Dictionary<string, object>)deserialized)["foo"], Is.InstanceOf<DateTime>());
        }

        [Test]
        public void Can_deserialize_object_dictionary()
        {
            JsConfig.TryToParsePrimitiveTypeValues = true;
            JsConfig.ConvertObjectTypesIntoStringDictionary = true;

            var json = "{\"foo\":\"bar\"}";
            var deserialized = JsonSerializer.DeserializeFromString<object>(json);
            Assert.That(deserialized, Is.InstanceOf<Dictionary<string, object>>());
            Assert.That(((Dictionary<string, object>)deserialized)["foo"], Is.EqualTo("bar"));
        }

        [Test, SetCulture("nl-NL")]
        public void Can_deserialize_object_dictionary_when_current_culture_has_decimal_comma()
        {
            JsConfig.TryToParsePrimitiveTypeValues = true;
            JsConfig.ConvertObjectTypesIntoStringDictionary = true;

            var json = "{\"decimalValue\": 79228162514264337593543950335,\"floatValue\": 3.40282347E+038,\"doubleValue\": 1.79769313486231570000E+308}";
            var deserialized = JsonSerializer.DeserializeFromString<object>(json);
            Assert.That(deserialized, Is.InstanceOf<Dictionary<string, object>>());
            var dict = (Dictionary<string, object>)deserialized;
            Assert.That(dict["decimalValue"], Is.InstanceOf<decimal>() & Is.EqualTo(decimal.MaxValue), "decimal");
            Assert.That(dict["floatValue"], Is.InstanceOf<float>() & Is.EqualTo(float.MaxValue), "float");
            Assert.That(dict["doubleValue"], Is.InstanceOf<double>() & Is.EqualTo(double.MaxValue), "double");
        }

		[Test]
		public void Can_deserialize_object_dictionary_with_mixed_values_and_nulls_and_empty_array()
		{
            JsConfig.TryToParsePrimitiveTypeValues = true;
            JsConfig.ConvertObjectTypesIntoStringDictionary = true;

		    var json = "{\"stringIntValue\": \"-13\",\"intValue\": -13,\"nullValue\": null,\"stringDecimalValue\": \"5.9\",\"decimalValue\": 5.9,\"emptyArrayValue\": [],\"stringValue\": \"Foo\",\"stringWithDigitsValue\": \"OR345\",\"dateValue\":\"\\/Date(785635200000)\\/\"}";
            var deserialized = JsonSerializer.DeserializeFromString<object>(json);
            Assert.That(deserialized, Is.InstanceOf<Dictionary<string, object>>());
		    var dict = (Dictionary<string, object>) deserialized;
            Assert.That(dict["stringIntValue"], Is.EqualTo("-13"));
            Assert.That(dict["intValue"], Is.EqualTo(-13));
            Assert.That(dict["intValue"], Is.Not.EqualTo(dict["stringIntValue"]));
            Assert.That(dict["nullValue"], Is.Null);
            Assert.That(dict["stringDecimalValue"], Is.EqualTo("5.9"));
            Assert.That(dict["decimalValue"], Is.EqualTo(5.9m));
            Assert.That(dict["decimalValue"], Is.Not.EqualTo(dict["stringDecimalValue"]));
            Assert.That(dict["emptyArrayValue"], Is.Not.Null);
            Assert.That(dict["stringValue"], Is.EqualTo("Foo"));
            Assert.That(dict["stringWithDigitsValue"], Is.EqualTo("OR345"));
            Assert.That(dict["dateValue"], Is.EqualTo(new DateTime(1994, 11, 24, 0, 0, 0, DateTimeKind.Utc)));
		}

        [Test]
		public void Can_deserialize_object_dictionary_with_line_breaks()
		{
            JsConfig.TryToParsePrimitiveTypeValues = true;
            JsConfig.ConvertObjectTypesIntoStringDictionary = true;

            var json = @"{
                    ""value""
:
   5   ,

                }";

            var deserialized = JsonSerializer.DeserializeFromString<object>(json);
            Assert.That(deserialized, Is.InstanceOf<Dictionary<string, object>>());
		    var dict = (Dictionary<string, object>) deserialized;
            Assert.That(dict["value"], Is.EqualTo(5));
		}

        [Test]
		public void Can_deserialize_object_array_with_line_breaks_before_first_element()
		{
            JsConfig.TryToParsePrimitiveTypeValues = true;
            JsConfig.ConvertObjectTypesIntoStringDictionary = true;

            var json = @"[
                {
                    ""name"":""foo""
                }]";

            var deserialized = JsonSerializer.DeserializeFromString<object>(json);
            Assert.That(deserialized, Is.InstanceOf<List<object>>());
            var arrayValues = (List<object>) deserialized;
            Assert.That(arrayValues.Count, Is.EqualTo(1));
            Assert.That(arrayValues[0], Is.Not.Null);
		}

        [Test]
		public void Can_deserialize_object_array_with_line_breaks_after_last_element()
		{
            JsConfig.TryToParsePrimitiveTypeValues = true;
            JsConfig.ConvertObjectTypesIntoStringDictionary = true;

            var json = @"[{
                    ""name"":""foo""
                }
                ]";

            var deserialized = JsonSerializer.DeserializeFromString<object>(json);
            Assert.That(deserialized, Is.InstanceOf<List<object>>());
            var arrayValues = (List<object>) deserialized;
            Assert.That(arrayValues.Count, Is.EqualTo(1));
            Assert.That(arrayValues[0], Is.Not.Null);
		}

        [Test]
		public void Can_deserialize_object_array_with_line_breaks_around_element()
		{
            JsConfig.TryToParsePrimitiveTypeValues = true;
            JsConfig.ConvertObjectTypesIntoStringDictionary = true;

            var json = @"[
                {
                    ""name"":""foo""
                }
                ]";

            var deserialized = JsonSerializer.DeserializeFromString<object>(json);
            Assert.That(deserialized, Is.InstanceOf<List<object>>());
            var arrayValues = (List<object>) deserialized;
            Assert.That(arrayValues.Count, Is.EqualTo(1));
            Assert.That(arrayValues[0], Is.Not.Null);
		}

        [Test]
		public void Can_deserialize_object_array_with_line_breaks_around_number_element()
		{
            JsConfig.TryToParsePrimitiveTypeValues = true;
            JsConfig.ConvertObjectTypesIntoStringDictionary = true;

            var json = @"[
                
                    5
                
                ]";

            var deserialized = JsonSerializer.DeserializeFromString<object>(json);
            Assert.That(deserialized, Is.InstanceOf<List<object>>());
            var arrayValues = (List<object>) deserialized;
            Assert.That(arrayValues.Count, Is.EqualTo(1));
            Assert.That(arrayValues[0], Is.EqualTo(5));
		}
	}
}