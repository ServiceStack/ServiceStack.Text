using System;
using System.Collections.Generic;
using ServiceStack.Text.Common;
using ServiceStack.Text.Json;
using ServiceStack.Text.Jsv;

namespace ServiceStack.Text
{
	public static class JsConfig
	{
		[ThreadStatic]
		public static bool ConvertObjectTypesIntoStringDictionary = false;

		[ThreadStatic]
		public static bool IncludeNullValues = false;

#if SILVERLIGHT || MONOTOUCH
		/// <summary>
		/// Provide hint to MonoTouch AOT compiler to pre-compile generic classes for all your DTOs.
		/// Just needs to be called once in a static constructor.
		/// </summary>
		public static void InitForAot() {}
		
		public static void RegisterForAot()
		{
			JsonAotConfig.Register<Poco>();
			
			RegisterElement<Poco, string>();
			
			RegisterElement<Poco, bool>();
			RegisterElement<Poco, char>();
			RegisterElement<Poco, byte>();
			RegisterElement<Poco, sbyte>();
			RegisterElement<Poco, short>();
			RegisterElement<Poco, ushort>();
			RegisterElement<Poco, int>();
			RegisterElement<Poco, uint>();
			RegisterElement<Poco, long>();
			RegisterElement<Poco, ulong>();
			RegisterElement<Poco, float>();
			RegisterElement<Poco, double>();
			RegisterElement<Poco, decimal>();
			RegisterElement<Poco, Guid>();
			RegisterElement<Poco, DateTime>();
			RegisterElement<Poco, TimeSpan>();

			RegisterElement<Poco, bool?>();
			RegisterElement<Poco, char?>();
			RegisterElement<Poco, byte?>();
			RegisterElement<Poco, sbyte?>();
			RegisterElement<Poco, short?>();
			RegisterElement<Poco, ushort?>();
			RegisterElement<Poco, int?>();
			RegisterElement<Poco, uint?>();
			RegisterElement<Poco, long?>();
			RegisterElement<Poco, ulong?>();
			RegisterElement<Poco, float?>();
			RegisterElement<Poco, double?>();
			RegisterElement<Poco, decimal?>();
			RegisterElement<Poco, Guid?>();
			RegisterElement<Poco, DateTime?>();
			RegisterElement<Poco, TimeSpan?>();
		}

		static void RegisterQueryStringWriter()
		{
			var i = 0;
			if (QueryStringWriter<Poco>.WriteFn() != null) i++;
		}

		static void RegisterCsvSerializer()
		{
			CsvSerializer<Poco>.WriteFn();
			CsvSerializer<Poco>.WriteObject(null, null);
			CsvWriter<Poco>.WriteObject(null, null);
			CsvWriter<Poco>.WriteObjectRow(null, null);
		}
		
		public static void RegisterElement<T,TElement>()
		{
			JsonAotConfig.RegisterElement<T, TElement>();
		}
#endif
		
	}

#if SILVERLIGHT || MONOTOUCH
	internal class Poco
	{
		public string Dummy { get; set; }
	}
	
	internal class JsonAotConfig
	{
		static JsReader<JsonTypeSerializer> reader;
		static JsonTypeSerializer serializer;
		
		static JsonAotConfig()
		{
			serializer = new JsonTypeSerializer();
			reader = new JsReader<JsonTypeSerializer>();
		}
		
		public static ParseStringDelegate GetParseFn(Type type)
		{
			var parseFn = JsonTypeSerializer.Instance.GetParseFn(type);
			return parseFn;
		}

		internal static ParseStringDelegate RegisterBuiltin<T>()
		{
			var i = 0;
			if (reader.GetParseFn<T>() != null) i++;
			if (JsonReader<T>.GetParseFn() != null) i++;
			if (JsonReader<T>.Parse(null) != null) i++;
			if (JsonWriter<T>.WriteFn() != null) i++;

			return serializer.GetParseFn<T>();
		}
		
		public static void Register<T>()
		{
			var i = 0;
			var serializer = JsonTypeSerializer.Instance;
			if (new List<T>() != null) i++;
			if (new T[0] != null) i++;
			if (serializer.GetParseFn<T>() != null) i++;
			if (DeserializeArray<T[], JsonTypeSerializer>.Parse != null) i++;
			
			DeserializeArrayWithElements<T, JsonTypeSerializer>.ParseGenericArray(null, null);
			DeserializeCollection<JsonTypeSerializer>.ParseCollection<T>(null, null, null);
			DeserializeListWithElements<T, JsonTypeSerializer>.ParseGenericList(null, null, null);

			SpecializedQueueElements<T>.ConvertToQueue(null);
			SpecializedQueueElements<T>.ConvertToStack(null);

			WriteListsOfElements<T, JsonTypeSerializer>.WriteList(null, null);
			WriteListsOfElements<T, JsonTypeSerializer>.WriteIList(null, null);
			WriteListsOfElements<T, JsonTypeSerializer>.WriteEnumerable(null, null);
			WriteListsOfElements<T, JsonTypeSerializer>.WriteListValueType(null, null);
			WriteListsOfElements<T, JsonTypeSerializer>.WriteIListValueType(null, null);

			JsonReader<T>.Parse(null);
			JsonWriter<T>.WriteFn();

			TranslateListWithElements<T>.LateBoundTranslateToGenericICollection(null, null);
			TranslateListWithConvertibleElements<T, T>.LateBoundTranslateToGenericICollection(null, null);
		}

		public static void RegisterElement<T, TElement>()
		{
			RegisterBuiltin<TElement>();
			DeserializeDictionary<JsonTypeSerializer>.ParseDictionary<T, TElement>(null, null, null, null);
			DeserializeDictionary<JsonTypeSerializer>.ParseDictionary<TElement, T>(null, null, null, null);

			ToStringDictionaryMethods<T, TElement, JsonTypeSerializer>.WriteIDictionary(null, null, null, null);
			ToStringDictionaryMethods<TElement, T, JsonTypeSerializer>.WriteIDictionary(null, null, null, null);

			TranslateListWithElements<TElement>.LateBoundTranslateToGenericICollection(null, typeof(List<TElement>));
			TranslateListWithConvertibleElements<TElement, TElement>.LateBoundTranslateToGenericICollection(null, typeof(List<TElement>));
		}		
	}	
#endif

}