using System;
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
	}
}