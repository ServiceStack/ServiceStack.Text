//
// http://code.google.com/p/servicestack/wiki/TypeSerializer
// ServiceStack.Text: .NET C# POCO Type Text Serializer.
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2011 Liquidbit Ltd.
//
// Licensed under the same terms of ServiceStack: new BSD license.
//

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using ServiceStack.Text.Common;

namespace ServiceStack.Text.Json
{
	internal static class JsonReader
	{
		public static readonly JsReader<JsonTypeSerializer> Instance = new JsReader<JsonTypeSerializer>();

		private static Dictionary<Type, ParseFactoryDelegate> ParseFnCache = new Dictionary<Type, ParseFactoryDelegate>();
        
		public static ParseStringDelegate GetParseFn(Type type)
		{
			ParseFactoryDelegate parseFactoryFn;
            ParseFnCache.TryGetValue(type, out parseFactoryFn);

            if (parseFactoryFn != null) return parseFactoryFn();

            var genericType = typeof(JsonReader<>).MakeGenericType(type);
            var mi = genericType.GetMethod("GetParseFn", BindingFlags.Public | BindingFlags.Static);
            parseFactoryFn = (ParseFactoryDelegate)Delegate.CreateDelegate(typeof(ParseFactoryDelegate), mi);

            Dictionary<Type, ParseFactoryDelegate> snapshot, newCache;
            do
            {
                snapshot = ParseFnCache;
                newCache = new Dictionary<Type, ParseFactoryDelegate>(ParseFnCache);
                newCache[type] = parseFactoryFn;

            } while (!ReferenceEquals(
                Interlocked.CompareExchange(ref ParseFnCache, newCache, snapshot), snapshot));
            
            return parseFactoryFn();
		}
	}

	public static class JsonReader<T>
	{
		private static readonly ParseStringDelegate ReadFn;

		static JsonReader()
		{
			ReadFn = JsonReader.Instance.GetParseFn<T>();
		}
		
		public static ParseStringDelegate GetParseFn()
		{
			return ReadFn;
		}

		public static object Parse(string value)
		{
			if (ReadFn == null)
			{
                if (typeof(T).IsAbstract || typeof(T).IsInterface)
				{
					if (string.IsNullOrEmpty(value)) return null;
					var concreteType = DeserializeType<JsonTypeSerializer>.ExtractType(value);
					if (concreteType != null)
					{
						return JsonReader.GetParseFn(concreteType)(value);
					}
					throw new NotSupportedException("Can not deserialize interface type: "
						+ typeof(T).Name);
				}
			}
			return value == null 
			       	? null 
			       	: ReadFn(value);
		}
	}
}