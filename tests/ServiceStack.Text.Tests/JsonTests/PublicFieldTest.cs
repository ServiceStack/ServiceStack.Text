using System;
using NUnit.Framework;

namespace ServiceStack.Text.Tests.JsonTests
{
#if !NETCORE
    [Serializable]
#endif
    public class TypeWithPublicFields
    {
        public readonly string Text;

        public TypeWithPublicFields(string text)
        {
            Text = text;
        }
    }


    [TestFixture]
    public class PublicFieldTest : TestBase
    {
        [Test]
        public void Public_readonly_fields_can_be_deserialized()
        {
            using (JsConfig.BeginScope())
            {
                JsConfig.IncludePublicFields = true;
                var instance = new TypeWithPublicFields("Hello");
                var deserilized = instance.ToJson();

                var copy = deserilized.FromJson<TypeWithPublicFields>();

                Assert.That(copy.Text, Is.EqualTo(instance.Text));
            }
        }
    }
}