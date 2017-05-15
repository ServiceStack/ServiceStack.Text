using NUnit.Framework;
using ServiceStack.Text.Support;
using System;
#if NETCORE
using Microsoft.Extensions.Primitives;
#endif

namespace ServiceStack.Text.Tests.Support
{
    [TestFixture]
    public class StringSegmentParse
    {
        [Test]
        public void Can_parse_int32()
        {
            Assert.That(new StringSegment("0").ParseInt32(), Is.EqualTo(0));
            Assert.That(new StringSegment("1").ParseInt32(), Is.EqualTo(1));
            Assert.That(new StringSegment(int.MaxValue.ToString()).ParseInt32(), Is.EqualTo(int.MaxValue));
            Assert.That(new StringSegment(int.MinValue.ToString()).ParseInt32(), Is.EqualTo(int.MinValue));
            Assert.Throws<FormatException>(() => new StringSegment("01").ParseInt32());
            Assert.That(new StringSegment("234").ParseInt32(), Is.EqualTo(234));
            Assert.That(new StringSegment("    234  ").ParseInt32(), Is.EqualTo(234));
            Assert.That(new StringSegment("234  ").ParseInt32(), Is.EqualTo(234));
            Assert.That(new StringSegment("   234").ParseInt32(), Is.EqualTo(234));
            Assert.That(new StringSegment("   -234    ").ParseInt32(), Is.EqualTo(-234));
            Assert.Throws<FormatException>(() => new StringSegment("   - 234    ").ParseInt32());
            Assert.Throws<FormatException>(() => new StringSegment("   2.34    ").ParseInt32());
            Assert.Throws<OverflowException>(() => new StringSegment("12345678901234567890").ParseInt32());
            Assert.Throws<FormatException>(() => new StringSegment("abbababab").ParseInt32());
            Assert.Throws<FormatException>(() => new StringSegment("x10").ParseInt32());
            Assert.Throws<FormatException>(() => new StringSegment("    1234  123").ParseInt32());
        }

    }
}
