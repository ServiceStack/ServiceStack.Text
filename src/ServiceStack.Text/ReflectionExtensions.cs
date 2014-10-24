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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using ServiceStack.Text.Support;

using ServiceStack.Text;

namespace ServiceStack
{
    public delegate EmptyCtorDelegate EmptyCtorFactoryDelegate(Type type);
    public delegate object EmptyCtorDelegate();

#if (NETFX_CORE || PCL)

    public enum TypeCode
    {
        Empty = 0,
        Object = 1,
        DBNull = 2,
        Boolean = 3,
        Char = 4,
        SByte = 5,
        Byte = 6,
        Int16 = 7,
        UInt16 = 8,
        Int32 = 9,
        UInt32 = 10,
        Int64 = 11,
        UInt64 = 12,
        Single = 13,
        Double = 14,
        Decimal = 15,
        DateTime = 16,
        String = 18,
    }
#endif

    public static class ReflectionExtensions
    {
#if (NETFX_CORE || PCL)
        private static readonly Dictionary<Type, TypeCode> _typeCodeTable =
        new Dictionary<Type, TypeCode>
        {
            { typeof(Boolean), TypeCode.Boolean },
            { typeof(Char), TypeCode.Char },
            { typeof(Byte), TypeCode.Byte },
            { typeof(Int16), TypeCode.Int16 },
            { typeof(Int32), TypeCode.Int32 },
            { typeof(Int64), TypeCode.Int64 },
            { typeof(SByte), TypeCode.SByte },
            { typeof(UInt16), TypeCode.UInt16 },
            { typeof(UInt32), TypeCode.UInt32 },
            { typeof(UInt64), TypeCode.UInt64 },
            { typeof(Single), TypeCode.Single },
            { typeof(Double), TypeCode.Double },
            { typeof(DateTime), TypeCode.DateTime },
            { typeof(Decimal), TypeCode.Decimal },
            { typeof(String), TypeCode.String },
        };

        public static Type[] GetGenericArguments(this Type type)
        {
            return type.GetTypeInfo().GenericTypeArguments;
        }

        public static TypeInfo GetTypeInfo(this Type type)
        {
            IReflectableType reflectableType = (IReflectableType)type;
            return reflectableType.GetTypeInfo();
        }
#endif

        public static TypeCode GetTypeCode(this Type type)
        {
#if (NETFX_CORE || PCL)
            if (type == null)
            {
                return TypeCode.Empty;
            }

            TypeCode result;
            if (!_typeCodeTable.TryGetValue(type, out result))
            {
                result = TypeCode.Object;
            }

            return result;
#else
            return Type.GetTypeCode(type);
#endif
        }

        public static bool IsInstanceOf(this Type type, Type thisOrBaseType)
        {
            while (type != null)
            {
                if (type == thisOrBaseType)
                    return true;

                type = type.BaseType();
            }
            return false;
        }

        public static bool HasGenericType(this Type type)
        {
            while (type != null)
            {
                if (type.IsGeneric())
                    return true;

                type = type.BaseType();
            }
            return false;
        }

        public static Type FirstGenericType(this Type type)
        {
            while (type != null)
            {
                if (type.IsGeneric())
                    return type;

                type = type.BaseType();
            }
            return null;
        }

        public static Type GetTypeWithGenericTypeDefinitionOfAny(this Type type, params Type[] genericTypeDefinitions)
        {
            foreach (var genericTypeDefinition in genericTypeDefinitions)
            {
                var genericType = type.GetTypeWithGenericTypeDefinitionOf(genericTypeDefinition);
                if (genericType == null && type == genericTypeDefinition)
                {
                    genericType = type;
                }

                if (genericType != null)
                    return genericType;
            }
            return null;
        }

        public static bool IsOrHasGenericInterfaceTypeOf(this Type type, Type genericTypeDefinition)
        {
            return (type.GetTypeWithGenericTypeDefinitionOf(genericTypeDefinition) != null)
                || (type == genericTypeDefinition);
        }

        public static Type GetTypeWithGenericTypeDefinitionOf(this Type type, Type genericTypeDefinition)
        {
            foreach (var t in type.GetTypeInterfaces())
            {
                if (t.IsGeneric() && t.GetGenericTypeDefinition() == genericTypeDefinition)
                {
                    return t;
                }
            }

            var genericType = type.FirstGenericType();
            if (genericType != null && genericType.GetGenericTypeDefinition() == genericTypeDefinition)
            {
                return genericType;
            }

            return null;
        }

        public static Type GetTypeWithInterfaceOf(this Type type, Type interfaceType)
        {
            if (type == interfaceType) return interfaceType;

            foreach (var t in type.GetTypeInterfaces())
            {
                if (t == interfaceType)
                    return t;
            }

            return null;
        }

        public static bool HasInterface(this Type type, Type interfaceType)
        {
            foreach (var t in type.GetTypeInterfaces())
            {
                if (t == interfaceType)
                    return true;
            }
            return false;
        }

        public static bool AllHaveInterfacesOfType(
            this Type assignableFromType, params Type[] types)
        {
            foreach (var type in types)
            {
                if (assignableFromType.GetTypeWithInterfaceOf(type) == null) return false;
            }
            return true;
        }

