using NUnit.Framework;
using ServiceStack.Text.Support;
using System;
using System.Globalization;
#if NETCORE
using Microsoft.Extensions.Primitives;
#endif

namespace ServiceStack.Text.Tests.Support
{
    [TestFixture]
    public class StringSegmentParseTests
    {
        [Test]
        public void Can_parse_int32()
        {
            Assert.That(new StringSegment("0").ParseInt32(), Is.EqualTo(0));
            Assert.That(new StringSegment("-0").ParseInt32(), Is.EqualTo(0));
            Assert.That(new StringSegment("1").ParseInt32(), Is.EqualTo(1));
            Assert.That(new StringSegment(int.MaxValue.ToString()).ParseInt32(), Is.EqualTo(int.MaxValue));
            Assert.That(new StringSegment(int.MinValue.ToString()).ParseInt32(), Is.EqualTo(int.MinValue));
            Assert.Throws<FormatException>(() => new StringSegment("01").ParseInt32());
            Assert.That(new StringSegment("234").ParseInt32(), Is.EqualTo(234));
            Assert.That(new StringSegment("    234  ").ParseInt32(), Is.EqualTo(234));
            Assert.That(new StringSegment("234  ").ParseInt32(), Is.EqualTo(234));
            Assert.That(new StringSegment("   234").ParseInt32(), Is.EqualTo(234));
            Assert.That(new StringSegment("   -234    ").ParseInt32(), Is.EqualTo(-234));
            Assert.Throws<FormatException>(() => new StringSegment("").ParseInt32());
            Assert.Throws<FormatException>(() => new StringSegment("01").ParseInt32());
            Assert.Throws<FormatException>(() => new StringSegment("-01").ParseInt32());
            Assert.Throws<FormatException>(() => new StringSegment("   - 234    ").ParseInt32());
            Assert.Throws<FormatException>(() => new StringSegment("   2.34    ").ParseInt32());
            Assert.Throws<OverflowException>(() => new StringSegment("12345678901234567890").ParseInt32());
            Assert.Throws<FormatException>(() => new StringSegment("abbababab").ParseInt32());
            Assert.Throws<FormatException>(() => new StringSegment("x10").ParseInt32());
            Assert.Throws<FormatException>(() => new StringSegment("    1234  123").ParseInt32());
        }

        [Test]
        public void Can_parse_decimal()
        {
            Assert.That(new StringSegment("1234.5678").ParseDecimal(), Is.EqualTo(1234.5678m));
            Assert.That(new StringSegment("1234").ParseDecimal(), Is.EqualTo(1234m));
            Assert.Throws<FormatException>(() => new StringSegment(".").ParseDecimal());
            Assert.Throws<FormatException>(() => new StringSegment("").ParseDecimal());
            Assert.That(new StringSegment("0").ParseDecimal(), Is.EqualTo(0));
            Assert.That(new StringSegment("-0").ParseDecimal(), Is.EqualTo(0));
            Assert.That(new StringSegment("0.").ParseDecimal(), Is.EqualTo(0));
            Assert.That(new StringSegment("-0.").ParseDecimal(), Is.EqualTo(0));
            Assert.That(new StringSegment(".1").ParseDecimal(), Is.EqualTo(.1m));
            Assert.That(new StringSegment("-.1").ParseDecimal(), Is.EqualTo(-.1m));
            Assert.That(new StringSegment("10.001").ParseDecimal(), Is.EqualTo(10.001m));
            Assert.That(new StringSegment("  10.001").ParseDecimal(), Is.EqualTo(10.001m));
            Assert.That(new StringSegment("10.001  ").ParseDecimal(), Is.EqualTo(10.001m));
            Assert.That(new StringSegment(" 10.001  ").ParseDecimal(), Is.EqualTo(10.001m));
            Assert.That(new StringSegment("-10.001").ParseDecimal(), Is.EqualTo(-10.001m));
            //large
            Assert.That(new StringSegment("12345678901234567890").ParseDecimal(), Is.EqualTo(12345678901234567890m));
            Assert.That(new StringSegment("12345678901234567890.12").ParseDecimal(), Is.EqualTo(12345678901234567890.12m));
            Assert.That(new StringSegment(decimal.MaxValue.ToString(CultureInfo.InvariantCulture)).ParseDecimal(), Is.EqualTo(decimal.MaxValue));
            Assert.That(new StringSegment(decimal.MinValue.ToString(CultureInfo.InvariantCulture)).ParseDecimal(), Is.EqualTo(decimal.MinValue));

            //exponent
            Assert.That(new StringSegment("7.67e-6").ParseDecimal(), Is.EqualTo(7.67e-6f));
            Assert.That(new StringSegment("10.001E3").ParseDecimal(), Is.EqualTo(10001m));
            Assert.That(new StringSegment(".001e5").ParseDecimal(), Is.EqualTo(100m));
            Assert.That(new StringSegment("10.001E-2").ParseDecimal(), Is.EqualTo(0.10001m));
            Assert.That(new StringSegment("10.001e-8").ParseDecimal(), Is.EqualTo(0.00000010001m));
            Assert.That(new StringSegment("2.e2").ParseDecimal(), Is.EqualTo(200m));
            //Assert.Throws<FormatException>(() => new StringSegment(".e2").ParseDecimal());

            //allow thouthands
            Assert.That(new StringSegment("1,234.5678").ParseDecimal(true), Is.EqualTo(1234.5678m));
            Assert.Throws<FormatException>(() => new StringSegment(",1234.5678").ParseDecimal(true));

        }

        [Test]
        public void Can_parse_guid()
        {
            Assert.That(new StringSegment("{b6170a18-3dd7-4a9b-b5d6-21033b5ad162}").ParseGuid(), Is.EqualTo(new Guid("{b6170a18-3dd7-4a9b-b5d6-21033b5ad162}")));
            Assert.That(new StringSegment("b6170a18-3dd7-4a9b-b5d6-21033b5ad162").ParseGuid(), Is.EqualTo(new Guid("{b6170a18-3dd7-4a9b-b5d6-21033b5ad162}")));
            Assert.That(new StringSegment("b6170a183dd74a9bb5d621033b5ad162").ParseGuid(), Is.EqualTo(new Guid("{b6170a18-3dd7-4a9b-b5d6-21033b5ad162}")));
        }
    }
}
