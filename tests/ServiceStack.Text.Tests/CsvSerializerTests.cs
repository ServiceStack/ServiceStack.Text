using System.Collections;
using Northwind.Common.DataModel;
using NUnit.Framework;
using ServiceStack.Text.Tests.Support;

namespace ServiceStack.Text.Tests
{
    [TestFixture]
    public class CsvSerializerTests
    {
        static CsvSerializerTests()
        {
            NorthwindData.LoadData(false);
        }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            JsConfig.SkipDateTimeConversion = true;
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            JsConfig.Reset();
        }

        public void Serialize<T>(T data)
        {
            //TODO: implement serializer and test properly
            var csv = CsvSerializer.SerializeToString(data);
            csv.Print();
        }

        public void SerializeAndDeserialize<T>(T data)
        {
            var csv = CsvSerializer.SerializeToString(data);
            csv.Print();

            var dto = CsvSerializer.DeserializeFromString<T>(csv);

            var dataArray = data is IEnumerable ? (data as IEnumerable).Map(x => x).ToArray() : null;
            var dtoArray = dto is IEnumerable ? (dto as IEnumerable).Map(x => x).ToArray() : null;

            if (dataArray != null && dtoArray != null)
                Assert.That(dtoArray, Is.EquivalentTo(dataArray));
            else
                Assert.That(dto, Is.EqualTo(data));
        }

        [Test]
        public void Does_parse_new_lines()
        {
            Assert.That(CsvReader.ParseLines("A,B\nC,D"), Is.EquivalentTo(new[] { "A,B", "C,D" }));
            Assert.That(CsvReader.ParseLines("A,B\nC,D\n"), Is.EquivalentTo(new[] { "A,B", "C,D" }));
            Assert.That(CsvReader.ParseLines("A,B\nC,D\n\n"), Is.EquivalentTo(new[] { "A,B", "C,D" }));

            Assert.That(CsvReader.ParseLines("A,B\r\nC,D"), Is.EquivalentTo(new[] { "A,B", "C,D" }));
            Assert.That(CsvReader.ParseLines("A,B\r\nC,D\r\n"), Is.EquivalentTo(new[] { "A,B", "C,D" }));
            Assert.That(CsvReader.ParseLines("A,B\r\nC,D\r\n\r\n"), Is.EquivalentTo(new[] { "A,B", "C,D" }));

            Assert.That(CsvReader.ParseLines("\"A,B\"\n\"C,D\""), Is.EquivalentTo(new[] { "\"A,B\"", "\"C,D\"" }));
            Assert.That(CsvReader.ParseLines("\"A,B\",B\nC,\"C,D\""), Is.EquivalentTo(new[] { "\"A,B\",B", "C,\"C,D\"" }));

            Assert.That(CsvReader.ParseLines("\"A\nB\",B\nC,\"C\r\nD\""), Is.EquivalentTo(new[] { "\"A\nB\",B", "C,\"C\r\nD\"" }));
        }

        [Test]
        public void Does_parse_fields()
        {
            Assert.That(CsvReader.ParseFields("A,B"), Is.EquivalentTo(new[] { "A", "B" }));
            Assert.That(CsvReader.ParseFields("\"A\",B"), Is.EquivalentTo(new[] { "A", "B" }));
            Assert.That(CsvReader.ParseFields("\"A\",\"B,C\""), Is.EquivalentTo(new[] { "A", "B,C" }));
            Assert.That(CsvReader.ParseFields("\"A\nB\",\"B,\r\nC\""), Is.EquivalentTo(new[] { "A\nB", "B,\r\nC" }));
            Assert.That(CsvReader.ParseFields("\"A\"\",B\""), Is.EquivalentTo(new[] { "A\",B" }));

            Assert.That(CsvReader.ParseFields(",A,B"), Is.EquivalentTo(new[] { null, "A", "B" }));
            Assert.That(CsvReader.ParseFields("A,,B"), Is.EquivalentTo(new[] { "A", null, "B" }));
            Assert.That(CsvReader.ParseFields("A,B,"), Is.EquivalentTo(new[] { "A", "B", null }));

            Assert.That(CsvReader.ParseFields("\"\",A,B"), Is.EquivalentTo(new[] { "", "A", "B" }));
            Assert.That(CsvReader.ParseFields("A,\"\",B"), Is.EquivalentTo(new[] { "A", "", "B" }));
            Assert.That(CsvReader.ParseFields("A,B,\"\""), Is.EquivalentTo(new[] { "A", "B", "" }));
        }

