using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using ServiceStack.Text;

namespace ServiceStack
{
    public static class PlatformExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInterface(this Type type)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetTypeInfo().IsInterface;
#else
            return type.IsInterface;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsArray(this Type type)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetTypeInfo().IsArray;
#else
            return type.IsArray;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValueType(this Type type)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetTypeInfo().IsValueType;
#else
            return type.IsValueType;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsGeneric(this Type type)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetTypeInfo().IsGenericType;
#else
            return type.IsGenericType;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type BaseType(this Type type)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetTypeInfo().BaseType;
#else
            return type.BaseType;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type ReflectedType(this PropertyInfo pi)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return pi.DeclaringType;
#else
            return pi.ReflectedType;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type ReflectedType(this FieldInfo fi)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return fi.DeclaringType;
#else
            return fi.ReflectedType;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type GenericTypeDefinition(this Type type)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetTypeInfo().GetGenericTypeDefinition();
#else
            return type.GetGenericTypeDefinition();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type[] GetTypeInterfaces(this Type type)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetTypeInfo().ImplementedInterfaces.ToArray();
#else
            return type.GetInterfaces();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type[] GetTypeGenericArguments(this Type type)
        {
#if (NETFX_CORE || PCL)
            return type.GenericTypeArguments;
#else
            return type.GetGenericArguments();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConstructorInfo GetEmptyConstructor(this Type type)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetTypeInfo().DeclaredConstructors.FirstOrDefault(c => c.GetParameters().Length == 0);
#else
            return type.GetConstructor(Type.EmptyTypes);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<ConstructorInfo> GetAllConstructors(this Type type)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetTypeInfo().DeclaredConstructors;
#else
            return type.GetConstructors();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static PropertyInfo[] GetTypesPublicProperties(this Type subType)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            var pis = new List<PropertyInfo>();
            foreach (var pi in subType.GetRuntimeProperties())
            {
                var mi = pi.GetMethod ?? pi.SetMethod;
                if (mi != null && mi.IsStatic) continue;
                pis.Add(pi);
            }
            return pis.ToArray();
#else
            return subType.GetProperties(
                BindingFlags.FlattenHierarchy |
                BindingFlags.Public |
                BindingFlags.Instance);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static PropertyInfo[] GetTypesProperties(this Type subType)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            var pis = new List<PropertyInfo>();
            foreach (var pi in subType.GetRuntimeProperties())
            {
                var mi = pi.GetMethod ?? pi.SetMethod;
                if (mi != null && mi.IsStatic) continue;
                pis.Add(pi);
            }
            return pis.ToArray();
#else
            return subType.GetProperties(
                BindingFlags.FlattenHierarchy |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Assembly GetAssembly(this Type type)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetTypeInfo().Assembly;
#else
            return type.Assembly;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodInfo GetMethod(this Type type, string methodName)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetTypeInfo().GetDeclaredMethod(methodName);
#else
            return type.GetMethod(methodName);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FieldInfo[] Fields(this Type type)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetRuntimeFields().ToArray();
#else
            return type.GetFields(
                BindingFlags.FlattenHierarchy |
                BindingFlags.Instance |
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.NonPublic);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PropertyInfo[] Properties(this Type type)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetRuntimeProperties().ToArray();
#else
            return type.GetProperties(
                BindingFlags.FlattenHierarchy |
                BindingFlags.Instance |
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.NonPublic);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FieldInfo[] GetAllFields(this Type type)
        {
            if (type.IsInterface())
            {
                return TypeConstants.EmptyFieldInfoArray;
            }

#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetRuntimeFields().ToArray();
#else
            return type.Fields();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FieldInfo[] GetPublicFields(this Type type)
        {
            if (type.IsInterface())
            {
                return TypeConstants.EmptyFieldInfoArray;
            }

#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetRuntimeFields().Where(p => p.IsPublic && !p.IsStatic).ToArray();
#else
            return type.GetFields(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance)
                .ToArray();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemberInfo[] GetPublicMembers(this Type type)
        {

#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            var members = new List<MemberInfo>();
            members.AddRange(type.GetRuntimeFields().Where(p => p.IsPublic && !p.IsStatic));
            members.AddRange(type.GetPublicProperties());
            return members.ToArray();
#else
            return type.GetMembers(BindingFlags.Public | BindingFlags.Instance);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemberInfo[] GetAllPublicMembers(this Type type)
        {

#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            var members = new List<MemberInfo>();
            members.AddRange(type.GetRuntimeFields().Where(p => p.IsPublic && !p.IsStatic));
            members.AddRange(type.GetPublicProperties());
            return members.ToArray();
#else
            return type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodInfo GetStaticMethod(this Type type, string methodName)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetMethodInfo(methodName);
#else
            return type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodInfo GetInstanceMethod(this Type type, string methodName)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetMethodInfo(methodName);
#else
            return type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodInfo Method(this Delegate fn)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return fn.GetMethodInfo();
#else
            return fn.Method;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAttribute<T>(this Type type)
        {
            return type.AllAttributes().Any(x => x.GetType() == typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAttribute<T>(this PropertyInfo pi)
        {
            return pi.AllAttributes().Any(x => x.GetType() == typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAttribute<T>(this FieldInfo fi)
        {
            return fi.AllAttributes().Any(x => x.GetType() == typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAttributeNamed(this Type type, string name)
        {
            var normalizedAttr = name.Replace("Attribute", "").ToLower();
            return type.AllAttributes().Any(x => x.GetType().Name.Replace("Attribute", "").ToLower() == normalizedAttr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAttributeNamed(this PropertyInfo pi, string name)
        {
            var normalizedAttr = name.Replace("Attribute", "").ToLower();
            return pi.AllAttributes().Any(x => x.GetType().Name.Replace("Attribute", "").ToLower() == normalizedAttr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAttributeNamed(this FieldInfo fi, string name)
        {
            var normalizedAttr = name.Replace("Attribute", "").ToLower();
            return fi.AllAttributes().Any(x => x.GetType().Name.Replace("Attribute", "").ToLower() == normalizedAttr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAttributeNamed(this MemberInfo mi, string name)
        {
            var normalizedAttr = name.Replace("Attribute", "").ToLower();
            return mi.AllAttributes().Any(x => x.GetType().Name.Replace("Attribute", "").ToLower() == normalizedAttr);
        }

        const string DataContract = "DataContractAttribute";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDto(this Type type)
        {
            if (type == null)
                return false;

#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.HasAttribute<DataContractAttribute>();
#else
            return !Env.IsMono
                ? type.HasAttribute<DataContractAttribute>()
                : type.GetCustomAttributes(true).Any(x => x.GetType().Name == DataContract);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodInfo PropertyGetMethod(this PropertyInfo pi, bool nonPublic = false)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            var mi = pi.GetMethod;
            return mi != null && (nonPublic || mi.IsPublic) ? mi : null;
#else
            return pi.GetGetMethod(nonPublic);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type[] Interfaces(this Type type)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetTypeInfo().ImplementedInterfaces.ToArray();
            //return type.GetTypeInfo().ImplementedInterfaces
            //    .FirstOrDefault(x => !x.GetTypeInfo().ImplementedInterfaces
            //        .Any(y => y.GetTypeInfo().ImplementedInterfaces.Contains(y)));
#else
            return type.GetInterfaces();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PropertyInfo[] AllProperties(this Type type)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetRuntimeProperties().ToArray();
#else
            return type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
#endif
        }

        //Should only register Runtime Attributes on StartUp, So using non-ThreadSafe Dictionary is OK
        static Dictionary<string, List<Attribute>> propertyAttributesMap
            = new Dictionary<string, List<Attribute>>();

        static Dictionary<Type, List<Attribute>> typeAttributesMap
            = new Dictionary<Type, List<Attribute>>();

        public static void ClearRuntimeAttributes()
        {
            propertyAttributesMap = new Dictionary<string, List<Attribute>>();
            typeAttributesMap = new Dictionary<Type, List<Attribute>>();
        }

        internal static string UniqueKey(this PropertyInfo pi)
        {
            if (pi.DeclaringType == null)
                throw new ArgumentException("Property '{0}' has no DeclaringType".Fmt(pi.Name));

            return pi.DeclaringType.Namespace + "." + pi.DeclaringType.Name + "." + pi.Name;
        }

        public static Type AddAttributes(this Type type, params Attribute[] attrs)
        {
            List<Attribute> typeAttrs;
            if (!typeAttributesMap.TryGetValue(type, out typeAttrs))
            {
                typeAttributesMap[type] = typeAttrs = new List<Attribute>();
            }

            typeAttrs.AddRange(attrs);
            return type;
        }

        /// <summary>
        /// Add a Property attribute at runtime. 
        /// <para>Not threadsafe, should only add attributes on Startup.</para>
        /// </summary>
        public static PropertyInfo AddAttributes(this PropertyInfo propertyInfo, params Attribute[] attrs)
        {
            List<Attribute> propertyAttrs;
            var key = propertyInfo.UniqueKey();
            if (!propertyAttributesMap.TryGetValue(key, out propertyAttrs))
            {
                propertyAttributesMap[key] = propertyAttrs = new List<Attribute>();
            }

            propertyAttrs.AddRange(attrs);

            return propertyInfo;
        }

        /// <summary>
        /// Add a Property attribute at runtime. 
        /// <para>Not threadsafe, should only add attributes on Startup.</para>
        /// </summary>
        public static PropertyInfo ReplaceAttribute(this PropertyInfo propertyInfo, Attribute attr)
        {
            var key = propertyInfo.UniqueKey();

            List<Attribute> propertyAttrs;
            if (!propertyAttributesMap.TryGetValue(key, out propertyAttrs))
            {
                propertyAttributesMap[key] = propertyAttrs = new List<Attribute>();
            }

            propertyAttrs.RemoveAll(x => x.GetType() == attr.GetType());

            propertyAttrs.Add(attr);

            return propertyInfo;
        }

        public static List<TAttr> GetAttributes<TAttr>(this PropertyInfo propertyInfo)
        {
            List<Attribute> propertyAttrs;
            return !propertyAttributesMap.TryGetValue(propertyInfo.UniqueKey(), out propertyAttrs)
                ? new List<TAttr>()
                : propertyAttrs.OfType<TAttr>().ToList();
        }

        public static List<Attribute> GetAttributes(this PropertyInfo propertyInfo)
        {
            List<Attribute> propertyAttrs;
            return !propertyAttributesMap.TryGetValue(propertyInfo.UniqueKey(), out propertyAttrs)
                ? new List<Attribute>()
                : propertyAttrs.ToList();
        }

        public static List<Attribute> GetAttributes(this PropertyInfo propertyInfo, Type attrType)
        {
            List<Attribute> propertyAttrs;
            return !propertyAttributesMap.TryGetValue(propertyInfo.UniqueKey(), out propertyAttrs)
                ? new List<Attribute>()
                : propertyAttrs.Where(x => attrType.IsInstanceOf(x.GetType())).ToList();
        }

        public static object[] AllAttributes(this PropertyInfo propertyInfo)
        {
#if (NETFX_CORE || PCL)
            return propertyInfo.GetCustomAttributes(true).ToArray();
#else
#if NETSTANDARD1_1
            var attrs = propertyInfo.GetCustomAttributes(true).ToArray();
#else
            var attrs = propertyInfo.GetCustomAttributes(true);
#endif
            var runtimeAttrs = propertyInfo.GetAttributes();
            if (runtimeAttrs.Count == 0)
                return attrs;

            runtimeAttrs.AddRange(attrs.Cast<Attribute>());
            return runtimeAttrs.Cast<object>().ToArray();
#endif
        }

        public static object[] AllAttributes(this PropertyInfo propertyInfo, Type attrType)
        {
#if (NETFX_CORE || PCL)
            return propertyInfo.GetCustomAttributes(true).Where(x => x.GetType().IsInstanceOf(attrType)).ToArray();
#else
#if NETSTANDARD1_1
            var attrs = propertyInfo.GetCustomAttributes(attrType, true).ToArray();
#else
            var attrs = propertyInfo.GetCustomAttributes(attrType, true);
#endif
            var runtimeAttrs = propertyInfo.GetAttributes(attrType);
            if (runtimeAttrs.Count == 0)
                return attrs;

            runtimeAttrs.AddRange(attrs.Cast<Attribute>());
            return runtimeAttrs.Cast<object>().ToArray();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object[] AllAttributes(this ParameterInfo paramInfo)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return paramInfo.GetCustomAttributes(true).ToArray();
#else
            return paramInfo.GetCustomAttributes(true);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object[] AllAttributes(this FieldInfo fieldInfo)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return fieldInfo.GetCustomAttributes(true).ToArray();
#else
            return fieldInfo.GetCustomAttributes(true);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object[] AllAttributes(this MemberInfo memberInfo)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return memberInfo.GetCustomAttributes(true).ToArray();
#else
            return memberInfo.GetCustomAttributes(true);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object[] AllAttributes(this ParameterInfo paramInfo, Type attrType)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return paramInfo.GetCustomAttributes(true).Where(x => x.GetType().IsInstanceOf(attrType)).ToArray();
#else
            return paramInfo.GetCustomAttributes(attrType, true);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object[] AllAttributes(this MemberInfo memberInfo, Type attrType)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return memberInfo.GetCustomAttributes(true).Where(x => x.GetType().IsInstanceOf(attrType)).ToArray();
#else
            var prop = memberInfo as PropertyInfo;
            if (prop != null)
                return prop.AllAttributes(attrType);

            return memberInfo.GetCustomAttributes(attrType, true);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object[] AllAttributes(this FieldInfo fieldInfo, Type attrType)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return fieldInfo.GetCustomAttributes(true).Where(x => x.GetType().IsInstanceOf(attrType)).ToArray();
#else
            return fieldInfo.GetCustomAttributes(attrType, true);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object[] AllAttributes(this Type type)
        {
#if (NETFX_CORE || PCL)
            return type.GetTypeInfo().GetCustomAttributes(true).ToArray();
#elif NETSTANDARD1_1
            return type.GetTypeInfo().GetCustomAttributes(true).Union(type.GetRuntimeAttributes()).ToArray();
#else
            return type.GetCustomAttributes(true).Union(type.GetRuntimeAttributes()).ToArray();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object[] AllAttributes(this Type type, Type attrType)
        {
#if (NETFX_CORE || PCL)
            return type.GetTypeInfo().GetCustomAttributes(true).Where(x => x.GetType().IsInstanceOf(attrType)).ToArray();
#elif NETSTANDARD1_1
            return type.GetTypeInfo().GetCustomAttributes(attrType, true)
                .Union(type.GetRuntimeAttributes(attrType))
                .ToArray();
#else
            return type.GetCustomAttributes(attrType, true).Union(type.GetRuntimeAttributes(attrType)).ToArray();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object[] AllAttributes(this Assembly assembly)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return assembly.GetCustomAttributes().ToArray();
#else
            return assembly.GetCustomAttributes(true).ToArray();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TAttr[] AllAttributes<TAttr>(this ParameterInfo pi)
        {
            return pi.AllAttributes(typeof(TAttr)).Cast<TAttr>().ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TAttr[] AllAttributes<TAttr>(this MemberInfo mi)
        {
            return mi.AllAttributes(typeof(TAttr)).Cast<TAttr>().ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TAttr[] AllAttributes<TAttr>(this FieldInfo fi)
        {
            return fi.AllAttributes(typeof(TAttr)).Cast<TAttr>().ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TAttr[] AllAttributes<TAttr>(this PropertyInfo pi)
        {
            return pi.AllAttributes(typeof(TAttr)).Cast<TAttr>().ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static IEnumerable<T> GetRuntimeAttributes<T>(this Type type)
        {
            List<Attribute> attrs;
            return typeAttributesMap.TryGetValue(type, out attrs)
                ? attrs.OfType<T>()
                : new List<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static IEnumerable<Attribute> GetRuntimeAttributes(this Type type, Type attrType = null)
        {
            List<Attribute> attrs;
            return typeAttributesMap.TryGetValue(type, out attrs)
                ? attrs.Where(x => attrType == null || attrType.IsInstanceOf(x.GetType()))
                : new List<Attribute>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TAttr[] AllAttributes<TAttr>(this Type type)
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            where TAttr : Attribute
#endif
        {
#if (NETFX_CORE || PCL)
            return type.GetTypeInfo().GetCustomAttributes<TAttr>(true).ToArray();
#elif NETSTANDARD1_1
            return type.GetTypeInfo().GetCustomAttributes<TAttr>(true)
                .Union(type.GetRuntimeAttributes<TAttr>())
                .ToArray();
#else
            return type.GetCustomAttributes(typeof(TAttr), true)
                .OfType<TAttr>()
                .Union(type.GetRuntimeAttributes<TAttr>())
                .ToArray();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TAttr FirstAttribute<TAttr>(this Type type) where TAttr : class
        {
#if (NETFX_CORE || PCL )

            return (TAttr)type.GetTypeInfo().GetCustomAttributes(typeof(TAttr), true)
                    .Cast<TAttr>()
                    .FirstOrDefault();
#elif NETSTANDARD1_1                   
            return (TAttr)type.GetTypeInfo().GetCustomAttributes(typeof(TAttr), true)
                    .Cast<TAttr>()
                    .FirstOrDefault()
                   ?? type.GetRuntimeAttributes<TAttr>().FirstOrDefault();
#else
            return (TAttr)type.GetCustomAttributes(typeof(TAttr), true)
                       .FirstOrDefault()
                   ?? type.GetRuntimeAttributes<TAttr>().FirstOrDefault();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TAttribute FirstAttribute<TAttribute>(this MemberInfo memberInfo)
        {
            return memberInfo.AllAttributes<TAttribute>().FirstOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TAttribute FirstAttribute<TAttribute>(this ParameterInfo paramInfo)
        {
            return paramInfo.AllAttributes<TAttribute>().FirstOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TAttribute FirstAttribute<TAttribute>(this PropertyInfo propertyInfo)
        {
            return propertyInfo.AllAttributes<TAttribute>().FirstOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type FirstGenericTypeDefinition(this Type type)
        {
            var genericType = type.FirstGenericType();
            return genericType != null ? genericType.GetGenericTypeDefinition() : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDynamic(this Assembly assembly)
        {
#if __IOS__ || WP || NETFX_CORE || PCL || NETSTANDARD1_1
            return false;
#else
            try
            {
                var isDyanmic = assembly is System.Reflection.Emit.AssemblyBuilder
                                || string.IsNullOrEmpty(assembly.Location);
                return isDyanmic;
            }
            catch (NotSupportedException)
            {
                //Ignore assembly.Location not supported in a dynamic assembly.
                return true;
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodInfo GetStaticMethod(this Type type, string methodName, Type[] types = null)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            foreach (MethodInfo method in type.GetTypeInfo().DeclaredMethods)
            {
                if (method.IsStatic && method.Name == methodName)
                {
                    return method;
                }
            }

            return null;
#else
            return types == null
                ? type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static)
                : type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static, null, types, null);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodInfo GetMethodInfo(this Type type, string methodName, Type[] types = null)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            if (types == null) 
                return type.GetRuntimeMethods().FirstOrDefault(p => p.Name.Equals(methodName));

            foreach(var mi in type.GetRuntimeMethods().Where(p => p.Name.Equals(methodName)))
            {
                var methodParams = mi.GetParameters().Select(p => p.ParameterType);
                if (methodParams.SequenceEqual(types))
                {
                    return mi;
                }
            }

            return null;
#else
            return types == null
                ? type.GetMethod(methodName)
                : type.GetMethod(methodName, types);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object InvokeMethod(this Delegate fn, object instance, object[] parameters = null)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return fn.GetMethodInfo().Invoke(instance, parameters ?? new object[] { });
#else
            return fn.Method.Invoke(instance, parameters ?? new object[] { });
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FieldInfo GetPublicStaticField(this Type type, string fieldName)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetRuntimeField(fieldName);
#else
            return type.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Delegate MakeDelegate(this MethodInfo mi, Type delegateType, bool throwOnBindFailure = true)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return mi.CreateDelegate(delegateType);
#else
            return Delegate.CreateDelegate(delegateType, mi, throwOnBindFailure);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type[] GenericTypeArguments(this Type type)
        {
#if (NETFX_CORE || PCL)
            return type.GenericTypeArguments;
#else
            return type.GetGenericArguments();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConstructorInfo[] DeclaredConstructors(this Type type)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetTypeInfo().DeclaredConstructors.ToArray();
#else
            return type.GetConstructors();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AssignableFrom(this Type type, Type fromType)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetTypeInfo().IsAssignableFrom(fromType.GetTypeInfo());
#else
            return type.IsAssignableFrom(fromType);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsStandardClass(this Type type)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsClass && !typeInfo.IsAbstract && !typeInfo.IsInterface;
#else
            return type.IsClass && !type.IsAbstract && !type.IsInterface;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAbstract(this Type type)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetTypeInfo().IsAbstract;
#else
            return type.IsAbstract;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PropertyInfo GetPropertyInfo(this Type type, string propertyName)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetRuntimeProperty(propertyName);
#else
            return type.GetProperty(propertyName);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FieldInfo GetFieldInfo(this Type type, string fieldName)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetRuntimeField(fieldName);
#else
            return type.GetField(fieldName);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FieldInfo[] GetWritableFields(this Type type)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetRuntimeFields().Where(p => !p.IsPublic && !p.IsStatic).ToArray();
#else
            return type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodInfo SetMethod(this PropertyInfo pi, bool nonPublic = true)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return pi.SetMethod;
#else
            return pi.GetSetMethod(nonPublic);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodInfo GetMethodInfo(this PropertyInfo pi, bool nonPublic = true)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return pi.GetMethod;
#else
            return pi.GetGetMethod(nonPublic);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool InstanceOfType(this Type type, object instance)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return instance.GetType().IsInstanceOf(type);
#else
            return type.IsInstanceOfType(instance);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAssignableFromType(this Type type, Type fromType)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetTypeInfo().IsAssignableFrom(fromType.GetTypeInfo());
#else
            return type.IsAssignableFrom(fromType);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsClass(this Type type)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetTypeInfo().IsClass;
#else
            return type.IsClass;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEnum(this Type type)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetTypeInfo().IsEnum;
#else
            return type.IsEnum;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEnumFlags(this Type type)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetTypeInfo().IsEnum && type.FirstAttribute<FlagsAttribute>() != null;
#else
            return type.IsEnum && type.FirstAttribute<FlagsAttribute>() != null;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUnderlyingEnum(this Type type)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetTypeInfo().IsEnum;
#else
            return type.IsEnum || type.UnderlyingSystemType.IsEnum;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodInfo[] GetMethodInfos(this Type type)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetRuntimeMethods().ToArray();
#else
            return type.GetMethods();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PropertyInfo[] GetPropertyInfos(this Type type)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetRuntimeProperties().ToArray();
#else
            return type.GetProperties();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsGenericTypeDefinition(this Type type)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetTypeInfo().IsGenericTypeDefinition;
#else
            return type.IsGenericTypeDefinition;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsGenericType(this Type type)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetTypeInfo().IsGenericType;
#else
            return type.IsGenericType;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsGenericParameters(this Type type)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
            return type.GetTypeInfo().ContainsGenericParameters;
#else
            return type.ContainsGenericParameters;
#endif
        }

#if (NETFX_CORE)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object GetDefaultValue(this Type type)
        {
            return type.GetTypeInfo().IsValueType ? Activator.CreateInstance(type) : null;
        }
#endif

#if NETFX_CORE || NETSTANDARD1_1
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PropertyInfo GetProperty(this Type type, String propertyName)
        {
            return type.GetTypeInfo().GetDeclaredProperty(propertyName);
        }
#endif

#if NETFX_CORE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MethodInfo GetMethod(this Type type, String methodName)
        {
            return type.GetTypeInfo().GetDeclaredMethod(methodName);
        }
#endif

#if SL5 || NETFX_CORE || PCL || NETSTANDARD1_1
        public static List<U> ConvertAll<T, U>(this List<T> list, Func<T, U> converter)
        {
            var result = new List<U>();
            foreach (var element in list)
            {
                result.Add(converter(element));
            }
            return result;
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetDeclaringTypeName(this Type type)
        {
            if (type.DeclaringType != null)
                return type.DeclaringType.Name;

#if !(NETFX_CORE || WP || PCL || NETSTANDARD1_1)
            if (type.ReflectedType != null)
                return type.ReflectedType.Name;
#endif

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetDeclaringTypeName(this MemberInfo mi)
        {
            if (mi.DeclaringType != null)
                return mi.DeclaringType.Name;

#if !(NETFX_CORE || WP || PCL || NETSTANDARD1_1)
            return mi.ReflectedType.Name;
#endif

            return null;
        }

        public static Delegate CreateDelegate(this MethodInfo methodInfo, Type delegateType)
        {
#if PCL || NETSTANDARD1_1
            return methodInfo.CreateDelegate(delegateType);
#else
            return Delegate.CreateDelegate(delegateType, methodInfo);
#endif
        }

        public static Delegate CreateDelegate(this MethodInfo methodInfo, Type delegateType, object target)
        {
#if PCL || NETSTANDARD1_1
            return methodInfo.CreateDelegate(delegateType, target);
#else
            return Delegate.CreateDelegate(delegateType, target, methodInfo);
#endif
        }

        public static Type ElementType(this Type type)
        {
#if PCL
            return type.GetTypeInfo().GetElementType();
#else
            return type.GetElementType();
#endif
        }

        public static Type GetCollectionType(this Type type)
        {
            return type.ElementType() ?? type.GetTypeGenericArguments().FirstOrDefault();
        }

        static Dictionary<string, Type> GenericTypeCache = new Dictionary<string, Type>();

        public static Type GetCachedGenericType(this Type type, params Type[] argTypes)
        {
            if (!type.IsGenericTypeDefinition())
                throw new ArgumentException(type.FullName + " is not a Generic Type Definition");

            if (argTypes == null)
                argTypes = TypeConstants.EmptyTypeArray;

            var sb = StringBuilderThreadStatic.Allocate()
                .Append(type.FullName);

            foreach (var argType in argTypes)
            {
                sb.Append('|')
                    .Append(argType.FullName);
            }

            var key = StringBuilderThreadStatic.ReturnAndFree(sb);

            Type genericType;
            if (GenericTypeCache.TryGetValue(key, out genericType))
                return genericType;

            genericType = type.MakeGenericType(argTypes);

            Dictionary<string, Type> snapshot, newCache;
            do
            {
                snapshot = GenericTypeCache;
                newCache = new Dictionary<string, Type>(GenericTypeCache);
                newCache[key] = genericType;

            } while (!ReferenceEquals(
                Interlocked.CompareExchange(ref GenericTypeCache, newCache, snapshot), snapshot));

            return genericType;
        }

        private static readonly ConcurrentDictionary<Type, ObjectDictionaryDefinition> toObjectMapCache =
            new ConcurrentDictionary<Type, ObjectDictionaryDefinition>();

        internal class ObjectDictionaryDefinition
        {
            public Type Type;
            public readonly List<ObjectDictionaryFieldDefinition> Fields = new List<ObjectDictionaryFieldDefinition>();
            public readonly Dictionary<string, ObjectDictionaryFieldDefinition> FieldsMap = new Dictionary<string, ObjectDictionaryFieldDefinition>();

            public void Add(string name, ObjectDictionaryFieldDefinition fieldDef)
            {
                Fields.Add(fieldDef);
                FieldsMap[name] = fieldDef;
            }
        }

        internal class ObjectDictionaryFieldDefinition
        {
            public string Name;
            public Type Type;

            public PropertyGetterDelegate GetValueFn;
            public PropertySetterDelegate SetValueFn;

            public Type ConvertType;
            public PropertyGetterDelegate ConvertValueFn;

            public void SetValue(object instance, object value)
            {
                if (SetValueFn == null)
                    return;

                if (!Type.InstanceOfType(value))
                {
                    lock (this)
                    {
                        //Only caches object converter used on first use
                        if (ConvertType == null)
                        {
                            ConvertType = value.GetType();
                            ConvertValueFn = TypeConverter.CreateTypeConverter(ConvertType, Type);
                        }
                    }

                    if (ConvertType.InstanceOfType(value))
                    {
                        value = ConvertValueFn(value);
                    }
                    else
                    {
                        var tempConvertFn = TypeConverter.CreateTypeConverter(value.GetType(), Type);
                        value = tempConvertFn(value);
                    }
                }

                SetValueFn(instance, value);
            }
        }

        public static Dictionary<string, object> ToObjectDictionary(this object obj)
        {
            if (obj == null)
                return null;

            var alreadyDict = obj as Dictionary<string, object>;
            if (alreadyDict != null)
                return alreadyDict;

            var interfaceDict = obj as IDictionary<string, object>;
            if (interfaceDict != null)
                return new Dictionary<string, object>(interfaceDict);

            var type = obj.GetType();

            ObjectDictionaryDefinition def;
            if (!toObjectMapCache.TryGetValue(type, out def))
                toObjectMapCache[type] = def = CreateObjectDictionaryDefinition(type);

            var dict = new Dictionary<string, object>();

            foreach (var fieldDef in def.Fields)
            {
                dict[fieldDef.Name] = fieldDef.GetValueFn(obj);
            }

            return dict;
        }

        public static object FromObjectDictionary(this Dictionary<string, object> values, Type type)
        {
            var alreadyDict = type == typeof(Dictionary<string, object>);
            if (alreadyDict)
                return alreadyDict;

            ObjectDictionaryDefinition def;
            if (!toObjectMapCache.TryGetValue(type, out def))
                toObjectMapCache[type] = def = CreateObjectDictionaryDefinition(type);

            var to = type.CreateInstance();
            foreach (var entry in values)
            {
                ObjectDictionaryFieldDefinition fieldDef;
                if (!def.FieldsMap.TryGetValue(entry.Key, out fieldDef) || entry.Value == null)
                    continue;

                fieldDef.SetValue(to, entry.Value);
            }
            return to;
        }

        public static object FromObjectDictionary<T>(this Dictionary<string, object> values)
        {
            return values.FromObjectDictionary(typeof(T));
        }

        private static ObjectDictionaryDefinition CreateObjectDictionaryDefinition(Type type)
        {
            var def = new ObjectDictionaryDefinition
            {
                Type = type,
            };

            foreach (var pi in type.GetSerializableProperties())
            {
                def.Add(pi.Name, new ObjectDictionaryFieldDefinition
                {
                    Name = pi.Name,
                    Type = pi.PropertyType,
                    GetValueFn = pi.GetPropertyGetterFn(),
                    SetValueFn = pi.GetPropertySetterFn(),
                });
            }

            if (JsConfig.IncludePublicFields)
            {
                foreach (var fi in type.GetSerializableFields())
                {
                    def.Add(fi.Name, new ObjectDictionaryFieldDefinition
                    {
                        Name = fi.Name,
                        Type = fi.FieldType,
                        GetValueFn = fi.GetFieldGetterFn(),
                        SetValueFn = fi.GetFieldSetterFn(),
                    });
                }
            }
            return def;
        }

        public static Dictionary<string, object> ToSafePartialObjectDictionary<T>(this T instance)
        {
            var to = new Dictionary<string, object>();
            var propValues = instance.ToObjectDictionary();
            if (propValues != null)
            {
                foreach (var entry in propValues)
                {
                    var valueType = entry.Value != null 
                        ? entry.Value.GetType() 
                        : null;

                    if (valueType == null || !valueType.IsClass() || valueType == typeof(string))
                    {
                        to[entry.Key] = entry.Value;
                    }
                    else if (!TypeSerializer.HasCircularReferences(entry.Value))
                    {
                        var enumerable = entry.Value as IEnumerable;
                        if (enumerable != null)
                        {
                            to[entry.Key] = entry.Value;
                        }
                        else
                        {
                            to[entry.Key] = entry.Value.ToSafePartialObjectDictionary();
                        }
                    }
                    else
                    {
                        to[entry.Key] = entry.Value.ToString();
                    }
                }
            }
            return to;
        }
    }
}