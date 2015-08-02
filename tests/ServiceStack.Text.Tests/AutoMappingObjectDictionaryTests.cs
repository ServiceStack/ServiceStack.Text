using System.Collections.Generic;
using NUnit.Framework;

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

            var fromDict = (User)map.FromObjectDictionary(typeof(User));
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
    }
}