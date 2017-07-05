using NUnit.Framework;

#if NETSTANDARD1_1
using Microsoft.Extensions.Primitives;
#endif

namespace ServiceStack.Text.Tests
{
    public class StringSegmentTests
    {
        [Test]
        public void Can_read_lines_with_ReadNextLine()
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

    }
}