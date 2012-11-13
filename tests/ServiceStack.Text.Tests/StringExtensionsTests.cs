﻿using System;
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

        [Test]
        public void Can_Url_Encode_String()
        {
            var text = "This string & has % unsafe ? characters for )_(*&^%$$^$@# a query string";

            var encoded = text.UrlEncode();

            Assert.That(encoded, 
                Is.EqualTo("This%20string%20%26%20has%20%25%20unsafe%20%3f%20characters%20for%20%29%5f%28%2a%26%5e%25%24%24%5e%24%40%23%20a%20query%20string"));

            var decoded = encoded.UrlDecode();

            Assert.That(decoded, Is.EqualTo(text));
        }

        [Test]
        public void Can_ToCamelCase_String()
        {
            Assert.That("U".ToCamelCase(), Is.EqualTo("u"));
            Assert.That("UU".ToCamelCase(), Is.EqualTo("uu"));
            Assert.That("UUU".ToCamelCase(), Is.EqualTo("uuu"));
            Assert.That("UUUU".ToCamelCase(), Is.EqualTo("uuuu"));
            Assert.That("l".ToCamelCase(), Is.EqualTo("l"));
            Assert.That("ll".ToCamelCase(), Is.EqualTo("ll"));
            Assert.That("lll".ToCamelCase(), Is.EqualTo("lll"));
            Assert.That("llll".ToCamelCase(), Is.EqualTo("llll"));
            Assert.That("Ul".ToCamelCase(), Is.EqualTo("ul"));
            Assert.That("Ull".ToCamelCase(), Is.EqualTo("ull"));
            Assert.That("Ulll".ToCamelCase(), Is.EqualTo("ulll"));
            Assert.That("UUl".ToCamelCase(), Is.EqualTo("uUl"));
            Assert.That("UUll".ToCamelCase(), Is.EqualTo("uUll"));
            Assert.That("UUUl".ToCamelCase(), Is.EqualTo("uuUl"));
            Assert.That("lU".ToCamelCase(), Is.EqualTo("lU"));
            Assert.That("lUl".ToCamelCase(), Is.EqualTo("lUl"));
            Assert.That("lUll".ToCamelCase(), Is.EqualTo("lUll"));
            Assert.That("llU".ToCamelCase(), Is.EqualTo("llU"));
            Assert.That("llUl".ToCamelCase(), Is.EqualTo("llUl"));
            Assert.That("lllU".ToCamelCase(), Is.EqualTo("lllU"));
            Assert.That("llUlll".ToCamelCase(), Is.EqualTo("llUlll"));
            Assert.That("lllUlll".ToCamelCase(), Is.EqualTo("lllUlll"));
            Assert.That("lllUUUlll".ToCamelCase(), Is.EqualTo("lllUUUlll"));
            Assert.That("lllUlllUlll".ToCamelCase(), Is.EqualTo("lllUlllUlll"));
            Assert.That("".ToCamelCase(), Is.EqualTo(""));
            Assert.That(((string)null).ToCamelCase(), Is.EqualTo((string)null));
        }

        [Test]
        public void Can_ToTitleCase_String()
        {
            var text = "Abc_def";

            var ttc = text.ToTitleCase();
            Assert.That(ttc, Is.EqualTo("AbcDef"));
        }

        [Test]
        public void Can_ToTitleCase_Empty_String()
        {
            var text = "";

            var ttc = text.ToTitleCase();
            Assert.That(ttc, Is.EqualTo(""));
        }

	    [Test]
        public void Can_Url_Encode_Unicode_String()
        {
            var text = "This string & has % 权뜑簒㮐ᾟ䗚璥趮⚦䭌䳅浝䕌ਥ⤧笫 characters";

            var encoded = text.UrlEncode();

            Assert.That(encoded, Is.EqualTo("This%20string%20%26%20has%20%25%20%e6%9d%83%eb%9c%91%e7%b0%92%e3%ae%90%e1%be%9f" +
                "%e4%97%9a%e7%92%a5%e8%b6%ae%e2%9a%a6%e4%ad%8c%e4%b3%85%e6%b5%9d%e4%95%8c%e0%a8%a5%e2%a4%a7%e7%ac%ab%20characters"));

            var decoded = encoded.UrlDecode();

            Assert.That(decoded, Is.EqualTo(text));
        }
	}
}