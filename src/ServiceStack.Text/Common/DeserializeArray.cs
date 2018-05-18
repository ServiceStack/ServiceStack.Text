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
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Threading;
#if NETSTANDARD2_0 
using Microsoft.Extensions.Primitives;
#endif
using ServiceStack.Text.Support;

namespace ServiceStack.Text.Common
{
    public static class DeserializeArrayWithElements<TSerializer>
        where TSerializer : ITypeSerializer
    {
        private static Dictionary<Type, ParseArrayOfElementsDelegate> ParseDelegateCache
            = new Dictionary<Type, ParseArrayOfElementsDelegate>();

        private delegate object ParseArrayOfElementsDelegate(StringSegment value, ParseStringSegmentDelegate parseFn);

        public static Func<string, ParseStringDelegate, object> GetParseFn(Type type)
        {
            var func = GetParseStringSegmentFn(type);
            return (s, d) => func(new StringSegment(s), v => d(v.Value));
        }

        private static readonly Type[] signature = {typeof(StringSegment), typeof(ParseStringSegmentDelegate)};

        public static Func<StringSegment, ParseStringSegmentDelegate, object> GetParseStringSegmentFn(Type type)
        {
            ParseArrayOfElementsDelegate parseFn;
            if (ParseDelegateCache.TryGetValue(type, out parseFn)) return parseFn.Invoke;

            var genericType = typeof(DeserializeArrayWithElements<,>).MakeGenericType(type, typeof(TSerializer));
            var mi = genericType.GetStaticMethod("ParseGenericArray", signature);
            parseFn = (ParseArrayOfElementsDelegate)mi.CreateDelegate(typeof(ParseArrayOfElementsDelegate));

            Dictionary<Type, ParseArrayOfElementsDelegate> snapshot, newCache;
            do
            {
                snapshot = ParseDelegateCache;
                newCache = new Dictionary<Type, ParseArrayOfElementsDelegate>(ParseDelegateCache);
                newCache[type] = parseFn;

            } while (!ReferenceEquals(
                Interlocked.CompareExchange(ref ParseDelegateCache, newCache, snapshot), snapshot));

            return parseFn.Invoke;
        }
    }

    public static class DeserializeArrayWithElements<T, TSerializer>
        where TSerializer : ITypeSerializer
    {
        private static readonly ITypeSerializer Serializer = JsWriter.GetTypeSerializer<TSerializer>();

        public static T[] ParseGenericArray(string value, ParseStringDelegate elementParseFn) =>
            ParseGenericArray(new StringSegment(value), v => elementParseFn(v.Value));

        public static T[] ParseGenericArray(StringSegment value, ParseStringSegmentDelegate elementParseFn)
        {
            if (!(value = DeserializeListWithElements<TSerializer>.StripList(value)).HasValue) return null;
            if (value.Length == 0) return new T[0];

            if (value.GetChar(0) == JsWriter.MapStartChar)
            {
                var itemValues = new List<StringSegment>();
                var i = 0;
                do
                {
                    itemValues.Add(Serializer.EatTypeValue(value, ref i));
                    Serializer.EatItemSeperatorOrMapEndChar(value, ref i);
                } while (i < value.Length);

                var results = new T[itemValues.Count];
                for (var j = 0; j < itemValues.Count; j++)
                {
                    results[j] = (T)elementParseFn(itemValues[j]);
                }
                return results;
            }
            else
            {
                var to = new List<T>();
                var valueLength = value.Length;

                var i = 0;
                while (i < valueLength)
                {
                    var elementValue = Serializer.EatValue(value, ref i);
                    var listValue = elementValue;
                    to.Add((T)elementParseFn(listValue));
                    if (Serializer.EatItemSeperatorOrMapEndChar(value, ref i)
                        && i == valueLength)
                    {
                        // If we ate a separator and we are at the end of the value, 
                        // it means the last element is empty => add default
                        to.Add(default(T));
                    }
                }

                return to.ToArray();
            }
        }
    }

