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
using System.IO;
using ServiceStack.Text.Json;
using ServiceStack.Text.Reflection;
using System.Linq;
using System.Runtime.Serialization;

namespace ServiceStack.Text.Common
{
    //internal static class WriteType
    //{
    //    private static WriteObjectDelegate GetWriteFn(Type type)
    //    {
    //        return WriteProperties;
    //    }
    //}

	internal static class WriteType<T, TSerializer>
		where TSerializer : ITypeSerializer
	{
		private static readonly ITypeSerializer Serializer = JsWriter.GetTypeSerializer<TSerializer>();

		private static readonly WriteObjectDelegate CacheFn;
		internal static TypePropertyWriter[] PropertyWriters;
		private static WriteObjectDelegate WriteTypeInfo;
		
		static WriteType()
		{
			CacheFn = Init() ? GetWriteFn() : WriteEmptyType;

			if (typeof(T).IsAbstract)
			{
				WriteTypeInfo = TypeInfoWriter;
			}
		}

		public static void TypeInfoWriter(TextWriter writer, object obj)
		{
			DidWriteTypeInfo(writer, obj);
		}

		private static bool DidWriteTypeInfo(TextWriter writer, object obj)
		{
			if (obj == null
			    || JsConfig.ExcludeTypeInfo
			    || JsConfig<T>.ExcludeTypeInfo) return false;

			Serializer.WriteRawString(writer, JsWriter.TypeAttr);
			writer.Write(JsWriter.MapKeySeperator);
			Serializer.WriteRawString(writer, obj.GetType().ToTypeString());
			return true;
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
			if (propertyInfos.Length == 0 && !JsState.IsWritingDynamic)
			{
				return typeof(T).IsDto();
			}

			var propertyNamesLength = propertyInfos.Length;

			PropertyWriters = new TypePropertyWriter[propertyNamesLength];

			// NOTE: very limited support for DataContractSerialization (DCS)
			//	NOT supporting Serializable
			//	support for DCS is intended for (re)Name of properties and Ignore by NOT having a DataMember present
			bool isDcsContract = typeof(T).GetCustomAttributes(typeof(DataContractAttribute), false).Any();
			for (var i = 0; i < propertyNamesLength; i++)
			{
				var propertyInfo = propertyInfos[i];

				string propertyName;
				if(isDcsContract)
				{
					var dcsDataMember = propertyInfo.GetCustomAttributes(typeof(DataMemberAttribute), false).FirstOrDefault() as DataMemberAttribute;
					if (dcsDataMember == null)
					{
						continue;
					}
					
					propertyName = dcsDataMember.Name ?? propertyInfo.Name;
				}
				else
				{
					propertyName = propertyInfo.Name;
				}
				var propertyNameCLSFriendly = JsonUtils.MemberNameToCamelCase(propertyInfo.Name);
				
				PropertyWriters[i] = new TypePropertyWriter
				(
					propertyName,
					propertyNameCLSFriendly,
					propertyInfo.GetValueGetter<T>(),
					Serializer.GetWriteFn(propertyInfo.PropertyType)
				);
			}

			return true;
		}

		internal struct TypePropertyWriter
		{
			internal string PropertyName
			{
				get
				{
					return (JsConfig.EmitCamelCaseNames)
						? _propertyNameCLSFriendly
						: _propertyName;
				}
			}
			internal readonly string _propertyName;
			internal readonly string _propertyNameCLSFriendly;
			internal readonly Func<T, object> GetterFn;
			internal readonly WriteObjectDelegate WriteFn;

			public TypePropertyWriter(string propertyName, string propertyNameCLSFriendly,
				Func<T, object> getterFn, WriteObjectDelegate writeFn)
			{
				this._propertyName = propertyName;
				this._propertyNameCLSFriendly = propertyNameCLSFriendly;
				this.GetterFn = getterFn;
				this.WriteFn = writeFn;
			}
		}

		public static void WriteEmptyType(TextWriter writer, object value)
		{
			writer.Write(JsWriter.EmptyMap);
		}

		public static void WriteProperties(TextWriter writer, object value)
		{
			if (typeof(TSerializer) == typeof(JsonTypeSerializer) && JsState.WritingKeyCount > 0) 
				writer.Write(JsWriter.QuoteChar);

			writer.Write(JsWriter.MapStartChar);

            var i = 0;
			if (WriteTypeInfo != null || JsState.IsWritingDynamic)
			{
				if (DidWriteTypeInfo(writer, value)) i++;
			}

			if (PropertyWriters != null)
			{
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

		public static void WriteQueryString(TextWriter writer, object value)
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
