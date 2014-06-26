using System;
using System.Globalization;
using System.Threading;
using NUnit.Framework;

namespace ServiceStack.Text.Tests
{
	[TestFixture]
	public class CultureInfoTests
		: TestBase
	{
		public class Point
		{
			public double Latitude { get; set; }
			public double Longitude { get; set; }

			public bool Equals(Point other)
			{
				if (ReferenceEquals(null, other)) return false;
				if (ReferenceEquals(this, other)) return true;
				return other.Latitude == Latitude && other.Longitude == Longitude;
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != typeof(Point)) return false;
				return Equals((Point)obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					return (Latitude.GetHashCode() * 397) ^ Longitude.GetHashCode();
				}
			}
		}

		private CultureInfo previousCulture = CultureInfo.InvariantCulture;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			previousCulture = Thread.CurrentThread.CurrentCulture;
			//Thread.CurrentThread.CurrentCulture = new CultureInfo("pt-BR");
			Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
			Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr-FR");
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			Thread.CurrentThread.CurrentCulture = previousCulture;
		}

		[Test]
		public void Can_deserialize_type_with_doubles_in_different_culture()
		{
			var point = new Point { Latitude = -23.5707, Longitude = -46.57239 };
			SerializeAndCompare(point);
		}

		[Test]
		public void Can_deserialize_type_with_Single_in_different_culture()
		{
			Single single = (float) 1.123;
			var txt = TypeSerializer.SerializeToString(single);

			Console.WriteLine(txt);
		}

		[Test]
		public void Serializes_doubles_using_InvariantCulture()
		{
			//Used in RedisClient
			var doubleUtf8 = 66121.202.ToUtf8Bytes();
			var doubleStr = doubleUtf8.FromUtf8Bytes();
			Assert.That(doubleStr, Is.EqualTo("66121.202"));
		}

		[Test]
		public void Serializes_long_double_without_E_notation()
		{
			//Used in RedisClient
			var doubleUtf8 = 1234567890123456d.ToUtf8Bytes();
			var doubleStr = doubleUtf8.FromUtf8Bytes();
			Assert.That(doubleStr, Is.EqualTo("1234567890123456"));
		}

        public class NumberClass
        {
            public int IntValue { get; set; }
            public uint UIntValue { get; set; }
            public long LongValue { get; set; }
            public ulong ULongValue { get; set; }
            public float FloatValue { get; set; }
            public double DoubleValue { get; set; }
            public decimal DecimalValue { get; set; }

            public static NumberClass Create(int i)
            {
                return new NumberClass
                {
                    IntValue = i*1000,
                    UIntValue = (uint) (i * 1000),
                    LongValue = i * 1000,
                    ULongValue = (ulong) (i * 1000),
                    FloatValue = (float) (i * 1000 + .999),
                    DoubleValue = i * 1000 + .999,
                    DecimalValue = (decimal) (i * 1000 + .999),
                };
            }
        }

	    [Test]
	    public void Does_use_invariant_culture_for_numbers()
	    {
	        var dto = NumberClass.Create(1);
            dto.ToJson().Print();
            dto.ToJsv().Print();
            dto.ToCsv().Print();

            Assert.That(dto.ToJson(), Is.Not.StringContaining("1000,9"));
            Assert.That(dto.ToJsv(), Is.Not.StringContaining("1000,9"));
            Assert.That(dto.ToCsv(), Is.Not.StringContaining("1000,9"));
	    }

	}
}