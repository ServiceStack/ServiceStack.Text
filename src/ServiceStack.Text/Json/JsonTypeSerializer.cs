//
// https://github.com/ServiceStack/ServiceStack.Text
// ServiceStack.Text: .NET C# POCO JSON, JSV and CSV Text Serializers.
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2012 ServiceStack Ltd.
//
// Licensed under the same terms of ServiceStack: new BSD license.
//

using System;
using System.Globalization;
using System.IO;
using System.Text;
using ServiceStack.Text.Common;

namespace ServiceStack.Text.Json
{
    internal class JsonTypeSerializer
        : ITypeSerializer
    {
        public static ITypeSerializer Instance = new JsonTypeSerializer();

        public bool IncludeNullValues
        {
            get { return JsConfig.IncludeNullValues; }
        }

        public string TypeAttrInObject
        {
            get { return JsConfig.JsonTypeAttrInObject; }
        }

        internal static string GetTypeAttrInObject(string typeAttr)
        {
            return string.Format("{{\"{0}\":", typeAttr);
        }

        public static readonly bool[] WhiteSpaceFlags = new bool[' ' + 1];

        static JsonTypeSerializer()
        {
            WhiteSpaceFlags[' '] = true;
            WhiteSpaceFlags['\t'] = true;
            WhiteSpaceFlags['\r'] = true;
            WhiteSpaceFlags['\n'] = true;
        }

        public WriteObjectDelegate GetWriteFn<T>()
        {
            return JsonWriter<T>.WriteFn();
        }

        public WriteObjectDelegate GetWriteFn(Type type)
        {
            return JsonWriter.GetWriteFn(type);
        }

        public TypeInfo GetTypeInfo(Type type)
        {
            return JsonWriter.GetTypeInfo(type);
        }

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
            JsonUtils.WriteString(writer, value != null ? value.ToString() : null);
        }

        public void WriteException(TextWriter writer, object value)
        {
            WriteString(writer, ((Exception)value).Message);
        }

        public void WriteDateTime(TextWriter writer, object oDateTime)
        {
            WriteRawString(writer, DateTimeSerializer.ToWcfJsonDate((DateTime)oDateTime));
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
            WriteRawString(writer, DateTimeSerializer.ToWcfJsonDateTimeOffset((DateTimeOffset)oDateTimeOffset));
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
            var stringValue = JsConfig.TimeSpanHandler == JsonTimeSpanHandler.StandardFormat
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
                WriteRawString(writer, ((char)charValue).ToString(CultureInfo.InvariantCulture));
        }

        public void WriteByte(TextWriter writer, object byteValue)
        {
            if (byteValue == null)
                writer.Write(JsonUtils.Null);
            else
                writer.Write((byte)byteValue);
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
                    writer.Write(floatVal.ToString(CultureInfo.InvariantCulture));
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
            WriteRawString(writer, enumValue.ToString());
        }

        public void WriteEnumFlags(TextWriter writer, object enumFlagValue)
        {
			JsWriter.WriteEnumFlags(writer, enumFlagValue);
        }

        public void WriteLinqBinary(TextWriter writer, object linqBinaryValue)
        {
#if !MONOTOUCH && !SILVERLIGHT && !XBOX  && !ANDROID
            WriteRawString(writer, Convert.ToBase64String(((System.Data.Linq.Binary)linqBinaryValue).ToArray()));
#endif
        }

        public ParseStringDelegate GetParseFn<T>()
        {
            return JsonReader.Instance.GetParseFn<T>();
        }

        public ParseStringDelegate GetParseFn(Type type)
        {
            return JsonReader.GetParseFn(type);
        }

        public string ParseRawString(string value)
        {
            return value;
        }

        public string ParseString(string value)
        {
            return string.IsNullOrEmpty(value) ? value : ParseRawString(value);
        }

        internal static bool IsEmptyMap(string value)
        {
            var i = 1;
            for (; i < value.Length; i++) { var c = value[i]; if (c >= WhiteSpaceFlags.Length || !WhiteSpaceFlags[c]) break; } //Whitespace inline
            if (value.Length == i) return true;
            return value[i++] == JsWriter.MapEndChar;
        }

