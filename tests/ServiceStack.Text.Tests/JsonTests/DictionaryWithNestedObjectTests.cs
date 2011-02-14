using System.Collections.Generic;
using NUnit.Framework;
using ServiceStack.Text.Common;

namespace ServiceStack.Text.Tests.JsonTests
{
    [TestFixture]
    public class DictionaryWithNestedObjectTests
    {
        [Test]
        public void DeserializeAsString_WhenConfiguredToDeserializeAsDictionary_EntityIsDeserializedAsDictionary()
        {
            var kv = new Dictionary<string, object>
            {
                {"String1", "A"},
                {"Int1", 1},
                {"Item1", new Item {Value = "asdf"}}
            };
            var json = JsonSerializer.SerializeToString(kv);

            JsState.ConvertObjectTypesIntoStringDictionary = true;
            var kv2 = JsonSerializer.DeserializeFromString<Dictionary<string, object>>(json);

            Assert.AreEqual("A", kv2["String1"]);
            Assert.AreEqual("1", kv2["Int1"]);
            var kv3 = (Dictionary<string, object>) kv2["Item1"];
            Assert.AreEqual("asdf", kv3["Value"]);
        }

        [Test]
        public void DeserializeAsString_WhenConfiguredNotToDeserializeAsDictionary_EntityIsDeserializedAsString()
        {
            var kv = new Dictionary<string, object>
            {
                {"String1", "A"},
                {"Int1", 1},
                {"Item1", new Item {Value = "asdf"}}
            };
            var json = JsonSerializer.SerializeToString(kv);

            JsState.ConvertObjectTypesIntoStringDictionary = false;
            var kv2 = JsonSerializer.DeserializeFromString<Dictionary<string, object>>(json);

            Assert.AreEqual("A", kv2["String1"]);
            Assert.AreEqual("1", kv2["Int1"]);
            Assert.AreEqual("{\"Value\":\"asdf\"}", kv2["Item1"]);
        }

        public class Item
        {
            public string Value { get; set; }
        }
    }
}