using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace ServiceStack.Text.Tests
{
    [TestFixture]
    public class HashSetTests
    {
        [Test]
        public void Can_deserialize_null_string_collection()
        {
            using (var config = JsConfig.BeginScope())
            {
                config.IncludeNullValues = true;
                config.ThrowOnDeserializationError = true;
                var original = new ModelWithStringHashSet { Value = null };
                var json = JsonSerializer.SerializeToString(original);
                var deserialized = JsonSerializer.DeserializeFromString<ModelWithStringHashSet>(json);

                json.Print();

                Assert.That(deserialized, Is.Not.Null);
                Assert.That(deserialized.Value, Is.EqualTo(original.Value));
            }
        }

        [Test]
        public void Can_deserialize_null_int_collection()
        {
            using (var config = JsConfig.BeginScope())
            {
                config.IncludeNullValues = true;
                config.ThrowOnDeserializationError = true;
                var original = new ModelWithIntHashSet { Value = null };
                var json = JsonSerializer.SerializeToString(original);
                var deserialized = JsonSerializer.DeserializeFromString<ModelWithIntHashSet>(json);

                json.Print();

                Assert.That(deserialized, Is.Not.Null);
                Assert.That(deserialized.Value, Is.EqualTo(original.Value));
            }
        }

        private class ModelWithStringHashSet
        {
            public HashSet<string> Value { get; set; }
        }

        private class ModelWithIntHashSet
        {
            public HashSet<string> Value { get; set; }
        }
    }
}
