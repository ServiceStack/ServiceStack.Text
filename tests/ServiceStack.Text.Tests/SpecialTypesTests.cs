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
	}
}