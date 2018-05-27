using System.Collections.Generic;
using System.Collections.ObjectModel;
using Northwind.Common.DataModel;
using NUnit.Framework;
using ServiceStack.Common.Tests.Models;

namespace ServiceStack.Text.Tests
{
    [TestFixture]
    public class AutoMappingObjectDictionaryTests
    {
        [Test]
        public void Can_convert_Car_to_ObjectDictionary()
        {
            var dto = new Car { Age = 10, Name = "ZCar" };
            var map = dto.ToObjectDictionary();

            Assert.That(map["Age"], Is.EqualTo(dto.Age));
            Assert.That(map["Name"], Is.EqualTo(dto.Name));

            var fromDict = (Car)map.FromObjectDictionary(typeof(Car));
            Assert.That(fromDict.Age, Is.EqualTo(dto.Age));
            Assert.That(fromDict.Name, Is.EqualTo(dto.Name));
        }

        [Test]
        public void Can_convert_Cart_to_ObjectDictionary()
        {
            var dto = new User
            {
                FirstName = "First",
                LastName = "Last",
                Car = new Car { Age = 10, Name = "ZCar" },
            };

            var map = dto.ToObjectDictionary();

            Assert.That(map["FirstName"], Is.EqualTo(dto.FirstName));
            Assert.That(map["LastName"], Is.EqualTo(dto.LastName));
            Assert.That(((Car)map["Car"]).Age, Is.EqualTo(dto.Car.Age));
            Assert.That(((Car)map["Car"]).Name, Is.EqualTo(dto.Car.Name));

            var fromDict = map.FromObjectDictionary<User>();
            Assert.That(fromDict.FirstName, Is.EqualTo(dto.FirstName));
            Assert.That(fromDict.LastName, Is.EqualTo(dto.LastName));
            Assert.That(fromDict.Car.Age, Is.EqualTo(dto.Car.Age));
            Assert.That(fromDict.Car.Name, Is.EqualTo(dto.Car.Name));
        }

        [Test]
        public void Can_Convert_from_ObjectDictionary_with_Different_Types()
        {
            var map = new Dictionary<string, object>
            {
                { "FirstName", 1 },
                { "LastName", true },
                { "Car",  new SubCar { Age = 10, Name = "SubCar", Custom = "Custom"} },
            };

            var fromDict = (User)map.FromObjectDictionary(typeof(User));
            Assert.That(fromDict.FirstName, Is.EqualTo("1"));
            Assert.That(fromDict.LastName, Is.EqualTo(bool.TrueString));
            Assert.That(fromDict.Car.Age, Is.EqualTo(10));
            Assert.That(fromDict.Car.Name, Is.EqualTo("SubCar"));
        }

        [Test]
        public void Can_Convert_from_ObjectDictionary_with_Different_Types_with_camelCase_names()
        {
            var map = new Dictionary<string, object>
            {
                { "firstName", 1 },
                { "lastName", true },
                { "car",  new SubCar { Age = 10, Name = "SubCar", Custom = "Custom"} },
            };

            var fromDict = (User)map.FromObjectDictionary(typeof(User));
            Assert.That(fromDict.FirstName, Is.EqualTo("1"));
            Assert.That(fromDict.LastName, Is.EqualTo(bool.TrueString));
            Assert.That(fromDict.Car.Age, Is.EqualTo(10));
            Assert.That(fromDict.Car.Name, Is.EqualTo("SubCar"));
        }

        [Test]
        public void Can_Convert_from_ObjectDictionary_with_Read_Only_Dictionary()
        {
            var map = new Dictionary<string, object>
            {
                { "FirstName", 1 },
                { "LastName", true },
                { "Car",  new SubCar { Age = 10, Name = "SubCar", Custom = "Custom"} },
            };

            var readOnlyMap = new ReadOnlyDictionary<string, object>(map);

            var fromDict = (User)readOnlyMap.FromObjectDictionary(typeof(User));
            Assert.That(fromDict.FirstName, Is.EqualTo("1"));
            Assert.That(fromDict.LastName, Is.EqualTo(bool.TrueString));
            Assert.That(fromDict.Car.Age, Is.EqualTo(10));
            Assert.That(fromDict.Car.Name, Is.EqualTo("SubCar"));
        }

        public class QueryCustomers : QueryDb<Customer>
        {
            public string CustomerId { get; set; }
            public string[] CountryIn { get; set; }
            public string[] CityIn { get; set; }
        }

