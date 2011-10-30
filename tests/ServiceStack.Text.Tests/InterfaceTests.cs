using System;
using System.Collections;
using NUnit.Framework;
using ServiceStack.Messaging;
using ServiceStack.ServiceHost;
using ServiceStack.Text.Tests.JsonTests;

namespace ServiceStack.Text.Tests
{
	[TestFixture]
	public class InterfaceTests : TestBase
	{
		[Test]
		public void Can_serialize_Message()
		{
			var message = new Message<string> { Body = "test" };
			var messageString = TypeSerializer.SerializeToString(message);

			Assert.That(messageString, Is.EqualTo(
			"{Id:00000000000000000000000000000000,CreatedDate:0001-01-01,Priority:0,RetryAttempts:0,Body:test}"));

			Serialize(message);
		}

		[Test]
		public void Can_serialize_IMessage()
		{
			var message = new Message<string> { Body = "test" };
			var messageString = TypeSerializer.SerializeToString((IMessage<string>)message);

			Assert.That(messageString, Is.EqualTo(
			"{\"__type\":\"ServiceStack.Messaging.Message`1[[System.String, mscorlib]], ServiceStack.Interfaces\","
			 + "Id:00000000000000000000000000000000,CreatedDate:0001-01-01,Priority:0,RetryAttempts:0,Body:test}"));
		}

		public class DtoWithInterface
		{
			public IMessage<string> Results { get; set; }
		}

		[Test]
		public void Can_deserialize_interface_into_concrete_type()
		{
			var dto = Serialize(new DtoWithInterface { Results = new Message<string>("Body") }, includeXml:false);
			Assert.That(dto.Results, Is.Not.Null);
		}

		public class DtoWithObject
		{
			public object Results { get; set; }
		}

		[Test]
		public void Can_deserialize_dto_with_object()
		{
			var dto = Serialize(new DtoWithObject { Results = new Message<string>("Body") }, includeXml: false);
			Assert.That(dto.Results, Is.Not.Null);
			Assert.That(dto.Results.GetType(), Is.EqualTo(typeof(Message<string>)));
		}
		
		[Test]
		public void Can_serialize_ToString()
		{
			var type = Type.GetType(typeof(Message<string>).AssemblyQualifiedName);
			Assert.That(type, Is.Not.Null);

			type = AssemblyUtils.FindType(typeof(Message<string>).AssemblyQualifiedName);
			Assert.That(type, Is.Not.Null);

			type = Type.GetType("ServiceStack.Messaging.Message`1[[System.String, mscorlib]], ServiceStack.Interfaces");
			Assert.That(type, Is.Not.Null);
		}

		[Test, TestCaseSource(typeof(InterfaceTests), "EndpointExpectations")]
		public void Does_serialize_minimum_type_info_whilst_still_working(
		Type type, string expectedTypeString)
		{
			Assert.That(type.ToTypeString(), Is.EqualTo(expectedTypeString));
			var newType = AssemblyUtils.FindType(type.ToTypeString());
			Assert.That(newType, Is.Not.Null);
			Assert.That(newType, Is.EqualTo(type));
		}

		public static IEnumerable EndpointExpectations
		{
			get
			{
				yield return new TestCaseData(typeof(Message<string>),
					"ServiceStack.Messaging.Message`1[[System.String, mscorlib]], ServiceStack.Interfaces");

				yield return new TestCaseData(typeof(Cat),
					"ServiceStack.Text.Tests.JsonTests.Cat, ServiceStack.Text.Tests");

				yield return new TestCaseData(typeof(Zoo),
					"ServiceStack.Text.Tests.JsonTests.Zoo, ServiceStack.Text.Tests");
			}
		}
	}
}