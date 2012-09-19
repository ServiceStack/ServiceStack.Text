using System;
using NUnit.Framework;

namespace ServiceStack.Text.Tests.JsonTests {
	#region Test types
	public struct ToStringOnly {
		public string Value { get; set; }

		public override string ToString () {
			return "WRONG!";
		}
	}

	public struct ToStringAndParse {
		public string Value { get; set; }
		public override string ToString () {
			return "OK";
		}
		public static ToStringAndParse Parse (string value) {
			return new ToStringAndParse { Value = "OK" };
		}
	}

	public class Container
	{
		public object Contents { get; set; }
	}

	#endregion
	[TestFixture]
	public class ParseAndToStringTests {
		[Test]
		public void Should_use_ToString_if_type_has_parse_method () {
			var original = new Container{Contents = new ToStringAndParse{Value="WRONG!"}};
			var str = original.ToJson();
			var copy = str.FromJson<ToStringAndParse>();

			Console.WriteLine(str);
			Assert.That(copy.Value, Is.EqualTo("OK"));
		}

		[Test]
		public void Should_not_use_ToString_if_type_has_no_parse_method () {
			var original = new Container{Contents = new ToStringOnly{Value="OK"}};
			var str = original.ToJson();
			var copy = str.FromJson<ToStringOnly>();

			Console.WriteLine(str);
			Assert.That(copy.Value, Is.EqualTo("OK"));
		}
	}
}
