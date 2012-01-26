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
using System.Linq.Expressions;
#endif

using System;
using System.Collections.Generic;
using System.Reflection;

namespace ServiceStack.Text.Common
{
	internal static class DeserializeType<TSerializer>
		where TSerializer : ITypeSerializer
	{
		private static readonly ITypeSerializer Serializer = JsWriter.GetTypeSerializer<TSerializer>();

		private static readonly string TypeAttrInObject = Serializer.TypeAttrInObject;

		public static ParseStringDelegate GetParseMethod(Type type)
		{
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
				map[propertyInfo.Name] = TypeAccessor.Create(Serializer, type, propertyInfo);
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

		public static TypeAccessor Create(ITypeSerializer serializer, Type type, PropertyInfo propertyInfo)
		{
			return new TypeAccessor {
				GetProperty = serializer.GetParseFn(propertyInfo.PropertyType),
				SetProperty = GetSetPropertyMethod(type, propertyInfo),
			};
		}

		internal static SetPropertyDelegate GetSetPropertyMethod(Type type, PropertyInfo propertyInfo)
		{
			var setMethodInfo = propertyInfo.GetSetMethod(true);
			if (setMethodInfo == null || setMethodInfo.GetParameters().Length > 1) return null;            

#if SILVERLIGHT || MONOTOUCH || XBOX
			return (instance, value) => setMethodInfo.Invoke(instance, new[] {value});
#else
			var oInstanceParam = Expression.Parameter(typeof(object), "oInstanceParam");
			var oValueParam = Expression.Parameter(typeof(object), "oValueParam");

			var instanceParam = Expression.Convert(oInstanceParam, type);
			var useType = propertyInfo.PropertyType;

			var valueParam = Expression.Convert(oValueParam, useType);
			var exprCallPropertySetFn = Expression.Call(instanceParam, setMethodInfo, valueParam);

			var propertySetFn = Expression.Lambda<SetPropertyDelegate>
				(
					exprCallPropertySetFn,
					oInstanceParam,
					oValueParam
				).Compile();

			return propertySetFn;
#endif
		}
	}
}