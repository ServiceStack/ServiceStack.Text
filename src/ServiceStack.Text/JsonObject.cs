using System;
using System.Collections.Generic;

namespace ServiceStack.Text
{
	public static class JsonExtensions
	{
		public static T JsonTo<T>(this Dictionary<string, string> map, string key)
		{
			string strVal;
			return map.TryGetValue(key, out strVal) ? JsonSerializer.DeserializeFromString<T>(strVal) : default(T);
		}

		public static string Get(this Dictionary<string, string> map, string key)
		{
			string strVal;
			return map.TryGetValue(key, out strVal) ? strVal : null;
		}

		public static JsonArrayObjects ArrayObjects(this string json, string propertyName)
		{
			return Text.JsonArrayObjects.Parse(json);
		}

		public static List<T> ConvertAll<T>(this JsonArrayObjects jsonArrayObjects, Func<JsonObject, T> converter)
		{
			var results = new List<T>();

			foreach (var jsonObject in jsonArrayObjects)
			{
				results.Add(converter(jsonObject));
			}

			return results;
		}
	}

	public class JsonObject : Dictionary<string, string>
	{
		public static JsonObject Parse(string json)
		{
			return JsonSerializer.DeserializeFromString<JsonObject>(json);
		}

		public JsonArrayObjects ArrayObjects(string propertyName)
		{
			string strValue;
			this.TryGetValue(propertyName, out strValue);
			return strValue == null 
				? null 
				: Text.JsonArrayObjects.Parse(strValue);
		}
	}

	public class JsonArrayObjects : List<JsonObject>
	{
		public static JsonArrayObjects Parse(string json)
		{
			return JsonSerializer.DeserializeFromString<JsonArrayObjects>(json);
		}
	}

}