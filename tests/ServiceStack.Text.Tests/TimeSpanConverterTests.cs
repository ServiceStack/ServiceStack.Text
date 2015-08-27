using System;
using NUnit.Framework;
using ServiceStack.Text.Support;

namespace ServiceStack.Text.Tests
{
    [TestFixture]
    public class TimeSpanConverterTests
    {
        [Test]
        public void Can_Serialize_TimeSpan()
        {
            Assert.That(TimeSpanConverter.ToXsdDuration(new TimeSpan(1, 0, 0, 0)), Is.EqualTo("P1D"));
            Assert.That(TimeSpanConverter.ToXsdDuration(new TimeSpan(1, 0, 0)), Is.EqualTo("PT1H"));
            Assert.That(TimeSpanConverter.ToXsdDuration(new TimeSpan(0, 1, 0)), Is.EqualTo("PT1M"));
            Assert.That(TimeSpanConverter.ToXsdDuration(new TimeSpan(0, 0, 1)), Is.EqualTo("PT1S"));
            Assert.That(TimeSpanConverter.ToXsdDuration(new TimeSpan(0, 0, 0, 0, 1)), Is.EqualTo("PT0.001S"));
            Assert.That(TimeSpanConverter.ToXsdDuration(new TimeSpan(1, 1, 1, 1, 1)), Is.EqualTo("P1DT1H1M1.001S"));

            Assert.That(TimeSpanConverter.ToXsdDuration(new TimeSpan(1)), Is.EqualTo("PT0.0000001S"));
            Assert.That(TimeSpanConverter.ToXsdDuration(TimeSpan.Zero), Is.EqualTo("PT0S"));
            Assert.That(TimeSpanConverter.ToXsdDuration(new DateTime(2010,1,1) - new DateTime(2000,1,1)), Is.EqualTo("P3653D"));
        }

        [Test]
        public void Can_deserialize_TimeSpan()
        {
            Assert.That(TimeSpanConverter.FromXsdDuration("P1D"), Is.EqualTo(new TimeSpan(1, 0, 0, 0)));
            Assert.That(TimeSpanConverter.FromXsdDuration("PT1H"), Is.EqualTo(new TimeSpan(1, 0, 0)));
            Assert.That(TimeSpanConverter.FromXsdDuration("PT1M"), Is.EqualTo(new TimeSpan(0, 1, 0)));
            Assert.That(TimeSpanConverter.FromXsdDuration("PT1S"), Is.EqualTo(new TimeSpan(0, 0, 1)));
            Assert.That(TimeSpanConverter.FromXsdDuration("PT0.001S"), Is.EqualTo(new TimeSpan(0, 0, 0, 0, 1)));
            Assert.That(TimeSpanConverter.FromXsdDuration("P1DT1H1M1.001S"), Is.EqualTo(new TimeSpan(1, 1, 1, 1, 1)));

            Assert.That(TimeSpanConverter.FromXsdDuration("PT0.0000001S"), Is.EqualTo(new TimeSpan(1)));
            Assert.That(TimeSpanConverter.FromXsdDuration("PT0S"), Is.EqualTo(TimeSpan.Zero));
            Assert.That(TimeSpanConverter.FromXsdDuration("P3650D"), Is.EqualTo(TimeSpan.FromDays(3650)));
        }
    }
}
