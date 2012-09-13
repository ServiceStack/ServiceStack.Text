using System.Collections;
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
		public IDictionary Headers { get; set; }
		public byte DeliveryMode { get; set; }
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

	#endregion

	[TestFixture]
	public class BasicPropertiesTests
	{

		[Test]
		public void FailureCondition()
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
                Timestamp = new AmqpTimestamp{UnixTime =123344044},
                Type = "Type",
                UserId = "user id",
                Headers = new Hashtable
                {
                    {"one", "header one"},
                    {"two", "header two"}
                }
            };

			var str = JsonSerializer.SerializeToString(originalProperties);
			var obj = JsonSerializer.DeserializeFromString<BasicProperties>(str);

			Assert.That(obj.AppId, Is.EqualTo(originalProperties.AppId));
		}
	}
}