        public static bool IsNullableType(this Type type)
        {
            return type.IsGenericType() && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static TypeCode GetUnderlyingTypeCode(this Type type)
        {
            return GetTypeCode(Nullable.GetUnderlyingType(type) ?? type);
        }

        public static bool IsNumericType(this Type type)
        {
            if (type == null) return false;

            if (type.IsEnum()) //TypeCode can be TypeCode.Int32
            {
                return JsConfig.TreatEnumAsInteger || type.IsEnumFlags();
            }

            switch (GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;

                case TypeCode.Object:
                    if (type.IsNullableType())
                    {
                        return IsNumericType(Nullable.GetUnderlyingType(type));
                    }
                    if (type.IsEnum())
                    {
                        return JsConfig.TreatEnumAsInteger || type.IsEnumFlags();
                    }
                    return false;
            }
            return false;
        }

        public static bool IsIntegerType(this Type type)
        {
            if (type == null) return false;

            switch (GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;

                case TypeCode.Object:
                    if (type.IsNullableType())
                    {
                        return IsNumericType(Nullable.GetUnderlyingType(type));
                    }
                    return false;
            }
            return false;
        }

        public static bool IsRealNumberType(this Type type)
        {
            if (type == null) return false;

            switch (GetTypeCode(type))
            {
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;

                case TypeCode.Object:
                    if (type.IsNullableType())
                    {
                        return IsNumericType(Nullable.GetUnderlyingType(type));
                    }
                    return false;
            }
            return false;
        }

        public static Type GetTypeWithGenericInterfaceOf(this Type type, Type genericInterfaceType)
        {
            foreach (var t in type.GetTypeInterfaces())
            {
                if (t.IsGeneric() && t.GetGenericTypeDefinition() == genericInterfaceType)
                    return t;
            }

            if (!type.IsGeneric()) return null;

            var genericType = type.FirstGenericType();
            return genericType.GetGenericTypeDefinition() == genericInterfaceType
                    ? genericType
                    : null;
        }

        public static bool HasAnyTypeDefinitionsOf(this Type genericType, params Type[] theseGenericTypes)
        {
            if (!genericType.IsGeneric()) return false;

            var genericTypeDefinition = genericType.GenericTypeDefinition();

            foreach (var thisGenericType in theseGenericTypes)
            {
                if (genericTypeDefinition == thisGenericType)
                    return true;
            }

            return false;
        }

        public static Type[] GetGenericArgumentsIfBothHaveSameGenericDefinitionTypeAndArguments(
            this Type assignableFromType, Type typeA, Type typeB)
        {
            var typeAInterface = typeA.GetTypeWithGenericInterfaceOf(assignableFromType);
            if (typeAInterface == null) return null;

            var typeBInterface = typeB.GetTypeWithGenericInterfaceOf(assignableFromType);
            if (typeBInterface == null) return null;

            var typeAGenericArgs = typeAInterface.GetTypeGenericArguments();
            var typeBGenericArgs = typeBInterface.GetTypeGenericArguments();

            if (typeAGenericArgs.Length != typeBGenericArgs.Length) return null;

            for (var i = 0; i < typeBGenericArgs.Length; i++)
            {
                if (typeAGenericArgs[i] != typeBGenericArgs[i])
                {
                    return null;
                }
            }

            return typeAGenericArgs;
        }

        public static TypePair GetGenericArgumentsIfBothHaveConvertibleGenericDefinitionTypeAndArguments(
            this Type assignableFromType, Type typeA, Type typeB)
        {
            var typeAInterface = typeA.GetTypeWithGenericInterfaceOf(assignableFromType);
            if (typeAInterface == null) return null;

            var typeBInterface = typeB.GetTypeWithGenericInterfaceOf(assignableFromType);
            if (typeBInterface == null) return null;

            var typeAGenericArgs = typeAInterface.GetTypeGenericArguments();
            var typeBGenericArgs = typeBInterface.GetTypeGenericArguments();

            if (typeAGenericArgs.Length != typeBGenericArgs.Length) return null;

            for (var i = 0; i < typeBGenericArgs.Length; i++)
            {
                if (!AreAllStringOrValueTypes(typeAGenericArgs[i], typeBGenericArgs[i]))
                {
                    return null;
                }
            }

            return new TypePair(typeAGenericArgs, typeBGenericArgs);
        }

        public static bool AreAllStringOrValueTypes(params Type[] types)
        {
            foreach (var type in types)
            {
                if (!(type == typeof(string) || type.IsValueType())) return false;
            }
            return true;
        }

        static Dictionary<Type, EmptyCtorDelegate> ConstructorMethods = new Dictionary<Type, EmptyCtorDelegate>();
        public static EmptyCtorDelegate GetConstructorMethod(Type type)
        {
            EmptyCtorDelegate emptyCtorFn;
            if (ConstructorMethods.TryGetValue(type, out emptyCtorFn)) return emptyCtorFn;

            emptyCtorFn = GetConstructorMethodToCache(type);

            Dictionary<Type, EmptyCtorDelegate> snapshot, newCache;
            do
            {
                snapshot = ConstructorMethods;
                newCache = new Dictionary<Type, EmptyCtorDelegate>(ConstructorMethods);
                newCache[type] = emptyCtorFn;

            } while (!ReferenceEquals(
                Interlocked.CompareExchange(ref ConstructorMethods, newCache, snapshot), snapshot));

            return emptyCtorFn;
        }

        static Dictionary<string, EmptyCtorDelegate> TypeNamesMap = new Dictionary<string, EmptyCtorDelegate>();
        public static EmptyCtorDelegate GetConstructorMethod(string typeName)
        {
            EmptyCtorDelegate emptyCtorFn;
            if (TypeNamesMap.TryGetValue(typeName, out emptyCtorFn)) return emptyCtorFn;

            var type = JsConfig.TypeFinder(typeName);
            if (type == null) return null;
            emptyCtorFn = GetConstructorMethodToCache(type);

            Dictionary<string, EmptyCtorDelegate> snapshot, newCache;
            do
            {
                snapshot = TypeNamesMap;
                newCache = new Dictionary<string, EmptyCtorDelegate>(TypeNamesMap);
                newCache[typeName] = emptyCtorFn;

            } while (!ReferenceEquals(
                Interlocked.CompareExchange(ref TypeNamesMap, newCache, snapshot), snapshot));

            return emptyCtorFn;
        }

        public static EmptyCtorDelegate GetConstructorMethodToCache(Type type)
        {
            if (type.IsInterface())
            {
                if (type.HasGenericType())
                {
                    var genericType = type.GetTypeWithGenericTypeDefinitionOfAny(
                        typeof(IDictionary<,>));

                    if (genericType != null)
                    {
                        var keyType = genericType.GenericTypeArguments()[0];
                        var valueType = genericType.GenericTypeArguments()[1];
                        return GetConstructorMethodToCache(typeof(Dictionary<,>).MakeGenericType(keyType, valueType));
                    }

                    genericType = type.GetTypeWithGenericTypeDefinitionOfAny(
                        typeof(IEnumerable<>),
                        typeof(ICollection<>),
                        typeof(IList<>));

                    if (genericType != null)
                    {
                        var elementType = genericType.GenericTypeArguments()[0];
                        return GetConstructorMethodToCache(typeof(List<>).MakeGenericType(elementType));
                    }
                }
            }
            else if (type.IsArray)
            {
                return () => Array.CreateInstance(type.GetElementType(), 0);
            }
            else if (type.IsGenericTypeDefinition())
            {
                var genericArgs = type.GetGenericArguments();
                var typeArgs = new Type[genericArgs.Length];
                for (var i = 0; i < genericArgs.Length; i++)
                    typeArgs[i] = typeof(object);

                var realizedType = type.MakeGenericType(typeArgs);
                return realizedType.CreateInstance;
            }

            var emptyCtor = type.GetEmptyConstructor();
            if (emptyCtor != null)
            {

#if __IOS__ || XBOX || NETFX_CORE
				return () => Activator.CreateInstance(type);
#elif WP || PCL
                return System.Linq.Expressions.Expression.Lambda<EmptyCtorDelegate>(
                    System.Linq.Expressions.Expression.New(type)).Compile();
#else

#if SL5 
                var dm = new System.Reflection.Emit.DynamicMethod("MyCtor", type, Type.EmptyTypes);
#else
                var dm = new System.Reflection.Emit.DynamicMethod("MyCtor", type, Type.EmptyTypes, typeof(ReflectionExtensions).Module, true);
#endif
                var ilgen = dm.GetILGenerator();
                ilgen.Emit(System.Reflection.Emit.OpCodes.Nop);
                ilgen.Emit(System.Reflection.Emit.OpCodes.Newobj, emptyCtor);
                ilgen.Emit(System.Reflection.Emit.OpCodes.Ret);

                return (EmptyCtorDelegate)dm.CreateDelegate(typeof(EmptyCtorDelegate));
#endif
            }

#if (SL5 && !WP) || XBOX
            return () => Activator.CreateInstance(type);
#elif WP || PCL
            return System.Linq.Expressions.Expression.Lambda<EmptyCtorDelegate>(
                System.Linq.Expressions.Expression.New(type)).Compile();
#else
            if (type == typeof(string))
                return () => String.Empty;

            //Anonymous types don't have empty constructors
            return () => FormatterServices.GetUninitializedObject(type);
#endif
        }

        private static class TypeMeta<T>
        {
            public static readonly EmptyCtorDelegate EmptyCtorFn;
            static TypeMeta()
            {
                EmptyCtorFn = GetConstructorMethodToCache(typeof(T));
            }
        }

        public static object CreateInstance<T>()
        {
            return TypeMeta<T>.EmptyCtorFn();
        }

        /// <summary>
        /// Creates a new instance of type. 
        /// First looks at JsConfig.ModelFactory before falling back to CreateInstance
        /// </summary>
        public static T New<T>(this Type type)
        {
            var factoryFn = JsConfig.ModelFactory(type)
                ?? GetConstructorMethod(type);
            return (T)factoryFn();
        }

        /// <summary>
        /// Creates a new instance of type. 
        /// First looks at JsConfig.ModelFactory before falling back to CreateInstance
        /// </summary>
        public static object New(this Type type)
        {
            var factoryFn = JsConfig.ModelFactory(type)
                ?? GetConstructorMethod(type);
            return factoryFn();
        }

        /// <summary>
        /// Creates a new instance from the default constructor of type
        /// </summary>
        public static object CreateInstance(this Type type)
        {
            if (type == null)
                return null;

            var ctorFn = GetConstructorMethod(type);
            return ctorFn();
        }

        public static T CreateInstance<T>(this Type type)
        {
            if (type == null)
                return default(T);

            var ctorFn = GetConstructorMethod(type);
            return (T)ctorFn();
        }

        public static object CreateInstance(string typeName)
        {
            if (typeName == null)
                return null;

            var ctorFn = GetConstructorMethod(typeName);
            return ctorFn();
        }

        public static PropertyInfo[] GetPublicProperties(this Type type)
        {
            if (type.IsInterface())
            {
                var propertyInfos = new List<PropertyInfo>();

                var considered = new List<Type>();
                var queue = new Queue<Type>();
                considered.Add(type);
                queue.Enqueue(type);

                while (queue.Count > 0)
                {
                    var subType = queue.Dequeue();
                    foreach (var subInterface in subType.GetTypeInterfaces())
                    {
                        if (considered.Contains(subInterface)) continue;

                        considered.Add(subInterface);
                        queue.Enqueue(subInterface);
                    }

                    var typeProperties = subType.GetTypesPublicProperties();

                    var newPropertyInfos = typeProperties
                        .Where(x => !propertyInfos.Contains(x));

                    propertyInfos.InsertRange(0, newPropertyInfos);
                }

                return propertyInfos.ToArray();
            }

            return type.GetTypesPublicProperties()
                .Where(t => t.GetIndexParameters().Length == 0) // ignore indexed properties
                .ToArray();
        }

        public const string DataMember = "DataMemberAttribute";

        internal static string[] IgnoreAttributesNamed = new[] {
            "IgnoreDataMemberAttribute",
            "JsonIgnoreAttribute"
        };

        internal static void Reset()
        {
            IgnoreAttributesNamed = new[] {
                "IgnoreDataMemberAttribute",
                "JsonIgnoreAttribute"
            };
        }

        public static PropertyInfo[] GetSerializableProperties(this Type type)
        {
            var publicProperties = GetPublicProperties(type);
            var publicReadableProperties = publicProperties.Where(x => x.PropertyGetMethod() != null);

            if (type.IsDto())
            {
                return publicReadableProperties.Where(attr =>
                    attr.HasAttribute<DataMemberAttribute>()).ToArray();
            }

            // else return those properties that are not decorated with IgnoreDataMember
            return publicReadableProperties
                .Where(prop => prop.AllAttributes().All(attr => {
                        var name = attr.GetType().Name;
                        return !IgnoreAttributesNamed.Contains(name);
                    }))
                .Where(prop => !JsConfig.ExcludeTypes.Contains(prop.PropertyType))
                .ToArray();
        }

        public static Func<object, string, object, object> GetOnDeserializing<T>()
        {
            var method = typeof(T).GetMethodInfo("OnDeserializing");
            if (method == null || method.ReturnType != typeof(object))
                return null;
            var obj = (Func<T, string, object, object>)method.CreateDelegate(typeof(Func<T, string, object, object>));
            return (instance, memberName, value) => obj((T)instance, memberName, value);
        }

        public static FieldInfo[] GetSerializableFields(this Type type)
        {
            if (type.IsDto())
            {
                return type.GetAllFields().Where(attr =>
                    attr.HasAttribute<DataMemberAttribute>()).ToArray();
            }

            if (!JsConfig.IncludePublicFields)
                return new FieldInfo[0];

            var publicFields = type.GetPublicFields();

            // else return those properties that are not decorated with IgnoreDataMember
            return publicFields
                .Where(prop => prop.AllAttributes()
                    .All(attr => !IgnoreAttributesNamed.Contains(attr.GetType().Name)))
                .Where(prop => !JsConfig.ExcludeTypes.Contains(prop.FieldType))
                .ToArray();
        }

        public static DataContractAttribute GetDataContract(this Type type)
        {
            var dataContract = type.FirstAttribute<DataContractAttribute>();

            if (dataContract == null && Env.IsMono)
                return PclExport.Instance.GetWeakDataContract(type);

            return dataContract;
        }

        public static DataMemberAttribute GetDataMember(this PropertyInfo pi)
        {
            var dataMember = pi.AllAttributes(typeof(DataMemberAttribute))
                .FirstOrDefault() as DataMemberAttribute;

            if (dataMember == null && Env.IsMono)
                return PclExport.Instance.GetWeakDataMember(pi);

            return dataMember;
        }

        public static DataMemberAttribute GetDataMember(this FieldInfo pi)
        {
            var dataMember = pi.AllAttributes(typeof(DataMemberAttribute))
                .FirstOrDefault() as DataMemberAttribute;

            if (dataMember == null && Env.IsMono)
                return PclExport.Instance.GetWeakDataMember(pi);

            return dataMember;
        }
    }

