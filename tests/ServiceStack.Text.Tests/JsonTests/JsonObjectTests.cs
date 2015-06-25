using System.Collections.Generic;
using NUnit.Framework;

namespace ServiceStack.Text.Tests.JsonTests
{
    [TestFixture]
    public class JsonObjectTests
    {
        [Test]
        public void Can_parse_empty_object()
        {
            Assert.That(JsonObject.Parse("{}"), Is.Empty);
        }

        [Test]
        public void Can_parse_empty_object_with_whitespaces()
        {
            Assert.That(JsonObject.Parse("{    }"), Is.Empty);
            Assert.That(JsonObject.Parse("{\n\n}"), Is.Empty);
            Assert.That(JsonObject.Parse("{\t\t}"), Is.Empty);
        }

        [Test]
        public void Can_parse_empty_object_with_mixed_whitespaces()
        {
            Assert.That(JsonObject.Parse("{ \n\t  \n\r}"), Is.Empty);
        }

        [Test]
        public void Can_Serialize_numbers()
        {
            string notNumber = "{\"field\":\"00001\"}";
            Assert.That(JsonObject.Parse(notNumber).ToJson(), Is.EqualTo(notNumber));

            string num1 = "{\"field\":0}";
            Assert.That(JsonObject.Parse(num1).ToJson(), Is.EqualTo(num1));

            string num2 = "{\"field\":0.5}";
            Assert.That(JsonObject.Parse(num2).ToJson(), Is.EqualTo(num2));

            string num3 = "{\"field\":.5}";
            Assert.That(JsonObject.Parse(num3).ToJson(), Is.EqualTo(num3));

            string num4 = "{\"field\":12312}";
            Assert.That(JsonObject.Parse(num4).ToJson(), Is.EqualTo(num4));

            string num5 = "{\"field\":12312.1231}";
            Assert.That(JsonObject.Parse(num5).ToJson(), Is.EqualTo(num5));

            string num6 = "{\"field\":1435252569117}";
            Assert.That(JsonObject.Parse(num6).ToJson(), Is.EqualTo(num6));

            string num7 = "{\"field\":1435052569117}";
            Assert.That(JsonObject.Parse(num7).ToJson(), Is.EqualTo(num7));
        }

        public class Jackalope
        {
            public string Name { get; set; }
            public Jackalope BabyJackalope { get; set; }
        }

        [Test]
        public void Can_serialise_json_object_deserialise_typed_object()
        {
            var jacks = new {
                Jack = new Jackalope { BabyJackalope = new Jackalope { Name = "in utero" } }
            };

            var jackString = JsonSerializer.SerializeToString(jacks.Jack);

            var jackJson = JsonObject.Parse(jackString);
            var jack = jackJson.Get<Jackalope>("BabyJackalope");

            Assert.That(jacks.Jack.BabyJackalope.Name, Is.EqualTo(jack.Name));

            var jackJsonString = jackJson.SerializeToString();
            Assert.That(jackString, Is.EqualTo(jackJsonString));

            var jackalope = JsonSerializer.DeserializeFromString<Jackalope>(jackJsonString);
            Assert.That(jackalope.BabyJackalope.Name, Is.EqualTo("in utero"));
        }
        
        readonly TextElementDto text = new TextElementDto {
            ElementId = "text_1",
            ElementType = "text",
            // Raw nesting - won't be escaped
            Content = new ElementContentDto { ElementId = "text_1", Content = "text goes here" },
            Action = new ElementActionDto { ElementId = "text_1", Action = "action goes here" }
        };

        readonly ImageElementDto image = new ImageElementDto {
            ElementId = "image_1",
            ElementType = "image",
            // String nesting - will be escaped
            Content = new ElementContentDto { ElementId = "image_1", Content = "image url goes here" }.ToJson(),
            Action = new ElementActionDto { ElementId = "image_1", Action = "action goes here" }.ToJson()
        };

        [Test]
        public void Can_Serialize_TypedContainerDto()
        {
            var container = new TypedContainerDto {
                Source = text,
                Destination = image
            };

            var json = container.ToJson();

            var fromJson = json.FromJson<TypedContainerDto>();

            Assert.That(container.Source.Action.ElementId, Is.EqualTo(fromJson.Source.Action.ElementId));

            var imgContent = container.Destination.Content.FromJson<ElementContentDto>();
            var fromContent = fromJson.Destination.Content.FromJson<ElementContentDto>();

            Assert.That(imgContent.ElementId, Is.EqualTo(fromContent.ElementId));
        }

