//
// https://github.com/ServiceStack/ServiceStack.Text
// ServiceStack.Text: .NET C# POCO JSON, JSV and CSV Text Serializers.
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2012 Service Stack LLC. All Rights Reserved.
//
// Licensed under the same terms of ServiceStack.
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using ServiceStack.Text.Json;

namespace ServiceStack.Text.Common
{
    public static class DeserializeType<TSerializer>
        where TSerializer : ITypeSerializer
    {
        private static readonly ITypeSerializer Serializer = JsWriter.GetTypeSerializer<TSerializer>();

        internal static ParseStringDelegate GetParseMethod(TypeConfig typeConfig)
        {
            var type = typeConfig.Type;

            if (!type.IsStandardClass()) return null;
            var map = DeserializeTypeRef.GetTypeAccessorMap(typeConfig, Serializer);

            var ctorFn = JsConfig.ModelFactory(type);
            if (map == null)
                return value => ctorFn();

            return typeof(TSerializer) == typeof(Json.JsonTypeSerializer)
                ? (ParseStringDelegate)(value => DeserializeTypeRefJson.StringToType(type, value, ctorFn, map))
                : value => DeserializeTypeRefJsv.StringToType(type, value, ctorFn, map);
        }

        public static object ObjectStringToType(string strType)
        {
            var type = ExtractType(strType);
            if (type != null)
            {
                var parseFn = Serializer.GetParseFn(type);
                var propertyValue = parseFn(strType);
                return propertyValue;
            }

            if (JsConfig.ConvertObjectTypesIntoStringDictionary && !string.IsNullOrEmpty(strType))
            {
                if (strType[0] == JsWriter.MapStartChar)
                {
                    var dynamicMatch = DeserializeDictionary<TSerializer>.ParseDictionary<string, object>(strType, null, Serializer.UnescapeString, Serializer.UnescapeString);
                    if (dynamicMatch != null && dynamicMatch.Count > 0)
                    {
                        return dynamicMatch;
                    }
                }

                if (strType[0] == JsWriter.ListStartChar)
                {
                    return DeserializeList<List<object>, TSerializer>.Parse(strType);
                }
            }

            return Serializer.UnescapeString(strType);
        }

        public static Type ExtractType(string strType)
        {
            var typeAttrInObject = Serializer.TypeAttrInObject;
            if (strType != null
                && strType.Length > typeAttrInObject.Length
                && strType.Substring(0, typeAttrInObject.Length) == typeAttrInObject)
            {
                var propIndex = typeAttrInObject.Length;
                var typeName = Serializer.UnescapeSafeString(Serializer.EatValue(strType, ref propIndex));

                var type = JsConfig.TypeFinder.Invoke(typeName);

                if (type == null)
                {
                    Tracer.Instance.WriteWarning("Could not find type: " + typeName);
                    return null;
                }

                return PclExport.Instance.UseType(type);
            }
            return null;
        }

        public static object ParseAbstractType<T>(string value)
        {
            if (typeof(T).IsAbstract())
            {
                if (string.IsNullOrEmpty(value)) return null;
                var concreteType = ExtractType(value);
                if (concreteType != null)
                {
                    return Serializer.GetParseFn(concreteType)(value);
                }
                Tracer.Instance.WriteWarning(
                    "Could not deserialize Abstract Type with unknown concrete type: " + typeof(T).FullName);
            }
            return null;
        }

        public static object ParseQuotedPrimitive(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;

            Guid guidValue;
            if (Guid.TryParse(value, out guidValue)) return guidValue;

            if (value.StartsWith(DateTimeSerializer.EscapedWcfJsonPrefix, StringComparison.Ordinal) || value.StartsWith(DateTimeSerializer.WcfJsonPrefix, StringComparison.Ordinal))
            {
                return DateTimeSerializer.ParseWcfJsonDate(value);
            }

            if (JsConfig.DateHandler == DateHandler.ISO8601)
            {
                // check that we have UTC ISO8601 date:
                // YYYY-MM-DDThh:mm:ssZ
                // YYYY-MM-DDThh:mm:ss+02:00
                // YYYY-MM-DDThh:mm:ss-02:00
                if (value.Length > 14 && value[10] == 'T' &&
                    (value.EndsWithInvariant("Z")
                    || value[value.Length - 6] == '+'
                    || value[value.Length - 6] == '-'))
                {
                    return DateTimeSerializer.ParseShortestXsdDateTime(value);
                }
            }

            if (JsConfig.DateHandler == DateHandler.RFC1123)
            {
                // check that we have RFC1123 date:
                // ddd, dd MMM yyyy HH:mm:ss GMT
                if (value.Length == 29 && (value.EndsWithInvariant("GMT")))
                {
                    return DateTimeSerializer.ParseRFC1123DateTime(value);
                }
            }

            return Serializer.UnescapeString(value);
        }

        public static object ParsePrimitive(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;

            bool boolValue;
            if (bool.TryParse(value, out boolValue)) return boolValue;

			// Try parse as a decimal (all numbers should be reasonably represented as a decimal (-7.9 x 10^28 to 7.9 x 10^28) / (10^0 to 10^28))
			decimal decimalValue;
			if(!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimalValue))
				return null;

			// Determine if the number is whole or decimal
			if(decimalValue != decimal.Truncate(decimalValue))
			{
				// Value is a decimal number (Use a decimal type e.g decimal | double | float)
				if(JsConfig.ParseNumericDecimalNumberAsType == typeof(decimal))
					return decimalValue;

				if(JsConfig.ParseNumericDecimalNumberAsType == typeof(double)) {
					double doubleValue;
					return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out doubleValue) ? doubleValue : Convert.ToDouble(decimalValue);
				}

				if(JsConfig.ParseNumericDecimalNumberAsType == typeof(float)) {
					float floatValue;
					return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out floatValue) ? floatValue : Convert.ToSingle(decimalValue);
				}

			} else {

				// Value is a whole number (Use byte | sbyte | Int16 | UInt16 | Int32 | UInt32 | Int64 | UInt64 based on user preference) 
				foreach(var type in JsConfig.ParseNumericWholeNumberAsTypePreference ?? JsConfig.ParseNumericWholeNumberAsTypeDefaultOrder)
				{
					if (type == typeof(byte) && decimalValue <= byte.MaxValue && decimalValue >= byte.MinValue) return (byte)decimalValue;
					if (type == typeof(sbyte) && decimalValue <= sbyte.MaxValue && decimalValue >= sbyte.MinValue) return (sbyte)decimalValue;
					if (type == typeof(Int16) && decimalValue <= Int16.MaxValue && decimalValue >= Int16.MinValue) return (Int16)decimalValue;
					if (type == typeof(UInt16) && decimalValue <= UInt16.MaxValue && decimalValue >= UInt16.MinValue) return (UInt16)decimalValue;
					if (type == typeof(Int32) && decimalValue <= Int32.MaxValue && decimalValue >= Int32.MinValue) return (Int32)decimalValue;
					if (type == typeof(UInt32) && decimalValue <= UInt32.MaxValue && decimalValue >= UInt32.MinValue) return (UInt32)decimalValue;
					if (type == typeof(Int64) && decimalValue <= Int64.MaxValue && decimalValue >= Int64.MinValue) return (Int64)decimalValue;
					if (type == typeof(UInt64) && decimalValue <= UInt64.MaxValue && decimalValue >= UInt64.MinValue) return (UInt64)decimalValue;
				}
			}

			return null;
        }

        internal static object ParsePrimitive(string value, char firstChar)
        {
            if (typeof(TSerializer) == typeof(JsonTypeSerializer))
            {
                return firstChar == JsWriter.QuoteChar
                           ? ParseQuotedPrimitive(value)
                           : ParsePrimitive(value);
            }
            return (ParsePrimitive(value) ?? ParseQuotedPrimitive(value));
        }
    }

    internal class TypeAccessor
    {
        internal ParseStringDelegate GetProperty;
        internal SetPropertyDelegate SetProperty;
        internal Type PropertyType;

        public static Type ExtractType(ITypeSerializer Serializer, string strType)
        {
            var typeAttrInObject = Serializer.TypeAttrInObject;

            if (strType != null
                && strType.Length > typeAttrInObject.Length
                && strType.Substring(0, typeAttrInObject.Length) == typeAttrInObject)
            {
                var propIndex = typeAttrInObject.Length;
                var typeName = Serializer.EatValue(strType, ref propIndex);
                var type = JsConfig.TypeFinder.Invoke(typeName);

                if (type == null)
                    Tracer.Instance.WriteWarning("Could not find type: " + typeName);

                return type;
            }
            return null;
        }

        public static TypeAccessor Create(ITypeSerializer serializer, TypeConfig typeConfig, PropertyInfo propertyInfo)
        {
            return new TypeAccessor
            {
                PropertyType = propertyInfo.PropertyType,
                GetProperty = serializer.GetParseFn(propertyInfo.PropertyType),
                SetProperty = GetSetPropertyMethod(typeConfig, propertyInfo),
            };
        }

        private static SetPropertyDelegate GetSetPropertyMethod(TypeConfig typeConfig, PropertyInfo propertyInfo)
        {
            if (propertyInfo.ReflectedType() != propertyInfo.DeclaringType)
                propertyInfo = propertyInfo.DeclaringType.GetPropertyInfo(propertyInfo.Name);

            if (!propertyInfo.CanWrite && !typeConfig.EnableAnonymousFieldSetterses) return null;

            FieldInfo fieldInfo = null;
            if (!propertyInfo.CanWrite)
            {
                //TODO: What string comparison is used in SST?
                string fieldNameFormat = Env.IsMono ? "<{0}>" : "<{0}>i__Field";
                var fieldName = string.Format(fieldNameFormat, propertyInfo.Name);

                var fieldInfos = typeConfig.Type.GetWritableFields();
                foreach (var f in fieldInfos)
                {
                    if (f.IsInitOnly && f.FieldType == propertyInfo.PropertyType && f.Name == fieldName)
                    {
                        fieldInfo = f;
                        break;
                    }
                }

                if (fieldInfo == null) return null;
            }

            return PclExport.Instance.GetSetMethod(propertyInfo, fieldInfo);
        }

        internal static SetPropertyDelegate GetSetPropertyMethod(Type type, PropertyInfo propertyInfo)
        {
            if (!propertyInfo.CanWrite || propertyInfo.GetIndexParameters().Any()) return null;

            return PclExport.Instance.GetSetPropertyMethod(propertyInfo);
        }

        internal static SetPropertyDelegate GetSetFieldMethod(Type type, FieldInfo fieldInfo)
        {
            return PclExport.Instance.GetSetFieldMethod(fieldInfo);
        }

        public static TypeAccessor Create(ITypeSerializer serializer, TypeConfig typeConfig, FieldInfo fieldInfo)
        {
            return new TypeAccessor
            {
                PropertyType = fieldInfo.FieldType,
                GetProperty = serializer.GetParseFn(fieldInfo.FieldType),
                SetProperty = GetSetFieldMethod(typeConfig, fieldInfo),
            };
        }

		private static SetPropertyDelegate GetSetFieldMethod(TypeConfig typeConfig, FieldInfo fieldInfo)
		{
            if (fieldInfo.ReflectedType() != fieldInfo.DeclaringType)
                fieldInfo = fieldInfo.DeclaringType.GetFieldInfo(fieldInfo.Name);

		    return PclExport.Instance.GetSetFieldMethod(fieldInfo);
		}
    }
}
