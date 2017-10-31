//
// https://github.com/ServiceStack/ServiceStack.Text
// ServiceStack.Text: .NET C# POCO JSON, JSV and CSV Text Serializers.
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2012 ServiceStack, Inc. All Rights Reserved.
//
// Licensed under the same terms of ServiceStack.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using ServiceStack.Text;
using ServiceStack.Text.Common;
#if !XBOX
using System.Linq.Expressions;
#endif
namespace ServiceStack.Reflection
{
    //Also exists in ServiceStack.Common in ServiceStack.Reflection namespace
    [Obsolete("Use TypeProperties/PropertyInvoker, TypeFields/FieldsInvoker")]
    public static class StaticAccessors
    {
        private static Dictionary<string, Func<object, object>> getterFnCache = new Dictionary<string, Func<object, object>>();

        [Obsolete("Use TypeProperties.Get(type).GetPublicGetter()")]
        public static Func<object, object> GetFastGetter(this Type type, string propName)
        {
            var key = $"{type.FullName}::{propName}";
            Func<object, object> fn;
            if (getterFnCache.TryGetValue(key, out fn))
                return fn;

            var pi = type.GetPropertyInfo(propName);
            if (pi == null)
                return null;

            fn = GetValueGetter(pi);

            Dictionary<string, Func<object, object>> snapshot, newCache;
            do
            {
                snapshot = getterFnCache;
                newCache = new Dictionary<string, Func<object, object>>(getterFnCache) { [key] = fn };

            } while (!ReferenceEquals(
                Interlocked.CompareExchange(ref getterFnCache, newCache, snapshot), snapshot));

            return fn;
        }

        private static Dictionary<string, Action<object, object>> setterFnCache = new Dictionary<string, Action<object, object>>();

        [Obsolete("Use TypeProperties.Get(type).GetPublicSetter()")]
        public static Action<object, object> GetFastSetter(this Type type, string propName)
        {
            var key = $"{type.FullName}::{propName}";
            Action<object, object> fn;
            if (setterFnCache.TryGetValue(key, out fn))
                return fn;

            var pi = type.GetPropertyInfo(propName);
            if (pi == null)
                return null;

            fn = GetValueSetter(pi);

            Dictionary<string, Action<object, object>> snapshot, newCache;
            do
            {
                snapshot = setterFnCache;
                newCache = new Dictionary<string, Action<object, object>>(setterFnCache) { [key] = fn };

            } while (!ReferenceEquals(
                Interlocked.CompareExchange(ref setterFnCache, newCache, snapshot), snapshot));

            return fn;
        }

        [Obsolete("Use propertyInfo.CreateGetter()")]
        public static Func<object, object> GetValueGetter(this PropertyInfo propertyInfo)
        {
            return GetValueGetter(propertyInfo, propertyInfo.DeclaringType);
        }

        [Obsolete("Use propertyInfo.CreateGetter()")]
        public static Func<object, object> GetValueGetter(this PropertyInfo propertyInfo, Type type)
        {
            var instance = Expression.Parameter(typeof(object), "i");
            var convertInstance = Expression.TypeAs(instance, type);
            var property = Expression.Property(convertInstance, propertyInfo);
            var convertProperty = Expression.TypeAs(property, typeof(object));
            return Expression.Lambda<Func<object, object>>(convertProperty, instance).Compile();
        }

        [Obsolete("Use propertyInfo.CreateGetter<T>()")]
        public static Func<T, object> GetValueGetter<T>(this PropertyInfo propertyInfo)
        {
            var instance = Expression.Parameter(typeof(T), "i");
            var property = typeof(T) != propertyInfo.DeclaringType
                ? Expression.Property(Expression.TypeAs(instance, propertyInfo.DeclaringType), propertyInfo)
                : Expression.Property(instance, propertyInfo);
            var convertProperty = Expression.TypeAs(property, typeof(object));
            return Expression.Lambda<Func<T, object>>(convertProperty, instance).Compile();
        }

