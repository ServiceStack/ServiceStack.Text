using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace ServiceStack.Text.Tests
{
	[TestFixture]
	public class AnonymousTypes
		: TestBase
	{
		[Test]
		public void Can_serialize_anonymous_types()
		{
			Serialize(new { Id = 1, Name = "Name", IntList = new[] { 1, 2, 3 } }, includeXml: false); // xmlserializer cannot serialize anonymous types.
		}

		[Test]
		public void Can_serialize_anonymous_type_and_read_as_string_Dictionary()
		{
			var json = JsonSerializer.SerializeToString(
				new { Id = 1, Name = "Name", IntList = new[] { 1, 2, 3 } });

			Console.WriteLine("JSON: " + json);

			var map = JsonSerializer.DeserializeFromString<Dictionary<string, string>>(json);

			Console.WriteLine("MAP: " + map.Dump());
		}
	}

}