using System.Collections.Specialized;
using NUnit.Framework;
using ServiceStack.Text.Dynamic;

namespace ServiceStack.Text.Tests.DynamicTests
{
    [TestFixture]
    class When_using_dynamic_namevaluecollection
    {
        dynamic dynamic;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            var nameValueCollection = new NameValueCollection
                                          {
                                              {"name", "Steve"},
                                              {"age", "3"},
                                              {"foods", "worms,beetles"}
                                          };
            dynamic = new DynamicNameValueCollection(nameValueCollection);
        }

        [Test]
        public void Then_badger_name_should_be_Steve()
        {
            string name = dynamic.name;
            Assert.AreEqual("Steve", name);
        }

        [Test]
        public void Then_badger_name_ToString_should_be_Steve()
        {
            Assert.AreEqual("Steve", dynamic.name.ToString());
        }

        [Test]
        public void Then_badger_age_should_be_3()
        {
            int age = dynamic.age;
            Assert.AreEqual(3, age);
        }

        [Test]
        public void Then_badger_foods_should_be_worms_and_beetles()
        {
            string[] foods = dynamic.foods;
            Assert.AreEqual("worms", foods[0]);
            Assert.AreEqual("beetles", foods[1]);
        }
    }
}