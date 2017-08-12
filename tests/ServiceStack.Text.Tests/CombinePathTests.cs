using NUnit.Framework;

namespace ServiceStack.Text.Tests
{
    public class CombinePathTests
    {
        [Test]
        public void Does_combine_paths()
        {
            Assert.That("/a".CombineWith("b"), Is.EqualTo("/a/b"));
            Assert.That("a".CombineWith("b"), Is.EqualTo("a/b"));
            Assert.That("/a/b".CombineWith("c"), Is.EqualTo("/a/b/c"));
            Assert.That("a/b".CombineWith("c"), Is.EqualTo("a/b/c"));
            Assert.That("/a/b".CombineWith("c/d"), Is.EqualTo("/a/b/c/d"));
            Assert.That("/a/b".CombineWith("c", "d"), Is.EqualTo("/a/b/c/d"));

            Assert.That("http://example.org/a/b".CombineWith("c", "d"), Is.EqualTo("http://example.org/a/b/c/d"));
        }

        [Test]
        public void Does_combine_paths_with_trailing_slashes()
        {
            Assert.That("/a/".CombineWith("b"), Is.EqualTo("/a/b"));
            Assert.That("/a/".CombineWith("b/"), Is.EqualTo("/a/b/"));
            Assert.That("a/".CombineWith("/b"), Is.EqualTo("a/b"));
            Assert.That("/a/b/".CombineWith("/c/"), Is.EqualTo("/a/b/c/"));
            Assert.That("a/b/".CombineWith("c"), Is.EqualTo("a/b/c"));
            Assert.That("/a/b/".CombineWith("/c/d"), Is.EqualTo("/a/b/c/d"));
            Assert.That("/a/b/".CombineWith("/c", "/d"), Is.EqualTo("/a/b/c/d"));

            Assert.That("http://example.org/a/b/".CombineWith("/c/", "/d"), Is.EqualTo("http://example.org/a/b/c/d"));
        }

        [Test]
        public void Can_resolve_paths()
        {
            Assert.That("/a/b/../".ResolvePaths(), Is.EqualTo("/a/"));
            Assert.That("/a/b/..".ResolvePaths(), Is.EqualTo("/a"));
            Assert.That("a/b/..".ResolvePaths(), Is.EqualTo("a"));

            Assert.That("a/../b".ResolvePaths(), Is.EqualTo("b"));
            Assert.That("a/../b/./c".ResolvePaths(), Is.EqualTo("b/c"));
            Assert.That("a/b/c/d/../..".ResolvePaths(), Is.EqualTo("a/b"));
            Assert.That("a/b/../../c/d".ResolvePaths(), Is.EqualTo("c/d"));

            Assert.That("a/..".ResolvePaths(), Is.EqualTo(""));
            Assert.That("a/../..".ResolvePaths(), Is.EqualTo(".."));
            Assert.That("a/../../".ResolvePaths(), Is.EqualTo("../"));
            Assert.That("a/../../b".ResolvePaths(), Is.EqualTo("../b"));
        }

    }
}