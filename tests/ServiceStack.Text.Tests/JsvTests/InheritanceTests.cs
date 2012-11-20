using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ServiceStack.Text.Jsv;

namespace ServiceStack.Text.Tests.JsvTests
{
    [TestFixture]
    public class InheritanceTests
    {
        public class TestParent
        {
            public string Parameter { get; private set; }

            public TestParent(string parameter)
            {
                Parameter = parameter;
            }
        }

        public class TestChild : TestParent
        {
            public TestChild(string parameter)
                : base(parameter)
            { }
        }

        public void Should_set_property_of_parent_class()
        {
            var serializer = new JsvSerializer<TestChild>();
            var serialized = serializer.SerializeToString(new TestChild("Test Value"));
            var deserialized = serializer.DeserializeFromString(serialized);
            Assert.That(deserialized.Parameter, Is.EqualTo("Test Value"));
        }
    }
}
