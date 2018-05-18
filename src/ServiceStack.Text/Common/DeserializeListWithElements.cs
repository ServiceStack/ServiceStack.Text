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
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading;
using ServiceStack.Text.Json;
#if NETSTANDARD2_0
using Microsoft.Extensions.Primitives;
#endif
using ServiceStack.Text.Support;


namespace ServiceStack.Text.Common
{
    public static class DeserializeListWithElements<TSerializer>
        where TSerializer : ITypeSerializer
    {
        internal static readonly ITypeSerializer Serializer = JsWriter.GetTypeSerializer<TSerializer>();

        private static Dictionary<Type, ParseListDelegate> ParseDelegateCache
            = new Dictionary<Type, ParseListDelegate>();

        private delegate object ParseListDelegate(StringSegment value, Type createListType, ParseStringSegmentDelegate parseFn);

        public static Func<string, Type, ParseStringDelegate, object> GetListTypeParseFn(
            Type createListType, Type elementType, ParseStringDelegate parseFn)
        {
            var func = GetListTypeParseStringSegmentFn(createListType, elementType, v => parseFn(v.Value));
            return (s, t, d) => func(new StringSegment(s), t, v => d(v.Value));
        }

        private static readonly Type[] signature = {typeof(StringSegment), typeof(Type), typeof(ParseStringSegmentDelegate)};

        public static Func<StringSegment, Type, ParseStringSegmentDelegate, object> GetListTypeParseStringSegmentFn(
            Type createListType, Type elementType, ParseStringSegmentDelegate parseFn)
        {
            ParseListDelegate parseDelegate;
            if (ParseDelegateCache.TryGetValue(elementType, out parseDelegate))
                return parseDelegate.Invoke;

            var genericType = typeof(DeserializeListWithElements<,>).MakeGenericType(elementType, typeof(TSerializer));
            var mi = genericType.GetStaticMethod("ParseGenericList", signature);
            parseDelegate = (ParseListDelegate)mi.MakeDelegate(typeof(ParseListDelegate));

            Dictionary<Type, ParseListDelegate> snapshot, newCache;
            do
            {
                snapshot = ParseDelegateCache;
                newCache = new Dictionary<Type, ParseListDelegate>(ParseDelegateCache);
                newCache[elementType] = parseDelegate;

            } while (!ReferenceEquals(
                Interlocked.CompareExchange(ref ParseDelegateCache, newCache, snapshot), snapshot));

            return parseDelegate.Invoke;
        }

        public static string StripList(string value)
        {   
            return StripList(new StringSegment(value)).Value;
        }

        public static StringSegment StripList(StringSegment value)
        {
            if (value.IsNullOrEmpty())
                return default(StringSegment);

            value = value.Trim();

            const int startQuotePos = 1;
            const int endQuotePos = 2;
            var ret = value.GetChar(0) == JsWriter.ListStartChar
                    ? value.Subsegment(startQuotePos, value.Length - endQuotePos)
                    : value;
            var pos = 0;
            Serializer.EatWhitespace(ret, ref pos);
            var val = ret.Subsegment(pos, ret.Length - pos);
            return val;
        }

        public static List<string> ParseStringList(string value)
        {
            return ParseStringList(new StringSegment(value));
        }

        public static List<string> ParseStringList(StringSegment value)
        {
            if (!(value = StripList(value)).HasValue) return null;
            if (value.Length == 0) return new List<string>();

            var to = new List<string>();
            var valueLength = value.Length;

            var i = 0;
            while (i < valueLength)
            {
                var elementValue = Serializer.EatValue(value, ref i);
                var listValue = Serializer.UnescapeString(elementValue);
                to.Add(listValue.Value);
                if (Serializer.EatItemSeperatorOrMapEndChar(value, ref i) && i == valueLength)
                {
                    // If we ate a separator and we are at the end of the value, 
                    // it means the last element is empty => add default
                    to.Add(null);
                }
            }

            return to;
        }

        public static List<int> ParseIntList(string value) => ParseIntList(new StringSegment(value));

        public static List<int> ParseIntList(StringSegment value)
        {
            if (!(value = StripList(value)).HasValue) return null;
            if (value.Length == 0) return new List<int>();

            var to = new List<int>();
            var valueLength = value.Length;

            var i = 0;
            while (i < valueLength)
            {
                var elementValue = Serializer.EatValue(value, ref i);
                to.Add(int.Parse(elementValue.Value));
                Serializer.EatItemSeperatorOrMapEndChar(value, ref i);
            }

            return to;
        }

        public static List<byte> ParseByteList(string value)
        {
            if ((value = StripList(value)) == null) return null;
            if (value == string.Empty) return new List<byte>();

            var to = new List<byte>();
            var valueLength = value.Length;

            var i = 0;
            while (i < valueLength)
            {
                var elementValue = Serializer.EatValue(value, ref i);
                to.Add(byte.Parse(elementValue));
                Serializer.EatItemSeperatorOrMapEndChar(value, ref i);
            }

            return to;
        }
    }

