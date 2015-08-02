// Copyright (c) Service Stack LLC. All Rights Reserved.
// License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using ServiceStack.Text;
using ServiceStack.Text.Common;

namespace ServiceStack
{
    [DataContract(Namespace = "http://schemas.servicestack.net/types")]
    public class CustomHttpResult { }

    public static class AutoMappingUtils
    {
        public static T ConvertTo<T>(this object from)
        {
            if (from == null)
                return default(T);

            if (from.GetType() == typeof(T))
                return (T)from;

            if (from.GetType().IsValueType() || typeof(T).IsValueType())
                return (T)ChangeValueType(from, typeof(T));

            if (typeof(IEnumerable).IsAssignableFromType(typeof(T)))
            {
                var listResult = TranslateListWithElements.TryTranslateCollections(
                    from.GetType(), typeof(T), from);

                return (T)listResult;
            }

            var to = typeof(T).CreateInstance<T>();
            return to.PopulateWith(from);
        }

        public static object ConvertTo(this object from, Type type)
        {
            if (from == null)
                return null;

            if (from.GetType() == type)
                return from;

            if (from.GetType().IsValueType() || type.IsValueType())
                return ChangeValueType(from, type);

            if (typeof(IEnumerable).IsAssignableFromType(type))
            {
                var listResult = TranslateListWithElements.TryTranslateCollections(
                    from.GetType(), type, from);

                return listResult;
            }

            var to = type.CreateInstance();
            return to.PopulateInstance(from);
        }

        private static object ChangeValueType(object from, Type type)
        {
            var strValue = from as string;
            if (strValue != null)
                return TypeSerializer.DeserializeFromString(strValue, type);

            if (type == typeof(string))
                return from.ToJsv();

            return Convert.ChangeType(from, type, provider: null);
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

        public static string GetAssemblyPath(this Type source)
        {
            return PclExport.Instance.GetAssemblyPath(source);
        }

        public static bool IsDebugBuild(this Assembly assembly)
        {
            return PclExport.Instance.IsDebugBuild(assembly);
        }

        /// <summary>
        /// Populate an object with Example data.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object PopulateWith(object obj)
        {
            if (obj == null) return null;
            var isHttpResult = obj.GetType().Interfaces().Any(x => x.Name == "IHttpResult"); // No coupling FTW!
            if (isHttpResult)
            {
                obj = new CustomHttpResult();
            }

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

        private static Dictionary<Type, object> DefaultValueTypes = new Dictionary<Type, object>();

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
                Interlocked.CompareExchange(ref DefaultValueTypes, newCache, snapshot), snapshot));

            return defaultValue;
        }

        private static readonly ConcurrentDictionary<string, AssignmentDefinition> AssignmentDefinitionCache
            = new ConcurrentDictionary<string, AssignmentDefinition>();

        internal static AssignmentDefinition GetAssignmentDefinition(Type toType, Type fromType)
        {
            var cacheKey = CreateCacheKey(fromType, toType);

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
        }

