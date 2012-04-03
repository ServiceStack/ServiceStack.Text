//
// http://code.google.com/p/servicestack/wiki/TypeSerializer
// ServiceStack.Text: .NET C# POCO Type Text Serializer.
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//   Damian Hickey (dhickey@gmail.com)
//
// Copyright 2011 Liquidbit Ltd.
//
// Licensed under the same terms of ServiceStack: new BSD license.
//

using System;

namespace ServiceStack.Text
{
	public static class SystemTime
	{
		public static Func<DateTime> UtcDateTimeResolver;

		public static DateTime Now
		{
			get
			{
				var temp = UtcDateTimeResolver;
				return temp == null ? DateTime.Now : temp().ToLocalTime();
			}
		}

		public static DateTime UtcNow
		{
			get
			{
				var temp = UtcDateTimeResolver;
				return temp == null ? DateTime.Now : temp();
			}
		}
	}
}
