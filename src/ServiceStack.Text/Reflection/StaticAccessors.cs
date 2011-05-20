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
using System.Linq.Expressions;
using System.Reflection;

namespace ServiceStack.Text.Reflection
{
	public static class StaticAccessors
	{
		public static Func<object, object> GetValueGetter(this PropertyInfo propertyInfo, Type type)
		{
			//if (type != propertyInfo.DeclaringType)
			//{
			//    throw new ArgumentException();
			//}

			var instance = Expression.Parameter(typeof(object), "i");
			var convertInstance = Expression.TypeAs(instance, propertyInfo.DeclaringType);
			var property = Expression.Property(convertInstance, propertyInfo);
			var convertProperty = Expression.TypeAs(property, typeof(object));
			return Expression.Lambda<Func<object, object>>(convertProperty, instance).Compile();
		}

		public static Func<T, object> GetValueGetter<T>(this PropertyInfo propertyInfo)
		{
			//Not true for interface types
			//if (typeof(T) != propertyInfo.DeclaringType)
			//{
			//    throw new ArgumentException();
			//}
			
#if SILVERLIGHT || MONOTOUCH
			var getMethodInfo = propertyInfo.GetGetMethod();
			if (getMethodInfo == null) return null;
			return x => getMethodInfo.Invoke(x, new object[0]);
#else
			var instance = Expression.Parameter(propertyInfo.DeclaringType, "i");
			var property = Expression.Property(instance, propertyInfo);
			var convert = Expression.TypeAs(property, typeof(object));
			return Expression.Lambda<Func<T, object>>(convert, instance).Compile();
#endif
		}

		public static Action<T, object> GetValueSetter<T>(this PropertyInfo propertyInfo)
		{
			if (typeof(T) != propertyInfo.DeclaringType)
			{
				throw new ArgumentException();
			}

			var instance = Expression.Parameter(propertyInfo.DeclaringType, "i");
			var argument = Expression.Parameter(typeof(object), "a");
			var setterCall = Expression.Call(
				instance,
				propertyInfo.GetSetMethod(),
				Expression.Convert(argument, propertyInfo.PropertyType));

			return Expression.Lambda<Action<T, object>>
			(
				setterCall, instance, argument
			).Compile();
		}

	}
}