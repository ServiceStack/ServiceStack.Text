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
			int i=0;
			DeserializeArrayWithElements<T, JsonTypeSerializer>.ParseGenericArray(null, null);
			if (DeserializeArray<T, TSerializer>.Parse != null) i++;
		}
	}
}