        internal static string ParseString(string json, ref int index)
        {
            var jsonLength = json.Length;
            if (json[index] != JsonUtils.QuoteChar)
                throw new Exception("Invalid unquoted string starting with: " + json.SafeSubstring(50));

        	var startIndex = ++index;
            do
            {
                char c = json[index];
                if (c == JsonUtils.QuoteChar) break;
                if (c != JsonUtils.EscapeChar) continue;
                c = json[index++];
                if (c == 'u')
                {
                    index += 4;
                }
            } while (index++ < jsonLength);
            index++;
            return json.Substring(startIndex, Math.Min(index, jsonLength) - startIndex - 1);
        }

        public string UnescapeString(string value)
        {
            var i = 0;
            return UnEscapeJsonString(value, ref i);
        }

        public string UnescapeSafeString(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value[0] == JsonUtils.QuoteChar && value[value.Length - 1] == JsonUtils.QuoteChar
                ? value.Substring(1, value.Length - 2)
                : value;

            //if (value[0] != JsonUtils.QuoteChar)
            //    throw new Exception("Invalid unquoted string starting with: " + value.SafeSubstring(50));

            //return value.Substring(1, value.Length - 2);
        }

        static readonly char[] IsSafeJsonChars = new[] { JsonUtils.QuoteChar, JsonUtils.EscapeChar };

        internal static string ParseJsonString(string json, ref int index)
        {
            for (; index < json.Length; index++) { var ch = json[index]; if (ch >= WhiteSpaceFlags.Length || !WhiteSpaceFlags[ch]) break; } //Whitespace inline

            return UnEscapeJsonString(json, ref index);
        }

