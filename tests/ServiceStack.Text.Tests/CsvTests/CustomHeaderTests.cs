using System;
using System.Collections.Generic;
using NUnit.Framework;
using ServiceStack.Text.Tests.Support;

namespace ServiceStack.Text.Tests.CsvTests
{
    [TestFixture]
    public class CustomHeaderTests
    {
        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            CsvConfig<TableItem>.Reset();
        }

        [Test]
        public void Can_serialize_custom_headers_map()
        {
            CsvConfig<TableItem>.CustomHeadersMap = new Dictionary<string, string> {
                {"Column1Data", "Column 1"},
                {"Column2Data", "Column 2"},
                {"Column3Data", "Column,3"},
                {"Column4Data", "Column\n4"},
                {"Column5Data", "Column 5"},
            };
            var data = new List<TableItem> {
                new TableItem { Column1Data = "I", Column2Data = "Like", Column3Data = "To", Column4Data = "Read", Column5Data = "Novels" },
                new TableItem { Column1Data = "I am", Column2Data = "Very", Column3Data = "Cool", Column4Data = "And", Column5Data = "Awesome" },
            };

            var csv = CsvSerializer.SerializeToCsv(data);

            Console.WriteLine(csv);

            Assert.That(csv, Is.EqualTo(
                "Column 1,Column 2,\"Column,3\",\"Column\n4\",Column 5\r\n"
                + "I,Like,To,Read,Novels\r\n"
                + "I am,Very,Cool,And,Awesome\r\n"
            ));
        }

        [Test]
        public void Can_serialize_custom_anonymous_type_headers()
        {
            CsvConfig<TableItem>.CustomHeaders = new
            {
                Column1Data = "Column 1",
                Column2Data = "Column 2",
                Column3Data = "Column,3",
                Column4Data = "Column\n4",
                Column5Data = "Column 5",
            };
            var data = new List<TableItem> {
                new TableItem { Column1Data = "I", Column2Data = "Like", Column3Data = "To", Column4Data = "Read", Column5Data = "Novels" },
                new TableItem { Column1Data = "I am", Column2Data = "Very", Column3Data = "Cool", Column4Data = "And", Column5Data = "Awesome" },
            };

            var csv = CsvSerializer.SerializeToCsv(data);

            Console.WriteLine(csv);

            Assert.That(csv, Is.EqualTo(
                "Column 1,Column 2,\"Column,3\",\"Column\n4\",Column 5\r\n"
                + "I,Like,To,Read,Novels\r\n"
                + "I am,Very,Cool,And,Awesome\r\n"
            ));
        }

        [Test]
        public void Can_serialize_partial_custom_headers_map()
        {
            CsvConfig<TableItem>.CustomHeadersMap = new Dictionary<string, string> {
                {"Column1Data", "Column 1"},
                {"Column3Data", "Column,3"},
                {"Column5Data", "Column 5"},
            };
            var data = new List<TableItem> {
                new TableItem { Column1Data = "I", Column2Data = "Like", Column3Data = "To", Column4Data = "Read", Column5Data = "Novels" },
                new TableItem { Column1Data = "I am", Column2Data = "Very", Column3Data = "Cool", Column4Data = "And", Column5Data = "Awesome" },
            };

            var csv = CsvSerializer.SerializeToCsv(data);

            Console.WriteLine(csv);

            Assert.That(csv, Is.EqualTo(
                "Column 1,\"Column,3\",Column 5\r\n"
                + "I,To,Novels\r\n"
                + "I am,Cool,Awesome\r\n"
            ));
        }

        [Test]
        public void Can_serialize_without_headers()
        {
            CsvConfig<TableItem>.OmitHeaders = true;

            CsvConfig<TableItem>.CustomHeadersMap = new Dictionary<string, string> {
                {"Column1Data", "Column 1"},
                {"Column2Data", "Column 2"},
                {"Column3Data", "Column,3"},
                {"Column4Data", "Column\n4"},
                {"Column5Data", "Column 5"},
            };
            var data = new List<TableItem> {
                new TableItem { Column1Data = "I", Column2Data = "Like", Column3Data = "To", Column4Data = "Read", Column5Data = "Novels" },
                new TableItem { Column1Data = "I am", Column2Data = "Very", Column3Data = "Cool", Column4Data = "And", Column5Data = "Awesome" },
            };

            var csv = CsvSerializer.SerializeToCsv(data);

            Console.WriteLine(csv);

            Assert.That(csv, Is.EqualTo(
                "I,Like,To,Read,Novels\r\n"
                + "I am,Very,Cool,And,Awesome\r\n"
            ));
        }

    }
}