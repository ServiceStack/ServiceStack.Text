using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ServiceStack.Text.Tests
{
    public class TestModel
    {
        public TestModel()
        {
            var i = 0;
            this.PublicInt = i++;
            this.PublicGetInt = i++;
            this.PublicSetInt = i++;
            this.PublicIntField = i++;
            this.PrivateInt = i++;
            this.ProtectedInt = i++;
        }

        public int PublicInt { get; set; }

        public int PublicGetInt { get; private set; }

        public int PublicSetInt { private get; set; }

        public int PublicIntField;

        private int PrivateInt { get; set; }

        protected int ProtectedInt { get; set; }

        public int IntMethod()
        {
            return this.PublicInt;
        }
    }

	[TestFixture]
	public class ReflectionExtensionTests
		: TestBase
	{

		[Test]
		public void Only_serializes_public_readable_properties()
		{
			var model = new TestModel();
			var modelStr = TypeSerializer.SerializeToString(model);

			Assert.That(modelStr, Is.EqualTo("{PublicInt:0,PublicGetInt:1}"));

			Serialize(model);
		}

        [Test]
        public void Can_create_instances_of_common_collections()
        {
            Assert.That(typeof(IEnumerable<TestModel>).CreateInstance() as IEnumerable<TestModel>, Is.Not.Null);
            Assert.That(typeof(ICollection<TestModel>).CreateInstance() as ICollection<TestModel>, Is.Not.Null);
            Assert.That(typeof(IList<TestModel>).CreateInstance() as IList<TestModel>, Is.Not.Null);
            Assert.That(typeof(IDictionary<string, TestModel>).CreateInstance() as IDictionary<string, TestModel>, Is.Not.Null);
            Assert.That(typeof(IDictionary<int, TestModel>).CreateInstance() as IDictionary<int, TestModel>, Is.Not.Null);
            Assert.That(typeof(TestModel[]).CreateInstance() as TestModel[], Is.Not.Null);
        }
    }
}