        [Test]
        public void Can_DeSerialize_TypedContainerDto_with_JsonObject()
        {
            var container = new TypedContainerDto {
                Source = text,
                Destination = image
            };

            var json = container.ToJson();

            var fromText = JsonObject.Parse(json).Get<TextElementDto>("Source");

            Assert.That(container.Source.Action.ElementId, Is.EqualTo(fromText.Action.ElementId));
        }

        [Test]
        public void Can_DeSerialize_TypedContainerDto_into_JsonValueContainerDto()
        {
            var container = new TypedContainerDto {
                Source = text,
                Destination = image
            };

            var json = container.ToJson();

            var fromJson = json.FromJson<JsonValueContainerDto>();

            var fromText = fromJson.Source.As<TextElementDto>();
            var fromImage = fromJson.Destination.As<ImageElementDto>();

            Assert.That(container.Source.Action.ElementId, Is.EqualTo(fromText.Action.ElementId));
            Assert.That(container.Destination.ElementId, Is.EqualTo(fromImage.ElementId));

            Assert.That(container.Destination.Action, Is.EqualTo(fromImage.Action));
            Assert.That(container.Destination.Content, Is.EqualTo(fromImage.Content));
        }

        [Test]
        public void Can_Serialize_StringContainerDto()
        {
            var container = new StringContainerDto {
                Source = text.ToJson(),
                Destination = image.ToJson()
            };

            var json = container.ToJson();

            var fromJson = json.FromJson<StringContainerDto>();

            var src = container.Source.FromJson<TextElementDto>();
            var dst = container.Destination.FromJson<ImageElementDto>();

            var fromSrc = fromJson.Source.FromJson<TextElementDto>();
            var fromDst = fromJson.Destination.FromJson<ImageElementDto>();

            Assert.That(src.Action.ElementId, Is.EqualTo(fromSrc.Action.ElementId));
            Assert.That(dst.Action, Is.EqualTo(fromDst.Action));
        }

        [Test]
        public void Can_handle_null_in_Collection_with_ShouldSerialize()
        {
            var dto = new Parent {
                ChildDtosWithShouldSerialize = new List<ChildWithShouldSerialize> {
                    new ChildWithShouldSerialize { Data = "xx" }, null,
                }
            };

            var json = JsonSerializer.SerializeToString(dto);
            Assert.That(json, Is.EqualTo("{\"ChildDtosWithShouldSerialize\":[{\"Data\":\"xx\"},{}]}"));
        }

        [Test]
        public void Can_handle_null_in_Collection_with_ShouldSerialize_PropertyName()
        {
            var dto = new Parent {
                ChildDtosWithShouldSerializeProperty = new List<ChildDtoWithShouldSerializeForProperty> {
                    new ChildDtoWithShouldSerializeForProperty {Data = "xx"},
                    null,
                }
            };

            var json = JsonSerializer.SerializeToString(dto);
            Assert.AreEqual(json, "{\"ChildDtosWithShouldSerializeProperty\":[{\"Data\":\"xx\"},{}]}");
        }

        public class Parent
        {
            public IList<ChildWithShouldSerialize> ChildDtosWithShouldSerialize { get; set; }
            public IList<ChildDtoWithShouldSerializeForProperty> ChildDtosWithShouldSerializeProperty { get; set; }
        }

        public class ChildWithShouldSerialize
        {
            protected virtual bool? ShouldSerialize(string fieldName)
            {
                return true;
            }

            public string Data { get; set; }
        }

        public class ChildDtoWithShouldSerializeForProperty
        {
            public virtual bool ShouldSerializeData()
            {
                return true;
            }

            public string Data { get; set; }
        }  

        readonly SimpleObj simple = new SimpleObj
        {
            value1 = "Foo",
            value2 = "Bar"
        };

