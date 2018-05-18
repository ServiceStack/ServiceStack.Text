//Copyright (c) ServiceStack, Inc. All Rights Reserved.
//License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using ServiceStack.Text.Common;
using ServiceStack.Text.Json;
#if NETSTANDARD2_0
using Microsoft.Extensions.Primitives;
#endif
using ServiceStack.Text.Support;

namespace ServiceStack.Text.Jsv
{
    public class JsvTypeSerializer
        : ITypeSerializer
    {
        public static ITypeSerializer Instance = new JsvTypeSerializer();

        public Func<StringSegment, object> ObjectDeserializer { get; set; }

        public bool IncludeNullValues => false;

        public bool IncludeNullValuesInDictionaries => false;

        public string TypeAttrInObject => JsConfig.JsvTypeAttrInObject;

        internal static string GetTypeAttrInObject(string typeAttr) => $"{{{typeAttr}:";

        public WriteObjectDelegate GetWriteFn<T>() => JsvWriter<T>.WriteFn();

        public WriteObjectDelegate GetWriteFn(Type type) => JsvWriter.GetWriteFn(type);

        static readonly TypeInfo DefaultTypeInfo = new TypeInfo { EncodeMapKey = false };

        public TypeInfo GetTypeInfo(Type type) => DefaultTypeInfo;

        public void WriteRawString(TextWriter writer, string value)
        {
            writer.Write(value.EncodeJsv());
        }

        public void WritePropertyName(TextWriter writer, string value)
        {
            writer.Write(value);
        }

        public void WriteBuiltIn(TextWriter writer, object value)
        {
            writer.Write(value);
        }

        public void WriteObjectString(TextWriter writer, object value)
        {
            if (value != null)
            {
                var strValue = value as string;
                if (strValue != null)
                {
                    WriteString(writer, strValue);
                }
                else
                {
                    writer.Write(value.ToString().EncodeJsv());
                }
            }
        }

        public void WriteException(TextWriter writer, object value)
        {
            writer.Write(((Exception)value).Message.EncodeJsv());
        }

        public void WriteString(TextWriter writer, string value)
        {
            if (JsState.QueryStringMode && !string.IsNullOrEmpty(value) && value.StartsWith(JsWriter.QuoteString) && value.EndsWith(JsWriter.QuoteString))
                value = String.Concat(JsWriter.QuoteChar, value, JsWriter.QuoteChar);
            else if (JsState.QueryStringMode && !string.IsNullOrEmpty(value) && value.Contains(JsWriter.ItemSeperatorString))
                value = String.Concat(JsWriter.QuoteChar, value, JsWriter.QuoteChar);

            writer.Write(value == "" ? "\"\"" : value.EncodeJsv());
        }

        public void WriteFormattableObjectString(TextWriter writer, object value)
        {
            var f = (IFormattable)value;
            writer.Write(f.ToString(null, CultureInfo.InvariantCulture).EncodeJsv());
        }

        public void WriteDateTime(TextWriter writer, object oDateTime)
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

        public void WriteNullableDateTime(TextWriter writer, object dateTime)
        {
            if (dateTime == null) return;
            WriteDateTime(writer, dateTime);
        }

        public void WriteDateTimeOffset(TextWriter writer, object oDateTimeOffset)
        {
            writer.Write(((DateTimeOffset)oDateTimeOffset).ToString("o"));
        }

        public void WriteNullableDateTimeOffset(TextWriter writer, object dateTimeOffset)
        {
            if (dateTimeOffset == null) return;
            this.WriteDateTimeOffset(writer, dateTimeOffset);
        }

        public void WriteTimeSpan(TextWriter writer, object oTimeSpan)
        {
            writer.Write(DateTimeSerializer.ToXsdTimeSpanString((TimeSpan)oTimeSpan));
        }

        public void WriteNullableTimeSpan(TextWriter writer, object oTimeSpan)
        {
            if (oTimeSpan == null) return;
            writer.Write(DateTimeSerializer.ToXsdTimeSpanString((TimeSpan?)oTimeSpan));
        }

        public void WriteGuid(TextWriter writer, object oValue)
        {
            writer.Write(((Guid)oValue).ToString("N"));
        }

        public void WriteNullableGuid(TextWriter writer, object oValue)
        {
            if (oValue == null) return;
            writer.Write(((Guid)oValue).ToString("N"));
        }

        public void WriteBytes(TextWriter writer, object oByteValue)
        {
            if (oByteValue == null) return;
            writer.Write(Convert.ToBase64String((byte[])oByteValue));
        }

        public void WriteChar(TextWriter writer, object charValue)
        {
            if (charValue == null) return;
            writer.Write((char)charValue);
        }

        public void WriteByte(TextWriter writer, object byteValue)
        {
            if (byteValue == null) return;
            writer.Write((byte)byteValue);
        }

        public void WriteSByte(TextWriter writer, object sbyteValue)
        {
            if (sbyteValue == null) return;
            writer.Write((sbyte)sbyteValue);
        }

        public void WriteInt16(TextWriter writer, object intValue)
        {
            if (intValue == null) return;
            writer.Write((short)intValue);
        }

        public void WriteUInt16(TextWriter writer, object intValue)
        {
            if (intValue == null) return;
            writer.Write((ushort)intValue);
        }

        public void WriteInt32(TextWriter writer, object intValue)
        {
            if (intValue == null) return;
            writer.Write((int)intValue);
        }

        public void WriteUInt32(TextWriter writer, object uintValue)
        {
            if (uintValue == null) return;
            writer.Write((uint)uintValue);
        }

        public void WriteUInt64(TextWriter writer, object ulongValue)
        {
            if (ulongValue == null) return;
            writer.Write((ulong)ulongValue);
        }

        public void WriteInt64(TextWriter writer, object longValue)
        {
            if (longValue == null) return;
            writer.Write((long)longValue);
        }

        public void WriteBool(TextWriter writer, object boolValue)
        {
            if (boolValue == null) return;
            writer.Write((bool)boolValue);
        }

        public void WriteFloat(TextWriter writer, object floatValue)
        {
            if (floatValue == null) return;
            var floatVal = (float)floatValue;
            var cultureInfo = JsState.IsCsv ? CsvConfig.RealNumberCultureInfo : null;

            if (Equals(floatVal, float.MaxValue) || Equals(floatVal, float.MinValue))
                writer.Write(floatVal.ToString("r", cultureInfo ?? CultureInfo.InvariantCulture));
            else
                writer.Write(floatVal.ToString("r", cultureInfo ?? CultureInfo.InvariantCulture));
        }

        public void WriteDouble(TextWriter writer, object doubleValue)
        {
            if (doubleValue == null) return;
            var doubleVal = (double)doubleValue;
            var cultureInfo = JsState.IsCsv ? CsvConfig.RealNumberCultureInfo : null;

            if (Equals(doubleVal, double.MaxValue) || Equals(doubleVal, double.MinValue))
                writer.Write(doubleVal.ToString("r", cultureInfo ?? CultureInfo.InvariantCulture));
            else
                writer.Write(doubleVal.ToString(cultureInfo ?? CultureInfo.InvariantCulture));
        }

        public void WriteDecimal(TextWriter writer, object decimalValue)
        {
            if (decimalValue == null) return;
            var cultureInfo = JsState.IsCsv ? CsvConfig.RealNumberCultureInfo : null;

            writer.Write(((decimal)decimalValue).ToString(cultureInfo ?? CultureInfo.InvariantCulture));
        }

        public void WriteEnum(TextWriter writer, object enumValue)
        {
            if (enumValue == null) return;
            if (JsConfig.TreatEnumAsInteger)
                JsWriter.WriteEnumFlags(writer, enumValue);
            else
                writer.Write(enumValue.ToString());
        }

        public void WriteEnumFlags(TextWriter writer, object enumFlagValue)
        {
            JsWriter.WriteEnumFlags(writer, enumFlagValue);
        }

        public void WriteEnumMember(TextWriter writer, object enumValue)
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

        public ParseStringDelegate GetParseFn<T>() => JsvReader.Instance.GetParseFn<T>();

        public ParseStringDelegate GetParseFn(Type type) => JsvReader.GetParseFn(type);

        public ParseStringSegmentDelegate GetParseStringSegmentFn<T>() => JsvReader.Instance.GetParseStringSegmentFn<T>();

        public ParseStringSegmentDelegate GetParseStringSegmentFn(Type type) => JsvReader.GetParseStringSegmentFn(type);

        public string UnescapeSafeString(string value) => value.FromCsvField();

        public StringSegment UnescapeSafeString(StringSegment value) => value.FromCsvField();

        public string ParseRawString(string value) => value;

        public string ParseString(string value) => value.FromCsvField();

        public string ParseString(StringSegment value) => value.Value.FromCsvField();

        public string UnescapeString(string value) => value.FromCsvField();

        public StringSegment UnescapeString(StringSegment value) => new StringSegment(value.Value.FromCsvField());

        public string EatTypeValue(string value, ref int i) => EatValue(new StringSegment(value), ref i).Value;

        public StringSegment EatTypeValue(StringSegment value, ref int i) => EatValue(value, ref i);

        public bool EatMapStartChar(string value, ref int i) => EatMapStartChar(new StringSegment(value), ref i);

        public bool EatMapStartChar(StringSegment value, ref int i)
        {
            var success = value.GetChar(i) == JsWriter.MapStartChar;
            if (success) i++;
            return success;
        }

        public string EatMapKey(string value, ref int i) => EatMapKey(new StringSegment(value), ref i).Value;

        public StringSegment EatMapKey(StringSegment value, ref int i)
        {
            var tokenStartPos = i;

            var valueLength = value.Length;

            var valueChar = value.GetChar(tokenStartPos);

            switch (valueChar)
            {
                case JsWriter.QuoteChar:
                    while (++i < valueLength)
                    {
                        valueChar = value.GetChar(i);

                        if (valueChar != JsWriter.QuoteChar) continue;

                        var isLiteralQuote = i + 1 < valueLength && value.GetChar(i + 1) == JsWriter.QuoteChar;

                        i++; //skip quote
                        if (!isLiteralQuote)
                            break;
                    }
                    return value.Subsegment(tokenStartPos, i - tokenStartPos);

                //Is Type/Map, i.e. {...}
                case JsWriter.MapStartChar:
                    var endsToEat = 1;
                    var withinQuotes = false;
                    while (++i < valueLength && endsToEat > 0)
                    {
                        valueChar = value.GetChar(i);

                        if (valueChar == JsWriter.QuoteChar)
                            withinQuotes = !withinQuotes;

                        if (withinQuotes)
                            continue;

                        if (valueChar == JsWriter.MapStartChar)
                            endsToEat++;

                        if (valueChar == JsWriter.MapEndChar)
                            endsToEat--;
                    }
                    return value.Subsegment(tokenStartPos, i - tokenStartPos);
            }

            while (value.GetChar(++i) != JsWriter.MapKeySeperator) { }
            return value.Subsegment(tokenStartPos, i - tokenStartPos);
        }

        public bool EatMapKeySeperator(string value, ref int i)
        {
            return value[i++] == JsWriter.MapKeySeperator;
        }

        public bool EatMapKeySeperator(StringSegment value, ref int i)
        {
            return value.GetChar(i++) == JsWriter.MapKeySeperator;
        }


        public bool EatItemSeperatorOrMapEndChar(string value, ref int i)
        {
            if (i == value.Length) return false;

            var success = value[i] == JsWriter.ItemSeperator
                || value[i] == JsWriter.MapEndChar;
            i++;
            return success;
        }

        public bool EatItemSeperatorOrMapEndChar(StringSegment value, ref int i)
        {
            if (i == value.Length) return false;

            var success = value.GetChar(i) == JsWriter.ItemSeperator
                          || value.GetChar(i) == JsWriter.MapEndChar;
            i++;
            return success;
        }


        public void EatWhitespace(string value, ref int i) {}

        public void EatWhitespace(StringSegment value, ref int i) { }

        public string EatValue(string value, ref int i)
        {
            return EatValue(new StringSegment(value), ref i).Value;
        }

        public StringSegment EatValue(StringSegment value, ref int i)
        {
            var tokenStartPos = i;
            var valueLength = value.Length;
            if (i == valueLength) return default(StringSegment);

            var valueChar = value.GetChar(i);
            var withinQuotes = false;
            var endsToEat = 1;

            switch (valueChar)
            {
                //If we are at the end, return.
                case JsWriter.ItemSeperator:
                case JsWriter.MapEndChar:
                    return default(StringSegment);

                //Is Within Quotes, i.e. "..."
                case JsWriter.QuoteChar:
                    while (++i < valueLength)
                    {
                        valueChar = value.GetChar(i);

                        if (valueChar != JsWriter.QuoteChar) continue;

                        var isLiteralQuote = i + 1 < valueLength && value.GetChar(i + 1) == JsWriter.QuoteChar;

                        i++; //skip quote
                        if (!isLiteralQuote)
                            break;
                    }
                    return value.Subsegment(tokenStartPos, i - tokenStartPos);

                //Is Type/Map, i.e. {...}
                case JsWriter.MapStartChar:
                    while (++i < valueLength && endsToEat > 0)
                    {
                        valueChar = value.GetChar(i);

                        if (valueChar == JsWriter.QuoteChar)
                            withinQuotes = !withinQuotes;

                        if (withinQuotes)
                            continue;

                        if (valueChar == JsWriter.MapStartChar)
                            endsToEat++;

                        if (valueChar == JsWriter.MapEndChar)
                            endsToEat--;
                    }
                    return value.Subsegment(tokenStartPos, i - tokenStartPos);

                //Is List, i.e. [...]
                case JsWriter.ListStartChar:
                    while (++i < valueLength && endsToEat > 0)
                    {
                        valueChar = value.GetChar(i);

                        if (valueChar == JsWriter.QuoteChar)
                            withinQuotes = !withinQuotes;

                        if (withinQuotes)
                            continue;

                        if (valueChar == JsWriter.ListStartChar)
                            endsToEat++;

                        if (valueChar == JsWriter.ListEndChar)
                            endsToEat--;
                    }
                    return value.Subsegment(tokenStartPos, i - tokenStartPos);
            }

            //Is Value
            while (++i < valueLength)
            {
                valueChar = value.GetChar(i);

                if (valueChar == JsWriter.ItemSeperator
                    || valueChar == JsWriter.MapEndChar)
                {
                    break;
                }
            }

            return value.Subsegment(tokenStartPos, i - tokenStartPos);
        }
    }
}