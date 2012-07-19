namespace ServiceStack.Text.Tests
{
    using System;
    using NUnit.Framework;

    public class EntityWithDateTimeOffsetTests
    {
        [Test]
        public void CanSerializableDateTimeOffsetField()
        {
            var model = new SampleModel { Id = 1, Date = new DateTimeOffset(2012, 6, 27, 11, 26, 04, 524, TimeSpan.FromHours(7)) };

            var s = JsonSerializer.SerializeToString(model);

            var afterModel = JsonSerializer.DeserializeFromString<SampleModel>("{\"Id\":1,\"Date\":\"\\/Date(1340771164524+0700)\\/\"}");

            Assert.AreEqual(model.Date, afterModel.Date);
        }

        public class SampleModel
        {
            public int Id { get; set; }

            public DateTimeOffset Date { get; set; }
        }
    }
}