    public static class PlatformExtensions //Because WinRT is a POS
    {
        public static bool IsInterface(this Type type)
        {
#if (NETFX_CORE || PCL)
            return type.GetTypeInfo().IsInterface;
#else
            return type.IsInterface;
#endif
        }

        public static bool IsArray(this Type type)
        {
#if (NETFX_CORE || PCL)
            return type.GetTypeInfo().IsArray;
#else
            return type.IsArray;
#endif
        }

        public static bool IsValueType(this Type type)
        {
#if (NETFX_CORE || PCL)
            return type.GetTypeInfo().IsValueType;
#else
            return type.IsValueType;
#endif
        }

        public static bool IsGeneric(this Type type)
        {
#if (NETFX_CORE || PCL)
            return type.GetTypeInfo().IsGenericType;
#else
            return type.IsGenericType;
#endif
        }

        public static Type BaseType(this Type type)
        {
#if (NETFX_CORE || PCL)
            return type.GetTypeInfo().BaseType;
#else
            return type.BaseType;
#endif
        }

        public static Type ReflectedType(this PropertyInfo pi)
        {
#if (NETFX_CORE || PCL)
            return pi.DeclaringType;
#else
            return pi.ReflectedType;
#endif
        }

        public static Type ReflectedType(this FieldInfo fi)
        {
#if (NETFX_CORE || PCL)
            return fi.DeclaringType;
#else
            return fi.ReflectedType;
#endif
        }

