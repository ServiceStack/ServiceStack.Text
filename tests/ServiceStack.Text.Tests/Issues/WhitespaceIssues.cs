using NUnit.Framework;

namespace ServiceStack.Text.Tests.Issues
{
    public class WhitespaceIssues
    {
        [Test]
        public void Does_deserialize_JsonObject_empty_string()
        {
            var json = "{\"Name\":\"\"}"; 
            var obj = json.FromJson<JsonObject>(); 
            var name = obj.Get("Name");
            Assert.That(name, Is.EqualTo(""));
        }

        [Test]
        public void Does_deserialize_empty_string_to_object()
        {
            var json = "\"\"";
            var obj = json.FromJson<object>();
            Assert.That(obj, Is.EqualTo(""));
        }
    }
}