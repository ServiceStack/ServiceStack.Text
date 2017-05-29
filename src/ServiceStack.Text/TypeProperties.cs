using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using ServiceStack.Text;

#if !NETSTANDARD1_1 || NETSTANDARD1_3
using System.Linq.Expressions;
#endif

#if NET45 || NETSTANDARD1_3
using System.Reflection.Emit;
#endif

namespace ServiceStack
{
    [Obsolete("Use TypeProperties<T>.Instance")]
    public static class TypeReflector<T> { }

    public class TypePropertyInfo
    {
        public TypePropertyInfo(
            PropertyInfo propertyInfo, 
            GetMemberDelegate publicGetter, 
            SetMemberDelegate publicSetter)
        {
            PropertyInfo = propertyInfo;
            PublicGetter = publicGetter;
            PublicSetter = publicSetter;
        }

        public PropertyInfo PropertyInfo { get; }

        public GetMemberDelegate PublicGetter { get; }

        public SetMemberDelegate PublicSetter { get; }
    }

    public class TypeProperties<T> : TypeProperties
    {
        public static readonly TypeProperties<T> Instance = new TypeProperties<T>();

        static TypeProperties()
        {
            Instance.Type = typeof(T);
            Instance.PublicPropertyInfos = typeof(T).GetPublicProperties();
            foreach (var pi in Instance.PublicPropertyInfos)
            {
                try
                {
                    Instance.PropertyMap[pi.Name] = new TypePropertyInfo(
                        pi,
                        PclExport.Instance.GetPropertyGetterFn(pi),
                        PclExport.Instance.GetPropertySetterFn(pi)
                    );
                }
                catch (Exception ex)
                {
                    Tracer.Instance.WriteError(ex);
                }
            }
        }
    }

    public abstract class TypeProperties
    {
        static Dictionary<Type, TypeProperties> CacheMap = new Dictionary<Type, TypeProperties>();

        public static Type FactoryType = typeof(TypeProperties<>);

        public static TypeProperties Get(Type type)
        {
            if (CacheMap.TryGetValue(type, out TypeProperties value))
                return value;

            var genericType = FactoryType.MakeGenericType(type);
            var instanceFi = genericType.GetPublicStaticField("Instance");
            var instance = (TypeProperties)instanceFi.GetValue(null);

            Dictionary<Type, TypeProperties> snapshot, newCache;
            do
            {
                snapshot = CacheMap;
                newCache = new Dictionary<Type, TypeProperties>(CacheMap)
                {
                    [type] = instance
                };
            } while (!ReferenceEquals(
                Interlocked.CompareExchange(ref CacheMap, newCache, snapshot), snapshot));

            return instance;
        }

        public Type Type { get; protected set; }

        public readonly Dictionary<string, TypePropertyInfo> PropertyMap =
            new Dictionary<string, TypePropertyInfo>(PclExport.Instance.InvariantComparerIgnoreCase);

        public PropertyInfo[] PublicPropertyInfos { get; protected set; }

        public PropertyInfo GetPublicProperty(string name)
        {
            foreach (var pi in PublicPropertyInfos)
            {
                if (pi.Name == name)
                    return pi;
            }
            return null;
        }

        public GetMemberDelegate GetPublicGetter(PropertyInfo pi) => GetPublicGetter(pi?.Name);

        public GetMemberDelegate GetPublicGetter(string name)
        {
            if (name == null)
                return null;

            return PropertyMap.TryGetValue(name, out TypePropertyInfo info)
                ? info.PublicGetter
                : null;
        }

        public SetMemberDelegate GetPublicSetter(PropertyInfo pi) => GetPublicSetter(pi?.Name);

        public SetMemberDelegate GetPublicSetter(string name)
        {
            if (name == null)
                return null;

            return PropertyMap.TryGetValue(name, out TypePropertyInfo info)
                ? info.PublicSetter
                : null;
        }
    }

    public static class PropertyInvoker
    {
        public static GetMemberDelegate GetPropertyGetterFn(this PropertyInfo propertyInfo) =>
            PclExport.Instance.GetPropertyGetterFn(propertyInfo);

