namespace ServiceStack.Text.Tests
{
    using System;
    using NUnit.Framework;

    public class EntityWithDateTimeOffsetTests
    {
        [Test]
        public void CanSerializableDateTimeOffsetField()
        {
            var expectedModel = new SampleModel { Id = 1, Date = new DateTimeOffset(2012, 6, 27, 11, 26, 04, 524, TimeSpan.FromHours(7)) };

            var serializeModel = JsonSerializer.SerializeToString(expectedModel);

            Assert.AreEqual("{\"Id\":1,\"Date\":\"\\/Date(1340771164524+0700)\\/\"}", serializeModel);

            var deserializeModel = JsonSerializer.DeserializeFromString<SampleModel>(serializeModel);

            Assert.AreEqual(expectedModel.Id, deserializeModel.Id);
            Assert.AreEqual(expectedModel.Date, deserializeModel.Date);
        }

        public class SampleModel
        {
            public int Id { get; set; }

            public DateTimeOffset Date { get; set; }
        }
    }
}
