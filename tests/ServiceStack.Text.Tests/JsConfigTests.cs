using System.Diagnostics;
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


    [TestFixture]
    public class SerializEmitLowerCaseUnderscoreNamesTests
    {
        [Test]
        public void TestJsonDataWithJsConfigScope()
        {
            using (JsConfig.With(emitLowercaseUnderscoreNames:true, 
                propertyConvention:PropertyConvention.Lenient))
                AssertObjectJson();
        }

        [Test]
        public void TestCloneObjectWithJsConfigScope()
        {
            using (JsConfig.With(emitLowercaseUnderscoreNames: true,
                propertyConvention: PropertyConvention.Lenient))
                AssertObject();
        }

        [Test]
        public void TestJsonDataWithJsConfigGlobal()
        {
            JsConfig.EmitLowercaseUnderscoreNames = true;
            JsConfig.PropertyConvention = PropertyConvention.Lenient;

            AssertObjectJson();

            JsConfig.Reset();
        }

        [Test]
        public void TestCloneObjectWithJsConfigGlobal()
        {
            JsConfig.EmitLowercaseUnderscoreNames = true;
            JsConfig.PropertyConvention = PropertyConvention.Lenient;

            AssertObject();

            JsConfig.Reset();
        }

        [Test]
        public void TestJsonDataWithJsConfigLocal()
        {
            JsConfig.EmitLowercaseUnderscoreNames = true;
            JsConfig.PropertyConvention = PropertyConvention.Lenient;

            AssertObjectJson();

            JsConfig.Reset();
        }

        [Test]
        public void TestCloneObjectWithJsConfigLocal()
        {
            JsConfig.EmitLowercaseUnderscoreNames = false;
            JsConfig<TestObject>.EmitLowercaseUnderscoreNames = true;
            JsConfig.PropertyConvention = PropertyConvention.Lenient;

            AssertObject();

            JsConfig.Reset();
        }

        [Test]
        public void TestCloneObjectWithoutLowercaseThroughJsConfigLocal()
        {
            JsConfig.EmitLowercaseUnderscoreNames = true;
            JsConfig<TestObject>.EmitLowercaseUnderscoreNames = false;
            JsConfig.PropertyConvention = PropertyConvention.Lenient;

            AssertObject();

            JsConfig.Reset();
        }

        private void AssertObject()
        {
            var obj = CreateObject();
            var clonedObj = Deserialize(Serialize(obj));

            Assert.AreEqual(obj.Id, clonedObj.Id, AssertMessageFormat.Fmt("Id"));
            Assert.AreEqual(obj.RootId, clonedObj.RootId, AssertMessageFormat.Fmt("RootId"));
            Assert.AreEqual(obj.DisplayName, clonedObj.DisplayName, AssertMessageFormat.Fmt("DisplayName"));
        }

        private void AssertObjectJson()
        {
            var obj = CreateObject();
            var json = Serialize(obj);
            AssertObjectJson("Object Json: {0}", json);

            var cloned = CloneObject(obj);
            var clonedJson = Serialize(cloned);
            AssertObjectJson("Clone Object Json: {0}", clonedJson);
        }

        private void AssertObjectJson(string traceFormat, string json)
        {
            Trace.WriteLine(string.Format(traceFormat, json));

            Assert.True(json.Contains("\"root_id\":100,"), AssertMessageFormat.Fmt("root_id"));
            Assert.True(json.Contains("\"display_name\":\"Test object\""), AssertMessageFormat.Fmt("display_name"));
        }

        private string Serialize(TestObject obj)
        {
            return obj.ToJson();
        }

        private TestObject Deserialize(string str)
        {
            return str.FromJson<TestObject>();
        }

        private TestObject CreateObject()
        {
            return new TestObject
            {
                Id = 1,
                RootId = 100,
                DisplayName = "Test object"
            };
        }

        private TestObject CloneObject(TestObject src)
        {
            return Deserialize(Serialize(src));
        }

        class TestObject
        {
            public int Id { get; set; }
            public int RootId { get; set; }
            public string DisplayName { get; set; }
        }

        private const string AssertMessageFormat = "Cannot find correct property value ({0})";
    }

}