        internal static string CreateCacheKey(Type fromType, Type toType)
        {
            var cacheKey = fromType.FullName + ">" + toType.FullName;
            return cacheKey;
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
                        if (propertyInfo.CanWrite && propertyInfo.SetMethod() != null)
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

        public static object PopulateInstance(this object to, object from)
        {
            if (to == null || from == null)
                return null;

            var assignmentDefinition = GetAssignmentDefinition(to.GetType(), from.GetType());

            assignmentDefinition.PopulateWithNonDefaultValues(to, from);

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

        public static To PopulateFromPropertiesWithoutAttribute<To, From>(this To to, From from,
            Type attributeType)
        {
            if (Equals(to, default(To)) || Equals(from, default(From))) return default(To);

            var assignmentDefinition = GetAssignmentDefinition(to.GetType(), from.GetType());

            assignmentDefinition.PopulateFromPropertiesWithoutAttribute(to, from, attributeType);

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
            // Properties on non-user defined classes should not be set
            // Currently we define those properties as properties declared on
            // types defined in mscorlib

            if (propertyInfo != null && propertyInfo.ReflectedType() != null)
            {
                return PclExport.Instance.InSameAssembly(propertyInfo.DeclaringType, typeof(object));
            }

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
#if SL5 || WP
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

                    var genericCollectionType = PclExport.Instance.GetGenericCollectionType(type);
                    if (genericCollectionType != null)
                    {
                        SetGenericCollection(genericCollectionType, value, recursionInfo);
                    }

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
                    foreach (var attribute in attributes)
                    {
                        yield return new KeyValuePair<PropertyInfo, T>(propertyInfo, (T)(object)attribute);
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
        public PropertyGetterDelegate ConvertValueFn;

        public AssignmentEntry(string name, AssignmentMember @from, AssignmentMember to)
        {
            Name = name;
            From = @from;
            To = to;

            GetValueFn = From.GetGetValueFn();
            SetValueFn = To.GetSetValueFn();
            ConvertValueFn = TypeConverter.CreateTypeConverter(From.Type, To.Type);
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
                return FieldInfo.GetFieldGetterFn();
            if (MethodInfo != null)
                return (PropertyGetterDelegate)
                    MethodInfo.CreateDelegate(typeof(PropertyGetterDelegate));

            return null;
        }

        public PropertySetterDelegate GetSetValueFn()
        {
            if (PropertyInfo != null)
                return PropertyInfo.GetPropertySetterFn();
            if (FieldInfo != null)
                return FieldInfo.GetFieldSetterFn();
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

        public void PopulateFromPropertiesWithoutAttribute(object to, object from, Type attributeType)
        {
            var hasAttributePredicate = (Func<PropertyInfo, bool>)
                (x => x.AllAttributes(attributeType).Length == 0);
            Populate(to, from, hasAttributePredicate, null);
        }

        public void PopulateWithNonDefaultValues(object to, object from)
        {
            var nonDefaultPredicate = (Func<object, Type, bool>)((x, t) =>
                    x != null && !Equals(x, t.GetDefaultValue())
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
            foreach (var assignmentEntryMap in AssignmentMemberMap)
            {
                var assignmentEntry = assignmentEntryMap.Value;
                var fromMember = assignmentEntry.From;
                var toMember = assignmentEntry.To;

                if (fromMember.PropertyInfo != null && propertyInfoPredicate != null)
                {
                    if (!propertyInfoPredicate(fromMember.PropertyInfo)) continue;
                }

                var fromType = fromMember.Type;
                var toType = toMember.Type;
                try
                {
                    var fromValue = assignmentEntry.GetValueFn(from);

                    if (valuePredicate != null)
                    {
                        if (!valuePredicate(fromValue, fromMember.PropertyInfo.PropertyType)) continue;
                    }

                    if (assignmentEntry.ConvertValueFn != null)
                    {
                        fromValue = assignmentEntry.ConvertValueFn(fromValue);
                    }

                    var setterFn = assignmentEntry.SetValueFn;
                    setterFn(to, fromValue);
                }
                catch (Exception ex)
                {
                    Tracer.Instance.WriteWarning("Error trying to set properties {0}.{1} > {2}.{3}:\n{4}",
                        FromType.FullName, fromType.Name,
                        ToType.FullName, toType.Name, ex);
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
            return PclExport.Instance.GetPropertySetterFn(propertyInfo);
        }

        public static PropertyGetterDelegate GetPropertyGetterFn(this PropertyInfo propertyInfo)
        {
            return PclExport.Instance.GetPropertyGetterFn(propertyInfo);
        }
    }

    internal static class FieldInvoker
    {
        public static PropertySetterDelegate GetFieldSetterFn(this FieldInfo fieldInfo)
        {
            return PclExport.Instance.GetFieldSetterFn(fieldInfo);
        }

        public static PropertyGetterDelegate GetFieldGetterFn(this FieldInfo fieldInfo)
        {
            return PclExport.Instance.GetFieldGetterFn(fieldInfo);
        }
    }

    internal static class TypeConverter
    {
        public static PropertyGetterDelegate CreateTypeConverter(Type fromType, Type toType)
        {
            if (fromType == toType)
                return null;

            if (fromType == typeof(string))
            {
                return fromValue => TypeSerializer.DeserializeFromString((string)fromValue, toType);
            }
            if (toType == typeof(string))
            {
                return TypeSerializer.SerializeToString;
            }
            if (toType.IsEnum() || fromType.IsEnum())
            {
                if (toType.IsEnum() && fromType.IsEnum())
                {
                    return fromValue => Enum.Parse(toType, fromValue.ToString(), ignoreCase: true);
                }
                if (toType.IsNullableType())
                {
                    var genericArg = toType.GenericTypeArguments()[0];
                    if (genericArg.IsEnum())
                    {
                        return fromValue => Enum.ToObject(genericArg, fromValue);
                    }
                }
                else if (toType.IsIntegerType())
                {
                    if (fromType.IsNullableType())
                    {
                        var genericArg = fromType.GenericTypeArguments()[0];
                        if (genericArg.IsEnum())
                        {
                            return fromValue => Enum.ToObject(genericArg, fromValue);
                        }
                    }
                    return fromValue => Enum.ToObject(fromType, fromValue);
                }
            }
            else if (toType.IsNullableType())
            {
                var toTypeBaseType = toType.GenericTypeArguments()[0];
                if (toTypeBaseType.IsEnum())
                {
                    if (fromType.IsEnum() || (fromType.IsNullableType() && fromType.GenericTypeArguments()[0].IsEnum()))
                        return fromValue => Enum.ToObject(toTypeBaseType, fromValue);
                }
                return null;
            }
            else if (typeof(IEnumerable).IsAssignableFromType(fromType))
            {
                return fromValue =>
                {
                    var listResult = TranslateListWithElements.TryTranslateCollections(
                        fromType, toType, fromValue);

                    return listResult ?? fromValue;
                };
            }
            else if (toType.IsValueType())
            {
                return fromValue => Convert.ChangeType(fromValue, toType, provider: null);
            }
            else
            {
                return fromValue =>
                {
                    if (fromValue == null)
                        return fromValue;

                    var toValue = toType.CreateInstance();
                    toValue.PopulateWith(fromValue);
                    return toValue;
                };
            }

            return null;
        }
    }

}