        [Test]
        public void Can_convert_from_ObjectDictionary_into_AutoQuery_DTO()
        {
            var map = new Dictionary<string, object>
            {
                { "CustomerId", "CustomerId"},
                { "CountryIn", new[]{"UK", "Germany"}},
                { "CityIn", "London,Berlin"},
                { "take", 5 },
                { "Meta", "{foo:bar}" },
            };

            var request = map.FromObjectDictionary<QueryCustomers>();

            Assert.That(request.CustomerId, Is.EqualTo("CustomerId"));
            Assert.That(request.CountryIn, Is.EquivalentTo(new[]{"UK", "Germany" }));
            Assert.That(request.CityIn, Is.EquivalentTo(new[]{ "London", "Berlin" }));
            Assert.That(request.Take, Is.EqualTo(5));
            Assert.That(request.Meta, Is.EquivalentTo(new Dictionary<string, object> {{"foo", "bar"}}));
        }

        public class Employee
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string DisplayName { get; set; }
        }

        [Test]
        public void Can_create_new_object_using_MergeIntoObjectDictionary()
        {
            var customer = new User { FirstName = "John", LastName = "Doe" };
            var map = customer.MergeIntoObjectDictionary(new { Initial = "Z" });
            map["DisplayName"] = map["FirstName"] + " " + map["Initial"] + " " + map["LastName"];
            var employee = map.FromObjectDictionary<Employee>();
            
            Assert.That(employee.DisplayName, Is.EqualTo("John Z Doe"));
        }

        [Test]
        public void Can_create_new_object_from_merged_objects()
        {
            var customer = new User { FirstName = "John", LastName = "Doe" };
            var map = MergeObjects(customer, new { Initial = "Z" });
            map["DisplayName"] = map["FirstName"] + " " + map["Initial"] + " " + map["LastName"];
            var employee = map.FromObjectDictionary<Employee>();

            Dictionary<string,object> MergeObjects(params object[] sources) {
                var to = new Dictionary<string, object>(); 
                sources.Each(x => x.ToObjectDictionary().Each(entry => to[entry.Key] = entry.Value));
                return to;
            }
            
            Assert.That(employee.DisplayName, Is.EqualTo("John Z Doe"));
        }

        [Test, TestCaseSource(nameof(TestDataFromObjectDictionaryWithNullableTypes))]
        public void Can_Convert_from_ObjectDictionary_with_Nullable_Properties(
            Dictionary<string, object> map,
            ModelWithFieldsOfNullableTypes expected)
        {
            var actual = map.FromObjectDictionary<ModelWithFieldsOfNullableTypes>();

            ModelWithFieldsOfNullableTypes.AssertIsEqual(actual, expected);
        }

        private static IEnumerable<TestCaseData> TestDataFromObjectDictionaryWithNullableTypes
        {
            get
            {
                var defaults = ModelWithFieldsOfNullableTypes.CreateConstant(1);

                yield return new TestCaseData(
                    new Dictionary<string, object>
                    {
                        { "Id", defaults.Id },
                        { "NId", defaults.NId },
                        { "NLongId", defaults.NLongId },
                        { "NGuid", defaults.NGuid },
                        { "NBool", defaults.NBool },
                        { "NDateTime", defaults.NDateTime },
                        { "NFloat", defaults.NFloat },
                        { "NDouble", defaults.NDouble },
                        { "NDecimal", defaults.NDecimal },
                        { "NTimeSpan", defaults.NTimeSpan }
                    },
                    defaults).SetName("All values populated");

                yield return new TestCaseData(
                    new Dictionary<string, object>
                    {
                        { "Id", defaults.Id.ToString() },
                        { "NId", defaults.NId.ToString() },
                        { "NLongId", defaults.NLongId.ToString() },
                        { "NGuid", defaults.NGuid.ToString() },
                        { "NBool", defaults.NBool.ToString() },
                        { "NDateTime", defaults.NDateTime.ToString() },
                        { "NFloat", defaults.NFloat.ToString() },
                        { "NDouble", defaults.NDouble.ToString() },
                        { "NDecimal", defaults.NDecimal.ToString() },
                        { "NTimeSpan", defaults.NTimeSpan.ToString() }
                    },
                    defaults).SetName("All values populated as strings");
            }
        }
    }


}