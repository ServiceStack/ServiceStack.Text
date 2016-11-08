using System;
using NUnit.Framework;

namespace ServiceStack.Text.Tests
{
    [TestFixture]
    public class DateTimeExtensionsTests
    {
        [TestCase]
        public void LastMondayTest()
        {
            var monday = new DateTime(2013, 04, 15);

            var lastMonday = DateTimeExtensions.LastMonday(monday);

            Assert.AreEqual(monday, lastMonday);
        } 

        [Test]
        public void Can_convert_DateTime_MaxValue()
        {
            var date = DateTime.MaxValue.ToUnixTime();
            DateTime dt = date.FromUnixTime();

            Assert.That(dt, Is.EqualTo(DateTime.MaxValue).Within(TimeSpan.FromSeconds(1)));
        }
    }
}