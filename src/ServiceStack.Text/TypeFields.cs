using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using ServiceStack.Reflection;
using ServiceStack.Text;
using ServiceStack.Text.Common;

namespace ServiceStack
{
    public class TypeFieldInfo
    {
        public TypeFieldInfo(
            FieldInfo fieldInfo,
            PropertyGetterDelegate publicGetter,
            PropertySetterDelegate publicSetter)
        {
            FieldInfo = fieldInfo;
            PublicGetter = publicGetter;
            PublicSetter = publicSetter;
        }

        public FieldInfo FieldInfo { get; }

        public PropertyGetterDelegate PublicGetter { get; }

        public PropertySetterDelegate PublicSetter { get; }
    }

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
                    Instance.FieldsMap[fi.Name] = new TypeFieldInfo(
                        fi,
                        PclExport.Instance.GetFieldGetterFn(fi),
                        PclExport.Instance.GetFieldSetterFn(fi));

                    Instance.GenericPublicSetters[fi.Name] = fi.GetValueSetterGenericRef<T>();
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

        public static Type FactoryType = typeof(TypeFields<>);

        public static TypeFields Get(Type type)
        {
            if (CacheMap.TryGetValue(type, out TypeFields value))
                return value;

            var genericType = FactoryType.MakeGenericType(type);
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

        public Type Type { get; protected set; }

        public readonly Dictionary<string, TypeFieldInfo> FieldsMap =
            new Dictionary<string, TypeFieldInfo>(PclExport.Instance.InvariantComparerIgnoreCase);

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

        public virtual PropertyGetterDelegate GetPublicGetter(FieldInfo fi) => GetPublicGetter(fi?.Name);

        public virtual PropertyGetterDelegate GetPublicGetter(string name)
        {
            if (name == null)
                return null;

            return FieldsMap.TryGetValue(name, out TypeFieldInfo info)
                ? info.PublicGetter
                : null;
        }

        public virtual PropertySetterDelegate GetPublicSetter(FieldInfo fi) => GetPublicSetter(fi?.Name);

        public virtual PropertySetterDelegate GetPublicSetter(string name)
        {
            if (name == null)
                return null;

            return FieldsMap.TryGetValue(name, out TypeFieldInfo info)
                ? info.PublicSetter
                : null;
        }

        public virtual SetPropertyDelegateRef GetPublicSetterRef(string name)
        {
            throw new NotImplementedException();
        }
    }
}