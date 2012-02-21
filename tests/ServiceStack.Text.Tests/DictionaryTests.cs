using System;
using System.Collections.Generic;
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
            };
		}

		public class MixType
		{
			public string a { get; set; }
			public int b { get; set; }
			public bool c { get; set; }
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

		//[Test, Ignore("No type info emitted for primitive values")]
		public void Test_ServiceStack_Text_TypeSerializer()
		{
			var dict = SetupDict();
			var json = TypeSerializer.SerializeToString(dict);
			var deserializedDict = TypeSerializer.DeserializeFromString<Dictionary<string, object>>(json);
			AssertDict(deserializedDict);
		}

		//[Test, Ignore("No type info emitted for primitive values")]
		public void Test_ServiceStack_Text_JsonSerializer()
		{
			var dict = SetupDict();
			var json = JsonSerializer.SerializeToString(dict);
			var deserializedDict = JsonSerializer.DeserializeFromString<Dictionary<string, object>>(json);
			AssertDict(deserializedDict);
		}

		[Test]
		public void Can_deserialize_mixed_dictionary_into_strongtyped_map()
		{
			var mixedMap = new Dictionary<string, object> {
                { "a", "text" },
                { "b", 32 },
                { "c", false },
            };

			var json = JsonSerializer.SerializeToString(mixedMap);
			Console.WriteLine("JSON:\n" + json);

			var mixedType = json.FromJson<MixType>();
			Assert.AreEqual("text", mixedType.a);
			Assert.AreEqual(32, mixedType.b);
			Assert.AreEqual(false, mixedType.c);
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
	}

}