//
// https://github.com/ServiceStack/ServiceStack.Text
// ServiceStack.Text: .NET C# POCO JSON, JSV and CSV Text Serializers.
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2012 ServiceStack, Inc. All Rights Reserved.
//
// Licensed under the same terms of ServiceStack.
//

using System;

namespace ServiceStack.Text.Common
{
    public static class DeserializeBuiltin<T>
    {
        private static readonly ParseStringSpanDelegate CachedParseFn;
        static DeserializeBuiltin()
        {
            CachedParseFn = GetParseStringSegmentFn();
        }

        public static ParseStringDelegate Parse => v => CachedParseFn(v.AsSpan());

        public static ParseStringSpanDelegate ParseStringSpan => CachedParseFn;

        private static ParseStringDelegate GetParseFn() => v => GetParseStringSegmentFn()(v.AsSpan());

        private static ParseStringSpanDelegate GetParseStringSegmentFn()
        {
            var nullableType = Nullable.GetUnderlyingType(typeof(T));
            if (nullableType == null)
            {
                var typeCode = typeof(T).GetTypeCode();
                switch (typeCode)
                {
                    case TypeCode.Boolean:
                        //Lots of kids like to use '1', HTML checkboxes use 'on' as a soft convention
                        return value =>
                            value.Length == 1 ?
                              value[0] == '1'
                            : value.Length == 2 ?
                              value[0] == 'o' && value[1] == 'n' :
                              value.ParseBoolean();

                    case TypeCode.Byte:
                        return value => value.ParseByte();
                    case TypeCode.SByte:
                        return value => value.ParseSByte();
                    case TypeCode.Int16:
                        return value => value.ParseInt16();
                    case TypeCode.UInt16:
                        return value => value.ParseUInt16();
                    case TypeCode.Int32:
                        return value => value.ParseInt32();
                    case TypeCode.UInt32:
                        return value => value.ParseUInt32();
                    case TypeCode.Int64:
                        return value => value.ParseInt64();
                    case TypeCode.UInt64:
                        return value => value.ParseUInt64();
                    case TypeCode.Single:
                        return value => value.ParseFloat();
                    case TypeCode.Double:
                        return value => value.ParseDouble();
                    case TypeCode.Decimal:
                        return value => value.ParseDecimal();
                    case TypeCode.DateTime:
                        return value => DateTimeSerializer.ParseShortestXsdDateTime(value.ToString());
                    case TypeCode.Char:
                        return value => value[0];
                }

                if (typeof(T) == typeof(Guid))
                    return value => value.ParseGuid();
                if (typeof(T) == typeof(DateTimeOffset))
                    return value => DateTimeSerializer.ParseDateTimeOffset(value.ToString());
                if (typeof(T) == typeof(TimeSpan))
                    return value => DateTimeSerializer.ParseTimeSpan(value.ToString());
            }
            else
            {
                var typeCode = nullableType.GetTypeCode();
                switch (typeCode)
                {
                    case TypeCode.Boolean:
                        return value => value.IsNullOrEmpty() ?
                              (bool?)null
                            : value.Length == 1 ?
                              value[0] == '1'
                            : value.Length == 2 ?
                              value[0] == 'o' && value[1] == 'n' :
                              value.ParseBoolean();

                    case TypeCode.Byte:
                        return value => value.IsNullOrEmpty() ? (byte?)null : value.ParseByte();
                    case TypeCode.SByte:
                        return value => value.IsNullOrEmpty() ? (sbyte?)null : value.ParseSByte();
                    case TypeCode.Int16:
                        return value => value.IsNullOrEmpty() ? (short?)null : value.ParseInt16();
                    case TypeCode.UInt16:
                        return value => value.IsNullOrEmpty() ? (ushort?)null : value.ParseUInt16();
                    case TypeCode.Int32:
                        return value => value.IsNullOrEmpty() ? (int?)null : value.ParseInt32();
                    case TypeCode.UInt32:
                        return value => value.IsNullOrEmpty() ? (uint?)null : value.ParseUInt32();
                    case TypeCode.Int64:
                        return value => value.IsNullOrEmpty() ? (long?)null : value.ParseInt64();
                    case TypeCode.UInt64:
                        return value => value.IsNullOrEmpty() ? (ulong?)null : value.ParseUInt64();
                    case TypeCode.Single:
                        return value => value.IsNullOrEmpty() ? (float?)null : value.ParseFloat();
                    case TypeCode.Double:
                        return value => value.IsNullOrEmpty() ? (double?)null : value.ParseDouble();
                    case TypeCode.Decimal:
                        return value => value.IsNullOrEmpty() ? (decimal?)null : value.ParseDecimal();
                    case TypeCode.DateTime:
                        return value => DateTimeSerializer.ParseShortestNullableXsdDateTime(value.ToString());
                    case TypeCode.Char:
                        return value => value.IsEmpty ? (char?)null : value[0];
                }

                if (typeof(T) == typeof(TimeSpan?))
                    return value => DateTimeSerializer.ParseNullableTimeSpan(value.ToString());
                if (typeof(T) == typeof(Guid?))
                    return value => value.IsNullOrEmpty() ? (Guid?)null : value.ParseGuid();
                if (typeof(T) == typeof(DateTimeOffset?))
                    return value => DateTimeSerializer.ParseNullableDateTimeOffset(value.ToString());
            }

            return null;
        }
    }
}