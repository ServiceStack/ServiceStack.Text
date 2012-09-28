using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;

namespace ServiceStack.Text.Tests.JsonTests
{
	public class CustomSerializerTests : TestBase
	{
		static CustomSerializerTests()
		{
            JsConfig<EntityWithValues>.RawSerializeFn = SerializeEntity;
            JsConfig<EntityWithValues>.RawDeserializeFn = DeserializeEntity;
		}

		[Test]
		public void Can_serialize_Entity()
		{
		    var originalEntity = new EntityWithValues {id = 5, Values = new Dictionary<string, string> {{"dog", "bark"}, {"cat", "meow"}}};
		    JsonSerializeAndCompare(originalEntity);
		}

		[Test]
		public void Can_serialize_arrays_of_entities()
		{
			var originalEntities = new[] { new EntityWithValues {id = 5, Values = new Dictionary<string, string> {{"dog", "bark"}}}, new EntityWithValues {id = 6, Values = new Dictionary<string, string> {{"cat", "meow"}}} };
			JsonSerializeAndCompare(originalEntities);
		}

        public class EntityWithValues
        {
            private Dictionary<string, string> _values;

            public int id { get; set; }

            public Dictionary<string, string> Values
            {
                get { return _values ?? (_values = new Dictionary<string, string>()); }
                set { _values = value; }
            }

            public override int GetHashCode()
            {
                return this.id.GetHashCode() ^ this.Values.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as EntityWithValues);
            }

            public bool Equals(EntityWithValues other)
            {
                return ReferenceEquals(this, other)
                       || (this.id == other.id && DictionaryEquality(Values, other.Values));
            }

            private bool DictionaryEquality(Dictionary<string, string> first, Dictionary<string, string> second)
            {
                return first.Count == second.Count
                       && first.Keys.All(second.ContainsKey)
                       && first.Keys.All(key => first[key] == second[key]);
            }
        }

        private static string SerializeEntity(EntityWithValues entity)
        {
            var dictionary = entity.Values.ToDictionary(pair => pair.Key, pair => pair.Value);
            if (entity.id > 0) {
                dictionary["id"] = entity.id.ToString(CultureInfo.InvariantCulture);
            }
            return JsonSerializer.SerializeToString(dictionary);
        }

        private static EntityWithValues DeserializeEntity(string value)
        {
            var dictionary = JsonSerializer.DeserializeFromString<Dictionary<string, string>>(value);
            if (dictionary == null) return null;
            var entity = new EntityWithValues();
            foreach (var pair in dictionary) {
                if (pair.Key == "id") {
                    if (!string.IsNullOrEmpty(pair.Value)) {
                        entity.id = int.Parse(pair.Value);
                    }
                } else {
                    entity.Values.Add(pair.Key, pair.Value);
                }
            }
            return entity;
        }
	}
}