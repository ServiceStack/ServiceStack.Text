using System;
using NUnit.Framework;

namespace ServiceStack.Text.Tests.JsonTests
{

	#region Test types

	public class Timestamp
	{
		long x;

		public Timestamp(long i)
		{
			x = i;
		}

		public long UnixTime { get { return x; }
		}
	}

	#endregion

	[TestFixture]
	public class LongTypeTests
	{
		[Test]
		public void Backed_get_properties_can_be_deserialised()
		{
			var original = new Timestamp(123344044);
			var str1 = original.ToJson();
			var copy = str1.FromJson<Timestamp>();

			Console.WriteLine(str1);

			Assert.That(copy.UnixTime, Is.EqualTo(original.UnixTime));
		}
	}
}
