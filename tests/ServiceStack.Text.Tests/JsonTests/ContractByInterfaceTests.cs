using System;
using NUnit.Framework;

namespace ServiceStack.Text.Tests.JsonTests {
	/// <summary>
	/// Service Bus messaging works best if processes can share interface message contracts
	/// but not have to share concrete types.
	/// </summary>
	[TestFixture]
	public class ContractByInterfaceTests {
		[Test]
		public void Should_be_able_to_serialise_based_on_an_interface () {
			JsConfig.PreferInterfaces = true;
			IContract myConcrete = new Concrete("boo", 1);
			var json = JsonSerializer.SerializeToString(myConcrete, typeof(IContract));

			Console.WriteLine(json);
			Assert.That(json, Is.StringContaining("\"ServiceStack.Text.Tests.JsonTests.IContract, ServiceStack.Text.Tests\""));
		}
		
		[Test]
		public void Should_not_use_interface_type_if_concrete_specified () {
			JsConfig.PreferInterfaces = false;
			IContract myConcrete = new Concrete("boo", 1);
			var json = JsonSerializer.SerializeToString(myConcrete, typeof(IContract));

			Console.WriteLine(json);
			Assert.That(json, Is.StringContaining("\"ServiceStack.Text.Tests.JsonTests.Concrete, ServiceStack.Text.Tests\""));
		}
		
		[Test]
		public void Should_be_able_to_deserialise_based_on_an_interface_with_no_concrete () {
			JsConfig.PreferInterfaces = true;
			var json = JsonSerializer.SerializeToString(new Concrete("boo", 42), typeof(IContract));
			json = json.Replace("ServiceStack.Text.Tests.JsonTests.IContract", "ServiceStack.Text.Tests.JsonTests.IIdenticalContract");

			Console.WriteLine(json);
			var result = JsonSerializer.DeserializeFromString<IIdenticalContract>(json);

			Assert.That(result.StringValue, Is.EqualTo("boo"));
			Assert.That(result.ChildProp.IntValue, Is.EqualTo(42));
		}
	}

	public class Concrete : IContract {
		public Concrete(string boo, int i) {
			StringValue = boo;
			ChildProp = new ConcreteChild { IntValue = i };
		}

		public string StringValue { get; set; }
		public IChildInterface ChildProp { get; set; }
	}
	public class ConcreteChild : IChildInterface {
		public int IntValue { get; set; }
	}

	public interface IChildInterface {
		int IntValue { get; set; }
	}

	public interface IContract {
		string StringValue { get; set; }
		IChildInterface ChildProp { get; set; }
	}
	public interface IIdenticalContract {
		string StringValue { get; set; }
		IChildInterface ChildProp { get; set; }
	}
}
