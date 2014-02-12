#if !MONO && !IOS

using NUnit.Framework;
using System.IO;

namespace ServiceStack.Text.Tests.JsonTests
{
	[TestFixture]
	internal class JsonSerializerTests
		: TestBase
	{
		[Test]
		public void SerializeToWriterTest()
		{
			string correctString = @"String with backslashes '\', 'single' and ""double quotes"", (along		with	other	special	symbols	like	tabs) wich may broke incorrect serializing/deserializing implementation ;)";
			// this is what a modern browser will produce from JSON.parse("\"This is a string\"");
			string json = "\"String with backslashes '\\\\', 'single' and \\\"double quotes\\\", (along\\t\\twith\\tother\\tspecial\\tsymbols\\tlike\\ttabs) wich may broke incorrect serializing/deserializing implementation ;)\"";

			using (MemoryStream ms = new MemoryStream())
			{
				StreamWriter sw = new StreamWriter(ms);
				JsonSerializer.SerializeToWriter(correctString, sw);
				sw.Flush();

				using (System.IO.StreamReader sr = new System.IO.StreamReader(ms))
				{
					ms.Position = 0;
					var ssJson = sr.ReadToEnd();
					Assert.AreEqual(json, ssJson, "Service Stack serializes correctly");

					ms.Position = 0;
					var ssString = JsonSerializer.DeserializeFromReader(sr, typeof(string));
					Assert.AreEqual(correctString, ssString, "Service Stack deserializes correctly");
				}
			}
		}

		[Test]
		public void SerializeToStreamTest()
		{
			string correctString = @"String with backslashes '\', 'single' and ""double quotes"", (along		with	other	special	symbols	like	tabs) wich may broke incorrect serializing/deserializing implementation ;)";
			// this is what a modern browser will produce from JSON.parse("\"This is a string\"");
			string json = "\"String with backslashes '\\\\', 'single' and \\\"double quotes\\\", (along\\t\\twith\\tother\\tspecial\\tsymbols\\tlike\\ttabs) wich may broke incorrect serializing/deserializing implementation ;)\"";

			using (MemoryStream ms = new MemoryStream())
			{
				JsonSerializer.SerializeToStream(correctString, ms);
				var ssJson = System.Text.UnicodeEncoding.UTF8.GetString(ms.ToArray());
				Assert.AreEqual(json, ssJson, "Service Stack serializes correctly");

				ms.Position = 0;
				var ssString = JsonSerializer.DeserializeFromStream(typeof(string), ms);
				Assert.AreEqual(correctString, ssString, "Service Stack deserializes correctly");
			}
		}
	}
}

#endif