        public static Type GenericTypeDefinition(this Type type)
        {
#if (NETFX_CORE || PCL)
            return type.GetTypeInfo().GetGenericTypeDefinition();
#else
            return type.GetGenericTypeDefinition();
#endif
        }

        public static Type[] GetTypeInterfaces(this Type type)
        {
#if (NETFX_CORE || PCL)
            return type.GetTypeInfo().ImplementedInterfaces.ToArray();
#else
            return type.GetInterfaces();
#endif
        }

        public static Type[] GetTypeGenericArguments(this Type type)
        {
#if (NETFX_CORE || PCL)
            return type.GenericTypeArguments;
#else
            return type.GetGenericArguments();
#endif
        }

        public static ConstructorInfo GetEmptyConstructor(this Type type)
        {
#if (NETFX_CORE || PCL)
            return type.GetTypeInfo().DeclaredConstructors.FirstOrDefault(c => c.GetParameters().Count() == 0);
#else
            return type.GetConstructor(Type.EmptyTypes);
#endif
        }

        internal static PropertyInfo[] GetTypesPublicProperties(this Type subType)
        {
#if (NETFX_CORE || PCL)
            return subType.GetRuntimeProperties().ToArray();
#else
            return subType.GetProperties(
                BindingFlags.FlattenHierarchy |
                BindingFlags.Public |
                BindingFlags.Instance);
#endif
        }

