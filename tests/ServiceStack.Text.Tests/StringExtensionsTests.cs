using NUnit.Framework;

namespace ServiceStack.Text.Tests
{
	[TestFixture]
	public class StringExtensionsTests
	{
		[Test]
		public void Can_SplitOnFirst_char_needle()
		{
			var parts = "user:pass@w:rd".SplitOnFirst(':');
			Assert.That(parts[0], Is.EqualTo("user"));
			Assert.That(parts[1], Is.EqualTo("pass@w:rd"));
		}

		[Test]
		public void Can_SplitOnFirst_string_needle()
		{
			var parts = "user:pass@w:rd".SplitOnFirst(":");
			Assert.That(parts[0], Is.EqualTo("user"));
			Assert.That(parts[1], Is.EqualTo("pass@w:rd"));
		}
	}
}