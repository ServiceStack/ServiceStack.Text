//
// https://github.com/ServiceStack/ServiceStack.Text
// ServiceStack.Text: .NET C# POCO JSON, JSV and CSV Text Serializers.
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2012 ServiceStack Ltd.
//
// Licensed under the same terms of ServiceStack: new BSD license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using ServiceStack.Text.Support;
#if WINDOWS_PHONE
using System.Linq.Expressions;
#endif

namespace ServiceStack.Text
{
    public delegate EmptyCtorDelegate EmptyCtorFactoryDelegate(Type type);
    public delegate object EmptyCtorDelegate();

    public static class ReflectionExtensions
    {
        private static Dictionary<Type, object> DefaultValueTypes = new Dictionary<Type, object>();

        public static object GetDefaultValue(this Type type)
        {
#if NETFX_CORE
            if (!type.GetTypeInfo().IsValueType) return null;
#else
            if (!type.IsValueType) return null;
#endif

            object defaultValue;
            if (DefaultValueTypes.TryGetValue(type, out defaultValue)) return defaultValue;

            defaultValue = Activator.CreateInstance(type);

            Dictionary<Type, object> snapshot, newCache;
            do
            {
                snapshot = DefaultValueTypes;
                newCache = new Dictionary<Type, object>(DefaultValueTypes);
                newCache[type] = defaultValue;

            } while (!ReferenceEquals(
                Interlocked.CompareExchange(ref DefaultValueTypes, newCache, snapshot), snapshot));

            return defaultValue;
        }

        public static bool IsInstanceOf(this Type type, Type thisOrBaseType)
        {
            while (type != null)
            {
                if (type == thisOrBaseType)
                    return true;

#if NETFX_CORE
                type = type.GetTypeInfo().BaseType;
#else
                type = type.BaseType;
#endif
            }
            return false;
        }

        public static bool IsGenericType(this Type type)
        {
            while (type != null)
            {
#if NETFX_CORE
                if (type.GetTypeInfo().IsGenericType)
                    return true;

                type = type.GetTypeInfo().BaseType;
#else
                if (type.IsGenericType)
                    return true;

                type = type.BaseType;
#endif
            }
            return false;
        }

