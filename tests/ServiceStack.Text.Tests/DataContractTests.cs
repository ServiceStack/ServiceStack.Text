using System;
using System.Collections;
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


		[CollectionDataContract]
		public class MyCollection : ICollection<MyType>
		{
			List<MyType> _internal = new List<MyType>{ new MyType()};

			public IEnumerator<MyType> GetEnumerator()
			{
				return _internal.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return _internal.GetEnumerator();
			}

			public void Add(MyType item)
			{
				_internal.Add(item);
			}

			public void Clear()
			{
				_internal.Clear();
			}

			public bool Contains(MyType item)
			{
				return _internal.Contains(item);
			}

			public void CopyTo(MyType[] array, int arrayIndex)
			{
				_internal.CopyTo(array, arrayIndex);
			}

			public bool Remove(MyType item)
			{
				return _internal.Remove(item);
			}

			public int Count
			{
				get { return _internal.Count; }
			}

			public bool IsReadOnly
			{
				get { return false; }
			}
		}

		[DataContract]
		public class MyType { }


		[Test]
		public void Can_Serialize_MyCollection()
		{
			var dto = new MyCollection();
			Serialize(dto);
		}

		[DataContract]
		public class PersonRecord
		{
			public int Id { get; set; }
			public string Name { get; set; }
		}

		[Test] //https://github.com/ServiceStack/ServiceStack.Text/issues/46
		public void Replicate_serialization_bug()
		{
			var p = new PersonRecord { Id = 27, Name = "John" };

			// Fails at this point, with a "Cannot access a closed Stream." exception.
			// Am I doing something wrong? 
			string output = XmlSerializer.SerializeToString(p);

			Console.WriteLine(output);
		}

		[DataContract]
		public class ClassOne
		{
			[DataMember]
			public int Id { get; set; }

			[DataMember(Name = "listClassTwo")]
			public List<ClassTwo> List { get; set; }

			public ClassOne()
			{
				List = new List<ClassTwo>();
			}
		}

		[DataContract]
		public class ClassTwo
		{
			[DataMember]
			public string Name { get; set; }
		}
		
		[Test]
		public void deserialize_from_string_with_the_dataMember_name()
		{
			const string jsonList =
                "{\"Id\":1,\"listClassTwo\":[{\"Name\":\"Name One\"},{\"Name\":\"Name Two\"}]}";

			var classOne = JsonSerializer.DeserializeFromString<ClassOne>(jsonList);

			Assert.AreEqual(1, classOne.Id);
			Assert.AreEqual(2, classOne.List.Count);
		}

		[Test]
		public void serialize()
		{
			var classOne= new ClassOne {
				Id = 1,
				List = new List<ClassTwo> { new ClassTwo { Name = "Name One" }, new ClassTwo { Name = "Name Two" } }
			};
			Console.WriteLine(JsonSerializer.SerializeToString(classOne));
		}
	}
}