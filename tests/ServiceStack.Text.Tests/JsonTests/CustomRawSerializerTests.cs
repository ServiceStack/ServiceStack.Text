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
    }
}