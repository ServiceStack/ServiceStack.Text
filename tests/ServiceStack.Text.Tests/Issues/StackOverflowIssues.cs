using System;
using NUnit.Framework;

namespace ServiceStack.Text.Tests.Issues
{
    public class StackOverflowIssues
    {
        public class MinTypes
        {
            public TimeSpan TimeSpan { get; set; }
        }

        [Test]
        public void Can_convert_min_TimeSpan()
        {
            var c1 = new MinTypes {
                TimeSpan = TimeSpan.MinValue,
            };
            var json = JsonSerializer.SerializeToString(c1, typeof(MinTypes));
            var dto = JsonSerializer.SerializeToString(c1, typeof(MinTypes));
            Assert.That(json, Is.EqualTo(dto));
        }
    }
}