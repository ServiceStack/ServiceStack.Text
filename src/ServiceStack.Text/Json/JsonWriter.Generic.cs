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
using System.Threading;
using ServiceStack.Text.Common;

namespace ServiceStack.Text.Json
{
	internal static class JsonWriter
	{
		public static readonly JsWriter<JsonTypeSerializer> Instance = new JsWriter<JsonTypeSerializer>();

        private static Dictionary<Type, WriteObjectDelegate> WriteFnCache = new Dictionary<Type, WriteObjectDelegate>();

		public static WriteObjectDelegate GetWriteFn(Type type)
		{
			try
			{
				WriteObjectDelegate writeFn;
                if (WriteFnCache.TryGetValue(type, out writeFn)) return writeFn;

                var genericType = typeof(JsonWriter<>).MakeGenericType(type);
                var mi = genericType.GetMethod("WriteFn", BindingFlags.Public | BindingFlags.Static);
                var writeFactoryFn = (Func<WriteObjectDelegate>)Delegate.CreateDelegate(typeof(Func<WriteObjectDelegate>), mi);
                writeFn = writeFactoryFn();

                Dictionary<Type, WriteObjectDelegate> snapshot, newCache;
                do
                {
                    snapshot = WriteFnCache;
                    newCache = new Dictionary<Type, WriteObjectDelegate>(WriteFnCache);
                    newCache[type] = writeFn;

                } while (!ReferenceEquals(
                    Interlocked.CompareExchange(ref WriteFnCache, newCache, snapshot), snapshot));

				return writeFn;
			}
			catch (Exception ex)
			{
				Tracer.Instance.WriteError(ex);
				throw;
			}
		}

		public static void WriteLateBoundObject(TextWriter writer, object value)
		{
			if (value == null) return;
			var writeFn = GetWriteFn(value.GetType());

			var prevState = JsState.IsWritingDynamic;
			JsState.IsWritingDynamic = true;
			writeFn(writer, value);
			JsState.IsWritingDynamic = prevState;
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
	internal static class JsonWriter<T>
	{
        private static Object cacheLock = new Object();
        private static WriteObjectDelegate _cacheFn = null;

        private static WriteObjectDelegate CacheFn
        {
            get
            {
                if (_cacheFn == null)
                {
                    lock (cacheLock)
                    {
                        if (_cacheFn == null)
                        {
                            _cacheFn = typeof(T) == typeof(object)
                                ? JsonWriter.WriteLateBoundObject
                                : JsonWriter.Instance.GetWriteFn<T>();
                        }
                    }
                }
                return _cacheFn;
            }
        }


        public static WriteObjectDelegate WriteFn()
        {
            return CacheFn;
        }

        static JsonWriter()
        {
        }

        public static void WriteObject(TextWriter writer, object value)
        {
            CacheFn(writer, value);
        }
	}

}