    public static class DeserializeListWithElements<T, TSerializer>
        where TSerializer : ITypeSerializer
    {
        private static readonly ITypeSerializer Serializer = JsWriter.GetTypeSerializer<TSerializer>();

        public static ICollection<T> ParseGenericList(string value, Type createListType, ParseStringDelegate parseFn)
        {
            return ParseGenericList(new StringSegment(value), createListType, v => parseFn(v.Value));
        }


        public static ICollection<T> ParseGenericList(StringSegment value, Type createListType, ParseStringSegmentDelegate parseFn)
        {
            if (!(value = DeserializeListWithElements<TSerializer>.StripList(value)).HasValue) return null;

            var isReadOnly = createListType != null
                && (createListType.IsGenericType && createListType.GetGenericTypeDefinition() == typeof(ReadOnlyCollection<>));

            var to = (createListType == null || isReadOnly)
                ? new List<T>()
                : (ICollection<T>)createListType.CreateInstance();

            if (value.Length == 0)
                return isReadOnly ? (ICollection<T>)Activator.CreateInstance(createListType, to) : to;

            var tryToParseItemsAsPrimitiveTypes =
                JsConfig.TryToParsePrimitiveTypeValues && typeof(T) == typeof(object);

            if (!value.IsNullOrEmpty())
            {
                var valueLength = value.Length;
                var i = 0;
                Serializer.EatWhitespace(value, ref i);
                if (i < valueLength && value.GetChar(i) == JsWriter.MapStartChar)
                {
                    do
                    {
                        var itemValue = Serializer.EatTypeValue(value, ref i);
                        if (itemValue.HasValue)
                        {
                            to.Add((T)parseFn(itemValue));
                        }
                        else
                        {
                            to.Add(default(T));
                        }
                        Serializer.EatWhitespace(value, ref i);
                    } while (++i < value.Length);
                }
                else
                {
                    
                    while (i < valueLength)
                    {
                        var startIndex = i;
                        var elementValue = Serializer.EatValue(value, ref i);
                        var listValue = elementValue;
                        if (listValue.HasValue)
                        {
                            if (tryToParseItemsAsPrimitiveTypes)
                            {
                                Serializer.EatWhitespace(value, ref startIndex);
                                to.Add((T)DeserializeType<TSerializer>.ParsePrimitive(elementValue.Value, value.GetChar(startIndex)));
                            }
                            else
                            {
                                to.Add((T)parseFn(elementValue));
                            }
                        }

                        if (Serializer.EatItemSeperatorOrMapEndChar(value, ref i) && i == valueLength)
                        {
                            // If we ate a separator and we are at the end of the value, 
                            // it means the last element is empty => add default
                            to.Add(default(T));
                            continue;
                        }

                        if (!listValue.HasValue)
                            to.Add(default(T));
                    }

                }
            }

            //TODO: 8-10-2011 -- this CreateInstance call should probably be moved over to ReflectionExtensions, 
            //but not sure how you'd like to go about caching constructors with parameters -- I would probably build a NewExpression, .Compile to a LambdaExpression and cache
            return isReadOnly ? (ICollection<T>)Activator.CreateInstance(createListType, to) : to;
        }
    }

