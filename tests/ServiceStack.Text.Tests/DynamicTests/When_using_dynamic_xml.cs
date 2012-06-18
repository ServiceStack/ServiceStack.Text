using System;
using NUnit.Framework;
using ServiceStack.Text.Dynamic;

namespace ServiceStack.Text.Tests.DynamicTests
{
    [TestFixture]
    class When_using_dynamic_xml
    {
        dynamic dynamic;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            var xml = @"<animals>
 <badger>
  <name>Steve</name>
  <age>3</age>
 </badger>
     <dog>
          <name>Rufus</name>
          <breed>labrador</breed>
     </dog>
     <dog>
          <name>Marty</name>
          <breed>whippet</breed>
     </dog>
     <cat name=""Matilda""/>
</animals>
";
            dynamic = new DynamicXml(xml);
        }

        [Test]
        public void Then_badger_name_should_be_Steve()
        {
            string name = dynamic.animals.badger.name;
            Assert.AreEqual("Steve", name);
        }

        [Test]
        public void Then_badger_name_ToString_should_be_Steve()
        {
            Assert.AreEqual("Steve", dynamic.animals.badger.name.ToString());
        }

        [Test]
        public void Then_badger_age_should_be_3()
        {
            int age = dynamic.animals.badger.age;
            Assert.AreEqual(3, age);
        }

        [Test]
        public void Then_there_should_be_2_dogs()
        {
            dynamic[] dogs = dynamic.animals.dog;
            Assert.AreEqual(2, dogs.Length);
        }

        [Test]
        public void Then_the_first_dogs_name_should_be_Rufus()
        {
            string name = dynamic.animals.dog[0].name;
            Assert.AreEqual("Rufus", name);
        }

        [Test]
        public void Then_the_second_dog_should_be_a_whippet()
        {
            string breed = dynamic.animals.dog[1].breed;
            Assert.AreEqual("whippet", breed);
        }

        [Test]
        public void Then_the_cats_name_should_be_Matilda()
        {
            string name = dynamic.animals.cat.name;
            Assert.AreEqual("Matilda", name);
        }
    }
}
