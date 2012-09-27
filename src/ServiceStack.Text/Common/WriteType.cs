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
using System.IO;
using ServiceStack.Text.Json;
using ServiceStack.Text.Reflection;
using System.Linq;
using System.Runtime.Serialization;

namespace ServiceStack.Text.Common
{
	internal static class WriteType<T, TSerializer>
		where TSerializer : ITypeSerializer
	{
		private static readonly ITypeSerializer Serializer = JsWriter.GetTypeSerializer<TSerializer>();

		private static readonly WriteObjectDelegate CacheFn;
		internal static TypePropertyWriter[] PropertyWriters;
		private static readonly WriteObjectDelegate WriteTypeInfo;

		private static bool IsIncluded
		{
			get { return (JsConfig.IncludeTypeInfo || JsConfig<T>.IncludeTypeInfo); }
		}
		private static bool IsExcluded
		{
			get { return (JsConfig.ExcludeTypeInfo || JsConfig<T>.ExcludeTypeInfo); }
		}

		static WriteType()
		{
			CacheFn = Init() ? GetWriteFn() : WriteEmptyType;

			if (IsIncluded)
			{
				WriteTypeInfo = TypeInfoWriter;
			}
			if (typeof(T).IsAbstract)
			{
				WriteTypeInfo = TypeInfoWriter;
				if (!typeof(T).IsInterface)
				{
					CacheFn = WriteAbstractProperties;
				}
			}
		}

		public static void TypeInfoWriter(TextWriter writer, object obj)
		{
			TryWriteTypeInfo(writer, obj);
		}

		private static bool ShouldSkipType() { return IsExcluded && !IsIncluded; }

		private static bool TryWriteSelfType (TextWriter writer) {
			if (ShouldSkipType()) return false;

			Serializer.WriteRawString(writer, JsConfig.TypeAttr);
			writer.Write(JsWriter.MapKeySeperator);
			Serializer.WriteRawString(writer, JsConfig.TypeWriter(typeof(T)));
			return true;
		}

		private static bool TryWriteTypeInfo(TextWriter writer, object obj)
		{
			if (obj == null || ShouldSkipType()) return false;

			Serializer.WriteRawString(writer, JsConfig.TypeAttr);
			writer.Write(JsWriter.MapKeySeperator);
			Serializer.WriteRawString(writer, JsConfig.TypeWriter(obj.GetType()));
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
			if (!typeof(T).IsClass && !typeof(T).IsInterface && !JsConfig.TreatAsRefType(typeof(T))) return false;

			var propertyInfos = TypeConfig<T>.Properties;
            var propertyNamesLength = propertyInfos.Length;
            PropertyWriters = new TypePropertyWriter[propertyNamesLength];

            if (propertyNamesLength == 0 && !JsState.IsWritingDynamic)
			{
				return typeof(T).IsDto();
			}

			// NOTE: very limited support for DataContractSerialization (DCS)
			//	NOT supporting Serializable
			//	support for DCS is intended for (re)Name of properties and Ignore by NOT having a DataMember present
			var isDataContract = typeof(T).GetCustomAttributes(typeof(DataContractAttribute), false).Any();
			for (var i = 0; i < propertyNamesLength; i++)
			{
				var propertyInfo = propertyInfos[i];

				string propertyName, propertyNameCLSFriendly, propertyNameLowercaseUnderscore;

				if (isDataContract)
				{
					var dcsDataMember = propertyInfo.GetCustomAttributes(typeof(DataMemberAttribute), false).FirstOrDefault() as DataMemberAttribute;
					if (dcsDataMember == null) continue;

					propertyName = dcsDataMember.Name ?? propertyInfo.Name;
					propertyNameCLSFriendly = dcsDataMember.Name ?? propertyName.ToCamelCase();
				    propertyNameLowercaseUnderscore = dcsDataMember.Name ?? propertyName.ToLowercaseUnderscore();
				}
				else
				{
					propertyName = propertyInfo.Name;
					propertyNameCLSFriendly = propertyName.ToCamelCase();
                    propertyNameLowercaseUnderscore = propertyName.ToLowercaseUnderscore();
				}

			    var propertyType = propertyInfo.PropertyType;
			    var suppressDefaultValue = propertyType.IsValueType && JsConfig.HasSerializeFn.Contains(propertyType)
			        ? propertyType.GetDefaultValue()
			        : null;

				PropertyWriters[i] = new TypePropertyWriter
				(
					propertyName,
					propertyNameCLSFriendly,
                    propertyNameLowercaseUnderscore,
					propertyInfo.GetValueGetter<T>(),
                    Serializer.GetWriteFn(propertyType),
                    suppressDefaultValue
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
				               ? propertyNameCLSFriendly
				               : (JsConfig.EmitLowercaseUnderscoreNames)
				                     ? propertyNameLowercaseUnderscore
				                     : propertyName;
				}
			}
			internal readonly string propertyName;
			internal readonly string propertyNameCLSFriendly;
            internal readonly string propertyNameLowercaseUnderscore;
			internal readonly Func<T, object> GetterFn;
            internal readonly WriteObjectDelegate WriteFn;
            internal readonly object DefaultValue;

			public TypePropertyWriter(string propertyName, string propertyNameCLSFriendly, string propertyNameLowercaseUnderscore,
				Func<T, object> getterFn, WriteObjectDelegate writeFn, object defaultValue)
			{
				this.propertyName = propertyName;
				this.propertyNameCLSFriendly = propertyNameCLSFriendly;
			    this.propertyNameLowercaseUnderscore = propertyNameLowercaseUnderscore;
				this.GetterFn = getterFn;
				this.WriteFn = writeFn;
			    this.DefaultValue = defaultValue;
			}
		}

		public static void WriteEmptyType(TextWriter writer, object value)
		{
			writer.Write(JsWriter.EmptyMap);
		}

		public static void WriteAbstractProperties(TextWriter writer, object value)
		{
			if (value == null)
			{
				writer.Write(JsWriter.EmptyMap);
				return;
			}
			var valueType = value.GetType();
			if (valueType.IsAbstract)
			{
				WriteProperties(writer, value);
				return;
			}

			var writeFn = Serializer.GetWriteFn(valueType);			
			if (!JsConfig<T>.ExcludeTypeInfo) JsState.IsWritingDynamic = true;
			writeFn(writer, value);
			if (!JsConfig<T>.ExcludeTypeInfo) JsState.IsWritingDynamic = false;
		}
		 
		public static void WriteProperties(TextWriter writer, object value)
		{
			if (typeof(TSerializer) == typeof(JsonTypeSerializer) && JsState.WritingKeyCount > 0)
				writer.Write(JsWriter.QuoteChar);

			writer.Write(JsWriter.MapStartChar);

			var i = 0;
			if (WriteTypeInfo != null || JsState.IsWritingDynamic)
			{
				if (JsConfig.PreferInterfaces && TryWriteSelfType(writer)) i++;
				else if (TryWriteTypeInfo(writer, value)) i++;
			}

			if (PropertyWriters != null)
			{
				var len = PropertyWriters.Length;
				for (int index = 0; index < len; index++)
				{
					var propertyWriter = PropertyWriters[index];
					var propertyValue = value != null 
						? propertyWriter.GetterFn((T)value)
						: null;

					if ((propertyValue == null
					     || (propertyWriter.DefaultValue != null && propertyWriter.DefaultValue.Equals(propertyValue)))
                        && !Serializer.IncludeNullValues) continue;

					if (i++ > 0)
						writer.Write(JsWriter.ItemSeperator);

					Serializer.WritePropertyName(writer, propertyWriter.PropertyName);
					writer.Write(JsWriter.MapKeySeperator);

					if (typeof (TSerializer) == typeof (JsonTypeSerializer)) JsState.IsWritingValue = true;
					if (propertyValue == null)
					{
						writer.Write(JsonUtils.Null);
					}
					else
					{
						propertyWriter.WriteFn(writer, propertyValue);
					}
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
