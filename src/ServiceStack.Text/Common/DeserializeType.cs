//
// https://github.com/ServiceStack/ServiceStack.Text
// ServiceStack.Text: .NET C# POCO JSON, JSV and CSV Text Serializers.
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2012 ServiceStack, Inc. All Rights Reserved.
//
// Licensed under the same terms of ServiceStack.
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using ServiceStack.Text.Support;
#if NETSTANDARD2_0
using Microsoft.Extensions.Primitives;
#endif

namespace ServiceStack.Text.Common
{
    public static class DeserializeType<TSerializer>
        where TSerializer : ITypeSerializer
    {
        private static readonly ITypeSerializer Serializer = JsWriter.GetTypeSerializer<TSerializer>();

        internal static ParseStringDelegate GetParseMethod(TypeConfig typeConfig) => v => GetParseStringSegmentMethod(typeConfig)(new StringSegment(v));

        internal static ParseStringSegmentDelegate GetParseStringSegmentMethod(TypeConfig typeConfig)
        {
            var type = typeConfig.Type;

            if (!type.IsStandardClass()) return null;
            var map = DeserializeTypeRef.GetTypeAccessorMap(typeConfig, Serializer);

            var ctorFn = JsConfig.ModelFactory(type);
            if (map == null)
                return value => ctorFn();

            return typeof(TSerializer) == typeof(Json.JsonTypeSerializer)
                ? (ParseStringSegmentDelegate)(value => DeserializeTypeRefJson.StringToType(typeConfig, value, ctorFn, map))
                : value => DeserializeTypeRefJsv.StringToType(typeConfig, value, ctorFn, map);
        }

        public static object ObjectStringToType(string strType) => ObjectStringToType(new StringSegment(strType));

        public static object ObjectStringToType(StringSegment strType)
        {
            var type = ExtractType(strType);
            if (type != null)
            {
                var parseFn = Serializer.GetParseStringSegmentFn(type);
                var propertyValue = parseFn(strType);
                return propertyValue;
            }

            if (JsConfig.ConvertObjectTypesIntoStringDictionary && !strType.IsNullOrEmpty())
            {
                if (strType.GetChar(0) == JsWriter.MapStartChar)
                {
                    var dynamicMatch = DeserializeDictionary<TSerializer>.ParseDictionary<string, object>(strType, null, v => Serializer.UnescapeString(v).Value, v => Serializer.UnescapeString(v).Value);
                    if (dynamicMatch != null && dynamicMatch.Count > 0)
                    {
                        return dynamicMatch;
                    }
                }

                if (strType.GetChar(0) == JsWriter.ListStartChar)
                {
                    return DeserializeList<List<object>, TSerializer>.ParseStringSegment(strType);
                }
            }

            var primitiveType = JsConfig.TryToParsePrimitiveTypeValues ? ParsePrimitive(strType.Value) : null;
            if (primitiveType != null)
                return primitiveType;

            if (Serializer.ObjectDeserializer != null)
                return Serializer.ObjectDeserializer(strType);

            return Serializer.UnescapeString(strType).Value;
        }

        public static Type ExtractType(string strType) => ExtractType(new StringSegment(strType));

        //TODO: optimize ExtractType
        public static Type ExtractType(StringSegment strType)
        {
            if (!strType.HasValue || strType.Length <= 1) return null;

            var hasWhitespace = Json.JsonUtils.WhiteSpaceChars.Contains(strType.GetChar(1));
            if (hasWhitespace)
            {
                var pos = strType.IndexOf('"');
                if (pos >= 0)
                    strType = new StringSegment("{" + strType.Substring(pos, strType.Length - pos));
            }

            var typeAttrInObject = Serializer.TypeAttrInObject;
            if (strType.Length > typeAttrInObject.Length
                && strType.Substring(0, typeAttrInObject.Length) == typeAttrInObject)
            {
                var propIndex = typeAttrInObject.Length;
                var typeName = Serializer.UnescapeSafeString(Serializer.EatValue(strType, ref propIndex)).Value;

                var type = JsConfig.TypeFinder(typeName);

                JsWriter.AssertAllowedRuntimeType(type);

                if (type == null)
                {
                    Tracer.Instance.WriteWarning("Could not find type: " + typeName);
                    return null;
                }

                return PclExport.Instance.UseType(type);
            }
            return null;
        }

        public static object ParseAbstractType<T>(string value) => ParseAbstractType<T>(new StringSegment(value));

        public static object ParseAbstractType<T>(StringSegment value)
        {
            if (typeof(T).IsAbstract)
            {
                if (value.IsNullOrEmpty()) return null;
                var concreteType = ExtractType(value);
                if (concreteType != null)
                {
                    return Serializer.GetParseStringSegmentFn(concreteType)(value);
                }
                Tracer.Instance.WriteWarning(
                    "Could not deserialize Abstract Type with unknown concrete type: " + typeof(T).FullName);
            }
            return null;
        }

        public static object ParseQuotedPrimitive(string value)
        {
            var fn = JsConfig.ParsePrimitiveFn;
            var result = fn?.Invoke(value);
            if (result != null)
                return result;

            if (string.IsNullOrEmpty(value))
                return null;

            if (Guid.TryParse(value, out Guid guidValue)) return guidValue;

            if (value.StartsWith(DateTimeSerializer.EscapedWcfJsonPrefix, StringComparison.Ordinal) || value.StartsWith(DateTimeSerializer.WcfJsonPrefix, StringComparison.Ordinal))
                return DateTimeSerializer.ParseWcfJsonDate(value);

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

        public static object ParsePrimitive(string value) => ParsePrimitive(new StringSegment(value));

        public static object ParsePrimitive(StringSegment value)
        {
            var fn = JsConfig.ParsePrimitiveFn;
            var result = fn?.Invoke(value.Value);
            if (result != null)
                return result;

            if (value.IsNullOrEmpty())
                return null;

            if (value.TryParseBoolean(out bool boolValue))
                return boolValue;

            return value.ParseNumber();
        }

        internal static object ParsePrimitive(string value, char firstChar)
        {
            if (typeof(TSerializer) == typeof(Json.JsonTypeSerializer))
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
        internal ParseStringSegmentDelegate GetProperty;
        internal SetMemberDelegate SetProperty;
        internal Type PropertyType;

        public static Type ExtractType(ITypeSerializer Serializer, string strType)
            => ExtractType(Serializer, new StringSegment(strType));

        public static Type ExtractType(ITypeSerializer Serializer, StringSegment strType)
        {
            if (!strType.HasValue || strType.Length <= 1) return null;

            var hasWhitespace = Json.JsonUtils.WhiteSpaceChars.Contains(strType.GetChar(1));
            if (hasWhitespace)
            {
                var pos = strType.IndexOf('"');
                if (pos >= 0)
                    strType = new StringSegment("{" + strType.Substring(pos));
            }

            var typeAttrInObject = Serializer.TypeAttrInObject;
            if (strType.Length > typeAttrInObject.Length
                && strType.Substring(0, typeAttrInObject.Length) == typeAttrInObject)
            {
                var propIndex = typeAttrInObject.Length;
                var typeName = Serializer.EatValue(strType, ref propIndex).Value;
                var type = JsConfig.TypeFinder(typeName);

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
                GetProperty = GetPropertyMethod(serializer, propertyInfo),
                SetProperty = GetSetPropertyMethod(typeConfig, propertyInfo),
            };
        }

        internal static ParseStringSegmentDelegate GetPropertyMethod(ITypeSerializer serializer, PropertyInfo propertyInfo)
        {
            var getPropertyFn = serializer.GetParseStringSegmentFn(propertyInfo.PropertyType);
            if (propertyInfo.PropertyType == typeof(object) || 
                propertyInfo.PropertyType.HasInterface(typeof(IEnumerable<object>)))
            {
                var declaringTypeNamespace = propertyInfo.DeclaringType?.Namespace;
                if (declaringTypeNamespace == null || (!JsConfig.AllowRuntimeTypeInTypesWithNamespaces.Contains(declaringTypeNamespace)
                    && !JsConfig.AllowRuntimeTypeInTypes.Contains(propertyInfo.DeclaringType.FullName)))
                {
                    return value =>
                    {
                        var hold = JsState.IsRuntimeType;
                        try
                        {
                            JsState.IsRuntimeType = true;
                            return getPropertyFn(value);
                        }
                        finally
                        {
                            JsState.IsRuntimeType = hold;
                        }
                    };
                }
            }
            return getPropertyFn;
        }

        private static SetMemberDelegate GetSetPropertyMethod(TypeConfig typeConfig, PropertyInfo propertyInfo)
        {
            if (typeConfig.Type != propertyInfo.DeclaringType)
                propertyInfo = propertyInfo.DeclaringType.GetProperty(propertyInfo.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (!propertyInfo.CanWrite && !typeConfig.EnableAnonymousFieldSetters) return null;

            FieldInfo fieldInfo = null;
            if (!propertyInfo.CanWrite)
            {
                var fieldNameFormat = Env.IsMono ? "<{0}>" : "<{0}>i__Field";
                var fieldName = string.Format(fieldNameFormat, propertyInfo.Name);

                var fieldInfos = typeConfig.Type.GetWritableFields();
                foreach (var f in fieldInfos)
                {
                    if (f.IsInitOnly && f.FieldType == propertyInfo.PropertyType && f.Name.EqualsIgnoreCase(fieldName))
                    {
                        fieldInfo = f;
                        break;
                    }
                }

                if (fieldInfo == null) return null;
            }

            return propertyInfo.CanWrite
                ? PclExport.Instance.CreateSetter(propertyInfo)
                : PclExport.Instance.CreateSetter(fieldInfo);
        }

        public static TypeAccessor Create(ITypeSerializer serializer, TypeConfig typeConfig, FieldInfo fieldInfo)
        {
            return new TypeAccessor
            {
                PropertyType = fieldInfo.FieldType,
                GetProperty = serializer.GetParseStringSegmentFn(fieldInfo.FieldType),
                SetProperty = GetSetFieldMethod(typeConfig, fieldInfo),
            };
        }

        private static SetMemberDelegate GetSetFieldMethod(TypeConfig typeConfig, FieldInfo fieldInfo)
        {
            if (typeConfig.Type != fieldInfo.DeclaringType)
                fieldInfo = fieldInfo.DeclaringType.GetField(fieldInfo.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            return PclExport.Instance.CreateSetter(fieldInfo);
        }
    }

    public static class DeserializeTypeExensions
    {
        public static bool Has(this ParseAsType flags, ParseAsType flag)
        {
            return (flag & flags) != 0;
        }

        public static object ParseNumber(this StringSegment value) => ParseNumber(value, JsConfig.TryParseIntoBestFit);
        public static object ParseNumber(this StringSegment value, bool bestFit)
        {
            if (value.Length == 1)
            {
                int singleDigit = value.GetChar(0);
                if (singleDigit >= 48 || singleDigit <= 57) // 0 - 9
                {
                    var result = singleDigit - 48;
                    if (bestFit)
                        return (byte) result;
                    return result;
                }
            }

            // Parse as decimal
            var acceptDecimal = JsConfig.ParsePrimitiveFloatingPointTypes.Has(ParseAsType.Decimal);
            var isDecimal = value.TryParseDecimal(out decimal decimalValue);

            // Check if the number is an Primitive Integer type given that we have a decimal
            if (isDecimal && decimalValue == decimal.Truncate(decimalValue))
            {
                // Value is a whole number
                var parseAs = JsConfig.ParsePrimitiveIntegerTypes;
                if (parseAs.Has(ParseAsType.Byte) && decimalValue <= byte.MaxValue && decimalValue >= byte.MinValue)
                    return (byte)decimalValue;
                if (parseAs.Has(ParseAsType.SByte) && decimalValue <= sbyte.MaxValue && decimalValue >= sbyte.MinValue)
                    return (sbyte)decimalValue;
                if (parseAs.Has(ParseAsType.Int16) && decimalValue <= Int16.MaxValue && decimalValue >= Int16.MinValue)
                    return (Int16)decimalValue;
                if (parseAs.Has(ParseAsType.UInt16) && decimalValue <= UInt16.MaxValue && decimalValue >= UInt16.MinValue)
                    return (UInt16)decimalValue;
                if (parseAs.Has(ParseAsType.Int32) && decimalValue <= Int32.MaxValue && decimalValue >= Int32.MinValue)
                    return (Int32)decimalValue;
                if (parseAs.Has(ParseAsType.UInt32) && decimalValue <= UInt32.MaxValue && decimalValue >= UInt32.MinValue)
                    return (UInt32)decimalValue;
                if (parseAs.Has(ParseAsType.Int64) && decimalValue <= Int64.MaxValue && decimalValue >= Int64.MinValue)
                    return (Int64)decimalValue;
                if (parseAs.Has(ParseAsType.UInt64) && decimalValue <= UInt64.MaxValue && decimalValue >= UInt64.MinValue)
                    return (UInt64)decimalValue;
                return decimalValue;
            }

            // Value is a floating point number

            // Return a decimal if the user accepts a decimal
            if (isDecimal && acceptDecimal)
                return decimalValue;

            var acceptFloat = JsConfig.ParsePrimitiveFloatingPointTypes.HasFlag(ParseAsType.Single);
            var isFloat = value.TryParseFloat(out float floatValue);
            if (acceptFloat && isFloat)
                return floatValue;

            var acceptDouble = JsConfig.ParsePrimitiveFloatingPointTypes.HasFlag(ParseAsType.Double);
            var isDouble = value.TryParseDouble(out double doubleValue);
            if (acceptDouble && isDouble)
                return doubleValue;

            if (isDecimal)
                return decimalValue;
            if (isFloat)
                return floatValue;
            if (isDouble)
                return doubleValue;

            return null;
        }
    }
}
