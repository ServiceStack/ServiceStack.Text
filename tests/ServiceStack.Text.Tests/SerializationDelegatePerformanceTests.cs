using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using NUnit.Framework;
using System.Diagnostics;

namespace ServiceStack.Text.Tests
{

    [TestFixture]
    public class SerializationDelegatePerformanceTests
        : TestBase
    {
        [TestFixtureSetUp]
        public void SetUp()
        {
            AddSerializeHooksForType<PerformanceTestHookClass>();
        }

        [TearDown]
        public void TearDown()
        {

        }

        [Test]
        public void TypeSerializer_Deserialize_Performance_WithoutHook()
        {
            var data = GenerateData<PerformanceTestClass>();

            var stringvalue = ServiceStack.Text.TypeSerializer.SerializeToString(data);

            Stopwatch watch = Stopwatch.StartNew();
			var deserializedData = ServiceStack.Text.TypeSerializer.DeserializeFromString<List<PerformanceTestClass>>(stringvalue);
			watch.Stop();

            Debug.WriteLine(String.Format("Elapsed time: {0}ms", watch.ElapsedMilliseconds));

            // should be at least less than 200ms
            Assert.LessOrEqual(watch.ElapsedMilliseconds, 200);
        }

        [Test]
        public void TypeSerializer_Deserialize_Performance_WitHook()
        {
            var data = GenerateData<PerformanceTestHookClass>();

            var stringvalue = ServiceStack.Text.TypeSerializer.SerializeToString(data);

            Stopwatch watch = Stopwatch.StartNew();
			var deserializedData = ServiceStack.Text.TypeSerializer.DeserializeFromString<List<PerformanceTestHookClass>>(stringvalue);
			watch.Stop();

            Debug.WriteLine(String.Format("Elapsed time: {0}ms", watch.ElapsedMilliseconds));

            // should be at least less than 600ms
            Assert.LessOrEqual(watch.ElapsedMilliseconds, 600);
        }

        [Test]
        public void TypeSerializer_Serialize_Performance_WithoutHook()
        {
            var data = GenerateData<PerformanceTestClass>();

            Stopwatch watch = Stopwatch.StartNew();
			var stringvalue = ServiceStack.Text.TypeSerializer.SerializeToString(data);
			watch.Stop();

            Debug.WriteLine(String.Format("Elapsed time: {0}ms", watch.ElapsedMilliseconds));
            
            // should be at least less than 100ms
            Assert.LessOrEqual(watch.ElapsedMilliseconds, 100);
        }

        [Test]
        public void TypeSerializer_Serialize_Performance_WithHook()
        {
            var data = GenerateData<PerformanceTestHookClass>();

            Stopwatch watch = Stopwatch.StartNew();
			var stringvalue = ServiceStack.Text.TypeSerializer.SerializeToString(data);
			watch.Stop();

            Debug.WriteLine(String.Format("Elapsed time: {0}ms", watch.ElapsedMilliseconds));

            // should be at least less than 100ms
            Assert.LessOrEqual(watch.ElapsedMilliseconds, 100);
        }

        private List<T> GenerateData<T>() where T : PerformanceTestClass, new()
        {
            List<T> result = new List<T>();

            for (int i = 0; i < 5000; i++)
            {
                T user = new T();
                user.FirstName = "Performance" + i;
                user.LastName = "Test";
                user.ID = i;
                user.Email = String.Format("mail{0}@test.com", i);
                user.UserName = "Test" + i;
                user.AddressID = i * 32;

                result.Add(user);    
            }

            return result;
        }

        public static void AddSerializeHooksForType<T>()
        {
            
                ServiceStack.Text.JsConfig<T>.OnSerializingFn = s =>
                {
                    return s;
                };
           
                ServiceStack.Text.JsConfig<T>.OnSerializedFn = s =>
                {
                  
                };           
            
                ServiceStack.Text.JsConfig<T>.OnDeserializedFn = s =>
                {                    
                    return s;
                };
            
        }

        class PerformanceTestClass : ServiceStack.Text.Tests.SerializationHookTests.HookTestSubClass
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public int ID { get; set; }
            public string Email { get; set; }
            public string UserName { get; set; }
            public int AddressID { get; set; }
        }

        class PerformanceTestHookClass : PerformanceTestClass { }
    }
}