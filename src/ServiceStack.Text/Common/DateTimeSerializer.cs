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
using System.Xml;
using ServiceStack.Text.Json;

namespace ServiceStack.Text.Common
{
    public static class DateTimeSerializer
    {
        public const string ShortDateTimeFormat = "yyyy-MM-dd";					//11
        public const string DefaultDateTimeFormat = "dd/MM/yyyy HH:mm:ss";		//20
        public const string DefaultDateTimeFormatWithFraction = "dd/MM/yyyy HH:mm:ss.fff";	//24
        public const string XsdDateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffffffZ";	//29
        public const string XsdDateTimeFormat3F = "yyyy-MM-ddTHH:mm:ss.fffZ";	//25
        public const string XsdDateTimeFormatSeconds = "yyyy-MM-ddTHH:mm:ssZ";	//21

        public const string EscapedWcfJsonPrefix = "\\/Date(";
        public const string EscapedWcfJsonSuffix = ")\\/";
        public const string WcfJsonPrefix = "/Date(";
        public const char WcfJsonSuffix = ')';

        /// <summary>
        /// If AlwaysUseUtc is set to true then convert all DateTime to UTC.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        private static DateTime Prepare(this DateTime dateTime)
        {
            if (JsConfig.AlwaysUseUtc && dateTime.Kind != DateTimeKind.Utc)
                return dateTime.ToStableUniversalTime();
            else
                return dateTime;
        }

        public static DateTime? ParseShortestNullableXsdDateTime(string dateTimeStr)
        {
            if (dateTimeStr == null)
                return null;

            return ParseShortestXsdDateTime(dateTimeStr);
        }

        public static DateTime ParseShortestXsdDateTime(string dateTimeStr)
        {
            if (string.IsNullOrEmpty(dateTimeStr))
                return DateTime.MinValue;

            if (dateTimeStr.StartsWith(EscapedWcfJsonPrefix, StringComparison.Ordinal) || dateTimeStr.StartsWith(WcfJsonPrefix, StringComparison.Ordinal))
                return ParseWcfJsonDate(dateTimeStr).Prepare();

            if (dateTimeStr.Length == DefaultDateTimeFormat.Length
                || dateTimeStr.Length == DefaultDateTimeFormatWithFraction.Length)
            {
                return DateTime.Parse(dateTimeStr, CultureInfo.InvariantCulture).Prepare();
            }

            if (dateTimeStr.Length == XsdDateTimeFormatSeconds.Length)
                return DateTime.ParseExact(dateTimeStr, XsdDateTimeFormatSeconds, null,
                                           DateTimeStyles.AdjustToUniversal);

            if (dateTimeStr.Length >= XsdDateTimeFormat3F.Length
                && dateTimeStr.Length <= XsdDateTimeFormat.Length)
            {
#if NETFX_CORE
                var dateTimeType = JsConfig.DateHandler != JsonDateHandler.ISO8601
                    ? "yyyy-MM-ddTHH:mm:sszzzzzzz"
                    : "yyyy-MM-ddTHH:mm:sszzzzzzz";

                return XmlConvert.ToDateTimeOffset(dateTimeStr, dateTimeType).DateTime.Prepare();
#else
                var dateTimeType = JsConfig.DateHandler != JsonDateHandler.ISO8601
                    ? XmlDateTimeSerializationMode.Local
                    : XmlDateTimeSerializationMode.RoundtripKind;

                return XmlConvert.ToDateTime(dateTimeStr, dateTimeType).Prepare();
#endif
            }

            return DateTime.Parse(dateTimeStr, null, DateTimeStyles.AssumeLocal).Prepare();
        }

        public static string ToDateTimeString(DateTime dateTime)
        {
            return dateTime.ToStableUniversalTime().ToString(XsdDateTimeFormat);
        }

        public static DateTime ParseDateTime(string dateTimeStr)
        {
            return DateTime.ParseExact(dateTimeStr, XsdDateTimeFormat, null);
        }

