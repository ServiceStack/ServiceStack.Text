using System;
using System.Drawing;
using NUnit.Framework;

namespace ServiceStack.Text.Tests
{
	public class BclStructTests : TestBase
	{
		[Test]
		public void Can_serialize_Color()
		{
			var color = Color.Red;

			var fromColor = Serialize(color);

			Assert.That(fromColor, Is.EqualTo(color));
		}

		public enum MyEnum
		{
			Enum1,
			Enum2,
			Enum3,
		}

		[Test]
		public void Can_serialize_arrays_of_enums()
		{
			var enums = new[] { MyEnum.Enum1, MyEnum.Enum2, MyEnum.Enum3 };
			var fromEnums = Serialize(enums);

			Assert.That(fromEnums[0], Is.EqualTo(MyEnum.Enum1));
			Assert.That(fromEnums[1], Is.EqualTo(MyEnum.Enum2));
			Assert.That(fromEnums[2], Is.EqualTo(MyEnum.Enum3));
		}
	}
}