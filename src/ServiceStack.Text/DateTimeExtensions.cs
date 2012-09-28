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
using ServiceStack.Text.Common;

namespace ServiceStack.Text
{
	/// <summary>
	/// A fast, standards-based, serialization-issue free DateTime serailizer.
	/// </summary>
	public static class DateTimeExtensions
	{
		public const long UnixEpoch = 621355968000000000L;
		private static readonly DateTime UnixEpochDateTimeUtc = new DateTime(UnixEpoch, DateTimeKind.Utc);
		private static readonly DateTime UnixEpochDateTimeUnspecified = new DateTime(UnixEpoch, DateTimeKind.Unspecified);

		public static long ToUnixTime(this DateTime dateTime)
		{
			return (dateTime.ToStableUniversalTime().Ticks - UnixEpoch) / TimeSpan.TicksPerSecond;
		}

		public static DateTime FromUnixTime(this double unixTime)
		{
			return UnixEpochDateTimeUtc + TimeSpan.FromSeconds(unixTime);
		}

		public static long ToUnixTimeMs(this DateTime dateTime)
		{
			return (dateTime.ToStableUniversalTime().Ticks - UnixEpoch) / TimeSpan.TicksPerMillisecond;
		}

        public static long ToUnixTimeMs(this long ticks)
        {
            return (ticks - UnixEpoch) / TimeSpan.TicksPerMillisecond;
        }

		public static DateTime FromUnixTimeMs(this double msSince1970)
		{
			return UnixEpochDateTimeUtc + TimeSpan.FromMilliseconds(msSince1970);
		}

		public static DateTime FromUnixTimeMs(this long msSince1970)
		{
			return UnixEpochDateTimeUtc + TimeSpan.FromMilliseconds(msSince1970);
		}

		public static DateTime FromUnixTimeMs(this long msSince1970, TimeSpan offset)
		{
			return UnixEpochDateTimeUnspecified + TimeSpan.FromMilliseconds(msSince1970) + offset;
		}

		public static DateTime FromUnixTimeMs(this double msSince1970, TimeSpan offset)
		{
			return UnixEpochDateTimeUnspecified + TimeSpan.FromMilliseconds(msSince1970) + offset;
		}

		public static DateTime FromUnixTimeMs(string msSince1970)
		{
			long ms;
			if (long.TryParse(msSince1970, out ms)) return ms.FromUnixTimeMs();

			// Do we really need to support fractional unix time ms time strings??
			return double.Parse(msSince1970).FromUnixTimeMs();
		}

		public static DateTime FromUnixTimeMs(string msSince1970, TimeSpan offset)
		{
			long ms;
			if (long.TryParse(msSince1970, out ms)) return ms.FromUnixTimeMs(offset);

			// Do we really need to support fractional unix time ms time strings??
			return double.Parse(msSince1970).FromUnixTimeMs(offset);
		}

		public static DateTime RoundToMs(this DateTime dateTime)
		{
			return new DateTime((dateTime.Ticks / TimeSpan.TicksPerMillisecond) * TimeSpan.TicksPerMillisecond);
		}

		public static DateTime RoundToSecond(this DateTime dateTime)
		{
			return new DateTime((dateTime.Ticks / TimeSpan.TicksPerSecond) * TimeSpan.TicksPerSecond);
		}

		public static string ToShortestXsdDateTimeString(this DateTime dateTime)
		{
			return DateTimeSerializer.ToShortestXsdDateTimeString(dateTime);
		}

		public static DateTime FromShortestXsdDateTimeString(this string xsdDateTime)
		{
			return DateTimeSerializer.ParseShortestXsdDateTime(xsdDateTime);
		}

		public static bool IsEqualToTheSecond(this DateTime dateTime, DateTime otherDateTime)
		{
			return dateTime.ToStableUniversalTime().RoundToSecond().Equals(otherDateTime.ToStableUniversalTime().RoundToSecond());
		}

		public static string ToTimeOffsetString(this TimeSpan offset, bool includeColon = false)
		{
			var sign = offset < TimeSpan.Zero ? "-" : "+";
			var hours = Math.Abs(offset.Hours);
			var minutes = Math.Abs(offset.Minutes);
			var separator = includeColon ? ":" : "";
			return string.Format("{0}{1:00}{2}{3:00}", sign, hours, separator, minutes);
		}

		public static TimeSpan FromTimeOffsetString(this string offsetString)
		{
			if (!offsetString.Contains(":"))
				offsetString = offsetString.Insert(offsetString.Length - 2, ":");

			offsetString = offsetString.TrimStart('+');

			return TimeSpan.Parse(offsetString);
		}

		public static DateTime ToStableUniversalTime(this DateTime dateTime)
		{
#if SILVERLIGHT
			// Silverlight 3, 4 and 5 all work ok with DateTime.ToUniversalTime, but have no TimeZoneInfo.ConverTimeToUtc implementation.
			return dateTime.ToUniversalTime();
#else
			// .Net 2.0 - 3.5 has an issue with DateTime.ToUniversalTime, but works ok with TimeZoneInfo.ConvertTimeToUtc.
			// .Net 4.0+ does this under the hood anyway.
			return TimeZoneInfo.ConvertTimeToUtc(dateTime);
#endif
		}

        public static string FmtSortableDate(this DateTime from)
        {
            return from.ToString("yyyy-MM-dd");
        }

        public static string FmtSortableDateTime(this DateTime from)
        {
            return from.ToString("u");
        }

        public static DateTime LastMonday(this DateTime from)
        {
            var modayOfWeekBefore = from.Date.AddDays(-(int)from.DayOfWeek - 6);
            return modayOfWeekBefore;
        }

        public static DateTime StartOfLastMonth(this DateTime from)
        {
            return new DateTime(from.Date.Year, from.Date.Month, 1).AddMonths(-1);
        }

        public static DateTime EndOfLastMonth(this DateTime from)
        {
            return new DateTime(from.Date.Year, from.Date.Month, 1).AddDays(-1); 
        }
    }
}