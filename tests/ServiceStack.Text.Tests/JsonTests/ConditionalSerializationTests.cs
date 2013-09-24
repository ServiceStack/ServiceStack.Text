using NUnit.Framework;
using System;

namespace ServiceStack.Text.Tests.JsonTests
{
    [TestFixture]
    public class ConditionalSerializationTests
    {
        [Test]
        public void TestSerializeRespected()
        {
            var obj = new Foo { X = "abc", Z = "def" }; // don't touch Y...

            string json = JsonSerializer.SerializeToString(obj);
            Assert.That(json, Is.StringMatching("{\"X\":\"abc\",\"Z\":\"def\"}"));   
        }
        [Test]
        public void TestSerializeRespectedWithInheritance()
        {
            var obj = new SuperFoo { X = "abc", Z = "def", A =123, C = 456 }; // don't touch Y or B...

            string json = JsonSerializer.SerializeToString(obj);
            Assert.That(json, Is.StringMatching("{\"A\":123,\"C\":456,\"X\":\"abc\",\"Z\":\"def\"}"));
        }
        public class Foo
        {
            public string X { get; set; } // not conditional

            public string Y // conditional: never serialized
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public bool ShouldSerializeY()
            {
                return false;
            }

            public string Z { get;set;} // conditional: always serialized
            public bool ShouldSerializeZ()
            {
                return true;
            }
        }

        public class SuperFoo : Foo
        {
            public int A { get; set; } // not conditional

            public int B // conditional: never serialized
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public bool ShouldSerializeB()
            {
                return false;
            }

            public int  C { get; set; } // conditional: always serialized
            public bool ShouldSerializeC()
            {
                return true;
            }
        }
    }
}