        [Obsolete("Use fieldInfo.CreateGetter<T>()")]
        public static Func<T, object> GetValueGetter<T>(this FieldInfo fieldInfo)
        {
            var instance = Expression.Parameter(typeof(T), "i");
            var field = typeof(T) != fieldInfo.DeclaringType
                ? Expression.Field(Expression.TypeAs(instance, fieldInfo.DeclaringType), fieldInfo)
                : Expression.Field(instance, fieldInfo);
            var convertField = Expression.TypeAs(field, typeof(object));
            return Expression.Lambda<Func<T, object>>(convertField, instance).Compile();
        }

        [Obsolete("Use propertyInfo.CreateSetter()")]
        public static Action<object, object> GetValueSetter(this PropertyInfo propertyInfo)
        {
            return GetValueSetter(propertyInfo, propertyInfo.DeclaringType);
        }

        [Obsolete("Use propertyInfo.CreateSetter()")]
        public static Action<object, object> GetValueSetter(this PropertyInfo propertyInfo, Type instanceType)
        {
            var instance = Expression.Parameter(typeof(object), "i");
            var argument = Expression.Parameter(typeof(object), "a");

            var type = (Expression)Expression.TypeAs(instance, instanceType);

            var setterCall = Expression.Call(
                type,
                propertyInfo.SetMethod(),
                Expression.Convert(argument, propertyInfo.PropertyType));

            return Expression.Lambda<Action<object, object>>
            (
                setterCall, instance, argument
            ).Compile();
        }

        [Obsolete("Use propertyInfo.CreateSetter<T>()")]
        public static Action<T, object> GetValueSetter<T>(this PropertyInfo propertyInfo)
        {
            var instance = Expression.Parameter(typeof(T), "i");
            var argument = Expression.Parameter(typeof(object), "a");

            var instanceType = typeof(T) != propertyInfo.DeclaringType
                ? (Expression)Expression.TypeAs(instance, propertyInfo.DeclaringType)
                : instance;

            var setterCall = Expression.Call(
                instanceType,
                propertyInfo.SetMethod(),
                Expression.Convert(argument, propertyInfo.PropertyType));

            return Expression.Lambda<Action<T, object>>
            (
                setterCall, instance, argument
            ).Compile();
        }

        [Obsolete("Use fieldInfo.CreateSetter()")]
        public static Action<object, object> GetValueSetter(this FieldInfo fieldInfo, Type instanceType)
        {
            var instance = Expression.Parameter(typeof(object), "i");
            var argument = Expression.Parameter(typeof(object), "a");

            var field = instanceType != fieldInfo.DeclaringType
                ? Expression.Field(Expression.TypeAs(instance, fieldInfo.DeclaringType), fieldInfo)
                : Expression.Field(Expression.Convert(instance, instanceType), fieldInfo);

            var setterCall = Expression.Assign(
                field,
                Expression.Convert(argument, fieldInfo.FieldType));

            return Expression.Lambda<Action<object, object>>
            (
                setterCall, instance, argument
            ).Compile();
        }

        [Obsolete("Use fieldInfo.CreateSetter<T>()")]
        public static Action<T, object> GetValueSetter<T>(this FieldInfo fieldInfo)
        {
            var instance = Expression.Parameter(typeof(T), "i");
            var argument = Expression.Parameter(typeof(object), "a");

            var field = typeof(T) != fieldInfo.DeclaringType
                ? Expression.Field(Expression.TypeAs(instance, fieldInfo.DeclaringType), fieldInfo)
                : Expression.Field(instance, fieldInfo);

            var setterCall = Expression.Assign(
                field,
                Expression.Convert(argument, fieldInfo.FieldType));

            return Expression.Lambda<Action<T, object>>
            (
                setterCall, instance, argument
            ).Compile();
        }

    }
}

