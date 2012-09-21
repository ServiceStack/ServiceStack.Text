using System;
using NUnit.Framework;

namespace ServiceStack.Text.Tests.JsonTests
{

	#region Test types

	public class GetOnlyWithBacking
	{
		public long backing;

		public long Property
		{
			get { return backing; }
		}
	}
	public class GetSetWithBacking
	{
		long backing;

		public long Property
		{
			get { return backing; }
			set { backing = value; }
		}
	}

	#endregion

	[TestFixture]
	public class BackingFieldTests
	{		[Test]
		public void Backed_get_set_properties_can_be_deserialised()
		{
			var original = new GetSetWithBacking { Property = 123344044 };
			var str1 = original.ToJson();
			var copy = str1.FromJson<GetSetWithBacking>();

			Console.WriteLine(str1);

			Assert.That(copy.Property, Is.EqualTo(original.Property));
		}

		[Test]
		public void Backed_get_properties_can_be_deserialised()
		{
			TypeConfig<GetOnlyWithBacking>.EnableAnonymousFieldSetters = true;
			var original = new GetOnlyWithBacking { backing = 123344044 };
			var str1 = original.ToJson();
			var copy = str1.FromJson<GetOnlyWithBacking>();

			Console.WriteLine(str1);

			Assert.That(copy.Property, Is.EqualTo(original.Property));
		}
	}
}
