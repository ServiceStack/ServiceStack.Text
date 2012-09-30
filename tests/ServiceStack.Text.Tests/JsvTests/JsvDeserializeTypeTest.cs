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

        [Test]
        public void Get_setter_method_for_dictionary_properties()
        {
            var dict = new Dictionary<string, string>();
            Type type = typeof (Dictionary<string,string>);
            foreach (var propertyInfo in type.GetProperties()) {
                SetPropertyDelegate setMethod = JsvDeserializeType.GetSetPropertyMethod(type, propertyInfo);
                if (setMethod == null) continue;
                Console.WriteLine(propertyInfo.Name);
                setMethod.Invoke(dict, propertyInfo.Name);
            }
        }

        private class Test
        {
            public string TestProperty { get; set; }
        }
    }
}