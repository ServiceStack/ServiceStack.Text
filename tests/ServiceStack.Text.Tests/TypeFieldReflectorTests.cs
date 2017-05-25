using NUnit.Framework;

namespace ServiceStack.Text.Tests
{
    public class TypeFieldReflectorTests
    {
        (string s, int i, long l, double d) CreateValueTuple() =>
            ("foo", 1, 2, 3.3);

        [Test]
        public void Can_cache_ValueTuple_field_accessors()
        {
            var typeAccessor = TypeFields.Get(typeof((string s, int i, long l, double d)));

            var oTuple = (object)CreateValueTuple();

            typeAccessor.GetPublicSetterRef("Item1")(ref oTuple, "bar");
            typeAccessor.GetPublicSetterRef("Item2")(ref oTuple, 10);
            typeAccessor.GetPublicSetterRef("Item3")(ref oTuple, 20L);
            typeAccessor.GetPublicSetterRef("Item4")(ref oTuple, 4.4d);

            var tuple = ((string s, int i, long l, double d))oTuple;

            Assert.That(tuple.s, Is.EqualTo("bar"));
            Assert.That(tuple.i, Is.EqualTo(10));
            Assert.That(tuple.l, Is.EqualTo(20));
            Assert.That(tuple.d, Is.EqualTo(4.4));
        }
    }
}