using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.Text.Common;
using ServiceStack.Text.Json;

namespace ServiceStack.Text
{
    public abstract class MemoryProvider
    {
        public static MemoryProvider Instance =
#if NETCORE2_1
            ServiceStack.Memory.NetCoreMemory.Provider;
#else
            DefaultMemory.Provider;
#endif

        internal const string BadFormat = "Input string was not in a correct format.";
        internal const string OverflowMessage = "Value was either too large or too small for an {0}.";

        public abstract bool TryParseBoolean(ReadOnlySpan<char> value, out bool result);
        public abstract bool ParseBoolean(ReadOnlySpan<char> value);

        public abstract bool TryParseDecimal(ReadOnlySpan<char> value, out decimal result);
        public abstract decimal ParseDecimal(ReadOnlySpan<char> value);
        public abstract decimal ParseDecimal(ReadOnlySpan<char> value, bool allowThousands);

        public abstract bool TryParseFloat(ReadOnlySpan<char> value, out float result);
        public abstract float ParseFloat(ReadOnlySpan<char> value);

        public abstract bool TryParseDouble(ReadOnlySpan<char> value, out double result);
        public abstract double ParseDouble(ReadOnlySpan<char> value);

        public abstract sbyte ParseSByte(ReadOnlySpan<char> value);
        public abstract byte ParseByte(ReadOnlySpan<char> value);
        public abstract short ParseInt16(ReadOnlySpan<char> value);
        public abstract ushort ParseUInt16(ReadOnlySpan<char> value);
        public abstract int ParseInt32(ReadOnlySpan<char> value);
        public abstract uint ParseUInt32(ReadOnlySpan<char> value);
        public abstract uint ParseUInt32(ReadOnlySpan<char> value, NumberStyles style);
        public abstract long ParseInt64(ReadOnlySpan<char> value);
        public abstract ulong ParseUInt64(ReadOnlySpan<char> value);

        public abstract Guid ParseGuid(ReadOnlySpan<char> value);

        public abstract byte[] ParseBase64(ReadOnlySpan<char> value);

        public abstract Task WriteAsync(Stream stream, ReadOnlySpan<char> value, CancellationToken token = default);
        public abstract Task WriteAsync(Stream stream, ReadOnlyMemory<byte> value, CancellationToken token = default);

        public abstract object Deserialize(Stream stream, Type type, DeserializeStringSpanDelegate deserializer);

        public abstract Task<object> DeserializeAsync(Stream stream, Type type,
            DeserializeStringSpanDelegate deserializer);

        public abstract StringBuilder Append(StringBuilder sb, ReadOnlySpan<char> value);

        public abstract int GetUtf8CharCount(ReadOnlySpan<byte> bytes);
        public abstract int GetUtf8ByteCount(ReadOnlySpan<char> chars);

        public abstract ReadOnlyMemory<byte> ToUtf8(ReadOnlySpan<char> source);
        public abstract ReadOnlyMemory<char> FromUtf8(ReadOnlySpan<byte> source);

        public abstract int ToUtf8(ReadOnlySpan<char> source, Span<byte> destination);
        public abstract int FromUtf8(ReadOnlySpan<byte> source, Span<char> destination);

        public abstract byte[] ToUtf8Bytes(ReadOnlySpan<char> source);
        public abstract string FromUtf8Bytes(ReadOnlySpan<byte> source);
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

    internal static class SignedInteger<T> where T : struct, IComparable<T>, IEquatable<T>, IConvertible
    {
        private static readonly TypeCode typeCode;
        private static readonly long minValue;
        private static readonly long maxValue;

        static SignedInteger()
        {
            typeCode = Type.GetTypeCode(typeof(T));

            switch (typeCode)
            {
                case TypeCode.SByte:
                    minValue = sbyte.MinValue;
                    maxValue = sbyte.MaxValue;
                    break;
                case TypeCode.Int16:
                    minValue = short.MinValue;
                    maxValue = short.MaxValue;
                    break;
                case TypeCode.Int32:
                    minValue = int.MinValue;
                    maxValue = int.MaxValue;
                    break;
                case TypeCode.Int64:
                    minValue = long.MinValue;
                    maxValue = long.MaxValue;
                    break;
                default:
                    throw new NotSupportedException($"{typeof(T).Name} is not a signed integer");
            }
        }

        internal static object ParseNullableObject(ReadOnlySpan<char> value)
        {
            if (value.IsNullOrEmpty())
                return null;

            return ParseObject(value);
        }

        internal static object ParseObject(ReadOnlySpan<char> value)
        {
            var result = ParseInt64(value);
            switch (typeCode)
            {
                case TypeCode.SByte:
                    return (sbyte) result;
                case TypeCode.Int16:
                    return (short) result;
                case TypeCode.Int32:
                    return (int) result;
                default:
                    return result;
            }
        }

        public static sbyte ParseSByte(ReadOnlySpan<char> value) => (sbyte) ParseInt64(value);
        public static short ParseInt16(ReadOnlySpan<char> value) => (short) ParseInt64(value);
        public static int ParseInt32(ReadOnlySpan<char> value) => (int) ParseInt64(value);

