using System;
using NUnit.Framework;

namespace ServiceStack.Text.Tests.JsonTests
{

	#region Test types

	public class GetOnlyWithBacking
	{
		long property;

		public GetOnlyWithBacking(long i)
		{
			property = i;
		}

		public long Property
		{
			get { return property; }
		}
	}
	public class GetSetWithBacking
	{
		long backing;

		public GetSetWithBacking(long i)
		{
			Property = i;
		}

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
			var original = new GetSetWithBacking(123344044);
			var str1 = original.ToJson();
			var copy = str1.FromJson<GetSetWithBacking>();

			Console.WriteLine(str1);

			Assert.That(copy.Property, Is.EqualTo(original.Property));
		}

		[Test]
		public void Backed_get_properties_can_be_deserialised()
		{
			TypeConfig<GetOnlyWithBacking>.EnableAnonymousFieldSetters = true;
			var original = new GetOnlyWithBacking(123344044);
			var str1 = original.ToJson();
			var copy = str1.FromJson<GetOnlyWithBacking>();

			Console.WriteLine(str1);

			// DeserializeType.cs Line ~145
			// use backing field guesseras last resort.

			Assert.That(copy.Property, Is.EqualTo(original.Property));
		}
	}
}
