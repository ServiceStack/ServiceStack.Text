//
// https://github.com/ServiceStack/ServiceStack.Text
// ServiceStack.Text: .NET C# POCO JSON, JSV and CSV Text Serializers.
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2012 Service Stack LLC. All Rights Reserved.
//
// Licensed under the same terms of ServiceStack.
//
using System;
using System.Reflection;

#if !XBOX
using System.Linq.Expressions;
#endif
namespace ServiceStack.Text.Reflection
{
    //Also exists in ServiceStack.Common in ServiceStack.Reflection namespace
    public static class StaticAccessors
    {
        public static Func<object, object> GetValueGetter(this PropertyInfo propertyInfo, Type type)
        {
#if NETFX_CORE
			var getMethodInfo = propertyInfo.GetMethod;
			if (getMethodInfo == null) return null;
			return x => getMethodInfo.Invoke(x, new object[0]);
#elif (SL5 && !WP) || __IOS__ || XBOX
			var getMethodInfo = propertyInfo.GetGetMethod();
			if (getMethodInfo == null) return null;
			return x => getMethodInfo.Invoke(x, new object[0]);
#else

            var instance = Expression.Parameter(typeof(object), "i");
            var convertInstance = Expression.TypeAs(instance, propertyInfo.DeclaringType);
            var property = Expression.Property(convertInstance, propertyInfo);
            var convertProperty = Expression.TypeAs(property, typeof(object));
            return Expression.Lambda<Func<object, object>>(convertProperty, instance).Compile();
#endif
        }

        public static Func<T, object> GetValueGetter<T>(this PropertyInfo propertyInfo)
        {
#if NETFX_CORE
			var getMethodInfo = propertyInfo.GetMethod;
            if (getMethodInfo == null) return null;
			return x => getMethodInfo.Invoke(x, new object[0]);
#elif (SL5 && !WP) || __IOS__ || XBOX
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

        public static Func<T, object> GetValueGetter<T>(this FieldInfo fieldInfo)
        {
#if (SL5 && !WP) || __IOS__ || XBOX
            return x => fieldInfo.GetValue(x);
#else
            var instance = Expression.Parameter(fieldInfo.DeclaringType, "i");
            var property = Expression.Field(instance, fieldInfo);
            var convert = Expression.TypeAs(property, typeof(object));
            return Expression.Lambda<Func<T, object>>(convert, instance).Compile();
#endif
        }

#if !XBOX
        public static Action<T, object> GetValueSetter<T>(this PropertyInfo propertyInfo)
        {
            if (typeof(T) != propertyInfo.DeclaringType && !typeof(T).IsSubclassOf(propertyInfo.DeclaringType))
            {
                throw new ArgumentException();
            }

            var instance = Expression.Parameter(propertyInfo.DeclaringType, "i");
            var argument = Expression.Parameter(typeof(object), "a");
            var setterCall = Expression.Call(
                instance,
                propertyInfo.SetMethod(),
                Expression.Convert(argument, propertyInfo.PropertyType));

            return Expression.Lambda<Action<T, object>>
            (
                setterCall, instance, argument
            ).Compile();
        }
#endif

    }
}

