using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using NUnit.Framework;
using ServiceStack.Text.Json;


#if !MONOTOUCH
using ServiceStack.Common.Tests.Models;
#endif

namespace ServiceStack.Text.Tests.JsonTests
{
	[TestFixture]
	public class BasicJsonTests
		: TestBase
	{
		public class JsonPrimitives
		{
			public int Int { get; set; }
			public long Long { get; set; }
			public float Float { get; set; }
			public double Double { get; set; }
			public bool Boolean { get; set; }
			public DateTime DateTime { get; set; }
			public string NullString { get; set; }

			public static JsonPrimitives Create(int i)
			{
				return new JsonPrimitives
				{
					Int = i,
					Long = i,
					Float = i,
					Double = i,
					Boolean = i % 2 == 0,
					DateTime = DateTimeExtensions.FromUnixTimeMs(1),
				};
			}
		}

        public class NullableValueTypes
        {
            public int? Int { get; set; }
            public long? Long { get; set; }
            public decimal? Decimal { get; set; }
            public double? Double { get; set; }
            public bool? Boolean { get; set; }
            public DateTime? DateTime { get; set; }
        }

		[SetUp]
		public void Setup ()
		{
#if MONOTOUCH
			JsConfig.Reset();
			JsConfig.RegisterTypeForAot<ExampleEnumWithoutFlagsAttribute>();
			JsConfig.RegisterTypeForAot<ExampleEnum>();
#endif
		}

        [TearDown]
        public void TearDown()
        {
            JsConfig.Reset();
        }

	    [Test]
	    public void Can_parse_json_with_nullable_valuetypes()
	    {
	        var json = "{}";

            var item = JsonSerializer.DeserializeFromString<NullableValueTypes>(json);

            Assert.That(item.Int, Is.Null, "int");
            Assert.That(item.Long, Is.Null, "long");
            Assert.That(item.Decimal, Is.Null, "decimal");
            Assert.That(item.Double, Is.Null, "double");
            Assert.That(item.Boolean, Is.Null, "boolean");
            Assert.That(item.DateTime, Is.Null, "datetime");
	    }

        [Test]
        public void Can_parse_json_with_nullable_valuetypes_that_has_included_null_values()
        {
            var json = "{\"Int\":null,\"Long\":null,\"Decimal\":null,\"Double\":null,\"Boolean\":null,\"DateTime\":null}";

            var item = JsonSerializer.DeserializeFromString<NullableValueTypes>(json);

            Assert.That(item.Int, Is.Null, "int");
            Assert.That(item.Long, Is.Null, "long");
            Assert.That(item.Decimal, Is.Null, "decimal");
            Assert.That(item.Double, Is.Null, "double");
            Assert.That(item.Boolean, Is.Null, "boolean");
            Assert.That(item.DateTime, Is.Null, "datetime");
        }

		[Test]
		public void Can_parse_json_with_nulls_or_empty_string_in_nullables()
		{
			const string json = "{\"Int\":null,\"Boolean\":\"\"}";
			var value = JsonSerializer.DeserializeFromString<NullableValueTypes>(json);

			Assert.That(value.Int, Is.EqualTo(null));
			Assert.That(value.Boolean, Is.EqualTo(null));
		}

        [Test]
        public void Can_parse_json_with_nullable_valuetypes_that_has_no_value_specified()
        {
            var json = "{\"Int\":,\"Long\":,\"Decimal\":,\"Double\":,\"Boolean\":,\"DateTime\":}";

            var item = JsonSerializer.DeserializeFromString<NullableValueTypes>(json);

            Assert.That(item.Int, Is.Null, "int");
            Assert.That(item.Long, Is.Null, "long");
            Assert.That(item.Decimal, Is.Null, "decimal");
            Assert.That(item.Double, Is.Null, "double");
            Assert.That(item.Boolean, Is.Null, "boolean");
            Assert.That(item.DateTime, Is.Null, "datetime");
        }

		[Test]
		public void Can_handle_json_primitives()
		{
			var json = JsonSerializer.SerializeToString(JsonPrimitives.Create(1));
			Log(json);

			Assert.That(json, Is.EqualTo(
				"{\"Int\":1,\"Long\":1,\"Float\":1,\"Double\":1,\"Boolean\":false,\"DateTime\":\"\\/Date(1)\\/\"}"));
		}

		[Test]
		public void Can_parse_json_with_nulls()
		{
			const string json = "{\"Int\":1,\"NullString\":null}";
			var value = JsonSerializer.DeserializeFromString<JsonPrimitives>(json);

			Assert.That(value.Int, Is.EqualTo(1));
			Assert.That(value.NullString, Is.Null);
		}

		[Test]
		public void Can_serialize_dictionary_of_int_int()
		{
			var json = JsonSerializer.SerializeToString<IntIntDictionary>(new IntIntDictionary() { Dictionary = { { 10, 100 }, { 20, 200 } } });
			const string expected = "{\"Dictionary\":{\"10\":100,\"20\":200}}";
			Assert.That(json, Is.EqualTo(expected));
		}

		private class IntIntDictionary
		{
			public IntIntDictionary()
			{
				Dictionary = new Dictionary<int, int>();
			}
			public IDictionary<int, int> Dictionary { get; set; }
		}

		[Test]
		public void Serialize_skips_null_values_by_default()
		{
			var o = new NullValueTester
			{
				Name = "Brandon",
				Type = "Programmer",
				SampleKey = 12,
				Nothing = (string)null,
				NullableDateTime = null
			};

			var s = JsonSerializer.SerializeToString(o);
			Assert.That(s, Is.EqualTo("{\"Name\":\"Brandon\",\"Type\":\"Programmer\",\"SampleKey\":12}"));
		}

		[Test]
		public void Serialize_can_include_null_values()
		{
			var o = new NullValueTester
			{
				Name = "Brandon",
				Type = "Programmer",
				SampleKey = 12,
				Nothing = null,
				NullClass = null,
				NullableDateTime = null,
			};

			JsConfig.IncludeNullValues = true;
			var s = JsonSerializer.SerializeToString(o);
			JsConfig.Reset();
			Assert.That(s, Is.EqualTo("{\"Name\":\"Brandon\",\"Type\":\"Programmer\",\"SampleKey\":12,\"Nothing\":null,\"NullClass\":null,\"NullableDateTime\":null}"));
		}

		private class NullClass
		{

		}

		[Test]
		public void Deserialize_sets_null_values()
		{
			var s = "{\"Name\":\"Brandon\",\"Type\":\"Programmer\",\"SampleKey\":12,\"Nothing\":null}";
			var o = JsonSerializer.DeserializeFromString<NullValueTester>(s);
			Assert.That(o.Name, Is.EqualTo("Brandon"));
			Assert.That(o.Type, Is.EqualTo("Programmer"));
			Assert.That(o.SampleKey, Is.EqualTo(12));
			Assert.That(o.Nothing, Is.Null);
		}

		[Test]
		public void Deserialize_ignores_omitted_values()
		{
			var s = "{\"Type\":\"Programmer\",\"SampleKey\":2}";
			var o = JsonSerializer.DeserializeFromString<NullValueTester>(s);
			Assert.That(o.Name, Is.EqualTo("Miguel"));
			Assert.That(o.Type, Is.EqualTo("Programmer"));
			Assert.That(o.SampleKey, Is.EqualTo(2));
			Assert.That(o.Nothing, Is.EqualTo("zilch"));
		}

		private class NullValueTester
		{
			public string Name
			{
				get;
				set;
			}

			public string Type
			{
				get;
				set;
			}

			public int SampleKey
			{
				get;
				set;
			}

			public string Nothing
			{
				get;
				set;
			}

			public NullClass NullClass { get; set; }

			public DateTime? NullableDateTime { get; set; }

			public NullValueTester()
			{
				Name = "Miguel";
				Type = "User";
				SampleKey = 1;
				Nothing = "zilch";
				NullableDateTime = new DateTime(2012, 01, 01);
			}
		}

#if !MONOTOUCH
		[DataContract]
		class Person
		{
			[DataMember(Name = "MyID")]
			public int Id { get; set; }
			[DataMember]
			public string Name { get; set; }
		}

		[Test]
		public void Can_override_name()
		{
			var person = new Person
			{
				Id = 123,
				Name = "Abc"
			};

			Assert.That(TypeSerializer.SerializeToString(person), Is.EqualTo("{MyID:123,Name:Abc}"));
			Assert.That(JsonSerializer.SerializeToString(person), Is.EqualTo("{\"MyID\":123,\"Name\":\"Abc\"}"));
		}
#endif

        [Flags]
        public enum ExampleEnum : ulong
        {
            None = 0,
            One = 1,
            Two = 2,
            Four = 4,
            Eight = 8
        }

        [Test]
        public void Can_serialize_unsigned_flags_enum()
        {
            var anon = new
            {
                EnumProp1 = ExampleEnum.One | ExampleEnum.Two,
                EnumProp2 = ExampleEnum.Eight,
            };

            Assert.That(TypeSerializer.SerializeToString(anon), Is.EqualTo("{EnumProp1:3,EnumProp2:8}"));
            Assert.That(JsonSerializer.SerializeToString(anon), Is.EqualTo("{\"EnumProp1\":3,\"EnumProp2\":8}"));
        }

        public enum ExampleEnumWithoutFlagsAttribute : ulong
        {
            None = 0,
            One = 1,
            Two = 2
        }

        public class ClassWithEnumWithoutFlagsAttribute
        {
            public ExampleEnumWithoutFlagsAttribute EnumProp1 { get; set; }
            public ExampleEnumWithoutFlagsAttribute EnumProp2 { get; set; }
        }

        [Test]
        public void Can_serialize_unsigned_enum_with_turned_on_TreatEnumAsInteger()
        {
            JsConfig.TreatEnumAsInteger = true;

            var anon = new ClassWithEnumWithoutFlagsAttribute
            {
                EnumProp1 = ExampleEnumWithoutFlagsAttribute.One,
                EnumProp2 = ExampleEnumWithoutFlagsAttribute.Two
            };

            Assert.That(JsonSerializer.SerializeToString(anon), Is.EqualTo("{\"EnumProp1\":1,\"EnumProp2\":2}"));
			Assert.That(TypeSerializer.SerializeToString(anon), Is.EqualTo("{EnumProp1:1,EnumProp2:2}"));
		}

        [Test]
        public void Can_deserialize_unsigned_enum_with_turned_on_TreatEnumAsInteger()
        {
            JsConfig.TreatEnumAsInteger = true;

            var s = "{\"EnumProp1\":1,\"EnumProp2\":2}";
            var o = JsonSerializer.DeserializeFromString<ClassWithEnumWithoutFlagsAttribute>(s);

            Assert.That(o.EnumProp1, Is.EqualTo(ExampleEnumWithoutFlagsAttribute.One));
            Assert.That(o.EnumProp2, Is.EqualTo(ExampleEnumWithoutFlagsAttribute.Two));
        }

        [Test]
        public void Can_serialize_object_array_with_nulls()
        {
            var objs = new[] { (object)"hello", (object)null };
            JsConfig.IncludeNullValues = false;

            Assert.That(objs.ToJson(), Is.EqualTo("[\"hello\",null]"));
        }
	}
}
