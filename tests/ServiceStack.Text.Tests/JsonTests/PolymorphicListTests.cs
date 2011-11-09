using System;
using System.Collections.Generic;
using NUnit.Framework;
using ServiceStack.Text.Common;

namespace ServiceStack.Text.Tests.JsonTests
{
	public interface ICat
	{
		string Name { get; set; }
	}

	public interface IDog
	{
		string Name { get; set; }
	}

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

	public class Dog : Animal, IDog
	{
		public override string Name
		{
			get;
			set;
		}
	}

	public class Cat : Animal, ICat
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
				new Dog { Name = @"Fido" },
				new Cat { Name = @"Tigger" }
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
	public class PolymorphicListTests : TestBase
	{
		[SetUp]
		public void SetUp()
		{
			JsConfig.Reset();
			JsConfig<ICat>.ExcludeTypeInfo = false;
		}

		[Test]
		public void Can_serialise_polymorphic_list()
		{
			var list = new List<Animal>
			{
				new Dog { Name = @"Fido" },
				new Cat { Name = @"Tigger"}
			};

			var asText = JsonSerializer.SerializeToString(list);

			Log(asText);

			Assert.That(asText,
				Is.EqualTo(
					"[{\"__type\":\""
					+ typeof(Dog).ToTypeString()
					+ "\",\"Name\":\"Fido\"},{\"__type\":\""
					+ typeof(Cat).ToTypeString()
					+ "\",\"Name\":\"Tigger\"}]"));
		}

		[Test]
		public void Can_serialise_an_entity_with_a_polymorphic_list()
		{
			var zoo = new Zoo {
				Name = @"City Zoo"
			};

			string asText = JsonSerializer.SerializeToString(zoo);

			Log(asText);

			Assert.That(
				asText,
				Is.EqualTo(
					"{\"Animals\":[{\"__type\":\""
					+ typeof(Dog).ToTypeString()
					+ "\",\"Name\":\"Fido\"},{\"__type\":\""
					+ typeof(Cat).ToTypeString()
					+ "\",\"Name\":\"Tigger\"}],\"Name\":\"City Zoo\"}"));
		}

		[Test]
		public void Can_deserialise_polymorphic_list()
		{
			var list =
				JsonSerializer.DeserializeFromString<List<Animal>>(
					"[{\"__type\":\""
					+ typeof(Dog).ToTypeString()
					+ "\",\"Name\":\"Fido\"},{\"__type\":\""
					+ typeof(Cat).ToTypeString()
					+ "\",\"Name\":\"Tigger\"}]");

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
					"{\"Animals\":[{\"__type\":\""
					+ typeof(Dog).ToTypeString()
					+ "\",\"Name\":\"Fido\"},{\"__type\":\""
					+ typeof(Cat).ToTypeString()
					+ "\",\"Name\":\"Tigger\"}],\"Name\":\"City Zoo\"}");

			Assert.That(zoo.Name, Is.EqualTo(@"City Zoo"));

			var animals = zoo.Animals;

			Assert.That(animals[0].GetType(), Is.EqualTo(typeof(Dog)));
			Assert.That(animals[1].GetType(), Is.EqualTo(typeof(Cat)));

			Assert.That(animals[0].Name, Is.EqualTo(@"Fido"));
			Assert.That(animals[1].Name, Is.EqualTo(@"Tigger"));
		}

		public class Pets
		{
			public ICat Cat { get; set; }
			public IDog Dog { get; set; }
		}

		[Test]
		public void Can_exclude_specific_TypeInfo()
		{
			JsConfig<ICat>.ExcludeTypeInfo = true;
			var pets = new Pets {
				Cat = new Cat { Name = "Cat" },
				Dog = new Dog { Name = "Dog" },
			};

			Assert.That(pets.ToJson(), Is.EqualTo(
				@"{""Cat"":{""Name"":""Cat""},""Dog"":{""__type"":""ServiceStack.Text.Tests.JsonTests.Dog, ServiceStack.Text.Tests"",""Name"":""Dog""}}"));
		}

		public class PetDog
		{
			public IDog Dog { get; set; }
		}

		public class WeirdCat
		{
			public Cat Dog { get; set; }
		}

		[Test]
		public void Can_read_as_Cat_from_Dog_with_typeinfo()
		{
			var petDog = new PetDog { Dog = new Dog { Name = "Woof!" } };
			var json = petDog.ToJson();

			Console.WriteLine(json);

			var weirdCat = json.FromJson<WeirdCat>();

			Assert.That(weirdCat.Dog, Is.Not.Null);
			Assert.That(weirdCat.Dog.Name, Is.EqualTo(petDog.Dog.Name));
		}

	}
}