using System;
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
			
			var expected = "{\"FlagsEnum\":1,\"NoFlagsEnum\":\"One\",\"NullableFlagsEnum\":2,\"NullableNoFlagsEnum\":\"Two\"}";
			var text = JsonSerializer.SerializeToString(item);
			
			Assert.AreEqual(expected, text);
		}
	}
}

