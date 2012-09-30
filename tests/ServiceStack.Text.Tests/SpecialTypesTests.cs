using System;
using System.Collections;
using NUnit.Framework;

namespace ServiceStack.Text.Tests
{
	[TestFixture]
	public class SpecialTypesTests
		: TestBase
	{
		[Test]
		public void Can_Serialize_Version()
		{
			Serialize(new Version());
			Serialize(Environment.Version);
		}

		public class JsonEntityWithPrivateGetter
		{
			public string Name { private get; set; }
		}

		public class JsonEntityWithNoProperties
		{
		}

		[Test]
		public void Can_Serialize_Type_with_no_public_getters()
		{
			Serialize(new JsonEntityWithPrivateGetter { Name = "Daniel" });
		}

		[Test]
		public void Can_Serialize_Type_with_no_public_properties()
		{
			Serialize(new JsonEntityWithNoProperties());
		}

		[Test]
		public void Can_Serialize_Type_with_ByteArray()
		{
			var test = new { Name = "Test", Data = new byte[] { 1, 2, 3, 4, 5 } };
			var json = JsonSerializer.SerializeToString(test);
			Assert.That(json, Is.EquivalentTo("{\"Name\":\"Test\",\"Data\":\"AQIDBAU=\"}"));
		}

		[Test]
		public void Can_Serialize_ByteArray()
		{
			var test = new byte[] { 1, 2, 3, 4, 5 };
			var json = JsonSerializer.SerializeToString(test);
			var fromJson = JsonSerializer.DeserializeFromString<byte[]>(json);

			Assert.That(test, Is.EquivalentTo(fromJson));
		}

	    [Test]
	    public void Can_Serialize_HashTable()
	    {
            var h = new Hashtable { { "A", 1 }, { "B", 2 } };
	        var fromJson = h.ToJson().FromJson<Hashtable>();
            Assert.That(fromJson.Count, Is.EqualTo(h.Count));
            Assert.That(fromJson["A"].ToString(), Is.EqualTo(h["A"].ToString()));
            Assert.That(fromJson["B"].ToString(), Is.EqualTo(h["B"].ToString()));
	    }

	}
}