        public static DateTimeOffset ParseDateTimeOffset(string dateTimeOffsetStr)
        {
            if (string.IsNullOrEmpty(dateTimeOffsetStr)) return default(DateTimeOffset);

            // for interop, do not assume format based on config
            // format: prefer TimestampOffset, DCJSCompatible
            if (dateTimeOffsetStr.StartsWith(EscapedWcfJsonPrefix, StringComparison.Ordinal) ||
                dateTimeOffsetStr.StartsWith(WcfJsonPrefix, StringComparison.Ordinal))
            {
                return ParseWcfJsonDateOffset(dateTimeOffsetStr);
            }

            // format: next preference ISO8601
            // assume utc when no offset specified
            if (dateTimeOffsetStr.LastIndexOfAny(TimeZoneChars) < 10)
            {
                if (!dateTimeOffsetStr.EndsWith("Z")) dateTimeOffsetStr += "Z";
#if __MonoCS__
                // Without that Mono uses a Local timezone))
                dateTimeOffsetStr = dateTimeOffsetStr.Substring(0, dateTimeOffsetStr.Length - 1) + "+00:00"; 
#endif
            }

            return DateTimeOffset.Parse(dateTimeOffsetStr, CultureInfo.InvariantCulture);
        }

        public static string ToXsdDateTimeString(DateTime dateTime)
        {
#if NETFX_CORE
            return XmlConvert.ToString(dateTime.ToStableUniversalTime(), XsdDateTimeFormat);
#else
            return XmlConvert.ToString(dateTime.ToStableUniversalTime(), XmlDateTimeSerializationMode.Utc);
#endif
        }

        public static string ToXsdTimeSpanString(TimeSpan timeSpan)
        {
            var r = XmlConvert.ToString(timeSpan);
#if __MonoCS__
            // Mono returns DT even if time is 00:00:00
            if (r.EndsWith("DT")) return r.Substring(0, r.Length - 1);
#endif
            return r;
        }

        public static string ToXsdTimeSpanString(TimeSpan? timeSpan)
        {
            return (timeSpan != null) ? ToXsdTimeSpanString(timeSpan.Value) : null;
        }

        public static DateTime ParseXsdDateTime(string dateTimeStr)
        {
#if NETFX_CORE
            return XmlConvert.ToDateTimeOffset(dateTimeStr).DateTime;
#else
            return XmlConvert.ToDateTime(dateTimeStr, XmlDateTimeSerializationMode.Utc);
#endif
        }

        public static TimeSpan ParseTimeSpan(string dateTimeStr)
        {
            return dateTimeStr.StartsWith("P", StringComparison.Ordinal) || dateTimeStr.StartsWith("-P", StringComparison.Ordinal)
                ? ParseXsdTimeSpan(dateTimeStr)
                : TimeSpan.Parse(dateTimeStr);
        }

        public static TimeSpan ParseXsdTimeSpan(string dateTimeStr)
        {
            return XmlConvert.ToTimeSpan(dateTimeStr);
        }

        public static TimeSpan? ParseNullableTimeSpan(string dateTimeStr)
        {
            return string.IsNullOrEmpty(dateTimeStr)
                ? (TimeSpan?)null
                : ParseTimeSpan(dateTimeStr);
        }

        public static TimeSpan? ParseXsdNullableTimeSpan(string dateTimeStr)
        {
            return String.IsNullOrEmpty(dateTimeStr) ?
                null :
                new TimeSpan?(XmlConvert.ToTimeSpan(dateTimeStr));
        }

        public static string ToShortestXsdDateTimeString(DateTime dateTime)
        {
            var timeOfDay = dateTime.TimeOfDay;

            if (timeOfDay.Ticks == 0)
                return dateTime.ToString(ShortDateTimeFormat);

            if (timeOfDay.Milliseconds == 0)
                return dateTime.ToStableUniversalTime().ToString(XsdDateTimeFormatSeconds);

            return ToXsdDateTimeString(dateTime);
        }


        static readonly char[] TimeZoneChars = new[] { '+', '-' };

