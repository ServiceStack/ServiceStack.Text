using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NUnit.Framework;
using ServiceStack.Text.Tests.DynamicModels;

namespace ServiceStack.Text.Tests
{
    public class User
    {
        public string FirstName { get; set; }
        [DataMember]
        public string LastName { get; set; }
        public Car Car { get; set; }
    }

    public class UserFields
    {
        public string FirstName;
        public string LastName;
        public Car Car;
    }

    public class SubUser : User { }
    public class SubUserFields : UserFields { }

    public class Car
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class UserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Car { get; set; }
    }

    public enum Color
    {
        Red,
        Green,
        Blue
    }

    public enum OtherColor
    {
        Red,
        Green,
        Blue
    }


    public class IntNullableId
    {
        public int? Id { get; set; }
    }

    public class IntId
    {
        public int Id { get; set; }
    }

    public class BclTypes
    {
        public int Int { get; set; }
        public long Long { get; set; }
        public double Double { get; set; }
        public decimal Decimal { get; set; }
    }

    public class BclTypeStrings
    {
        public string Int { get; set; }
        public string Long { get; set; }
        public string Double { get; set; }
        public string Decimal { get; set; }
    }

    public class NullableConversion
    {
        public decimal Amount { get; set; }
    }

    public class NullableConversionDto
    {
        public decimal? Amount { get; set; }
    }

    public class NullableEnumConversion
    {
        public Color Color { get; set; }
    }

    public class ReallyNullableEnumConversion
    {
        public Color? Color { get; set; }
    }

    public class EnumConversion
    {
        public Color Color { get; set; }
    }

    public class NullableEnumConversionDto
    {
        public OtherColor? Color { get; set; }
    }

    public class EnumConversionDto
    {
        public OtherColor Color { get; set; }
    }

    public class EnumConversionStringDto
    {
        public string Color { get; set; }
    }

    public class EnumConversionIntDto
    {
        public int Color { get; set; }
    }

    public class ModelWithEnumerable
    {
        public IEnumerable<User> Collection { get; set; }
    }

    public class ModelWithList
    {
        public List<User> Collection { get; set; }
    }

    public class ModelWithArray
    {
        public User[] Collection { get; set; }
    }

    public class ModelWithHashSet
    {
        public HashSet<User> Collection { get; set; }
    }

    public class ModelWithIgnoredFields
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [ReadOnly]
        public int Ignored { get; set; }
    }

    public class ReadOnlyAttribute : AttributeBase { }

    [TestFixture]
    public class AutoMappingTests
    {
        [Test]
        public void Does_populate()
        {
            var user = new User()
            {
                FirstName = "Demis",
                LastName = "Bellot",
                Car = new Car() { Name = "BMW X6", Age = 3 }
            };

            var userDto = new UserDto().PopulateWith(user);

            Assert.That(userDto.FirstName, Is.EqualTo(user.FirstName));
            Assert.That(userDto.LastName, Is.EqualTo(user.LastName));
            Assert.That(userDto.Car, Is.EqualTo("{Name:BMW X6,Age:3}"));
        }

        [Test]
        public void Does_translate()
        {
            var user = new User()
            {
                FirstName = "Demis",
                LastName = "Bellot",
                Car = new Car() { Name = "BMW X6", Age = 3 }
            };

            var userDto = user.ConvertTo<UserDto>();

            Assert.That(userDto.FirstName, Is.EqualTo(user.FirstName));
            Assert.That(userDto.LastName, Is.EqualTo(user.LastName));
            Assert.That(userDto.Car, Is.EqualTo("{Name:BMW X6,Age:3}"));
        }

        [Test]
        public void Does_enumstringconversion_translate()
        {
            var conversion = new EnumConversion { Color = Color.Blue };
            var conversionDto = conversion.ConvertTo<EnumConversionStringDto>();

            Assert.That(conversionDto.Color, Is.EqualTo("Blue"));
        }

        [Test]
        public void Does_enumintconversion_translate()
        {
            var conversion = new EnumConversion { Color = Color.Green };
            var conversionDto = conversion.ConvertTo<EnumConversionIntDto>();

            Assert.That(conversionDto.Color, Is.EqualTo(1));
        }

        [Test]
        public void Does_nullableconversion_translate()
        {
            var conversion = new NullableConversion { Amount = 123.45m };
            var conversionDto = conversion.ConvertTo<NullableConversionDto>();

            Assert.That(conversionDto.Amount, Is.EqualTo(123.45m));
        }

        [Test]
        public void Does_Enumnullableconversion_translate()
        {
            var conversion = new NullableEnumConversion { Color = Color.Green };
            var conversionDto = conversion.ConvertTo<NullableEnumConversionDto>();

            Assert.That(conversionDto.Color, Is.EqualTo(OtherColor.Green));
        }

        [Test]
        public void Does_Enumconversion_translate()
        {
            var conversion = new NullableEnumConversion { Color = Color.Green };
            var conversionDto = conversion.ConvertTo<EnumConversionDto>();

            Assert.That(conversionDto.Color, Is.EqualTo(OtherColor.Green));
        }

        [Test]
        public void Does_ReallyEnumnullableconversion_translate()
        {
            var conversion = new ReallyNullableEnumConversion { Color = Color.Green };
            var conversionDto = conversion.ConvertTo<NullableEnumConversionDto>();

            Assert.That(conversionDto.Color, Is.EqualTo(OtherColor.Green));
        }

        [Test]
        public void Does_RealyEnumconversion_translate()
        {
            var conversion = new ReallyNullableEnumConversion { Color = Color.Green };
            var conversionDto = conversion.ConvertTo<EnumConversionDto>();

            Assert.That(conversionDto.Color, Is.EqualTo(OtherColor.Green));
        }

        [Test]
        public void Does_Enumconversion_translateFromNull()
        {
            var conversion = new ReallyNullableEnumConversion { Color = null };
            var conversionDto = conversion.ConvertTo<EnumConversionDto>();

            Assert.That(conversionDto.Color, Is.EqualTo(default(OtherColor)));
        }

        [Test]
        public void Does_translate_nullableInt_to_and_from()
        {
            var nullable = new IntNullableId();

            var nonNullable = nullable.ConvertTo<IntId>();

            nonNullable.Id = 10;

            var expectedNullable = nonNullable.ConvertTo<IntNullableId>();

            Assert.That(expectedNullable.Id.Value, Is.EqualTo(nonNullable.Id));
        }

        [Test]
        public void Does_translate_from_properties_to_fields()
        {
            var user = new User
            {
                FirstName = "Demis",
                LastName = "Bellot",
                Car = new Car { Name = "BMW X6", Age = 3 }
            };

            var to = user.ConvertTo<UserFields>();
            Assert.That(to.FirstName, Is.EqualTo(user.FirstName));
            Assert.That(to.LastName, Is.EqualTo(user.LastName));
            Assert.That(to.Car.Name, Is.EqualTo(user.Car.Name));
            Assert.That(to.Car.Age, Is.EqualTo(user.Car.Age));
        }

        [Test]
        public void Does_translate_from_fields_to_properties()
        {
            var user = new UserFields
            {
                FirstName = "Demis",
                LastName = "Bellot",
                Car = new Car { Name = "BMW X6", Age = 3 }
            };

            var to = user.ConvertTo<UserFields>();
            Assert.That(to.FirstName, Is.EqualTo(user.FirstName));
            Assert.That(to.LastName, Is.EqualTo(user.LastName));
            Assert.That(to.Car.Name, Is.EqualTo(user.Car.Name));
            Assert.That(to.Car.Age, Is.EqualTo(user.Car.Age));
        }

        [Test]
        public void Does_translate_from_inherited_propeties()
        {
            var user = new SubUser
            {
                FirstName = "Demis",
                LastName = "Bellot",
                Car = new Car { Name = "BMW X6", Age = 3 }
            };

            var to = user.ConvertTo<UserFields>();
            Assert.That(to.FirstName, Is.EqualTo(user.FirstName));
            Assert.That(to.LastName, Is.EqualTo(user.LastName));
            Assert.That(to.Car.Name, Is.EqualTo(user.Car.Name));
            Assert.That(to.Car.Age, Is.EqualTo(user.Car.Age));
        }

        [Test]
        public void Does_translate_to_inherited_propeties()
        {
            var user = new User
            {
                FirstName = "Demis",
                LastName = "Bellot",
                Car = new Car { Name = "BMW X6", Age = 3 }
            };

            var to = user.ConvertTo<SubUserFields>();
            Assert.That(to.FirstName, Is.EqualTo(user.FirstName));
            Assert.That(to.LastName, Is.EqualTo(user.LastName));
            Assert.That(to.Car.Name, Is.EqualTo(user.Car.Name));
            Assert.That(to.Car.Age, Is.EqualTo(user.Car.Age));
        }

        [Test]
        public void Can_convert_BclTypes()
        {
            Assert.That("from".ConvertTo<string>(), Is.EqualTo("from"));
            Assert.That(1.ConvertTo<long>(), Is.EqualTo(1L));
            Assert.That(2L.ConvertTo<int>(), Is.EqualTo(2));
            Assert.That(3.3d.ConvertTo<float>(), Is.EqualTo(3.3f));
            Assert.That(4.4d.ConvertTo<decimal>(), Is.EqualTo(4.4m));
        }

        [Test]
        public void Does_coerce_from_BclTypes_to_strings()
        {
            var from = new BclTypes
            {
                Int = 1,
                Long = 2,
                Double = 3.3,
                Decimal = 4.4m,
            };

            var to = from.ConvertTo<BclTypeStrings>();
            Assert.That(to.Int, Is.EqualTo("1"));
            Assert.That(to.Long, Is.EqualTo("2"));
            Assert.That(to.Double, Is.EqualTo("3.3"));
            Assert.That(to.Decimal, Is.EqualTo("4.4"));
        }

        [Test]
        public void Does_coerce_from_strings_to_BclTypes()
        {
            var from = new BclTypeStrings
            {
                Int = "1",
                Long = "2",
                Double = "3.3",
                Decimal = "4.4",
            };

            var to = from.ConvertTo<BclTypes>();
            Assert.That(to.Int, Is.EqualTo(1));
            Assert.That(to.Long, Is.EqualTo(2));
            Assert.That(to.Double, Is.EqualTo(3.3d));
            Assert.That(to.Decimal, Is.EqualTo(4.4m));
        }

        [Test]
        public void Does_map_only_properties_with_specified_Attribute()
        {
            var user = new User
            {
                FirstName = "Demis",
                LastName = "Bellot",
                Car = new Car { Name = "BMW X6", Age = 3 }
            };

            var to = new User();
            to.PopulateFromPropertiesWithAttribute(user, typeof(DataMemberAttribute));

            Assert.That(to.LastName, Is.EqualTo(user.LastName));
            Assert.That(to.FirstName, Is.Null);
            Assert.That(to.Car, Is.Null);
        }

        [Test]
        public void Does_convert_ModelWithAllTypes()
        {
            var to = ModelWithAllTypes.Create(1);
            var from = to.ConvertTo<ModelWithAllTypes>();

            Assert.That(to.Equals(from));
        }

        public bool MatchesUsers(IEnumerable<User> u1s, IEnumerable<User> u2s)
        {
            if (u1s == null || u2s == null)
                return false;

            var u1sList = u1s.ToList();
            var u2sList = u2s.ToList();

            if (u1sList.Count != u2sList.Count)
                return false;

            for (var i = 0; i < u1sList.Count; i++)
            {
                var u1 = u1sList[i];
                var u2 = u2sList[i];

                if (u1.FirstName != u2.FirstName)
                    return false;
                if (u1.LastName != u2.LastName)
                    return false;
                if (u1.Car.Name != u2.Car.Name)
                    return false;
                if (u1.Car.Age != u2.Car.Age)
                    return false;
            }

            return true;
        }

        [Test]
        public void Does_convert_models_with_collections()
        {
            var from = new ModelWithEnumerable
            {
                Collection = new[] {
                    new User { FirstName = "First1", LastName = "Last1", Car = new Car { Name = "Car1", Age = 1} },
                    new User { FirstName = "First2", LastName = "Last2", Car = new Car { Name = "Car2", Age = 2} },
                }
            };

            Assert.That(MatchesUsers(from.Collection, from.ConvertTo<ModelWithEnumerable>().Collection));
            Assert.That(MatchesUsers(from.Collection, from.ConvertTo<ModelWithList>().Collection));
            Assert.That(MatchesUsers(from.Collection, from.ConvertTo<ModelWithArray>().Collection));
            Assert.That(MatchesUsers(from.Collection, from.ConvertTo<ModelWithHashSet>().Collection));

            Assert.That(MatchesUsers(from.Collection, from.Collection.ConvertTo<IEnumerable<User>>()));
            Assert.That(MatchesUsers(from.Collection, from.Collection.ConvertTo<List<User>>()));
            Assert.That(MatchesUsers(from.Collection, from.Collection.ConvertTo<User[]>()));
            Assert.That(MatchesUsers(from.Collection, from.Collection.ConvertTo<HashSet<User>>()));

            var array = from.Collection.ToArray();
            Assert.That(MatchesUsers(array, from.Collection.ConvertTo<IEnumerable<User>>()));
            Assert.That(MatchesUsers(array, from.Collection.ConvertTo<List<User>>()));
            Assert.That(MatchesUsers(array, from.Collection.ConvertTo<User[]>()));
            Assert.That(MatchesUsers(array, from.Collection.ConvertTo<HashSet<User>>()));

            var hashset = from.Collection.ToHashSet();
            Assert.That(MatchesUsers(hashset, from.Collection.ConvertTo<IEnumerable<User>>()));
            Assert.That(MatchesUsers(hashset, from.Collection.ConvertTo<List<User>>()));
            Assert.That(MatchesUsers(hashset, from.Collection.ConvertTo<User[]>()));
            Assert.That(MatchesUsers(hashset, from.Collection.ConvertTo<HashSet<User>>()));
        }


        public class Customer
        {
            public CompanyInfo CompanyInfo { get; set; }
            // other properties
        }

        public class CustomerDto
        {
            public CompanyInfoDto CompanyInfo { get; set; }
        }

        public class CompanyInfo
        {
            public int Id { get; set; }
            public string ITN { get; set; }
        }

        public class CompanyInfoDto
        {
            public int Id { get; set; }
            public string ITN { get; set; }
        }

        [Test]
        public void Does_retain_null_properties()
        {
            var user = new User { FirstName = "Foo" };
            var userDto = user.ConvertTo<UserFields>();

            Assert.That(userDto.FirstName, Is.EqualTo("Foo"));
            Assert.That(userDto.LastName, Is.Null);
            Assert.That(userDto.Car, Is.Null);

            var customer = new Customer { CompanyInfo = null };
            var dto = customer.ConvertTo<CustomerDto>();

            Assert.That(dto.CompanyInfo, Is.Null);
        }

        [Test]
        public void Does_ignore_properties_without_attributes()
        {
            var model = new ModelWithIgnoredFields
            {
                Id = 1,
                Name = "Foo",
                Ignored = 2
            };

            var dto = new ModelWithIgnoredFields { Ignored = 10 }
                .PopulateFromPropertiesWithoutAttribute(model, typeof(ReadOnlyAttribute));

            Assert.That(dto.Id, Is.EqualTo(model.Id));
            Assert.That(dto.Name, Is.EqualTo(model.Name));
            Assert.That(dto.Ignored, Is.EqualTo(10));
        }

        public class IgnoredModel
        {
            public int Id { get; set; }

            [IgnoreDataMember]
            public int DataMemberIgnoreId { get; set; }

            [JsonIgnore]
            public int JsonIgnoreId { get; set; }
        }

        //Matches JSON.NET's [JsonIgnore] by name
        public class JsonIgnoreAttribute : AttributeBase { }

        [Test]
        public void Can_change_ignored_properties()
        {
            var dto = new IgnoredModel();

            JsConfig.IgnoreAttributesNamed = new[] {
                typeof(IgnoreDataMemberAttribute).Name //i.e. Remove [JsonIgnore] 
            };

            Assert.That(dto.ToJson(), Is.EqualTo("{\"Id\":0,\"JsonIgnoreId\":0}"));
        }
    }
}
