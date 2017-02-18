using NUnit.Framework;
using ServiceStack.Text.Tests.Shared;

namespace ServiceStack.Text.Tests.JsvTests
{
    public class JsvBasicDataTests
    {
        [Test]
        public void Can_serialize_ModelWithFloatTypes()
        {
            var dto = new ModelWithFloatTypes
            {
                Float = 1.1f,
                Double = 2.2d,
                Decimal = 3.3m
            };

            var jsv = dto.ToJsv();
            Assert.That(jsv,Is.EqualTo("{Float:1.1,Double:2.2,Decimal:3.3}"));

            var fromJsv = jsv.FromJsv<ModelWithFloatTypes>();
            Assert.That(fromJsv, Is.EqualTo(dto));

            dto = new ModelWithFloatTypes
            {
                Float   = 1111111.11f,
                Double  = 2222222.22d,
                Decimal = 33333333.33m
            };

            jsv = dto.ToJsv();
            Assert.That(jsv, Is.EqualTo("{Float:1111111,Double:2222222.22,Decimal:33333333.33}"));

            fromJsv = jsv.FromJsv<ModelWithFloatTypes>();
            Assert.That(fromJsv, Is.EqualTo(dto));
        }
    }
}