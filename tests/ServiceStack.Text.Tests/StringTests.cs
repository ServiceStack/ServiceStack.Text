using NUnit.Framework;
using ServiceStack.Client;
using ServiceStack.ServiceModel.Serialization;

namespace ServiceStack.Text.Tests
{
	[TestFixture]
	public class StringTests
	{
		[Test]
		public void SerializerTests()
		{
			string v = "This is a string";

			// serialize to JSON using ServiceStack
			string jsonString = ServiceStack.Text.JsonSerializer.SerializeToString(v);

			// serialize to JSON using BCL
			//var bclJsonString = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(v);
			var bclJsonString = BclJsonDataContractSerializer.Instance.Parse(v);

			string correctJSON = "\"This is a string\""; // this is what a modern browser will produce with JSON.stringify("This is a string");

			Assert.AreEqual(correctJSON, bclJsonString, "BCL serializes string correctly");
			Assert.AreEqual(correctJSON, jsonString, "Service Stack serializes string correctly");
		}

		[Test]
		public void RoundTripTest()
		{
			string json = "\"This is a string\"";
			string correctString = "This is a string"; // this is what a modern browser will produce from JSON.parse("\"This is a string\"");

			//var bclString = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<string>(json);
			var bclString = BclJsonDataContractDeserializer.Instance.Parse<string>(json);
			var ssString = ServiceStack.Text.JsonSerializer.DeserializeFromString<string>(json);

			Assert.AreEqual(correctString, bclString, "BCL deserializes correctly");
			Assert.AreEqual(correctString, ssString, "Service Stack deserializes correctly");

			var ssJson = ServiceStack.Text.JsonSerializer.SerializeToString(ssString, typeof(string));
			//var bclJson = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(bclString);
			var bclJson = BclJsonDataContractSerializer.Instance.Parse(bclString);

			Assert.AreEqual(json, bclJson, "BCL round trips correctly");
			Assert.AreEqual(json, ssJson, "Service Stack round trips correctly");
		}
		 
	}
}