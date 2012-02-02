using System;
using System.Collections.Generic;
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
		}

        [Test]
        public void Test_structs_with_double_quotes()
        {
            var dto = new Foo { Content1 = "My \"quoted\" content", Name = "My \"quoted\" name" };
            
            JsConfig<Text>.SerializeFn = text => text.ToString();
            JsConfig<Text>.DeSerializeFn = v => new Text(v);

            var json = JsonSerializer.SerializeToString(dto, dto.GetType());
            Assert.That(json, Is.EqualTo("{\"Name\":\"My \\\"quoted\\\" name\",\"Content1\":\"My \\\"quoted\\\" content\"}"));
            
            var foo = JsonSerializer.DeserializeFromString<Foo>(json);
            Assert.That(foo.Content1, Is.EqualTo(dto.Content1));
        }
	}
}