        public static Assembly GetAssembly(this Type type)
        {
#if (NETFX_CORE || PCL)
            return type.GetTypeInfo().Assembly;
#else
            return type.Assembly;
#endif
        }

        public static MethodInfo GetMethod(this Type type, string methodName)
        {
#if (NETFX_CORE || PCL)
            return type.GetTypeInfo().GetDeclaredMethod(methodName);
#else
            return type.GetMethod(methodName);
#endif
        }

        public static FieldInfo[] Fields(this Type type)
        {
#if (NETFX_CORE || PCL)
            return type.GetRuntimeFields().ToArray();
#else
            return type.GetFields();
#endif
        }

        public static PropertyInfo[] Properties(this Type type)
        {
#if (NETFX_CORE || PCL)
            return type.GetRuntimeProperties().ToArray();
#else
            return type.GetProperties();
#endif
        }

        public static FieldInfo[] GetAllFields(this Type type)
        {
            if (type.IsInterface())
            {
                return new FieldInfo[0];
            }

#if (NETFX_CORE || PCL)
            return type.GetRuntimeFields().ToArray();
#else
            return type.GetPublicFields();
#endif
        }

        public static FieldInfo[] GetPublicFields(this Type type)
        {
            if (type.IsInterface())
            {
                return new FieldInfo[0];
            }

#if (NETFX_CORE || PCL)
            return type.GetRuntimeFields().Where(p => p.IsPublic && !p.IsStatic).ToArray();
#else
            return type.GetFields(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance)
            .ToArray();
#endif
        }

        public static MemberInfo[] GetPublicMembers(this Type type)
        {

#if (NETFX_CORE || PCL)
            var members = new List<MemberInfo>();
            members.AddRange(type.GetRuntimeFields().Where(p => p.IsPublic && !p.IsStatic));
            members.AddRange(type.GetPublicProperties());
            return members.ToArray();
#else
            return type.GetMembers(BindingFlags.Public | BindingFlags.Instance);
#endif
        }

        public static MemberInfo[] GetAllPublicMembers(this Type type)
        {

#if (NETFX_CORE || PCL)
            var members = new List<MemberInfo>();
            members.AddRange(type.GetRuntimeFields().Where(p => p.IsPublic && !p.IsStatic));
            members.AddRange(type.GetPublicProperties());
            return members.ToArray();
#else
            return type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
#endif
        }

        public static MethodInfo GetStaticMethod(this Type type, string methodName)
        {
#if (NETFX_CORE || PCL)
            return type.GetMethodInfo(methodName);
#else
            return type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
#endif
        }

        public static MethodInfo GetInstanceMethod(this Type type, string methodName)
        {
#if (NETFX_CORE || PCL)
            return type.GetMethodInfo(methodName);
#else
            return type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
#endif
        }

        public static MethodInfo Method(this Delegate fn)
        {
#if NETFX_CORE || PCL
            return fn.GetMethodInfo();
#else
            return fn.Method;
#endif
        }

        public static bool HasAttribute<T>(this Type type)
        {
            return type.AllAttributes().Any(x => x.GetType() == typeof(T));
        }

        public static bool HasAttribute<T>(this PropertyInfo pi)
        {
            return pi.AllAttributes().Any(x => x.GetType() == typeof(T));
        }

        public static bool HasAttribute<T>(this FieldInfo fi)
        {
            return fi.AllAttributes().Any(x => x.GetType() == typeof(T));
        }

        public static bool HasAttributeNamed(this Type type, string name)
        {
            var normalizedAttr = name.Replace("Attribute", "").ToLower();
            return type.AllAttributes().Any(x => x.GetType().Name.Replace("Attribute", "").ToLower() == normalizedAttr);
        }

        public static bool HasAttributeNamed(this PropertyInfo pi, string name)
        {
            var normalizedAttr = name.Replace("Attribute", "").ToLower();
            return pi.AllAttributes().Any(x => x.GetType().Name.Replace("Attribute", "").ToLower() == normalizedAttr);
        }

        public static bool HasAttributeNamed(this FieldInfo fi, string name)
        {
            var normalizedAttr = name.Replace("Attribute", "").ToLower();
            return fi.AllAttributes().Any(x => x.GetType().Name.Replace("Attribute", "").ToLower() == normalizedAttr);
        }

