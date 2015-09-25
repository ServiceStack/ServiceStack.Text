using System.Globalization;
using NUnit.Framework;
using ServiceStack.Text.Tests.Support;

namespace ServiceStack.Text.Tests
{
    [TestFixture]
    public class CustomCultureInfoTests
        : TestBase
    {
        [Test]
        public void Does_not_use_custom_decimal()
        {
            CsvConfig.RealNumberCultureInfo = CultureInfo.CreateSpecificCulture("nl-NL");

            var num = new NumberTypes
            {
                Int = 1111,
                Float = 2222.2222f,
                Double = 3333.3333,
                Decimal = 4444.4444M,
            };

            num.ToJson().Print();
            num.ToJsv().Print();
            num.ToCsv().Print();

            Assert.That(num.ToJson(), Is.StringContaining("4444.4444"));
            Assert.That(num.ToJsv(), Is.StringContaining("4444.4444"));
            Assert.That(num.ToCsv(), Is.StringContaining("4444,4444"));

            CsvConfig.RealNumberCultureInfo = null;
        }
    }
}