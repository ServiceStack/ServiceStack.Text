using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using ServiceStack.Reflection;
using ServiceStack.Text;
using ServiceStack.Text.Common;

namespace ServiceStack
{
    public class TypeFields<T> : TypeFields
    {
        public static readonly TypeFields<T> Instance = new TypeFields<T>();

        public readonly Dictionary<string, SetPropertyDelegateRefGeneric<T>> GenericPublicSetters =
            new Dictionary<string, SetPropertyDelegateRefGeneric<T>>(PclExport.Instance.InvariantComparerIgnoreCase);

        static TypeFields()
        {
            Instance.Type = typeof(T);
            Instance.PublicFieldInfos = typeof(T).GetPublicFields();
            foreach (var fi in Instance.PublicFieldInfos)
            {
                try
                {
                    Instance.PublicGetters[fi.Name] = PclExport.Instance.GetFieldGetterFn(fi);
                    Instance.PublicSetters[fi.Name] = fi.GetValueSetter(typeof(T));
                    Instance.GenericPublicSetters[fi.Name] = fi.GetValueSetterGenericRef<T>();
                    Instance.PublicFields[fi.Name] = fi;
                }
                catch (Exception ex)
                {
                    Tracer.Instance.WriteError(ex);
                }
            }
        }

        public override SetPropertyDelegateRef GetPublicSetterRef(string name)
        {
            if (name == null)
                return null;

            return GenericPublicSetters.TryGetValue(name, out SetPropertyDelegateRefGeneric<T> fn)
                ? delegate (ref object instance, object arg)
                  {
                      var valueInstance = (T)instance;
                      fn(ref valueInstance, arg);
                      instance = valueInstance;
                  }
            : (SetPropertyDelegateRef)null;
        }
    }

    public abstract class TypeFields
    {
        static Dictionary<Type, TypeFields> CacheMap = new Dictionary<Type, TypeFields>();

        public static TypeFields Get(Type type)
        {
            if (CacheMap.TryGetValue(type, out TypeFields value))
                return value;

            var genericType = typeof(TypeFields<>).MakeGenericType(type);
            var instanceFi = genericType.GetPublicStaticField("Instance");
            var instance = (TypeFields)instanceFi.GetValue(null);

            Dictionary<Type, TypeFields> snapshot, newCache;
            do
            {
                snapshot = CacheMap;
                newCache = new Dictionary<Type, TypeFields>(CacheMap)
                {
                    [type] = instance
                };
            } while (!ReferenceEquals(
                Interlocked.CompareExchange(ref CacheMap, newCache, snapshot), snapshot));

            return instance;
        }

        public Type Type;

        public readonly Dictionary<string, PropertyGetterDelegate> PublicGetters =
            new Dictionary<string, PropertyGetterDelegate>(PclExport.Instance.InvariantComparerIgnoreCase);

        public readonly Dictionary<string, Action<object, object>> PublicSetters =
            new Dictionary<string, Action<object, object>>(PclExport.Instance.InvariantComparerIgnoreCase);

        public readonly Dictionary<string, FieldInfo> PublicFields =
            new Dictionary<string, FieldInfo>(PclExport.Instance.InvariantComparerIgnoreCase);

        public FieldInfo[] PublicFieldInfos { get; protected set; }

        public virtual FieldInfo GetPublicField(string name)
        {
            foreach (var fi in PublicFieldInfos)
            {
                if (fi.Name == name)
                    return fi;
            }
            return null;
        }

        public virtual PropertyGetterDelegate GetPublicGetter(FieldInfo fi)
        {
            if (fi == null)
                return null;

            return PublicGetters.TryGetValue(fi.Name, out PropertyGetterDelegate fn)
                ? fn
                : PclExport.Instance.GetFieldGetterFn(fi);
        }

        public virtual PropertyGetterDelegate GetPublicGetter(string name)
        {
            if (name == null)
                return null;

            return PublicGetters.TryGetValue(name, out PropertyGetterDelegate fn)
                ? fn
                : null;
        }

        public virtual Action<object, object> GetPublicSetter(FieldInfo fi)
        {
            if (fi == null)
                return null;

            return PublicSetters.TryGetValue(fi.Name, out Action<object, object> fn)
                ? fn
                : fi.GetValueSetter(Type);
        }

        public virtual Action<object, object> GetPublicSetter(string name)
        {
            if (name == null)
                return null;

            return PublicSetters.TryGetValue(name, out Action<object, object> fn)
                ? fn
                : null;
        }

        public virtual SetPropertyDelegateRef GetPublicSetterRef(string name)
        {
            throw new NotImplementedException();
        }
    }
}