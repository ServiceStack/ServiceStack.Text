using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace ServiceStack.Text.Tests.JsonTests
{
    public class TypeInfoTests
    {
        class MyClass : IComparable
        {
            public int CompareTo(object obj)
            {
                return 0;
            }
        }

        [Test]
        [TestCase("[{'__type':'ServiceStack.Text.Tests.JsonTests.TypeInfoTests+MyClass'}]")]
        [TestCase("[{ '__type':'ServiceStack.Text.Tests.JsonTests.TypeInfoTests+MyClass'}]")]
        [TestCase("[{\n'__type':'ServiceStack.Text.Tests.JsonTests.TypeInfoTests+MyClass'}]")]
        [TestCase("[{\t'__type':'ServiceStack.Text.Tests.JsonTests.TypeInfoTests+MyClass'}]")]
        [TestCase("[ {'__type':'ServiceStack.Text.Tests.JsonTests.TypeInfoTests+MyClass'}]")]
        [TestCase("[\n{'__type':'ServiceStack.Text.Tests.JsonTests.TypeInfoTests+MyClass'}]")]
        [TestCase("[\t{'__type':'ServiceStack.Text.Tests.JsonTests.TypeInfoTests+MyClass'}]")]
        [TestCase("[ { '__type':'ServiceStack.Text.Tests.JsonTests.TypeInfoTests+MyClass'}]")]
        [TestCase("[\n{\n'__type':'ServiceStack.Text.Tests.JsonTests.TypeInfoTests+MyClass'}]")]
        [TestCase("[\t{\t'__type':'ServiceStack.Text.Tests.JsonTests.TypeInfoTests+MyClass'}]")]
        [TestCase("[{'__type':'ServiceStack.Text.Tests.JsonTests.TypeInfoTests+MyClass'} ]")]
        [TestCase("[{ '__type':'ServiceStack.Text.Tests.JsonTests.TypeInfoTests+MyClass'}\t]")]
        [TestCase("[{\n'__type':'ServiceStack.Text.Tests.JsonTests.TypeInfoTests+MyClass'}\n]")]
        [TestCase("[{\t'__type':'ServiceStack.Text.Tests.JsonTests.TypeInfoTests+MyClass' }]")]
        [TestCase("[ {'__type':'ServiceStack.Text.Tests.JsonTests.TypeInfoTests+MyClass'\t}]")]
        [TestCase("[\n{'__type':'ServiceStack.Text.Tests.JsonTests.TypeInfoTests+MyClass'\n}]")]
        [TestCase("[\t{'__type':'ServiceStack.Text.Tests.JsonTests.TypeInfoTests+MyClass'\t}\n]")]
        [TestCase("[ { '__type':'ServiceStack.Text.Tests.JsonTests.TypeInfoTests+MyClass', }]")]
        [TestCase("[\n{\n'__type':'ServiceStack.Text.Tests.JsonTests.TypeInfoTests+MyClass'}]")]
        [TestCase("[\t{\t'__type':'ServiceStack.Text.Tests.JsonTests.TypeInfoTests+MyClass'}]")]
        public void TypeAttrInObject(string json)
        {
            json = json.Replace('\'', '"');
            var deserDto = JsonSerializer.DeserializeFromString<List<IComparable>>(json);
            Console.WriteLine(json);
            Assert.IsNotNull(deserDto);
            Assert.AreEqual(1, deserDto.Count);
            Assert.IsNotNull(deserDto[0]);
        }
 
    }
}