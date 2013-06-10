using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using NUnit.Framework;

namespace ServiceStack.Text.Tests
{
    [TestFixture]
    public class EnumTests
    {
        [SetUp]
        public void SetUp()
        {
            JsConfig.Reset();
        }

        public enum EnumWithoutFlags
        {
            One = 1,
            Two = 2
        }

        [Flags]
        public enum EnumWithFlags
        {
            One = 1,
            Two = 2
        }

        [DataContract(Namespace = "urn:example")]
        public enum EnumWithContract
        {
            NotPresentInContract,
            [EnumMember(Value="contract field 1")]
            ContractField1,
            [EnumMember(Value = "contract field 2")]
            ContractField2,
        }

        public class ClassWithEnums
        {
            public EnumWithFlags FlagsEnum { get; set; }
            public EnumWithoutFlags NoFlagsEnum { get; set; }
            public EnumWithFlags? NullableFlagsEnum { get; set; }
            public EnumWithoutFlags? NullableNoFlagsEnum { get; set; }
            public EnumWithContract ContractEnum { get; set; }
        }

        [Test]
        public void Can_correctly_serialize_enums()
        {
            var item = new ClassWithEnums
            {
                FlagsEnum = EnumWithFlags.One,
                NoFlagsEnum = EnumWithoutFlags.One,
                NullableFlagsEnum = EnumWithFlags.Two,
                NullableNoFlagsEnum = EnumWithoutFlags.Two,
                ContractEnum = EnumWithContract.ContractField1
            };

            const string expected = "{\"FlagsEnum\":1,\"NoFlagsEnum\":\"One\",\"NullableFlagsEnum\":2,\"NullableNoFlagsEnum\":\"Two\",\"ContractEnum\":\"contract field 1\"}";
            var text = JsonSerializer.SerializeToString(item);

            Assert.AreEqual(expected, text);
        }

        public void Should_deserialize_enum()
        {
            Assert.That(JsonSerializer.DeserializeFromString<EnumWithoutFlags>("\"Two\""), Is.EqualTo(EnumWithoutFlags.Two));
        }

        public void Should_handle_empty_enum()
        {
            Assert.That(JsonSerializer.DeserializeFromString<EnumWithoutFlags>(""), Is.EqualTo((EnumWithoutFlags)0));
        }

        [Test]
        public void CanSerializeIntFlag()
        {
            JsConfig.TreatEnumAsInteger = true;
            var val = JsonSerializer.SerializeToString(FlagEnum.A);

            Assert.AreEqual("0", val);
        }

        [Test]
        public void CanSerializeSbyteFlag()
        {
            JsConfig.TryToParsePrimitiveTypeValues = true;
            JsConfig.TreatEnumAsInteger = true;
            JsConfig.IncludeNullValues = true;
            var val = JsonSerializer.SerializeToString(SbyteFlagEnum.A);

            Assert.AreEqual("0", val);
        }

        [Test]
        public void CanSerializeDataContract()
        {
            var val = JsonSerializer.SerializeToString(EnumWithContract.ContractField2);
            Assert.AreEqual("\"contract field 2\"", val);
        }

        [Test]
        public void CanDeserializeDataContract()
        {
            var val = JsonSerializer.DeserializeFromString("\"contract field 2\"", typeof(EnumWithContract));
            Assert.AreEqual(val, EnumWithContract.ContractField2);
        }


        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void CanThrowOnDeserializeWhenEnumValueNotInContract()
        {
            //should throw ArgumentException (like Enum.Parse)
            var val = JsonSerializer.DeserializeFromString("\"NotPresentInContract\"", typeof(EnumWithContract));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void CanThrowOnSerializeWhenEnumValueNotInContract()
        {
            //should throw ArgumentException (the XML serializer throws SerializationException, would this be preferrable?)
            var val = JsonSerializer.SerializeToString(EnumWithContract.NotPresentInContract);
        }


        [Flags]
        public enum FlagEnum
        {
            A,
            B
        }

        [Flags]
        public enum SbyteFlagEnum : sbyte
        {
            A,
            B
        }

        [Flags]
        public enum AnEnum
        {
            This,
            Is,
            An,
            Enum
        }

        [Test]
        public void Can_use_enum_as_key_in_map()
        {
            var dto = new Dictionary<AnEnum, int> { { AnEnum.This, 1 } };
            var json = dto.ToJson();
            json.Print();
            
            var map = json.FromJson<Dictionary<AnEnum, int>>();
            Assert.That(map[AnEnum.This], Is.EqualTo(1));
        }

    }
}