        public static bool HasAttributeNamed(this MemberInfo mi, string name)
        {
            var normalizedAttr = name.Replace("Attribute", "").ToLower();
            return mi.AllAttributes().Any(x => x.GetType().Name.Replace("Attribute", "").ToLower() == normalizedAttr);
        }

        const string DataContract = "DataContractAttribute";
        public static bool IsDto(this Type type)
        {
#if (NETFX_CORE || PCL)
            return type.HasAttribute<DataContractAttribute>();
#else
            return !Env.IsMono
                   ? type.HasAttribute<DataContractAttribute>()
                   : type.GetCustomAttributes(true).Any(x => x.GetType().Name == DataContract);
#endif
        }

        public static MethodInfo PropertyGetMethod(this PropertyInfo pi, bool nonPublic = false)
        {
#if (NETFX_CORE || PCL)
            return pi.GetMethod;
#else
            return pi.GetGetMethod(false);
#endif
        }

        public static Type[] Interfaces(this Type type)
        {
#if (NETFX_CORE || PCL)
            return type.GetTypeInfo().ImplementedInterfaces.ToArray();
            //return type.GetTypeInfo().ImplementedInterfaces
            //    .FirstOrDefault(x => !x.GetTypeInfo().ImplementedInterfaces
            //        .Any(y => y.GetTypeInfo().ImplementedInterfaces.Contains(y)));
#else
            return type.GetInterfaces();
#endif
        }

        public static PropertyInfo[] AllProperties(this Type type)
        {
#if (NETFX_CORE || PCL)
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
                : propertyAttrs.Where(x => attrType.IsInstanceOf(x.GetType()) ).ToList();
        }

        public static object[] AllAttributes(this PropertyInfo propertyInfo)
        {
#if (NETFX_CORE || PCL)
            return propertyInfo.GetCustomAttributes(true).ToArray();
#else
            var attrs = propertyInfo.GetCustomAttributes(true);
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
            return propertyInfo.GetCustomAttributes(true).Where(x => attrType.IsInstanceOf(x.GetType())).ToArray();
#else
            var attrs = propertyInfo.GetCustomAttributes(attrType, true);
            var runtimeAttrs = propertyInfo.GetAttributes(attrType);
            if (runtimeAttrs.Count == 0)
                return attrs;

            runtimeAttrs.AddRange(attrs.Cast<Attribute>());
            return runtimeAttrs.Cast<object>().ToArray();
#endif
        }

        public static object[] AllAttributes(this ParameterInfo paramInfo)
        {
#if (NETFX_CORE || PCL)
            return paramInfo.GetCustomAttributes(true).ToArray();
#else
            return paramInfo.GetCustomAttributes(true);
#endif
        }

        public static object[] AllAttributes(this FieldInfo fieldInfo)
        {
#if (NETFX_CORE || PCL)
            return fieldInfo.GetCustomAttributes(true).ToArray();
#else
            return fieldInfo.GetCustomAttributes(true);
#endif
        }

        public static object[] AllAttributes(this MemberInfo memberInfo)
        {
#if (NETFX_CORE || PCL)
            return memberInfo.GetCustomAttributes(true).ToArray();
#else
            return memberInfo.GetCustomAttributes(true);
#endif
        }

        public static object[] AllAttributes(this ParameterInfo paramInfo, Type attrType)
        {
#if (NETFX_CORE || PCL)
            return paramInfo.GetCustomAttributes(true).Where(x => attrType.IsInstanceOf(x.GetType())).ToArray();
#else
            return paramInfo.GetCustomAttributes(attrType, true);
#endif
        }

        public static object[] AllAttributes(this MemberInfo memberInfo, Type attrType)
        {
#if (NETFX_CORE || PCL)
            return memberInfo.GetCustomAttributes(true).Where(x => attrType.IsInstanceOf(x.GetType())).ToArray();
#else
            return memberInfo.GetCustomAttributes(attrType, true);
#endif
        }

        public static object[] AllAttributes(this FieldInfo fieldInfo, Type attrType)
        {
#if (NETFX_CORE || PCL)
            return fieldInfo.GetCustomAttributes(true).Where(x => attrType.IsInstanceOf(x.GetType())).ToArray();
#else
            return fieldInfo.GetCustomAttributes(attrType, true);
#endif
        }

        public static object[] AllAttributes(this Type type)
        {
#if (NETFX_CORE || PCL)
            return type.GetTypeInfo().GetCustomAttributes(true).ToArray();
#else
            return type.GetCustomAttributes(true).Union(type.GetRuntimeAttributes()).ToArray();
#endif
        }

        public static object[] AllAttributes(this Type type, Type attrType)
        {
#if (NETFX_CORE || PCL)
            return type.GetTypeInfo().GetCustomAttributes(true).Where(x => attrType.IsInstanceOf(x.GetType())).ToArray();
#else
            return type.GetCustomAttributes(true).Union(type.GetRuntimeAttributes()).ToArray();
#endif
        }

        public static object[] AllAttributes(this Assembly assembly)
        {
#if (NETFX_CORE || PCL)
            return assembly.GetCustomAttributes().ToArray();
#else
            return assembly.GetCustomAttributes(true).ToArray();
#endif
        }

        public static TAttr[] AllAttributes<TAttr>(this ParameterInfo pi)
        {
            return pi.AllAttributes(typeof(TAttr)).Cast<TAttr>().ToArray();
        }

