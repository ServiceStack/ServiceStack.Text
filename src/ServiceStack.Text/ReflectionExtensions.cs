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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using ServiceStack.Text.Support;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

using ServiceStack.Text;

namespace ServiceStack
{
    public delegate EmptyCtorDelegate EmptyCtorFactoryDelegate(Type type);
    public delegate object EmptyCtorDelegate();

#if (NETFX_CORE || PCL || NETSTANDARD1_1)

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
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
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
            //http://stackoverflow.com/a/39140220/85785
            return type.GetTypeInfo().IsGenericTypeDefinition
                ? type.GetTypeInfo().GenericTypeParameters
                : type.GetTypeInfo().GenericTypeArguments;
        }

        internal static TypeInfo GetTypeInfo(this Type type)
        {
            return ((IReflectableType)type).GetTypeInfo();
        }
#endif

#if NETSTANDARD1_1
        private static readonly Func<Type, object> GetUninitializedObjectDelegate = 
            (Func<Type, object>) typeof(string).GetTypeInfo().Assembly
               .GetType("System.Runtime.Serialization.FormatterServices")
               ?.GetMethod("GetUninitializedObject")
               ?.CreateDelegate(typeof(Func<Type, object>));
#endif

        public static TypeCode GetTypeCode(this Type type)
        {
#if (NETFX_CORE || PCL || NETSTANDARD1_1)
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
                newCache = new Dictionary<string, EmptyCtorDelegate>(TypeNamesMap) {
                    [typeName] = emptyCtorFn
                };

            } while (!ReferenceEquals(
                Interlocked.CompareExchange(ref TypeNamesMap, newCache, snapshot), snapshot));

            return emptyCtorFn;
        }

        public static EmptyCtorDelegate GetConstructorMethodToCache(Type type)
        {
            if (type == typeof(string))
            {
                return () => String.Empty;
            }
            else if (type.IsInterface())
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
#if NETSTANDARD1_1
                var genericArgs = type.GetTypeInfo().GenericTypeParameters;
#else
                var genericArgs = type.GetGenericArguments();
#endif
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
#elif WP || PCL || NETSTANDARD1_1
                System.Linq.Expressions.Expression conversion = Expression.Convert(
                    System.Linq.Expressions.Expression.New(type), typeof(object));

                return System.Linq.Expressions.Expression.Lambda<EmptyCtorDelegate>(conversion).Compile();
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
#elif NETSTANDARD1_1
            if (GetUninitializedObjectDelegate != null)
                return () => GetUninitializedObjectDelegate(type);

            return () => Activator.CreateInstance(type);
#elif WP || PCL
            return System.Linq.Expressions.Expression.Lambda<EmptyCtorDelegate>(
                System.Linq.Expressions.Expression.New(type)).Compile();
#else
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object CreateInstance(this Type type)
        {
            if (type == null)
                return null;

            var ctorFn = GetConstructorMethod(type);
            return ctorFn();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T CreateInstance<T>(this Type type)
        {
            if (type == null)
                return default(T);

            var ctorFn = GetConstructorMethod(type);
            return (T)ctorFn();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object CreateInstance(string typeName)
        {
            if (typeName == null)
                return null;

            var ctorFn = GetConstructorMethod(typeName);
            return ctorFn();
        }

        public static PropertyInfo[] GetAllProperties(this Type type)
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

                    var typeProperties = subType.GetTypesProperties();

                    var newPropertyInfos = typeProperties
                        .Where(x => !propertyInfos.Contains(x));

                    propertyInfos.InsertRange(0, newPropertyInfos);
                }

                return propertyInfos.ToArray();
            }

            return type.GetTypesProperties()
                .Where(t => t.GetIndexParameters().Length == 0) // ignore indexed properties
                .ToArray();
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
            var properties = type.IsDto()
                ? type.GetAllProperties()
                : type.GetPublicProperties();
            return properties.OnlySerializableProperties(type);
        }

        public static PropertyInfo[] OnlySerializableProperties(this PropertyInfo[] properties, Type type = null)
        {
            var isDto = type.IsDto();
            var readableProperties = properties.Where(x => x.PropertyGetMethod(nonPublic: isDto) != null);

            if (isDto)
            {
                return readableProperties.Where(attr =>
                    attr.HasAttribute<DataMemberAttribute>()).ToArray();
            }

            // else return those properties that are not decorated with IgnoreDataMember
            return readableProperties
                .Where(prop => prop.AllAttributes()
                    .All(attr =>
                    {
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
                return type.GetAllFields().Where(f =>
                    f.HasAttribute<DataMemberAttribute>()).ToArray();
            }

            if (!JsConfig.IncludePublicFields)
                return TypeConstants.EmptyFieldInfoArray;

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

        public static string GetDataMemberName(this PropertyInfo pi)
        {
            var attr = pi.GetDataMember();
            return attr?.Name;
        }

        public static string GetDataMemberName(this FieldInfo fi)
        {
            var attr = fi.GetDataMember();
            return attr?.Name;
        }
    }
}