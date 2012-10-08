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
using System.Reflection;
using ServiceStack.Text.Jsv;

namespace ServiceStack.Text.Common
{
	internal delegate object ParseDelegate(string value);

	public static class StaticParseMethod<T>
	{
		const string ParseMethod = "Parse";

		private static readonly ParseStringDelegate CacheFn;

		public static ParseStringDelegate Parse
		{
			get { return CacheFn; }
		}

		static StaticParseMethod()
		{
			CacheFn = GetParseFn();
		}

		public static ParseStringDelegate GetParseFn()
		{
			// Get the static Parse(string) method on the type supplied
			var parseMethodInfo = typeof(T).GetMethod(
				ParseMethod, BindingFlags.Public | BindingFlags.Static, null,
				new[] { typeof(string) }, null);

			if (parseMethodInfo == null) return null;

			ParseDelegate parseDelegate;
			try
			{
				parseDelegate = (ParseDelegate)Delegate.CreateDelegate(typeof(ParseDelegate), parseMethodInfo);
			}
			catch (ArgumentException)
			{
				//Try wrapping strongly-typed return with wrapper fn.
				var typedParseDelegate = (Func<string, T>)Delegate.CreateDelegate(typeof(Func<string, T>), parseMethodInfo);
				parseDelegate = x => typedParseDelegate(x);
			}
			if (parseDelegate != null)
				return value => parseDelegate(value.FromCsvField());

			return null;
		}
	}

	internal static class StaticParseRefTypeMethod<TSerializer, T>
		where TSerializer : ITypeSerializer
	{
		static string ParseMethod = typeof(TSerializer) == typeof(JsvTypeSerializer)
			? "ParseJsv"
			: "ParseJson";

		private static readonly ParseStringDelegate CacheFn;

		public static ParseStringDelegate Parse
		{
			get { return CacheFn; }
		}

		static StaticParseRefTypeMethod()
		{			
			CacheFn = GetParseFn();
		}

		public static ParseStringDelegate GetParseFn()
		{
			// Get the static Parse(string) method on the type supplied
			var parseMethodInfo = typeof(T).GetMethod(
				ParseMethod, BindingFlags.Public | BindingFlags.Static, null,
				new[] { typeof(string) }, null);

			if (parseMethodInfo == null) return null;

			ParseDelegate parseDelegate;
			try
			{
				parseDelegate = (ParseDelegate)Delegate.CreateDelegate(typeof(ParseDelegate), parseMethodInfo);
			}
			catch (ArgumentException)
			{
				//Try wrapping strongly-typed return with wrapper fn.
				var typedParseDelegate = (Func<string, T>)Delegate.CreateDelegate(typeof(Func<string, T>), parseMethodInfo);
				parseDelegate = x => typedParseDelegate(x);
			}
			if (parseDelegate != null)
				return value => parseDelegate(value);

			return null;
		}
	}

}