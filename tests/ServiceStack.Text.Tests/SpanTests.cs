using System;
using NUnit.Framework;

namespace ServiceStack.Text.Tests
{
    public class SpanTests
    {
        [Test]
        public void Can_use_Span()
        {
            //.AsReadOnlySpan() not available in System.Memory v4.4.0-preview1-25305-02 on NuGet yet
            ReadOnlySpan<char> a = "foo bar".AsSpan();

            var foo = a.Slice(0,3).ToArray();

            Assert.That(foo, Is.EqualTo("foo".ToCharArray()));
        }
    }
}