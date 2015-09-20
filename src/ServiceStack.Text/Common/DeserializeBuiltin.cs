//
// https://github.com/ServiceStack/ServiceStack.Text
// ServiceStack.Text: .NET C# POCO JSON, JSV and CSV Text Serializers.
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2012 Service Stack LLC. All Rights Reserved.
//
// Licensed under the same terms of ServiceStack.
//

using System;
using System.Globalization;

namespace ServiceStack.Text.Common
{
    public static class DeserializeBuiltin<T>
    {
        private static readonly ParseStringDelegate CachedParseFn;
        static DeserializeBuiltin()
        {
            CachedParseFn = GetParseFn();
        }

        public static ParseStringDelegate Parse
        {
            get { return CachedParseFn; }
        }

        private static ParseStringDelegate GetParseFn()
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
                              value == "1" 
                            : value.Length == 2 ? 
                              value == "on" : 
                              bool.Parse(value);

                    case TypeCode.Byte:
                        return value => byte.Parse(value, CultureInfo.InvariantCulture);
                    case TypeCode.SByte:
                        return value => sbyte.Parse(value, CultureInfo.InvariantCulture);
                    case TypeCode.Int16:
                        return value => short.Parse(value, CultureInfo.InvariantCulture);
                    case TypeCode.UInt16:
                        return value => ushort.Parse(value, CultureInfo.InvariantCulture);
                    case TypeCode.Int32:
                        return value => int.Parse(value, CultureInfo.InvariantCulture);
                    case TypeCode.UInt32:
                        return value => uint.Parse(value, CultureInfo.InvariantCulture);
                    case TypeCode.Int64:
                        return value => long.Parse(value, CultureInfo.InvariantCulture);
                    case TypeCode.UInt64:
                        return value => ulong.Parse(value, CultureInfo.InvariantCulture);
                    case TypeCode.Single:
                        return value => float.Parse(value, CultureInfo.InvariantCulture);
                    case TypeCode.Double:
                        return value => double.Parse(value, CultureInfo.InvariantCulture);
                    case TypeCode.Decimal:
                        return value => decimal.Parse(value, CultureInfo.InvariantCulture);
                    case TypeCode.DateTime:
                        return value => DateTimeSerializer.ParseShortestXsdDateTime(value);
                    case TypeCode.Char:
                        return value => {
                            char cValue;
                            return char.TryParse(value, out cValue) ? cValue : '\0';
                        };
                }

                if (typeof(T) == typeof(Guid))
                    return value => new Guid(value);
                if (typeof(T) == typeof(DateTimeOffset))
                    return value => DateTimeSerializer.ParseDateTimeOffset(value);
                if (typeof(T) == typeof(TimeSpan))
                    return value => DateTimeSerializer.ParseTimeSpan(value);
            }
            else
            {
                var typeCode = nullableType.GetTypeCode();
                switch (typeCode)
                {
                    case TypeCode.Boolean:
                        return value => string.IsNullOrEmpty(value) ? 
                              (bool?)null 
                            : value.Length == 1 ?
                              value == "1"
                            : value.Length == 2 ?
                              value == "on" :
                              bool.Parse(value);

                    case TypeCode.Byte:
                        return value => string.IsNullOrEmpty(value) ? (byte?)null : byte.Parse(value, CultureInfo.InvariantCulture);
                    case TypeCode.SByte:
                        return value => string.IsNullOrEmpty(value) ? (sbyte?)null : sbyte.Parse(value, CultureInfo.InvariantCulture);
                    case TypeCode.Int16:
                        return value => string.IsNullOrEmpty(value) ? (short?)null : short.Parse(value, CultureInfo.InvariantCulture);
                    case TypeCode.UInt16:
                        return value => string.IsNullOrEmpty(value) ? (ushort?)null : ushort.Parse(value, CultureInfo.InvariantCulture);
                    case TypeCode.Int32:
                        return value => string.IsNullOrEmpty(value) ? (int?)null : int.Parse(value, CultureInfo.InvariantCulture);
                    case TypeCode.UInt32:
                        return value => string.IsNullOrEmpty(value) ? (uint?)null : uint.Parse(value, CultureInfo.InvariantCulture);
                    case TypeCode.Int64:
                        return value => string.IsNullOrEmpty(value) ? (long?)null : long.Parse(value, CultureInfo.InvariantCulture);
                    case TypeCode.UInt64:
                        return value => string.IsNullOrEmpty(value) ? (ulong?)null : ulong.Parse(value, CultureInfo.InvariantCulture);
                    case TypeCode.Single:
                        return value => string.IsNullOrEmpty(value) ? (float?)null : float.Parse(value, CultureInfo.InvariantCulture);
                    case TypeCode.Double:
                        return value => string.IsNullOrEmpty(value) ? (double?)null : double.Parse(value, CultureInfo.InvariantCulture);
                    case TypeCode.Decimal:
                        return value => string.IsNullOrEmpty(value) ? (decimal?)null : decimal.Parse(value, CultureInfo.InvariantCulture);
                    case TypeCode.DateTime:
                        return value => DateTimeSerializer.ParseShortestNullableXsdDateTime(value);
                    case TypeCode.Char:
                        return value => {
                            char cValue;
                            return string.IsNullOrEmpty(value) ? (char?)null : char.TryParse(value, out cValue) ? cValue : '\0';
                        };
                }

                if (typeof(T) == typeof(TimeSpan?))
                    return value => DateTimeSerializer.ParseNullableTimeSpan(value);
                if (typeof(T) == typeof(Guid?))
                    return value => string.IsNullOrEmpty(value) ? (Guid?)null : new Guid(value);
                if (typeof(T) == typeof(DateTimeOffset?))
                    return value => DateTimeSerializer.ParseNullableDateTimeOffset(value);
            }

            return null;
        }
    }
}