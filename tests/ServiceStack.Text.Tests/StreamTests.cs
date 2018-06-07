using System.IO;
using NUnit.Framework;

namespace ServiceStack.Text.Tests
{
    [TestFixture]
    public class StreamTests
    {
        [Test]
        public void Does_escape_string_when_serializing_to_TextWriter()
        {
            var expected = @"String with backslashes '\', 'single' and ""double quotes"", (along		with	other	special	symbols	like	tabs) wich may broke incorrect serializing/deserializing implementation ;)";

            var json = "\"String with backslashes '\\\\', 'single' and \\\"double quotes\\\", (along\\t\\twith\\tother\\tspecial\\tsymbols\\tlike\\ttabs) wich may broke incorrect serializing/deserializing implementation ;)\"";

            using (var ms = new MemoryStream())
            {
                var sw = new StreamWriter(ms);
                JsonSerializer.SerializeToWriter(expected, sw);
                sw.Flush();

                using (var sr = new StreamReader(ms))
                {
                    ms.Position = 0;
                    var ssJson = ms.ReadToEnd();
                    Assert.That(ssJson, Is.EqualTo(json));

                    ms.Position = 0;
                    var ssString = JsonSerializer.DeserializeFromReader(sr, typeof(string));
                    Assert.That(ssString, Is.EqualTo(expected));
                }
            }
        }

        [Test]
        public void Does_escape_string_when_serializing_to_Stream()
        {
            var expected = @"String with backslashes '\', 'single' and ""double quotes"", (along		with	other	special	symbols	like	tabs) wich may broke incorrect serializing/deserializing implementation ;)";

            var json = "\"String with backslashes '\\\\', 'single' and \\\"double quotes\\\", (along\\t\\twith\\tother\\tspecial\\tsymbols\\tlike\\ttabs) wich may broke incorrect serializing/deserializing implementation ;)\"";

            using (var ms = new MemoryStream())
            {
                JsonSerializer.SerializeToStream(expected, ms);
                var ssJson = ms.ReadToEnd();

                Assert.That(ssJson, Is.EqualTo(json));

                ms.Position = 0;
                var ssString = JsonSerializer.DeserializeFromStream(typeof(string), ms);

                Assert.That(ssString, Is.EqualTo(expected));
            }
        }
    }
}