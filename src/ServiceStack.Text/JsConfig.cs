using System;
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

		/// <summary>
		/// Registers for AOT.
		/// </summary>
		public static void RegisterForAot<T>()
		{
			JsonAotConfig.Instance.Register<T>();
		}
	}

	public class JsonAotConfig
	{
		public static JsonAotConfig Instance = new JsonAotConfig();

		public void Register<T>()
		{
			var i = 0;
			DeserializeArrayWithElements<T, JsonTypeSerializer>.ParseGenericArray(null, null);
			if (DeserializeArray<T, JsonTypeSerializer>.Parse != null) i++;
			DeserializeCollection<JsonTypeSerializer>.ParseCollection<T>(null, null, null);
			DeserializeListWithElements<T, JsonTypeSerializer>.ParseGenericList(null, null, null);

			SpecializedQueueElements<T>.ConvertToQueue(null);
			SpecializedQueueElements<T>.ConvertToStack(null);

			RegisterForCommonBuiltinTypes<T>();

			WriteListsOfElements<T, JsonTypeSerializer>.WriteList(null, null);
			WriteListsOfElements<T, JsonTypeSerializer>.WriteIList(null, null);
			WriteListsOfElements<T, JsonTypeSerializer>.WriteEnumerable(null, null);
			WriteListsOfElements<T, JsonTypeSerializer>.WriteListValueType(null, null);
			WriteListsOfElements<T, JsonTypeSerializer>.WriteIListValueType(null, null);

			CsvSerializer<T>.WriteFn();
			CsvSerializer<T>.WriteObject(null, null);
			CsvWriter<T>.WriteObject(null, null);
			CsvWriter<T>.WriteObjectRow(null, null);

			JsonReader<T>.Parse(null);
			JsonWriter<T>.WriteFn();

			QueryStringWriter<T>.WriteFn();

			TranslateListWithElements<T>.LateBoundTranslateToGenericICollection(null, null);
			TranslateListWithConvertibleElements<T, JsonTypeSerializer>.LateBoundTranslateToGenericICollection(null, null);
		}

		private void RegisterForCommonBuiltinTypes<T>()
		{
			RegisterElementBuiltinTypes<T, int>();
			RegisterElementBuiltinTypes<T, string>();
			RegisterElementBuiltinTypes<T, Guid>();
		}

		private void RegisterElementBuiltinTypes<T, TElement>()
		{
			DeserializeDictionary<JsonTypeSerializer>.ParseDictionary<T, TElement>(null, null, null, null);
			DeserializeDictionary<JsonTypeSerializer>.ParseDictionary<TElement, T>(null, null, null, null);

			ToStringDictionaryMethods<T, TElement, JsonTypeSerializer>.WriteIDictionary(null, null, null, null);
			ToStringDictionaryMethods<TElement, T, JsonTypeSerializer>.WriteIDictionary(null, null, null, null);

			TranslateListWithElements<TElement>.LateBoundTranslateToGenericICollection(null, typeof(T));
			TranslateListWithConvertibleElements<TElement, JsonTypeSerializer>.LateBoundTranslateToGenericICollection(null, typeof(T));
		}
	}
}