        public static TAttr[] AllAttributes<TAttr>(this MemberInfo mi)
        {
            return mi.AllAttributes(typeof(TAttr)).Cast<TAttr>().ToArray();
        }

        public static TAttr[] AllAttributes<TAttr>(this FieldInfo fi)
        {
            return fi.AllAttributes(typeof(TAttr)).Cast<TAttr>().ToArray();
        }

        public static TAttr[] AllAttributes<TAttr>(this PropertyInfo pi)
        {
            return pi.AllAttributes(typeof(TAttr)).Cast<TAttr>().ToArray();
        }

        static IEnumerable<T> GetRuntimeAttributes<T>(this Type type)
        {
            List<Attribute> attrs;
            return typeAttributesMap.TryGetValue(type, out attrs)
                ? attrs.OfType<T>()
                : new List<T>();
        }

        static IEnumerable<Attribute> GetRuntimeAttributes(this Type type, Type attrType = null)
        {
            List<Attribute> attrs;
            return typeAttributesMap.TryGetValue(type, out attrs)
                ? attrs.Where(x => attrType == null || attrType.IsInstanceOf(x.GetType()))
                : new List<Attribute>();
        }

        public static TAttr[] AllAttributes<TAttr>(this Type type)
#if (NETFX_CORE || PCL)
            where TAttr : Attribute
#endif
        {
#if (NETFX_CORE || PCL)
            return type.GetTypeInfo().GetCustomAttributes<TAttr>(true).ToArray();
#else
            return type.GetCustomAttributes(typeof(TAttr), true)
                .OfType<TAttr>()
                .Union(type.GetRuntimeAttributes<TAttr>())
                .ToArray();
#endif
        }

        public static TAttr FirstAttribute<TAttr>(this Type type) where TAttr : class
        {
#if (NETFX_CORE || PCL)

            return (TAttr)type.GetTypeInfo().GetCustomAttributes(typeof(TAttr), true)
                    .Cast<TAttr>()
                    .FirstOrDefault();
#else
            return (TAttr)type.GetCustomAttributes(typeof(TAttr), true)
                   .FirstOrDefault()
                   ?? type.GetRuntimeAttributes<TAttr>().FirstOrDefault();
#endif
        }

        public static TAttribute FirstAttribute<TAttribute>(this MemberInfo memberInfo)
        {
            return memberInfo.AllAttributes<TAttribute>().FirstOrDefault();
        }

        public static TAttribute FirstAttribute<TAttribute>(this ParameterInfo paramInfo)
        {
            return paramInfo.AllAttributes<TAttribute>().FirstOrDefault();
        }

        public static TAttribute FirstAttribute<TAttribute>(this PropertyInfo propertyInfo)
        {
            return propertyInfo.AllAttributes<TAttribute>().FirstOrDefault();
        }

        public static Type FirstGenericTypeDefinition(this Type type)
        {
            var genericType = type.FirstGenericType();
            return genericType != null ? genericType.GetGenericTypeDefinition() : null;
        }

