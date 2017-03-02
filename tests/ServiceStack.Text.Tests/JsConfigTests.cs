using System;
using System.Diagnostics;
using NUnit.Framework;

namespace ServiceStack.Text.Tests
{
    public class Foo
    {
        public string FooBar { get; set; }
    }

    public class Bar
    {
        public string FooBar { get; set; }
    }

    [TestFixture]
    public class JsConfigAdhocTests
    {
        [Test]
        public void Can_escape_Html_Chars()
        {
            var dto = new Foo { FooBar = "<script>danger();</script>" };

            Assert.That(dto.ToJson(), Is.EqualTo("{\"FooBar\":\"<script>danger();</script>\"}"));

            JsConfig.EscapeHtmlChars = true;

            Assert.That(dto.ToJson(), Is.EqualTo("{\"FooBar\":\"\\u003cscript\\u003edanger();\\u003c/script\\u003e\"}"));

            JsConfig.Reset();
        }
    }

    [TestFixture]
    public class JsConfigTests
    {
        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            JsConfig.EmitLowercaseUnderscoreNames = true;
            JsConfig<Bar>.EmitLowercaseUnderscoreNames = false;
        }

        [OneTimeTearDown]
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

    [TestFixture]
    public class SerializEmitLowerCaseUnderscoreNamesTests
    {
        [Test]
        public void TestJsonDataWithJsConfigScope()
        {
            using (JsConfig.With(emitLowercaseUnderscoreNames: true,
                propertyConvention: PropertyConvention.Lenient))
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

    [TestFixture]
    public class JsConfigCreateTests
    {
        [Test]
        public void Does_create_scope_from_string()
        {
            var scope = JsConfig.CreateScope("EmitCamelCaseNames,emitlowercaseunderscorenames,IncludeNullValues:false,ExcludeDefaultValues:0,IncludeDefaultEnums:1");
            Assert.That(scope.EmitCamelCaseNames.Value);
            Assert.That(scope.EmitLowercaseUnderscoreNames.Value);
            Assert.That(!scope.IncludeNullValues.Value);
            Assert.That(!scope.ExcludeDefaultValues.Value);
            Assert.That(scope.IncludeDefaultEnums.Value);
            scope.Dispose();

            scope = JsConfig.CreateScope("DateHandler:ISO8601,timespanhandler:durationformat,PropertyConvention:strict");
            Assert.That(scope.DateHandler, Is.EqualTo(DateHandler.ISO8601));
            Assert.That(scope.TimeSpanHandler, Is.EqualTo(TimeSpanHandler.DurationFormat));
            Assert.That(scope.PropertyConvention, Is.EqualTo(PropertyConvention.Strict));
            scope.Dispose();
        }

        [Test]
        public void Does_create_scope_from_string_using_CamelCaseHumps()
        {
            var scope = JsConfig.CreateScope("eccn,elun,inv:false,edv:0,ide:1");
            Assert.That(scope.EmitCamelCaseNames.Value);
            Assert.That(scope.EmitLowercaseUnderscoreNames.Value);
            Assert.That(!scope.IncludeNullValues.Value);
            Assert.That(!scope.ExcludeDefaultValues.Value);
            Assert.That(scope.IncludeDefaultEnums.Value);
            scope.Dispose();

            scope = JsConfig.CreateScope("dh:ISO8601,tsh:df,pc:strict");
            Assert.That(scope.DateHandler, Is.EqualTo(DateHandler.ISO8601));
            Assert.That(scope.TimeSpanHandler, Is.EqualTo(TimeSpanHandler.DurationFormat));
            Assert.That(scope.PropertyConvention, Is.EqualTo(PropertyConvention.Strict));
            scope.Dispose();
        }
    }
}