using System;
using System.Runtime.CompilerServices;
using System.Globalization;
#if NETSTANDARD1_1
using Microsoft.Extensions.Primitives;
#endif

namespace ServiceStack.Text.Support
{
    public static class StringSegmentExtensions
    {
        const string BadFormat = "Input string was not in a correct format";

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
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }

            if (count < 0 || value.Offset + start + count > value.Buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            var index = value.Buffer.IndexOfAny(chars, start + value.Offset, count);
            if (index != -1)
            {
                return index - value.Offset;
            }
            else
            {
                return index;
            }
        }

        public static int IndexOfAny(this StringSegment value, char[] chars, int start) => value.IndexOfAny(chars, start, value.Length - start);

        public static string Substring(this StringSegment value, int pos) => value.Substring(pos, value.Length - pos);

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

        public static bool ParseBoolean(this StringSegment value)
        {
            bool result = false;

            if (!value.TryParseBoolean(out result))
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

            if (value.CompareIgnoreCase(bool.FalseString))
            {
                result = false;
                return true;
            }

            return false;
        }

        public static bool TryParseDecimal(this StringSegment value, out decimal result)
        {
            return decimal.TryParse(value.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }

        public static bool TryParseFloat(this StringSegment value, out float result)
        {
            return float.TryParse(value.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }

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

        public static sbyte ParseSByte(this StringSegment value) => (sbyte)ParseSignedInteger(value, sbyte.MaxValue, sbyte.MinValue);
        public static byte ParseByte(this StringSegment value) => (byte)ParseUnsignedInteger(value, byte.MaxValue);

        public static short ParseInt16(this StringSegment value) => (short)ParseSignedInteger(value, short.MaxValue, short.MinValue);
        public static ushort ParseUInt16(this StringSegment value) => (ushort)ParseUnsignedInteger(value, ushort.MaxValue);

        public static int ParseInt32(this StringSegment value) => (int)ParseSignedInteger(value, Int32.MaxValue, Int32.MinValue);
        public static uint ParseUInt32(this StringSegment value) => (uint)ParseUnsignedInteger(value, UInt32.MaxValue);

        public static long ParseInt64(this StringSegment value) => ParseSignedInteger(value, Int64.MaxValue, Int64.MinValue);
        public static ulong ParseUInt64(this StringSegment value) => ParseUnsignedInteger(value, UInt64.MaxValue);

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
                        if (Char.IsWhiteSpace(c))
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
                        else
                        {
                            throw new FormatException(BadFormat);
                        }
                        break;
                    case ParseState.Number:
                        if (c >= '0' && c <= '9')
                        {
                            checked
                            {
                                result = 10 * result + (ulong)(c - '0');
                            }
                            if (result > maxValue) //check only minvalue, because in absolute value it's greater than maxvalue
                                throw new OverflowException();
                        }
                        else if (Char.IsWhiteSpace(c))
                        {
                            state = ParseState.TrailingWhite;
                        }
                        else
                        {
                            throw new FormatException(BadFormat);
                        }
                        break;
                    case ParseState.TrailingWhite:
                        if (Char.IsWhiteSpace(c))
                        {
                            state = ParseState.TrailingWhite;
                        }
                        else
                        {
                            throw new FormatException(BadFormat);
                        }
                        break;
                }
            }

