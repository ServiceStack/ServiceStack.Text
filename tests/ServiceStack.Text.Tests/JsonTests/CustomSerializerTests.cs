using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using System.Runtime.Serialization;

namespace ServiceStack.Text.Tests.JsonTests
{
    public class CustomSerializerTests : TestBase
    {
        static CustomSerializerTests()
        {
            JsConfig<EntityWithValues>.RawSerializeFn = SerializeEntity;
            JsConfig<EntityWithValues>.RawDeserializeFn = DeserializeEntity;
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            JsConfig.Reset();
        }

        [Test]
        public void Can_serialize_Entity()
        {
            var originalEntity = new EntityWithValues { id = 5, Values = new Dictionary<string, string> { { "dog", "bark" }, { "cat", "meow" } } };
            JsonSerializeAndCompare(originalEntity);
        }

        [Test]
        public void Can_serialize_arrays_of_entities()
        {
            var originalEntities = new[] { new EntityWithValues { id = 5, Values = new Dictionary<string, string> { { "dog", "bark" } } }, new EntityWithValues { id = 6, Values = new Dictionary<string, string> { { "cat", "meow" } } } };
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
            if (entity.id > 0)
            {
                dictionary["id"] = entity.id.ToString(CultureInfo.InvariantCulture);
            }
            return JsonSerializer.SerializeToString(dictionary);
        }

        private static EntityWithValues DeserializeEntity(string value)
        {
            var dictionary = JsonSerializer.DeserializeFromString<Dictionary<string, string>>(value);
            if (dictionary == null) return null;
            var entity = new EntityWithValues();
            foreach (var pair in dictionary)
            {
                if (pair.Key == "id")
                {
                    if (!string.IsNullOrEmpty(pair.Value))
                    {
                        entity.id = int.Parse(pair.Value);
                    }
                }
                else
                {
                    entity.Values.Add(pair.Key, pair.Value);
                }
            }
            return entity;
        }

        [DataContract]
        private class Test1Base
        {
            public Test1Base(bool itb, bool itbm)
            {
                InTest1Base = itb; InTest1BaseM = itbm;
            }

            [DataMember]
            public bool InTest1BaseM { get; set; }

            public bool InTest1Base { get; set; }
        }

        [DataContract]
        private class Test1 : Test1Base
        {
            public Test1(bool it, bool itm, bool itb, bool itbm)
                : base(itb, itbm)
            {
                InTest1 = it; InTest1M = itm;
            }

            [DataMember]
            public bool InTest1M { get; set; }

            public bool InTest1 { get; set; }
        }

        [Test]
        public void Can_Serialize_With_Custom_Constructor()
        {
            bool hit = false;
            JsConfig.ModelFactory = type => {
                if (typeof(Test1) == type)
                {
                    hit = true;
                    return () => new Test1(false, false, true, false);
                }
                return null;
            };

            var t1 = new Test1(true, true, true, true);

            var data = JsonSerializer.SerializeToString(t1);

            var t2 = JsonSerializer.DeserializeFromString<Test1>(data);

            Assert.IsTrue(hit);
            Assert.IsTrue(t2.InTest1BaseM);
            Assert.IsTrue(t2.InTest1M);
            Assert.IsTrue(t2.InTest1Base);
            Assert.IsFalse(t2.InTest1);
        }


        public class Dto
        {
            public string Name { get; set; }
        }

        public interface IHasVersion
        {
            int Version { get; set; }
        }

        public class DtoV1 : IHasVersion
        {
            public int Version { get; set; }
            public string Name { get; set; }

            public DtoV1()
            {
                Version = 1;
            }
        }

        [Test]
        public void Can_detect_dto_with_no_Version()
        {
            using (JsConfig.With(modelFactory:type => {
                if (typeof(IHasVersion).IsAssignableFrom(type))
                {
                    return () => {
                        var obj = (IHasVersion)type.CreateInstance();
                        obj.Version = 0;
                        return obj;
                    };
                }
                return type.CreateInstance;
            }))
            {
                var dto = new Dto { Name = "Foo" };
                var fromDto = dto.ToJson().FromJson<DtoV1>();
                Assert.That(fromDto.Version, Is.EqualTo(0));
                Assert.That(fromDto.Name, Is.EqualTo("Foo"));

                var dto1 = new DtoV1 { Name = "Foo 1" };
                var fromDto1 = dto1.ToJson().FromJson<DtoV1>();
                Assert.That(fromDto1.Version, Is.EqualTo(1));
                Assert.That(fromDto1.Name, Is.EqualTo("Foo 1"));
            }
        }

        public class ErrorPoco
        {
            public string ErrorCode { get; set; }
            public string ErrorDescription { get; set; }
        }

        [Test]
        public void Can_deserialize_json_with_underscores()
        {
            var json = "{\"error_code\":\"anErrorCode\",\"error_description\",\"the description\"}";

            var dto = json.FromJson<ErrorPoco>();

            Assert.That(dto.ErrorCode, Is.Null);

            using (JsConfig.With(propertyConvention: PropertyConvention.Lenient))
            {
                dto = json.FromJson<ErrorPoco>();

                Assert.That(dto.ErrorCode, Is.EqualTo("anErrorCode"));
                Assert.That(dto.ErrorDescription, Is.EqualTo("the description"));

                dto.PrintDump();
            }
        }
    }
}