        public static long ParseInt64(ReadOnlySpan<char> value)
        {
            if (value.IsEmpty)
                throw new FormatException(MemoryProvider.BadFormat);

            long result = 0;
            int i = 0;
            int end = value.Length;
            var state = ParseState.LeadingWhite;
            bool negative = false;

            //skip leading whitespaces
            while (i < end && JsonUtils.IsWhiteSpace(value[i])) i++;

            if (i == end)
                throw new FormatException(MemoryProvider.BadFormat);

            //skip leading zeros
            while (i < end && value[i] == '0')
            {
                state = ParseState.Number;
                i++;
            }

            while (i < end)
            {
                var c = value[i++];

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
                        else throw new FormatException(MemoryProvider.BadFormat);

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
                        else throw new FormatException(MemoryProvider.BadFormat);

                        break;
                    case ParseState.Number:
                        if (c >= '0' && c <= '9')
                        {
                            checked
                            {
                                result = 10 * result - (c - '0');
                            }

                            if (result < minValue
                            ) //check only minvalue, because in absolute value it's greater than maxvalue
                                throw DefaultMemory.CreateOverflowException(maxValue);
                        }
                        else if (JsonUtils.IsWhiteSpace(c))
                        {
                            state = ParseState.TrailingWhite;
                        }
                        else throw new FormatException(MemoryProvider.BadFormat);

                        break;
                    case ParseState.TrailingWhite:
                        if (JsonUtils.IsWhiteSpace(c))
                        {
                            state = ParseState.TrailingWhite;
                        }
                        else throw new FormatException(MemoryProvider.BadFormat);

                        break;
                }
            }

            if (state != ParseState.Number && state != ParseState.TrailingWhite)
                throw new FormatException(MemoryProvider.BadFormat);

            if (negative)
                return result;

            checked
            {
                result = -result;
            }

            if (result > maxValue)
                throw DefaultMemory.CreateOverflowException(maxValue);

            return result;
        }
    }

    internal static class UnsignedInteger<T> where T : struct, IComparable<T>, IEquatable<T>, IConvertible
    {
        private static readonly TypeCode typeCode;
        private static readonly ulong maxValue;

        static UnsignedInteger()
        {
            typeCode = Type.GetTypeCode(typeof(T));

            switch (typeCode)
            {
                case TypeCode.Byte:
                    maxValue = byte.MaxValue;
                    break;
                case TypeCode.UInt16:
                    maxValue = ushort.MaxValue;
                    break;
                case TypeCode.UInt32:
                    maxValue = uint.MaxValue;
                    break;
                case TypeCode.UInt64:
                    maxValue = ulong.MaxValue;
                    break;
                default:
                    throw new NotSupportedException($"{typeof(T).Name} is not a signed integer");
            }
        }

        internal static object ParseNullableObject(ReadOnlySpan<char> value)
        {
            if (value.IsNullOrEmpty())
                return null;

            return ParseObject(value);
        }
        
        internal static object ParseObject(ReadOnlySpan<char> value)
        {
            var result = ParseUInt64(value);
            switch (typeCode)
            {
                case TypeCode.Byte:
                    return (byte) result;
                case TypeCode.UInt16:
                    return (ushort) result;
                case TypeCode.UInt32:
                    return (uint) result;
                default:
                    return result;
            }
        }

        public static byte ParseByte(ReadOnlySpan<char> value) => (byte) ParseUInt64(value);
        public static ushort ParseUInt16(ReadOnlySpan<char> value) => (ushort) ParseUInt64(value);
        public static uint ParseUInt32(ReadOnlySpan<char> value) => (uint) ParseUInt64(value);

        internal static ulong ParseUInt64(ReadOnlySpan<char> value)
        {
            if (value.IsEmpty)
                throw new FormatException(MemoryProvider.BadFormat);

            ulong result = 0;
            int i = 0;
            int end = value.Length;
            var state = ParseState.LeadingWhite;

            //skip leading whitespaces
            while (i < end && JsonUtils.IsWhiteSpace(value[i])) i++;

            if (i == end)
                throw new FormatException(MemoryProvider.BadFormat);

            //skip leading zeros
            while (i < end && value[i] == '0')
            {
                state = ParseState.Number;
                i++;
            }

            while (i < end)
            {
                var c = value[i++];

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
                            result = (ulong) (c - '0');
                            state = ParseState.Number;
                        }
                        else throw new FormatException(MemoryProvider.BadFormat);


                        break;
                    case ParseState.Number:
                        if (c >= '0' && c <= '9')
                        {
                            checked
                            {
                                result = 10 * result + (ulong) (c - '0');
                            }

                            if (result > maxValue
                            ) //check only minvalue, because in absolute value it's greater than maxvalue
                                throw DefaultMemory.CreateOverflowException(maxValue);
                        }
                        else if (JsonUtils.IsWhiteSpace(c))
                        {
                            state = ParseState.TrailingWhite;
                        }
                        else throw new FormatException(MemoryProvider.BadFormat);

                        break;
                    case ParseState.TrailingWhite:
                        if (JsonUtils.IsWhiteSpace(c))
                        {
                            state = ParseState.TrailingWhite;
                        }
                        else throw new FormatException(MemoryProvider.BadFormat);

                        break;
                }
            }

            if (state != ParseState.Number && state != ParseState.TrailingWhite)
                throw new FormatException(MemoryProvider.BadFormat);

            return result;
        }    
    }
}