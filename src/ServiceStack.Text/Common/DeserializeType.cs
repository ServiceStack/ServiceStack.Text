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
using System.Runtime.Serialization;

namespace ServiceStack.Text.Common
{
	internal static class DeserializeType<TSerializer>
		where TSerializer : ITypeSerializer
	{
		private static readonly ITypeSerializer Serializer = JsWriter.GetTypeSerializer<TSerializer>();

		public static ParseStringDelegate GetParseMethod(Type type)
		{
			if (!type.IsClass) 
				return null;

			var propertyInfos = type.GetProperties();
			
			if (propertyInfos.Length == 0)
			{
				var emptyCtorFn = ReflectionExtensions.GetConstructorMethodToCache(type);
				return value => emptyCtorFn();
			}

			var ctorFn = ReflectionExtensions.GetConstructorMethodToCache(type);
			return value => StringToType(type, value, ctorFn, PropertyMapLookup<TSerializer>.PropertyMapFor( type, Serializer ));
		}

		private static object StringToType(Type type, string strType, 
           EmptyCtorDelegate ctorFn,
			PropertyMap propertyMap )
		{
			var index = 0;

			if (!Serializer.EatMapStartChar(strType, ref index))
			{
				throw new SerializationException(string.Format(
					"Type definitions should start with a '{0}', expecting serialized type '{1}', got string starting with: {2}",
					JsWriter.MapStartChar, type.Name, strType.Substring(0, strType.Length < 50 ? strType.Length : 50)));
			}

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
				propertyMap = PropertyMapLookup<TSerializer>.PropertyMapFor( instance.GetType( ), Serializer ) ;
			}
			else
			{
				instance = ctorFn( ) ;
			}

			foreach( var pair in propertyNamesAndValues )
			{
				string name = pair.Key ;
				string value = pair.Value ;

				ParseStringDelegate parseStringFn = propertyMap.TryGetParserFor( name ) ;

				if (parseStringFn != null)
				{
					var propertyValue = parseStringFn(value);

					SetPropertyDelegate setterFn = propertyMap.TryGetSetterFor( name ) ;

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
			if( type == null )
			{
				throw new InvalidOperationException(
					@"Cannot create a type named '{0}' that derives from '{1}'.  No such type exists".FormatWith(
						fullName,
						baseType.Name ) ) ;
			}

			return Activator.CreateInstance( type ) ;
		}
	}
}