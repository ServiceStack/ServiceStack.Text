using System;
using System.Linq;
using ServiceStack.Text.Json;

namespace ServiceStack.Text.Common
{
    internal static class DeserializeCustomGenericType<TSerializer>
        where TSerializer : ITypeSerializer
    {
        private static readonly ITypeSerializer Serializer = JsWriter.GetTypeSerializer<TSerializer>();

        public static ParseStringDelegate GetParseMethod(Type type) => v => GetParseStringSegmentMethod(type)(v.AsSpan());

        public static ParseStringSpanDelegate GetParseStringSegmentMethod(Type type)
        {
            if (type.Name.IndexOf("Tuple`", StringComparison.Ordinal) >= 0)
                return x => ParseTuple(type, x);

            return null;
        }

        public static object ParseTuple(Type tupleType, string value) => ParseTuple(tupleType, value.AsSpan());

        public static object ParseTuple(Type tupleType, ReadOnlySpan<char> value)
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
                if (keyValue.IsEmpty) continue;

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