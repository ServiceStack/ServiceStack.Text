//Copyright (c) Service Stack LLC. All Rights Reserved.
//License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

using System;
using System.Collections.Generic;
using System.Threading;
using ServiceStack.Text.Common;

namespace ServiceStack.Text.Json
{
	internal static class JsonReader
	{
		public static readonly JsReader<JsonTypeSerializer> Instance = new JsReader<JsonTypeSerializer>();
#if   PLATFORM_NO_USE_INTERLOCKED_COMPARE_EXCHANGE_T 
		
                
		private static object _parseFnCache = new Dictionary<Type, ParseFactoryDelegate>();

                
		private static Dictionary<Type, ParseFactoryDelegate> ParseFnCache {

            get {  return    (Dictionary<Type, ParseFactoryDelegate> ) _parseFnCache ;}

        }

		
		
#else
	
		private static Dictionary<Type, ParseFactoryDelegate> ParseFnCache = new Dictionary<Type, ParseFactoryDelegate>();
		
		
		
#endif
		

	    internal static ParseStringDelegate GetParseFn(Type type)
		{
			ParseFactoryDelegate parseFactoryFn;
            ParseFnCache.TryGetValue(type, out parseFactoryFn);

            if (parseFactoryFn != null) return parseFactoryFn();

            var genericType = typeof(JsonReader<>).MakeGenericType(type);
            var mi = genericType.GetPublicStaticMethod("GetParseFn");
            parseFactoryFn = (ParseFactoryDelegate)mi.MakeDelegate(typeof(ParseFactoryDelegate));

            Dictionary<Type, ParseFactoryDelegate> snapshot, newCache;
            do
            {
                snapshot = ParseFnCache;
                newCache = new Dictionary<Type, ParseFactoryDelegate>(ParseFnCache);
                newCache[type] = parseFactoryFn;

            } while (!ReferenceEquals(					
#if   PLATFORM_NO_USE_INTERLOCKED_COMPARE_EXCHANGE_T 
	

				Interlocked.CompareExchange(ref _parseFnCache,  ( object ) newCache,  ( object ) snapshot), snapshot));					
#else
			
			
                Interlocked.CompareExchange(ref ParseFnCache, newCache, snapshot), snapshot));
 					
					
#endif
           
            return parseFactoryFn();
		}
	}

    internal static class JsonReader<T>
	{
		private static readonly ParseStringDelegate ReadFn;

		static JsonReader()
		{
			ReadFn = JsonReader.Instance.GetParseFn<T>();
		}
		
		public static ParseStringDelegate GetParseFn()
		{
			return ReadFn ?? Parse;
		}

		public static object Parse(string value)
		{
            TypeConfig<T>.AssertValidUsage();

            if (ReadFn == null)
            {
                if (typeof(T).IsAbstract() || typeof(T).IsInterface())
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