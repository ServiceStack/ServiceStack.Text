using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;

namespace ServiceStack.Text.Tests
{
	[TestFixture]
	public class DictionaryTests
		: TestBase
	{

		[Test]
		public void Can_serialize_one_level_dictionary()
		{
			var map = new Dictionary<string, int>
          	{
				{"One", 1}, {"Two", 2}, {"Three", 3}, 
          	};

			Serialize(map);
		}

		[Test]
		public void Can_serialize_empty_map()
		{
			var emptyMap = new Dictionary<string, int>();

			Serialize(emptyMap);
		}

		[Test]
		public void Can_serialize_empty_string_map()
		{
			var emptyMap = new Dictionary<string, string>();

			Serialize(emptyMap);
		}

		[Test]
		public void Can_serialize_two_level_dictionary()
		{
			var map = new Dictionary<string, Dictionary<string, int>>
          		{
					{"map1", new Dictionary<string, int>
			         	{
							{"One", 1}, {"Two", 2}, {"Three", 3}, 
			         	}
					},
					{"map2", new Dictionary<string, int>
			         	{
							{"Four", 4}, {"Five", 5}, {"Six", 6}, 
			         	}
					},
          		};

			Serialize(map);
		}

		[Test]
		public void Can_serialize_two_level_dictionary_with_int_key()
		{
			var map = new Dictionary<int, Dictionary<string, int>>
          		{
					{1, new Dictionary<string, int>
			         	{
							{"One", 1}, {"Two", 2}, {"Three", 3}, 
			         	}
					},
					{2, new Dictionary<string, int>
			         	{
							{"Four", 4}, {"Five", 5}, {"Six", 6}, 
			         	}
					},
          		};

			Serialize(map);
		}

		private static Dictionary<string, object> SetupDict()
		{
			return new Dictionary<string, object> {
                { "a", "text" },
                { "b", 32 },
                { "c", false },
                { "d", new[] {1, 2, 3} }
            };
		}

		public class MixType
		{
			public string a { get; set; }
			public int b { get; set; }
			public bool c { get; set; }
		    public int[] d { get; set; }
		}

		private static void AssertDict(Dictionary<string, object> dict)
		{
			Assert.AreEqual("text", dict["a"]);
			Assert.AreEqual(32, dict["b"]);
			Assert.AreEqual(false, dict["c"]);
		}

		//[Test]
		//public void Test_JsonNet()
		//{
		//    var dict = SetupDict();
		//    var json = JsonConvert.SerializeObject(dict);
		//    var deserializedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
		//    AssertDict(deserializedDict);
		//}

		[Test]
		public void Test_ServiceStack_Text_TypeSerializer()
		{
            JsConfig.TryToParsePrimitiveTypeValues = true;
            JsConfig.ConvertObjectTypesIntoStringDictionary = true;
            try {
			    var dict = SetupDict();
			    var json = TypeSerializer.SerializeToString(dict);
			    var deserializedDict = TypeSerializer.DeserializeFromString<Dictionary<string, object>>(json);
			    AssertDict(deserializedDict);
            } finally {
                JsConfig.TryToParsePrimitiveTypeValues = false;
                JsConfig.ConvertObjectTypesIntoStringDictionary = false;
            }
		}

		[Test]
		public void Test_ServiceStack_Text_JsonSerializer()
		{
            JsConfig.TryToParsePrimitiveTypeValues = true;
            JsConfig.ConvertObjectTypesIntoStringDictionary = true;
            try {
			    var dict = SetupDict();
			    var json = JsonSerializer.SerializeToString(dict);
			    var deserializedDict = JsonSerializer.DeserializeFromString<Dictionary<string, object>>(json);
			    AssertDict(deserializedDict);
            } finally {
                JsConfig.TryToParsePrimitiveTypeValues = false;
                JsConfig.ConvertObjectTypesIntoStringDictionary = false;
            }
		}

		[Test]
		public void Test_ServiceStack_Text_JsonSerializer_Array_Value_Deserializes_Correctly()
        {
            JsConfig.TryToParsePrimitiveTypeValues = true;
            JsConfig.ConvertObjectTypesIntoStringDictionary = true;
            try {
			    var dict = SetupDict();
			    var json = JsonSerializer.SerializeToString(dict);
			    var deserializedDict = JsonSerializer.DeserializeFromString<Dictionary<string, object>>(json);
			    Assert.AreEqual("text", deserializedDict["a"]);
			    Assert.AreEqual(new List<int> {1, 2, 3}, deserializedDict["d"]);                
            } finally {
                JsConfig.TryToParsePrimitiveTypeValues = false;
                JsConfig.ConvertObjectTypesIntoStringDictionary = false;
            }
		}

		[Test]
		public void Can_deserialize_mixed_dictionary_into_strongtyped_map()
		{
			var mixedMap = SetupDict();

			var json = JsonSerializer.SerializeToString(mixedMap);
			Console.WriteLine("JSON:\n" + json);

			var mixedType = json.FromJson<MixType>();
			Assert.AreEqual("text", mixedType.a);
			Assert.AreEqual(32, mixedType.b);
			Assert.AreEqual(false, mixedType.c);
			Assert.AreEqual(new[] {1, 2, 3}, mixedType.d);
		}

		[Test]
		public void Can_serialise_null_values_from_dictionary_correctly()
		{
			JsConfig.IncludeNullValues = true;
			var dictionary = new Dictionary<string, object> { { "value", null } };

			Serialize(dictionary, includeXml: false);

			var json = JsonSerializer.SerializeToString(dictionary);
			Log(json);

			Assert.That(json, Is.EqualTo("{\"value\":null}"));
			JsConfig.Reset();
		}

		[Test]
		public void Will_ignore_null_values_from_dictionary_correctly()
		{
			JsConfig.IncludeNullValues = false;
			var dictionary = new Dictionary<string, string> { { "value", null } };

			Serialize(dictionary, includeXml: false);

			var json = JsonSerializer.SerializeToString(dictionary);
			Log(json);

			Assert.That(json, Is.EqualTo("{}"));
			JsConfig.Reset();
		}

		public class FooSlash
		{
			public Dictionary<string, string> Nested { get; set; }
			public string Bar { get; set; }
		}

		[Test]
		public void Can_serialize_Dictionary_with_end_slash()
		{
			var foo = new FooSlash {
				Nested = new Dictionary<string, string> { { "key", "value\"" } },
				Bar = "BarValue"
			};
			Serialize(foo);
		}

        [Test]
        public void Can_serialise_null_values_from_nested_dictionary_correctly()
        {
            JsConfig.IncludeNullValues = true;
            var foo = new FooSlash();
            var json = JsonSerializer.SerializeToString(foo);
            Assert.That(json, Is.EqualTo("{\"Nested\":null,\"Bar\":null}"));
			JsConfig.Reset();
		}

		[Test]
		public void Can_serialize_Dictionary_with_quotes()
		{
			var dto = new Dictionary<string, string> { { "title", "\"test\"" } };
			var to = Serialize(dto);

			Assert.That(to["title"], Is.EqualTo(dto["title"]));
		}

        [Test]
        public void Can_Deserialize_Object_To_Dictionary()
        {
            const string json = "{\"Id\":1}";
            var d = json.To<Dictionary<string, string>>();
            Assert.That(d.ContainsKey("Id"));
            Assert.That(d["Id"], Is.EqualTo("1"));
        }

#if NET40
        [Test]
        public void Nongeneric_implementors_of_IDictionary_K_V_Should_serialize_like_Dictionary_K_V()
        {
            dynamic expando = new System.Dynamic.ExpandoObject();
            expando.Property = "Value";
            IDictionary<string, object> dict = expando;
            Assert.AreEqual(dict.Dump(), new Dictionary<string, object>(dict).Dump());
        }
#endif
	}

}