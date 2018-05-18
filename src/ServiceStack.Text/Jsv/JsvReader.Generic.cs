//Copyright (c) ServiceStack, Inc. All Rights Reserved.
//License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

using System;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.CompilerServices;
using ServiceStack.Text.Common;
#if NETSTANDARD2_0
using Microsoft.Extensions.Primitives;
#else
using ServiceStack.Text.Support;
#endif

namespace ServiceStack.Text.Jsv
{
    public static class JsvReader
    {
        internal static readonly JsReader<JsvTypeSerializer> Instance = new JsReader<JsvTypeSerializer>();

        private static Dictionary<Type, ParseFactoryDelegate> ParseFnCache = new Dictionary<Type, ParseFactoryDelegate>();

        public static ParseStringDelegate GetParseFn(Type type) => v => GetParseStringSegmentFn(type)(new StringSegment(v));

        public static ParseStringSegmentDelegate GetParseStringSegmentFn(Type type)
        {
            ParseFactoryDelegate parseFactoryFn;
            ParseFnCache.TryGetValue(type, out parseFactoryFn);

            if (parseFactoryFn != null) return parseFactoryFn();

            var genericType = typeof(JsvReader<>).MakeGenericType(type);
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

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void InitAot<T>()
        {
            Text.Jsv.JsvReader.Instance.GetParseFn<T>();
            Text.Jsv.JsvReader<T>.Parse(null);
            Text.Jsv.JsvReader<T>.GetParseFn();
            Text.Jsv.JsvReader<T>.GetParseStringSegmentFn();
        }
    }

    internal static class JsvReader<T>
    {
        private static ParseStringSegmentDelegate ReadFn;

        static JsvReader()
        {
            Refresh();
        }

        public static void Refresh()
        {
            JsConfig.InitStatics();

            if (JsvReader.Instance == null)
                return;

            ReadFn = JsvReader.Instance.GetParseStringSegmentFn<T>();
        }

        public static ParseStringDelegate GetParseFn() => ReadFn != null
            ? (ParseStringDelegate)(v => ReadFn(new StringSegment(v)))
            : Parse;

        public static ParseStringSegmentDelegate GetParseStringSegmentFn() => ReadFn ?? Parse;

        public static object Parse(string value) => value != null
            ? Parse(new StringSegment(value))
            : null;

        public static object Parse(StringSegment value)
        {
            TypeConfig<T>.Init();

            if (ReadFn == null)
            {
                if (typeof(T).IsInterface)
                {
                    throw new NotSupportedException("Can not deserialize interface type: "
                        + typeof(T).Name);
                }

                Refresh();
            }

            return value.HasValue ? ReadFn(value) : null;
        }
    }
}