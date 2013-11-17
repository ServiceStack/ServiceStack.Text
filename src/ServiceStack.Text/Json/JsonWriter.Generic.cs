//Copyright (c) Service Stack LLC. All Rights Reserved.
//License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ServiceStack.Text.Common;

namespace ServiceStack.Text.Json
{
	internal static class JsonWriter
	{
		public static readonly JsWriter<JsonTypeSerializer> Instance = new JsWriter<JsonTypeSerializer>();
#if   PLATFORM_NO_USE_INTERLOCKED_COMPARE_EXCHANGE_T 
		

        private static  object _writeFnCache = new Dictionary<Type, WriteObjectDelegate>();

        private static Dictionary<Type, WriteObjectDelegate> WriteFnCache {

            get { return  (Dictionary<Type, WriteObjectDelegate> ) _writeFnCache; }

        }

		
		
#else
	
		
			
		private static Dictionary<Type, WriteObjectDelegate> WriteFnCache = new Dictionary<Type, WriteObjectDelegate>();
	
		
#endif

        internal static void RemoveCacheFn(Type forType)
        {
            Dictionary<Type, WriteObjectDelegate> snapshot, newCache;
            do
            {
                snapshot = WriteFnCache;
                newCache = new Dictionary<Type, WriteObjectDelegate>(WriteFnCache);
                newCache.Remove(forType);
                
            } while (!ReferenceEquals(					
#if   PLATFORM_NO_USE_INTERLOCKED_COMPARE_EXCHANGE_T 
	
		Interlocked.CompareExchange(ref _writeFnCache, ( object )newCache, ( object )snapshot), snapshot));			
#else
			
			
                Interlocked.CompareExchange(ref WriteFnCache, newCache, snapshot), snapshot));
					
					
#endif
        }

	    internal static WriteObjectDelegate GetWriteFn(Type type)
		{
			try
			{
				WriteObjectDelegate writeFn;
				if (WriteFnCache.TryGetValue(type, out writeFn)) return writeFn;

				var genericType = typeof(JsonWriter<>).MakeGenericType(type);
                var mi = genericType.GetPublicStaticMethod("WriteFn");
                var writeFactoryFn = (Func<WriteObjectDelegate>)mi.MakeDelegate(typeof(Func<WriteObjectDelegate>));
                writeFn = writeFactoryFn();

				Dictionary<Type, WriteObjectDelegate> snapshot, newCache;
				do
				{
					snapshot = WriteFnCache;
					newCache = new Dictionary<Type, WriteObjectDelegate>(WriteFnCache);
					newCache[type] = writeFn;

				} while (!ReferenceEquals(					
#if   PLATFORM_NO_USE_INTERLOCKED_COMPARE_EXCHANGE_T 
	
				
					Interlocked.CompareExchange(ref _writeFnCache, ( object )newCache, ( object )snapshot), snapshot));	
#else
			
					Interlocked.CompareExchange(ref WriteFnCache, newCache, snapshot), snapshot));
					
					
#endif
			

				return writeFn;
			}
			catch (Exception ex)
			{
				Tracer.Instance.WriteError(ex);
				throw;
			}
		}
#if   PLATFORM_NO_USE_INTERLOCKED_COMPARE_EXCHANGE_T 
		
        private static object _jsonTypeInfoCache = new Dictionary<Type, TypeInfo>();

        private static Dictionary<Type, TypeInfo> JsonTypeInfoCache {

            get {  return  ( Dictionary<Type, TypeInfo> ) _jsonTypeInfoCache  ;}

        }
		
		
#else
	
		private static Dictionary<Type, TypeInfo> JsonTypeInfoCache = new Dictionary<Type, TypeInfo>();
		
		
		
#endif
		

	    internal static TypeInfo GetTypeInfo(Type type)
		{
			try
			{
				TypeInfo writeFn;
				if (JsonTypeInfoCache.TryGetValue(type, out writeFn)) return writeFn;

				var genericType = typeof(JsonWriter<>).MakeGenericType(type);
                var mi = genericType.GetPublicStaticMethod("GetTypeInfo");
                var writeFactoryFn = (Func<TypeInfo>)mi.MakeDelegate(typeof(Func<TypeInfo>));
                writeFn = writeFactoryFn();

				Dictionary<Type, TypeInfo> snapshot, newCache;
				do
				{
					snapshot = JsonTypeInfoCache;
					newCache = new Dictionary<Type, TypeInfo>(JsonTypeInfoCache);
					newCache[type] = writeFn;

				} while (!ReferenceEquals(					
#if   PLATFORM_NO_USE_INTERLOCKED_COMPARE_EXCHANGE_T 
	
					Interlocked.CompareExchange(ref _jsonTypeInfoCache, ( object ) newCache, ( object )snapshot), snapshot));
#else
			
					Interlocked.CompareExchange(ref JsonTypeInfoCache, newCache, snapshot), snapshot));
					
					
#endif
			

				return writeFn;
			}
			catch (Exception ex)
			{
				Tracer.Instance.WriteError(ex);
				throw;
			}
		}

	    internal static void WriteLateBoundObject(TextWriter writer, object value)
		{
			if (value == null)
			{
				writer.Write(JsonUtils.Null);
				return;
			}

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
				    ? WriteType<object, JsonTypeSerializer>.WriteObjectType
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

	    internal static WriteObjectDelegate GetValueTypeToStringMethod(Type type)
		{
			return Instance.GetValueTypeToStringMethod(type);
		}
	}

	internal class TypeInfo
	{
        internal bool EncodeMapKey;
        internal bool IsNumeric;
    }

	/// <summary>
	/// Implement the serializer using a more static approach
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal static class JsonWriter<T>
	{
		internal static TypeInfo TypeInfo;
		private static WriteObjectDelegate CacheFn;

        public static void Reset()
        {
            JsonWriter.RemoveCacheFn(typeof(T));

            CacheFn = typeof(T) == typeof(object) 
                ? JsonWriter.WriteLateBoundObject 
                : JsonWriter.Instance.GetWriteFn<T>();
        }

		public static WriteObjectDelegate WriteFn()
		{
			return CacheFn ?? WriteObject;
		}

		public static TypeInfo GetTypeInfo()
		{
			return TypeInfo;
		}

		static JsonWriter()
		{
		    var isNumeric = typeof(T).IsNumericType();
			TypeInfo = new TypeInfo {
                EncodeMapKey = typeof(T) == typeof(bool) || isNumeric,
                IsNumeric = isNumeric
			};

            CacheFn = typeof(T) == typeof(object) 
                ? JsonWriter.WriteLateBoundObject 
                : JsonWriter.Instance.GetWriteFn<T>();
		}

        public static void WriteObject(TextWriter writer, object value)
        {
#if MONOTOUCH     ||  ( UNITY3D  && PLATFORM_USE_AOT  )
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
#if MONOTOUCH   ||  ( UNITY3D  && PLATFORM_USE_AOT  )
			if (writer == null) return;
#endif
            TypeConfig<T>.AssertValidUsage();

            JsState.Depth = 0;
            CacheFn(writer, value);
        }
    }

}