        /// <summary>
        /// WCF Json format: /Date(unixts+0000)/
        /// </summary>
        /// <param name="wcfJsonDate"></param>
        /// <returns></returns>
        public static DateTimeOffset ParseWcfJsonDateOffset(string wcfJsonDate)
        {
            if (wcfJsonDate[0] == '\\')
            {
                wcfJsonDate = wcfJsonDate.Substring(1);
            }

            var suffixPos = wcfJsonDate.IndexOf(WcfJsonSuffix);
            var timeString = (suffixPos < 0) ? wcfJsonDate : wcfJsonDate.Substring(WcfJsonPrefix.Length, suffixPos - WcfJsonPrefix.Length);

            // for interop, do not assume format based on config
            if (!wcfJsonDate.StartsWith(WcfJsonPrefix, StringComparison.Ordinal))
            {
                return DateTimeOffset.Parse(timeString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            }

            var timeZonePos = timeString.LastIndexOfAny(TimeZoneChars);
            var timeZone = timeZonePos <= 0 ? string.Empty : timeString.Substring(timeZonePos);
            var unixTimeString = timeString.Substring(0, timeString.Length - timeZone.Length);

            var unixTime = long.Parse(unixTimeString);

            if (timeZone == string.Empty)
            {
                // when no timezone offset is supplied, then treat the time as UTC
                return unixTime.FromUnixTimeMs();
            }

            if (JsConfig.DateHandler == JsonDateHandler.DCJSCompatible)
            {
                // DCJS ignores the offset and considers it local time if any offset exists
                // REVIEW: DCJS shoves offset in a separate field 'offsetMinutes', we have the offset in the format, so shouldn't we use it?
                return unixTime.FromUnixTimeMs().ToLocalTime();
            }

            var offset = timeZone.FromTimeOffsetString();
            var date = unixTime.FromUnixTimeMs();
            return new DateTimeOffset(date.Ticks, offset);
        }

        /// <summary>
        /// WCF Json format: /Date(unixts+0000)/
        /// </summary>
        /// <param name="wcfJsonDate"></param>
        /// <returns></returns>
        public static DateTime ParseWcfJsonDate(string wcfJsonDate)
        {
            if (wcfJsonDate[0] == JsonUtils.EscapeChar)
            {
                wcfJsonDate = wcfJsonDate.Substring(1);
            }

            var suffixPos = wcfJsonDate.IndexOf(WcfJsonSuffix);
            var timeString = wcfJsonDate.Substring(WcfJsonPrefix.Length, suffixPos - WcfJsonPrefix.Length);

            // for interop, do not assume format based on config
            if (!wcfJsonDate.StartsWith(WcfJsonPrefix, StringComparison.Ordinal))
            {
                return DateTime.Parse(timeString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            }

            var timeZonePos = timeString.LastIndexOfAny(TimeZoneChars);
            var timeZone = timeZonePos <= 0 ? string.Empty : timeString.Substring(timeZonePos);
            var unixTimeString = timeString.Substring(0, timeString.Length - timeZone.Length);

            var unixTime = long.Parse(unixTimeString);

            if (timeZone == string.Empty)
            {
                // when no timezone offset is supplied, then treat the time as UTC
                return unixTime.FromUnixTimeMs();
            }

            if (JsConfig.DateHandler == JsonDateHandler.DCJSCompatible)
            {
                // DCJS ignores the offset and considers it local time if any offset exists
                return unixTime.FromUnixTimeMs().ToLocalTime();
            }

            var offset = timeZone.FromTimeOffsetString();
            var date = unixTime.FromUnixTimeMs(offset);
            return new DateTimeOffset(date, offset).DateTime;
        }

        public static string ToWcfJsonDate(DateTime dateTime)
        {
            if (JsConfig.DateHandler == JsonDateHandler.ISO8601)
            {
                return dateTime.ToString("o", CultureInfo.InvariantCulture);
            }

            var timestamp = dateTime.ToUnixTimeMs();
            var offset = dateTime.Kind == DateTimeKind.Utc
                ? string.Empty
                : TimeZoneInfo.Local.GetUtcOffset(dateTime).ToTimeOffsetString();

            return EscapedWcfJsonPrefix + timestamp + offset + EscapedWcfJsonSuffix;
        }

        public static string ToWcfJsonDateTimeOffset(DateTimeOffset dateTimeOffset)
        {
            if (JsConfig.DateHandler == JsonDateHandler.ISO8601)
            {
                return dateTimeOffset.ToString("o", CultureInfo.InvariantCulture);
            }

            var timestamp = dateTimeOffset.Ticks.ToUnixTimeMs();
            var offset = dateTimeOffset.Offset == TimeSpan.Zero
                ? string.Empty
                : dateTimeOffset.Offset.ToTimeOffsetString();

            return EscapedWcfJsonPrefix + timestamp + offset + EscapedWcfJsonSuffix;
        }
    }
}