        public static bool IsDynamic(this Assembly assembly)
        {
#if __IOS__ || WP || NETFX_CORE || PCL
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

        public static MethodInfo GetStaticMethod(this Type type, string methodName, Type[] types = null)
        {
#if (NETFX_CORE || PCL)
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

        public static MethodInfo GetMethodInfo(this Type type, string methodName, Type[] types = null)
        {
#if (NETFX_CORE || PCL)
            return type.GetRuntimeMethods().FirstOrDefault(p => p.Name.Equals(methodName));
#else
            return types == null
                ? type.GetMethod(methodName)
                : type.GetMethod(methodName, types);
#endif
        }

        public static object InvokeMethod(this Delegate fn, object instance, object[] parameters = null)
        {
#if (NETFX_CORE || PCL)
            return fn.GetMethodInfo().Invoke(instance, parameters ?? new object[] { });
#else
            return fn.Method.Invoke(instance, parameters ?? new object[] { });
#endif
        }

        public static FieldInfo GetPublicStaticField(this Type type, string fieldName)
        {
#if (NETFX_CORE || PCL)
            return type.GetRuntimeField(fieldName);
#else
            return type.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
#endif
        }

        public static Delegate MakeDelegate(this MethodInfo mi, Type delegateType, bool throwOnBindFailure = true)
        {
#if (NETFX_CORE || PCL)
            return mi.CreateDelegate(delegateType);
#else
            return Delegate.CreateDelegate(delegateType, mi, throwOnBindFailure);
#endif
        }

        public static Type[] GenericTypeArguments(this Type type)
        {
#if (NETFX_CORE || PCL)
            return type.GenericTypeArguments;
#else
            return type.GetGenericArguments();
#endif
        }

        public static ConstructorInfo[] DeclaredConstructors(this Type type)
        {
#if (NETFX_CORE || PCL)
            return type.GetTypeInfo().DeclaredConstructors.ToArray();
#else
            return type.GetConstructors();
#endif
        }

        public static bool AssignableFrom(this Type type, Type fromType)
        {
#if (NETFX_CORE || PCL)
            return type.GetTypeInfo().IsAssignableFrom(fromType.GetTypeInfo());
#else
            return type.IsAssignableFrom(fromType);
#endif
        }

        public static bool IsStandardClass(this Type type)
        {
#if (NETFX_CORE || PCL)
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsClass && !typeInfo.IsAbstract && !typeInfo.IsInterface;
#else
            return type.IsClass && !type.IsAbstract && !type.IsInterface;
#endif
        }

        public static bool IsAbstract(this Type type)
        {
#if (NETFX_CORE || PCL)
            return type.GetTypeInfo().IsAbstract;
#else
            return type.IsAbstract;
#endif
        }

        public static PropertyInfo GetPropertyInfo(this Type type, string propertyName)
        {
#if (NETFX_CORE || PCL)
            return type.GetRuntimeProperty(propertyName);
#else
            return type.GetProperty(propertyName);
#endif
        }

        public static FieldInfo GetFieldInfo(this Type type, string fieldName)
        {
#if (NETFX_CORE || PCL)
            return type.GetRuntimeField(fieldName);
#else
            return type.GetField(fieldName);
#endif
        }

        public static FieldInfo[] GetWritableFields(this Type type)
        {
#if (NETFX_CORE || PCL)
            return type.GetRuntimeFields().Where(p => !p.IsPublic && !p.IsStatic).ToArray();
#else
            return type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField);
#endif
        }

        public static MethodInfo SetMethod(this PropertyInfo pi, bool nonPublic = true)
        {
#if (NETFX_CORE || PCL)
            return pi.SetMethod;
#else
            return pi.GetSetMethod(nonPublic);
#endif
        }

        public static MethodInfo GetMethodInfo(this PropertyInfo pi, bool nonPublic = true)
        {
#if (NETFX_CORE || PCL)
            return pi.GetMethod;
#else
            return pi.GetGetMethod(nonPublic);
#endif
        }

        public static bool InstanceOfType(this Type type, object instance)
        {
#if (NETFX_CORE || PCL)
            return type.IsInstanceOf(instance.GetType());
#else
            return type.IsInstanceOfType(instance);
#endif
        }

        public static bool IsAssignableFromType(this Type type, Type fromType)
        {
#if (NETFX_CORE || PCL)
            return type.GetTypeInfo().IsAssignableFrom(fromType.GetTypeInfo());
#else
            return type.IsAssignableFrom(fromType);
#endif
        }

        public static bool IsClass(this Type type)
        {
#if (NETFX_CORE || PCL)
            return type.GetTypeInfo().IsClass;
#else
            return type.IsClass;
#endif
        }

        public static bool IsEnum(this Type type)
        {
#if (NETFX_CORE || PCL)
            return type.GetTypeInfo().IsEnum;
#else
            return type.IsEnum;
#endif
        }

        public static bool IsEnumFlags(this Type type)
        {
#if (NETFX_CORE || PCL)
            return type.GetTypeInfo().IsEnum && type.FirstAttribute<FlagsAttribute>() != null;
#else
            return type.IsEnum && type.FirstAttribute<FlagsAttribute>() != null;
#endif
        }

        public static bool IsUnderlyingEnum(this Type type)
        {
#if (NETFX_CORE || PCL)
            return type.GetTypeInfo().IsEnum;
#else
            return type.IsEnum || type.UnderlyingSystemType.IsEnum;
#endif
        }

        public static MethodInfo[] GetMethodInfos(this Type type)
        {
#if (NETFX_CORE || PCL)
            return type.GetRuntimeMethods().ToArray();
#else
            return type.GetMethods();
#endif
        }

        public static PropertyInfo[] GetPropertyInfos(this Type type)
        {
#if (NETFX_CORE || PCL)
            return type.GetRuntimeProperties().ToArray();
#else
            return type.GetProperties();
#endif
        }

        public static bool IsGenericTypeDefinition(this Type type)
        {
#if (NETFX_CORE || PCL)
            return type.GetTypeInfo().IsGenericTypeDefinition;
#else
            return type.IsGenericTypeDefinition;
#endif
        }

        public static bool IsGenericType(this Type type)
        {
#if (NETFX_CORE || PCL)
            return type.GetTypeInfo().IsGenericType;
#else
            return type.IsGenericType;
#endif
        }

#if (NETFX_CORE)
        public static object GetDefaultValue(this Type type)
        {
            return type.GetTypeInfo().IsValueType ? Activator.CreateInstance(type) : null;
        }

        public static PropertyInfo GetProperty(this Type type, String propertyName)
        {
            return type.GetTypeInfo().GetDeclaredProperty(propertyName);
        }

        public static MethodInfo GetMethod(this Type type, String methodName)
        {
            return type.GetTypeInfo().GetDeclaredMethod(methodName);
        }
#endif

#if SL5 || NETFX_CORE || PCL
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

        public static string GetDeclaringTypeName(this Type type)
        {
            if (type.DeclaringType != null)
                return type.DeclaringType.Name;

#if !(NETFX_CORE || WP || PCL)
            if (type.ReflectedType != null)
                return type.ReflectedType.Name;
#endif

            return null;
        }

        public static string GetDeclaringTypeName(this MemberInfo mi)
        {
            if (mi.DeclaringType != null)
                return mi.DeclaringType.Name;

#if !(NETFX_CORE || WP || PCL)
            return mi.ReflectedType.Name;
#endif

            return null;
        }

        public static Delegate CreateDelegate(this MethodInfo methodInfo, Type delegateType)
        {
#if PCL
            return methodInfo.CreateDelegate(delegateType);
#else
            return Delegate.CreateDelegate(delegateType, methodInfo);
#endif
        }

        public static Delegate CreateDelegate(this MethodInfo methodInfo, Type delegateType, object target)
        {
#if PCL
            return methodInfo.CreateDelegate(delegateType, target);
#else
            return Delegate.CreateDelegate(delegateType, target, methodInfo);
#endif
        }
    }

}