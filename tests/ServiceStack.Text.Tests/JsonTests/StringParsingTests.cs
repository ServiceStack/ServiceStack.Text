using NUnit.Framework;

namespace ServiceStack.Text.Tests.JsonTests
{
    [TestFixture]
    public class StringParsingTests
    {
        [TestCase("test", "test")]
        [TestCase("", "\"\"")]
        [TestCase("asdf asdf asdf ", "asdf asdf asdf ")]
        [TestCase("test\t\ttest", "test\\t\\ttest")]
        [TestCase("\t\ttesttest", "\\t\\ttesttest")]
        [TestCase("testtest\t\t", "testtest\\t\\t")]
        [TestCase("test\tt\test", "test\\tt\\test")]
        [TestCase("\ttest\ttest", "\\ttest\\ttest")]
        [TestCase("test\ttest\t", "test\\ttest\\t")]
        [TestCase("\\", "\\")]
        [TestCase("test\t", "test\t")]
        [TestCase("test\ttest\\", "test\\ttest\\")]
        [TestCase("test\ttest", "test\\ttest")]
        [TestCase("\ttesttest", "\\ttesttest")]
        [TestCase("testtest\t", "testtest\\t")]
        [TestCase("the word is \ab","the word is \\ab")]
        [TestCase("the word is \u1ab9","the word is \\u1ab9")]
        [TestCase("the word is \x00ff","the word is \\x00ff")]
        [TestCase("the word is \x00","the word is \\x00")]
        [TestCase("the word is \\x0","the word is \\x0")]
        [TestCase("test tab \t", "test tab \\t")]
        [TestCase("test return \r", "test return \\r")]
        [TestCase("test bell \b", "test bell \\b")]
        [TestCase("test quote \"", "test quote \\\"")]
        [TestCase("\"", "\\\"")]
        [TestCase("\"double quote\"", "\\\"double quote\\\"")]
        [TestCase("\"triple quote\"", "\"\\\"triple quote\\\"\"")]
        [TestCase("\"double triple quote\" and \"double triple quote\"",
                  "\"\\\"double triple quote\\\" and \\\"double triple quote\\\"\"")]
        public void AssertUnescapes(string expected, string given)
        {
            Assert.AreEqual(expected, JsonSerializer.DeserializeFromString<string>(given));
        }
    }
}