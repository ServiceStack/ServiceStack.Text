using System;
using System.Collections.Generic;
using NUnit.Framework;

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
        public void Can_not_detect_null_empty_string_memory()
        {
            var n = ((string)null).AsMemory();
            var e = "".AsMemory();
            
            Assert.That(!n.Equals(e)); //null + "" memory are not equal

            Assert.That(n.Equals(((string)null).AsMemory()));
            Assert.That(e.Equals("".AsMemory()));
        }
    }
}