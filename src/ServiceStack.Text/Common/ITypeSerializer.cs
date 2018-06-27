using System;
using System.IO;
using ServiceStack.Text.Json;

namespace ServiceStack.Text.Common
{
    public delegate object ObjectDeserializerDelegate(ReadOnlySpan<char> value);
    
    public abstract class ITypeSerializer
    {
        public abstract ObjectDeserializerDelegate ObjectDeserializer { get; set; }

        public abstract bool IncludeNullValues { get; }
        public abstract bool IncludeNullValuesInDictionaries { get; }
        public abstract string TypeAttrInObject { get; }

        public abstract WriteObjectDelegate GetWriteFn<T>();
        public abstract WriteObjectDelegate GetWriteFn(Type type);
        public abstract TypeInfo GetTypeInfo(Type type);

        public abstract void WriteRawString(TextWriter writer, string value);
        public abstract void WritePropertyName(TextWriter writer, string value);

        public abstract void WriteBuiltIn(TextWriter writer, object value);
        public abstract void WriteObjectString(TextWriter writer, object value);
        public abstract void WriteException(TextWriter writer, object value);
        public abstract void WriteString(TextWriter writer, string value);
        public abstract void WriteFormattableObjectString(TextWriter writer, object value);
        public abstract void WriteDateTime(TextWriter writer, object oDateTime);
        public abstract void WriteNullableDateTime(TextWriter writer, object dateTime);
        public abstract void WriteDateTimeOffset(TextWriter writer, object oDateTimeOffset);
        public abstract void WriteNullableDateTimeOffset(TextWriter writer, object dateTimeOffset);
        public abstract void WriteTimeSpan(TextWriter writer, object dateTimeOffset);
        public abstract void WriteNullableTimeSpan(TextWriter writer, object dateTimeOffset);
        public abstract void WriteGuid(TextWriter writer, object oValue);
        public abstract void WriteNullableGuid(TextWriter writer, object oValue);
        public abstract void WriteBytes(TextWriter writer, object oByteValue);
        public abstract void WriteChar(TextWriter writer, object charValue);
        public abstract void WriteByte(TextWriter writer, object byteValue);
        public abstract void WriteSByte(TextWriter writer, object sbyteValue);
        public abstract void WriteInt16(TextWriter writer, object intValue);
        public abstract void WriteUInt16(TextWriter writer, object intValue);
        public abstract void WriteInt32(TextWriter writer, object intValue);
        public abstract void WriteUInt32(TextWriter writer, object uintValue);
        public abstract void WriteInt64(TextWriter writer, object longValue);
        public abstract void WriteUInt64(TextWriter writer, object ulongValue);
        public abstract void WriteBool(TextWriter writer, object boolValue);
        public abstract void WriteFloat(TextWriter writer, object floatValue);
        public abstract void WriteDouble(TextWriter writer, object doubleValue);
        public abstract void WriteDecimal(TextWriter writer, object decimalValue);
        public abstract void WriteEnum(TextWriter writer, object enumValue);
        public abstract void WriteEnumFlags(TextWriter writer, object enumFlagValue);
        public abstract void WriteEnumMember(TextWriter writer, object enumFlagValue);

        //object EncodeMapKey(object value);

        public abstract ParseStringDelegate GetParseFn<T>();
        public abstract ParseStringSpanDelegate GetParseStringSegmentFn<T>();
        public abstract ParseStringDelegate GetParseFn(Type type);
        public abstract ParseStringSpanDelegate GetParseStringSegmentFn(Type type);

        public abstract string ParseRawString(string value);
        public abstract string ParseString(string value);
        public abstract string ParseString(ReadOnlySpan<char> value);
        public abstract string UnescapeString(string value);
        public abstract ReadOnlySpan<char> UnescapeString(ReadOnlySpan<char> value);
        public abstract string UnescapeSafeString(string value);
        public abstract ReadOnlySpan<char> UnescapeSafeString(ReadOnlySpan<char> value);
        public abstract string EatTypeValue(string value, ref int i);
        public abstract ReadOnlySpan<char> EatTypeValue(ReadOnlySpan<char> value, ref int i);
        public abstract bool EatMapStartChar(string value, ref int i);
        public abstract bool EatMapStartChar(ReadOnlySpan<char> value, ref int i);
        public abstract string EatMapKey(string value, ref int i);
        public abstract ReadOnlySpan<char> EatMapKey(ReadOnlySpan<char> value, ref int i);
        public abstract bool EatMapKeySeperator(string value, ref int i);
        public abstract bool EatMapKeySeperator(ReadOnlySpan<char> value, ref int i);
        public abstract void EatWhitespace(string value, ref int i);
        public abstract void EatWhitespace(ReadOnlySpan<char> value, ref int i);
        public abstract string EatValue(string value, ref int i);
        public abstract ReadOnlySpan<char> EatValue(ReadOnlySpan<char> value, ref int i);
        public abstract bool EatItemSeperatorOrMapEndChar(string value, ref int i);
        public abstract bool EatItemSeperatorOrMapEndChar(ReadOnlySpan<char> value, ref int i);
    }
}