        private static string UnEscapeJsonString(string json, ref int index)
        {
            if (string.IsNullOrEmpty(json)) return json;
            var jsonLength = json.Length;
            if (json[index] == JsonUtils.QuoteChar)
            {
                index++;

                //MicroOp: See if we can short-circuit evaluation (to avoid StringBuilder)
                var strEndPos = json.IndexOfAny(IsSafeJsonChars, index);
                if (strEndPos == -1) return json.Substring(index, jsonLength - index);
                if (json[strEndPos] == JsonUtils.QuoteChar)
                {
                    var potentialValue = json.Substring(index, strEndPos - index);
                    index = strEndPos + 1;
                    return potentialValue;
                }
            }

            var sb = new StringBuilder(jsonLength);

        	while (true)
            {
                if (index == jsonLength) break;

                char c = json[index++];
                if (c == JsonUtils.QuoteChar) break;

                if (c == JsonUtils.EscapeChar)
                {
                    if (index == jsonLength)
                    {
                        break;
                    }
                    c = json[index++];
                    switch (c)
                    {
                        case '"':
                            sb.Append('"');
                            break;
                        case '\\':
                            sb.Append('\\');
                            break;
                        case '/':
                            sb.Append('/');
                            break;
                        case 'b':
                            sb.Append('\b');
                            break;
                        case 'f':
                            sb.Append('\f');
                            break;
                        case 'n':
                            sb.Append('\n');
                            break;
                        case 'r':
                            sb.Append('\r');
                            break;
                        case 't':
                            sb.Append('\t');
                            break;
                        case 'u':
                            var remainingLength = jsonLength - index;
                            if (remainingLength >= 4)
                            {
                                var unicodeString = json.Substring(index, 4);
                                var unicodeIntVal = UInt32.Parse(unicodeString, NumberStyles.HexNumber);
                                sb.Append(ConvertFromUtf32((int) unicodeIntVal));
                                index += 4;
                            }
                            else
                            {
                                break;
                            }
                            break;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Since Silverlight doesn't have char.ConvertFromUtf32() so putting Mono's implemenation inline.
        /// </summary>
        /// <param name="utf32"></param>
        /// <returns></returns>
        private static string ConvertFromUtf32(int utf32)
        {
            if (utf32 < 0 || utf32 > 0x10FFFF)
                throw new ArgumentOutOfRangeException("utf32", "The argument must be from 0 to 0x10FFFF.");
            if (0xD800 <= utf32 && utf32 <= 0xDFFF)
                throw new ArgumentOutOfRangeException("utf32", "The argument must not be in surrogate pair range.");
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

        public bool EatMapStartChar(string value, ref int i)
        {
            for (; i < value.Length; i++) { var c = value[i]; if (c >= WhiteSpaceFlags.Length || !WhiteSpaceFlags[c]) break; } //Whitespace inline
            return value[i++] == JsWriter.MapStartChar;
        }

        public string EatMapKey(string value, ref int i)
        {
            return ParseJsonString(value, ref i);
        }

        public bool EatMapKeySeperator(string value, ref int i)
        {
            for (; i < value.Length; i++) { var c = value[i]; if (c >= WhiteSpaceFlags.Length || !WhiteSpaceFlags[c]) break; } //Whitespace inline
            if (value.Length == i) return false;
            return value[i++] == JsWriter.MapKeySeperator;
        }

        public bool EatItemSeperatorOrMapEndChar(string value, ref int i)
        {
            for (; i < value.Length; i++) { var c = value[i]; if (c >= WhiteSpaceFlags.Length || !WhiteSpaceFlags[c]) break; } //Whitespace inline

            if (i == value.Length) return false;

            var success = value[i] == JsWriter.ItemSeperator
                || value[i] == JsWriter.MapEndChar;

            i++;

            if (success)
            {
                for (; i < value.Length; i++) { var c = value[i]; if (c >= WhiteSpaceFlags.Length || !WhiteSpaceFlags[c]) break; } //Whitespace inline
            }

            return success;
        }

        public void EatWhitespace(string value, ref int i)
        {
            for (; i < value.Length; i++) { var c = value[i]; if (c >= WhiteSpaceFlags.Length || !WhiteSpaceFlags[c]) break; } //Whitespace inline
        }

        public string EatValue(string value, ref int i)
        {
            var valueLength = value.Length;
            if (i == valueLength) return null;

            for (; i < value.Length; i++) { var c = value[i]; if (c >= WhiteSpaceFlags.Length || !WhiteSpaceFlags[c]) break; } //Whitespace inline
            if (i == valueLength) return null;

            var tokenStartPos = i;
            var valueChar = value[i];
            var withinQuotes = false;
            var endsToEat = 1;

            switch (valueChar)
            {
                //If we are at the end, return.
                case JsWriter.ItemSeperator:
                case JsWriter.MapEndChar:
                    return null;

                //Is Within Quotes, i.e. "..."
                case JsWriter.QuoteChar:
                    return ParseString(value, ref i);

                //Is Type/Map, i.e. {...}
                case JsWriter.MapStartChar:
                    while (++i < valueLength && endsToEat > 0)
                    {
                        valueChar = value[i];

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

                        if (valueChar == JsWriter.MapEndChar)
                            endsToEat--;
                    }
                    return value.Substring(tokenStartPos, i - tokenStartPos);

                //Is List, i.e. [...]
                case JsWriter.ListStartChar:
                    while (++i < valueLength && endsToEat > 0)
                    {
                        valueChar = value[i];

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

                        if (valueChar == JsWriter.ListEndChar)
                            endsToEat--;
                    }
                    return value.Substring(tokenStartPos, i - tokenStartPos);
            }

            //Is Value
            while (++i < valueLength)
            {
                valueChar = value[i];

                if (valueChar == JsWriter.ItemSeperator
                    || valueChar == JsWriter.MapEndChar
                    //If it doesn't have quotes it's either a keyword or number so also has a ws boundary
                    || (valueChar < WhiteSpaceFlags.Length && WhiteSpaceFlags[valueChar])
                )
                {
                    break;
                }
            }

            var strValue = value.Substring(tokenStartPos, i - tokenStartPos);
            return strValue == JsonUtils.Null ? null : strValue;
        }
    }

}