            return result;
        }

        private static long ParseSignedInteger(StringSegment value, long maxValue, long minValue)
        {
            if (value.Length == 0)
                throw new FormatException(BadFormat);

            long result = 0;
            int i = 0;
            var state = ParseState.LeadingWhite;
            bool negative = false;

            while (i < value.Length)
            {
                var c = value.GetChar(i++);

                switch (state)
                {
                    case ParseState.LeadingWhite:
                        if (Char.IsWhiteSpace(c))
                            break;

                        if (c == '-')
                        {
                            negative = true;
                            state = ParseState.Sign;
                        } else if ( c == '0')
                        {
                            state = ParseState.TrailingWhite;
                        } else if (c > '0' && c <= '9')
                        {
                            result = - (c - '0');
                            state = ParseState.Number;
                        } else
                        {
                            throw new FormatException(BadFormat);
                        }
                        break;
                    case ParseState.Sign:
                        if (c == '0')
                        {
                            state = ParseState.TrailingWhite;
                        } else if (c > '0' && c <= '9')
                        {
                            result = - (c - '0');
                            state = ParseState.Number;
                        } else
                        {
                            throw new FormatException(BadFormat);
                        }
                        break;
                    case ParseState.Number:
                        if (c >= '0' && c <= '9')
                        {
                            checked
                            {
                                result = 10 * result - (c - '0');
                            }
                            if (result < minValue) //check only minvalue, because in absolute value it's greater than maxvalue
                                throw new OverflowException();
                        }
                        else if (Char.IsWhiteSpace(c))
                        {
                            state = ParseState.TrailingWhite;
                        } else
                        {
                            throw new FormatException(BadFormat);
                        }
                        break;
                    case ParseState.TrailingWhite:
                        if (Char.IsWhiteSpace(c))
                        {
                            state = ParseState.TrailingWhite;
                        } else
                        {
                            throw new FormatException(BadFormat);
                        }
                        break;
                }
            }

            if (negative)
                return result;

            checked
            {
                result = -result;
            }

            if (result > maxValue)
                throw new OverflowException();

            return result;
        }

        public static decimal ParseDecimal(this StringSegment value, bool allowThousands = false)
        {
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
                        if (Char.IsWhiteSpace(c))
                            break;

                        if (c == '-')
                        {
                            negative = true;
                            state = ParseState.Sign;
                        } else if (c == '.')
                        {
                            noIntegerPart = true;
                            state = ParseState.FractionNumber;

                            if (i == end)
                            {
                                throw new FormatException(BadFormat);
                            }
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
                        else
                        {
                            throw new FormatException(BadFormat);
                        }
                        break;
                    case ParseState.Sign:
                        if (c == '.')
                        {
                            noIntegerPart = true;
                            state = ParseState.FractionNumber;

                            if (i == end)
                            {
                                throw new FormatException(BadFormat);
                            }
                        } else if (c == '0')
                        {
                            state = ParseState.DecimalPoint;
                        }
                        else if (c > '0' && c <= '9')
                        {
                            preResult = (ulong)(c - '0');
                            state = ParseState.Number;
                        }
                        else
                        {
                            throw new FormatException(BadFormat);
                        }
                        break;
                    case ParseState.Number:
                        if (c == '.')
                        {
                            state = ParseState.FractionNumber;
                        } else if (c >= '0' && c <= '9')
                        {
                            if (isLargeNumber)
                            {
                                checked
                                {
                                    result = 10 * result + (c - '0');
                                }
                            } else
                            {
                                preResult = 10 * preResult + (ulong)(c - '0');
                                if (preResult > ulong.MaxValue / 10 - 10)
                                {
                                    isLargeNumber = true;
                                    result = preResult;
                                }
                            }
                        }
                        else if (Char.IsWhiteSpace(c))
                        {
                            state = ParseState.TrailingWhite;
                        }
                        else
                        {
                            throw new FormatException(BadFormat);
                        }
                        break;
                    case ParseState.DecimalPoint:
                        if (c == '.')
                        {
                            state = ParseState.FractionNumber;
                        } else
                        {
                            throw new FormatException(BadFormat);
                        }
                        break;
                    case ParseState.FractionNumber:
                        if (Char.IsWhiteSpace(c))
                        {
                            if (noIntegerPart)
                                throw new FormatException(BadFormat);
                            state = ParseState.TrailingWhite;
                        } else if (c == 'e' || c == 'E')
                        {
                            if (noIntegerPart && scale == 0.1m)
                                throw new FormatException(BadFormat);
                            state = ParseState.Exponent;
                        } else if (c >= '0' && c <= '9')
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
                        } else
                        {
                            throw new FormatException(BadFormat);
                        }
                        break;
                    case ParseState.Exponent:
                        if (c == '-' || (c >= '0' && c <= '9'))
                        {
                            var exp = (sbyte)- new StringSegment(value.Buffer, i - 1, end - i + 1).ParseSByte();
                            if (exp >= 0 || scale > -exp)
                            {
                                scale += exp;
                            } else
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
                        } else
                        {
                            throw new FormatException(BadFormat);
                        }
                        break;
                    case ParseState.TrailingWhite:
                        if (!Char.IsWhiteSpace(c))
                            throw new FormatException(BadFormat);
                        break;
                }
            }

            if (!isLargeNumber)
            {
                var mid = (int) (preResult >> 32);
                var lo = (int) (preResult & 0xffffffff);
                result = new decimal(lo, mid, 0, negative, (byte) scale);
            }
            else
            {
                var bits = decimal.GetBits(result);
                result = new decimal(bits[0], bits[1], bits[2], negative, (byte) scale);
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
            //Guid can be in one of 3 forms:
            //1. General `{dddddddd-dddd-dddd-dddd-dddddddddddd}` or `(dddddddd-dddd-dddd-dddd-dddddddddddd)` 8-4-4-4-12 chars
            //2. Hex `{0xdddddddd,0xdddd,0xdddd,{0xdd,0xdd,0xdd,0xdd,0xdd,0xdd,0xdd,0xdd}}`  8-4-4-8x2 chars
            //3. No style `dddddddddddddddddddddddddddddddd` 32 chars

            int i = value.Offset;
            int end = value.Offset + value.Length;
            while (Char.IsWhiteSpace(value.Buffer[i]) && i < end) i++;

            if (i == end)
                throw new FormatException(BadFormat);

            Guid result;

            int guidLen;
            result = ParseGeneralStyleGuid(new StringSegment(value.Buffer, i, end - i), out guidLen);
            i += guidLen;

            while (i < end && Char.IsWhiteSpace(value.Buffer[i])) i++;

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
