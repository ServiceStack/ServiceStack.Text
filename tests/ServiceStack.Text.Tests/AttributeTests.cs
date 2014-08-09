using System;
using System.Linq;
using NUnit.Framework;

namespace ServiceStack.Text.Tests
{
    [TestFixture]
    public class AttributeTests
    {
        [Test]
        public void Does_get_Single_Default_Attribute()
        {
            var attrs = typeof(DefaultWithSingleAttribute).AllAttributes<RouteDefaultAttribute>();
            Assert.That(attrs[0].ToString(), Is.EqualTo("/path:"));

            var attr = typeof(DefaultWithSingleAttribute).FirstAttribute<RouteDefaultAttribute>();
            Assert.That(attr.ToString(), Is.EqualTo("/path:"));
        }

        [Test]
        public void Does_get_Single_TypeId_Attribute()
        {
            var attrs = typeof(TypeIdWithSingleAttribute).AllAttributes<RouteTypeIdAttribute>();
            Assert.That(attrs[0].ToString(), Is.EqualTo("/path:"));

            var attr = typeof(TypeIdWithSingleAttribute).FirstAttribute<RouteTypeIdAttribute>();
            Assert.That(attr.ToString(), Is.EqualTo("/path:"));
        }

        [Test]
        public void Does_get_Multiple_Default_Attributes()
        {
            var attrs = typeof(DefaultWithMultipleAttributes).AllAttributes<RouteDefaultAttribute>();
            Assert.That(attrs.Length, Is.EqualTo(4));

            var values = attrs.ToList().ConvertAll(x => x.ToString());

            Assert.That(values, Is.EquivalentTo(new[] {
                "/path:", "/path/2:", "/path:GET", "/path:POST", 
            }));

            var objAttrs = typeof(DefaultWithMultipleAttributes).AllAttributes();
            values = objAttrs.ToList().ConvertAll(x => x.ToString());

            Assert.That(values, Is.EquivalentTo(new[] {
                "/path:", "/path/2:", "/path:GET", "/path:POST", 
            }));

            objAttrs = typeof(DefaultWithMultipleAttributes).AllAttributes(typeof(RouteDefaultAttribute));
            values = objAttrs.ToList().ConvertAll(x => x.ToString());

            Assert.That(values, Is.EquivalentTo(new[] {
                "/path:", "/path/2:", "/path:GET", "/path:POST", 
            }));
        }

        [Test]
        public void Does_get_Multiple_TypeId_Attributes()
        {
            var attrs = typeof(TypeIdWithMultipleAttributes).AllAttributes<RouteTypeIdAttribute>();
            Assert.That(attrs.Length, Is.EqualTo(4));

            var values = attrs.ToList().ConvertAll(x => x.ToString());

            Assert.That(values, Is.EquivalentTo(new[] {
                "/path:", "/path/2:", "/path:GET", "/path:POST", 
            }));

            var objAttrs = typeof(TypeIdWithMultipleAttributes).AllAttributes();
            values = objAttrs.ToList().ConvertAll(x => x.ToString());

            Assert.That(values, Is.EquivalentTo(new[] {
                "/path:", "/path/2:", "/path:GET", "/path:POST", 
            }));

            objAttrs = typeof(TypeIdWithMultipleAttributes).AllAttributes(typeof(RouteTypeIdAttribute));
            values = objAttrs.ToList().ConvertAll(x => x.ToString());

            Assert.That(values, Is.EquivalentTo(new[] {
                "/path:", "/path/2:", "/path:GET", "/path:POST", 
            }));
        }
    }

