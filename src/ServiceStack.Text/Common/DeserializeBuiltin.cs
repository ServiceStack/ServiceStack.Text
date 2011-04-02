//
// http://code.google.com/p/servicestack/wiki/TypeSerializer
// ServiceStack.Text: .NET C# POCO Type Text Serializer.
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2010 Liquidbit Ltd.
//
// Licensed under the same terms of ServiceStack: new BSD license.
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

        private delegate T2 ParseDelegate<T2>(string val);

        private static object ParseNullable<T3>(string val, ParseDelegate<T3> del)
        {
            if (val == null)
                return null;
            else
                return (object)del(val);
        }

        private static object ParseNonNullable<T3>(string val, ParseDelegate<T3> del)
        {
            if (val == null)
                return default(T3);
            else
                return (object)del(val);
        }

		private static ParseStringDelegate GetParseFn()
		{
			//Note the generic typeof(T) is faster than using var type = typeof(T)
			if (typeof(T) == typeof(bool))
				return value => ParseNonNullable(value,bool.Parse);
            if (typeof(T) == typeof(bool?))
                return value => ParseNullable(value, bool.Parse);
			if (typeof(T) == typeof(byte))
				return value => ParseNonNullable(value,byte.Parse);
            if (typeof(T) == typeof(byte?))
                return value => ParseNullable(value, byte.Parse);
			if (typeof(T) == typeof(sbyte))
                return value => ParseNonNullable(value,sbyte.Parse);
            if (typeof(T) == typeof(sbyte?))
                return value => ParseNullable(value, sbyte.Parse);
			if (typeof(T) == typeof(short))
				return value => ParseNonNullable(value,short.Parse);
            if(typeof(T) == typeof(short?))
                return value => ParseNullable(value, short.Parse);
			if (typeof(T) == typeof(int))
				return value => ParseNonNullable(value,int.Parse);
            if (typeof(T) == typeof(int?))
                return value => ParseNullable(value, int.Parse);
            if (typeof(T) == typeof(long))
                return value => ParseNonNullable(value, long.Parse);
            if (typeof(T) == typeof(long?))
                return value => ParseNullable(value, long.Parse);
			if (typeof(T) == typeof(float))
                return value => ParseNonNullable(value, x => float.Parse(x, CultureInfo.InvariantCulture));
            if (typeof(T) == typeof(float?))
                return value => ParseNullable(value, x =>  float.Parse(x, CultureInfo.InvariantCulture));
			if (typeof(T) == typeof(double))
				return value => ParseNonNullable(value, x => double.Parse(x, CultureInfo.InvariantCulture));
            if (typeof(T) == typeof(double?))
                return value => ParseNullable(value, x => double.Parse(x, CultureInfo.InvariantCulture));
			if (typeof(T) == typeof(decimal))
                return value => ParseNonNullable(value, x => decimal.Parse(x, CultureInfo.InvariantCulture));
            if(typeof(T) == typeof(decimal?))
                return value => ParseNullable(value, x => decimal.Parse(x, CultureInfo.InvariantCulture));

			if (typeof(T) == typeof(Guid))
				return value => new Guid(value);
            if (typeof(T) == typeof(Guid?))
                return value => ParseNullable(value, x => new Guid(x));
			if (typeof(T) == typeof(DateTime))
				return value => DateTimeSerializer.ParseShortestXsdDateTime(value);
            if (typeof(T) == typeof(DateTime?))
                return value => ParseNullable(value, DateTimeSerializer.ParseShortestXsdDateTime);
			if (typeof(T) == typeof(TimeSpan) || typeof(T) == typeof(TimeSpan?))
				return value => value == null ? TimeSpan.Zero : TimeSpan.Parse(value);

			if (typeof(T) == typeof(char) || typeof(T) == typeof(char?))
			{
				char cValue;
				return value => char.TryParse(value, out cValue) ? cValue : '\0';
			}
			if (typeof(T) == typeof(ushort))
				return value => ParseNonNullable(value,ushort.Parse);
            if(typeof(T) == typeof(ushort?))
                return value => ParseNullable(value,ushort.Parse);
			if (typeof(T) == typeof(uint))
				return value => ParseNonNullable(value,uint.Parse);
            if(typeof(T) == typeof(uint?))
                return value => ParseNullable(value, uint.Parse);
            if (typeof(T) == typeof(ulong))
                return value => ParseNonNullable(value, ulong.Parse);
            if(typeof(T) == typeof(ulong?))
                return value => ParseNullable(value, ulong.Parse);

			return null;
		}
	}
}