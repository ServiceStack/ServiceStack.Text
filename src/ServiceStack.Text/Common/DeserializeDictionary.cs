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
using System.Text;
using System.Threading;
using System.Linq;
using ServiceStack.Text.Json;
using ServiceStack.Text.Pools;
using ServiceStack.Text.Support;

#if NETSTANDARD2_0
using Microsoft.Extensions.Primitives;
#endif

namespace ServiceStack.Text.Common
{
    public static class DeserializeDictionary<TSerializer>
        where TSerializer : ITypeSerializer
    {
        private static readonly ITypeSerializer Serializer = JsWriter.GetTypeSerializer<TSerializer>();

        const int KeyIndex = 0;
        const int ValueIndex = 1;

        public static ParseStringDelegate GetParseMethod(Type type) => v => GetParseStringSegmentMethod(type)(new StringSegment(v));

        public static ParseStringSegmentDelegate GetParseStringSegmentMethod(Type type)
        {
            var mapInterface = type.GetTypeWithGenericInterfaceOf(typeof(IDictionary<,>));
            if (mapInterface == null)
            {
                var fn = PclExport.Instance.GetDictionaryParseStringSegmentMethod<TSerializer>(type);
                if (fn != null)
                    return fn;

                if (type == typeof(IDictionary))
                {
                    return GetParseStringSegmentMethod(typeof(Dictionary<object, object>));
                }
                if (typeof(IDictionary).IsAssignableFrom(type))
                {
                    return s => ParseIDictionary(s, type);
                }

                throw new ArgumentException(string.Format("Type {0} is not of type IDictionary<,>", type.FullName));
            }

            //optimized access for regularly used types
            if (type == typeof(Dictionary<string, string>))
            {
                return ParseStringDictionary;
            }
            if (type == typeof(JsonObject))
            {
                return ParseJsonObject;
            }

            var dictionaryArgs = mapInterface.GetGenericArguments();
            var keyTypeParseMethod = Serializer.GetParseStringSegmentFn(dictionaryArgs[KeyIndex]);
            if (keyTypeParseMethod == null) return null;

            var valueTypeParseMethod = Serializer.GetParseStringSegmentFn(dictionaryArgs[ValueIndex]);
            if (valueTypeParseMethod == null) return null;

            var createMapType = type.HasAnyTypeDefinitionsOf(typeof(Dictionary<,>), typeof(IDictionary<,>))
                ? null : type;

            return value => ParseDictionaryType(value, createMapType, dictionaryArgs, keyTypeParseMethod, valueTypeParseMethod);
        }

        public static JsonObject ParseJsonObject(string value) => ParseJsonObject(new StringSegment(value));

        public static JsonObject ParseJsonObject(StringSegment value)
        {
            if (value.Length == 0)
                return null;

            var index = VerifyAndGetStartIndex(value, typeof(JsonObject));

            var result = new JsonObject();

            if (JsonTypeSerializer.IsEmptyMap(value, index)) return result;

            var valueLength = value.Length;
            while (index < valueLength)
            {
                var keyValue = Serializer.EatMapKey(value, ref index);
                Serializer.EatMapKeySeperator(value, ref index);
                var elementValue = Serializer.EatValue(value, ref index);
                if (!keyValue.HasValue) continue;

                var mapKey = keyValue.Value;
                var mapValue = elementValue.Value;

                result[mapKey] = mapValue;

                Serializer.EatItemSeperatorOrMapEndChar(value, ref index);
            }

            return result;
        }

        public static Dictionary<string, string> ParseStringDictionary(string value) => ParseStringDictionary(new StringSegment(value));

        public static Dictionary<string, string> ParseStringDictionary(StringSegment value)
        {
            if (!value.HasValue)
                return null;

            var index = VerifyAndGetStartIndex(value, typeof(Dictionary<string, string>));

            var result = new Dictionary<string, string>();

            if (JsonTypeSerializer.IsEmptyMap(value, index)) return result;

            var valueLength = value.Length;
            while (index < valueLength)
            {
                var keyValue = Serializer.EatMapKey(value, ref index);
                Serializer.EatMapKeySeperator(value, ref index);
                var elementValue = Serializer.EatValue(value, ref index);
                if (!keyValue.HasValue) continue;

                var mapKey = Serializer.UnescapeString(keyValue);
                var mapValue = Serializer.UnescapeString(elementValue);

                result[mapKey.Value] = mapValue.Value;

                Serializer.EatItemSeperatorOrMapEndChar(value, ref index);
            }

            return result;
        }

        public static IDictionary ParseIDictionary(string value, Type dictType) => ParseIDictionary(new StringSegment(value), dictType);

        public static IDictionary ParseIDictionary(StringSegment value, Type dictType)
        {
            if (!value.HasValue) return null;

            var index = VerifyAndGetStartIndex(value, dictType);

            var valueParseMethod = Serializer.GetParseStringSegmentFn(typeof(object));
            if (valueParseMethod == null) return null;

            var to = (IDictionary)dictType.CreateInstance();

            if (JsonTypeSerializer.IsEmptyMap(value, index)) return to;

            var valueLength = value.Length;
            while (index < valueLength)
            {
                var keyValue = Serializer.EatMapKey(value, ref index);
                Serializer.EatMapKeySeperator(value, ref index);
                var elementStartIndex = index;
                var elementValue = Serializer.EatTypeValue(value, ref index);
                if (!keyValue.HasValue) continue;

                var mapKey = valueParseMethod(keyValue);

                if (elementStartIndex < valueLength)
                {
                    Serializer.EatWhitespace(value, ref elementStartIndex);
                    to[mapKey] = DeserializeType<TSerializer>.ParsePrimitive(elementValue.Value, value.GetChar(elementStartIndex));
                }
                else
                {
                    to[mapKey] = valueParseMethod(elementValue);
                }

                Serializer.EatItemSeperatorOrMapEndChar(value, ref index);
            }

            return to;
        }

        public static IDictionary<TKey, TValue> ParseDictionary<TKey, TValue>(
            string value, Type createMapType,
            ParseStringDelegate parseKeyFn, ParseStringDelegate parseValueFn)
        {
            return ParseDictionary<TKey, TValue>(new StringSegment(value),
                createMapType,
                v => parseKeyFn(v.Value),
                v => parseValueFn(v.Value)
                );
        }


        public static IDictionary<TKey, TValue> ParseDictionary<TKey, TValue>(
            StringSegment value, Type createMapType,
            ParseStringSegmentDelegate parseKeyFn, ParseStringSegmentDelegate parseValueFn)
        {
            if (!value.HasValue) return null;

            var tryToParseItemsAsDictionaries =
                JsConfig.ConvertObjectTypesIntoStringDictionary && typeof(TValue) == typeof(object);
            var tryToParseItemsAsPrimitiveTypes =
                JsConfig.TryToParsePrimitiveTypeValues && typeof(TValue) == typeof(object);

            var index = VerifyAndGetStartIndex(value, createMapType);

            var to = (createMapType == null)
                ? new Dictionary<TKey, TValue>()
                : (IDictionary<TKey, TValue>)createMapType.CreateInstance();

            if (JsonTypeSerializer.IsEmptyMap(value, index)) return to;

            var valueLength = value.Length;
            while (index < valueLength)
            {
                var keyValue = Serializer.EatMapKey(value, ref index);
                Serializer.EatMapKeySeperator(value, ref index);
                var elementStartIndex = index;
                var elementValue = Serializer.EatTypeValue(value, ref index);
                if (!keyValue.HasValue) continue;

                TKey mapKey = (TKey)parseKeyFn(keyValue);

                if (tryToParseItemsAsDictionaries)
                {
                    Serializer.EatWhitespace(value, ref elementStartIndex);
                    if (elementStartIndex < valueLength && value.GetChar(elementStartIndex) == JsWriter.MapStartChar)
                    {
                        var tmpMap = ParseDictionary<TKey, TValue>(elementValue, createMapType, parseKeyFn, parseValueFn);
                        if (tmpMap != null && tmpMap.Count > 0)
                        {
                            to[mapKey] = (TValue)tmpMap;
                        }
                    }
                    else if (elementStartIndex < valueLength && value.GetChar(elementStartIndex) == JsWriter.ListStartChar)
                    {
                        to[mapKey] = (TValue)DeserializeList<List<object>, TSerializer>.ParseStringSegment(elementValue);
                    }
                    else
                    {
                        to[mapKey] = (TValue)(tryToParseItemsAsPrimitiveTypes && elementStartIndex < valueLength
                                        ? DeserializeType<TSerializer>.ParsePrimitive(elementValue.Value, value.GetChar(elementStartIndex))
                                        : parseValueFn(elementValue));
                    }
                }
                else
                {
                    if (tryToParseItemsAsPrimitiveTypes && elementStartIndex < valueLength)
                    {
                        Serializer.EatWhitespace(value, ref elementStartIndex);
                        to[mapKey] = (TValue)DeserializeType<TSerializer>.ParsePrimitive(elementValue.Value, value.GetChar(elementStartIndex));
                    }
                    else
                    {
                        to[mapKey] = (TValue)parseValueFn(elementValue);
                    }
                }

                Serializer.EatItemSeperatorOrMapEndChar(value, ref index);
            }

            return to;
        }

        private static int VerifyAndGetStartIndex(StringSegment value, Type createMapType)
        {
            var index = 0;
            if (value.Length > 0 && !Serializer.EatMapStartChar(value, ref index))
            {
                //Don't throw ex because some KeyValueDataContractDeserializer don't have '{}'
                Tracer.Instance.WriteDebug("WARN: Map definitions should start with a '{0}', expecting serialized type '{1}', got string starting with: {2}",
                    JsWriter.MapStartChar, createMapType != null ? createMapType.Name : "Dictionary<,>", value.Substring(0, value.Length < 50 ? value.Length : 50));
            }
            return index;
        }

        private static Dictionary<TypesKey, ParseDictionaryDelegate> ParseDelegateCache
            = new Dictionary<TypesKey, ParseDictionaryDelegate>();

        private delegate object ParseDictionaryDelegate(StringSegment value, Type createMapType,
            ParseStringSegmentDelegate keyParseFn, ParseStringSegmentDelegate valueParseFn);

        public static object ParseDictionaryType(string value, Type createMapType, Type[] argTypes,
            ParseStringDelegate keyParseFn, ParseStringDelegate valueParseFn) =>
            ParseDictionaryType(new StringSegment(value), createMapType, argTypes,
                v => keyParseFn(v.Value), v => valueParseFn(v.Value));

        static readonly Type[] signature = {typeof(StringSegment), typeof(Type), typeof(ParseStringSegmentDelegate), typeof(ParseStringSegmentDelegate)};

        public static object ParseDictionaryType(StringSegment value, Type createMapType, Type[] argTypes,
            ParseStringSegmentDelegate keyParseFn, ParseStringSegmentDelegate valueParseFn)
        {

            ParseDictionaryDelegate parseDelegate;
            var key = new TypesKey(argTypes[0], argTypes[1]);
            if (ParseDelegateCache.TryGetValue(key, out parseDelegate))
                return parseDelegate(value, createMapType, keyParseFn, valueParseFn);

            var mi = typeof(DeserializeDictionary<TSerializer>).GetStaticMethod("ParseDictionary", signature);
            var genericMi = mi.MakeGenericMethod(argTypes);
            parseDelegate = (ParseDictionaryDelegate)genericMi.MakeDelegate(typeof(ParseDictionaryDelegate));

            Dictionary<TypesKey, ParseDictionaryDelegate> snapshot, newCache;
            do
            {
                snapshot = ParseDelegateCache;
                newCache = new Dictionary<TypesKey, ParseDictionaryDelegate>(ParseDelegateCache);
                newCache[key] = parseDelegate;

            } while (!ReferenceEquals(
                Interlocked.CompareExchange(ref ParseDelegateCache, newCache, snapshot), snapshot));

            return parseDelegate(value, createMapType, keyParseFn, valueParseFn);
        }

        struct TypesKey
        {
            Type Type1 { get; }
            Type Type2 { get; }

            readonly int hashcode;

            public TypesKey(Type type1, Type type2)
            {
                Type1 = type1;
                Type2 = type2;
                unchecked
                {
                    hashcode = Type1.GetHashCode() ^ (37 * Type2.GetHashCode());
                }
            }

            public override bool Equals(object obj)
            {
                var types = (TypesKey)obj;

                return Type1 == types.Type1 && Type2 == types.Type2;
            }

            public override int GetHashCode() => hashcode;
        }
    }
}