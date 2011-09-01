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

namespace ServiceStack.Text.Common
{
	internal static class DeserializeType<TSerializer>
		where TSerializer : ITypeSerializer
	{
		private static readonly ITypeSerializer Serializer = JsWriter.GetTypeSerializer<TSerializer>();

		public static ParseStringDelegate GetParseMethod(Type type)
		{
			if (!type.IsClass) return null;

			var propertyInfos = type.GetProperties();
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

		private static object StringToType(Type type, string strType, 
           EmptyCtorDelegate ctorFn,
		   IDictionary<string, SetPropertyDelegate> setterMap,
		   IDictionary<string, ParseStringDelegate> parseStringFnMap)
		{
			var index = 0;

			if (!Serializer.EatMapStartChar(strType, ref index))
				throw new SerializationException(string.Format(
					"Type definitions should start with a '{0}', expecting serialized type '{1}', got string starting with: {2}",
					JsWriter.MapStartChar, type.Name, strType.Substring(0, strType.Length < 50 ? strType.Length : 50)));


			if (strType == JsWriter.EmptyMap)
			{
				return ctorFn();
			}

			var strTypeLength = strType.Length;

			var propertyNamesAndValues = new Dictionary<string, string>();

			while (index < strTypeLength)
			{
				string propertyName = Serializer.EatMapKey(strType, ref index) ;

				Serializer.EatMapKeySeperator(strType, ref index);

				var propertyValueString = Serializer.EatValue(strType, ref index);

				propertyNamesAndValues.Add( propertyName, propertyValueString );

				Serializer.EatItemSeperatorOrMapEndChar(strType, ref index);
			}

			object instance ;

			if (propertyNamesAndValues.ContainsKey(@"__type"))
			{
				instance = typeFromTypeSpecifiedInText( type, propertyNamesAndValues[@"__type"]);
			}
			else
			{
				instance = ctorFn( ) ;
			}

			foreach( var pair in propertyNamesAndValues )
			{
				string name = pair.Key ;
				string value = pair.Value ;

				ParseStringDelegate parseStringFn;
				parseStringFnMap.TryGetValue(name, out parseStringFn);

				if (parseStringFn != null)
				{
					var propertyValue = parseStringFn(value);

					SetPropertyDelegate setterFn;
					setterMap.TryGetValue(name, out setterFn);

					if (setterFn != null)
					{
						setterFn(instance, propertyValue);
					}
				}
			}

			return instance;
		}

		static object typeFromTypeSpecifiedInText( Type baseType, string value )
		{
			int firstColon = value.IndexOf( @":" ) ;

			string className = value.Substring( 0, firstColon ) ;
			string namespaceName = value.Substring( firstColon + 2 ) ;

			string fullName = @"{0}.{1},{2}".FormatWith( namespaceName, className, baseType.Assembly.FullName) ;

			Type type = Type.GetType( fullName ) ;

			return Activator.CreateInstance( type ) ;
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