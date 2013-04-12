using System;

namespace ServiceStack.Text.Tests.CsvTests
{
	public static class TestData
	{
		public static string TestCsv = "country,query,artist,title" + Environment.NewLine +
		                               "US,Your Song,Elton John,Your Song" + Environment.NewLine +
		                               "US,Patience guns n roses,Guns 'n Roses,Patience";

		public static string TestCsvBlankColumn = "country,query,artist,title" + Environment.NewLine +
		                                          "US,,Guns 'n Roses,Patience";

		public static string TestCsvCommaColumn = "country,query,artist,title" + Environment.NewLine +
		                                          "UK,\"Definately, Maybe\",Oasis,\"Definately, Maybe\"";

		public static string TestCsvNotMatching = "country,query,artist,title" + Environment.NewLine +
		                                          "UK,Definately, Maybe,Oasis,Definately, Maybe";
		
	}
}