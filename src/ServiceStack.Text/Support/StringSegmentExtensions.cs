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
                throw new FormatException("Input string was not in a correct format");

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
            ulong result = 0;
            int i = 0;
            var state = ParseState.LeadingWhite;

            while (i < value.Length)
            {
                var c = value.GetChar(i);

                switch (state)
                {
                    case ParseState.LeadingWhite:
                        if (Char.IsWhiteSpace(c))
                        {
                            i++;
                        }
                        else if (c == '0')
                        {
                            state = ParseState.TrailingWhite;
                            i++;
                        }
                        else if (c > '0' && c <= '9')
                        {
                            result = (ulong)(c - '0');
                            state = ParseState.Number;
                            i++;
                        }
                        else
                        {
                            throw new FormatException("Input string was not in a correct format");
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
                            i++;
                        }
                        else if (Char.IsWhiteSpace(c))
                        {
                            state = ParseState.TrailingWhite;
                            i++;
                        }
                        else
                        {
                            throw new FormatException("Input string was not in a correct format");
                        }
                        break;
                    case ParseState.TrailingWhite:
                        if (Char.IsWhiteSpace(c))
                        {
                            state = ParseState.TrailingWhite;
                            i++;
                        }
                        else
                        {
                            throw new FormatException("Input string was not in a correct format");
                        }
                        break;
                }
            }

            return result;
        }


        private static long ParseSignedInteger(StringSegment value, long maxValue, long minValue)
        {
            long result = 0;
            int i = 0;
            var state = ParseState.LeadingWhite;
            bool negative = false;

            while (i < value.Length)
            {
                var c = value.GetChar(i);

                switch (state)
                {
                    case ParseState.LeadingWhite:
                        if (Char.IsWhiteSpace(c))
                        {
                            i++;
                        } else if (c == '-')
                        {
                            negative = true;
                            state = ParseState.Number;
                            i++;
                        } else if ( c == '0')
                        {
                            state = ParseState.TrailingWhite;
                            i++;
                        } else if (c > '0' && c <= '9')
                        {
                            result = - (c - '0');
                            state = ParseState.Number;
                            i++;
                        } else
                        {
                            throw new FormatException("Input string was not in a correct format");
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
                            i++;
                        }
                        else if (Char.IsWhiteSpace(c))
                        {
                            state = ParseState.TrailingWhite;
                            i++;
                        } else
                        {
                            throw new FormatException("Input string was not in a correct format");
                        }
                        break;
                    case ParseState.TrailingWhite:
                        if (Char.IsWhiteSpace(c))
                        {
                            state = ParseState.TrailingWhite;
                            i++;
                        } else
                        {
                            throw new FormatException("Input string was not in a correct format");
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

    }
}
