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

namespace ServiceStack.Text.Common
{
    internal static class DeserializeType<TSerializer>
        where TSerializer : ITypeSerializer
    {
        private static readonly ITypeSerializer Serializer = JsWriter.GetTypeSerializer<TSerializer>();

        private static readonly string TypeAttrInObject = Serializer.TypeAttrInObject;

        public static ParseStringDelegate GetParseMethod(TypeConfig typeConfig)
        {
            var type = typeConfig.Type;

            if (!type.IsClass || type.IsAbstract || type.IsInterface) return null;

            var propertyInfos = type.GetSerializableProperties();
            if (propertyInfos.Length == 0)
            {
                var emptyCtorFn = ReflectionExtensions.GetConstructorMethodToCache(type);
                return value => emptyCtorFn();
            }

            var map = new Dictionary<string, TypeAccessor>(StringComparer.OrdinalIgnoreCase);

            var isDataContract = type.GetCustomAttributes(typeof(DataContractAttribute), false).Any();

            foreach (var propertyInfo in propertyInfos)
            {
                var propertyName = propertyInfo.Name;
                if (isDataContract)
                {
                    var dcsDataMember = propertyInfo.GetCustomAttributes(typeof(DataMemberAttribute), false).FirstOrDefault() as DataMemberAttribute;
                    if (dcsDataMember != null && dcsDataMember.Name != null)
                    {
                        propertyName = dcsDataMember.Name;
                    }
                }
                map[propertyName] = TypeAccessor.Create(Serializer, typeConfig, propertyInfo);
            }

            var ctorFn = ReflectionExtensions.GetConstructorMethodToCache(type);

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

            return Serializer.UnescapeString(strType);
        }

        public static Type ExtractType(string strType)
        {
            if (strType != null
				&& strType.Length > TypeAttrInObject.Length
				&& strType.Substring(0, TypeAttrInObject.Length) == TypeAttrInObject)
            {
                var propIndex = TypeAttrInObject.Length;
                var typeName = Serializer.UnescapeSafeString(Serializer.EatValue(strType, ref propIndex));
                var type = AssemblyUtils.FindType(typeName);

                if (type == null)
                    Tracer.Instance.WriteWarning("Could not find type: " + typeName);

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

    }

    internal class TypeAccessor
    {
        internal ParseStringDelegate GetProperty;
        internal SetPropertyDelegate SetProperty;

        public static Type ExtractType(ITypeSerializer Serializer, string strType)
        {
            var TypeAttrInObject = Serializer.TypeAttrInObject;

            if (strType != null
				&& strType.Length > TypeAttrInObject.Length
				&& strType.Substring(0, TypeAttrInObject.Length) == TypeAttrInObject)
            {
                var propIndex = TypeAttrInObject.Length;
                var typeName = Serializer.EatValue(strType, ref propIndex);
                var type = AssemblyUtils.FindType(typeName);

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
            if (!propertyInfo.CanWrite) return null;

#if SILVERLIGHT || MONOTOUCH || XBOX
            var setMethodInfo = propertyInfo.GetSetMethod(true);
            return (instance, value) => setMethodInfo.Invoke(instance, new[] { value });
#else
			return CreateIlPropertySetter(propertyInfo);
#endif
        }
    }
}
