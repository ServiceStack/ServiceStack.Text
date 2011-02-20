using NUnit.Framework;
using ServiceStack.Common.Tests.Models;

namespace ServiceStack.Text.Tests
{
	[TestFixture]
	public class NullableTypesTests
	{
		[Test]
		public void Can_Serialize_populated_model_of_NullableTypes()
		{
			var model = ModelWithFieldsOfNullableTypes.Create(1);

			var json = JsonSerializer.SerializeToString(model);

			var fromJson = JsonSerializer.DeserializeFromString<ModelWithFieldsOfNullableTypes>(json);

			ModelWithFieldsOfNullableTypes.AssertIsEqual(model, fromJson);
		}

		[Test]
		public void Can_Serialize_empty_model_of_NullableTypes()
		{
			var model = new ModelWithFieldsOfNullableTypes();

			var json = JsonSerializer.SerializeToString(model);

			var fromJson = JsonSerializer.DeserializeFromString<ModelWithFieldsOfNullableTypes>(json);

			ModelWithFieldsOfNullableTypes.AssertIsEqual(model, fromJson);
		}
	}

}