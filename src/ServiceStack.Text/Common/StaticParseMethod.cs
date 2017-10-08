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
using System.Reflection;
using System.Linq;
using ServiceStack.Text.Jsv;
#if NETSTANDARD2_0
using Microsoft.Extensions.Primitives;
#else
using ServiceStack.Text.Support;
#endif

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

        public static ParseStringSegmentDelegate GetParseStringSegmentFn<T>(string parseMethod)
        {
            // Get the static Parse(string) method on the type supplied
            var parseMethodInfo = typeof(T).GetStaticMethod(parseMethod, new[] { typeof(string) });
            if (parseMethodInfo == null)
                return null;

            ParseStringSegmentDelegate parseDelegate = null;
            try
            {
                if (parseMethodInfo.ReturnType != typeof(T))
                {
                    parseDelegate = (ParseStringSegmentDelegate)parseMethodInfo.MakeDelegate(typeof(ParseStringSegmentDelegate), false);
                }
                if (parseDelegate == null)
                {
                    //Try wrapping strongly-typed return with wrapper fn.
                    var typedParseDelegate = (Func<StringSegment, T>)parseMethodInfo.MakeDelegate(typeof(Func<StringSegment, T>));
                    parseDelegate = x => typedParseDelegate(x);
                }
            }
            catch (ArgumentException)
            {
                Tracer.Instance.WriteDebug("Nonstandard Parse method on type {0}", typeof(T));
            }

            if (parseDelegate != null)
                return value => parseDelegate(new StringSegment(value.Value.FromCsvField()));

            return null;
        }
    }

    public static class StaticParseMethod<T>
    {
        const string ParseMethod = "Parse";
        const string ParseStringSegmentMethod = "ParseStringSegment";

        private static readonly ParseStringDelegate CacheFn;
        private static readonly ParseStringSegmentDelegate CacheStringSegmentFn;

        public static ParseStringDelegate Parse => CacheFn;
        public static ParseStringSegmentDelegate ParseStringSegment => CacheStringSegmentFn;

        static StaticParseMethod()
        {
            CacheFn = ParseMethodUtilities.GetParseFn<T>(ParseMethod);
            CacheStringSegmentFn = ParseMethodUtilities.GetParseStringSegmentFn<T>(ParseMethod);
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
        private static readonly ParseStringSegmentDelegate CacheStringSegmentFn;

        public static ParseStringDelegate Parse => CacheFn;
        public static ParseStringSegmentDelegate ParseStringSegment => CacheStringSegmentFn;

        static StaticParseRefTypeMethod()
        {
            CacheFn = ParseMethodUtilities.GetParseFn<T>(ParseMethod);
            CacheStringSegmentFn = ParseMethodUtilities.GetParseStringSegmentFn<T>(ParseStringSegmentMethod);
        }
    }

}