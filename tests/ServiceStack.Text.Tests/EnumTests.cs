using System;
using System.Collections.Generic;
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
            Zero = 0,
            One = 1,
            Two = 2
        }

        [Flags]
        public enum EnumWithFlags
        {
            Zero = 0,
            One = 1,
            Two = 2
        }

        public class ClassWithEnums
        {
            public EnumWithFlags FlagsEnum { get; set; }
            public EnumWithoutFlags NoFlagsEnum { get; set; }
            public EnumWithFlags? NullableFlagsEnum { get; set; }
            public EnumWithoutFlags? NullableNoFlagsEnum { get; set; }
        }

        [Test]
        public void Can_correctly_serialize_enums()
        {
            var item = new ClassWithEnums
            {
                FlagsEnum = EnumWithFlags.One,
                NoFlagsEnum = EnumWithoutFlags.One,
                NullableFlagsEnum = EnumWithFlags.Two,
                NullableNoFlagsEnum = EnumWithoutFlags.Two
            };

            const string expected = "{\"FlagsEnum\":1,\"NoFlagsEnum\":\"One\",\"NullableFlagsEnum\":2,\"NullableNoFlagsEnum\":\"Two\"}";
            var text = JsonSerializer.SerializeToString(item);

            Assert.AreEqual(expected, text);
        }

        [Test]
        public void Can_exclude_default_enums()
        {
            var item = new ClassWithEnums
            {
                FlagsEnum = EnumWithFlags.Zero,
                NoFlagsEnum = EnumWithoutFlags.One,
            };

            Assert.That(item.ToJson(), Is.EqualTo("{\"FlagsEnum\":0,\"NoFlagsEnum\":\"One\"}"));

            JsConfig.IncludeDefaultEnums = false;

            Assert.That(item.ToJson(), Is.EqualTo("{\"NoFlagsEnum\":\"One\"}"));

            JsConfig.Reset();
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

