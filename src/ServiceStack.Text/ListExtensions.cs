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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.Text.Common;

namespace ServiceStack.Text
{
	public static class ListExtensions
	{
		public static string Join<T>(this IEnumerable<T> values)
		{
			return Join(values, JsWriter.ItemSeperatorString);
		}

		public static string Join<T>(this IEnumerable<T> values, string seperator)
		{
			var sb = new StringBuilder();
			foreach (var value in values)
			{
				if (sb.Length > 0)
					sb.Append(seperator);
				sb.Append(value);
			}
			return sb.ToString();
		}

		public static bool IsNullOrEmpty<T>(this List<T> list)
		{
			return list == null || list.Count == 0;
		}

		//TODO: make it work
		public static IEnumerable<TFrom> SafeWhere<TFrom>(this List<TFrom> list, Func<TFrom, bool> predicate)
		{
			return list.Where(predicate);
		}

		public static int NullableCount<T>(this List<T> list)
		{
			return list == null ? 0 : list.Count;
		}

		public static void AddIfNotExists<T>(this List<T> list, T item)
		{
			if (!list.Contains(item))
				list.Add(item);
		}
	}
}