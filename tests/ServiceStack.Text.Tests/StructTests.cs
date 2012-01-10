using System;
using NUnit.Framework;

namespace ServiceStack.Text.Tests
{
    [TestFixture]
    public class StructTests
    {
        [Serializable]
        public class Foo
        {
            public string Name { get; set; }

            public Text Content1 { get; set; }

            public Text Content2 { get; set; }
        }

        public interface IText { }

        [Serializable]
        public struct Text
        {
            private readonly string _value;

            public Text(string value)
            {
                _value = value;
            }

            public static Text Parse(string value)
            {
                return value == null ? null : new Text(value);
            }

            public static implicit operator Text(string value)
            {
                return new Text(value);
            }

            public static implicit operator string(Text item)
            {
                return item._value;
            }

            public override string ToString()
            {
                return _value;
            }
        }

        [Test]
        public void Test_structs()
        {
            JsConfig<Text>.SerializeFn = text => text.ToString();

            var dto = new Foo { Content1 = "My content", Name = "My name" };

            var json = JsonSerializer.SerializeToString(dto, dto.GetType());

            Assert.That(json, Is.EqualTo("{\"Name\":\"My name\",\"Content1\":\"My content\"}"));

            //var c = JsonSerializer.DeserializeFromString<Foo>(json);
            //Console.WriteLine(c.Name);
            //Console.WriteLine(c.Content1);
            //Console.WriteLine(c.Content2);
        }

    }
}