using NUnit.Framework;

using ServiceStack.Text.Support;

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
            int pos = 0;
            StringSegment line;
            while ((line = buf.ReadNextLine(ref pos)).HasValue)
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

    }
}