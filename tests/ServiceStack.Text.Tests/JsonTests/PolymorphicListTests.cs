using System.Collections.Generic;
using NUnit.Framework;

namespace ServiceStack.Text.Tests.JsonTests
{
	//[KnownType(typeof(Dog))]
	//[KnownType(typeof(Cat))]
	public abstract class Animal
	{
		public abstract string Name
		{
			get;
			set;
		}
	}

	public class Dog : Animal
	{
		public override string Name
		{
			get;
			set;
		}
	}

	public class Cat : Animal
	{
		public override string Name
		{
			get;
			set;
		}
	}

	public class Zoo
	{
		public Zoo()
		{
			Animals = new List<Animal>
					{
						new Dog
							{
								Name = @"Fido"
							},
						new Cat
							{
								Name = @"Tigger"
							}
					};
		}

		public List<Animal> Animals
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}
	}

	[TestFixture]
	public class PolymorphicListTests
		: TestBase
	{
		[Test]
		public void Can_serialise_polymorphic_list()
		{
			var list = new List<Animal>
				{
					new Dog
						{
							Name = @"Fido"
						},
					new Cat
						{
							Name = @"Tigger"
						}
				};

			string asText = JsonSerializer.SerializeToString(list);

			Log(asText);

			Assert.That(
				asText,
				Is.EqualTo(
					"[{\"__type\":\"Dog:#ServiceStack.Text.Tests.JsonTests\",\"Name\":\"Fido\"},{\"__type\":\"Cat:#ServiceStack.Text.Tests.JsonTests\",\"Name\":\"Tigger\"}]"));
		}

		[Test]
		public void Can_serialise_an_entity_with_a_polymorphic_list()
		{
			var zoo = new Zoo
				{
					Name = @"City Zoo"
				};

			string asText = JsonSerializer.SerializeToString(zoo);

			Log(asText);

			Assert.That(
				asText,
				Is.EqualTo(
					"{\"Animals\":[{\"__type\":\"Dog:#ServiceStack.Text.Tests.JsonTests\",\"Name\":\"Fido\"},{\"__type\":\"Cat:#ServiceStack.Text.Tests.JsonTests\",\"Name\":\"Tigger\"}],\"Name\":\"City Zoo\"}"));
		}

		[Test]
		public void Can_deserialise_polymorphic_list()
		{
			var list =
				JsonSerializer.DeserializeFromString<List<Animal>>(
					"[{\"__type\":\"Dog:#ServiceStack.Text.Tests.JsonTests\",\"Name\":\"Fido\"},{\"__type\":\"Cat:#ServiceStack.Text.Tests.JsonTests\",\"Name\":\"Tigger\"}]");

			Assert.That(list.Count, Is.EqualTo(2));

			Assert.That(list[0].GetType(), Is.EqualTo(typeof(Dog)));
			Assert.That(list[1].GetType(), Is.EqualTo(typeof(Cat)));

			Assert.That(list[0].Name, Is.EqualTo(@"Fido"));
			Assert.That(list[1].Name, Is.EqualTo(@"Tigger"));
		}

		[Test]
		public void Can_deserialise_an_entity_containing_a_polymorphic_list()
		{
			var zoo =
				JsonSerializer.DeserializeFromString<Zoo>(
					"{\"Animals\":[{\"__type\":\"Dog:#ServiceStack.Text.Tests.JsonTests\",\"Name\":\"Fido\"},{\"__type\":\"Cat:#ServiceStack.Text.Tests.JsonTests\",\"Name\":\"Tigger\"}],\"Name\":\"City Zoo\"}");

			Assert.That(zoo.Name, Is.EqualTo(@"City Zoo"));

			var animals = zoo.Animals;

			Assert.That(animals[0].GetType(), Is.EqualTo(typeof(Dog)));
			Assert.That(animals[1].GetType(), Is.EqualTo(typeof(Cat)));

			Assert.That(animals[0].Name, Is.EqualTo(@"Fido"));
			Assert.That(animals[1].Name, Is.EqualTo(@"Tigger"));
		}
	}
}