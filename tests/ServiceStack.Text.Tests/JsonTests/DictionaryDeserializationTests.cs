using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace ServiceStack.Text.Tests.JsonTests
{
    [TestFixture]
    public class DictionaryDeserializationTests
    {
        [Test]
        public void CanDeserializeDictionaryOfComplexTypes()
        {
            JsConfig.ConvertObjectTypesIntoStringDictionary = true;

            var dict = new Dictionary<string, object>();

            dict["ChildDict"] = new Dictionary<string, object>
            {
                {"age", 12},
                {"name", "mike"}
            };

            dict["ChildIntList"] = new List<int> {1, 2, 3};
            dict["ChildStringList"] = new List<string> {"a", "b", "c"};
            dict["ChildObjectList"] = new List<object> {1, "cat", new Dictionary<string, object> {{"s", "s"}, {"n", 1}}};

            var serialized = JsonSerializer.SerializeToString(dict);

            var deserialized = JsonSerializer.DeserializeFromString<Dictionary<string, object>>(serialized);

            Assert.IsNotNull(deserialized["ChildDict"]);
            Assert.IsInstanceOf<Dictionary<string, object>>(deserialized["ChildDict"]);

            Assert.AreEqual("12", ((IDictionary)deserialized["ChildDict"])["age"]);
            Assert.AreEqual("mike", ((IDictionary)deserialized["ChildDict"])["name"]);
        }
    }
}