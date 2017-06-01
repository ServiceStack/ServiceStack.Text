using System;
using System.Runtime.CompilerServices;
using System.Globalization;
using ServiceStack.Text.Json;
#if NETSTANDARD1_1
using Microsoft.Extensions.Primitives;
#endif

namespace ServiceStack.Text.Support
{
    public static class StringSegmentExtensions
    {
        const string BadFormat = "Input string was not in a correct format.";
        const string OverflowMessage = "Value was either too large or too small for an {0}.";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty(this StringSegment value)
        {
            return value.Buffer == null || value.Length == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Char GetChar(this StringSegment value, int index)
        {
            return value.Buffer[value.Offset + index];
        }

        public static int IndexOfAny(this StringSegment value, char[] chars, int start, int count)
        {
            if (start < 0 || value.Offset + start > value.Buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(start));

            if (count < 0 || value.Offset + start + count > value.Buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            var index = value.Buffer.IndexOfAny(chars, start + value.Offset, count);
            if (index != -1)
                return index - value.Offset;

            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfAny(this StringSegment value, char[] chars, int start) => value.IndexOfAny(chars, start, value.Length - start);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Substring(this StringSegment value, int pos) => value.Substring(pos, value.Length - pos);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CompareIgnoreCase(this StringSegment value, string text) => value.Equals(text, StringComparison.OrdinalIgnoreCase);

        public static StringSegment FromCsvField(this StringSegment text)
        {
            return text.IsNullOrEmpty() || !text.StartsWith(CsvConfig.ItemDelimiterString, StringComparison.Ordinal)
                ? text
                : new StringSegment(
                    text.Subsegment(CsvConfig.ItemDelimiterString.Length, text.Length - CsvConfig.EscapedItemDelimiterString.Length)
                    .Value
                    .Replace(CsvConfig.EscapedItemDelimiterString, CsvConfig.ItemDelimiterString));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ParseBoolean(this StringSegment value)
        {
            if (!value.TryParseBoolean(out bool result))
                throw new FormatException(BadFormat);

            return result;
        }

        public static bool TryParseBoolean(this StringSegment value, out bool result)
        {
            result = false;

            if (value.CompareIgnoreCase(bool.TrueString))
            {
                result = true;
                return true;
            }

            return value.CompareIgnoreCase(bool.FalseString);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParseDecimal(this StringSegment value, out decimal result)
        {
            return decimal.TryParse(value.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParseFloat(this StringSegment value, out float result)
        {
            return float.TryParse(value.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParseDouble(this StringSegment value, out double result)
        {
            return double.TryParse(value.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }

        enum ParseState
        {
            LeadingWhite,
            Sign,
            Number,
            DecimalPoint,
            FractionNumber,
            Exponent,
            ExponentSign,
            ExponentValue,
            TrailingWhite
        }

        private static Exception CreateOverflowException(long maxValue) =>
            new OverflowException(string.Format(OverflowMessage, SignedMaxValueToIntType(maxValue)));
        private static Exception CreateOverflowException(ulong maxValue) =>
            new OverflowException(string.Format(OverflowMessage, UnsignedMaxValueToIntType(maxValue)));

        private static string SignedMaxValueToIntType(long maxValue)
        {
            switch (maxValue)
            {
                case SByte.MaxValue:
                    return nameof(SByte);
                case Int16.MaxValue:
                    return nameof(Int16);
                case Int32.MaxValue:
                    return nameof(Int32);
                case Int64.MaxValue:
                    return nameof(Int64);
                default:
                    return "Unknown";
            }
        }

        private static string UnsignedMaxValueToIntType(ulong maxValue)
        {
            switch (maxValue)
            {
                case Byte.MaxValue:
                    return nameof(Byte);
                case UInt16.MaxValue:
                    return nameof(UInt16);
                case UInt32.MaxValue:
                    return nameof(UInt32);
                case UInt64.MaxValue:
                    return nameof(UInt64);
                default:
                    return "Unknown";
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte ParseSByte(this StringSegment value)
        {
            return JsConfig.UseSystemParseMethods
                ? sbyte.Parse(value.Value, CultureInfo.InvariantCulture)
                : (sbyte)ParseSignedInteger(value, sbyte.MaxValue, sbyte.MinValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ParseByte(this StringSegment value)
        {
            return JsConfig.UseSystemParseMethods
                ? byte.Parse(value.Value, CultureInfo.InvariantCulture)
                : (byte)ParseUnsignedInteger(value, byte.MaxValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ParseInt16(this StringSegment value)
        {
            return JsConfig.UseSystemParseMethods
                ? short.Parse(value.Value, CultureInfo.InvariantCulture)
                : (short)ParseSignedInteger(value, short.MaxValue, short.MinValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ParseUInt16(this StringSegment value)
        {
            return JsConfig.UseSystemParseMethods
                ? ushort.Parse(value.Value, CultureInfo.InvariantCulture)
                : (ushort)ParseUnsignedInteger(value, ushort.MaxValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ParseInt32(this StringSegment value)
        {
            return JsConfig.UseSystemParseMethods
                ? int.Parse(value.Value, CultureInfo.InvariantCulture)
                : (int)ParseSignedInteger(value, Int32.MaxValue, Int32.MinValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ParseUInt32(this StringSegment value)
        {
            return JsConfig.UseSystemParseMethods
                ? uint.Parse(value.Value, CultureInfo.InvariantCulture)
                : (uint)ParseUnsignedInteger(value, UInt32.MaxValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ParseInt64(this StringSegment value)
        {
            return JsConfig.UseSystemParseMethods
                ? long.Parse(value.Value, CultureInfo.InvariantCulture)
                : ParseSignedInteger(value, Int64.MaxValue, Int64.MinValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ParseUInt64(this StringSegment value)
        {
            return JsConfig.UseSystemParseMethods
                ? ulong.Parse(value.Value, CultureInfo.InvariantCulture)
                : ParseUnsignedInteger(value, UInt64.MaxValue);
        }

        private static ulong ParseUnsignedInteger(StringSegment value, ulong maxValue)
        {
            if (value.Length == 0)
                throw new FormatException(BadFormat);

            ulong result = 0;
            int i = 0;
            var state = ParseState.LeadingWhite;

            while (i < value.Length)
            {
                var c = value.GetChar(i++);

                switch (state)
                {
                    case ParseState.LeadingWhite:
                        if (JsonUtils.IsWhiteSpace(c))
                            break;
                        if (c == '0')
                        {
                            state = ParseState.TrailingWhite;
                        }
                        else if (c > '0' && c <= '9')
                        {
                            result = (ulong)(c - '0');
                            state = ParseState.Number;
                        }
                        else throw new FormatException(BadFormat);
                        break;
                    case ParseState.Number:
                        if (c >= '0' && c <= '9')
                        {
                            checked
                            {
                                result = 10 * result + (ulong)(c - '0');
                            }
                            if (result > maxValue) //check only minvalue, because in absolute value it's greater than maxvalue
                                throw CreateOverflowException(maxValue);
                        }
                        else if (JsonUtils.IsWhiteSpace(c))
                        {
                            state = ParseState.TrailingWhite;
                        }
                        else throw new FormatException(BadFormat);
                        break;
                    case ParseState.TrailingWhite:
                        if (JsonUtils.IsWhiteSpace(c))
                        {
                            state = ParseState.TrailingWhite;
                        }
                        else throw new FormatException(BadFormat);
                        break;
                }
            }

            if (state != ParseState.Number && state != ParseState.TrailingWhite)
                throw new FormatException(BadFormat);

            return result;
        }

        private static long ParseSignedInteger(StringSegment value, long maxValue, long minValue)
        {
            if (value.Buffer == null)
                throw new ArgumentNullException(nameof(value));

            if (value.Length == 0)
                throw new FormatException(BadFormat);

            long result = 0;
            int i = value.Offset;
            int end = value.Offset + value.Length;
            var state = ParseState.LeadingWhite;
            bool negative = false;

            //skip leading whitespaces
            while (i < end && JsonUtils.IsWhiteSpace(value.Buffer[i])) i++;
            if (i == end)
                throw new FormatException(BadFormat);

            while (i < end)
            {
                var c = value.Buffer[i++];

                switch (state)
                {
                    case ParseState.LeadingWhite:
                        if (c == '-')
                        {
                            negative = true;
                            state = ParseState.Sign;
                        }
                        else if (c == '0')
                        {
                            state = ParseState.TrailingWhite;
                        }
                        else if (c > '0' && c <= '9')
                        {
                            result = -(c - '0');
                            state = ParseState.Number;
                        }
                        else throw new FormatException(BadFormat);
                        break;
                    case ParseState.Sign:
                        if (c == '0')
                        {
                            state = ParseState.TrailingWhite;
                        }
                        else if (c > '0' && c <= '9')
                        {
                            result = -(c - '0');
                            state = ParseState.Number;
                        }
                        else throw new FormatException(BadFormat);
                        break;
                    case ParseState.Number:
                        if (c >= '0' && c <= '9')
                        {
                            checked
                            {
                                result = 10 * result - (c - '0');
                            }
                            if (result < minValue) //check only minvalue, because in absolute value it's greater than maxvalue
                                throw CreateOverflowException(maxValue);
                        }
                        else if (JsonUtils.IsWhiteSpace(c))
                        {
                            state = ParseState.TrailingWhite;
                        }
                        else throw new FormatException(BadFormat);
                        break;
                    case ParseState.TrailingWhite:
                        if (JsonUtils.IsWhiteSpace(c))
                        {
                            state = ParseState.TrailingWhite;
                        }
                        else throw new FormatException(BadFormat);
                        break;
                }
            }

            if (state != ParseState.Number && state != ParseState.TrailingWhite)
                throw new FormatException(BadFormat);

            if (negative)
                return result;

            checked
            {
                result = -result;
            }

            if (result > maxValue)
                throw CreateOverflowException(maxValue);

            return result;
        }

        public static decimal ParseDecimal(this StringSegment value, bool allowThousands = false)
        {
            if (JsConfig.UseSystemParseMethods)
            {
                return decimal.Parse(value.Value,
                    allowThousands ? NumberStyles.Float : (NumberStyles.Float | NumberStyles.AllowThousands),
                    CultureInfo.InvariantCulture);
            }

            if (value.Length == 0)
                throw new FormatException(BadFormat);

            decimal result = 0;
            ulong preResult = 0;
            bool isLargeNumber = false;
            int i = value.Offset;
            int end = i + value.Length;
            var state = ParseState.LeadingWhite;
            bool negative = false;
            bool noIntegerPart = false;
            sbyte scale = 0;

            while (i < end)
            {
                var c = value.Buffer[i++];

                switch (state)
                {
                    case ParseState.LeadingWhite:
                        if (JsonUtils.IsWhiteSpace(c))
                            break;

                        if (c == '-')
                        {
                            negative = true;
                            state = ParseState.Sign;
                        }
                        else if (c == '.')
                        {
                            noIntegerPart = true;
                            state = ParseState.FractionNumber;

                            if (i == end)
                                throw new FormatException(BadFormat);
                        }
                        else if (c == '0')
                        {
                            state = ParseState.DecimalPoint;
                        }
                        else if (c > '0' && c <= '9')
                        {
                            preResult = (ulong)(c - '0');
                            state = ParseState.Number;
                        }
                        else throw new FormatException(BadFormat);
                        break;
                    case ParseState.Sign:
                        if (c == '.')
                        {
                            noIntegerPart = true;
                            state = ParseState.FractionNumber;

                            if (i == end)
                                throw new FormatException(BadFormat);
                        }
                        else if (c == '0')
                        {
                            state = ParseState.DecimalPoint;
                        }
                        else if (c > '0' && c <= '9')
                        {
                            preResult = (ulong)(c - '0');
                            state = ParseState.Number;
                        }
                        else throw new FormatException(BadFormat);
                        break;
                    case ParseState.Number:
                        if (c == '.')
                        {
                            state = ParseState.FractionNumber;
                        }
                        else if (c >= '0' && c <= '9')
                        {
                            if (isLargeNumber)
                            {
                                checked
                                {
                                    result = 10 * result + (c - '0');
                                }
                            }
                            else
                            {
                                preResult = 10 * preResult + (ulong)(c - '0');
                                if (preResult > ulong.MaxValue / 10 - 10)
                                {
                                    isLargeNumber = true;
                                    result = preResult;
                                }
                            }
                        }
                        else if (JsonUtils.IsWhiteSpace(c))
                        {
                            state = ParseState.TrailingWhite;
                        }
                        else if (allowThousands && c == ',')
                        {
                        }
                        else throw new FormatException(BadFormat);
                        break;
                    case ParseState.DecimalPoint:
                        if (c == '.')
                        {
                            state = ParseState.FractionNumber;
                        }
                        else throw new FormatException(BadFormat);
                        break;
                    case ParseState.FractionNumber:
                        if (JsonUtils.IsWhiteSpace(c))
                        {
                            if (noIntegerPart)
                                throw new FormatException(BadFormat);
                            state = ParseState.TrailingWhite;
                        }
                        else if (c == 'e' || c == 'E')
                        {
                            if (noIntegerPart && scale == 0)
                                throw new FormatException(BadFormat);
                            state = ParseState.Exponent;
                        }
                        else if (c >= '0' && c <= '9')
                        {
                            if (isLargeNumber)
                            {
                                checked
                                {
                                    result = 10 * result + (c - '0');
                                }
                            }
                            else
                            {
                                preResult = 10 * preResult + (ulong)(c - '0');
                                if (preResult > ulong.MaxValue / 10 - 10)
                                {
                                    isLargeNumber = true;
                                    result = preResult;
                                }
                            }
                            scale++;
                        }
                        else throw new FormatException(BadFormat);
                        break;
                    case ParseState.Exponent:
                        bool expNegative = false;
                        if (c == '-')
                        {
                            if (i == end)
                                throw new FormatException(BadFormat);

                            expNegative = true;
                            c = value.Buffer[i++];
                        }
                        else if (c == '+')
                        {
                            if (i == end)
                                throw new FormatException(BadFormat);
                            c = value.Buffer[i++];
                        }

                        //skip leading zeroes
                        while (c == '0' && i < end) c = value.Buffer[i++];

                        if (c > '0' && c <= '9')
                        {
                            var exp = new StringSegment(value.Buffer, i - 1, end - i + 1).ParseSByte();
                            if (!expNegative)
                            {
                                exp = (sbyte)-exp;
                            }

                            if (exp >= 0 || scale > -exp)
                            {
                                scale += exp;
                            }
                            else
                            {
                                for (int j = 0; j < -exp - scale; j++)
                                {
                                    if (isLargeNumber)
                                    {
                                        checked
                                        {
                                            result = 10 * result;
                                        }
                                    }
                                    else
                                    {
                                        preResult = 10 * preResult;
                                        if (preResult > ulong.MaxValue / 10)
                                        {
                                            isLargeNumber = true;
                                            result = preResult;
                                        }
                                    }
                                }
                                scale = 0;
                            }

                            //set i to end of string, because ParseInt16 eats number and all trailing whites
                            i = end;
                        }
                        else throw new FormatException(BadFormat);
                        break;
                    case ParseState.TrailingWhite:
                        if (!JsonUtils.IsWhiteSpace(c))
                            throw new FormatException(BadFormat);
                        break;
                }
            }

            if (!isLargeNumber)
            {
                var mid = (int)(preResult >> 32);
                var lo = (int)(preResult & 0xffffffff);
                result = new decimal(lo, mid, 0, negative, (byte)scale);
            }
            else
            {
                var bits = decimal.GetBits(result);
                result = new decimal(bits[0], bits[1], bits[2], negative, (byte)scale);
            }

            return result;
        }
        private static readonly byte[] lo16 = {
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 0, 1,
            2, 3, 4, 5, 6, 7, 8, 9, 255, 255,
            255, 255, 255, 255, 255, 10, 11, 12, 13, 14,
            15, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 10, 11, 12,
            13, 14, 15
        };

        private static readonly byte[] hi16 = {
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 0, 16,
            32, 48, 64, 80, 96, 112, 128, 144, 255, 255,
            255, 255, 255, 255, 255, 160, 176, 192, 208, 224,
            240, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 160, 176, 192,
            208, 224, 240
        };

        public static Guid ParseGuid(this StringSegment value)
        {
            if (JsConfig.UseSystemParseMethods)
                return Guid.Parse(value.Value);

            if (value.Buffer == null)
                throw new ArgumentNullException();

            if (value.Length == 0)
                throw new FormatException(BadFormat);

            //Guid can be in one of 3 forms:
            //1. General `{dddddddd-dddd-dddd-dddd-dddddddddddd}` or `(dddddddd-dddd-dddd-dddd-dddddddddddd)` 8-4-4-4-12 chars
            //2. Hex `{0xdddddddd,0xdddd,0xdddd,{0xdd,0xdd,0xdd,0xdd,0xdd,0xdd,0xdd,0xdd}}`  8-4-4-8x2 chars
            //3. No style `dddddddddddddddddddddddddddddddd` 32 chars

            int i = value.Offset;
            int end = value.Offset + value.Length;
            while (i < end && JsonUtils.IsWhiteSpace(value.Buffer[i])) i++;

            if (i == end)
                throw new FormatException(BadFormat);

            Guid result;

            int guidLen;
            result = ParseGeneralStyleGuid(new StringSegment(value.Buffer, i, end - i), out guidLen);
            i += guidLen;

            while (i < end && JsonUtils.IsWhiteSpace(value.Buffer[i])) i++;

            if (i < end)
                throw new FormatException(BadFormat);

            return result;
        }

        private static Guid ParseGeneralStyleGuid(StringSegment value, out int len)
        {
            var buf = value.Buffer;
            var n = value.Offset;

            int dash = 0;
            len = 32;
            bool hasParentesis = false;

            if (value.Length < len)
                throw new FormatException(BadFormat);

            var cs = value.GetChar(0);
            if (cs == '{' || cs == '(')
            {
                n++;
                len += 2;
                hasParentesis = true;

                if (buf[8 + n] != '-')
                    throw new FormatException(BadFormat);
            }

            if (buf[8 + n] == '-')
            {
                if (buf[13 + n] != '-'
                    || buf[18 + n] != '-'
                    || buf[23 + n] != '-')
                    throw new FormatException(BadFormat);

                len += 4;
                dash = 1;
            }

            if (value.Length < len)
                throw new FormatException(BadFormat);

            if (hasParentesis)
            {
                var ce = buf[value.Offset + len - 1];

                if ((cs != '{' || ce != '}') && (cs != '(' || ce != ')'))
                    throw new FormatException(BadFormat);
            }

            int a;
            short b, c;
            byte d, e, f, g, h, i, j, k;

            byte a1 = ParseHexByte(buf[n], buf[n + 1]);
            n += 2;
            byte a2 = ParseHexByte(buf[n], buf[n + 1]);
            n += 2;
            byte a3 = ParseHexByte(buf[n], buf[n + 1]);
            n += 2;
            byte a4 = ParseHexByte(buf[n], buf[n + 1]);
            a = (a1 << 24) + (a2 << 16) + (a3 << 8) + a4;
            n += 2 + dash;

            byte b1 = ParseHexByte(buf[n], buf[n + 1]);
            n += 2;
            byte b2 = ParseHexByte(buf[n], buf[n + 1]);
            b = (short)((b1 << 8) + b2);
            n += 2 + dash;

            byte c1 = ParseHexByte(buf[n], buf[n + 1]);
            n += 2;
            byte c2 = ParseHexByte(buf[n], buf[n + 1]);
            c = (short)((c1 << 8) + c2);
            n += 2 + dash;

            d = ParseHexByte(buf[n], buf[n + 1]);
            n += 2;
            e = ParseHexByte(buf[n], buf[n + 1]);
            n += 2 + dash;

            f = ParseHexByte(buf[n], buf[n + 1]);
            n += 2;
            g = ParseHexByte(buf[n], buf[n + 1]);
            n += 2;
            h = ParseHexByte(buf[n], buf[n + 1]);
            n += 2;
            i = ParseHexByte(buf[n], buf[n + 1]);
            n += 2;
            j = ParseHexByte(buf[n], buf[n + 1]);
            n += 2;
            k = ParseHexByte(buf[n], buf[n + 1]);

            return new Guid(a, b, c, d, e, f, g, h, i, j, k);
        }

        private static byte ParseHexByte(char c1, char c2)
        {
            try
            {
                byte lo = lo16[c2];
                byte hi = hi16[c1];

                if (lo == 255 || hi == 255)
                    throw new FormatException(BadFormat);

                return (byte)(hi + lo);
            }
            catch (IndexOutOfRangeException)
            {
                throw new FormatException(BadFormat);
            }
        }
    }
}
