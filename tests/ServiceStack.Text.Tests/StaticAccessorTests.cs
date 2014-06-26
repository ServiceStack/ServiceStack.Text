using NUnit.Framework;
using ServiceStack.Reflection;

namespace ServiceStack.Text.Tests
{
    public class AccessorBase
    {
        public string Base { get; set; }
        public string BaseField;
    }

    public class Accessor
    {
        public string Declared { get; set; }
    }

    public class SubAccessor : AccessorBase
    {
        public string Sub { get; set; }
        public string SubField;
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

        [Test]
        public void Can_get_property_accessor_from_sub_and_super_types()
        {
            var sub = new SubAccessor();
            var subGet = StaticAccessors.GetValueGetter<SubAccessor>(typeof(SubAccessor).GetProperty("Sub"));
            var subSet = StaticAccessors.GetValueSetter<SubAccessor>(typeof(SubAccessor).GetProperty("Sub"));

            subSet(sub, "sub");
            Assert.That(subGet(sub), Is.EqualTo("sub"));

            var sup = new AccessorBase();
            var supGet = StaticAccessors.GetValueGetter<AccessorBase>(typeof(AccessorBase).GetProperty("Base"));
            var supSet = StaticAccessors.GetValueSetter<AccessorBase>(typeof(AccessorBase).GetProperty("Base"));

            supSet(sup, "base");
            Assert.That(supGet(sup), Is.EqualTo("base"));
            supSet(sub, "base");
            Assert.That(supGet(sub), Is.EqualTo("base"));
        }

        [Test]
        public void Can_get_field_accessor_from_sub_and_super_types()
        {
            var sub = new SubAccessor();
            var subGet = StaticAccessors.GetValueGetter<SubAccessor>(typeof(SubAccessor).GetField("SubField"));
            var subSet = StaticAccessors.GetValueSetter<SubAccessor>(typeof(SubAccessor).GetField("SubField"));

            subSet(sub, "sub");
            Assert.That(subGet(sub), Is.EqualTo("sub"));

            var sup = new AccessorBase();
            var supGet = StaticAccessors.GetValueGetter<AccessorBase>(typeof(AccessorBase).GetField("BaseField"));
            var supSet = StaticAccessors.GetValueSetter<AccessorBase>(typeof(AccessorBase).GetField("BaseField"));

            supSet(sup, "base");
            Assert.That(supGet(sup), Is.EqualTo("base"));
            supSet(sub, "base");
            Assert.That(supGet(sub), Is.EqualTo("base"));
        }
    }


}