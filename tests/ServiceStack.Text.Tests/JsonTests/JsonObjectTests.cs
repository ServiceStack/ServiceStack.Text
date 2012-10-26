using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ServiceStack.Text.Tests.JsonTests
{
    [TestFixture]
    public class JsonObjectTests
    {
        [Test]
        public void Can_parse_empty_object()
        {
            Assert.That(JsonObject.Parse("{}"), Is.Empty);
        }

        [Test]
        public void Can_parse_empty_object_with_whitespaces()
        {
            Assert.That(JsonObject.Parse("{    }"), Is.Empty);
            Assert.That(JsonObject.Parse("{\n\n}"), Is.Empty);
            Assert.That(JsonObject.Parse("{\t\t}"), Is.Empty);
        }

        [Test]
        public void Can_parse_empty_object_with_mixed_whitespaces()
        {
            Assert.That(JsonObject.Parse("{ \n\t  \n\r}"), Is.Empty);
        }

        public class Jackalope
        {
            public string Name { get; set; }
            public Jackalope BabyJackalope { get; set; }
        }

        [Test]
        public void Can_serialise_json_object_deserialise_typed_object()
        {
            var jacks = new
            {
                Jack = (new Jackalope { BabyJackalope = new Jackalope { Name = "in utero" } })
            };

            var jackString = JsonSerializer.SerializeToString(jacks.Jack);
            var jackJson = JsonObject.Parse(jackString);
            var jackJsonString = jackJson.SerializeToString();
            Assert.AreEqual(jackString, jackJsonString);

            var jackalope = JsonSerializer.DeserializeFromString<Jackalope>(jackJsonString);
            Assert.That(jackalope.BabyJackalope.Name, Is.EqualTo("in utero"));
        }
    }
}
