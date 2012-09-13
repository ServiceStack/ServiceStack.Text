using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace ServiceStack.Text.Tests.EasyNetQ_Bug
{
	#region Test case types

	public struct AmqpTimestamp
	{
		public long UnixTime { get; set; }
	}
	public class BasicProperties
	{
		public string ContentType { get; set; }
		public string ContentEncoding { get; set; }
		public byte DeliveryMode { get; set; }
		public IDictionary Headers { get; set; }
		public byte Priority { get; set; }
		public string CorrelationId { get; set; }
		public string ReplyTo { get; set; }
		public string Expiration { get; set; }
		public string MessageId { get; set; }
		public AmqpTimestamp Timestamp { get; set; }
		public string Type { get; set; }
		public string UserId { get; set; }
		public string AppId { get; set; }
		public string ClusterId { get; set; }
		public int ProtocolClassId { get; set; }
		public string ProtocolClassName { get; set; }
	}

	public class MinimalFailure
	{
		public IDictionary Container { get; set; }
	}
	public class MinimalPass
	{
		public Dictionary<string, string> Container { get; set; }
	}

	#endregion

	[TestFixture]
	public class BasicPropertiesTests
	{

		[Test]
		public void FailureCondition1()
		{

			var originalProperties = new BasicProperties
			{
				AppId = "some app id",
				ClusterId = "cluster id",
				ContentEncoding = "content encoding",
				ContentType = "content type",
				CorrelationId = "correlation id",
				DeliveryMode = 4,
				Expiration = "expiration",
				MessageId = "message id",
				Priority = 1,
				ReplyTo = "abc",
				Timestamp = new AmqpTimestamp { UnixTime = 123344044 },
				Type = "Type",
				UserId = "user id",
				Headers = new Dictionary<string, string>
                {
                    {"one", "header one"},
                    {"two", "header two"}
                }
			};

			var str = JsonSerializer.SerializeToString(originalProperties);
			var obj = JsonSerializer.DeserializeFromString<BasicProperties>(str);

			Assert.That(obj.AppId, Is.EqualTo(originalProperties.AppId));
		}

		[Test]
		public void FailureCondition2()
		{
			var original = new MinimalFailure
			{
				Container = new Dictionary<string, string>
                {
                    {"one", "header one"},
                    {"two", "header two"}
                }
			};

			var str = JsonSerializer.SerializeToString(original);
			var obj = JsonSerializer.DeserializeFromString<MinimalFailure>(str);

			Assert.That(obj.Container, Is.EquivalentTo(original.Container));
		}
		
		[Test]
		public void FailureCondition3()
		{
			var original = new MinimalFailure // Using IDictionary backing
			{
				Container = new Dictionary<string, string>
                {
                    {"one", "header one"},
                    {"two", "header two"}
                }
			};

			var str = JsonSerializer.SerializeToString(original);
			var obj = JsonSerializer.DeserializeFromString<MinimalPass>(str); // decoding to Dictionary<,>

			Assert.That(obj.Container, Is.EquivalentTo(original.Container));
		}
		
		[Test]
		public void FailureCondition4()
		{
			var original = new MinimalPass // Using Dictionary<,> backing
			{
				Container = new Dictionary<string, string>
                {
                    {"one", "header one"},
                    {"two", "header two"}
                }
			};

			var str = JsonSerializer.SerializeToString(original);
			var obj = JsonSerializer.DeserializeFromString<MinimalFailure>(str); // decoding to IDictionary

			Assert.That(obj.Container, Is.EquivalentTo(original.Container));
		}


		[Test]
		public void PassCondition()
		{
			var original = new MinimalPass
			{
				Container = new Dictionary<string, string>
                {
                    {"one", "header one"},
                    {"two", "header two"}
                }
			};

			var str = JsonSerializer.SerializeToString(original);
			var obj = JsonSerializer.DeserializeFromString<MinimalPass>(str);

			Assert.That(obj.Container, Is.EquivalentTo(original.Container));
		}

		[Test]
		public void SerialiserTest()
		{
			JsConfig.PreferInterfaces = true;
			JsConfig.ExcludeTypeInfo = false;
			JsConfig.ConvertObjectTypesIntoStringDictionary = false;

			var passing = new MinimalPass
			{
				Container = new Dictionary<string, string>
                {
                    {"one", "header one"},
                    {"two", "header two"}
                }
			};
			var subject = new MinimalFailure
			{
				Container = new Dictionary<string, string>
                {
                    {"one", "header one"},
                    {"two", "header two"}
                }
			};

			var str1 = passing.ToJson();
			var str2 = subject.ToJson();

			Console.WriteLine("Working --> " + str1);
			Console.WriteLine();
			Console.WriteLine("Failing --> " + str2);

			Assert.That(str1, Is.EqualTo(str2));
		}
	}
}
