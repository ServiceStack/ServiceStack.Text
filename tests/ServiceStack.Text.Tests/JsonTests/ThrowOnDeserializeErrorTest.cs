using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using NUnit.Framework;
using ServiceStack.Text.Tests.Support;

namespace ServiceStack.Text.Tests.JsonTests
{
	[TestFixture]
	public class ThrowOnDeserializeErrorTest
	{
        [Test]
        [ExpectedException(typeof(DeserializationException), ExpectedMessage = "Failed to set property 'idBadProt' with 'abc'")]
        public void Throws_on_protected_setter()
        {
            JsConfig.Reset();
            JsConfig.ThrowOnDeserializationError = true;

            string json = @"{""idBadProt"":""abc"", ""idGood"":""2"" }";
            JsonSerializer.DeserializeFromString(json, typeof(TestDto));
        }

		[Test]
        [ExpectedException(typeof(DeserializationException), ExpectedMessage = "Failed to set property 'idBad' with 'abc'")]
		public void Throws_on_incorrect_type()
		{
			JsConfig.Reset();
			JsConfig.ThrowOnDeserializationError = true;

			string json = @"{""idBad"":""abc"", ""idGood"":""2"" }";
    		JsonSerializer.DeserializeFromString(json, typeof(TestDto));
		}

		[Test]
		public void TestDoesNotThrow()
		{
			JsConfig.Reset();
			JsConfig.ThrowOnDeserializationError = false;
			string json = @"{""idBad"":""abc"", ""idGood"":""2"" }";
			JsonSerializer.DeserializeFromString(json, typeof(TestDto));
		}

		[Test]
		public void TestReset()
		{
			JsConfig.Reset();
			Assert.IsFalse(JsConfig.ThrowOnDeserializationError);
			JsConfig.ThrowOnDeserializationError = true;
			Assert.IsTrue(JsConfig.ThrowOnDeserializationError);
			JsConfig.Reset();
			Assert.IsFalse(JsConfig.ThrowOnDeserializationError);
		}

		[DataContract]
		class TestDto
		{
            [DataMember(Name = "idBadProt")]
            public int protId { get; protected set; }
            [DataMember(Name = "idGood")]
			public int IdGood { get; set; }
			[DataMember(Name = "idBad")]
			public int IdBad { get; set; }
        }
	}
}
