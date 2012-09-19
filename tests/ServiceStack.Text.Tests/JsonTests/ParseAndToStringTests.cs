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

	public struct ToStringAndStringConstructor {
		string value;
		public string Value
		{
			get { return value; }
			set { this.value = value; }
		}

		public override string ToString () {
			return "OK";
		}

		public ToStringAndStringConstructor(string value)
		{
			this.value = value;
		}
	}

	public class ContainerA
	{
		public ToStringAndParse Contents { get; set; }
	}
	public class ContainerB
	{
		public ToStringOnly Contents { get; set; }
	}
	public class ContainerC
	{
		public ToStringAndStringConstructor Contents { get; set; }
	}

	#endregion
	[TestFixture]
	public class ParseAndToStringTests {
		[Test]
		public void Should_use_ToString_if_type_has_parse_method () {
			var original = new ContainerA{Contents = new ToStringAndParse{Value="WRONG!"}};
			var str = original.ToJson();
			var copy = str.FromJson<ContainerA>();

			Console.WriteLine(str);
			Assert.That(copy.Contents.Value, Is.EqualTo("OK"));
		}
		[Test]
		public void Should_use_ToString_if_type_has_string_constructor () {
			var original = new ContainerC{Contents = new ToStringAndStringConstructor{Value="WRONG!"}};
			var str = original.ToJson();
			var copy = str.FromJson<ContainerC>();

			Console.WriteLine(str);
			Assert.That(copy.Contents.Value, Is.EqualTo("OK"));
		}

		[Test]
		public void Should_not_use_ToString_if_type_has_no_parse_method () {
			var original = new ContainerB{Contents = new ToStringOnly{Value="OK"}};
			var str = original.ToJson();
			Console.WriteLine(str);

			var copy = str.FromJson<ContainerB>();

			Assert.That(copy.Contents.Value, Is.EqualTo("OK"));
		}
	}
}