        public static Type GetGenericType(this Type type)
        {
            while (type != null)
            {
#if NETFX_CORE
                if (type.GetTypeInfo().IsGenericType)
                    return type;

                type = type.GetTypeInfo().BaseType;
#else
                if (type.IsGenericType)
                    return type;

                type = type.BaseType;
#endif
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
#if NETFX_CORE
            foreach (var t in type.GetTypeInfo().ImplementedInterfaces)
#else
            foreach (var t in type.GetInterfaces())
#endif
            {
#if NETFX_CORE
                if (t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == genericTypeDefinition)
#else
                if (t.IsGenericType && t.GetGenericTypeDefinition() == genericTypeDefinition)
#endif
                {
                    return t;
                }
            }

            var genericType = type.GetGenericType();
            if (genericType != null && genericType.GetGenericTypeDefinition() == genericTypeDefinition)
            {
                return genericType;
            }

            return null;
        }

        public static Type GetTypeWithInterfaceOf(this Type type, Type interfaceType)
        {
            if (type == interfaceType) return interfaceType;

#if NETFX_CORE
            foreach (var t in type.GetTypeInfo().ImplementedInterfaces)
#else
            foreach (var t in type.GetInterfaces())
#endif
            {
                if (t == interfaceType)
                    return t;
            }

            return null;
        }

        public static bool HasInterface(this Type type, Type interfaceType)
        {
#if NETFX_CORE
            foreach (var t in type.GetTypeInfo().ImplementedInterfaces)
#else
            foreach (var t in type.GetInterfaces())
#endif
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

        public static bool IsNumericType(this Type type)
        {
#if NETFX_CORE
            if (!type.GetTypeInfo().IsValueType) return false;
#else 
            if (!type.IsValueType) return false;
#endif
            return type.IsIntegerType() || type.IsRealNumberType();
        }

        public static bool IsIntegerType(this Type type)
        {
#if NETFX_CORE
            if (!type.GetTypeInfo().IsValueType) return false;
#else
            if (!type.IsValueType) return false;
#endif
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
            return underlyingType == typeof(byte)
               || underlyingType == typeof(sbyte)
               || underlyingType == typeof(short)
               || underlyingType == typeof(ushort)
               || underlyingType == typeof(int)
               || underlyingType == typeof(uint)
               || underlyingType == typeof(long)
               || underlyingType == typeof(ulong);
        }

        public static bool IsRealNumberType(this Type type)
        {
#if NETFX_CORE
            if (!type.GetTypeInfo().IsValueType) return false;
#else
            if (!type.IsValueType) return false;
#endif
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
            return underlyingType == typeof(float)
               || underlyingType == typeof(double)
               || underlyingType == typeof(decimal);
        }

        public static Type GetTypeWithGenericInterfaceOf(this Type type, Type genericInterfaceType)
        {
#if NETFX_CORE
            foreach (var t in type.GetTypeInfo().ImplementedInterfaces)
#else
            foreach (var t in type.GetInterfaces())
#endif
            {
#if NETFX_CORE
                if (t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == genericInterfaceType) return t;
#else
                if (t.IsGenericType && t.GetGenericTypeDefinition() == genericInterfaceType) return t;
#endif
            }

#if NETFX_CORE
            if (!type.GetTypeInfo().IsGenericType) return null;
#else
            if (!type.IsGenericType) return null;
#endif

            var genericType = type.GetGenericType();
            return genericType.GetGenericTypeDefinition() == genericInterfaceType
                    ? genericType
                    : null;
        }

        public static bool HasAnyTypeDefinitionsOf(this Type genericType, params Type[] theseGenericTypes)
        {
#if NETFX_CORE
            if (!genericType.GetTypeInfo().IsGenericType) return false;
#else
            if (!genericType.IsGenericType) return false;
#endif
            var genericTypeDefinition = genericType.GetGenericTypeDefinition();

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

#if NETFX_CORE
            var typeAGenericArgs = typeAInterface.GenericTypeArguments;
            var typeBGenericArgs = typeBInterface.GenericTypeArguments;
#else
            var typeAGenericArgs = typeAInterface.GetGenericArguments();
            var typeBGenericArgs = typeBInterface.GetGenericArguments();
#endif
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
            
#if NETFX_CORE
            var typeAGenericArgs = typeAInterface.GenericTypeArguments;
            var typeBGenericArgs = typeBInterface.GenericTypeArguments;
#else
            var typeAGenericArgs = typeAInterface.GetGenericArguments();
            var typeBGenericArgs = typeBInterface.GetGenericArguments();
#endif
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
#if NETFX_CORE
                if (!(type == typeof(string) || type.GetTypeInfo().IsValueType)) return false;
#else
                if (!(type == typeof(string) || type.IsValueType)) return false;
#endif
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

            var type = JsConfig.TypeFinder.Invoke(typeName);
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
#if NETFX_CORE
            var emptyCtor = type.GetTypeInfo().DeclaredConstructors.FirstOrDefault(c => c.GetParameters().Count() == 0);
#else
            var emptyCtor = type.GetConstructor(Type.EmptyTypes);
#endif
            if (emptyCtor != null)
            {

#if MONOTOUCH || c|| XBOX || NETFX_CORE
				return () => Activator.CreateInstance(type);
#elif WINDOWS_PHONE
                return Expression.Lambda<EmptyCtorDelegate>(Expression.New(type)).Compile();
#else
#if SILVERLIGHT
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

#if (SILVERLIGHT && !WINDOWS_PHONE) || XBOX
            return () => Activator.CreateInstance(type);
#elif WINDOWS_PHONE
            return Expression.Lambda<EmptyCtorDelegate>(Expression.New(type)).Compile();
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

        public static object CreateInstance<T>()
        {
            return TypeMeta<T>.EmptyCtorFn();
        }

        public static object CreateInstance(this Type type)
        {
            var ctorFn = GetConstructorMethod(type);
            return ctorFn();
        }

        public static object CreateInstance(string typeName)
        {
            var ctorFn = GetConstructorMethod(typeName);
            return ctorFn();
        }

        public static PropertyInfo[] GetPublicProperties(this Type type)
        {
#if NETFX_CORE
            if (type.GetTypeInfo().IsInterface)
#else
            if (type.IsInterface)
#endif
            {
                var propertyInfos = new List<PropertyInfo>();

                var considered = new List<Type>();
                var queue = new Queue<Type>();
                considered.Add(type);
                queue.Enqueue(type);
                while (queue.Count > 0)
                {
                    var subType = queue.Dequeue();
#if NETFX_CORE
                    foreach (var subInterface in subType.GetTypeInfo().ImplementedInterfaces)
#else
                    foreach (var subInterface in subType.GetInterfaces())
#endif
                    {
                        if (considered.Contains(subInterface)) continue;

                        considered.Add(subInterface);
                        queue.Enqueue(subInterface);
                    }

#if NETFX_CORE 
                    var typeProperties = subType.GetRuntimeProperties();
#else
                    var typeProperties = subType.GetProperties(
                        BindingFlags.FlattenHierarchy
                        | BindingFlags.Public
                        | BindingFlags.Instance);
#endif

                    var newPropertyInfos = typeProperties
                        .Where(x => !propertyInfos.Contains(x));

                    propertyInfos.InsertRange(0, newPropertyInfos);
                }

                return propertyInfos.ToArray();
            }

#if NETFX_CORE
            return type.GetRuntimeProperties()
                .Where(t => t.GetIndexParameters().Length == 0) // ignore indexed properties
                .ToArray();
#else
            return type.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance)
                .Where(t => t.GetIndexParameters().Length == 0) // ignore indexed properties
                .ToArray();
#endif
        }

        public static FieldInfo[] GetPublicFields(this Type type)
        {
#if NETFX_CORE
            if (type.GetTypeInfo().IsInterface)
#else
            if (type.IsInterface)
#endif
            {

                return new FieldInfo[0];
            }

#if NETFX_CORE
            return type.GetRuntimeFields().Where(p => p.IsPublic && !p.IsStatic).ToArray();
#else
            return type.GetFields(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance)
                .ToArray();
#endif
        }

        const string DataContract = "DataContractAttribute";
        const string DataMember = "DataMemberAttribute";
        const string IgnoreDataMember = "IgnoreDataMemberAttribute";

        public static PropertyInfo[] GetSerializableProperties(this Type type)
        {
            var publicProperties = GetPublicProperties(type);
#if NETFX_CORE
            var publicReadableProperties = publicProperties.Where(x => x.GetMethod != null);
#else
            var publicReadableProperties = publicProperties.Where(x => x.GetGetMethod(false) != null);
#endif

            if (type.IsDto())
            {
                return !Env.IsMono
                    ? publicReadableProperties.Where(attr => 
                        attr.IsDefined(typeof(DataMemberAttribute), false)).ToArray()
                    : publicReadableProperties.Where(attr => 
                        attr.GetCustomAttributes(false).Any(x => x.GetType().Name == DataMember)).ToArray();
            }

            // else return those properties that are not decorated with IgnoreDataMember
            return publicReadableProperties.Where(prop => !prop.GetCustomAttributes(false).Any(attr => attr.GetType().Name == IgnoreDataMember)).ToArray();
        }

        public static FieldInfo[] GetSerializableFields(this Type type)
        {
            if (type.IsDto()) {
                return new FieldInfo[0];
            }
            
            var publicFields = GetPublicFields(type);

            // else return those properties that are not decorated with IgnoreDataMember
            return publicFields.Where(prop => !prop.GetCustomAttributes(false).Any(attr => attr.GetType().Name == IgnoreDataMember)).ToArray();
        }

        public static bool IsDto(this Type type)
        {
#if NETFX_CORE
            return type.GetTypeInfo().IsDefined(typeof(DataContractAttribute), false);
#else
            return !Env.IsMono
                ? type.IsDefined(typeof(DataContractAttribute), false)
                : type.GetCustomAttributes(true).Any(x => x.GetType().Name == DataContract);
#endif
        }

        public static bool HasAttr<T>(this Type type) where T : Attribute
        {
#if NETFX_CORE
            return type.GetTypeInfo().GetCustomAttributes(true).Any(x => x.GetType() == typeof(T));
#else
            return type.GetCustomAttributes(true).Any(x => x.GetType() == typeof(T));
#endif
        }

#if !SILVERLIGHT && !MONOTOUCH 
        static readonly Dictionary<Type, FastMember.TypeAccessor> typeAccessorMap 
            = new Dictionary<Type, FastMember.TypeAccessor>();
#endif

        public static DataContractAttribute GetDataContract(this Type type)
        {
#if NETFX_CORE
            var dataContract = type.GetTypeInfo().GetCustomAttributes(typeof(DataContractAttribute), true)
                .FirstOrDefault() as DataContractAttribute;
#else
            var dataContract = type.GetCustomAttributes(typeof(DataContractAttribute), true)
                .FirstOrDefault() as DataContractAttribute;
#endif

#if !SILVERLIGHT && !MONOTOUCH && !XBOX
            if (dataContract == null && Env.IsMono)
                return type.GetWeakDataContract();
#endif
            return dataContract;
        }

        public static DataMemberAttribute GetDataMember(this PropertyInfo pi)
        {
            var dataMember = pi.GetCustomAttributes(typeof(DataMemberAttribute), false)
                .FirstOrDefault() as DataMemberAttribute;

#if !SILVERLIGHT && !MONOTOUCH && !XBOX
            if (dataMember == null && Env.IsMono)
                return pi.GetWeakDataMember();
#endif
            return dataMember;
        }

#if !SILVERLIGHT && !MONOTOUCH && !XBOX
        public static DataContractAttribute GetWeakDataContract(this Type type)
        {
            var attr = type.GetCustomAttributes(true).FirstOrDefault(x => x.GetType().Name == DataContract);
            if (attr != null)
            {
                var attrType = attr.GetType();

                FastMember.TypeAccessor accessor;
                lock (typeAccessorMap)
                {
                    if (!typeAccessorMap.TryGetValue(attrType, out accessor))
                        typeAccessorMap[attrType] = accessor = FastMember.TypeAccessor.Create(attr.GetType());
                }

                return new DataContractAttribute {
                    Name = (string)accessor[attr, "Name"],
                    Namespace = (string)accessor[attr, "Namespace"],
                };
            }
            return null;
        }

        public static DataMemberAttribute GetWeakDataMember(this PropertyInfo pi)
        {
            var attr = pi.GetCustomAttributes(true).FirstOrDefault(x => x.GetType().Name == DataMember);
            if (attr != null)
            {
                var attrType = attr.GetType();

                FastMember.TypeAccessor accessor;
                lock (typeAccessorMap)
                {
                    if (!typeAccessorMap.TryGetValue(attrType, out accessor))
                        typeAccessorMap[attrType] = accessor = FastMember.TypeAccessor.Create(attr.GetType());
                }

                var newAttr = new DataMemberAttribute {
                    Name = (string) accessor[attr, "Name"],
                    EmitDefaultValue = (bool)accessor[attr, "EmitDefaultValue"],
                    IsRequired = (bool)accessor[attr, "IsRequired"],
                };

                var order = (int)accessor[attr, "Order"];
                if (order >= 0)
                    newAttr.Order = order; //Throws Exception if set to -1

                return newAttr;
            }
            return null;
        }
#endif

    }

}