        [Test]
        public void Can_Deserialize_JsonValue()
        {
            var json = simple.ToJson();
            var jsonValue = new JsonValue(json);

            var fromJson = jsonValue.As<SimpleObj>();

            Assert.That(fromJson.value1, Is.EqualTo(simple.value1));
            Assert.That(fromJson.value2, Is.EqualTo(simple.value2));
        }

        [Test]
        public void Can_Serialize_JsonValue_Multiple_Times()
        {
            var json = simple.ToJson();
            var jsonValue = new JsonValue(json);

            var jsonAfter = jsonValue.ToJson();

            Assert.That(jsonAfter, Is.EqualTo(json));
        }

        [Test]
        public void Can_Deserialize_JsonValue_After_Multiple_Serialize()
        {
            var json = simple.ToJson();
            var jsonValue = new JsonValue(json);

            jsonValue = new JsonValue(jsonValue.ToJson());

            var fromJson = jsonValue.As<SimpleObj>();

            Assert.That(fromJson.value1, Is.EqualTo(simple.value1));
            Assert.That(fromJson.value2, Is.EqualTo(simple.value2));
        }

        [Test]
        public void Can_Deserialize_JsonValue_After_Multiple_Serialize_2()
        {
            var json = simple.ToJson();
            var jsonValue = new JsonValue(json);

            json = jsonValue.ToJson();

            var fromJson = json.FromJson<SimpleObj>();

            Assert.That(fromJson.value1, Is.EqualTo(simple.value1));
            Assert.That(fromJson.value2, Is.EqualTo(simple.value2));
        }

        [Test]
        public void Can_Serialize_NestedJsonValueDto()
        {
            var scaffold = new List<JsonValue> { new JsonValue(text.ToJson()), new JsonValue(text.ToJson()) };

            var container = new NestedJsonValueDto
            {
                ElementId = "container_1",
                ElementType = "container",
                // Raw nesting - won't be escaped
                Content = new ElementContentDto { ElementId = "container_1", Content = "container goes here" },
                Action = new ElementActionDto { ElementId = "container_1", Action = "action goes here" },
                Scaffolding = scaffold
            };

            var json = container.ToJson();

            var fromJson = json.FromJson<NestedJsonValueDto>();

            foreach (var jsonValue in fromJson.Scaffolding)
            {
                var fromJsonValue = jsonValue.As<TextElementDto>();
                Assert.That(fromJsonValue.ElementId, Is.EqualTo(text.ElementId));
                Assert.That(fromJsonValue.Action.ElementId, Is.EqualTo(text.Action.ElementId));
                Assert.That(fromJsonValue.Content.ElementId, Is.EqualTo(text.Content.ElementId));
            }
        }

        public class SimpleObj
        {
            public string value1 { get; set; }
            public string value2 { get; set; }
        }

        public class NestedSimpleJsonValue
        {
            public JsonValue Simple { get; set; }
        }

        public class NestedJsonValueDto
        {
            public string ElementType { get; set; }
            public string ElementId { get; set; }

            public ElementContentDto Content { get; set; }
            public ElementActionDto Action { get; set; }

            public List<JsonValue> Scaffolding { get; set; }
        }

        public class TypedContainerDto
        {
            public TextElementDto Source { get; set; }
            public ImageElementDto Destination { get; set; }
        }
        // DTOs
        public class StringContainerDto // This is the request dto
        {
            public string Source { get; set; } // This will be some ElementDto
            public string Destination { get; set; } // This will be some ElementDto
        }

        // DTOs
        public class JsonValueContainerDto // This is the request dto
        {
            public JsonValue Source { get; set; } // This will be some ElementDto
            public JsonValue Destination { get; set; } // This will be some ElementDto
        }

        public class TextElementDto
        {
            public string ElementType { get; set; }
            public string ElementId { get; set; }

            public ElementContentDto Content { get; set; }
            public ElementActionDto Action { get; set; }
        }

        public class ImageElementDto
        {
            public string ElementType { get; set; }
            public string ElementId { get; set; }

            public string Content { get; set; }
            public string Action { get; set; }
        }

        public class ElementContentDto
        {
            public string ElementId { get; set; }
            public string Content { get; set; }
            // There can be more nested objects in here
        }

        public class ElementActionDto
        {
            public string ElementId { get; set; }
            public string Action { get; set; }
            // There can be more nested objects in here
        }

    }
}
