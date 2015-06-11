//Copyright (c) Service Stack LLC. All Rights Reserved.
//License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ServiceStack.Text.Common;

namespace ServiceStack.Text.Jsv
{
	internal static class JsvWriter
	{
		public static readonly JsWriter<JsvTypeSerializer> Instance = new JsWriter<JsvTypeSerializer>();

        private static Dictionary<Type, WriteObjectDelegate> WriteFnCache = new Dictionary<Type, WriteObjectDelegate>();
        
        internal static void RemoveCacheFn(Type forType)
        {
            Dictionary<Type, WriteObjectDelegate> snapshot, newCache;
            do
            {
                snapshot = WriteFnCache;
                newCache = new Dictionary<Type, WriteObjectDelegate>(WriteFnCache);
                newCache.Remove(forType);
                
            } while (!ReferenceEquals(
                Interlocked.CompareExchange(ref WriteFnCache, newCache, snapshot), snapshot));
        }

		public static WriteObjectDelegate GetWriteFn(Type type)
		{
			try
			{
                WriteObjectDelegate writeFn;
                if (WriteFnCache.TryGetValue(type, out writeFn)) return writeFn;

                var genericType = typeof(JsvWriter<>).MakeGenericType(type);
                var mi = genericType.GetStaticMethod("WriteFn");
                var writeFactoryFn = (Func<WriteObjectDelegate>)mi.MakeDelegate(typeof(Func<WriteObjectDelegate>));

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
            if (value == null) 
                return;

            try
            {
                if (++JsState.Depth > JsConfig.MaxDepth)
                {
                    Tracer.Instance.WriteError("Exceeded MaxDepth limit of {0} attempting to serialize {1}"
                        .Fmt(JsConfig.MaxDepth, value.GetType().Name));
                    return;
                }

                var type = value.GetType();
                var writeFn = type == typeof(object)
                    ? WriteType<object, JsvTypeSerializer>.WriteObjectType
                    : GetWriteFn(type);

                var prevState = JsState.IsWritingDynamic;
                JsState.IsWritingDynamic = true;
                writeFn(writer, value);
                JsState.IsWritingDynamic = prevState;
            }
            finally
            {
                JsState.Depth--;
            }
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
	public static class JsvWriter<T>
	{
		private static WriteObjectDelegate CacheFn;
        
        public static void Reset()
        {
            JsvWriter.RemoveCacheFn(typeof(T));
            Refresh();
        }

	    public static void Refresh()
	    {
            if (JsvWriter.Instance == null)
                return;

            CacheFn = typeof(T) == typeof(object)
	            ? JsvWriter.WriteLateBoundObject
	            : JsvWriter.Instance.GetWriteFn<T>();
	    }

	    public static WriteObjectDelegate WriteFn()
		{
			return CacheFn ?? WriteObject;
		}

		static JsvWriter()
		{
		    CacheFn = typeof(T) == typeof(object) 
                ? JsvWriter.WriteLateBoundObject 
                : JsvWriter.Instance.GetWriteFn<T>();
		}

        public static void WriteObject(TextWriter writer, object value)
        {
#if __IOS__
			if (writer == null) return;
#endif
            TypeConfig<T>.AssertValidUsage();

            try
            {
                if (++JsState.Depth > JsConfig.MaxDepth)
                {
                    Tracer.Instance.WriteError("Exceeded MaxDepth limit of {0} attempting to serialize {1}"
                        .Fmt(JsConfig.MaxDepth, value.GetType().Name));
                    return;
                }

                CacheFn(writer, value);
            }
            finally 
            {
                JsState.Depth--;
            }
        }

        public static void WriteRootObject(TextWriter writer, object value)
        {
#if __IOS__
			if (writer == null) return;
#endif
            try
            {
                TypeConfig<T>.AssertValidUsage();
            }
            catch (Exception ex)
            {
                var inner = ex.GetInnerMostException();
                throw inner;
            }

            JsState.Depth = 0;
            CacheFn(writer, value);
        }

    }
}