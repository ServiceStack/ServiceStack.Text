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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using ServiceStack.Text.Json;
using ServiceStack.Text.Pools;
#if NETSTANDARD2_0
using Microsoft.Extensions.Primitives;
#endif
using ServiceStack.Text.Support;


namespace ServiceStack.Text.Common
{
    internal static class DeserializeKeyValuePair<TSerializer>
        where TSerializer : ITypeSerializer
    {
        private static readonly ITypeSerializer Serializer = JsWriter.GetTypeSerializer<TSerializer>();

        const int KeyIndex = 0;
        const int ValueIndex = 1;

        public static ParseStringDelegate GetParseMethod(Type type) => v => GetParseStringSegmentMethod(type)(new StringSegment(v));

        public static ParseStringSegmentDelegate GetParseStringSegmentMethod(Type type)
        {
            var mapInterface = type.GetTypeWithGenericInterfaceOf(typeof(KeyValuePair<,>));

            var keyValuePairArgs = mapInterface.GetGenericArguments();
            var keyTypeParseMethod = Serializer.GetParseStringSegmentFn(keyValuePairArgs[KeyIndex]);
            if (keyTypeParseMethod == null) return null;

            var valueTypeParseMethod = Serializer.GetParseStringSegmentFn(keyValuePairArgs[ValueIndex]);
            if (valueTypeParseMethod == null) return null;

            var createMapType = type.HasAnyTypeDefinitionsOf(typeof(KeyValuePair<,>))
                ? null : type;

            return value => ParseKeyValuePairType(value, createMapType, keyValuePairArgs, keyTypeParseMethod, valueTypeParseMethod);
        }

        public static object ParseKeyValuePair<TKey, TValue>(
            string value, Type createMapType,
            ParseStringDelegate parseKeyFn, ParseStringDelegate parseValueFn) =>
            ParseKeyValuePair<TKey, TValue>(new StringSegment(value), createMapType,
                v => parseKeyFn(v.Value), v => parseValueFn(v.Value));

        public static object ParseKeyValuePair<TKey, TValue>(
            StringSegment value, Type createMapType,
            ParseStringSegmentDelegate parseKeyFn, ParseStringSegmentDelegate parseValueFn)
        {
            if (!value.HasValue) return default(KeyValuePair<TKey, TValue>);

            var index = VerifyAndGetStartIndex(value, createMapType);

            if (JsonTypeSerializer.IsEmptyMap(value, index)) return new KeyValuePair<TKey, TValue>();
            var keyValue = default(TKey);
            var valueValue = default(TValue);

            var valueLength = value.Length;
            while (index < valueLength)
            {
                var key = Serializer.EatMapKey(value, ref index);
                Serializer.EatMapKeySeperator(value, ref index);
                var keyElementValue = Serializer.EatTypeValue(value, ref index);

                if (key.CompareIgnoreCase("key"))
                    keyValue = (TKey)parseKeyFn(keyElementValue);
                else if (key.CompareIgnoreCase("value"))
                    valueValue = (TValue)parseValueFn(keyElementValue);
                else
                    throw new SerializationException("Incorrect KeyValuePair property: " + key);
                Serializer.EatItemSeperatorOrMapEndChar(value, ref index);
            }

            return new KeyValuePair<TKey, TValue>(keyValue, valueValue);
        }

        private static int VerifyAndGetStartIndex(StringSegment value, Type createMapType)
        {
            var index = 0;
            if (!Serializer.EatMapStartChar(value, ref index))
            {
                //Don't throw ex because some KeyValueDataContractDeserializer don't have '{}'
                Tracer.Instance.WriteDebug("WARN: Map definitions should start with a '{0}', expecting serialized type '{1}', got string starting with: {2}",
                                           JsWriter.MapStartChar, createMapType != null ? createMapType.Name : "Dictionary<,>", value.Substring(0, value.Length < 50 ? value.Length : 50));
            }
            return index;
        }

        private static Dictionary<string, ParseKeyValuePairDelegate> ParseDelegateCache
            = new Dictionary<string, ParseKeyValuePairDelegate>();

        private delegate object ParseKeyValuePairDelegate(StringSegment value, Type createMapType,
            ParseStringSegmentDelegate keyParseFn, ParseStringSegmentDelegate valueParseFn);

        public static object ParseKeyValuePairType(string value, Type createMapType, Type[] argTypes,
            ParseStringDelegate keyParseFn, ParseStringDelegate valueParseFn) =>
            ParseKeyValuePairType(new StringSegment(value), createMapType, argTypes,
                v => keyParseFn(v.Value), v => valueParseFn(v.Value));

        static readonly Type[] signature = { typeof(StringSegment), typeof(Type), typeof(ParseStringSegmentDelegate), typeof(ParseStringSegmentDelegate) };

        public static object ParseKeyValuePairType(StringSegment value, Type createMapType, Type[] argTypes,
            ParseStringSegmentDelegate keyParseFn, ParseStringSegmentDelegate valueParseFn)
        {

            ParseKeyValuePairDelegate parseDelegate;
            var key = GetTypesKey(argTypes);
            if (ParseDelegateCache.TryGetValue(key, out parseDelegate))
                return parseDelegate(value, createMapType, keyParseFn, valueParseFn);

            var mi = typeof(DeserializeKeyValuePair<TSerializer>).GetStaticMethod("ParseKeyValuePair", signature);
            var genericMi = mi.MakeGenericMethod(argTypes);
            parseDelegate = (ParseKeyValuePairDelegate)genericMi.MakeDelegate(typeof(ParseKeyValuePairDelegate));

            Dictionary<string, ParseKeyValuePairDelegate> snapshot, newCache;
            do
            {
                snapshot = ParseDelegateCache;
                newCache = new Dictionary<string, ParseKeyValuePairDelegate>(ParseDelegateCache);
                newCache[key] = parseDelegate;

            } while (!ReferenceEquals(
                Interlocked.CompareExchange(ref ParseDelegateCache, newCache, snapshot), snapshot));

            return parseDelegate(value, createMapType, keyParseFn, valueParseFn);
        }

        private static string GetTypesKey(params Type[] types)
        {
            var sb = StringBuilderThreadStatic.Allocate();
            foreach (var type in types)
            {
                if (sb.Length > 0)
                    sb.Append(">");

                sb.Append(type.FullName);
            }
            return StringBuilderThreadStatic.ReturnAndFree(sb);
        }
    }
}