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

#if !XBOX && !MONOTOUCH && !SILVERLIGHT
using System.Reflection.Emit;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using ServiceStack.Text.Json;

namespace ServiceStack.Text.Common
{
    internal static class DeserializeType<TSerializer>
        where TSerializer : ITypeSerializer
    {
        private static readonly ITypeSerializer Serializer = JsWriter.GetTypeSerializer<TSerializer>();

        public static ParseStringDelegate GetParseMethod(TypeConfig typeConfig)
        {
            var type = typeConfig.Type;

            if (!type.IsClass || type.IsAbstract || type.IsInterface) return null;

            var map = DeserializeTypeRef.GetTypeAccessorMap(typeConfig, Serializer);
            var ctorFn = ReflectionExtensions.GetConstructorMethodToCache(type);
            if (map == null) {
                return value => ctorFn();
            }

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

            if (JsConfig.ConvertObjectTypesIntoStringDictionary && !string.IsNullOrEmpty(strType)) {
                if (strType[0] == JsWriter.MapStartChar) {
                    var dynamicMatch = DeserializeDictionary<TSerializer>.ParseDictionary<string, object>(strType, null, Serializer.UnescapeString, Serializer.UnescapeString);
                    if (dynamicMatch != null && dynamicMatch.Count > 0) {
                        return dynamicMatch;
                    }
                }

                if (strType[0] == JsWriter.ListStartChar) {
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

				if (type == null) {
					Tracer.Instance.WriteWarning("Could not find type: " + typeName);
					return null;
				}

#if !SILVERLIGHT && !MONOTOUCH
				if (type.IsInterface || type.IsAbstract) {
					return DynamicProxy.GetInstanceFor(type).GetType();
				}
#endif

                return type;
            }
            return null;
        }

        public static object ParseAbstractType<T>(string value)
        {
            if (typeof(T).IsAbstract)
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

#if NET40
            Guid guidValue;
            if (Guid.TryParse(value, out guidValue)) return guidValue;
#endif
            if (value.StartsWith(DateTimeSerializer.EscapedWcfJsonPrefix) || value.StartsWith(DateTimeSerializer.WcfJsonPrefix)) {
                return DateTimeSerializer.ParseWcfJsonDate(value);
            }

            return Serializer.UnescapeString(value);
        }

        public static object ParsePrimitive(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;

            bool boolValue;
            if (bool.TryParse(value, out boolValue)) return boolValue;
            byte byteValue;
            if (byte.TryParse(value, out byteValue)) return byteValue;
            sbyte sbyteValue;
            if (sbyte.TryParse(value, out sbyteValue)) return sbyteValue;
            short shortValue;
            if (short.TryParse(value, out shortValue)) return shortValue;
            ushort ushortValue;
            if (ushort.TryParse(value, out ushortValue)) return ushortValue;
            int intValue;
            if (int.TryParse(value, out intValue)) return intValue;
            uint uintValue;
            if (uint.TryParse(value, out uintValue)) return uintValue;
            long longValue;
            if (long.TryParse(value, out longValue)) return longValue;
            ulong ulongValue;
            if (ulong.TryParse(value, out ulongValue)) return ulongValue;
            float floatValue;
            if (float.TryParse(value, out floatValue)) return floatValue;
            double doubleValue;
            if (double.TryParse(value, out doubleValue)) return doubleValue;
            decimal decimalValue;
            if (decimal.TryParse(value, out decimalValue)) return decimalValue;

            return null;
        }

        internal static object ParsePrimitive(string value, char firstChar)
        {
            if (typeof(TSerializer) == typeof(JsonTypeSerializer)) {
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
            if (!propertyInfo.CanWrite && !typeConfig.EnableAnonymousFieldSetterses) return null;

            FieldInfo fieldInfo = null;
            if (!propertyInfo.CanWrite)
            {
                //TODO: What string comparison is used in SST?
				string fieldNameFormat = Env.IsMono ? "<{0}>" : "<{0}>i__Field";
                var fieldName = string.Format(fieldNameFormat, propertyInfo.Name);
                var fieldInfos = typeConfig.Type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField);
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

#if SILVERLIGHT || MONOTOUCH || XBOX
            if (propertyInfo.CanWrite)
            {
                var setMethodInfo = propertyInfo.GetSetMethod(true);
                return (instance, value) => setMethodInfo.Invoke(instance, new[] { value });
            }
            if (fieldInfo == null) return null;
            return (instance, value) => fieldInfo.SetValue(instance, value);
#else
			return propertyInfo.CanWrite
				? CreateIlPropertySetter(propertyInfo)
				: CreateIlFieldSetter(fieldInfo);
#endif
        }

#if !SILVERLIGHT && !MONOTOUCH && !XBOX

		private static SetPropertyDelegate CreateIlPropertySetter(PropertyInfo propertyInfo)
		{
			var propSetMethod = propertyInfo.GetSetMethod(true);
			if (propSetMethod == null)
				return null;

			var setter = CreateDynamicSetMethod(propertyInfo);

			var generator = setter.GetILGenerator();
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
			generator.Emit(OpCodes.Ldarg_1);

			generator.Emit(propertyInfo.PropertyType.IsClass
				? OpCodes.Castclass
				: OpCodes.Unbox_Any,
				propertyInfo.PropertyType);

			generator.EmitCall(OpCodes.Callvirt, propSetMethod, (Type[])null);
			generator.Emit(OpCodes.Ret);

			return (SetPropertyDelegate)setter.CreateDelegate(typeof(SetPropertyDelegate));
		}

		private static SetPropertyDelegate CreateIlFieldSetter(FieldInfo fieldInfo)
		{
			var setter = CreateDynamicSetMethod(fieldInfo);

			var generator = setter.GetILGenerator();
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Castclass, fieldInfo.DeclaringType);
			generator.Emit(OpCodes.Ldarg_1);

			generator.Emit(fieldInfo.FieldType.IsClass
				? OpCodes.Castclass
				: OpCodes.Unbox_Any,
				fieldInfo.FieldType);

			generator.Emit(OpCodes.Stfld, fieldInfo);
			generator.Emit(OpCodes.Ret);

			return (SetPropertyDelegate)setter.CreateDelegate(typeof(SetPropertyDelegate));
		}

		private static DynamicMethod CreateDynamicSetMethod(MemberInfo memberInfo)
		{
			var args = new[] { typeof(object), typeof(object) };
			var name = string.Format("_{0}{1}_", "Set", memberInfo.Name);
			var returnType = typeof(void);

			return !memberInfo.DeclaringType.IsInterface
				? new DynamicMethod(name, returnType, args, memberInfo.DeclaringType, true)
				: new DynamicMethod(name, returnType, args, memberInfo.Module, true);
		}
#endif

        internal static SetPropertyDelegate GetSetPropertyMethod(Type type, PropertyInfo propertyInfo)
        {
            if (!propertyInfo.CanWrite || propertyInfo.GetIndexParameters().Any()) return null;

#if SILVERLIGHT || MONOTOUCH || XBOX
            var setMethodInfo = propertyInfo.GetSetMethod(true);
            return (instance, value) => setMethodInfo.Invoke(instance, new[] { value });
#else
			return CreateIlPropertySetter(propertyInfo);
#endif
        }
    }
}
