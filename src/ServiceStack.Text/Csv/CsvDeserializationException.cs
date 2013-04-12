using System.Runtime.Serialization;

namespace ServiceStack.Text.Csv
{
	public class CsvDeserializationException : SerializationException
	{
		public CsvDeserializationException(string message)
			: base(message)
		{ }
	}
}