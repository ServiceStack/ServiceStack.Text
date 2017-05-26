using NUnit.Framework;
using ServiceStack.Text.Support;
using System;
using System.Collections.Generic;
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
        public void Can_parse_invalid_int32()
        {
            foreach (var data in Parse_Invalid_TestData())
            {
                Assert.Throws((Type)data[3], () => new StringSegment((string) data[0]).ParseInt32());
            }
        }


        //ivalid tests data from 
        //https://github.com/dotnet/corefx/blob/df8d8ac7c49e6c4acdce2ea684d8815be5da6a25/src/System.Runtime/tests/System/Int32Tests.cs#L150
        public static IEnumerable<object[]> Parse_Invalid_TestData()
        {
            // String is null, empty or entirely whitespace
            yield return new object[] { null, NumberStyles.Integer, null, typeof(ArgumentNullException) };
            yield return new object[] { null, NumberStyles.Any, null, typeof(ArgumentNullException) };
            yield return new object[] { "", NumberStyles.Integer, null, typeof(FormatException) };
            yield return new object[] { "", NumberStyles.Any, null, typeof(FormatException) };
            yield return new object[] { " \t \n \r ", NumberStyles.Integer, null, typeof(FormatException) };
            yield return new object[] { " \t \n \r ", NumberStyles.Any, null, typeof(FormatException) };

            // String is garbage
            yield return new object[] { "Garbage", NumberStyles.Integer, null, typeof(FormatException) };
            yield return new object[] { "Garbage", NumberStyles.Any, null, typeof(FormatException) };

            // String has leading zeros
            yield return new object[] { "\0\0123", NumberStyles.Integer, null, typeof(FormatException) };
            yield return new object[] { "\0\0123", NumberStyles.Any, null, typeof(FormatException) };

            // String has internal zeros
            yield return new object[] { "1\023", NumberStyles.Integer, null, typeof(FormatException) };
            yield return new object[] { "1\023", NumberStyles.Any, null, typeof(FormatException) };

            // Integer doesn't allow hex, exponents, paretheses, currency, thousands, decimal
            yield return new object[] { "abc", NumberStyles.Integer, null, typeof(FormatException) };
            yield return new object[] { "1E23", NumberStyles.Integer, null, typeof(FormatException) };
            yield return new object[] { "(123)", NumberStyles.Integer, null, typeof(FormatException) };
            yield return new object[] { 1000.ToString("C0"), NumberStyles.Integer, null, typeof(FormatException) };
            yield return new object[] { 1000.ToString("N0"), NumberStyles.Integer, null, typeof(FormatException) };
            yield return new object[] { 678.90.ToString("F2"), NumberStyles.Integer, null, typeof(FormatException) };

            // HexNumber
            yield return new object[] { "0xabc", NumberStyles.HexNumber, null, typeof(FormatException) };
            yield return new object[] { "&habc", NumberStyles.HexNumber, null, typeof(FormatException) };
            yield return new object[] { "G1", NumberStyles.HexNumber, null, typeof(FormatException) };
            yield return new object[] { "g1", NumberStyles.HexNumber, null, typeof(FormatException) };
            yield return new object[] { "+abc", NumberStyles.HexNumber, null, typeof(FormatException) };
            yield return new object[] { "-abc", NumberStyles.HexNumber, null, typeof(FormatException) };

            // AllowLeadingSign
            yield return new object[] { "+", NumberStyles.AllowLeadingSign, null, typeof(FormatException) };
            yield return new object[] { "-", NumberStyles.AllowLeadingSign, null, typeof(FormatException) };
            yield return new object[] { "+-123", NumberStyles.AllowLeadingSign, null, typeof(FormatException) };
            yield return new object[] { "-+123", NumberStyles.AllowLeadingSign, null, typeof(FormatException) };
            yield return new object[] { "- 123", NumberStyles.AllowLeadingSign, null, typeof(FormatException) };
            yield return new object[] { "+ 123", NumberStyles.AllowLeadingSign, null, typeof(FormatException) };

            // AllowTrailingSign
            yield return new object[] { "123-+", NumberStyles.AllowTrailingSign, null, typeof(FormatException) };
            yield return new object[] { "123+-", NumberStyles.AllowTrailingSign, null, typeof(FormatException) };
            yield return new object[] { "123 -", NumberStyles.AllowTrailingSign, null, typeof(FormatException) };
            yield return new object[] { "123 +", NumberStyles.AllowTrailingSign, null, typeof(FormatException) };

            // Parentheses has priority over CurrencySymbol and PositiveSign
            NumberFormatInfo currencyNegativeParenthesesFormat = new NumberFormatInfo()
            {
                CurrencySymbol = "(",
                PositiveSign = "))"
            };
            yield return new object[] { "(100))", NumberStyles.AllowParentheses | NumberStyles.AllowCurrencySymbol | NumberStyles.AllowTrailingSign, currencyNegativeParenthesesFormat, typeof(FormatException) };

            // AllowTrailingSign and AllowLeadingSign
            yield return new object[] { "+123+", NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingSign, null, typeof(FormatException) };
            yield return new object[] { "+123-", NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingSign, null, typeof(FormatException) };
            yield return new object[] { "-123+", NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingSign, null, typeof(FormatException) };
            yield return new object[] { "-123-", NumberStyles.AllowLeadingSign | NumberStyles.AllowTrailingSign, null, typeof(FormatException) };

            // AllowLeadingSign and AllowParentheses
            yield return new object[] { "-(1000)", NumberStyles.AllowLeadingSign | NumberStyles.AllowParentheses, null, typeof(FormatException) };
            yield return new object[] { "(-1000)", NumberStyles.AllowLeadingSign | NumberStyles.AllowParentheses, null, typeof(FormatException) };

            // Not in range of Int32
            yield return new object[] { "2147483648", NumberStyles.Any, null, typeof(OverflowException) };
            yield return new object[] { "2147483648", NumberStyles.Integer, null, typeof(OverflowException) };
            yield return new object[] { "-2147483649", NumberStyles.Any, null, typeof(OverflowException) };
            yield return new object[] { "-2147483649", NumberStyles.Integer, null, typeof(OverflowException) };

            yield return new object[] { "9223372036854775808", NumberStyles.Integer, null, typeof(OverflowException) };
            yield return new object[] { "-9223372036854775809", NumberStyles.Integer, null, typeof(OverflowException) };
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
            Assert.Throws<FormatException>(() => new StringSegment(".e2").ParseDecimal());
            Assert.That(new StringSegment("9.e+000027").ParseDecimal(), Is.EqualTo(decimal.Parse("9.e+000027", NumberStyles.Float, CultureInfo.InvariantCulture)));

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
