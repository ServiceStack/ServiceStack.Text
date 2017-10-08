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
using System.Globalization;
#if NETSTANDARD2_0
using Microsoft.Extensions.Primitives;
#endif
using ServiceStack.Text.Support;

namespace ServiceStack.Text.Common
{
    public static class DeserializeBuiltin<T>
    {
        private static readonly ParseStringSegmentDelegate CachedParseFn;
        static DeserializeBuiltin()
        {
            CachedParseFn = GetParseStringSegmentFn();
        }

        public static ParseStringDelegate Parse => v => CachedParseFn(new StringSegment(v));

        public static ParseStringSegmentDelegate ParseStringSegment => CachedParseFn;

        private static ParseStringDelegate GetParseFn() => v => GetParseStringSegmentFn()(new StringSegment(v));

        private static ParseStringSegmentDelegate GetParseStringSegmentFn()
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
                              value.Equals("1")
                            : value.Length == 2 ?
                              value.Equals("on") :
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
                        return value => float.Parse(value.Value, CultureInfo.InvariantCulture);
                    case TypeCode.Double:
                        return value => double.Parse(value.Value, CultureInfo.InvariantCulture);
                    case TypeCode.Decimal:
                        return value => value.ParseDecimal(allowThousands: true);
                    case TypeCode.DateTime:
                        return value => DateTimeSerializer.ParseShortestXsdDateTime(value.Value);
                    case TypeCode.Char:
                        return value =>
                        {
                            char cValue;
                            return char.TryParse(value.Value, out cValue) ? cValue : '\0';
                        };
                }

                if (typeof(T) == typeof(Guid))
                    return value => value.ParseGuid();
                if (typeof(T) == typeof(DateTimeOffset))
                    return value => DateTimeSerializer.ParseDateTimeOffset(value.Value);
                if (typeof(T) == typeof(TimeSpan))
                    return value => DateTimeSerializer.ParseTimeSpan(value.Value);
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
                              value.Equals("1")
                            : value.Length == 2 ?
                              value.Equals("on") :
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
                        return value => value.IsNullOrEmpty() ? (float?)null : float.Parse(value.Value, CultureInfo.InvariantCulture);
                    case TypeCode.Double:
                        return value => value.IsNullOrEmpty() ? (double?)null : double.Parse(value.Value, CultureInfo.InvariantCulture);
                    case TypeCode.Decimal:
                        return value => value.IsNullOrEmpty() ? (decimal?)null : value.ParseDecimal(allowThousands: true);
                    case TypeCode.DateTime:
                        return value => DateTimeSerializer.ParseShortestNullableXsdDateTime(value.Value);
                    case TypeCode.Char:
                        return value =>
                        {
                            char cValue;
                            return value.IsNullOrEmpty() ? (char?)null : char.TryParse(value.Value, out cValue) ? cValue : '\0';
                        };
                }

                if (typeof(T) == typeof(TimeSpan?))
                    return value => DateTimeSerializer.ParseNullableTimeSpan(value.Value);
                if (typeof(T) == typeof(Guid?))
                    return value => value.IsNullOrEmpty() ? (Guid?)null : value.ParseGuid();
                if (typeof(T) == typeof(DateTimeOffset?))
                    return value => DateTimeSerializer.ParseNullableDateTimeOffset(value.Value);
            }

            return null;
        }
    }
}