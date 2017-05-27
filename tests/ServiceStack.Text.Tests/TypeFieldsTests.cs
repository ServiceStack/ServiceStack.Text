using NUnit.Framework;

namespace ServiceStack.Text.Tests
{
    public class TypeFieldsTests
    {
        (string s, int i, long l, double d) CreateValueTuple() =>
            ("foo", 1, 2, 3.3);

        [Test]
        public void Can_cache_ValueTuple_field_accessors()
        {
            var typeFields = TypeFields.Get(typeof((string s, int i, long l, double d)));

            var oTuple = (object)CreateValueTuple();

            typeFields.GetPublicSetterRef("Item1")(ref oTuple, "bar");
            typeFields.GetPublicSetterRef("Item2")(ref oTuple, 10);
            typeFields.GetPublicSetterRef("Item3")(ref oTuple, 20L);
            typeFields.GetPublicSetterRef("Item4")(ref oTuple, 4.4d);

            Assert.That(typeFields.GetPublicGetter("Item1")(oTuple), Is.EqualTo("bar"));
            Assert.That(typeFields.GetPublicGetter("Item2")(oTuple), Is.EqualTo(10));
            Assert.That(typeFields.GetPublicGetter("Item3")(oTuple), Is.EqualTo(20));
            Assert.That(typeFields.GetPublicGetter("Item4")(oTuple), Is.EqualTo(4.4));

            var tuple = ((string s, int i, long l, double d))oTuple;

            Assert.That(tuple.s, Is.EqualTo("bar"));
            Assert.That(tuple.i, Is.EqualTo(10));
            Assert.That(tuple.l, Is.EqualTo(20));
            Assert.That(tuple.d, Is.EqualTo(4.4));
        }
    }
}