using System;
using System.IO;
using ServiceStack.Text.Json;
#if NETSTANDARD2_0
using Microsoft.Extensions.Primitives;
#else
using ServiceStack.Text.Support;
#endif


namespace ServiceStack.Text.Common
{
    public interface ITypeSerializer
    {
        Func<StringSegment, object> ObjectDeserializer { get; set; }

        bool IncludeNullValues { get; }
        bool IncludeNullValuesInDictionaries { get; }
        string TypeAttrInObject { get; }

        WriteObjectDelegate GetWriteFn<T>();
        WriteObjectDelegate GetWriteFn(Type type);
        TypeInfo GetTypeInfo(Type type);

        void WriteRawString(TextWriter writer, string value);
        void WritePropertyName(TextWriter writer, string value);

        void WriteBuiltIn(TextWriter writer, object value);
        void WriteObjectString(TextWriter writer, object value);
        void WriteException(TextWriter writer, object value);
        void WriteString(TextWriter writer, string value);
        void WriteFormattableObjectString(TextWriter writer, object value);
        void WriteDateTime(TextWriter writer, object oDateTime);
        void WriteNullableDateTime(TextWriter writer, object dateTime);
        void WriteDateTimeOffset(TextWriter writer, object oDateTimeOffset);
        void WriteNullableDateTimeOffset(TextWriter writer, object dateTimeOffset);
        void WriteTimeSpan(TextWriter writer, object dateTimeOffset);
        void WriteNullableTimeSpan(TextWriter writer, object dateTimeOffset);
        void WriteGuid(TextWriter writer, object oValue);
        void WriteNullableGuid(TextWriter writer, object oValue);
        void WriteBytes(TextWriter writer, object oByteValue);
        void WriteChar(TextWriter writer, object charValue);
        void WriteByte(TextWriter writer, object byteValue);
        void WriteSByte(TextWriter writer, object sbyteValue);
        void WriteInt16(TextWriter writer, object intValue);
        void WriteUInt16(TextWriter writer, object intValue);
        void WriteInt32(TextWriter writer, object intValue);
        void WriteUInt32(TextWriter writer, object uintValue);
        void WriteInt64(TextWriter writer, object longValue);
        void WriteUInt64(TextWriter writer, object ulongValue);
        void WriteBool(TextWriter writer, object boolValue);
        void WriteFloat(TextWriter writer, object floatValue);
        void WriteDouble(TextWriter writer, object doubleValue);
        void WriteDecimal(TextWriter writer, object decimalValue);
        void WriteEnum(TextWriter writer, object enumValue);
        void WriteEnumFlags(TextWriter writer, object enumFlagValue);
        void WriteEnumMember(TextWriter writer, object enumFlagValue);

        //object EncodeMapKey(object value);

        ParseStringDelegate GetParseFn<T>();
        ParseStringSegmentDelegate GetParseStringSegmentFn<T>();
        ParseStringDelegate GetParseFn(Type type);
        ParseStringSegmentDelegate GetParseStringSegmentFn(Type type);

        string ParseRawString(string value);
        string ParseString(string value);
        string ParseString(StringSegment value);
        string UnescapeString(string value);
        StringSegment UnescapeString(StringSegment value);
        string UnescapeSafeString(string value);
        StringSegment UnescapeSafeString(StringSegment value);
        string EatTypeValue(string value, ref int i);
        StringSegment EatTypeValue(StringSegment value, ref int i);
        bool EatMapStartChar(string value, ref int i);
        bool EatMapStartChar(StringSegment value, ref int i);
        string EatMapKey(string value, ref int i);
        StringSegment EatMapKey(StringSegment value, ref int i);
        bool EatMapKeySeperator(string value, ref int i);
        bool EatMapKeySeperator(StringSegment value, ref int i);
        void EatWhitespace(string value, ref int i);
        void EatWhitespace(StringSegment value, ref int i);
        string EatValue(string value, ref int i);
        StringSegment EatValue(StringSegment value, ref int i);
        bool EatItemSeperatorOrMapEndChar(string value, ref int i);
        bool EatItemSeperatorOrMapEndChar(StringSegment value, ref int i);
    }
}