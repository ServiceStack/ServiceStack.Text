using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace ServiceStack.Text.Tests.JsonTests
{
	[TestFixture]
	public class AnonymousDeserializationTests
		: TestBase
	{
		private class Item
		{
			public int IntValue { get; set; }
			public string StringValue { get; set; }

			public static Item Create()
			{
				return new Item { IntValue = 42, StringValue = "Foo" };
			}
		}

		[Test]
		public void Can_deserialize_to_anonymous_type()
		{
			var original = Item.Create();
			var json = JsonSerializer.SerializeToString(original);
			
			var item = DeserializeAnonymousType(new { IntValue = default(int), StringValue = default(string) }, json);

			Assert.That(item.IntValue, Is.EqualTo(42));
			Assert.That(item.StringValue, Is.EqualTo("Foo"));
		}

		private static T DeserializeAnonymousType<T>(T template, string json) 
		{
			TypeConfig<T>.EnableAnonymousFieldSetters = true;
			return (T)JsonSerializer.DeserializeFromString(json, template.GetType());
		}

        [Test]
        public void Deserialize_dynamic_json()
        {
            var json = "{\"Id\":\"fb1d17c7298c448cb7b91ab7041e9ff6\",\"Name\":\"John\",\"DateOfBirth\":\"\\/Date(317433600000-0000)\\/\"}";

            var obj = JsonObject.Parse(json);
            obj.Get<Guid>("Id").ToString().Print();
            obj.Get<string>("Name").Print();
            obj.Get<DateTime>("DateOfBirth").ToLongDateString().Print();

            dynamic dyn = DynamicJson.Deserialize(json);
            string id = dyn.Id;
            string name = dyn.Name;
            string dob = dyn.DateOfBirth;
            "DynamicJson: {0}, {1}, {2}".Print(id, name, dob);

            using (JsConfig.With(convertObjectTypesIntoStringDictionary: true))
            {
                "Object Dictionary".Print();
                var map = (Dictionary<string, object>)json.FromJson<object>();
                map.PrintDump();
            }
        }
	}
}