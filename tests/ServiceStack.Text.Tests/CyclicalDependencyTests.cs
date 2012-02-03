using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace ServiceStack.Text.Tests
{
	public class Error
	{
		public Error()
		{
			ExtendedData = new Dictionary<string, object>();
			Tags = new HashSet<string>();
			StackTrace = new Collection<StackFrame>();
		}

		public string Id { get; set; }
		public string Message { get; set; }
		public string Type { get; set; }
		//public Module Module { get; set; }
		public string Description { get; set; }
		public DateTime OccurrenceDate { get; set; }
		public string Code { get; set; }
		public IDictionary<string, object> ExtendedData { get; set; }
		public HashSet<string> Tags { get; set; }

		public Error Inner { get; set; }

		public ICollection<StackFrame> StackTrace { get; set; }
		public string Contact { get; set; }
		public string Notes { get; set; }
	}


	[TestFixture]
	public class CyclicalDependencyTests : TestBase
	{
		[Test]
		public void Can_serialize_Error()
		{
			var dto = new Error {
				Id = "Id",
				Message = "Message",
				Type = "Type",
				Description = "Description",
				OccurrenceDate = new DateTime(2012, 01, 01),
				Code = "Code",
				ExtendedData = new Dictionary<string, object>(),
				Tags = new HashSet<string> { "C#", "ruby" },
				Inner = new Error {
					Id = "Id2",
					Message = "Message2",
				},
				Contact = "Contact",
				Notes = "Notes",
			};

			var from = Serialize(dto, includeXml: false);
			Console.WriteLine(from.Dump());

			Assert.That(from.Id, Is.EqualTo(dto.Id));
			Assert.That(from.Message, Is.EqualTo(dto.Message));
			Assert.That(from.Type, Is.EqualTo(dto.Type));
			Assert.That(from.Description, Is.EqualTo(dto.Description));
			Assert.That(from.OccurrenceDate, Is.EqualTo(dto.OccurrenceDate));
			Assert.That(from.Code, Is.EqualTo(dto.Code));
			Assert.That(from.Inner.Id, Is.EqualTo(dto.Inner.Id));
			Assert.That(from.Inner.Message, Is.EqualTo(dto.Inner.Message));
			Assert.That(from.Contact, Is.EqualTo(dto.Contact));
			Assert.That(from.Notes, Is.EqualTo(dto.Notes));
		}
	}
}
