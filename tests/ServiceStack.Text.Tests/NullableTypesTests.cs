using System;
using System.Collections.Generic;
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
            JsConfig.IncludeNullValues = false;
        }

        public class Answer
        {
            public string tag_name { get; set; }
            public int question_score { get; set; }
            public int question_count { get; set; }
            public int answer_score { get; set; }
            public int answer_count { get; set; }
            public int user_id { get; set; }
        }

        public class TopAnswers
        {
            public TopAnswers()
            {
                this.Items = new List<Answer>();
            }

            public List<Answer> Items { get; set; }
        }

        [Test]
        public void Can_handle_null_in_quotes_in_TopAnswers()
        {
            var topAnswers = new TopAnswers
            {
                Items = {
                    new Answer {
                        tag_name = "null",
                        question_score= 0,
                        question_count= 0,
                        answer_score= 17,
                        answer_count= 2,
                        user_id= 236255
                    },
                }
            };

            var json = topAnswers.ToJson();
            var fromJson = json.FromJson<TopAnswers>();

            fromJson.PrintDump();

            Assert.That(fromJson.Items[0].tag_name, Is.EqualTo("null"));
        }

        [Test]
        public void Can_handle_null_in_Answer()
        {
            var json = "{\"tag_name\":null,\"question_score\":0,\"question_count\":0,\"answer_score\":17,\"answer_count\":2,\"user_id\":236255}";
            var fromJson = json.FromJson<Answer>();

            Assert.That(fromJson.tag_name, Is.Null);
        }

        [Test]
        public void Can_handle_null_in_quotes_in_Answer()
        {
            var answer = new Answer
            {
                tag_name = "null",
                question_score = 0,
                question_count = 0,
                answer_score = 17,
                answer_count = 2,
                user_id = 236255
            };

            var json = answer.ToJson();
            json.Print();
            var fromJson = json.FromJson<Answer>();

            fromJson.PrintDump();

            Assert.That(fromJson.tag_name, Is.EqualTo("null"));
        }

        [Test]
        public void Deserialize_WithNullCollection_CollectionIsNull()
        {
            JsConfig.IncludeNullValues = true;
            
            var item = new Foo { Strings = null };
            var json = JsonSerializer.SerializeToString(item);
            var result = JsonSerializer.DeserializeFromString<Foo>(json);
            Assert.IsNull(result.Strings);

            var jsv = TypeSerializer.SerializeToString(item);
            result = TypeSerializer.DeserializeFromString<Foo>(jsv);
            Assert.IsEmpty(result.Strings); //JSV doesn't support setting null values explicitly

            JsConfig.IncludeNullValues = false;
        }

        public class Foo
        {
            public Foo()
            {
                Strings = new List<string>();
            }
            public List<string> Strings { get; set; }
        }

    }

}