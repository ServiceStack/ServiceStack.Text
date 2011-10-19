using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using NUnit.Framework;
using ServiceStack.Text.Tests.Support;

namespace ServiceStack.Text.Tests
{
	[TestFixture]
	public class DataContractTests
		: TestBase
	{
		[Test]
		public void Only_Serializes_DataMember_fields_for_DataContracts()
		{
			var dto = new ResponseStatus
			{
				ErrorCode = "ErrorCode",
				Message = "Message",
				StackTrace = "StackTrace",
				Errors = new List<ResponseError>(),
			};

			Serialize(dto);
		}

        public class RequestWithIgnoredMembers
        {
            public string Name { get; set; }

            [IgnoreDataMember]
            public string Comment { get; set; }
        }

        private void DoIgnoreMemberTest(Func<RequestWithIgnoredMembers, string> serialize, Func<string, RequestWithIgnoredMembers> deserialize)
        {
            var dto = new RequestWithIgnoredMembers()
            {
                Name = "John",
                Comment = "Some Comment"
            };

            var clone = deserialize(serialize(dto));

            Assert.AreEqual(dto.Name, clone.Name);
            Assert.IsNull(clone.Comment);
        }

        [Test]
        public void JsonSerializerHonorsIgnoreMemberAttribute()
        {
            DoIgnoreMemberTest(r => JsonSerializer.SerializeToString(r), s => JsonSerializer.DeserializeFromString<RequestWithIgnoredMembers>(s));
        }

        [Test]
        public void JsvSerializerHonorsIgnoreMemberAttribute()
        {
            DoIgnoreMemberTest(r => TypeSerializer.SerializeToString(r), s => TypeSerializer.DeserializeFromString<RequestWithIgnoredMembers>(s));
        }

        [Test]
        public void XmlSerializerHonorsIgnoreMemberAttribute()
        {
            DoIgnoreMemberTest(r => XmlSerializer.SerializeToString(r), s => XmlSerializer.DeserializeFromString<RequestWithIgnoredMembers>(s));
        }

		[DataContract]
		public class EmptyDataContract
		{
		}

		[Test]
		public void Can_Serialize_Empty_DataContract()
		{
			var dto = new EmptyDataContract();
			Serialize(dto);
		}

	}
}