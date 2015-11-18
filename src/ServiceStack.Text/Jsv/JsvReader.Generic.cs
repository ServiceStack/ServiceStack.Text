//Copyright (c) Service Stack LLC. All Rights Reserved.
//License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

using System;
using System.Collections.Generic;
using System.Threading;
using ServiceStack.Text.Common;

namespace ServiceStack.Text.Jsv
{
    public static class JsvReader
	{
		internal static readonly JsReader<JsvTypeSerializer> Instance = new JsReader<JsvTypeSerializer>();

        private static Dictionary<Type, ParseFactoryDelegate> ParseFnCache = new Dictionary<Type, ParseFactoryDelegate>();

        public static ParseStringDelegate GetParseFn(Type type)
		{
			ParseFactoryDelegate parseFactoryFn;
            ParseFnCache.TryGetValue(type, out parseFactoryFn);

            if (parseFactoryFn != null) return parseFactoryFn();

            var genericType = typeof(JsvReader<>).MakeGenericType(type);
            var mi = genericType.GetStaticMethod("GetParseFn");
            parseFactoryFn = (ParseFactoryDelegate)mi.MakeDelegate(typeof(ParseFactoryDelegate));

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

    internal static class JsvReader<T>
	{
		private static ParseStringDelegate ReadFn;

		static JsvReader()
        {
            Refresh();
        }

        public static void Refresh()
        {
            JsConfig.InitStatics();

            if (JsvReader.Instance == null)
                return;

            ReadFn = JsvReader.Instance.GetParseFn<T>();
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
                if (typeof(T).IsInterface())
                {
					throw new NotSupportedException("Can not deserialize interface type: "
						+ typeof(T).Name);
				}

                Refresh();
            }

            return value == null 
			       	? null 
			       	: ReadFn(value);
		}
	}
}