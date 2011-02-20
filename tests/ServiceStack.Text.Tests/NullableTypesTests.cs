using NUnit.Framework;
using ServiceStack.Common.Tests.Models;

namespace ServiceStack.Text.Tests
{
	[TestFixture]
	public class NullableTypesTests
		: TestBase
	{
		[Test]
		public void Can_Serialize_populated_model_of_NullableTypes()
		{
			var model = ModelWithFieldsOfNullableTypes.Create(1);
			Serialize(model);
		}

		[Test]
		public void Can_Serialize_empty_model_of_NullableTypes()
		{
			var model = new ModelWithFieldsOfNullableTypes();
			Serialize(model);
		}

	}
}