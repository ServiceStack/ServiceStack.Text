using NUnit.Framework;
using System;

namespace ServiceStack.Text.Tests.CsvTests
{
    [TestFixture]
    public class ObjectSerializerTests
    {
        [Test]
        public void IEnumerableObjectSerialization()
        {
            var data = GenerateSampleData();

            JsConfig<DateTime>.SerializeFn =
                time => new DateTime(time.Ticks, DateTimeKind.Utc).ToString("yyyy-MM-dd HH:mm:ss");

            var csv = CsvSerializer.SerializeToCsv(data);
            Console.WriteLine(csv);

            Assert.AreEqual("DateTime\r\n"
                + "2017-06-14 00:00:00\r\n"
                + "2017-01-31 01:23:45\r\n",
                csv);
        }

        [Test]
        public void IEnumerableObjectSerializationBaseline()
        {
            var data = new object[]
            {
                new { Value = true },
                new { Value = false },
                new { Value = new bool?() }
            };

            var csv = CsvSerializer.SerializeToCsv(data);
            Console.WriteLine(csv);

            Assert.AreEqual("Value\r\n"
                + "True\r\n"
                + "False\r\n"
                + "\r\n",
                csv);
        }

        [Test]
        public void IEnumerableObjectSerializationCustomSerializer()
        {
            var data = new object[]
            {
                new { Value = true },
                new { Value = false }
            };

            JsConfig<bool>.SerializeFn =
                value => value == true ? "Yes" : "No";

            var csv = CsvSerializer.SerializeToCsv(data);
            Console.WriteLine(csv);

            Assert.AreEqual("Value\r\n"
                + "Yes\r\n"
                + "No\r\n",
                csv);
        }

        [Test]
        public void IEnumerableObjectSerializationCustomSerializerOfNullableType()
        {
            var data = new object[]
            {
                new { Value = new bool?(true) },
                new { Value = new bool?(false) },
                new { Value = new bool?() }
            };

            JsConfig<bool?>.SerializeFn =
                value => value.HasValue ? (value == true ? "Yes" : "No") : "Maybe";

            var csv = CsvSerializer.SerializeToCsv(data);
            Console.WriteLine(csv);

            Assert.AreEqual("Value\r\n"
                + "Yes\r\n"
                + "No\r\n"
                + "\r\n",
                csv);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            JsConfig<bool>.SerializeFn = null;
            JsConfig<bool>.Reset();
            JsConfig<bool?>.SerializeFn = null;
            JsConfig<bool?>.Reset();
            JsConfig<DateTime>.SerializeFn = null;
            JsConfig<DateTime>.Reset();

            CsvConfig.Reset();
            JsConfig.Reset();
        }

        object[] GenerateSampleData()
        {
            return new object[] {
            new POCO
            {
                DateTime = new DateTime(2017,6,14)
            },
            new POCO
            {
                DateTime = new DateTime(2017,1,31, 01, 23, 45)
            }
         };
        }

    }

    public class POCO
    {
        public DateTime DateTime { get; set; }
    }
}