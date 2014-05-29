using NUnit.Framework;
using ServiceStack.Text.Reflection;

namespace ServiceStack.Text.Tests
{
    public class AccessorBase
    {
        public string Base { get; set; }
    }

    public class Accessor
    {
        public string Declared { get; set; }
    }

    [TestFixture]
    public class StaticAccessorTests
    {
        [Test]
        public void Can_get_accessor_in_declared_and_base_class()
        {
            var baseProperty = typeof(AccessorBase).GetProperty("Base");
            var declaredProperty = typeof(Accessor).GetProperty("Declared");

            var baseSetter = baseProperty.GetValueSetter<AccessorBase>();
            Assert.That(baseSetter, Is.Not.Null);

            var declaredSetter = declaredProperty.GetValueSetter<Accessor>();
            Assert.That(declaredSetter, Is.Not.Null);
        }
    }
}