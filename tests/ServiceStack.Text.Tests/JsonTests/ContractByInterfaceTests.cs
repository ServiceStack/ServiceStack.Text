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
			IContract myConcrete = new Concrete("boo");
			var json = JsonSerializer.SerializeToString(myConcrete, typeof(IContract));

			Console.WriteLine(json);
			Assert.That(json, Is.StringContaining("\"ServiceStack.Text.Tests.JsonTests.IContract, ServiceStack.Text.Tests\""));
		}
		
		[Test]
		public void Should_not_use_interface_type_if_concrete_specified () {
			JsConfig.PreferInterfaces = false;
			IContract myConcrete = new Concrete("boo");
			var json = JsonSerializer.SerializeToString(myConcrete, typeof(IContract));

			Console.WriteLine(json);
			Assert.That(json, Is.StringContaining("\"ServiceStack.Text.Tests.JsonTests.Concrete, ServiceStack.Text.Tests\""));
		}
		
		[Test, Ignore("Not yet implemented")]
		public void Should_be_able_to_deserialise_based_on_an_interface () {
			JsConfig.PreferInterfaces = true;
			var json = JsonSerializer.SerializeToString(new Concrete("boo"), typeof(IContract));
			json = json.Replace("ServiceStack.Text.Tests.JsonTests.IContract", "ServiceStack.Text.Tests.JsonTests.IIdenticalContract");

			var result = JsonSerializer.DeserializeFromString<IIdenticalContract>(json);

			Assert.That(result.StringValue, Is.EqualTo("boo"));
		}
	}

	public class Concrete : IContract {
		public Concrete(string boo) {
			StringValue = boo;
		}

		public string StringValue { get; set; }
	}

	public interface IContract {
		string StringValue { get; set; }
	}
	public interface IIdenticalContract {
		string StringValue { get; set; }
	}
}
