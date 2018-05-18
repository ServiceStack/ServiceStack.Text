﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
#if !NETCORE
using System.Web.Script.Serialization;
#endif
using NUnit.Framework;
using ServiceStack.Text.Tests.DynamicModels;
using ServiceStack.Web;

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

    public class SubCar : Car
    {
        public string Custom { get; set; }
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
        public Color? NullableColor { get; set; }
    }

    public class NullableEnumConversionDto
    {
        public OtherColor? Color { get; set; }
    }

    public class EnumConversionDto
    {
        public OtherColor Color { get; set; }
    }

    public class EnumConversionString
    {
        public string Color { get; set; }
        public string NullableColor { get; set; }
    }

    public class EnumConversionInt
    {
        public int Color { get; set; }
        public int? NullableColor { get; set; }
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
            var user = new User
            {
                FirstName = "Demis",
                LastName = "Bellot",
                Car = new Car { Name = "BMW X6", Age = 3 }
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
            var conversionDto = conversion.ConvertTo<EnumConversionString>();

            Assert.That(conversionDto.Color, Is.EqualTo("Blue"));
        }

        [Test]
        public void Does_convert_to_EnumConversionInt()
        {
            var conversion = new EnumConversion
            {
                Color = Color.Green,
                NullableColor = Color.Green,
            };
            var conversionDto = conversion.ConvertTo<EnumConversionInt>();

            Assert.That(conversionDto.Color, Is.EqualTo(1));
            Assert.That(conversionDto.NullableColor, Is.EqualTo(1));
        }

        [Test]
        public void Does_convert_from_EnumConversionInt()
        {
            var conversion = new EnumConversionInt
            {
                Color = 1,
                NullableColor = 1,
            };
            var conversionDto = conversion.ConvertTo<EnumConversion>();

            Assert.That(conversionDto.Color, Is.EqualTo(Color.Green));
            Assert.That(conversionDto.NullableColor, Is.EqualTo(Color.Green));
        }

        [Test]
        public void Does_convert_to_EnumConversionString()
        {
            var conversion = new EnumConversion
            {
                Color = Color.Green,
                NullableColor = Color.Green,
            };
            var conversionDto = conversion.ConvertTo<EnumConversionString>();

            Assert.That(conversionDto.Color, Is.EqualTo("Green"));
            Assert.That(conversionDto.NullableColor, Is.EqualTo("Green"));
        }

        [Test]
        public void Does_convert_from_EnumConversionString()
        {
            var conversion = new EnumConversionString
            {
                Color = "Green",
                NullableColor = "Green",
            };
            var conversionDto = conversion.ConvertTo<EnumConversion>();

            Assert.That(conversionDto.Color, Is.EqualTo(Color.Green));
            Assert.That(conversionDto.NullableColor, Is.EqualTo(Color.Green));
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

#if !NETCORE
        public class IgnoredModel
        {
            public int Id { get; set; }

            [IgnoreDataMember]
            public int DataMemberIgnoreId { get; set; }

            [JsonIgnore]
            public int JsonIgnoreId { get; set; }

            [ScriptIgnore]
            public int ScriptIgnoreId { get; set; }
        }

        //Matches JSON.NET's [JsonIgnore] by name
        public class JsonIgnoreAttribute : AttributeBase { }

        [Test]
        public void Can_change_ignored_properties()
        {
            JsConfig.IgnoreAttributesNamed = JsConfig.IgnoreAttributesNamed.NewArray(
                with: typeof(ScriptIgnoreAttribute).Name,
                without: typeof(JsonIgnoreAttribute).Name);

            var dto = new IgnoredModel { JsonIgnoreId = 1, ScriptIgnoreId = 2 };

            Assert.That(dto.ToJson(), Is.EqualTo("{\"Id\":0,\"JsonIgnoreId\":1}"));
        }
#endif

        [Test]
        public void Does_convert_to_ValueType()
        {
            Assert.That("1".ConvertTo(typeof(int)), Is.EqualTo(1));
            Assert.That("1".ConvertTo(typeof(long)), Is.EqualTo(1L));
            Assert.That("1.1".ConvertTo(typeof(float)), Is.EqualTo(1.1f));
            Assert.That("1.1".ConvertTo(typeof(double)), Is.EqualTo(1.1d));
            Assert.That("1.1".ConvertTo(typeof(decimal)), Is.EqualTo(1.1M));

            Assert.That("2001-01-01".ConvertTo<DateTime>(), Is.EqualTo(new DateTime(2001, 01, 01)));
            Assert.That("98ece8400be4452eb6ad7c3a4404f119".ConvertTo<Guid>(), Is.EqualTo(new Guid("98ece8400be4452eb6ad7c3a4404f119")));
        }

        [Test]
        public void Does_convert_from_ValueType_to_strings()
        {
            Assert.That(1.ConvertTo(typeof(string)), Is.EqualTo("1"));
            Assert.That(1L.ConvertTo(typeof(string)), Is.EqualTo("1"));
            Assert.That(1.1f.ConvertTo(typeof(string)), Is.EqualTo("1.1"));
            Assert.That(1.1d.ConvertTo(typeof(string)), Is.EqualTo("1.1"));
            Assert.That(1.1M.ConvertTo(typeof(string)), Is.EqualTo("1.1"));
            
            Assert.That(new DateTime(2001, 01, 01).ConvertTo<string>(), Is.EqualTo("2001-01-01"));
            Assert.That(new Guid("98ECE840-0BE4-452E-B6AD-7C3A4404F119").ConvertTo<string>(), Is.EqualTo("98ece8400be4452eb6ad7c3a4404f119"));
        }

        [Test]
        public void Can_convert_from_List_object()
        {
            var from = 3.Times(i => (object)new Car { Age = i, Name = "Name" + i });
            var to = (List<Car>)TranslateListWithElements.TryTranslateCollections(
                typeof(List<object>), typeof(List<Car>), from);

            Assert.That(to.Count, Is.EqualTo(3));
            Assert.That(to[0].Age, Is.EqualTo(0));
            Assert.That(to[0].Name, Is.EqualTo("Name0"));
        }

        [Test]
        public void Can_convert_from_List_SubType()
        {
            var from = 3.Times(i => new SubCar { Age = i, Name = "Name" + i });
            var to = (List<Car>)TranslateListWithElements.TryTranslateCollections(
                typeof(List<SubCar>), typeof(List<Car>), from);

            Assert.That(to.Count, Is.EqualTo(3));
            Assert.That(to[0].Age, Is.EqualTo(0));
            Assert.That(to[0].Name, Is.EqualTo("Name0"));
        }

        [Test]
        public void Can_create_Dictionary_default_value()
        {
            var obj = (Dictionary<string, ClassWithEnum>)AutoMappingUtils.CreateDefaultValue(typeof(Dictionary<string, ClassWithEnum>), new Dictionary<Type, int>());
            Assert.That(obj, Is.Not.Null);
        }
    }

    public enum ClassWithEnumType
    {
        One = 1,
        Two = 2,
        Three = 3
    }

    public class ClassWithEnum
    {
        public ClassWithEnumType Type { get; set; }
    }    

    public class Test
    {
        public string Name { get; set; }
    }

    public class PropertyExpressionTests
    {
        [Test]
        public void Can_call_typed_setter_Expressions()
        {
            var nameProperty = typeof(Test).GetProperty("Name");
            var setMethod = nameProperty.GetSetMethod();

            var instance = Expression.Parameter(typeof(Test), "i");
            var argument = Expression.Parameter(typeof(string), "a");

            var setterCall = Expression.Call(instance, setMethod, argument);
            var fn = Expression.Lambda<Action<Test, string>>(setterCall, instance, argument).Compile();

            var test = new Test();
            fn(test, "Foo");
            Assert.That(test.Name, Is.EqualTo("Foo"));
        }

        [Test]
        public void Can_call_object_setter_Expressions()
        {
            var nameProperty = typeof(Test).GetProperty("Name");

            var instance = Expression.Parameter(typeof(object), "i");
            var argument = Expression.Parameter(typeof(object), "a");

            var instanceParam = Expression.Convert(instance, nameProperty.ReflectedType);
            var valueParam = Expression.Convert(argument, nameProperty.PropertyType);

            var setterCall = Expression.Call(instanceParam, nameProperty.GetSetMethod(nonPublic:true), valueParam);

            var fn = Expression.Lambda<Action<object, object>>(setterCall, instance, argument).Compile();

            var test = new Test();
            fn(test, "Foo");
            Assert.That(test.Name, Is.EqualTo("Foo"));
        }

        public class RawRequest : IRequiresRequestStream
        {
            public Stream RequestStream { get; set; }
        }

        [Test]
        public void Can_create_DTO_with_Stream()
        {
            var o = typeof(RawRequest).CreateInstance();
            var requestObj = AutoMappingUtils.PopulateWith(o);

            Assert.That(requestObj, Is.Not.Null);
        }
    }
}
