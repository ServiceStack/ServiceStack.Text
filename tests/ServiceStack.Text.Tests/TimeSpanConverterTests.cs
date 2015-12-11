using System;
using NUnit.Framework;
using ServiceStack.Text.Support;

namespace ServiceStack.Text.Tests
{
    [TestFixture]
    public class TimeSpanConverterTests
    {
        private readonly TimeSpan oneTick = new TimeSpan(1);
        private readonly TimeSpan oneDay = new TimeSpan(1, 0, 0, 0);
        private readonly TimeSpan oneHour = new TimeSpan(1, 0, 0);
        private readonly TimeSpan oneMinute = new TimeSpan(0, 1, 0);
        private readonly TimeSpan oneSecond = new TimeSpan(0, 0, 1);
        private readonly TimeSpan oneMilliSecond = new TimeSpan(0, 0, 0, 0, 1);
        private readonly TimeSpan oneDayHourMinuteSecondMilliSecond = new TimeSpan(1, 1, 1, 1, 1);
        private readonly TimeSpan threeThousandSixHundredAndFiveDays = TimeSpan.FromDays(3605);
        private readonly TimeSpan arbitraryTimeSpan = new TimeSpan(1, 2, 3, 4, 567).Add(TimeSpan.FromTicks(1));

        [Test]
        public void Can_Serialize_TimeSpan()
        {
            Assert.That(TimeSpanConverter.ToXsdDuration(oneDay), Is.EqualTo("P1D"));
            Assert.That(TimeSpanConverter.ToXsdDuration(oneHour), Is.EqualTo("PT1H"));
            Assert.That(TimeSpanConverter.ToXsdDuration(oneMinute), Is.EqualTo("PT1M"));
            Assert.That(TimeSpanConverter.ToXsdDuration(oneSecond), Is.EqualTo("PT1S"));
            Assert.That(TimeSpanConverter.ToXsdDuration(oneMilliSecond), Is.EqualTo("PT0.001S"));
            Assert.That(TimeSpanConverter.ToXsdDuration(oneDayHourMinuteSecondMilliSecond), Is.EqualTo("P1DT1H1M1.001S"));
            Assert.That(TimeSpanConverter.ToXsdDuration(arbitraryTimeSpan), Is.EqualTo("P1DT2H3M4.5670001S"));

            Assert.That(TimeSpanConverter.ToXsdDuration(-oneDay), Is.EqualTo("-P1D"));
            Assert.That(TimeSpanConverter.ToXsdDuration(-oneHour), Is.EqualTo("-PT1H"));
            Assert.That(TimeSpanConverter.ToXsdDuration(-oneMinute), Is.EqualTo("-PT1M"));
            Assert.That(TimeSpanConverter.ToXsdDuration(-oneSecond), Is.EqualTo("-PT1S"));
            Assert.That(TimeSpanConverter.ToXsdDuration(-oneMilliSecond), Is.EqualTo("-PT0.001S"));
            Assert.That(TimeSpanConverter.ToXsdDuration(-arbitraryTimeSpan), Is.EqualTo("-P1DT2H3M4.5670001S"));

            Assert.That(TimeSpanConverter.ToXsdDuration(oneTick), Is.EqualTo("PT0.0000001S"));
            Assert.That(TimeSpanConverter.ToXsdDuration(-oneTick), Is.EqualTo("-PT0.0000001S"));

            Assert.That(TimeSpanConverter.ToXsdDuration(TimeSpan.Zero), Is.EqualTo("PT0S"));

            Assert.That(TimeSpanConverter.ToXsdDuration(threeThousandSixHundredAndFiveDays), Is.EqualTo("P3605D"));
            Assert.That(TimeSpanConverter.ToXsdDuration(-threeThousandSixHundredAndFiveDays), Is.EqualTo("-P3605D"));
        }

        [Test]
        public void Can_deserialize_TimeSpan()
        {
            Assert.That(TimeSpanConverter.FromXsdDuration("P1D"), Is.EqualTo(oneDay));
            Assert.That(TimeSpanConverter.FromXsdDuration("PT1H"), Is.EqualTo(oneHour));
            Assert.That(TimeSpanConverter.FromXsdDuration("PT1M"), Is.EqualTo(oneMinute));
            Assert.That(TimeSpanConverter.FromXsdDuration("PT1S"), Is.EqualTo(oneSecond));
            Assert.That(TimeSpanConverter.FromXsdDuration("PT0.001S"), Is.EqualTo(oneMilliSecond));
            Assert.That(TimeSpanConverter.FromXsdDuration("P1DT1H1M1.001S"), Is.EqualTo(oneDayHourMinuteSecondMilliSecond));
            Assert.That(TimeSpanConverter.FromXsdDuration("P1DT2H3M4.5670001S"), Is.EqualTo(arbitraryTimeSpan));

            Assert.That(TimeSpanConverter.FromXsdDuration("-P1D"), Is.EqualTo(-oneDay));
            Assert.That(TimeSpanConverter.FromXsdDuration("-PT1H"), Is.EqualTo(-oneHour));
            Assert.That(TimeSpanConverter.FromXsdDuration("-PT1M"), Is.EqualTo(-oneMinute));
            Assert.That(TimeSpanConverter.FromXsdDuration("-PT1S"), Is.EqualTo(-oneSecond));
            Assert.That(TimeSpanConverter.FromXsdDuration("-PT0.001S"), Is.EqualTo(-oneMilliSecond));
            Assert.That(TimeSpanConverter.FromXsdDuration("-P1DT1H1M1.001S"), Is.EqualTo(-oneDayHourMinuteSecondMilliSecond));
            Assert.That(TimeSpanConverter.FromXsdDuration("-P1DT2H3M4.5670001S"), Is.EqualTo(-arbitraryTimeSpan));

            Assert.That(TimeSpanConverter.FromXsdDuration("PT0.0000001S"), Is.EqualTo(oneTick));
            Assert.That(TimeSpanConverter.FromXsdDuration("-PT0.0000001S"), Is.EqualTo(-oneTick));

            Assert.That(TimeSpanConverter.FromXsdDuration("PT0S"), Is.EqualTo(TimeSpan.Zero));

            Assert.That(TimeSpanConverter.FromXsdDuration("P3605D"), Is.EqualTo(threeThousandSixHundredAndFiveDays));
            Assert.That(TimeSpanConverter.FromXsdDuration("-P3605D"), Is.EqualTo(-threeThousandSixHundredAndFiveDays));
        }
    }
}
