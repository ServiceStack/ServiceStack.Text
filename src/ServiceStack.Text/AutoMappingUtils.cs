﻿// Copyright (c) Service Stack LLC. All Rights Reserved.
// License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

using System;
using System.Collections.Generic;

#if NET40
using System.Collections.Concurrent;

#endif

using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

#if PLATFORM_USE_SERIALIZATION_DLL
using System.Runtime.Serialization;
#endif


using System.Threading;
using ServiceStack.Text;

namespace ServiceStack
{
	
	
#if PLATFORM_USE_SERIALIZATION_DLL
	
    [DataContract(Namespace = "http://schemas.servicestack.net/types")]
    public class CustomHttpResult { }
	
#endif
	
	
    public static class AutoMappingUtils
    {
        public static T ConvertTo<T>(this object from)
        {
            var to = typeof(T).CreateInstance<T>();
            return to.PopulateWith(from);
        }


		
		
        private static readonly Dictionary<Type, List<string>> TypePropertyNamesMap = new Dictionary<Type, List<string>>();


		
        public static List<string> GetPropertyNames(this Type type)
        {
            lock (TypePropertyNamesMap)
            {
                List<string> propertyNames;
                if (!TypePropertyNamesMap.TryGetValue(type, out propertyNames))
                {
                    propertyNames = type.Properties().ToList().ConvertAll(x => x.Name);
                    TypePropertyNamesMap[type] = propertyNames;
                }
                return propertyNames;
            }
        }

#if !SILVERLIGHT
        public static string GetAssemblyPath(this Type source)
        {
            var assemblyUri =
                new Uri(source.Assembly.EscapedCodeBase);

            return assemblyUri.LocalPath;
        }
#endif

        public static bool IsDebugBuild(this Assembly assembly)
        {
#if NETFX_CORE
            return assembly.GetCustomAttributes()
                .OfType<DebuggableAttribute>()
                .Any();
#elif WINDOWS_PHONE || SILVERLIGHT
            return assembly.GetCustomAttributes(false)
                .OfType<DebuggableAttribute>()
                .Any();
#else
            return assembly.GetCustomAttributes(false)
                .OfType<DebuggableAttribute>()
                .Select(attr => attr.IsJITTrackingEnabled)
                .FirstOrDefault();
#endif
        }

        /// <summary>
        /// Populate an object with Example data.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object PopulateWith(object obj)
        {
            if (obj == null) return null;
			
			
#if PLATFORM_USE_SERIALIZATION_DLL   
			
            var isHttpResult = obj.GetType().GetInterfaces().Any(x => x.Name == "IHttpResult"); // No coupling FTW!
            if (isHttpResult)
            {
                obj = new CustomHttpResult();
            }
			
#endif
			
            var type = obj.GetType();
            if (type.IsArray() || type.IsValueType() || type.IsGeneric())
            {
                var value = CreateDefaultValue(type, new Dictionary<Type, int>(20));
                return value;
            }

            return PopulateObjectInternal(obj, new Dictionary<Type, int>(20));
        }

        /// <summary>
        /// Populates the object with example data.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="recursionInfo">Tracks how deeply nested we are</param>
        /// <returns></returns>
        private static object PopulateObjectInternal(object obj, Dictionary<Type, int> recursionInfo)
        {
            if (obj == null) return null;
            if (obj is string) return obj; // prevents it from dropping into the char[] Chars property.  Sheesh
            var type = obj.GetType();

            var members = type.GetPublicMembers();
            foreach (var info in members)
            {
                var fieldInfo = info as FieldInfo;
                var propertyInfo = info as PropertyInfo;
                if (fieldInfo != null || propertyInfo != null)
                {
                    var memberType = fieldInfo != null ? fieldInfo.FieldType : propertyInfo.PropertyType;
                    var value = CreateDefaultValue(memberType, recursionInfo);
                    SetValue(fieldInfo, propertyInfo, obj, value);
                }
            }
            return obj;
        }
 
		
				
#if   PLATFORM_NO_USE_INTERLOCKED_COMPARE_EXCHANGE_T 
		private static object _defaultValueTypes = new Dictionary<Type, object>();
		
	
		private  static Dictionary<Type, object> DefaultValueTypes
		{ get {  return ( Dictionary<Type, object>)  _defaultValueTypes; } }	

#else
		
