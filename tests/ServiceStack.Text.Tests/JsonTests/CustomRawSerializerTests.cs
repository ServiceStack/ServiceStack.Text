using System;
using NUnit.Framework;

namespace ServiceStack.Text.Tests.JsonTests
{
    [TestFixture]
    public class CustomRawSerializerTests
    {
        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            JsConfig.Reset();
        }
        
        public class RealType
        {
            public string Name { get; set; }
            public byte[] Data { get; set; }
        }

        [Test]
        public void Can_Serialize_TypeProperties_WithCustomFunction()
        {
            var test = new RealType { Name = "Test", Data = new byte[] { 1, 2, 3, 4, 5 } };

            // Act: now we set a custom function for byte[]
            JsConfig<byte[]>.RawSerializeFn = c =>
                {
                    var temp = new int[c.Length];
                    Array.Copy(c, temp, c.Length);
                    return JsonSerializer.SerializeToString(temp);
                };
            var json = JsonSerializer.SerializeToString(test);

            // Assert:
            Assert.That(json, Is.EquivalentTo("{\"Name\":\"Test\",\"Data\":[1,2,3,4,5]}"));
        }

        [Test]
        public void Can_Serialize_AnonymousTypeProperties_WithCustomFunction()
        {
            var test = new { Name = "Test", Data = new byte[] { 1, 2, 3, 4, 5 } };

            // Act: now we set a custom function for byte[]
            JsConfig<byte[]>.RawSerializeFn = c =>
                {
                    var temp = new int[c.Length];
                    Array.Copy(c, temp, c.Length);
                    return JsonSerializer.SerializeToString(temp);
                };
            var json = JsonSerializer.SerializeToString(test);

            // Assert:
            Assert.That(json, Is.EquivalentTo("{\"Name\":\"Test\",\"Data\":[1,2,3,4,5]}"));
        }

        [Test]
        public void Reset_ShouldClear_JsConfigT_CachedFunctions()
        {
            var test = new { Name = "Test", Data = new byte[] { 1, 2, 3, 4, 5 } };
            JsConfig<byte[]>.RawSerializeFn = c =>
                {
                    var temp = new int[c.Length];
                    Array.Copy(c, temp, c.Length);
                    return JsonSerializer.SerializeToString(temp);
                };
            var json = JsonSerializer.SerializeToString(test);

            Assert.That(json, Is.EquivalentTo("{\"Name\":\"Test\",\"Data\":[1,2,3,4,5]}"));
            // Act: now we set a custom function for byte[]
            JsConfig.Reset();
            json = JsonSerializer.SerializeToString(test);
            // Assert:
            Assert.That(json, Is.EquivalentTo("{\"Name\":\"Test\",\"Data\":\"AQIDBAU=\"}"));
        }

        [Test]
        public void Can_override_Guid_serialization_format()
        {
            var guid = new Guid("ADFA988B-01F6-490D-B65B-63750F869496");

            Assert.That(guid.ToJson().Trim('"'), Is.EqualTo("adfa988b01f6490db65b63750f869496"));
            Assert.That(guid.ToJsv(), Is.EqualTo("adfa988b01f6490db65b63750f869496"));

            JsConfig<Guid>.RawSerializeFn = x => x.ToString();

            Assert.That(guid.ToJson().Trim('"'), Is.EqualTo("adfa988b-01f6-490d-b65b-63750f869496"));
            Assert.That(guid.ToJsv(), Is.EqualTo("adfa988b-01f6-490d-b65b-63750f869496"));
        }

        public class Parent
        {
            public ICar Car { get; set; }
        }

        public interface ICar
        {
            string CarType { get; }
        }

        public class LuxaryCar : ICar
        {
            public string Sunroof { get; set; }

            public string CarType { get { return "Luxary"; } }
        }

        public class CheapCar : ICar
        {
            public bool HasCupHolder { get; set; }

            public string CarType { get { return "Cheap"; } }
        }

        [Test]
        public void Does_call_RawSerializeFn_for_toplevel_types()
        {
            JsConfig<ICar>.RawSerializeFn = SerializeCar;

            var luxaryParent = new Parent() { Car = new LuxaryCar() { Sunroof = "Big" } };
            var cheapParent = new Parent() { Car = new CheapCar() { HasCupHolder = true } };

            // Works when ICar is a child
            var luxaryParentJson = luxaryParent.ToJson();
            var cheapParentJson = cheapParent.ToJson();

            Assert.That(luxaryParentJson, Is.Not.StringContaining("__type"));
            Assert.That(cheapParentJson, Is.Not.StringContaining("__type"));

            ICar luxary = new LuxaryCar() { Sunroof = "Big" };
            ICar cheap = new CheapCar() { HasCupHolder = true };

            // ToJson() loses runtime cast of interface type, to keep it we need to specify it on call-site
            var luxaryJson = JsonSerializer.SerializeToString(luxary, typeof(ICar));
            var cheapJson = JsonSerializer.SerializeToString(cheap, typeof(ICar));

            Assert.That(luxaryJson, Is.Not.StringContaining("__type"));
            Assert.That(cheapJson, Is.Not.StringContaining("__type"));

            JsConfig.Reset();
        }

        private static string SerializeCar(ICar car)
        {
            var jsonObject = JsonObject.Parse(car.ToJson());

            if (jsonObject.ContainsKey("__type"))
                jsonObject.Remove("__type");

            return jsonObject.ToJson();
        }

        [Test]
        public void Does_call_RawSerializeFn_for_toplevel_concrete_type()
        {
            JsConfig<LuxaryCar>.RawSerializeFn = c => "{\"foo\":1}";

            ICar luxary = new LuxaryCar { Sunroof = "Big" };

            var luxaryJson = luxary.ToJson();

            Assert.That(luxaryJson, Is.StringContaining("foo"));

            JsConfig.Reset();
        }

        [Test]
        public void Can_call_different_nested_types_custom_serializers()
        {
            JsConfig<InnerType>.SerializeFn = o => InnerType.Serialize(o);
            JsConfig<InnerType>.DeSerializeFn = str => InnerType.Deserialize(str);
            JsConfig<OuterType>.RawSerializeFn = d => JsonSerializer.SerializeToString(d.P1);
            JsConfig<OuterType>.RawDeserializeFn = str =>
            {
                var d = str.FromJson<InnerType>();
                return new OuterType {
                    P1 = d
                };
            };

            var t = new InnerType { A = "Hello", B = "World" };

            var data = new OuterType { P1 = t };

            var json = data.ToJson();
            json.Print();

            Assert.That(json, Is.EqualTo(@"""Hello-World"""));

            var outer = json.FromJson<OuterType>();
            Assert.That(outer.P1.A, Is.EqualTo("Hello"));
            Assert.That(outer.P1.B, Is.EqualTo("World"));
        }
    }

    public class OuterType
    {
        public InnerType P1 { get; set; }
    }

    public class InnerType
    {
        public string A { get; set; }

        public string B { get; set; }

        public static string Serialize(InnerType o)
        {
            return o.A + "-" + o.B;
        }

        public static InnerType Deserialize(string s)
        {
            var p = s.Split('-');
            return new InnerType {
                A = p[0],
                B = p[1]
            };
        }
    }

}