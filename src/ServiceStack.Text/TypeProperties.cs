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

    public class TypePropertyInfo
    {
        public TypePropertyInfo(
            PropertyInfo propertyInfo, 
            Func<object, object> publicGetter, 
            Action<object, object> publicSetter)
        {
            PropertyInfo = propertyInfo;
            PublicGetter = publicGetter;
            PublicSetter = publicSetter;
        }

        public PropertyInfo PropertyInfo { get; }

        public Func<object, object> PublicGetter { get; }

        public Action<object, object> PublicSetter { get; }
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
                        pi.GetValueGetter(typeof(T)),
                        pi.GetValueSetter(typeof(T))
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

        public Func<object, object> GetPublicGetter(PropertyInfo pi) => GetPublicGetter(pi?.Name);

        public Func<object, object> GetPublicGetter(string name)
        {
            if (name == null)
                return null;

            return PropertyMap.TryGetValue(name, out TypePropertyInfo info)
                ? info.PublicGetter
                : null;
        }

        public Action<object, object> GetPublicSetter(PropertyInfo pi) => GetPublicSetter(pi?.Name);

        public Action<object, object> GetPublicSetter(string name)
        {
            if (name == null)
                return null;

            return PropertyMap.TryGetValue(name, out TypePropertyInfo info)
                ? info.PublicSetter
                : null;
        }
    }
}