        public static SetMemberDelegate GetPropertySetterFn(this PropertyInfo propertyInfo) =>
            PclExport.Instance.GetPropertySetterFn(propertyInfo);

        public static GetMemberDelegate GetReflection(PropertyInfo propertyInfo) => propertyInfo.GetValue;
        public static SetMemberDelegate SetReflection(PropertyInfo propertyInfo) => propertyInfo.SetValue;

#if !NETSTANDARD1_1 || NETSTANDARD1_3
        public static GetMemberDelegate GetExpression(PropertyInfo propertyInfo)
        {
            var getMethodInfo = propertyInfo.GetMethodInfo();
            if (getMethodInfo == null) return null;
            try
            {
                var oInstanceParam = Expression.Parameter(typeof(object), "oInstanceParam");
                var instanceParam = Expression.Convert(oInstanceParam, propertyInfo.ReflectedType()); //propertyInfo.DeclaringType doesn't work on Proxy types

                var exprCallPropertyGetFn = Expression.Call(instanceParam, getMethodInfo);
                var oExprCallPropertyGetFn = Expression.Convert(exprCallPropertyGetFn, typeof(object));

                var propertyGetFn = Expression.Lambda<GetMemberDelegate>
                (
                    oExprCallPropertyGetFn,
                    oInstanceParam
                ).Compile();

                return propertyGetFn;

            }
            catch (Exception ex)
            {
                Tracer.Instance.WriteError(ex);
                throw;
            }
        }

        public static SetMemberDelegate SetExpression(PropertyInfo propertyInfo)
        {
            var propertySetMethod = propertyInfo.SetMethod();
            if (propertySetMethod == null) return null;

            try
            {
                var instance = Expression.Parameter(typeof(object), "i");
                var argument = Expression.Parameter(typeof(object), "a");

                var instanceParam = Expression.Convert(instance, propertyInfo.ReflectedType());
                var valueParam = Expression.Convert(argument, propertyInfo.PropertyType);

                var setterCall = Expression.Call(instanceParam, propertySetMethod, valueParam);

                return Expression.Lambda<SetMemberDelegate>(setterCall, instance, argument).Compile();
            }
            catch //fallback for Android
            {
                return (o, convertedValue) =>
                    propertySetMethod.Invoke(o, new[] { convertedValue });
            }
        }
#endif

#if NET45 || NETSTANDARD1_3
        public static GetMemberDelegate GetEmit(PropertyInfo propertyInfo)
        {
            var getter = FieldInvoker.CreateDynamicGetMethod(propertyInfo);

            var gen = getter.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);

            if (propertyInfo.DeclaringType.IsValueType())
            {
                gen.Emit(OpCodes.Unbox, propertyInfo.DeclaringType);
            }
            else
            {
                gen.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
            }

            gen.Emit(OpCodes.Callvirt, propertyInfo.GetGetMethod());

            if (propertyInfo.PropertyType.IsValueType())
            {
                gen.Emit(OpCodes.Box, propertyInfo.PropertyType);
            }

            gen.Emit(OpCodes.Ret);

            return (GetMemberDelegate)getter.CreateDelegate(typeof(GetMemberDelegate));
        }

        public static SetMemberDelegate SetEmit(PropertyInfo propertyInfo)
        {
            var propSetMethod = propertyInfo.GetSetMethod(true);
            if (propSetMethod == null)
                return null;

            var setter = FieldInvoker.CreateDynamicSetMethod(propertyInfo);

            var gen = setter.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);

            if (propertyInfo.DeclaringType.IsValueType())
            {
                gen.Emit(OpCodes.Unbox, propertyInfo.DeclaringType);
            }
            else
            {
                gen.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
            }

            gen.Emit(OpCodes.Ldarg_1);

            if (propertyInfo.PropertyType.IsValueType())
            {
                gen.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
            }
            else
            {
                gen.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
            }

            gen.EmitCall(OpCodes.Callvirt, propSetMethod, (Type[])null);
            gen.Emit(OpCodes.Ret);

            return (SetMemberDelegate)setter.CreateDelegate(typeof(SetMemberDelegate));
        }
#endif

    }
}