using System;
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

        [Test]
        public void Serialize_array_with_null_should_always_produce_Valid_JSON()
        {
            JsConfig.IncludeNullValues = true;
            string json = new Object[] { 1, 2, 3, null, 5 }.ToJson();  // [1,2,3,,5]  - Should be [1,2,3,null,5]
            Assert.That(json, Is.EqualTo("[1,2,3,null,5]"));
        }
    }

}