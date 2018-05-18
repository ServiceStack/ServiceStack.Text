﻿using System;
using System.Linq;
using ServiceStack.Text.Json;
#if NETSTANDARD2_0
using Microsoft.Extensions.Primitives;
#endif
using ServiceStack.Text.Support;

namespace ServiceStack.Text.Common
{
    internal static class DeserializeCustomGenericType<TSerializer>
        where TSerializer : ITypeSerializer
    {
        private static readonly ITypeSerializer Serializer = JsWriter.GetTypeSerializer<TSerializer>();

        public static ParseStringDelegate GetParseMethod(Type type) => v => GetParseStringSegmentMethod(type)(new StringSegment(v));

        public static ParseStringSegmentDelegate GetParseStringSegmentMethod(Type type)
        {
            if (type.Name.IndexOf("Tuple`", StringComparison.Ordinal) >= 0)
                return x => ParseTuple(type, x);

            return null;
        }

        public static object ParseTuple(Type tupleType, string value) => ParseTuple(tupleType, new StringSegment(value));

        public static object ParseTuple(Type tupleType, StringSegment value)
        {
            var index = 0;
            Serializer.EatMapStartChar(value, ref index);
            if (JsonTypeSerializer.IsEmptyMap(value, index))
                return tupleType.CreateInstance();

            var genericArgs = tupleType.GetGenericArguments();
            var argValues = new object[genericArgs.Length];
            var valueLength = value.Length;
            while (index < valueLength)
            {
                var keyValue = Serializer.EatMapKey(value, ref index);
                Serializer.EatMapKeySeperator(value, ref index);
                var elementValue = Serializer.EatValue(value, ref index);
                if (!keyValue.HasValue) continue;

                var keyIndex = keyValue.Substring("Item".Length).ToInt() - 1;
                var parseFn = Serializer.GetParseStringSegmentFn(genericArgs[keyIndex]);
                argValues[keyIndex] = parseFn(elementValue);

                Serializer.EatItemSeperatorOrMapEndChar(value, ref index);
            }

            var ctor = tupleType.GetConstructors()
                .First(x => x.GetParameters().Length == genericArgs.Length);
            return ctor.Invoke(argValues);
        }
    }
}