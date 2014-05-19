using NUnit.Framework;

namespace ServiceStack.Text.Tests
{
    [TestFixture]
    public class JsConfigTests
    {
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            JsConfig.EmitLowercaseUnderscoreNames = true;
            JsConfig<Bar>.EmitLowercaseUnderscoreNames = false;
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            JsConfig.Reset();
        }

        [Test]
        public void Does_use_specific_configuration()
        {
            Assert.That(new Foo { FooBar = "value" }.ToJson(), Is.EqualTo("{\"foo_bar\":\"value\"}"));
            Assert.That(new Bar { FooBar = "value" }.ToJson(), Is.EqualTo("{\"FooBar\":\"value\"}"));
        }

        [Test]
        public void Can_override_default_configuration()
        {
            using (JsConfig.With(emitLowercaseUnderscoreNames: false))
            {
                Assert.That(new Foo { FooBar = "value" }.ToJson(), Is.EqualTo("{\"FooBar\":\"value\"}"));
            }
        }
    }

    public class Foo
    {
        public string FooBar { get; set; }
    }

    public class Bar
    {
        public string FooBar { get; set; }
    }
}