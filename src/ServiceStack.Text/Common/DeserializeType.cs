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
using System.Linq.Expressions ;
#endif

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using ServiceStack.Text.Json;
using ServiceStack.Text.Support;

namespace ServiceStack.Text.Common
{
	internal static class DeserializeType<TSerializer>
		where TSerializer : ITypeSerializer
	{
		private static readonly ITypeSerializer Serializer = JsWriter.GetTypeSerializer<TSerializer>();

		private static readonly string TypeAttrInObject = Serializer.TypeAttrInObject;

		public static ParseStringDelegate GetParseMethod(Type type)
		{
			if (!type.IsClass) return null;

            var propertyInfos = type.GetSerializableProperties();
			if (propertyInfos.Length == 0)
			{
				var emptyCtorFn = ReflectionExtensions.GetConstructorMethodToCache(type);
				return value => emptyCtorFn();
			}


			var setterMap = new Dictionary<string, SetPropertyDelegate>();
			var map = new Dictionary<string, ParseStringDelegate>();

			foreach (var propertyInfo in propertyInfos)
			{
				map[propertyInfo.Name] = Serializer.GetParseFn(propertyInfo.PropertyType);
				setterMap[propertyInfo.Name] = GetSetPropertyMethod(type, propertyInfo);
			}

			var ctorFn = ReflectionExtensions.GetConstructorMethodToCache(type);
			return value => StringToType(type, value, ctorFn, setterMap, map);
		}

		public static object ObjectStringToType(string strType)
		{
			if (strType != null
				&& strType.Length >= TypeAttrInObject.Length
				&& strType.Substring(1, TypeAttrInObject.Length) == TypeAttrInObject)
			{
				var propIndex = TypeAttrInObject.Length;
				var typeName = Serializer.EatValue(strType, ref propIndex);
				typeName = Serializer.ParseString(typeName);
				var propType = AssemblyUtils.FindType(typeName);
				var parseFn = Serializer.GetParseFn(propType);
				var propertyValue = parseFn(strType);
				return propertyValue;
			}

			return strType;
		}

		private static object StringToType(Type type, string strType, 
           EmptyCtorDelegate ctorFn,
		   IDictionary<string, SetPropertyDelegate> setterMap,
		   IDictionary<string, ParseStringDelegate> parseStringFnMap)
		{
			var index = 0;
				
			if (strType == null)
					return null;
				
			if (!Serializer.EatMapStartChar(strType, ref index))
				throw new SerializationException(string.Format(
					"Type definitions should start with a '{0}', expecting serialized type '{1}', got string starting with: {2}",
					JsWriter.MapStartChar, type.Name, strType.Substring(0, strType.Length < 50 ? strType.Length : 50)));

			if (strType == JsWriter.EmptyMap) return ctorFn();

			object instance = null;
			string propertyName;
			ParseStringDelegate parseStringFn;
			SetPropertyDelegate setterFn;

			var strTypeLength = strType.Length;

			while (index < strTypeLength)
			{
				propertyName = Serializer.EatMapKey(strType, ref index);

				Serializer.EatMapKeySeperator(strType, ref index);

				var propertyValueString = Serializer.EatValue(strType, ref index);

				if (propertyName == JsWriter.TypeAttr)
				{
					var typeName = Serializer.ParseString(propertyValueString);
					instance = ReflectionExtensions.CreateInstance(typeName);
					if (instance == null)
						Tracer.Instance.WriteWarning("Could not find type: " + propertyValueString);
					Serializer.EatItemSeperatorOrMapEndChar(strType, ref index);
					continue;
				}

				if (instance == null) instance = ctorFn();

				if (propertyValueString != null 
				    && propertyValueString.Length >= TypeAttrInObject.Length
				    && propertyValueString.Substring(0, TypeAttrInObject.Length) == TypeAttrInObject)
				{
					var propIndex = TypeAttrInObject.Length;
					var typeName = Serializer.EatValue(propertyValueString, ref propIndex);
					typeName = Serializer.ParseString(typeName);
					var propType = AssemblyUtils.FindType(typeName);
					if (propType == null)
					{
						Tracer.Instance.WriteWarning("Could not find type: " + typeName);
					}
					else
					{
						try
						{
							var parseFn = Serializer.GetParseFn(propType);
							var propertyValue = parseFn(propertyValueString);
	
							setterMap.TryGetValue(propertyName, out setterFn);
	
							if (setterFn != null)
							{
								setterFn(instance, propertyValue);
							}
						}
						catch (Exception)
						{
							Tracer.Instance.WriteWarning("WARN: failed to set dynamic property {0} with: {1}", propertyName, propertyValueString);
						}
					}

					Serializer.EatItemSeperatorOrMapEndChar(strType, ref index);
					continue;					
				}

				parseStringFnMap.TryGetValue(propertyName, out parseStringFn);

				if (parseStringFn != null)
				{
					try
					{
						var propertyValue = parseStringFn(propertyValueString);
	
						setterMap.TryGetValue(propertyName, out setterFn);
	
						if (setterFn != null)
						{
							setterFn(instance, propertyValue);
						}
					}
					catch (Exception)
					{
                        Tracer.Instance.WriteWarning("WARN: failed to set property {0} with: {1}", propertyName, propertyValueString);
					}
				}

				Serializer.EatItemSeperatorOrMapEndChar(strType, ref index);
			}

			return instance;
		}

		public static SetPropertyDelegate GetSetPropertyMethod(Type type, PropertyInfo propertyInfo)
		{
			var setMethodInfo = propertyInfo.GetSetMethod(true);
			if (setMethodInfo == null) return null;
			
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