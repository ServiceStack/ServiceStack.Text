using System;
using System.Collections.Generic;
using System.Reflection;
using ServiceStack.Reflection;
using ServiceStack.Text;

namespace ServiceStack
{
    public static class TypeReflector<T>
    {
        public static readonly Dictionary<string, Func<object, object>> PublicGetters =
            new Dictionary<string, Func<object, object>>(PclExport.Instance.InvariantComparerIgnoreCase);

        public static readonly Dictionary<string, Action<object, object>> PublicSetters =
            new Dictionary<string, Action<object, object>>(PclExport.Instance.InvariantComparerIgnoreCase);

        public static readonly Dictionary<string, PropertyInfo> PublicProperties =
            new Dictionary<string, PropertyInfo>(PclExport.Instance.InvariantComparerIgnoreCase);

        public static readonly PropertyInfo[] PublicPropertyInfos;

        static TypeReflector()
        {
            PublicPropertyInfos = typeof(T).GetPublicProperties();
            foreach (var pi in PublicPropertyInfos)
            {
                try
                {
                    PublicGetters[pi.Name] = pi.GetValueGetter(typeof(T));
                    PublicSetters[pi.Name] = pi.GetValueSetter(typeof(T));
                    PublicProperties[pi.Name] = pi;
                }
                catch (Exception ex)
                {
                    Tracer.Instance.WriteError(ex);
                }
            }
        }

        public static PropertyInfo GetPublicProperty(string name)
        {
            foreach (var pi in PublicPropertyInfos)
            {
                if (pi.Name == name)
                    return pi;
            }
            return null;
        }

        public static Func<object, object> GetPublicGetter(PropertyInfo pi)
        {
            if (pi == null)
                return null;

            Func<object, object> fn;
            return PublicGetters.TryGetValue(pi.Name, out fn)
                ? fn
                : pi.GetValueGetter();
        }

        public static Func<object, object> GetPublicGetter(string name)
        {
            if (name == null)
                return null;

            Func<object, object> fn;
            return PublicGetters.TryGetValue(name, out fn)
                ? fn
                : null;
        }

        public static Action<object, object> GetPublicSetter(PropertyInfo pi)
        {
            if (pi == null)
                return null;

            Action<object, object> fn;
            return PublicSetters.TryGetValue(pi.Name, out fn)
                ? fn
                : pi.GetValueSetter();
        }

        public static Action<object, object> GetPublicSetter(string name)
        {
            if (name == null)
                return null;

            Action<object, object> fn;
            return PublicSetters.TryGetValue(name, out fn)
                ? fn
                : null;
        }
    }
}