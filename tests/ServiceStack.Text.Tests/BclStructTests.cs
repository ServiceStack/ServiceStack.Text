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
	}
}