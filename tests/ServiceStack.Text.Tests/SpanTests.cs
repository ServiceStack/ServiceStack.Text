using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using ServiceStack.Text.Pools;

namespace ServiceStack.Text.Tests
{
    public class SpanTests
    {
        [Test]
        public void Can_use_Span()
        {
            ReadOnlySpan<char> a = "foo bar".AsSpan();
            
            var foo = a.Slice(0,3).ToArray();

            Assert.That(foo, Is.EqualTo("foo".ToCharArray()));
        }

        [Test]
        public void Can_not_detect_null_empty_string_spans()
        {
            var n = ((string)null).AsSpan();
            var e = "".AsSpan();
            
            Assert.That(n.SequenceEqual(e)); //null + "" spans are considered equal
        }

        [Test]
        public void Can_read_lines_with_TryReadLine_using_Span()
        {
            var str = "A\nB\r\nC\rD\r\n";
            var expected = new[] {"A", "B", "C", "D"};

            var i = 0;
            var buf = str.AsSpan();
            var pos = 0;
            while (buf.TryReadLine(out ReadOnlySpan<char> line, ref pos))
            {
                Assert.That(line.ToString(), Is.EqualTo(expected[i++]));
            }

            Assert.That(pos, Is.EqualTo(buf.Length));
            Assert.That(i, Is.EqualTo(expected.Length));
        }

        [Test]
        public void Can_read_parts_with_TryReadPart_using_Span()
        {
            var str = "A.BB.CCC.DD DD";
            var expected = new[] { "A", "BB", "CCC", "DD DD" };

            var i = 0;
            var buf = str.AsSpan();
            var pos = 0;
            while (buf.TryReadPart(".".AsSpan(), out ReadOnlySpan<char> part, ref pos))
            {
                Assert.That(part.ToString(), Is.EqualTo(expected[i++]));
            }

            Assert.That(pos, Is.EqualTo(buf.Length));
            Assert.That(i, Is.EqualTo(expected.Length));

            str = "A||BB||CCC||DD DD";

            i = 0;
            buf = str.AsSpan();
            pos = 0;
            while (buf.TryReadPart("||".AsSpan(), out ReadOnlySpan<char> part, ref pos))
            {
                Assert.That(part.ToString(), Is.EqualTo(expected[i++]));
            }

            Assert.That(pos, Is.EqualTo(buf.Length));
            Assert.That(i, Is.EqualTo(expected.Length));
        }

        [Test]
        public void Can_SplitOnFirst_using_Span()
        {
            "a:b:c".AsSpan().SplitOnFirst(':', out var first, out var last);
            Assert.That(first.ToString(), Is.EqualTo("a"));
            Assert.That(last.ToString(), Is.EqualTo("b:c"));

            "a::b::c".AsSpan().SplitOnFirst("::".AsSpan(), out first, out last);
            Assert.That(first.ToString(), Is.EqualTo("a"));
            Assert.That(last.ToString(), Is.EqualTo("b::c"));
        }

        [Test]
        public void Can_SplitOnLast_using_Span()
        {
            "a:b:c".AsSpan().SplitOnLast(':', out var first, out var last);
            Assert.That(first.ToString(), Is.EqualTo("a:b"));
            Assert.That(last.ToString(), Is.EqualTo("c"));

            "a::b::c".AsSpan().SplitOnLast("::".AsSpan(), out first, out last);
            Assert.That(first.ToString(), Is.EqualTo("a::b"));
            Assert.That(last.ToString(), Is.EqualTo("c"));
        }

        [Test]
        public void Can_ToUtf8_and_FromUtf8_using_Span()
        {
            foreach (var test in Utf8Case.Source)
            {
                ReadOnlyMemory<byte> bytes = test.expectedString.AsSpan().ToUtf8();
                Assert.That(bytes.Length, Is.EqualTo(test.count));
                Assert.That(bytes.ToArray(), Is.EquivalentTo(test.expectedBytes));

                ReadOnlyMemory<char> chars = bytes.FromUtf8();
                Assert.That(chars.Length, Is.EqualTo(test.expectedString.Length));
                Assert.That(chars.ToString(), Is.EqualTo(test.expectedString));
            }
        }

        [Test]
        public void Can_ToUtf8_and_FromUtf8_in_place_using_Span()
        {
            foreach (var test in Utf8Case.Source)
            {
                var chars = test.expectedString.AsSpan();
                Memory<byte> buffer = BufferPool.GetBuffer(MemoryProvider.Instance.GetUtf8ByteCount(chars));
                var bytesWritten = MemoryProvider.Instance.ToUtf8(chars, buffer.Span);
                var bytes = buffer.Slice(0, bytesWritten);
                
                Assert.That(bytes.Length, Is.EqualTo(test.count));
                Assert.That(bytes.ToArray(), Is.EquivalentTo(test.expectedBytes));

                Memory<char> charBuff = CharPool.GetBuffer(MemoryProvider.Instance.GetUtf8CharCount(bytes.Span));
                var charsWritten = MemoryProvider.Instance.FromUtf8(bytes.Span, charBuff.Span);
                chars = charBuff.Slice(0, charsWritten).Span;

                Assert.That(chars.Length, Is.EqualTo(test.expectedString.Length));
                Assert.That(chars.ToString(), Is.EqualTo(test.expectedString));
            }
        }

        [Test]
        public async Task Can_deserialize_from_MemoryStream_using_Memory()
        {
            var from = new Person { Id = 1, Name = "FooBA\u0400R" };
            var json = from.ToJson();

            var ms = MemoryStreamFactory.GetStream(json.ToUtf8Bytes());

            var to = (Person)await MemoryProvider.Instance.DeserializeAsync(ms, typeof(Person), JsonSerializer.DeserializeFromSpan);
            
            Assert.That(to, Is.EqualTo(from));
        }
    }

    public class Utf8Case
    {
        public static Utf8Case[] Source = new[] {
            new Utf8Case(new byte[] {70, 111, 111, 66, 65, 208, 128, 82}, 0, 8, "FooBA\u0400R"),                
        };

        public byte[] expectedBytes;
        public int index;
        public int count;
        public string expectedString;
            
        public Utf8Case(byte[] expectedBytes, int index, int count, string expectedString)
        {
            this.expectedBytes = expectedBytes;
            this.index = index;
            this.count = count;
            this.expectedString = expectedString;
        }
    }

    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }

        protected bool Equals(Person other)
        {
            return Id == other.Id && string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Person) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Id * 397) ^ (Name != null ? Name.GetHashCode() : 0);
            }
        }
    }
   
}