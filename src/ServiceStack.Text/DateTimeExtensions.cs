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
			return (dateTime.ToUniversalTime().Ticks - UnixEpoch) / TimeSpan.TicksPerSecond;
		}

		public static DateTime FromUnixTime(this double unixTime)
		{
			return UnixEpochDateTimeUtc + TimeSpan.FromSeconds(unixTime);
		}

		public static long ToUnixTimeMs(this DateTime dateTime)
		{
			return (dateTime.ToUniversalTime().Ticks - UnixEpoch) / TimeSpan.TicksPerMillisecond;
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
			return dateTime.ToUniversalTime().RoundToSecond().Equals(otherDateTime.ToUniversalTime().RoundToSecond());
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
	}
}