        [Test]
        public void Can_Serialize_Movie()
        {
            Serialize(MoviesData.Movies[0]);
        }

        [Test]
        public void Can_Serialize_Movies()
        {
            SerializeAndDeserialize(MoviesData.Movies);
        }

        [Test]
        public void Can_Serialize_MovieResponse_Dto()
        {
            SerializeAndDeserialize(new MovieResponse { Movie = MoviesData.Movies[0] });
        }

        [Test]
        public void Can_Serialize_MoviesResponse_Dto()
        {
            SerializeAndDeserialize(new MoviesResponse { Movies = MoviesData.Movies });
        }

        [Test]
        public void Can_Serialize_MoviesResponse2_Dto()
        {
            SerializeAndDeserialize(new MoviesResponse2 { Movies = MoviesData.Movies });
        }

        [Test]
        public void serialize_Category()
        {
            SerializeAndDeserialize(NorthwindData.Categories[0]);
        }

        [Test]
        public void serialize_Categories()
        {
            SerializeAndDeserialize(NorthwindData.Categories);
        }

        [Test]
        public void serialize_Customer()
        {
            SerializeAndDeserialize(NorthwindData.Customers[0]);
        }

        [Test]
        public void serialize_Customers()
        {
            SerializeAndDeserialize(NorthwindData.Customers);
        }

        [Test]
        public void serialize_Employee()
        {
            SerializeAndDeserialize(NorthwindData.Employees[0]);
        }

        [Test]
        public void serialize_Employees()
        {
            SerializeAndDeserialize(NorthwindData.Employees);
        }

        [Test]
        public void serialize_EmployeeTerritory()
        {
            SerializeAndDeserialize(NorthwindData.EmployeeTerritories[0]);
        }

        [Test]
        public void serialize_EmployeeTerritories()
        {
            SerializeAndDeserialize(NorthwindData.EmployeeTerritories);
        }

        [Test]
        public void serialize_OrderDetail()
        {
            SerializeAndDeserialize(NorthwindData.OrderDetails[0]);
        }

        [Test]
        public void serialize_OrderDetails()
        {
            SerializeAndDeserialize(NorthwindData.OrderDetails);
        }

        [Test]
        public void serialize_Order()
        {
            SerializeAndDeserialize(NorthwindData.Orders[0]);
        }

        [Test]
        public void serialize_Orders()
        {
            Serialize(NorthwindData.Orders);
        }

        [Test]
        public void serialize_Product()
        {
            SerializeAndDeserialize(NorthwindData.Products[0]);
        }

        [Test]
        public void serialize_Products()
        {
            SerializeAndDeserialize(NorthwindData.Products);
        }

        [Test]
        public void serialize_Region()
        {
            SerializeAndDeserialize(NorthwindData.Regions[0]);
        }

        [Test]
        public void serialize_Regions()
        {
            SerializeAndDeserialize(NorthwindData.Regions);
        }

        [Test]
        public void serialize_Shipper()
        {
            SerializeAndDeserialize(NorthwindData.Shippers[0]);
        }

        [Test]
        public void serialize_Shippers()
        {
            SerializeAndDeserialize(NorthwindData.Shippers);
        }

        [Test]
        public void serialize_Supplier()
        {
            SerializeAndDeserialize(NorthwindData.Suppliers[0]);
        }

        [Test]
        public void serialize_Suppliers()
        {
            SerializeAndDeserialize(NorthwindData.Suppliers);
        }

        [Test]
        public void serialize_Territory()
        {
            SerializeAndDeserialize(NorthwindData.Territories[0]);
        }

        [Test]
        public void serialize_Territories()
        {
            SerializeAndDeserialize(NorthwindData.Territories);
        }
    }
}