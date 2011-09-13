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

using System;
using System.Collections.Generic;
using System.Linq.Expressions ;
using System.Reflection;

namespace ServiceStack.Text.Common
{
	internal static class ParseUtils
	{
		public static object NullValueType(Type type)
		{
			return ReflectionExtensions.GetDefaultValue(type);
		}

		public static object ParseObject(string value)
		{
			return value;
		}

		public static object ParseEnum(Type type, string value)
		{
			return Enum.Parse(type, value, false);
		}

		public static ParseStringDelegate GetSpecialParseMethod(Type type)
		{
			if (type == typeof(Uri))
				return x => new Uri(x.FromCsvField());

			//Warning: typeof(object).IsInstanceOfType(typeof(Type)) == True??
			if (type.IsInstanceOfType(typeof(Type)))
				return ParseType;

			if (type == typeof(Exception))
				return x => new Exception(x);

			if (type.IsInstanceOf(typeof(Exception)))
				return DeserializeTypeUtils.GetParseMethod(type);

			return null;
		}

		public static Type ParseType(string assemblyQualifiedName)
		{
			return Type.GetType(assemblyQualifiedName.FromCsvField());
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