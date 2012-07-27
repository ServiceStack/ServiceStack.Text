//
// https://github.com/ServiceStack/ServiceStack.Text
// ServiceStack.Text: .NET C# POCO JSON, JSV and CSV Text Serializers.
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2012 ServiceStack Ltd.
//
// Licensed under the same terms of ServiceStack: new BSD license.
//

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using ServiceStack.Text.Json;

namespace ServiceStack.Text.Common
{
	internal static class DeserializeDictionary<TSerializer>
		where TSerializer : ITypeSerializer
	{
		private static readonly ITypeSerializer Serializer = JsWriter.GetTypeSerializer<TSerializer>();

		const int KeyIndex = 0;
		const int ValueIndex = 1;

		public static ParseStringDelegate GetParseMethod(Type type)
		{
			var mapInterface = type.GetTypeWithGenericInterfaceOf(typeof(IDictionary<,>));
			if (mapInterface == null)
				throw new ArgumentException(string.Format("Type {0} is not of type IDictionary<,>", type.FullName));

			//optimized access for regularly used types
            if (type == typeof(Dictionary<string, string>))
            {
                return ParseStringDictionary;
            }
            if (type == typeof(JsonObject))
            {
                return ParseJsonObject;
            }

            var dictionaryArgs = mapInterface.GetGenericArguments();

			var keyTypeParseMethod = Serializer.GetParseFn(dictionaryArgs[KeyIndex]);
			if (keyTypeParseMethod == null) return null;

			var valueTypeParseMethod = Serializer.GetParseFn(dictionaryArgs[ValueIndex]);
			if (valueTypeParseMethod == null) return null;

			var createMapType = type.HasAnyTypeDefinitionsOf(typeof(Dictionary<,>), typeof(IDictionary<,>))
				? null : type;

			return value => ParseDictionaryType(value, createMapType, dictionaryArgs, keyTypeParseMethod, valueTypeParseMethod);
		}

        public static JsonObject ParseJsonObject(string value)
        {
            var index = VerifyAndGetStartIndex(value, typeof(JsonObject));

            var result = new JsonObject();

            if (value == JsWriter.EmptyMap) return result;

            var valueLength = value.Length;
            while (index < valueLength)
            {
                var keyValue = Serializer.EatMapKey(value, ref index);
                Serializer.EatMapKeySeperator(value, ref index);
                var elementValue = Serializer.EatValue(value, ref index);

                var mapKey = keyValue;
                var mapValue = elementValue;

                result[mapKey] = mapValue;

                Serializer.EatItemSeperatorOrMapEndChar(value, ref index);
            }

            return result;
        }

        public static Dictionary<string, string> ParseStringDictionary(string value)
        {
            var index = VerifyAndGetStartIndex(value, typeof(Dictionary<string, string>));

            var result = new Dictionary<string, string>();

            if (value == JsWriter.EmptyMap) return result;

            var valueLength = value.Length;
            while (index < valueLength)
            {
                var keyValue = Serializer.EatMapKey(value, ref index);
                Serializer.EatMapKeySeperator(value, ref index);
                var elementValue = Serializer.EatValue(value, ref index);

                var mapKey = keyValue;
                var mapValue = Serializer.UnescapeString(elementValue);

                result[mapKey] = mapValue;

                Serializer.EatItemSeperatorOrMapEndChar(value, ref index);
            }

            return result;
        }

		public static IDictionary<TKey, TValue> ParseDictionary<TKey, TValue>(
			string value, Type createMapType,
			ParseStringDelegate parseKeyFn, ParseStringDelegate parseValueFn)
		{
			if (value == null) return null;

			var tryToParseItemsAsDictionaries =
				JsConfig.ConvertObjectTypesIntoStringDictionary && typeof(TValue) == typeof(object);

			var index = VerifyAndGetStartIndex(value, createMapType);

			var to = (createMapType == null)
				? new Dictionary<TKey, TValue>()
				: (IDictionary<TKey, TValue>)createMapType.CreateInstance();

			if (value == JsWriter.EmptyMap) return to;

			var valueLength = value.Length;
			while (index < valueLength)
			{
				var keyValue = Serializer.EatMapKey(value, ref index);
				Serializer.EatMapKeySeperator(value, ref index);
				var elementValue = Serializer.EatValue(value, ref index);

				var mapKey = (TKey)parseKeyFn(keyValue);
				var mapValue = (TValue)parseValueFn(elementValue);

				if (tryToParseItemsAsDictionaries)
				{
					var mapValueString = mapValue as string;
					var tryParseValueAsDictionary = JsonUtils.IsJsObject(mapValueString);
					if (tryParseValueAsDictionary)
					{
						var tmpMap = ParseDictionary<TKey, TValue>(mapValueString, createMapType, parseKeyFn, parseValueFn);
						to[mapKey] = (tmpMap != null && tmpMap.Count > 0)
							? (TValue)tmpMap
							: to[mapKey] = mapValue;
					}
					else
					{
						to[mapKey] = mapValue;
					}
				}
				else
				{
					to[mapKey] = mapValue;
				}

				Serializer.EatItemSeperatorOrMapEndChar(value, ref index);
			}

			return to;
		}

		private static int VerifyAndGetStartIndex(string value, Type createMapType)
		{
			var index = 0;
			if (!Serializer.EatMapStartChar(value, ref index))
			{
				//Don't throw ex because some KeyValueDataContractDeserializer don't have '{}'
				Tracer.Instance.WriteDebug("WARN: Map definitions should start with a '{0}', expecting serialized type '{1}', got string starting with: {2}",
					JsWriter.MapStartChar, createMapType != null ? createMapType.Name : "Dictionary<,>", value.Substring(0, value.Length < 50 ? value.Length : 50));
			}
			return index;
		}

		private static Dictionary<string, ParseDictionaryDelegate> ParseDelegateCache
			= new Dictionary<string, ParseDictionaryDelegate>();

		private delegate object ParseDictionaryDelegate(string value, Type createMapType,
			ParseStringDelegate keyParseFn, ParseStringDelegate valueParseFn);

		public static object ParseDictionaryType(string value, Type createMapType, Type[] argTypes,
			ParseStringDelegate keyParseFn, ParseStringDelegate valueParseFn)
		{

			ParseDictionaryDelegate parseDelegate;
			var key = GetTypesKey(argTypes);
			if (ParseDelegateCache.TryGetValue(key, out parseDelegate))
                return parseDelegate(value, createMapType, keyParseFn, valueParseFn);

            var mi = typeof(DeserializeDictionary<TSerializer>).GetMethod("ParseDictionary", BindingFlags.Static | BindingFlags.Public);
            var genericMi = mi.MakeGenericMethod(argTypes);
            parseDelegate = (ParseDictionaryDelegate)Delegate.CreateDelegate(typeof(ParseDictionaryDelegate), genericMi);

            Dictionary<string, ParseDictionaryDelegate> snapshot, newCache;
            do
            {
                snapshot = ParseDelegateCache;
                newCache = new Dictionary<string, ParseDictionaryDelegate>(ParseDelegateCache);
                newCache[key] = parseDelegate;

            } while (!ReferenceEquals(
                Interlocked.CompareExchange(ref ParseDelegateCache, newCache, snapshot), snapshot));

			return parseDelegate(value, createMapType, keyParseFn, valueParseFn);
		}

		private static string GetTypesKey(params Type[] types)
		{
			var sb = new StringBuilder(256);
			foreach (var type in types)
			{
				if (sb.Length > 0)
					sb.Append(">");

				sb.Append(type.FullName);
			}
			return sb.ToString();
		}
	}
}