using System;
using NUnit.Framework;

namespace ServiceStack.Text.Tests
{
    public class SpanTests
    {
        [Test]
        public void Can_use_Span()
        {
            var a = "foo bar".AsSpan();

            var foo = a.Slice(0,3).ToArray();

            Assert.That(foo, Is.EqualTo("foo".ToCharArray()));
        }
    }
}