using NUnit.Framework;

namespace ServiceStack.Text.Tests
{
    [TestFixture]
    public class JsonObjectTests
    {
        private const string JsonCentroid = @"{""place"":{ ""woeid"":12345, ""placeTypeName"":""St\\ate"" } }";

        [Test]
        public void Can_dynamically_parse_JSON_with_escape_chars()
        {
            var placeTypeName = JsonObject.Parse(JsonCentroid).Object("place").Get("placeTypeName");
            Assert.That(placeTypeName, Is.EqualTo("St\\ate"));

            placeTypeName = JsonObject.Parse(JsonCentroid).Object("place").Get<string>("placeTypeName");
            Assert.That(placeTypeName, Is.EqualTo("St\\ate"));
        }

        [Test]
        public void Does_escape_string_access()
        {
            string test = "\"quoted string\"";
            var json = JsonSerializer.SerializeToString(new { a = test });
            var jsonObject = JsonObject.Parse(json);

            var actual = jsonObject["a"];
            Assert.That(actual, Is.EqualTo(test));
            Assert.That(jsonObject.Get("a"), Is.EqualTo(test));
            Assert.That(jsonObject.Get<string>("a"), Is.EqualTo(test));

            Assert.That(jsonObject.GetUnescaped("a"), Is.EqualTo(test.Replace("\"","\\\"")));
        }
    }
}