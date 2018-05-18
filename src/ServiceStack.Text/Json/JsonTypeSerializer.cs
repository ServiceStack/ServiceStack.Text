//Copyright (c) ServiceStack, Inc. All Rights Reserved.
//License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using ServiceStack.Text.Common;
using ServiceStack.Text.Pools;
#if NETSTANDARD2_0
using Microsoft.Extensions.Primitives;
#endif
using ServiceStack.Text.Support;


namespace ServiceStack.Text.Json
{
    public class JsonTypeSerializer
        : ITypeSerializer
    {
        public static ITypeSerializer Instance = new JsonTypeSerializer();

        public Func<StringSegment, object> ObjectDeserializer { get; set; }

        public bool IncludeNullValues => JsConfig.IncludeNullValues;

        public bool IncludeNullValuesInDictionaries => JsConfig.IncludeNullValuesInDictionaries;

        public string TypeAttrInObject => JsConfig.JsonTypeAttrInObject;

        internal static string GetTypeAttrInObject(string typeAttr) => $"{{\"{typeAttr}\":";

        public WriteObjectDelegate GetWriteFn<T>() => JsonWriter<T>.WriteFn();

        public WriteObjectDelegate GetWriteFn(Type type) => JsonWriter.GetWriteFn(type);

        public TypeInfo GetTypeInfo(Type type) => JsonWriter.GetTypeInfo(type);

        /// <summary>
        /// Shortcut escape when we're sure value doesn't contain any escaped chars
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        public void WriteRawString(TextWriter writer, string value)
        {
            writer.Write(JsWriter.QuoteChar);
            writer.Write(value);
            writer.Write(JsWriter.QuoteChar);
        }

        public void WritePropertyName(TextWriter writer, string value)
        {
            if (JsState.WritingKeyCount > 0)
            {
                writer.Write(JsWriter.EscapedQuoteString);
                writer.Write(value);
                writer.Write(JsWriter.EscapedQuoteString);
            }
            else
            {
                WriteRawString(writer, value);
            }
        }

        public void WriteString(TextWriter writer, string value)
        {
            JsonUtils.WriteString(writer, value);
        }

        public void WriteBuiltIn(TextWriter writer, object value)
        {
            if (JsState.WritingKeyCount > 0 && !JsState.IsWritingValue) writer.Write(JsonUtils.QuoteChar);

            WriteRawString(writer, value.ToString());

            if (JsState.WritingKeyCount > 0 && !JsState.IsWritingValue) writer.Write(JsonUtils.QuoteChar);
        }

        public void WriteObjectString(TextWriter writer, object value)
        {
            JsonUtils.WriteString(writer, value?.ToString());
        }

        public void WriteFormattableObjectString(TextWriter writer, object value)
        {
            var formattable = value as IFormattable;
            JsonUtils.WriteString(writer, formattable?.ToString(null, CultureInfo.InvariantCulture));
        }

        public void WriteException(TextWriter writer, object value)
        {
            WriteString(writer, ((Exception)value).Message);
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

            writer.Write(JsWriter.QuoteString);
            DateTimeSerializer.WriteWcfJsonDate(writer, dateTime);
            writer.Write(JsWriter.QuoteString);
        }

        public void WriteNullableDateTime(TextWriter writer, object dateTime)
        {
            if (dateTime == null)
                writer.Write(JsonUtils.Null);
            else
                WriteDateTime(writer, dateTime);
        }

        public void WriteDateTimeOffset(TextWriter writer, object oDateTimeOffset)
        {
            writer.Write(JsWriter.QuoteString);
            DateTimeSerializer.WriteWcfJsonDateTimeOffset(writer, (DateTimeOffset)oDateTimeOffset);
            writer.Write(JsWriter.QuoteString);
        }

        public void WriteNullableDateTimeOffset(TextWriter writer, object dateTimeOffset)
        {
            if (dateTimeOffset == null)
                writer.Write(JsonUtils.Null);
            else
                WriteDateTimeOffset(writer, dateTimeOffset);
        }

        public void WriteTimeSpan(TextWriter writer, object oTimeSpan)
        {
            var stringValue = JsConfig.TimeSpanHandler == TimeSpanHandler.StandardFormat
                ? oTimeSpan.ToString()
                : DateTimeSerializer.ToXsdTimeSpanString((TimeSpan)oTimeSpan);
            WriteRawString(writer, stringValue);
        }

        public void WriteNullableTimeSpan(TextWriter writer, object oTimeSpan)
        {

            if (oTimeSpan == null) return;
            WriteTimeSpan(writer, ((TimeSpan?)oTimeSpan).Value);
        }

        public void WriteGuid(TextWriter writer, object oValue)
        {
            WriteRawString(writer, ((Guid)oValue).ToString("N"));
        }

        public void WriteNullableGuid(TextWriter writer, object oValue)
        {
            if (oValue == null) return;
            WriteRawString(writer, ((Guid)oValue).ToString("N"));
        }

        public void WriteBytes(TextWriter writer, object oByteValue)
        {
            if (oByteValue == null) return;
            WriteRawString(writer, Convert.ToBase64String((byte[])oByteValue));
        }

        public void WriteChar(TextWriter writer, object charValue)
        {
            if (charValue == null)
                writer.Write(JsonUtils.Null);
            else
                WriteString(writer, ((char)charValue).ToString());
        }

        public void WriteByte(TextWriter writer, object byteValue)
        {
            if (byteValue == null)
                writer.Write(JsonUtils.Null);
            else
                writer.Write((byte)byteValue);
        }

        public void WriteSByte(TextWriter writer, object sbyteValue)
        {
            if (sbyteValue == null)
                writer.Write(JsonUtils.Null);
            else
                writer.Write((sbyte)sbyteValue);
        }

        public void WriteInt16(TextWriter writer, object intValue)
        {
            if (intValue == null)
                writer.Write(JsonUtils.Null);
            else
                writer.Write((short)intValue);
        }

        public void WriteUInt16(TextWriter writer, object intValue)
        {
            if (intValue == null)
                writer.Write(JsonUtils.Null);
            else
                writer.Write((ushort)intValue);
        }

        public void WriteInt32(TextWriter writer, object intValue)
        {
            if (intValue == null)
                writer.Write(JsonUtils.Null);
            else
                writer.Write((int)intValue);
        }

        public void WriteUInt32(TextWriter writer, object uintValue)
        {
            if (uintValue == null)
                writer.Write(JsonUtils.Null);
            else
                writer.Write((uint)uintValue);
        }

        public void WriteInt64(TextWriter writer, object integerValue)
        {
            if (integerValue == null)
                writer.Write(JsonUtils.Null);
            else
                writer.Write((long)integerValue);
        }

        public void WriteUInt64(TextWriter writer, object ulongValue)
        {
            if (ulongValue == null)
            {
                writer.Write(JsonUtils.Null);
            }
            else
                writer.Write((ulong)ulongValue);
        }

        public void WriteBool(TextWriter writer, object boolValue)
        {
            if (boolValue == null)
                writer.Write(JsonUtils.Null);
            else
                writer.Write(((bool)boolValue) ? JsonUtils.True : JsonUtils.False);
        }

        public void WriteFloat(TextWriter writer, object floatValue)
        {
            if (floatValue == null)
                writer.Write(JsonUtils.Null);
            else
            {
                var floatVal = (float)floatValue;
                if (Equals(floatVal, float.MaxValue) || Equals(floatVal, float.MinValue))
                    writer.Write(floatVal.ToString("r", CultureInfo.InvariantCulture));
                else
                    writer.Write(floatVal.ToString("r", CultureInfo.InvariantCulture));
            }
        }

        public void WriteDouble(TextWriter writer, object doubleValue)
        {
            if (doubleValue == null)
                writer.Write(JsonUtils.Null);
            else
            {
                var doubleVal = (double)doubleValue;
                if (Equals(doubleVal, double.MaxValue) || Equals(doubleVal, double.MinValue))
                    writer.Write(doubleVal.ToString("r", CultureInfo.InvariantCulture));
                else
                    writer.Write(doubleVal.ToString(CultureInfo.InvariantCulture));
            }
        }

        public void WriteDecimal(TextWriter writer, object decimalValue)
        {
            if (decimalValue == null)
                writer.Write(JsonUtils.Null);
            else
                writer.Write(((decimal)decimalValue).ToString(CultureInfo.InvariantCulture));
        }

        public void WriteEnum(TextWriter writer, object enumValue)
        {
            if (enumValue == null) return;
            if (GetTypeInfo(enumValue.GetType()).IsNumeric)
                JsWriter.WriteEnumFlags(writer, enumValue);
            else
                WriteRawString(writer, enumValue.ToString());
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
            WriteRawString(writer, useValue.ToString());
        }

        public ParseStringDelegate GetParseFn<T>()
        {
            return JsonReader.Instance.GetParseFn<T>();
        }

        public ParseStringSegmentDelegate GetParseStringSegmentFn<T>()
        {
            return JsonReader.Instance.GetParseStringSegmentFn<T>();
        }

        public ParseStringDelegate GetParseFn(Type type)
        {
            return JsonReader.GetParseFn(type);
        }

        public ParseStringSegmentDelegate GetParseStringSegmentFn(Type type)
        {
            return JsonReader.GetParseStringSegmentFn(type);
        }

        public string ParseRawString(string value)
        {
            return value;
        }

        public string ParseString(StringSegment value)
        {
            return value.IsNullOrEmpty() ? value.Value : ParseRawString(value.Value);
        }

        public string ParseString(string value)
        {
            return string.IsNullOrEmpty(value) ? value : ParseRawString(value);
        }

        public static bool IsEmptyMap(string value, int i = 1) => IsEmptyMap(new StringSegment(value));

        public static bool IsEmptyMap(StringSegment value, int i = 1)
        {
            for (; i < value.Length; i++) { var c = value.GetChar(i); if (!JsonUtils.IsWhiteSpace(c)) break; } //Whitespace inline
            if (value.Length == i) return true;
            return value.GetChar(i++) == JsWriter.MapEndChar;
        }

        internal static string ParseString(string json, ref int index)
        {
            return ParseString(new StringSegment(json), ref index).Value;
        }

        internal static StringSegment ParseString(StringSegment json, ref int index)
        {
            var jsonLength = json.Length;
            var buffer = json.Buffer;
            var offset = json.Offset;

            if (buffer[offset + index] != JsonUtils.QuoteChar)
                throw new Exception("Invalid unquoted string starting with: " + json.Value.SafeSubstring(50));

            var startIndex = ++index;
            do
            {
                char c = buffer[offset + index];
                if (c == JsonUtils.QuoteChar) break;
                if (c != JsonUtils.EscapeChar) continue;
                c = buffer[offset + index];
                index++;
                if (c == 'u')
                {
                    index += 4;
                }
            } while (index++ < jsonLength);
            if (index == jsonLength)
                throw new Exception("Invalid unquoted string ending with: " + json.Value.SafeSubstring(json.Length - 50, 50));
            index++;
            return new StringSegment(buffer, offset + startIndex, Math.Min(index, jsonLength) - startIndex - 1);
        }

        public string UnescapeString(string value)
        {
            var i = 0;
            return UnEscapeJsonString(value, ref i);
        }

        public StringSegment UnescapeString(StringSegment value)
        {
            var i = 0;
            return UnEscapeJsonString(value, ref i);
        }

        public string UnescapeSafeString(string value) => UnescapeSafeString(new StringSegment(value)).Value;

        public StringSegment UnescapeSafeString(StringSegment value)
        {
            if (value.IsNullOrEmpty()) return value;
            return value.GetChar(0) == JsonUtils.QuoteChar && value.GetChar(value.Length - 1) == JsonUtils.QuoteChar
                ? value.Subsegment(1, value.Length - 2)
                : value;

            //if (value[0] != JsonUtils.QuoteChar)
            //    throw new Exception("Invalid unquoted string starting with: " + value.SafeSubstring(50));

            //return value.Substring(1, value.Length - 2);
        }

        static readonly char[] IsSafeJsonChars = new[] { JsonUtils.QuoteChar, JsonUtils.EscapeChar };

        internal static StringSegment ParseJsonString(StringSegment json, ref int index)
        {
            for (; index < json.Length; index++) { var ch = json.GetChar(index); if (!JsonUtils.IsWhiteSpace(ch)) break; } //Whitespace inline

            return UnEscapeJsonString(json, ref index);
        }

        private static string UnEscapeJsonString(string json, ref int index)
        {
            return UnEscapeJsonString(new StringSegment(json), ref index).Value;
        }

        private static StringSegment UnEscapeJsonString(StringSegment json, ref int index)
        {
            if (json.IsNullOrEmpty()) return json;
            var jsonLength = json.Length;
            var buffer = json.Buffer;
            var offset = json.Offset;

            var firstChar = buffer[offset + index];
            if (firstChar == JsonUtils.QuoteChar)
            {
                index++;

                //MicroOp: See if we can short-circuit evaluation (to avoid StringBuilder)
                var strEndPos = json.IndexOfAny(IsSafeJsonChars, index);
                if (strEndPos == -1) return json.Subsegment(index, jsonLength - index);

                if (json.GetChar(strEndPos) == JsonUtils.QuoteChar)
                {
                    var potentialValue = json.Subsegment(index, strEndPos - index);
                    index = strEndPos + 1;
                    return potentialValue;
                }
            }
            else
            {
                var i = index + offset;
                var end = offset + jsonLength;

                while (i < end)
                {
                    var c = buffer[i];
                    if (c == JsonUtils.QuoteChar || c == JsonUtils.EscapeChar)
                        break;
                    i++;
                }
                if (i == end) return new StringSegment(buffer, offset + index, jsonLength - index);
            }

            return Unescape(json);
        }

        public static string Unescape(string input) => Unescape(input, true);
        public static string Unescape(string input, bool removeQuotes)
        {
            return Unescape(new StringSegment(input), removeQuotes).Value;
        }

        public static StringSegment Unescape(StringSegment input) => Unescape(input, true);
        public static StringSegment Unescape(StringSegment input, bool removeQuotes)
        {
            var length = input.Length;
            int start = 0;
            int count = 0;
            var output = StringBuilderThreadStatic.Allocate();
            for (; count < length;)
            {
                if (removeQuotes)
                {
                    if (input.GetChar(count) == JsonUtils.QuoteChar)
                    {
                        if (start != count)
                        {
                            output.Append(input.Buffer, input.Offset + start, count - start);
                        }
                        count++;
                        start = count;
                        continue;
                    }
                }

                if (input.GetChar(count) == JsonUtils.EscapeChar)
                {
                    if (start != count)
                    {
                        output.Append(input.Buffer, input.Offset + start, count - start);
                    }
                    start = count;
                    count++;
                    if (count >= length) continue;

                    //we will always be parsing an escaped char here
                    var c = input.GetChar(count);

                    switch (c)
                    {
                        case 'a':
                            output.Append('\a');
                            count++;
                            break;
                        case 'b':
                            output.Append('\b');
                            count++;
                            break;
                        case 'f':
                            output.Append('\f');
                            count++;
                            break;
                        case 'n':
                            output.Append('\n');
                            count++;
                            break;
                        case 'r':
                            output.Append('\r');
                            count++;
                            break;
                        case 'v':
                            output.Append('\v');
                            count++;
                            break;
                        case 't':
                            output.Append('\t');
                            count++;
                            break;
                        case 'u':
                            if (count + 4 < length)
                            {
                                var unicodeString = input.Substring(count + 1, 4);
                                var unicodeIntVal = UInt32.Parse(unicodeString, NumberStyles.HexNumber);
                                output.Append(ConvertFromUtf32((int)unicodeIntVal));
                                count += 5;
                            }
                            else
                            {
                                output.Append(c);
                            }
                            break;
                        case 'x':
                            if (count + 4 < length)
                            {
                                var unicodeString = input.Substring(count + 1, 4);
                                var unicodeIntVal = uint.Parse(unicodeString, NumberStyles.HexNumber);
                                output.Append(ConvertFromUtf32((int)unicodeIntVal));
                                count += 5;
                            }
                            else
                            if (count + 2 < length)
                            {
                                var unicodeString = input.Substring(count + 1, 2);
                                var unicodeIntVal = uint.Parse(unicodeString, NumberStyles.HexNumber);
                                output.Append(ConvertFromUtf32((int)unicodeIntVal));
                                count += 3;
                            }
                            else
                            {
                                output.Append(input.Buffer, input.Offset + start, count - start);
                            }
                            break;
                        default:
                            output.Append(c);
                            count++;
                            break;
                    }
                    start = count;
                }
                else
                {
                    count++;
                }
            }
            output.Append(input.Buffer, input.Offset + start, length - start);
            return new StringSegment(StringBuilderThreadStatic.ReturnAndFree(output));
        }

        /// <summary>
        /// Given a character as utf32, returns the equivalent string provided that the character
        /// is legal json.
        /// </summary>
        /// <param name="utf32"></param>
        /// <returns></returns>
        public static string ConvertFromUtf32(int utf32)
        {
            if (utf32 < 0 || utf32 > 0x10FFFF)
                throw new ArgumentOutOfRangeException("utf32", "The argument must be from 0 to 0x10FFFF.");
            if (utf32 < 0x10000)
                return new string((char)utf32, 1);
            utf32 -= 0x10000;
            return new string(new[] {(char) ((utf32 >> 10) + 0xD800),
                                (char) (utf32 % 0x0400 + 0xDC00)});
        }

        public string EatTypeValue(string value, ref int i)
        {
            return EatValue(value, ref i);
        }

        public StringSegment EatTypeValue(StringSegment value, ref int i)
        {
            return EatValue(value, ref i);
        }

        public bool EatMapStartChar(string value, ref int i) => EatMapStartChar(new StringSegment(value), ref i);

        public bool EatMapStartChar(StringSegment value, ref int i)
        {
            for (; i < value.Length; i++) { var c = value.GetChar(i); if (!JsonUtils.IsWhiteSpace(c)) break; } //Whitespace inline
            return value.GetChar(i++) == JsWriter.MapStartChar;
        }

        public string EatMapKey(string value, ref int i) => EatMapKey(new StringSegment(value), ref i).Value;

        public StringSegment EatMapKey(StringSegment value, ref int i)
        {
            var valueLength = value.Length;
            for (; i < value.Length; i++) { var c = value.GetChar(i); if (!JsonUtils.IsWhiteSpace(c)) break; } //Whitespace inline

            var tokenStartPos = i;
            var valueChar = value.GetChar(i);

            switch (valueChar)
            {
                //If we are at the end, return.
                case JsWriter.ItemSeperator:
                case JsWriter.MapEndChar:
                    return default(StringSegment);

                //Is Within Quotes, i.e. "..."
                case JsWriter.QuoteChar:
                    return ParseString(value, ref i);
            }

            //Is Value
            while (++i < valueLength)
            {
                valueChar = value.GetChar(i);

                if (valueChar == JsWriter.ItemSeperator
                    //If it doesn't have quotes it's either a keyword or number so also has a ws boundary
                    || (JsonUtils.IsWhiteSpace(valueChar))
                )
                {
                    break;
                }
            }

            return value.Subsegment(tokenStartPos, i - tokenStartPos);
        }

        public bool EatMapKeySeperator(string value, ref int i) => EatMapKeySeperator(new StringSegment(value), ref i);


        public bool EatMapKeySeperator(StringSegment value, ref int i)
        {
            for (; i < value.Length; i++) { var c = value.GetChar(i); if (!JsonUtils.IsWhiteSpace(c)) break; } //Whitespace inline
            if (value.Length == i) return false;
            return value.GetChar(i++) == JsWriter.MapKeySeperator;
        }

        public bool EatItemSeperatorOrMapEndChar(string value, ref int i)
        {
            return EatItemSeperatorOrMapEndChar(new StringSegment(value), ref i);
        }

        public bool EatItemSeperatorOrMapEndChar(StringSegment value, ref int i)
        {
            for (; i < value.Length; i++) { var c = value.GetChar(i); if (!JsonUtils.IsWhiteSpace(c)) break; } //Whitespace inline

            if (i == value.Length) return false;

            var success = value.GetChar(i) == JsWriter.ItemSeperator
                || value.GetChar(i) == JsWriter.MapEndChar;

            i++;

            if (success)
            {
                for (; i < value.Length; i++) { var c = value.GetChar(i); if (!JsonUtils.IsWhiteSpace(c)) break; } //Whitespace inline
            }

            return success;
        }

        public void EatWhitespace(StringSegment value, ref int i)
        {
            for (; i < value.Length; i++) { var c = value.GetChar(i); if (!JsonUtils.IsWhiteSpace(c)) break; } //Whitespace inline
        }

        public void EatWhitespace(string value, ref int i)
        {
            for (; i < value.Length; i++) { var c = value[i]; if (!JsonUtils.IsWhiteSpace(c)) break; } //Whitespace inline
        }

        public string EatValue(string value, ref int i)
        {
            return EatValue(new StringSegment(value), ref i).Value;
        }

        public StringSegment EatValue(StringSegment value, ref int i)
        {
            var buf = value.Buffer;
            var valueLength = value.Length;
            var offset = value.Offset;
            if (i == valueLength) return default(StringSegment);

            while (i < valueLength && JsonUtils.IsWhiteSpace(buf[offset + i])) i++; //Whitespace inline
            if (i == valueLength) return default(StringSegment);

            var tokenStartPos = i;
            var valueChar = buf[offset + i];
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
                    return ParseString(value, ref i);

                //Is Type/Map, i.e. {...}
                case JsWriter.MapStartChar:
                    while (++i < valueLength)
                    {
                        valueChar = buf[offset +i];

                        if (valueChar == JsonUtils.EscapeChar)
                        {
                            i++;
                            continue;
                        }

                        if (valueChar == JsWriter.QuoteChar)
                            withinQuotes = !withinQuotes;

                        if (withinQuotes)
                            continue;

                        if (valueChar == JsWriter.MapStartChar)
                            endsToEat++;

                        if (valueChar == JsWriter.MapEndChar && --endsToEat == 0)
                        {
                            i++;
                            break;
                        }
                    }
                    return value.Subsegment(tokenStartPos, i - tokenStartPos);

                //Is List, i.e. [...]
                case JsWriter.ListStartChar:
                    while (++i < valueLength)
                    {
                        valueChar = buf[offset + i];

                        if (valueChar == JsonUtils.EscapeChar)
                        {
                            i++;
                            continue;
                        }

                        if (valueChar == JsWriter.QuoteChar)
                            withinQuotes = !withinQuotes;

                        if (withinQuotes)
                            continue;

                        if (valueChar == JsWriter.ListStartChar)
                            endsToEat++;

                        if (valueChar == JsWriter.ListEndChar && --endsToEat == 0)
                        {
                            i++;
                            break;
                        }
                    }
                    return value.Subsegment(tokenStartPos, i - tokenStartPos);
            }

            //Is Value
            while (++i < valueLength)
            {
                valueChar = buf[offset + i];

                if (valueChar == JsWriter.ItemSeperator
                    || valueChar == JsWriter.MapEndChar
                    //If it doesn't have quotes it's either a keyword or number so also has a ws boundary
                    || JsonUtils.IsWhiteSpace(valueChar)
                )
                {
                    break;
                }
            }

            var strValue = value.Subsegment(tokenStartPos, i - tokenStartPos);
            return strValue == new StringSegment(JsonUtils.Null) ? default(StringSegment) : strValue;
        }
    }

}