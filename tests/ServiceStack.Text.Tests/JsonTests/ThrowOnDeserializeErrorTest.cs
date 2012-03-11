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
		public void TestThrows()
		{
			JsConfig.Reset();
			JsConfig.ThrowOnDeserializationError = true;

			string json = @"{""idBad"":""abc"", ""idGood"":""2"" }";

			bool threw = false;
			try
			{
				JsonSerializer.DeserializeFromString(json, typeof(TestDto));
			}
			catch (Exception)
			{
				threw = true;
			}

			Assert.IsTrue(threw, "Should have thrown");
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
			[DataMember(Name = "idGood")]
			public int IdGood { get; set; }
			[DataMember(Name = "idBad")]
			public int IdBad { get; set; }
		}
	}
}