    [TestFixture]
    public class RuntimeAttributesTests
    {
        [Test]
        public void Can_add_to_Multiple_Default_Attributes()
        {
            typeof (DefaultWithMultipleAttributes).AddAttributes(
                new RouteDefaultAttribute("/path-add"),
                new RouteDefaultAttribute("/path-add", "GET"));

            var attrs = typeof(DefaultWithMultipleAttributes).AllAttributes<RouteDefaultAttribute>();
            Assert.That(attrs.Length, Is.EqualTo(6));

            var values = attrs.ToList().ConvertAll(x => x.ToString());

            Assert.That(values, Is.EquivalentTo(new[] {
                "/path:", "/path/2:", "/path:GET", "/path:POST", 
                "/path-add:", "/path-add:GET",
            }));

            var objAttrs = typeof(DefaultWithMultipleAttributes).AllAttributes();
            values = objAttrs.ToList().ConvertAll(x => x.ToString());

            Assert.That(values, Is.EquivalentTo(new[] {
                "/path:", "/path/2:", "/path:GET", "/path:POST", 
                "/path-add:", "/path-add:GET",
            }));

            objAttrs = typeof(DefaultWithMultipleAttributes).AllAttributes(typeof(RouteDefaultAttribute));
            values = objAttrs.ToList().ConvertAll(x => x.ToString());

            Assert.That(values, Is.EquivalentTo(new[] {
                "/path:", "/path/2:", "/path:GET", "/path:POST", 
                "/path-add:", "/path-add:GET",
            }));
        }

        [Test]
        public void Does_get_Multiple_TypeId_Attributes()
        {
            typeof(TypeIdWithMultipleAttributes).AddAttributes(
                new RouteTypeIdAttribute("/path-add"),
                new RouteTypeIdAttribute("/path-add", "GET"));

            var attrs = typeof(TypeIdWithMultipleAttributes).AllAttributes<RouteTypeIdAttribute>();
            Assert.That(attrs.Length, Is.EqualTo(6));

            var values = attrs.ToList().ConvertAll(x => x.ToString());

            Assert.That(values, Is.EquivalentTo(new[] {
                "/path:", "/path/2:", "/path:GET", "/path:POST", 
                "/path-add:", "/path-add:GET",
            }));

            var objAttrs = typeof(TypeIdWithMultipleAttributes).AllAttributes();
            values = objAttrs.ToList().ConvertAll(x => x.ToString());

            Assert.That(values, Is.EquivalentTo(new[] {
                "/path:", "/path/2:", "/path:GET", "/path:POST", 
                "/path-add:", "/path-add:GET",
            }));

            objAttrs = typeof(TypeIdWithMultipleAttributes).AllAttributes(typeof(RouteTypeIdAttribute));
            values = objAttrs.ToList().ConvertAll(x => x.ToString());

            Assert.That(values, Is.EquivalentTo(new[] {
                "/path:", "/path/2:", "/path:GET", "/path:POST", 
                "/path-add:", "/path-add:GET",
            }));
        }
    }

    [RouteTypeId("/path")]
    public class TypeIdWithSingleAttribute { }

    [RouteTypeId("/path")]
    [RouteTypeId("/path/2")]
    [RouteTypeId("/path", "GET")]
    [RouteTypeId("/path", "POST")]
    public class TypeIdWithMultipleAttributes { }

    [RouteDefault("/path")]
    public class DefaultWithSingleAttribute { }

    [RouteDefault("/path")]
    [RouteDefault("/path/2")]
    [RouteDefault("/path", "GET")]
    [RouteDefault("/path", "POST")]
    public class DefaultWithMultipleAttributes { }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RouteTypeIdAttribute : Attribute
    {
        public RouteTypeIdAttribute(string path) : this(path, null) {}
        public RouteTypeIdAttribute(string path, string verbs)
        {
            Path = path;
            Verbs = verbs;
        }

        public string Path { get; set; }
        public string Verbs { get; set; }

        public override object TypeId
        {
            get
            {
                return (Path ?? "")
                    + (Verbs ?? "");
            }
        }

        public override string ToString()
        {
            return "{0}:{1}".Fmt(Path, Verbs);
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RouteDefaultAttribute : Attribute
    {
        public RouteDefaultAttribute(string path) : this(path, null) {}
        public RouteDefaultAttribute(string path, string verbs)
        {
            Path = path;
            Verbs = verbs;
        }

        public string Path { get; set; }
        public string Verbs { get; set; }

        public override string ToString()
        {
            return "{0}:{1}".Fmt(Path, Verbs);
        }
    }

}