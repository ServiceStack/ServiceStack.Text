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

#if !XBOX
using System.Linq;
#endif

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

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

			foreach (var propertyInfo in propertyInfos)
			{
				map[propertyInfo.Name] = TypeAccessor.Create(Serializer, typeConfig, propertyInfo);
			}

			var ctorFn = ReflectionExtensions.GetConstructorMethodToCache(type);

			return typeof(TSerializer) == typeof(Json.JsonTypeSerializer)
				? (ParseStringDelegate) (value => DeserializeTypeRefJson.StringToType(type, value, ctorFn, map))
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

			return strType;
		}

		public static Type ExtractType(string strType)
		{
			if (strType != null
				&& strType.Length > TypeAttrInObject.Length
				&& strType.Substring(0, TypeAttrInObject.Length) == TypeAttrInObject)
			{
				var propIndex = TypeAttrInObject.Length;
				var typeName = Serializer.EatValue(strType, ref propIndex);
				typeName = Serializer.ParseString(typeName);
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
				typeName = Serializer.ParseString(typeName);
				var type = AssemblyUtils.FindType(typeName);

				if (type == null)
					Tracer.Instance.WriteWarning("Could not find type: " + typeName);

				return type;
			}
			return null;
		}

		public static TypeAccessor Create(ITypeSerializer serializer, TypeConfig typeConfig, PropertyInfo propertyInfo)
		{
			return new TypeAccessor {
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
				var fieldName = string.Format("<{0}>i__Field", propertyInfo.Name);
				fieldInfo = typeConfig.Type
					.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField)
					.SingleOrDefault(f => f.IsInitOnly && f.FieldType == propertyInfo.PropertyType && f.Name == fieldName); 

				if (fieldInfo == null)
					return null;
			}

#if SILVERLIGHT || MONOTOUCH || XBOX
			return (instance, value) => setMethodInfo.Invoke(instance, new[] {value});
#else
			return propertyInfo.CanWrite 
				? CreateIlPropertySetter(propertyInfo) 
				: CreateIlFieldSetter(fieldInfo);
#endif
		}

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
					   ? new DynamicMethod(
							 name,
							 returnType,
							 args,
							 memberInfo.DeclaringType,
							 true)
					   : new DynamicMethod(
							 name,
							 returnType,
							 args,
							 memberInfo.Module,
							 true);
		}

		internal static SetPropertyDelegate GetSetPropertyMethod(Type type, PropertyInfo propertyInfo)
		{
			if (!propertyInfo.CanWrite) return null;

#if SILVERLIGHT || MONOTOUCH || XBOX
			return (instance, value) => setMethodInfo.Invoke(instance, new[] {value});
#else
			return CreateIlPropertySetter(propertyInfo);
#endif
		}
	}
}