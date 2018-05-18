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
using System.Threading;
using System.Linq;
#if NETSTANDARD2_0
using Microsoft.Extensions.Primitives;
#else
using ServiceStack.Text.Support;
#endif

namespace ServiceStack.Text.Common
{
    internal static class DeserializeCollection<TSerializer>
        where TSerializer : ITypeSerializer
    {
        private static readonly ITypeSerializer Serializer = JsWriter.GetTypeSerializer<TSerializer>();

        public static ParseStringDelegate GetParseMethod(Type type) => v => GetParseStringSegmentMethod(type)(new StringSegment(v));

        public static ParseStringSegmentDelegate GetParseStringSegmentMethod(Type type)
        {
            var collectionInterface = type.GetTypeWithGenericInterfaceOf(typeof(ICollection<>));
            if (collectionInterface == null)
                throw new ArgumentException(string.Format("Type {0} is not of type ICollection<>", type.FullName));

            //optimized access for regularly used types
            if (type.HasInterface(typeof(ICollection<string>)))
                return value => ParseStringCollection(value, type);

            if (type.HasInterface(typeof(ICollection<int>)))
                return value => ParseIntCollection(value, type);

            var elementType = collectionInterface.GetGenericArguments()[0];
            var supportedTypeParseMethod = Serializer.GetParseStringSegmentFn(elementType);
            if (supportedTypeParseMethod != null)
            {
                var createCollectionType = type.HasAnyTypeDefinitionsOf(typeof(ICollection<>))
                    ? null : type;

                return value => ParseCollectionType(value, createCollectionType, elementType, supportedTypeParseMethod);
            }

            return null;
        }

        public static ICollection<string> ParseStringCollection(string value, Type createType) => ParseStringCollection(
            new StringSegment(value), createType);

        public static ICollection<string> ParseStringCollection(StringSegment value, Type createType)
        {
            var items = DeserializeArrayWithElements<string, TSerializer>.ParseGenericArray(value, Serializer.ParseString);
            return CollectionExtensions.CreateAndPopulate(createType, items);
        }

        public static ICollection<int> ParseIntCollection(string value, Type createType) => ParseIntCollection(
            new StringSegment(value), createType);

        public static ICollection<int> ParseIntCollection(StringSegment value, Type createType)
        {
            var items = DeserializeArrayWithElements<int, TSerializer>.ParseGenericArray(value, x => int.Parse(x.Value));
            return CollectionExtensions.CreateAndPopulate(createType, items);
        }

        public static ICollection<T> ParseCollection<T>(string value, Type createType, ParseStringDelegate parseFn) =>
            ParseCollection<T>(new StringSegment(value), createType, v => parseFn(v.Value));

        public static ICollection<T> ParseCollection<T>(StringSegment value, Type createType, ParseStringSegmentDelegate parseFn)
        {
            if (!value.HasValue) return null;

            var items = DeserializeArrayWithElements<T, TSerializer>.ParseGenericArray(value, parseFn);
            return CollectionExtensions.CreateAndPopulate(createType, items);
        }

        private static Dictionary<Type, ParseCollectionDelegate> ParseDelegateCache
            = new Dictionary<Type, ParseCollectionDelegate>();

        private delegate object ParseCollectionDelegate(StringSegment value, Type createType, ParseStringSegmentDelegate parseFn);

        public static object ParseCollectionType(string value, Type createType, Type elementType, ParseStringDelegate parseFn) =>
            ParseCollectionType(new StringSegment(value), createType, elementType, v => parseFn(v.Value));


        public static object ParseCollectionType(StringSegment value, Type createType, Type elementType, ParseStringSegmentDelegate parseFn)
        {
            if (ParseDelegateCache.TryGetValue(elementType, out var parseDelegate))
                return parseDelegate(value, createType, parseFn);

            var mi = typeof(DeserializeCollection<TSerializer>).GetStaticMethod("ParseCollection", 
                new[] { typeof (StringSegment), typeof(Type), typeof(ParseStringSegmentDelegate)});
            var genericMi = mi.MakeGenericMethod(new[] { elementType });
            parseDelegate = (ParseCollectionDelegate)genericMi.MakeDelegate(typeof(ParseCollectionDelegate));

            Dictionary<Type, ParseCollectionDelegate> snapshot, newCache;
            do
            {
                snapshot = ParseDelegateCache;
                newCache = new Dictionary<Type, ParseCollectionDelegate>(ParseDelegateCache);
                newCache[elementType] = parseDelegate;

            } while (!ReferenceEquals(
                Interlocked.CompareExchange(ref ParseDelegateCache, newCache, snapshot), snapshot));

            return parseDelegate(value, createType, parseFn);
        }
    }
}