        private static Dictionary<Type, object> DefaultValueTypes = new Dictionary<Type, object>();
		
#endif
		
        public static object GetDefaultValue(this Type type)
        {
            if (!type.IsValueType()) return null;

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

				
#if   PLATFORM_NO_USE_INTERLOCKED_COMPARE_EXCHANGE_T 
				
				Interlocked.CompareExchange (ref _defaultValueTypes, (object )newCache, (object ) snapshot), snapshot));
	
#else
              
			Interlocked.CompareExchange(ref DefaultValueTypes, newCache, snapshot), snapshot));
			
#endif
			
            return defaultValue;
        }

		
#if NET4
        private static readonly ConcurrentDictionary<string, AssignmentDefinition> AssignmentDefinitionCache
            = new ConcurrentDictionary<string, AssignmentDefinition>();
#else
		        
		private static readonly  Dictionary<string, AssignmentDefinition> AssignmentDefinitionCache
            = new Dictionary<string, AssignmentDefinition>();
		
		private  static object  _assignmentDefinitionCache_Locker =  new object( );
		
#endif
		
        internal static AssignmentDefinition GetAssignmentDefinition(Type toType, Type fromType)
        {
            var cacheKey = toType.FullName + "<" + fromType.FullName;

			
#if NET4
			return AssignmentDefinitionCache.GetOrAdd(cacheKey, delegate
            
            {

                var definition = new AssignmentDefinition
                {
                    ToType = toType,
                    FromType = fromType,
                };

                var readMap = GetMembers(fromType, isReadable: true);
                var writeMap = GetMembers(toType, isReadable: false);

                foreach (var assignmentMember in readMap)
                {
                    AssignmentMember writeMember;
                    if (writeMap.TryGetValue(assignmentMember.Key, out writeMember))
                    {
                        definition.AddMatch(assignmentMember.Key, assignmentMember.Value, writeMember);
                    }
                }

                return definition;
            });
	
#else
			AssignmentDefinition  v;
			
			lock( _assignmentDefinitionCache_Locker )  
			{
				
				if ( AssignmentDefinitionCache.TryGetValue( cacheKey, out  v ) )
					return  v;
				else
				{
	
					                
					var definition = new AssignmentDefinition
	                {
	                    ToType = toType,
	                    FromType = fromType,
	                };
	
	                var readMap = GetMembers(fromType, isReadable: true);
	                var writeMap = GetMembers(toType, isReadable: false);
	
	                foreach (var assignmentMember in readMap)
	                {
	                    AssignmentMember writeMember;
	                    if (writeMap.TryGetValue(assignmentMember.Key, out writeMember))
	                    {
	                        definition.AddMatch(assignmentMember.Key, assignmentMember.Value, writeMember);
	                    }
	                }
	
	                return definition;
					
					
				}
				
				
			}
			
			
#endif
			
			
			
			
        }


        private static Dictionary<string, AssignmentMember> GetMembers(Type type, bool isReadable)
        {
            var map = new Dictionary<string, AssignmentMember>();

            var members = type.GetAllPublicMembers();
            foreach (var info in members)
            {
                if (info.DeclaringType == typeof(object)) continue;

                var propertyInfo = info as PropertyInfo;
                if (propertyInfo != null)
                {
                    if (isReadable)
                    {
                        if (propertyInfo.CanRead)
                        {
                            map[info.Name] = new AssignmentMember(propertyInfo.PropertyType, propertyInfo);
                            continue;
                        }
                    }
                    else
                    {
                        if (propertyInfo.CanWrite && propertyInfo.GetSetMethod() != null)
                        {
                            map[info.Name] = new AssignmentMember(propertyInfo.PropertyType, propertyInfo);
                            continue;
                        }
                    }
                }

                var fieldInfo = info as FieldInfo;
                if (fieldInfo != null)
                {
                    map[info.Name] = new AssignmentMember(fieldInfo.FieldType, fieldInfo);
                    continue;
                }

                var methodInfo = info as MethodInfo;
                if (methodInfo != null)
                {
                    var parameterInfos = methodInfo.GetParameters();
                    if (isReadable)
                    {
                        if (parameterInfos.Length == 0)
                        {
                            var name = info.Name.StartsWith("get_") ? info.Name.Substring(4) : info.Name;
                            if (!map.ContainsKey(name))
                            {
                                map[name] = new AssignmentMember(methodInfo.ReturnType, methodInfo);
                                continue;
                            }
                        }
                    }
                    else
                    {
                        if (parameterInfos.Length == 1 && methodInfo.ReturnType == typeof(void))
                        {
                            var name = info.Name.StartsWith("set_") ? info.Name.Substring(4) : info.Name;
                            if (!map.ContainsKey(name))
                            {
                                map[name] = new AssignmentMember(parameterInfos[0].ParameterType, methodInfo);
                                continue;
                            }
                        }
                    }
                }
            }

            return map;
        }

        public static To PopulateWith<To, From>(this To to, From from)
        {
            if (Equals(to, default(To)) || Equals(from, default(From))) return default(To);

            var assignmentDefinition = GetAssignmentDefinition(to.GetType(), from.GetType());

            assignmentDefinition.Populate(to, from);

            return to;
        }

        public static To PopulateWithNonDefaultValues<To, From>(this To to, From from)
        {
            if (Equals(to, default(To)) || Equals(from, default(From))) return default(To);

            var assignmentDefinition = GetAssignmentDefinition(to.GetType(), from.GetType());

            assignmentDefinition.PopulateWithNonDefaultValues(to, from);

            return to;
        }

        public static To PopulateFromPropertiesWithAttribute<To, From>(this To to, From from,
            Type attributeType)
        {
            if (Equals(to, default(To)) || Equals(from, default(From))) return default(To);

            var assignmentDefinition = GetAssignmentDefinition(to.GetType(), from.GetType());

            assignmentDefinition.PopulateFromPropertiesWithAttribute(to, from, attributeType);

            return to;
        }

        public static void SetProperty(this PropertyInfo propertyInfo, object obj, object value)
        {
            if (!propertyInfo.CanWrite)
            {
                Tracer.Instance.WriteWarning("Attempted to set read only property '{0}'", propertyInfo.Name);
                return;
            }

            var propertySetMetodInfo = propertyInfo.SetMethod();
            if (propertySetMetodInfo != null)
            {
                propertySetMetodInfo.Invoke(obj, new[] { value });
            }
        }

        public static object GetProperty(this PropertyInfo propertyInfo, object obj)
        {
            if (propertyInfo == null || !propertyInfo.CanRead)
                return null;

            var getMethod = propertyInfo.GetMethodInfo();
            return getMethod != null ? getMethod.Invoke(obj, new object[0]) : null;
        }

        public static void SetValue(FieldInfo fieldInfo, PropertyInfo propertyInfo, object obj, object value)
        {
            try
            {
                if (IsUnsettableValue(fieldInfo, propertyInfo)) return;
                if (fieldInfo != null && !fieldInfo.IsLiteral)
                {
                    fieldInfo.SetValue(obj, value);
                }
                else
                {
                    SetProperty(propertyInfo, obj, value);
                }
            }
            catch (Exception ex)
            {
                var name = (fieldInfo != null) ? fieldInfo.Name : propertyInfo.Name;
                Tracer.Instance.WriteDebug("Could not set member: {0}. Error: {1}", name, ex.Message);
            }
        }

        public static bool IsUnsettableValue(FieldInfo fieldInfo, PropertyInfo propertyInfo)
        {
#if NETFX_CORE
            if (propertyInfo != null)
            {
                // Properties on non-user defined classes should not be set
                // Currently we define those properties as properties declared on
                // types defined in mscorlib

                if (propertyInfo.DeclaringType.AssemblyQualifiedName.Equals(typeof(object).AssemblyQualifiedName))
                {
                    return true;
                }
            }
#else
            if (propertyInfo != null && propertyInfo.ReflectedType != null)
            {
                // Properties on non-user defined classes should not be set
                // Currently we define those properties as properties declared on
                // types defined in mscorlib

                if (propertyInfo.DeclaringType.Assembly == typeof(object).Assembly)
                {
                    return true;
                }
            }
#endif

            return false;
        }

        public static object[] CreateDefaultValues(IEnumerable<Type> types, Dictionary<Type, int> recursionInfo)
        {
            var values = new List<object>();
            foreach (var type in types)
            {
                values.Add(CreateDefaultValue(type, recursionInfo));
            }
            return values.ToArray();
        }

        private const int MaxRecursionLevelForDefaultValues = 2; // do not nest a single type more than this deep.

        public static object CreateDefaultValue(Type type, Dictionary<Type, int> recursionInfo)
        {
            if (type == typeof(string))
            {
                return type.Name;
            }

            if (type.IsEnum())
            {
#if SILVERLIGHT4 || WINDOWS_PHONE
                return Enum.ToObject(type, 0);
#else
                return Enum.GetValues(type).GetValue(0);
#endif
            }

            // If we have hit our recursion limit for this type, then return null
            int recurseLevel; // will get set to 0 if TryGetValue() fails
            recursionInfo.TryGetValue(type, out recurseLevel);
            if (recurseLevel > MaxRecursionLevelForDefaultValues) return null;

            recursionInfo[type] = recurseLevel + 1; // increase recursion level for this type
            try // use a try/finally block to make sure we decrease the recursion level for this type no matter which code path we take,
            {

                //when using KeyValuePair<TKey, TValue>, TKey must be non-default to stuff in a Dictionary
                if (type.IsGeneric() && type.GenericTypeDefinition() == typeof(KeyValuePair<,>))
                {
                    var genericTypes = type.GenericTypeArguments();
                    var valueType = Activator.CreateInstance(type, CreateDefaultValue(genericTypes[0], recursionInfo), CreateDefaultValue(genericTypes[1], recursionInfo));
                    return PopulateObjectInternal(valueType, recursionInfo);
                }

                if (type.IsValueType())
                {
                    return type.CreateInstance();
                }

                if (type.IsArray)
                {
                    return PopulateArray(type, recursionInfo);
                }

                var constructorInfo = type.GetEmptyConstructor();
                var hasEmptyConstructor = constructorInfo != null;

                if (hasEmptyConstructor)
                {
                    var value = constructorInfo.Invoke(new object[0]);

#if !SILVERLIGHT && !MONOTOUCH && !XBOX

                    var genericCollectionType = GetGenericCollectionType(type);
                    if (genericCollectionType != null)
                    {
                        SetGenericCollection(genericCollectionType, value, recursionInfo);
                    }
#endif

                    //when the object might have nested properties such as enums with non-0 values, etc
                    return PopulateObjectInternal(value, recursionInfo);
                }
                return null;
            }
            finally
            {
                recursionInfo[type] = recurseLevel;
            }
        }

        private static Type GetGenericCollectionType(Type type)
        {
#if NETFX_CORE
            var genericCollectionType =
                type.GetTypeInfo().ImplementedInterfaces
                    .FirstOrDefault(t => t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == typeof (ICollection<>));
#elif WINDOWS_PHONE || SILVERLIGHT
            var genericCollectionType =
                type.GetInterfaces()
                    .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof (ICollection<>));
