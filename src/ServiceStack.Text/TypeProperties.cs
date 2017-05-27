using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using ServiceStack.Reflection;
using ServiceStack.Text;

namespace ServiceStack
{
    [Obsolete("Use TypeProperties<T>.Instance")]
    public static class TypeReflector<T> { }

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
                    Instance.PublicGetters[pi.Name] = pi.GetValueGetter(typeof(T));
                    Instance.PublicSetters[pi.Name] = pi.GetValueSetter(typeof(T));
                    Instance.PublicProperties[pi.Name] = pi;
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

        public readonly Dictionary<string, Func<object, object>> PublicGetters =
            new Dictionary<string, Func<object, object>>(PclExport.Instance.InvariantComparerIgnoreCase);

        public readonly Dictionary<string, Action<object, object>> PublicSetters =
            new Dictionary<string, Action<object, object>>(PclExport.Instance.InvariantComparerIgnoreCase);

        public readonly Dictionary<string, PropertyInfo> PublicProperties =
            new Dictionary<string, PropertyInfo>(PclExport.Instance.InvariantComparerIgnoreCase);

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

        public Func<object, object> GetPublicGetter(PropertyInfo pi)
        {
            if (pi == null)
                return null;

            return PublicGetters.TryGetValue(pi.Name, out Func<object, object> fn)
                ? fn
                : pi.GetValueGetter();
        }

        public Func<object, object> GetPublicGetter(string name)
        {
            if (name == null)
                return null;

            return PublicGetters.TryGetValue(name, out Func<object, object> fn)
                ? fn
                : null;
        }

        public Action<object, object> GetPublicSetter(PropertyInfo pi)
        {
            if (pi == null)
                return null;

            return PublicSetters.TryGetValue(pi.Name, out Action<object, object> fn)
                ? fn
                : pi.GetValueSetter();
        }

        public Action<object, object> GetPublicSetter(string name)
        {
            if (name == null)
                return null;

            return PublicSetters.TryGetValue(name, out Action<object, object> fn)
                ? fn
                : null;
        }
    }
}