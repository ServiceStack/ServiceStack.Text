//
// http://code.google.com/p/servicestack/wiki/TypeSerializer
// ServiceStack.Text: .NET C# POCO Type Text Serializer.
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2011 Liquidbit Ltd.
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

		public static DateTime ParseShortestXsdDateTime(string dateTimeStr)
		{
			if (string.IsNullOrEmpty(dateTimeStr))
				return DateTime.MinValue;

			if (dateTimeStr.StartsWith(EscapedWcfJsonPrefix) || dateTimeStr.StartsWith(WcfJsonPrefix))
				return ParseWcfJsonDate(dateTimeStr);

			if (dateTimeStr.Length == DefaultDateTimeFormat.Length
				|| dateTimeStr.Length == DefaultDateTimeFormatWithFraction.Length)
				return DateTime.Parse(dateTimeStr, CultureInfo.InvariantCulture);

			if (dateTimeStr.Length == XsdDateTimeFormatSeconds.Length)
				return DateTime.ParseExact(dateTimeStr, XsdDateTimeFormatSeconds, null,
										   DateTimeStyles.AdjustToUniversal);

			if (dateTimeStr.Length >= XsdDateTimeFormat3F.Length
				&& dateTimeStr.Length <= XsdDateTimeFormat.Length)
				return XmlConvert.ToDateTime(dateTimeStr, XmlDateTimeSerializationMode.Local);

			return DateTime.Parse(dateTimeStr, null, DateTimeStyles.AssumeLocal);
		}

		public static string ToDateTimeString(DateTime dateTime)
		{
			return dateTime.ToStableUniversalTime().ToString(XsdDateTimeFormat);
		}

		public static DateTime ParseDateTime(string dateTimeStr)
		{
			return DateTime.ParseExact(dateTimeStr, XsdDateTimeFormat, null);
		}

		public static string ToXsdDateTimeString(DateTime dateTime)
		{
			return XmlConvert.ToString(dateTime.ToStableUniversalTime(), XmlDateTimeSerializationMode.Utc);
		}

		public static DateTime ParseXsdDateTime(string dateTimeStr)
		{
			return XmlConvert.ToDateTime(dateTimeStr, XmlDateTimeSerializationMode.Utc);
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
		public static DateTime ParseWcfJsonDate(string wcfJsonDate)
		{

			if (wcfJsonDate[0] == JsonUtils.EscapeChar)
			{
				wcfJsonDate = wcfJsonDate.Substring(1);
			}

			var suffixPos = wcfJsonDate.IndexOf(WcfJsonSuffix);
			var timeString = wcfJsonDate.Substring(WcfJsonPrefix.Length, suffixPos - WcfJsonPrefix.Length);

			if (JsConfig.DateHandler == JsonDateHandler.ISO8601)
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
			return date;
		}

		public static string ToWcfJsonDate(DateTime dateTime)
		{
			if (JsConfig.DateHandler == JsonDateHandler.ISO8601)
			{
				return EscapedWcfJsonPrefix + dateTime.ToString("o", CultureInfo.InvariantCulture) + EscapedWcfJsonSuffix;
			}

			var timestamp = dateTime.ToUnixTimeMs();
			var offset = dateTime.Kind == DateTimeKind.Utc || dateTime.Kind == DateTimeKind.Unspecified
							? string.Empty
							: TimeZoneInfo.Local.GetUtcOffset(dateTime).ToTimeOffsetString();
			return EscapedWcfJsonPrefix + timestamp + offset + EscapedWcfJsonSuffix;
		}
	}
}