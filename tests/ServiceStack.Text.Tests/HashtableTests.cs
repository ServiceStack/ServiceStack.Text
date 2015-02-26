using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ServiceStack.Text.Tests
{
    [TestFixture]
    public class HashtableTests
        : TestBase
    {

        [Test]
        public void Can_deserialize_null_hashtable()
        {
            using (var config = JsConfig.BeginScope())
            {
                config.IncludeNullValues = true;
                config.ThrowOnDeserializationError = true;
                var original = new ModelWithHashtable { Value = null };
                var json = JsonSerializer.SerializeToString(original);
                var deserialized = JsonSerializer.DeserializeFromString<ModelWithHashtable>(json);

                json.Print();

                Assert.That(deserialized, Is.Not.Null);
                Assert.That(deserialized.Value, Is.EqualTo(original.Value));
            }
        }

        private class ModelWithHashtable
        {
            public Hashtable Value { get; set; }
        }
    }
}
