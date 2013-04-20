﻿using System;
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
      public enum SbyteFlagEnum: sbyte
      {
          A,
          B
      }
	}
}

