using System;
using System.IO;
using System.Text;
using BenchmarkDotNet.Attributes;
using ServiceStack.Text;
using ServiceStack.Text.Tests.DynamicModels;
using ServiceStack.Text.Json;

namespace ServiceStack.Text.Benchmarks
{
	public class ModelWithCommonTypes
	{
		public char CharValue { get; set; }

		public byte ByteValue { get; set; }

		public sbyte SByteValue { get; set; }

		public short ShortValue { get; set; }

		public ushort UShortValue { get; set; }

		public int IntValue { get; set; }

		public uint UIntValue { get; set; }

		public long LongValue { get; set; }

		public ulong ULongValue { get; set; }

		public float FloatValue { get; set; }

		public double DoubleValue { get; set; }

		public decimal DecimalValue { get; set; }

		public DateTime DateTimeValue { get; set; }

		public TimeSpan TimeSpanValue { get; set; }

		public Guid GuidValue { get; set; }

		public static ModelWithCommonTypes Create(byte i)
		{
			return new ModelWithCommonTypes
			{
				ByteValue = i,
				CharValue = (char)i,
				DateTimeValue = new DateTime(2000, 1, 1 + i),
				DecimalValue = i,
				DoubleValue = i,
				FloatValue = i,
				IntValue = i,
				LongValue = i,
				SByteValue = (sbyte)i,
				ShortValue = i,
				TimeSpanValue = new TimeSpan(i),
				UIntValue = i,
				ULongValue = i,
				UShortValue = i,
				GuidValue = Guid.NewGuid(),
			};
		}
	}

    public class JsonSerializationBenchmarks
    {
        static ModelWithAllTypes allTypesModel = ModelWithAllTypes.Create(3);
        static ModelWithCommonTypes commonTypesModel = ModelWithCommonTypes.Create(3);
        static MemoryStream stream = new MemoryStream(16384);
        const string serializedString = "this is the test string";

        [Benchmark]
        public void SerializeJsonAllTypes()
        {
            string result = JsonSerializer.SerializeToString<ModelWithAllTypes>(allTypesModel);
        }

        [Benchmark]
        public void SerializeJsonCommonTypes()
        {
            string result = JsonSerializer.SerializeToString<ModelWithCommonTypes>(commonTypesModel);
        }

        [Benchmark]
        public void SerializeJsonString()
        {
            string result = JsonSerializer.SerializeToString<string>(serializedString);
        }

        [Benchmark]
        public void SerializeJsonStringToStream()
        {
            stream.Position = 0;
            JsonSerializer.SerializeToStream<string>(serializedString, stream);
        }

        [Benchmark]
        public void SerializeJsonStringToStreamDirectly()
        {
            stream.Position = 0;
            string tmp = JsonSerializer.SerializeToString<string>(serializedString);
            byte[] arr = Encoding.UTF8.GetBytes(tmp);
            stream.Write(arr, 0, arr.Length);
        }


        [Benchmark]
        public void SerializeJsonAllTypesToStream()
        {
            stream.Position = 0;
            JsonSerializer.SerializeToStream<ModelWithAllTypes>(allTypesModel, stream);
        }
        
        [Benchmark]
        public void SerializeJsonCommonTypesToStream()
        {
            stream.Position = 0;
            JsonSerializer.SerializeToStream<ModelWithCommonTypes>(commonTypesModel, stream);
        }

/*        [Benchmark]
        public void SerializeJsonStringToStreamUsingDirectStreamWriter()
        {
            stream.Position = 0;
            var writer = new DirectStreamWriter(stream);
            JsonWriter<string>.WriteRootObject(writer, serializedString);
            writer.Flush();
        }
*/        
    }
}
