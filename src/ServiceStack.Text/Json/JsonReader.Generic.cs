//Copyright (c) ServiceStack, Inc. All Rights Reserved.
//License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

using System;
using System.Collections.Generic;
using System.Threading;
using ServiceStack.Text.Common;
#if NETSTANDARD1_1
using Microsoft.Extensions.Primitives;
#endif
using ServiceStack.Text.Support;

namespace ServiceStack.Text.Json
{
    public static class JsonReader
    {
        public static readonly JsReader<JsonTypeSerializer> Instance = new JsReader<JsonTypeSerializer>();

        private static Dictionary<Type, ParseFactoryDelegate> ParseFnCache = new Dictionary<Type, ParseFactoryDelegate>();

        internal static ParseStringDelegate GetParseFn(Type type) => v => GetParseStringSegmentFn(type)(new StringSegment(v));

        internal static ParseStringSegmentDelegate GetParseStringSegmentFn(Type type)
        {
            ParseFactoryDelegate parseFactoryFn;
            ParseFnCache.TryGetValue(type, out parseFactoryFn);

            if (parseFactoryFn != null) return parseFactoryFn();

            var genericType = typeof(JsonReader<>).MakeGenericType(type);
            var mi = genericType.GetStaticMethod("GetParseStringSegmentFn");    
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

    public static class JsonReader<T>
    {
        private static ParseStringSegmentDelegate ReadFn;

        static JsonReader()
        {
            Refresh();
        }

        public static void Refresh()
        {
            JsConfig.InitStatics();

            if (JsonReader.Instance == null)
                return;

            ReadFn = JsonReader.Instance.GetParseStringSegmentFn<T>();
        }

        public static ParseStringDelegate GetParseFn() => 
            ReadFn != null 
            ? (ParseStringDelegate)(v => ReadFn(new StringSegment(v))) 
            : Parse;

        public static ParseStringSegmentDelegate GetParseStringSegmentFn() => ReadFn ?? Parse;

        public static object Parse(string value) => Parse(new StringSegment(value));

        public static object Parse(StringSegment value)
        {
            TypeConfig<T>.Init();

            if (ReadFn == null)
            {
                if (typeof(T).IsAbstract() || typeof(T).IsInterface())
                {
                    if (value.IsNullOrEmpty()) return null;
                    var concreteType = DeserializeType<JsonTypeSerializer>.ExtractType(value);
                    if (concreteType != null)
                    {
                        return JsonReader.GetParseStringSegmentFn(concreteType)(value);
                    }
                    throw new NotSupportedException("Can not deserialize interface type: "
                        + typeof(T).Name);
                }

                Refresh();
            }

            return value.HasValue
                    ? ReadFn(value)
                    : null;
        }
    }
}