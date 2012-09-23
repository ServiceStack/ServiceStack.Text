using NUnit.Framework;

namespace ServiceStack.Text.Tests.JsonTests
{
    [TestFixture]
    public class PropertyConventionTests : TestBase
    {
        [Test]
        public void Does_require_exact_match_by_default()
        {
            Assert.That(JsConfig.PropertyConvention, Is.EqualTo(JsonPropertyConvention.ExactMatch));
            const string bad = "{ \"total_count\":45, \"was_published\":true }";
            const string good = "{ \"TotalCount\":45, \"WasPublished\":true }";
            
            var actual = JsonSerializer.DeserializeFromString<Example>(bad);
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.TotalCount, Is.EqualTo(0));
            Assert.That(actual.WasPublished, Is.EqualTo(false));

            actual = JsonSerializer.DeserializeFromString<Example>(good);
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.TotalCount, Is.EqualTo(45));
            Assert.That(actual.WasPublished, Is.EqualTo(true));
        }
        
        [Test]
        public void Does_deserialize_from_inexact_source_when_lenient_convention_is_used()
        {
            JsConfig.PropertyConvention = JsonPropertyConvention.Lenient;
            const string bad = "{ \"total_count\":45, \"was_published\":true }";
            
            var actual = JsonSerializer.DeserializeFromString<Example>(bad);
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.TotalCount, Is.EqualTo(45));
            Assert.That(actual.WasPublished, Is.EqualTo(true));
            
            JsConfig.Reset();
        }

        public class Example
        {
            public int TotalCount { get; set; }
            public bool WasPublished { get; set; }
        }
    }
}