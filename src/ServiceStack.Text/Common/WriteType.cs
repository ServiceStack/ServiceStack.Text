﻿//
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
using System.IO;
using ServiceStack.Text.Json;
using ServiceStack.Text.Reflection ;


namespace ServiceStack.Text.Common
{
	internal static class WriteType<T, TSerializer>
		where TSerializer : ITypeSerializer
	{
		private static readonly ITypeSerializer Serializer = JsWriter.GetTypeSerializer<TSerializer>();

		private static readonly WriteObjectDelegate CacheFn;
		internal static TypePropertyWriter[] PropertyWriters;

		static WriteType()
		{
			CacheFn = Init() ? GetWriteFn() : WriteEmptyType;
		}

		public static WriteObjectDelegate Write
		{
			get { return CacheFn; }
		}

		private static WriteObjectDelegate GetWriteFn()
		{
			return WriteProperties;
		}

		private static bool Init()
		{
			if (!typeof(T).IsClass && !typeof(T).IsInterface) return false;

			var propertyInfos = TypeConfig<T>.Properties;
			if (propertyInfos.Length == 0)
			{
				return typeof(T).IsDto();
			}

			var propertyNamesLength = propertyInfos.Length;

			PropertyWriters = new TypePropertyWriter[propertyNamesLength];

			for (var i = 0; i < propertyNamesLength; i++)
			{
				var propertyInfo = propertyInfos[i];

				PropertyWriters[i] = new TypePropertyWriter
				(
					propertyInfo.Name,
					propertyInfo.GetValueGetter<T>(),
					Serializer.GetWriteFn(propertyInfo.PropertyType)
				);
			}

			return true;
		}

		internal struct TypePropertyWriter
		{
			internal readonly string PropertyName;
			internal readonly Func<T, object> GetterFn;
			internal readonly WriteObjectDelegate WriteFn;

			public TypePropertyWriter(string propertyName,
				Func<T, object> getterFn, WriteObjectDelegate writeFn)
			{
				this.PropertyName = propertyName;
				this.GetterFn = getterFn;
				this.WriteFn = writeFn;
			}
		}

		public static void WriteEmptyType(TextWriter writer, object value, bool includeType=false)
		{
			writer.Write(JsWriter.EmptyMap);
		}

		public static void WriteProperties(TextWriter writer, object value, bool includeType=false)
		{
			if (typeof(TSerializer) == typeof(JsonTypeSerializer) && JsState.WritingKeyCount > 0) 
				writer.Write(JsWriter.QuoteChar);

			writer.Write(JsWriter.MapStartChar);

			if( includeType )
			{
				writer.Write( @"{0}__type{0}:{0}{1}:#{2}{0},", JsWriter.QuoteChar,justTheTypeName(value), justTheNamespaceName(value) );
			}

			if (PropertyWriters != null)
			{
				var i = 0;
				foreach (var propertyWriter in PropertyWriters)
				{
					var propertyValue = propertyWriter.GetterFn((T)value);
					if (propertyValue == null && !JsConfig.IncludeNullValues) continue;

					if (i++ > 0)
						writer.Write(JsWriter.ItemSeperator);

					Serializer.WritePropertyName(writer, propertyWriter.PropertyName);
					writer.Write(JsWriter.MapKeySeperator);

					if (typeof(TSerializer) == typeof(JsonTypeSerializer)) JsState.IsWritingValue = true;
					propertyWriter.WriteFn(writer, propertyValue);
					if (typeof(TSerializer) == typeof(JsonTypeSerializer)) JsState.IsWritingValue = false;
				}
			}

			writer.Write(JsWriter.MapEndChar);

			if (typeof(TSerializer) == typeof(JsonTypeSerializer) && JsState.WritingKeyCount > 0)
				writer.Write(JsWriter.QuoteChar);
		}

		static string justTheNamespaceName( object value )
		{
			Type t = value.GetType( ) ;
			return t.Namespace ;
		}

		static string justTheTypeName( object value )
		{
			Type t = value.GetType( ) ;
			return t.Name ;
		}

		public static void WriteQueryString(TextWriter writer, object value, bool includeType=false)
		{
			var i = 0;
			foreach (var propertyWriter in PropertyWriters)
			{
				var propertyValue = propertyWriter.GetterFn((T)value);
				if (propertyValue == null) continue;
				var propertyValueString = propertyValue as string;
				if (propertyValueString != null)
				{
					propertyValue = propertyValueString.UrlEncode();
				}

				if (i++ > 0)
					writer.Write('&');

				Serializer.WritePropertyName(writer, propertyWriter.PropertyName);
				writer.Write('=');
				propertyWriter.WriteFn(writer, propertyValue);
			}
		}
	}
}
