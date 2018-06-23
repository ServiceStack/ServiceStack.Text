//
// https://github.com/ServiceStack/ServiceStack.Text
// ServiceStack.Text: .NET C# POCO JSON, JSV and CSV Text Serializers.
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2012 ServiceStack, Inc. All Rights Reserved.
//
// Licensed under the same terms of ServiceStack.
//

using System;
using ServiceStack.Text.Jsv;

namespace ServiceStack.Text.Common
{
    internal delegate object ParseDelegate(string value);

    internal static class ParseMethodUtilities
    {
        public static ParseStringDelegate GetParseFn<T>(string parseMethod)
        {
            // Get the static Parse(string) method on the type supplied
            var parseMethodInfo = typeof(T).GetStaticMethod(parseMethod, new[] { typeof(string) });
            if (parseMethodInfo == null)
                return null;

            ParseDelegate parseDelegate = null;
            try
            {
                if (parseMethodInfo.ReturnType != typeof(T))
                {
                    parseDelegate = (ParseDelegate)parseMethodInfo.MakeDelegate(typeof(ParseDelegate), false);
                }
                if (parseDelegate == null)
                {
                    //Try wrapping strongly-typed return with wrapper fn.
                    var typedParseDelegate = (Func<string, T>)parseMethodInfo.MakeDelegate(typeof(Func<string, T>));
                    parseDelegate = x => typedParseDelegate(x);
                }
            }
            catch (ArgumentException)
            {
                Tracer.Instance.WriteDebug("Nonstandard Parse method on type {0}", typeof(T));
            }

            if (parseDelegate != null)
                return value => parseDelegate(value.FromCsvField());

            return null;
        }

        delegate T ParseStringSegmentGenericDelegate<T>(ReadOnlySpan<char> value);

        public static ParseStringSpanDelegate GetParseStringSegmentFn<T>(string parseMethod)
        {
            // Get the static Parse(string) method on the type supplied
            var parseMethodInfo = typeof(T).GetStaticMethod(parseMethod, new[] { typeof(string) });
            if (parseMethodInfo == null)
                return null;

            ParseStringSpanDelegate parseDelegate = null;
            try
            {
                if (parseMethodInfo.ReturnType != typeof(T))
                {
                    parseDelegate = (ParseStringSpanDelegate)parseMethodInfo.MakeDelegate(typeof(ParseStringSpanDelegate), false);
                }
                if (parseDelegate == null)
                {
                    //Try wrapping strongly-typed return with wrapper fn.
                    var typedParseDelegate = (ParseStringSegmentGenericDelegate<T>)parseMethodInfo.MakeDelegate(typeof(ParseStringSegmentGenericDelegate<T>));
                    parseDelegate = x => typedParseDelegate(x);
                }
            }
            catch (ArgumentException)
            {
                Tracer.Instance.WriteDebug("Nonstandard Parse method on type {0}", typeof(T));
            }

            if (parseDelegate != null)
                return value => parseDelegate(value.ToString().FromCsvField().AsSpan());

            return null;
        }
    }

    public static class StaticParseMethod<T>
    {
        const string ParseMethod = "Parse";
        const string ParseStringSegmentMethod = "ParseStringSegment";

        private static readonly ParseStringDelegate CacheFn;
        private static readonly ParseStringSpanDelegate CacheStringSpanFn;

        public static ParseStringDelegate Parse => CacheFn;
        public static ParseStringSpanDelegate ParseStringSpan => CacheStringSpanFn;

        static StaticParseMethod()
        {
            CacheFn = ParseMethodUtilities.GetParseFn<T>(ParseMethod);
            CacheStringSpanFn = ParseMethodUtilities.GetParseStringSegmentFn<T>(ParseMethod);
        }

    }

    internal static class StaticParseRefTypeMethod<TSerializer, T>
        where TSerializer : ITypeSerializer
    {
        static readonly string ParseMethod = typeof(TSerializer) == typeof(JsvTypeSerializer)
            ? "ParseJsv"
            : "ParseJson";

        static readonly string ParseStringSegmentMethod = typeof(TSerializer) == typeof(JsvTypeSerializer)
            ? "ParseStringSegmentJsv"
            : "ParseStringSegmentJson";

        private static readonly ParseStringDelegate CacheFn;
        private static readonly ParseStringSpanDelegate CacheStringSpanFn;

        public static ParseStringDelegate Parse => CacheFn;
        public static ParseStringSpanDelegate ParseStringSpan => CacheStringSpanFn;

        static StaticParseRefTypeMethod()
        {
            CacheFn = ParseMethodUtilities.GetParseFn<T>(ParseMethod);
            CacheStringSpanFn = ParseMethodUtilities.GetParseStringSegmentFn<T>(ParseStringSegmentMethod);
        }
    }

}