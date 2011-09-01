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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ServiceStack.Text.Common;

namespace ServiceStack.Text.Jsv
{
	internal static class JsvWriter
	{
		public static readonly JsWriter<JsvTypeSerializer> Instance = new JsWriter<JsvTypeSerializer>();

		private static readonly Dictionary<Type, WriteObjectDelegate> WriteFnCache =
			new Dictionary<Type, WriteObjectDelegate>();

		public static WriteObjectDelegate GetWriteFn(Type type)
		{
			try
			{
				WriteObjectDelegate writeFn;
				lock (WriteFnCache)
				{
					if (!WriteFnCache.TryGetValue(type, out writeFn))
					{
						var genericType = typeof(JsvWriter<>).MakeGenericType(type);
						var mi = genericType.GetMethod("WriteFn",
							BindingFlags.Public | BindingFlags.Static);

						var writeFactoryFn = (Func<WriteObjectDelegate>)Delegate.CreateDelegate(
							typeof(Func<WriteObjectDelegate>), mi);
						writeFn = writeFactoryFn();
						WriteFnCache.Add(type, writeFn);
					}
				}
				return writeFn;
			}
			catch (Exception ex)
			{
				Tracer.Instance.WriteError(ex);
				throw;
			}
		}

		public static void WriteLateBoundObject(TextWriter writer, object value, bool includeType=false)
		{
			if (value == null) return;
			var writeFn = GetWriteFn(value.GetType());
			writeFn(writer, value);
		}

		public static WriteObjectDelegate GetValueTypeToStringMethod(Type type)
		{
			return Instance.GetValueTypeToStringMethod(type);
		}
	}

	/// <summary>
	/// Implement the serializer using a more static approach
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal static class JsvWriter<T>
	{
		private static readonly WriteObjectDelegate CacheFn;

		public static WriteObjectDelegate WriteFn()
		{
			return CacheFn;
		}

		static JsvWriter()
		{
			if (typeof(T) == typeof(object))
			{
				CacheFn = JsvWriter.WriteLateBoundObject;
			}
			else
			{
				CacheFn = JsvWriter.Instance.GetWriteFn<T>();
			}
		}

		public static void WriteObject(TextWriter writer, object value, bool includeType=false)
		{
			CacheFn(writer, value,includeType);
		}

	}
}