using NUnit.Framework;

#if NETSTANDARD2_0
using Microsoft.Extensions.Primitives;
#endif

namespace ServiceStack.Text.Tests
{
    public class StringSegmentTests
    {
        [Test]
        public void Can_read_lines_with_TryReadLine()
        {
            var str = "A\nB\r\nC\rD\r\n";
            var expected = new[] {"A", "B", "C", "D"};

            var i = 0;
            var buf = new StringSegment(str);
            var pos = 0;
            while (buf.TryReadLine(out StringSegment line, ref pos))
            {
                Assert.That(line.ToString(), Is.EqualTo(expected[i++]));
            }

            Assert.That(pos, Is.EqualTo(buf.Length));
            Assert.That(i, Is.EqualTo(expected.Length));
        }

        [Test]
        public void Can_read_parts_with_TryReadPart()
        {
            var str = "A.BB.CCC.DD DD";
            var expected = new[] { "A", "BB", "CCC", "DD DD" };

            var i = 0;
            var buf = new StringSegment(str);
            var pos = 0;
            while (buf.TryReadPart(".", out StringSegment part, ref pos))
            {
                Assert.That(part.ToString(), Is.EqualTo(expected[i++]));
            }

            Assert.That(pos, Is.EqualTo(buf.Length));
            Assert.That(i, Is.EqualTo(expected.Length));

            str = "A||BB||CCC||DD DD";

            i = 0;
            buf = new StringSegment(str);
            pos = 0;
            while (buf.TryReadPart("||", out StringSegment part, ref pos))
            {
                Assert.That(part.ToString(), Is.EqualTo(expected[i++]));
            }

            Assert.That(pos, Is.EqualTo(buf.Length));
            Assert.That(i, Is.EqualTo(expected.Length));
        }

        [Test]
        public void Can_SplitOnFirst()
        {
            var parts = new StringSegment("a:b:c").SplitOnFirst(':');
            Assert.That(parts[0].ToString(), Is.EqualTo("a"));
            Assert.That(parts[1].ToString(), Is.EqualTo("b:c"));

            parts = new StringSegment("a::b::c").SplitOnFirst("::");
            Assert.That(parts[0].ToString(), Is.EqualTo("a"));
            Assert.That(parts[1].ToString(), Is.EqualTo("b::c"));
        }

        [Test]
        public void Can_SplitOnLast()
        {
            var parts = new StringSegment("a:b:c").SplitOnLast(':');
            Assert.That(parts[0].ToString(), Is.EqualTo("a:b"));
            Assert.That(parts[1].ToString(), Is.EqualTo("c"));

            parts = new StringSegment("a::b::c").SplitOnLast("::");
            Assert.That(parts[0].ToString(), Is.EqualTo("a::b"));
            Assert.That(parts[1].ToString(), Is.EqualTo("c"));
        }

        [Test]
        public void Does_convert_to_UTF8_bytes()
        {
            var str = "this is a UTF8 test string";
            var seg = str.ToStringSegment();
            var ut8Test = seg.Subsegment(seg.IndexOf("UTF8"), "UTF8 test".Length);

            var segBytes = ut8Test.ToUtf8Bytes();
            Assert.That(segBytes, Is.EquivalentTo("UTF8 test".ToUtf8Bytes()));
        }

        [Test]
        public void Does_parse_into_preferred_signed_number_type()
        {
            Assert.That(int.MinValue.ToString().ToStringSegment().ParseSignedInteger() is int);
            Assert.That(int.MaxValue.ToString().ToStringSegment().ParseSignedInteger() is int);
            Assert.That((int.MinValue - (long)1).ToString().ToStringSegment().ParseSignedInteger() is long);
            Assert.That((int.MaxValue + (long)1).ToString().ToStringSegment().ParseSignedInteger() is long);
        }

    }
}