    internal static class DeserializeArray<TSerializer>
        where TSerializer : ITypeSerializer
    {
        private static Dictionary<Type, ParseStringSegmentDelegate> ParseDelegateCache = new Dictionary<Type, ParseStringSegmentDelegate>();

        public static ParseStringDelegate GetParseFn(Type type) => v => GetParseStringSegmentFn(type)(new StringSegment(v));

        public static ParseStringSegmentDelegate GetParseStringSegmentFn(Type type)
        {
            ParseStringSegmentDelegate parseFn;
            if (ParseDelegateCache.TryGetValue(type, out parseFn)) return parseFn;

            var genericType = typeof(DeserializeArray<,>).MakeGenericType(type, typeof(TSerializer));

            var mi = genericType.GetStaticMethod("GetParseStringSegmentFn");
            var parseFactoryFn = (Func<ParseStringSegmentDelegate>)mi.MakeDelegate(
                typeof(Func<ParseStringSegmentDelegate>));
            parseFn = parseFactoryFn();

            Dictionary<Type, ParseStringSegmentDelegate> snapshot, newCache;
            do
            {
                snapshot = ParseDelegateCache;
                newCache = new Dictionary<Type, ParseStringSegmentDelegate>(ParseDelegateCache);
                newCache[type] = parseFn;

            } while (!ReferenceEquals(
                Interlocked.CompareExchange(ref ParseDelegateCache, newCache, snapshot), snapshot));

            return parseFn;
        }
    }

    internal static class DeserializeArray<T, TSerializer>
        where TSerializer : ITypeSerializer
    {
        private static readonly ITypeSerializer Serializer = JsWriter.GetTypeSerializer<TSerializer>();

        private static readonly ParseStringSegmentDelegate CacheFn;

        static DeserializeArray()
        {
            CacheFn = GetParseStringSegmentFn();
        }

        public static ParseStringDelegate Parse => v => CacheFn(new StringSegment(v));

        public static ParseStringSegmentDelegate ParseStringSegment => CacheFn;

        public static ParseStringDelegate GetParseFn() => v => GetParseStringSegmentFn()(new StringSegment(v));

        public static ParseStringSegmentDelegate GetParseStringSegmentFn()
        {
            var type = typeof(T);
            if (!type.IsArray)
                throw new ArgumentException(string.Format("Type {0} is not an Array type", type.FullName));

            if (type == typeof(string[]))
                return ParseStringArray;
            if (type == typeof(byte[]))
                return v => ParseByteArray(v.Value);

            var elementType = type.GetElementType();
            var elementParseFn = Serializer.GetParseStringSegmentFn(elementType);
            if (elementParseFn != null)
            {
                var parseFn = DeserializeArrayWithElements<TSerializer>.GetParseStringSegmentFn(elementType);
                return value => parseFn(value, elementParseFn);
            }
            return null;
        }

        public static string[] ParseStringArray(StringSegment value)
        {
            if (!(value = DeserializeListWithElements<TSerializer>.StripList(value)).HasValue) return null;
            return value.Length == 0
                ? TypeConstants.EmptyStringArray
                : DeserializeListWithElements<TSerializer>.ParseStringList(value).ToArray();
        }


        public static string[] ParseStringArray(string value)
        {
            if ((value = DeserializeListWithElements<TSerializer>.StripList(value)) == null) return null;
            return value == string.Empty
                    ? TypeConstants.EmptyStringArray
                    : DeserializeListWithElements<TSerializer>.ParseStringList(value).ToArray();
        }

        public static byte[] ParseByteArray(string value)
        {
            var isArray = !string.IsNullOrEmpty(value) && value.Length > 1 && value[0] == '[';
            if ((value = DeserializeListWithElements<TSerializer>.StripList(value)) == null) return null;
            if ((value = Serializer.UnescapeString(value)) == null) return null;
            return value == string.Empty
                    ? TypeConstants.EmptyByteArray
                    : !isArray 
                        ? Convert.FromBase64String(value)
                        : DeserializeListWithElements<TSerializer>.ParseByteList(value).ToArray();
        }
    }
}