//Copyright (c) ServiceStack, Inc. All Rights Reserved.
//License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using ServiceStack.Text.Common;
using ServiceStack.Text.Json;

namespace ServiceStack.Text.Jsv
{
    public sealed class JsvTypeSerializer
        : ITypeSerializer
    {
        public static ITypeSerializer Instance = new JsvTypeSerializer();

        public override ObjectDeserializerDelegate ObjectDeserializer { get; set; }

        public override bool IncludeNullValues => false;

        public override bool IncludeNullValuesInDictionaries => false;

        public override string TypeAttrInObject => JsConfig.JsvTypeAttrInObject;

        internal static string GetTypeAttrInObject(string typeAttr) => $"{{{typeAttr}:";

        public override WriteObjectDelegate GetWriteFn<T>() => JsvWriter<T>.WriteFn();

        public override WriteObjectDelegate GetWriteFn(Type type) => JsvWriter.GetWriteFn(type);

        static readonly TypeInfo DefaultTypeInfo = new TypeInfo { EncodeMapKey = false };

        public override TypeInfo GetTypeInfo(Type type) => DefaultTypeInfo;

        public override void WriteRawString(TextWriter writer, string value)
        {
            writer.Write(value.EncodeJsv());
        }

        public override void WritePropertyName(TextWriter writer, string value)
        {
            writer.Write(value);
        }

        public override void WriteBuiltIn(TextWriter writer, object value)
        {
            writer.Write(value);
        }

        public override void WriteObjectString(TextWriter writer, object value)
        {
            if (value != null)
            {
                if (value is string strValue)
                {
                    WriteString(writer, strValue);
                }
                else
                {
                    writer.Write(value.ToString().EncodeJsv());
                }
            }
        }

        public override void WriteException(TextWriter writer, object value)
        {
            writer.Write(((Exception)value).Message.EncodeJsv());
        }

        public override void WriteString(TextWriter writer, string value)
        {
            if (JsState.QueryStringMode && !string.IsNullOrEmpty(value) && value.StartsWith(JsWriter.QuoteString) && value.EndsWith(JsWriter.QuoteString))
                value = String.Concat(JsWriter.QuoteChar, value, JsWriter.QuoteChar);
            else if (JsState.QueryStringMode && !string.IsNullOrEmpty(value) && value.Contains(JsWriter.ItemSeperatorString))
                value = String.Concat(JsWriter.QuoteChar, value, JsWriter.QuoteChar);

            writer.Write(value == "" ? "\"\"" : value.EncodeJsv());
        }

        public override void WriteFormattableObjectString(TextWriter writer, object value)
        {
            var f = (IFormattable)value;
            writer.Write(f.ToString(null, CultureInfo.InvariantCulture).EncodeJsv());
        }

        public override void WriteDateTime(TextWriter writer, object oDateTime)
        {
            var dateTime = (DateTime)oDateTime;
            switch (JsConfig.DateHandler)
            {
                case DateHandler.UnixTime:
                    writer.Write(dateTime.ToUnixTime());
                    return;
                case DateHandler.UnixTimeMs:
                    writer.Write(dateTime.ToUnixTimeMs());
                    return;
            }

            writer.Write(DateTimeSerializer.ToShortestXsdDateTimeString((DateTime)oDateTime));
        }

        public override void WriteNullableDateTime(TextWriter writer, object dateTime)
        {
            if (dateTime == null) return;
            WriteDateTime(writer, dateTime);
        }

        public override void WriteDateTimeOffset(TextWriter writer, object oDateTimeOffset)
        {
            writer.Write(((DateTimeOffset)oDateTimeOffset).ToString("o"));
        }

        public override void WriteNullableDateTimeOffset(TextWriter writer, object dateTimeOffset)
        {
            if (dateTimeOffset == null) return;
            this.WriteDateTimeOffset(writer, dateTimeOffset);
        }

        public override void WriteTimeSpan(TextWriter writer, object oTimeSpan)
        {
            writer.Write(DateTimeSerializer.ToXsdTimeSpanString((TimeSpan)oTimeSpan));
        }

        public override void WriteNullableTimeSpan(TextWriter writer, object oTimeSpan)
        {
            if (oTimeSpan == null) return;
            writer.Write(DateTimeSerializer.ToXsdTimeSpanString((TimeSpan?)oTimeSpan));
        }

        public override void WriteGuid(TextWriter writer, object oValue)
        {
            writer.Write(((Guid)oValue).ToString("N"));
        }

        public override void WriteNullableGuid(TextWriter writer, object oValue)
        {
            if (oValue == null) return;
            writer.Write(((Guid)oValue).ToString("N"));
        }

        public override void WriteBytes(TextWriter writer, object oByteValue)
        {
            if (oByteValue == null) return;
            writer.Write(Convert.ToBase64String((byte[])oByteValue));
        }

        public override void WriteChar(TextWriter writer, object charValue)
        {
            if (charValue == null) return;
            writer.Write((char)charValue);
        }

        public override void WriteByte(TextWriter writer, object byteValue)
        {
            if (byteValue == null) return;
            writer.Write((byte)byteValue);
        }

        public override void WriteSByte(TextWriter writer, object sbyteValue)
        {
            if (sbyteValue == null) return;
            writer.Write((sbyte)sbyteValue);
        }

        public override void WriteInt16(TextWriter writer, object intValue)
        {
            if (intValue == null) return;
            writer.Write((short)intValue);
        }

        public override void WriteUInt16(TextWriter writer, object intValue)
        {
            if (intValue == null) return;
            writer.Write((ushort)intValue);
        }

        public override void WriteInt32(TextWriter writer, object intValue)
        {
            if (intValue == null) return;
            writer.Write((int)intValue);
        }

        public override void WriteUInt32(TextWriter writer, object uintValue)
        {
            if (uintValue == null) return;
            writer.Write((uint)uintValue);
        }

        public override void WriteUInt64(TextWriter writer, object ulongValue)
        {
            if (ulongValue == null) return;
            writer.Write((ulong)ulongValue);
        }

        public override void WriteInt64(TextWriter writer, object longValue)
        {
            if (longValue == null) return;
            writer.Write((long)longValue);
        }

        public override void WriteBool(TextWriter writer, object boolValue)
        {
            if (boolValue == null) return;
            writer.Write((bool)boolValue);
        }

        public override void WriteFloat(TextWriter writer, object floatValue)
        {
            if (floatValue == null) return;
            var floatVal = (float)floatValue;
            var cultureInfo = JsState.IsCsv ? CsvConfig.RealNumberCultureInfo : null;

            if (Equals(floatVal, float.MaxValue) || Equals(floatVal, float.MinValue))
                writer.Write(floatVal.ToString("r", cultureInfo ?? CultureInfo.InvariantCulture));
            else
                writer.Write(floatVal.ToString("r", cultureInfo ?? CultureInfo.InvariantCulture));
        }

        public override void WriteDouble(TextWriter writer, object doubleValue)
        {
            if (doubleValue == null) return;
            var doubleVal = (double)doubleValue;
            var cultureInfo = JsState.IsCsv ? CsvConfig.RealNumberCultureInfo : null;

            if (Equals(doubleVal, double.MaxValue) || Equals(doubleVal, double.MinValue))
                writer.Write(doubleVal.ToString("r", cultureInfo ?? CultureInfo.InvariantCulture));
            else
                writer.Write(doubleVal.ToString(cultureInfo ?? CultureInfo.InvariantCulture));
        }

        public override void WriteDecimal(TextWriter writer, object decimalValue)
        {
            if (decimalValue == null) return;
            var cultureInfo = JsState.IsCsv ? CsvConfig.RealNumberCultureInfo : null;

            writer.Write(((decimal)decimalValue).ToString(cultureInfo ?? CultureInfo.InvariantCulture));
        }

        public override void WriteEnum(TextWriter writer, object enumValue)
        {
            if (enumValue == null) return;
            if (JsConfig.TreatEnumAsInteger)
                JsWriter.WriteEnumFlags(writer, enumValue);
            else
                writer.Write(enumValue.ToString());
        }

        public override void WriteEnumFlags(TextWriter writer, object enumFlagValue)
        {
            JsWriter.WriteEnumFlags(writer, enumFlagValue);
        }

        public override void WriteEnumMember(TextWriter writer, object enumValue)
        {
            if (enumValue == null) return;

            var enumType = enumValue.GetType();
            var mi = enumType.GetMember(enumValue.ToString());
            var enumMemberAttr = mi[0].FirstAttribute<EnumMemberAttribute>();
            var useValue = enumMemberAttr?.Value ?? enumValue;
            writer.Write(enumValue.ToString());
        }

        public object EncodeMapKey(object value)
        {
            return value;
        }

        public override ParseStringDelegate GetParseFn<T>() => JsvReader.Instance.GetParseFn<T>();

        public override ParseStringDelegate GetParseFn(Type type) => JsvReader.GetParseFn(type);

        public override ParseStringSpanDelegate GetParseStringSegmentFn<T>() => JsvReader.Instance.GetParseStringSegmentFn<T>();

        public override ParseStringSpanDelegate GetParseStringSegmentFn(Type type) => JsvReader.GetParseStringSegmentFn(type);

        public override string UnescapeSafeString(string value) => value.FromCsvField();

        public override ReadOnlySpan<char> UnescapeSafeString(ReadOnlySpan<char> value) => value.FromCsvField();

        public override string ParseRawString(string value) => value;

        public override string ParseString(string value) => value.FromCsvField();

        public override string ParseString(ReadOnlySpan<char> value) => value.ToString().FromCsvField();

        public override string UnescapeString(string value) => value.FromCsvField();

        public override ReadOnlySpan<char> UnescapeString(ReadOnlySpan<char> value) => value.FromCsvField();

        public override string EatTypeValue(string value, ref int i) => EatValue(value, ref i);

        public override ReadOnlySpan<char> EatTypeValue(ReadOnlySpan<char> value, ref int i) => EatValue(value, ref i);

        public override bool EatMapStartChar(string value, ref int i) => EatMapStartChar(value, ref i);

        public override bool EatMapStartChar(ReadOnlySpan<char> value, ref int i)
        {
            var success = value[i] == JsWriter.MapStartChar;
            if (success) i++;
            return success;
        }

        public override string EatMapKey(string value, ref int i) => EatMapKey(value.AsSpan(), ref i).ToString();

        public override ReadOnlySpan<char> EatMapKey(ReadOnlySpan<char> value, ref int i)
        {
            var tokenStartPos = i;

            var valueLength = value.Length;

            var valueChar = value[tokenStartPos];

            switch (valueChar)
            {
                case JsWriter.QuoteChar:
                    while (++i < valueLength)
                    {
                        valueChar = value[i];

                        if (valueChar != JsWriter.QuoteChar) continue;

                        var isLiteralQuote = i + 1 < valueLength && value[i + 1] == JsWriter.QuoteChar;

                        i++; //skip quote
                        if (!isLiteralQuote)
                            break;
                    }
                    return value.Slice(tokenStartPos, i - tokenStartPos);

                //Is Type/Map, i.e. {...}
                case JsWriter.MapStartChar:
                    var endsToEat = 1;
                    var withinQuotes = false;
                    while (++i < valueLength && endsToEat > 0)
                    {
                        valueChar = value[i];

                        if (valueChar == JsWriter.QuoteChar)
                            withinQuotes = !withinQuotes;

                        if (withinQuotes)
                            continue;

                        if (valueChar == JsWriter.MapStartChar)
                            endsToEat++;

                        if (valueChar == JsWriter.MapEndChar)
                            endsToEat--;
                    }
                    return value.Slice(tokenStartPos, i - tokenStartPos);
            }

            while (value[++i] != JsWriter.MapKeySeperator) { }
            return value.Slice(tokenStartPos, i - tokenStartPos);
        }

        public override bool EatMapKeySeperator(string value, ref int i)
        {
            return value[i++] == JsWriter.MapKeySeperator;
        }

        public override bool EatMapKeySeperator(ReadOnlySpan<char> value, ref int i)
        {
            return value[i++] == JsWriter.MapKeySeperator;
        }

        public override bool EatItemSeperatorOrMapEndChar(string value, ref int i)
        {
            if (i == value.Length) return false;

            var success = value[i] == JsWriter.ItemSeperator
                || value[i] == JsWriter.MapEndChar;
            i++;
            return success;
        }

        public override bool EatItemSeperatorOrMapEndChar(ReadOnlySpan<char> value, ref int i)
        {
            if (i == value.Length) return false;

            var success = value[i] == JsWriter.ItemSeperator
                || value[i] == JsWriter.MapEndChar;
            i++;
            return success;
        }


        public override void EatWhitespace(string value, ref int i) {}

        public override void EatWhitespace(ReadOnlySpan<char> value, ref int i) { }

        public override string EatValue(string value, ref int i)
        {
            return EatValue(value.AsSpan(), ref i).ToString();
        }

        public override ReadOnlySpan<char> EatValue(ReadOnlySpan<char> value, ref int i)
        {
            var tokenStartPos = i;
            var valueLength = value.Length;
            if (i == valueLength) return default(ReadOnlySpan<char>);

            var valueChar = value[i];
            var withinQuotes = false;
            var endsToEat = 1;

            switch (valueChar)
            {
                //If we are at the end, return.
                case JsWriter.ItemSeperator:
                case JsWriter.MapEndChar:
                    return default(ReadOnlySpan<char>);

                //Is Within Quotes, i.e. "..."
                case JsWriter.QuoteChar:
                    while (++i < valueLength)
                    {
                        valueChar = value[i];

                        if (valueChar != JsWriter.QuoteChar) continue;

                        var isLiteralQuote = i + 1 < valueLength && value[i + 1] == JsWriter.QuoteChar;

                        i++; //skip quote
                        if (!isLiteralQuote)
                            break;
                    }
                    return value.Slice(tokenStartPos, i - tokenStartPos);

                //Is Type/Map, i.e. {...}
                case JsWriter.MapStartChar:
                    while (++i < valueLength && endsToEat > 0)
                    {
                        valueChar = value[i];

                        if (valueChar == JsWriter.QuoteChar)
                            withinQuotes = !withinQuotes;

                        if (withinQuotes)
                            continue;

                        if (valueChar == JsWriter.MapStartChar)
                            endsToEat++;

                        if (valueChar == JsWriter.MapEndChar)
                            endsToEat--;
                    }
                    return value.Slice(tokenStartPos, i - tokenStartPos);

                //Is List, i.e. [...]
                case JsWriter.ListStartChar:
                    while (++i < valueLength && endsToEat > 0)
                    {
                        valueChar = value[i];

                        if (valueChar == JsWriter.QuoteChar)
                            withinQuotes = !withinQuotes;

                        if (withinQuotes)
                            continue;

                        if (valueChar == JsWriter.ListStartChar)
                            endsToEat++;

                        if (valueChar == JsWriter.ListEndChar)
                            endsToEat--;
                    }
                    return value.Slice(tokenStartPos, i - tokenStartPos);
            }

            //Is Value
            while (++i < valueLength)
            {
                valueChar = value[i];

                if (valueChar == JsWriter.ItemSeperator
                    || valueChar == JsWriter.MapEndChar)
                {
                    break;
                }
            }

            return value.Slice(tokenStartPos, i - tokenStartPos);
        }
    }
}