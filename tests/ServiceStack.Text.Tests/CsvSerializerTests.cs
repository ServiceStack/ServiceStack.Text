using System;
using System.IO;
using System.Linq;
using Northwind.Common.DataModel;
using NUnit.Framework;
using ServiceStack.Text.Csv;
using ServiceStack.Text.Tests.CsvTests;
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

		public void Serialize<T>(T data)
		{
			//TODO: implement serializer and test properly
			var csv = CsvSerializer.SerializeToString(data);
			csv.Print();
		}

		[Test]
		public void Can_Serialize_Movie()
		{
			Serialize(MoviesData.Movies[0]);
		}

		[Test]
		public void Can_Serialize_Movies()
		{
			Serialize(MoviesData.Movies);
		}

        [Test]
        public void Can_Serialize_MovieResponse_Dto()
        {
            Serialize(new MovieResponse { Movie = MoviesData.Movies[0] });
        }

        [Test]
        public void Can_Serialize_MoviesResponse_Dto()
        {
            Serialize(new MoviesResponse { Movies = MoviesData.Movies });
        }

        [Test]
        public void Can_Serialize_MoviesResponse2_Dto()
        {
            Serialize(new MoviesResponse2 { Movies = MoviesData.Movies });
        }

		[Test]
		public void serialize_Category()
		{
			Serialize(NorthwindData.Categories[0]);
		}

		[Test]
		public void serialize_Categories()
		{
			Serialize(NorthwindData.Categories);
		}

		[Test]
		public void serialize_Customer()
		{
			Serialize(NorthwindData.Customers[0]);
		}

		[Test]
		public void serialize_Customers()
		{
			Serialize(NorthwindData.Customers);
		}

		[Test]
		public void serialize_Employee()
		{
			Serialize(NorthwindData.Employees[0]);
		}

		[Test]
		public void serialize_Employees()
		{
			Serialize(NorthwindData.Employees);
		}

		[Test]
		public void serialize_EmployeeTerritory()
		{
			Serialize(NorthwindData.EmployeeTerritories[0]);
		}

		[Test]
		public void serialize_EmployeeTerritories()
		{
			Serialize(NorthwindData.EmployeeTerritories);
		}

		[Test]
		public void serialize_OrderDetail()
		{
			Serialize(NorthwindData.OrderDetails[0]);
		}

		[Test]
		public void serialize_OrderDetails()
		{
			Serialize(NorthwindData.OrderDetails);
		}

		[Test]
		public void serialize_Order()
		{
			Serialize(NorthwindData.Orders[0]);
		}

		[Test]
		public void serialize_Orders()
		{
			Serialize(NorthwindData.Orders);
		}

		[Test]
		public void serialize_Product()
		{
			Serialize(NorthwindData.Products[0]);
		}

		[Test]
		public void serialize_Products()
		{
			Serialize(NorthwindData.Products);
		}

		[Test]
		public void serialize_Region()
		{
			Serialize(NorthwindData.Regions[0]);
		}

		[Test]
		public void serialize_Regions()
		{
			Serialize(NorthwindData.Regions);
		}

		[Test]
		public void serialize_Shipper()
		{
			Serialize(NorthwindData.Shippers[0]);
		}

		[Test]
		public void serialize_Shippers()
		{
			Serialize(NorthwindData.Shippers);
		}

		[Test]
		public void serialize_Supplier()
		{
			Serialize(NorthwindData.Suppliers[0]);
		}

		[Test]
		public void serialize_Suppliers()
		{
			Serialize(NorthwindData.Suppliers);
		}

		[Test]
		public void serialize_Territory()
		{
			Serialize(NorthwindData.Territories[0]);
		}

		[Test]
		public void serialize_Territories()
		{
			Serialize(NorthwindData.Territories);
		}

		[Test]
		public void Can_deserialize_from_stream()
		{
			var testCsv = TestData.TestCsv.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToArray();
			var tempFileName = Path.GetTempFileName();
			File.WriteAllLines(tempFileName, testCsv);

			using (var fs = new FileStream(tempFileName, FileMode.Open, FileAccess.Read))
			{
				var queryRows = CsvSerializer.DeserializeEnumerableFromStream<QueryRow>(fs).ToList();
				Assert.That(queryRows.Count, Is.EqualTo(testCsv.Length - 1));
				Assert.That(queryRows[0].Artist, Is.EqualTo("Elton John"));
				Assert.That(queryRows[0].Country, Is.EqualTo("US"));
				Assert.That(queryRows[0].Query, Is.EqualTo("Your Song"));
				Assert.That(queryRows[0].Title, Is.EqualTo("Your Song"));
			}

			File.Delete(tempFileName);
		}
	}
}