#else
            var genericCollectionType = type.FindInterfaces((t, critera) =>
                t.IsGenericType
                && t.GetGenericTypeDefinition() == typeof(ICollection<>), null).FirstOrDefault();
#endif

            return genericCollectionType;
        }

        public static void SetGenericCollection(Type realisedListType, object genericObj, Dictionary<Type, int> recursionInfo)
        {
            var args = realisedListType.GenericTypeArguments();
            if (args.Length != 1)
            {
                Tracer.Instance.WriteError("Found a generic list that does not take one generic argument: {0}", realisedListType);

                return;
            }

            var methodInfo = realisedListType.GetMethodInfo("Add");
            if (methodInfo != null)
            {
                var argValues = CreateDefaultValues(args, recursionInfo);

                methodInfo.Invoke(genericObj, argValues);
            }
        }

        public static Array PopulateArray(Type type, Dictionary<Type, int> recursionInfo)
        {
            var elementType = type.GetElementType();
            var objArray = Array.CreateInstance(elementType, 1);
            var objElementType = CreateDefaultValue(elementType, recursionInfo);
            objArray.SetValue(objElementType, 0);

            return objArray;
        }

        //TODO: replace with InAssignableFrom
        public static bool CanCast(Type toType, Type fromType)
        {
            if (toType.IsInterface())
            {
                var interfaceList = fromType.Interfaces().ToList();
                if (interfaceList.Contains(toType)) return true;
            }
            else
            {
                Type baseType = fromType;
                bool areSameTypes;
                do
                {
                    areSameTypes = baseType == toType;
                }
                while (!areSameTypes && (baseType = fromType.BaseType()) != null);

                if (areSameTypes) return true;
            }

            return false;
        }

        public static IEnumerable<KeyValuePair<PropertyInfo, T>> GetPropertyAttributes<T>(Type fromType)
        {
            var attributeType = typeof(T);
            var baseType = fromType;
            do
            {
                var propertyInfos = baseType.AllProperties();
                foreach (var propertyInfo in propertyInfos)
                {
                    var attributes = propertyInfo.GetCustomAttributes(attributeType, true);
                    foreach (T attribute in attributes)
                    {
                        yield return new KeyValuePair<PropertyInfo, T>(propertyInfo, attribute);
                    }
                }
            }
            while ((baseType = baseType.BaseType()) != null);
        }
    }

    public class AssignmentEntry
    {
        public string Name;
        public AssignmentMember From;
        public AssignmentMember To;
        public PropertyGetterDelegate GetValueFn;
        public PropertySetterDelegate SetValueFn;

        public AssignmentEntry(string name, AssignmentMember @from, AssignmentMember to)
        {
            Name = name;
            From = @from;
            To = to;

            GetValueFn = From.GetGetValueFn();
            SetValueFn = To.GetSetValueFn();
        }
    }

    public class AssignmentMember
    {
        public AssignmentMember(Type type, PropertyInfo propertyInfo)
        {
            Type = type;
            PropertyInfo = propertyInfo;
        }

        public AssignmentMember(Type type, FieldInfo fieldInfo)
        {
            Type = type;
            FieldInfo = fieldInfo;
        }

        public AssignmentMember(Type type, MethodInfo methodInfo)
        {
            Type = type;
            MethodInfo = methodInfo;
        }

        public Type Type;
        public PropertyInfo PropertyInfo;
        public FieldInfo FieldInfo;
        public MethodInfo MethodInfo;

        public PropertyGetterDelegate GetGetValueFn()
        {
            if (PropertyInfo != null)
                return PropertyInfo.GetPropertyGetterFn();
            if (FieldInfo != null)
                return o => FieldInfo.GetValue(o);
            if (MethodInfo != null)
#if NETFX_CORE
                return (PropertyGetterDelegate)
                    MethodInfo.CreateDelegate(typeof(PropertyGetterDelegate));
#else
                return (PropertyGetterDelegate)
                    Delegate.CreateDelegate(typeof(PropertyGetterDelegate), MethodInfo);
#endif
            return null;
        }

        public PropertySetterDelegate GetSetValueFn()
        {
            if (PropertyInfo != null)
                return PropertyInfo.GetPropertySetterFn();
            if (FieldInfo != null)
                return (o, v) => FieldInfo.SetValue(o, v);
            if (MethodInfo != null)
                return (PropertySetterDelegate)MethodInfo.MakeDelegate(typeof(PropertySetterDelegate));

            return null;
        }
    }

    internal class AssignmentDefinition
    {
        public AssignmentDefinition()
        {
            this.AssignmentMemberMap = new Dictionary<string, AssignmentEntry>();
        }

        public Type FromType { get; set; }
        public Type ToType { get; set; }

        public Dictionary<string, AssignmentEntry> AssignmentMemberMap { get; set; }

        public void AddMatch(string name, AssignmentMember readMember, AssignmentMember writeMember)
        {
            this.AssignmentMemberMap[name] = new AssignmentEntry(name, readMember, writeMember);
        }

        public void PopulateFromPropertiesWithAttribute(object to, object from, Type attributeType)
        {
            var hasAttributePredicate = (Func<PropertyInfo, bool>)
                (x => x.AllAttributes(attributeType).Length > 0);
            Populate(to, from, hasAttributePredicate, null);
        }

        public void PopulateWithNonDefaultValues(object to, object from)
        {
            var nonDefaultPredicate = (Func<object, Type, bool>)((x, t) =>
                    x != null && !Equals(x, AutoMappingUtils.GetDefaultValue(t))
                );

            Populate(to, from, null, nonDefaultPredicate);
        }

        public void Populate(object to, object from)
        {
            Populate(to, from, null, null);
        }

        public void Populate(object to, object from,
            Func<PropertyInfo, bool> propertyInfoPredicate,
            Func<object, Type, bool> valuePredicate)
        {
            foreach (var assignmentEntry in AssignmentMemberMap)
            {
                var assignmentMember = assignmentEntry.Value;
                var fromMember = assignmentEntry.Value.From;
                var toMember = assignmentEntry.Value.To;

                if (fromMember.PropertyInfo != null && propertyInfoPredicate != null)
                {
                    if (!propertyInfoPredicate(fromMember.PropertyInfo)) continue;
                }

                try
                {
                    var fromValue = assignmentMember.GetValueFn(from);

                    if (valuePredicate != null)
                    {
                        if (!valuePredicate(fromValue, fromMember.PropertyInfo.PropertyType)) continue;
                    }

                    if (fromMember.Type != toMember.Type)
                    {
                        if (fromMember.Type == typeof(string))
                        {
                            fromValue = TypeSerializer.DeserializeFromString((string)fromValue, toMember.Type);
                        }
                        else if (toMember.Type == typeof(string))
                        {
                            fromValue = TypeSerializer.SerializeToString(fromValue);
                        }
                        else if (toMember.Type.IsGeneric()
                            && toMember.Type.GenericTypeDefinition() == typeof(Nullable<>))
                        {
                            Type genericArg = toMember.Type.GenericTypeArguments()[0];
                            if (genericArg.IsEnum())
                            {
                                fromValue = Enum.ToObject(genericArg, fromValue);
                            }
                        }
                        else
                        {
                            var listResult = TranslateListWithElements.TryTranslateToGenericICollection(
                                fromMember.Type, toMember.Type, fromValue);

                            if (listResult != null)
                            {
                                fromValue = listResult;
                            }
                        }
                    }

                    var setterFn = assignmentMember.SetValueFn;
                    setterFn(to, fromValue);
                }
                catch (Exception ex)
                {
                    Tracer.Instance.WriteWarning("Error trying to set properties {0}.{1} > {2}.{3}:\n{4}",
                        FromType.FullName, fromMember.Type.Name,
                        ToType.FullName, toMember.Type.Name, ex);
                }
            }
        }
    }

    public delegate void PropertySetterDelegate(object instance, object value);
    public delegate object PropertyGetterDelegate(object instance);

    internal static class PropertyInvoker
    {
        public static PropertySetterDelegate GetPropertySetterFn(this PropertyInfo propertyInfo)
        {
            var propertySetMethod = propertyInfo.SetMethod();
            if (propertySetMethod == null) return null;

#if MONOTOUCH || SILVERLIGHT || XBOX
            return (o, convertedValue) =>
            {
                propertySetMethod.Invoke(o, new[] { convertedValue });
                return;
            };
#else
            var instance = Expression.Parameter(typeof(object), "i");
            var argument = Expression.Parameter(typeof(object), "a");

            var instanceParam = Expression.Convert(instance, propertyInfo.ReflectedType);
            var valueParam = Expression.Convert(argument, propertyInfo.PropertyType);

            var setterCall = Expression.Call(instanceParam, propertyInfo.GetSetMethod(), valueParam);

            return Expression.Lambda<PropertySetterDelegate>(setterCall, instance, argument).Compile();
#endif
        }

        public static PropertyGetterDelegate GetPropertyGetterFn(this PropertyInfo propertyInfo)
        {
            var getMethodInfo = propertyInfo.GetMethodInfo();
            if (getMethodInfo == null) return null;

#if MONOTOUCH || SILVERLIGHT || XBOX
#if NETFX_CORE
            return o => propertyInfo.GetMethod.Invoke(o, new object[] { });
#else
            return o => propertyInfo.GetGetMethod().Invoke(o, new object[] { });
#endif
#else
            try
            {
                var oInstanceParam = Expression.Parameter(typeof(object), "oInstanceParam");
                var instanceParam = Expression.Convert(oInstanceParam, propertyInfo.ReflectedType); //propertyInfo.DeclaringType doesn't work on Proxy types

                var exprCallPropertyGetFn = Expression.Call(instanceParam, getMethodInfo);
                var oExprCallPropertyGetFn = Expression.Convert(exprCallPropertyGetFn, typeof(object));

                var propertyGetFn = Expression.Lambda<PropertyGetterDelegate>
                    (
                        oExprCallPropertyGetFn,
                        oInstanceParam
                    ).Compile();

                return propertyGetFn;

            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                throw;
            }
#endif
        }
    }
}
