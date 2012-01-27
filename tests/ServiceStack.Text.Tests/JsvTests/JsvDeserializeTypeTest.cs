using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ServiceStack.Text.Common;
using ServiceStack.Text.Jsv;

namespace ServiceStack.Text.Tests.JsvTests
{
    [TestFixture]
    public class JsvDeserializeTypeTests
    {
        [Test]
        public void Get_setter_method_for_simple_properties()
        {
            Type type = typeof (Test);
            PropertyInfo propertyInfo = type.GetProperty("TestProperty");
            SetPropertyDelegate setMethod = JsvDeserializeType.GetSetPropertyMethod(type, propertyInfo);
            Test test = new Test();
            setMethod.Invoke(test, "test");
            Assert.AreEqual("test", test.TestProperty);
        }

        private class Test
        {
            public string TestProperty { get; set; }
        }
    }
}