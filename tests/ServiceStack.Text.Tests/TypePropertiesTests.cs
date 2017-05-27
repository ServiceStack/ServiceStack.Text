using NUnit.Framework;

namespace ServiceStack.Text.Tests
{
    class TypedTuple
    {
        public string S { get; set; }
        public int I { get; set; }
        public long L { get; set; }
        public double D { get; set; }
    }

    public class TypePropertiesTests
    {
        static TypedTuple CreateTypedTuple() =>
            new TypedTuple { S = "foo", I = 1, L = 2, D = 3.3 };

        [Test]
        public void Can_cache_ValueTuple_field_accessors()
        {
            var typeProperties = TypeProperties.Get(typeof(TypedTuple));

            var oTuple = (object)CreateTypedTuple();

            typeProperties.GetPublicSetter("S")(oTuple, "bar");
            typeProperties.GetPublicSetter("I")(oTuple, 10);
            typeProperties.GetPublicSetter("L")(oTuple, 20L);
            typeProperties.GetPublicSetter("D")(oTuple, 4.4d);

            Assert.That(typeProperties.GetPublicGetter("S")(oTuple), Is.EqualTo("bar"));
            Assert.That(typeProperties.GetPublicGetter("I")(oTuple), Is.EqualTo(10));
            Assert.That(typeProperties.GetPublicGetter("L")(oTuple), Is.EqualTo(20));
            Assert.That(typeProperties.GetPublicGetter("D")(oTuple), Is.EqualTo(4.4));

            var tuple = (TypedTuple)oTuple;

            Assert.That(tuple.S, Is.EqualTo("bar"));
            Assert.That(tuple.I, Is.EqualTo(10));
            Assert.That(tuple.L, Is.EqualTo(20));
            Assert.That(tuple.D, Is.EqualTo(4.4));
        }
    }
}