    public static class DeserializeList<T, TSerializer>
        where TSerializer : ITypeSerializer
    {
        private static readonly ParseStringSegmentDelegate CacheFn;

        static DeserializeList()
        {
            CacheFn = GetParseStringSegmentFn();
        }

        public static ParseStringDelegate Parse => v => CacheFn(new StringSegment(v));

        public static ParseStringSegmentDelegate ParseStringSegment => CacheFn;

        public static ParseStringDelegate GetParseFn() => v => GetParseStringSegmentFn()(new StringSegment(v));

        public static ParseStringSegmentDelegate GetParseStringSegmentFn()
        {
            var listInterface = typeof(T).GetTypeWithGenericInterfaceOf(typeof(IList<>));
            if (listInterface == null)
                throw new ArgumentException($"Type {typeof(T).FullName} is not of type IList<>");

            //optimized access for regularly used types
            if (typeof(T) == typeof(List<string>))
                return DeserializeListWithElements<TSerializer>.ParseStringList;

            if (typeof(T) == typeof(List<int>))
                return DeserializeListWithElements<TSerializer>.ParseIntList;

            var elementType = listInterface.GetGenericArguments()[0];

            var supportedTypeParseMethod = DeserializeListWithElements<TSerializer>.Serializer.GetParseStringSegmentFn(elementType);
            if (supportedTypeParseMethod != null)
            {
                var createListType = typeof(T).HasAnyTypeDefinitionsOf(typeof(List<>), typeof(IList<>))
                    ? null : typeof(T);

                var parseFn = DeserializeListWithElements<TSerializer>.GetListTypeParseStringSegmentFn(createListType, elementType, supportedTypeParseMethod);
                return value => parseFn(value, createListType, supportedTypeParseMethod);
            }

            return null;
        }

    }

    internal static class DeserializeEnumerable<T, TSerializer>
        where TSerializer : ITypeSerializer
    {
        private static readonly ParseStringSegmentDelegate CacheFn;

        static DeserializeEnumerable()
        {
            CacheFn = GetParseStringSegmentFn();
        }

        public static ParseStringDelegate Parse => v => CacheFn(new StringSegment(v));

        public static ParseStringSegmentDelegate ParseStringSegment => CacheFn;

        public static ParseStringDelegate GetParseFn() => v => GetParseStringSegmentFn()(new StringSegment(v));

        public static ParseStringSegmentDelegate GetParseStringSegmentFn()
        {
            var enumerableInterface = typeof(T).GetTypeWithGenericInterfaceOf(typeof(IEnumerable<>));
            if (enumerableInterface == null)
                throw new ArgumentException($"Type {typeof(T).FullName} is not of type IEnumerable<>");

            //optimized access for regularly used types
            if (typeof(T) == typeof(IEnumerable<string>))
                return DeserializeListWithElements<TSerializer>.ParseStringList;

            if (typeof(T) == typeof(IEnumerable<int>))
                return DeserializeListWithElements<TSerializer>.ParseIntList;

            var elementType = enumerableInterface.GetGenericArguments()[0];

            var supportedTypeParseMethod = DeserializeListWithElements<TSerializer>.Serializer.GetParseStringSegmentFn(elementType);
            if (supportedTypeParseMethod != null)
            {
                const Type createListTypeWithNull = null; //Use conversions outside this class. see: Queue

                var parseFn = DeserializeListWithElements<TSerializer>.GetListTypeParseStringSegmentFn(
                    createListTypeWithNull, elementType, supportedTypeParseMethod);

                return value => parseFn(value, createListTypeWithNull, supportedTypeParseMethod);
            }

            return null;
        }

    }
}