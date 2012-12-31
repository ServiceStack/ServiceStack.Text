using System.Collections;
using NUnit.Framework;

namespace ServiceStack.Text.Tests.JsonTests
{
    [TestFixture]
    public class ArrayListTests : TestBase
    {
        [Test]
        public void CanSerializeArrayListOfMixedTypes()
        {
            var arr = new ArrayList();

            arr.Add(1.0);
            arr.Add(1.1);
            arr.Add(1);
            var serialized = JsonSerializer.SerializeToString(arr);

            Assert.IsNotNull(serialized);
        }
        
    }
}