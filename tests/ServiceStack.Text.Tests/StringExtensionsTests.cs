using System;
using System.IO;
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

		[Test]
		public void Can_SplitOnLast_char_needle()
		{
			var parts = "user:name:pass@word".SplitOnLast(':');
			Assert.That(parts[0], Is.EqualTo("user:name"));
			Assert.That(parts[1], Is.EqualTo("pass@word"));
		}

		[Test]
		public void Can_SplitOnLast_string_needle()
		{
			var parts = "user:name:pass@word".SplitOnLast(":");
			Assert.That(parts[0], Is.EqualTo("user:name"));
			Assert.That(parts[1], Is.EqualTo("pass@word"));
		}

		private static readonly char DirSep = Path.DirectorySeparatorChar;
		private static readonly char AltDirSep = Path.DirectorySeparatorChar == '/' ? '\\' : '/';

		[Test]
		public void Does_get_ParentDirectory()
		{
			var dirSep = DirSep;
			var filePath = "path{0}to{0}file".FormatWith(dirSep);
			Assert.That(filePath.ParentDirectory(), Is.EqualTo("path{0}to".FormatWith(dirSep)));
			Assert.That(filePath.ParentDirectory().ParentDirectory(), Is.EqualTo("path".FormatWith(dirSep)));
			Assert.That(filePath.ParentDirectory().ParentDirectory().ParentDirectory(), Is.Null);

			var filePathWithExt = "path{0}to{0}file/".FormatWith(dirSep);
			Assert.That(filePathWithExt.ParentDirectory(), Is.EqualTo("path{0}to".FormatWith(dirSep)));
			Assert.That(filePathWithExt.ParentDirectory().ParentDirectory(), Is.EqualTo("path".FormatWith(dirSep)));
			Assert.That(filePathWithExt.ParentDirectory().ParentDirectory().ParentDirectory(), Is.Null);
		}

		[Test]
		public void Does_get_ParentDirectory_of_AltDirectorySeperator()
		{
			var dirSep = AltDirSep;
			var filePath = "path{0}to{0}file".FormatWith(dirSep);
			Assert.That(filePath.ParentDirectory(), Is.EqualTo("path{0}to".FormatWith(dirSep)));
			Assert.That(filePath.ParentDirectory().ParentDirectory(), Is.EqualTo("path".FormatWith(dirSep)));
			Assert.That(filePath.ParentDirectory().ParentDirectory().ParentDirectory(), Is.Null);

			var filePathWithExt = "path{0}to{0}file{0}".FormatWith(dirSep);
			Assert.That(filePathWithExt.ParentDirectory(), Is.EqualTo("path{0}to".FormatWith(dirSep)));
			Assert.That(filePathWithExt.ParentDirectory().ParentDirectory(), Is.EqualTo("path".FormatWith(dirSep)));
			Assert.That(filePathWithExt.ParentDirectory().ParentDirectory().ParentDirectory(), Is.Null);
		}

		[Test]
		public void Does_not_alter_filepath_without_extension()
		{
			var path = "path/dir.with.dot/to/file";
			Assert.That(path.WithoutExtension(), Is.EqualTo(path));

			Assert.That("path/to/file.ext".WithoutExtension(), Is.EqualTo("path/to/file"));
		}

		[Test]
		public void Does_find_IndexOfAny_strings()
		{
			var text = "text with /* and <!--";
			var pos = text.IndexOfAny("<!--", "/*");
			//Console.WriteLine(text.Substring(pos));
			Assert.That(pos, Is.EqualTo("text with ".Length));
		}

		[Test]
		public void Does_ExtractContent_first_pattern_from_Document_without_marker()
		{
			var text = "text with random <!--comment--> and Contents: <!--Contents--> are here";
			var extract = text.ExtractContents("<!--", "-->");

			Assert.That(extract, Is.EqualTo("comment"));
		}

		[Test]
		public void Does_ExtractContents_from_Document()
		{
			var text = "text with random <!--comment--> and Contents: <!--Contents--> are here";
			var extract = text.ExtractContents("Contents:", "<!--", "-->");

			Assert.That(extract, Is.EqualTo("Contents"));
		}

	}
}