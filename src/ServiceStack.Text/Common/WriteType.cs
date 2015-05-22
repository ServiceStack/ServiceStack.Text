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
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using ServiceStack.Reflection;
using ServiceStack.Text.Json;

namespace ServiceStack.Text.Common
{
    internal static class WriteType<T, TSerializer>
        where TSerializer : ITypeSerializer
    {
        private static readonly ITypeSerializer Serializer = JsWriter.GetTypeSerializer<TSerializer>();

        private static readonly WriteObjectDelegate CacheFn;
        internal static TypePropertyWriter[] PropertyWriters;
        private static readonly WriteObjectDelegate WriteTypeInfo;

        private static bool IsIncluded
        {
            get { return (JsConfig<T>.IncludeTypeInfo.GetValueOrDefault(JsConfig.IncludeTypeInfo)); }
        }

        private static bool IsExcluded
        {
            get { return (JsConfig<T>.ExcludeTypeInfo.GetValueOrDefault(JsConfig.ExcludeTypeInfo)); }
        }

        static WriteType()
        {
            if (typeof(T) == typeof(Object))
            {
                CacheFn = WriteObjectType;
            }
            else
            {
                CacheFn = Init() ? GetWriteFn() : WriteEmptyType;
            }

            if (IsIncluded)
            {
                WriteTypeInfo = TypeInfoWriter;
            }

            if (typeof(T).IsAbstract())
            {
                WriteTypeInfo = TypeInfoWriter;
                if (!JsConfig.PreferInterfaces || !typeof(T).IsInterface())
                {
                    CacheFn = WriteAbstractProperties;
                }
            }
        }

        public static void TypeInfoWriter(TextWriter writer, object obj)
        {
            TryWriteTypeInfo(writer, obj);
        }

        private static bool ShouldSkipType() { return IsExcluded && !IsIncluded; }

        private static bool TryWriteSelfType(TextWriter writer)
        {
            if (ShouldSkipType()) return false;

            Serializer.WriteRawString(writer, JsConfig.TypeAttr);
            writer.Write(JsWriter.MapKeySeperator);
            Serializer.WriteRawString(writer, JsConfig.TypeWriter(typeof(T)));
            return true;
        }

        private static bool TryWriteTypeInfo(TextWriter writer, object obj)
        {
            if (obj == null || ShouldSkipType()) return false;

            Serializer.WriteRawString(writer, JsConfig.TypeAttr);
            writer.Write(JsWriter.MapKeySeperator);
            Serializer.WriteRawString(writer, JsConfig.TypeWriter(obj.GetType()));
            return true;
        }

        public static WriteObjectDelegate Write
        {
            get { return CacheFn; }
        }

        private static WriteObjectDelegate GetWriteFn()
        {
            return WriteProperties;
        }

        static Func<T, bool> GetShouldSerializeMethod(MemberInfo member)
        {
            var method = member.DeclaringType.GetInstanceMethod("ShouldSerialize" + member.Name);
            return method == null || method.ReturnType != typeof(bool) 
                ? null 
                : (Func<T, bool>)method.CreateDelegate(typeof(Func<T, bool>));
        }

        static Func<T, string, bool?> ShouldSerialize(Type type)
        {
            var method = type.GetMethodInfo("ShouldSerialize");
            return method == null || method.ReturnType != typeof(bool?) 
                ? null
                : (Func<T, string, bool?>)method.CreateDelegate(typeof(Func<T, string, bool?>));
        }

        private static bool Init()
        {
            if (!typeof(T).IsClass() && !typeof(T).IsInterface() && !JsConfig.TreatAsRefType(typeof(T))) return false;

            var propertyInfos = TypeConfig<T>.Properties;
            var fieldInfos = TypeConfig<T>.Fields;
            var propertyNamesLength = propertyInfos.Length;
            var fieldNamesLength = fieldInfos.Length;
            PropertyWriters = new TypePropertyWriter[propertyNamesLength + fieldNamesLength];

            if (propertyNamesLength + fieldNamesLength == 0 && !JsState.IsWritingDynamic)
            {
                return typeof(T).IsDto();
            }

            var shouldSerializeDynamic = ShouldSerialize(typeof(T));

            // NOTE: very limited support for DataContractSerialization (DCS)
            //	NOT supporting Serializable
            //	support for DCS is intended for (re)Name of properties and Ignore by NOT having a DataMember present
            var isDataContract = typeof(T).IsDto();
            for (var i = 0; i < propertyNamesLength; i++)
            {
                var propertyInfo = propertyInfos[i];

                string propertyName, propertyNameCLSFriendly, propertyNameLowercaseUnderscore, propertyDeclaredTypeName;
                int propertyOrder = -1;
                var propertyType = propertyInfo.PropertyType;
                var defaultValue = propertyType.GetDefaultValue();
                bool propertySuppressDefaultConfig = defaultValue != null 
                    && propertyType.IsValueType() 
                    && !propertyType.IsEnum() 
                    && JsConfig.HasSerializeFn.Contains(propertyType) 
                    && !JsConfig.HasIncludeDefaultValue.Contains(propertyType);
                bool propertySuppressDefaultAttribute = false;

                var shouldSerialize = GetShouldSerializeMethod(propertyInfo);
                if (isDataContract)
                {
                    var dcsDataMember = propertyInfo.GetDataMember();
                    if (dcsDataMember == null) continue;

                    propertyName = dcsDataMember.Name ?? propertyInfo.Name;
                    propertyNameCLSFriendly = dcsDataMember.Name ?? propertyName.ToCamelCase();
                    propertyNameLowercaseUnderscore = dcsDataMember.Name ?? propertyName.ToLowercaseUnderscore();
                    propertyDeclaredTypeName = propertyType.GetDeclaringTypeName();
                    propertyOrder = dcsDataMember.Order;
                    propertySuppressDefaultAttribute = !dcsDataMember.EmitDefaultValue;
                }
                else
                {
                    propertyName = propertyInfo.Name;
                    propertyNameCLSFriendly = propertyName.ToCamelCase();
                    propertyNameLowercaseUnderscore = propertyName.ToLowercaseUnderscore();
                    propertyDeclaredTypeName = propertyInfo.GetDeclaringTypeName();
                }


                PropertyWriters[i] = new TypePropertyWriter
                (
                    propertyName,
                    propertyDeclaredTypeName,
                    propertyNameCLSFriendly,
                    propertyNameLowercaseUnderscore,
                    propertyOrder,
                    propertySuppressDefaultConfig,
                    propertySuppressDefaultAttribute,
                    propertyInfo.GetValueGetter<T>(),
                    Serializer.GetWriteFn(propertyType),
                    propertyType.GetDefaultValue(),
                    shouldSerialize,
                    shouldSerializeDynamic,
                    propertyType.IsEnum()
                );
            }

            for (var i = 0; i < fieldNamesLength; i++)
            {
                var fieldInfo = fieldInfos[i];

                string propertyName, propertyNameCLSFriendly, propertyNameLowercaseUnderscore, propertyDeclaredTypeName;
                int propertyOrder = -1;
                var propertyType = fieldInfo.FieldType;
                var defaultValue = propertyType.GetDefaultValue();
                bool propertySuppressDefaultConfig = defaultValue != null 
                    && propertyType.IsValueType() && !propertyType.IsEnum() 
                    && JsConfig.HasSerializeFn.Contains(propertyType) 
                    && !JsConfig.HasIncludeDefaultValue.Contains(propertyType);
                bool propertySuppressDefaultAttribute = false;
#if (NETFX_CORE)
                var shouldSerialize = (Func<T, bool>)null;
#else
                var shouldSerialize = GetShouldSerializeMethod(fieldInfo);
#endif
                if (isDataContract)
                {
                    var dcsDataMember = fieldInfo.GetDataMember();
                    if (dcsDataMember == null) continue;

                    propertyName = dcsDataMember.Name ?? fieldInfo.Name;
                    propertyNameCLSFriendly = dcsDataMember.Name ?? propertyName.ToCamelCase();
                    propertyNameLowercaseUnderscore = dcsDataMember.Name ?? propertyName.ToLowercaseUnderscore();
                    propertyDeclaredTypeName = fieldInfo.DeclaringType.Name;
                    propertyOrder = dcsDataMember.Order;
                    propertySuppressDefaultAttribute = !dcsDataMember.EmitDefaultValue;
                }
                else
                {
                    propertyName = fieldInfo.Name;
                    propertyNameCLSFriendly = propertyName.ToCamelCase();
                    propertyNameLowercaseUnderscore = propertyName.ToLowercaseUnderscore();
                    propertyDeclaredTypeName = fieldInfo.DeclaringType.Name;
                }

                PropertyWriters[i + propertyNamesLength] = new TypePropertyWriter
                (
                    propertyName,
                    propertyDeclaredTypeName,
                    propertyNameCLSFriendly,
                    propertyNameLowercaseUnderscore,
                    propertyOrder,
                    propertySuppressDefaultConfig,
                    propertySuppressDefaultAttribute,
                    fieldInfo.GetValueGetter<T>(),
                    Serializer.GetWriteFn(propertyType),
                    defaultValue,
                    shouldSerialize,
                    shouldSerializeDynamic,
                    propertyType.IsEnum()
                );
            }

            PropertyWriters = PropertyWriters.OrderBy(x => x.propertyOrder).ToArray();
            return true;
        }

        internal struct TypePropertyWriter
        {
            internal string PropertyName
            {
                get
                {
                    return (JsConfig<T>.EmitCamelCaseNames.GetValueOrDefault(JsConfig.EmitCamelCaseNames))
                        ? propertyNameCLSFriendly
                        : (JsConfig<T>.EmitLowercaseUnderscoreNames.GetValueOrDefault(JsConfig.EmitLowercaseUnderscoreNames))
                            ? propertyNameLowercaseUnderscore
                            : propertyName;
                }
            }
            internal readonly string propertyName;
            internal readonly int propertyOrder;
            internal readonly bool propertySuppressDefaultConfig;
            internal readonly bool propertySuppressDefaultAttribute;
            internal readonly string propertyReferenceName;
            internal readonly string propertyNameCLSFriendly;
            internal readonly string propertyNameLowercaseUnderscore;
            internal readonly Func<T, object> GetterFn;
            internal readonly WriteObjectDelegate WriteFn;
            internal readonly object DefaultValue;
            internal readonly Func<T, bool> shouldSerialize;
            internal readonly Func<T, string, bool?> shouldSerializeDynamic;
            internal readonly bool isEnum;

            public TypePropertyWriter(string propertyName, string propertyDeclaredTypeName, string propertyNameCLSFriendly, 
                string propertyNameLowercaseUnderscore, int propertyOrder, bool propertySuppressDefaultConfig,bool propertySuppressDefaultAttribute,
                Func<T, object> getterFn, WriteObjectDelegate writeFn, object defaultValue, 
                Func<T, bool> shouldSerialize, 
                Func<T,string, bool?> shouldSerializeDynamic,
                bool isEnum)
            {
                this.propertyName = propertyName;
                this.propertyOrder = propertyOrder;
                this.propertySuppressDefaultConfig = propertySuppressDefaultConfig;
                this.propertySuppressDefaultAttribute = propertySuppressDefaultAttribute;
                this.propertyReferenceName = propertyDeclaredTypeName + "." + propertyName;
                this.propertyNameCLSFriendly = propertyNameCLSFriendly;
                this.propertyNameLowercaseUnderscore = propertyNameLowercaseUnderscore;
                this.GetterFn = getterFn;
                this.WriteFn = writeFn;
                this.DefaultValue = defaultValue;
                this.shouldSerialize = shouldSerialize;
                this.shouldSerializeDynamic = shouldSerializeDynamic;
                this.isEnum = isEnum;
            }

            public bool ShouldWriteProperty(object propertyValue)
            {
                if ((propertySuppressDefaultAttribute || JsConfig.ExcludeDefaultValues) && Equals(DefaultValue, propertyValue))
                    return false;

                if (!Serializer.IncludeNullValues
                    && (propertyValue == null
                        || (propertySuppressDefaultConfig && Equals(DefaultValue, propertyValue))))
                {
                    return false;
                }

                if (isEnum && !JsConfig.IncludeDefaultEnums && Equals(DefaultValue, propertyValue))
                    return false;

                return true;
            }
        }

        public static void WriteObjectType(TextWriter writer, object value)
        {
            writer.Write(JsWriter.EmptyMap);
        }

        public static void WriteEmptyType(TextWriter writer, object value)
        {
            if (WriteTypeInfo != null || JsState.IsWritingDynamic)
            {
                writer.Write(JsWriter.MapStartChar);
                if (!(JsConfig.PreferInterfaces && TryWriteSelfType(writer)))
                {
                    TryWriteTypeInfo(writer, value);
                }
                writer.Write(JsWriter.MapEndChar);
                return;
            }
            writer.Write(JsWriter.EmptyMap);
        }

        public static void WriteAbstractProperties(TextWriter writer, object value)
        {
            if (value == null)
            {
                writer.Write(JsWriter.EmptyMap);
                return;
            }
            var valueType = value.GetType();
            if (valueType.IsAbstract())
            {
                WriteProperties(writer, value);
                return;
            }

            WriteLateboundProperties(writer, value, valueType);
        }

        public static void WriteProperties(TextWriter writer, object value)
        {
            if (value == null)
            {
                writer.Write(JsWriter.EmptyMap);
                return;
            }

            var valueType = value.GetType();
            if (PropertyWriters != null && valueType != typeof(T) && !typeof(T).IsAbstract())
            {
                WriteLateboundProperties(writer, value, valueType);
                return;
            }

            if (typeof(TSerializer) == typeof(JsonTypeSerializer) && JsState.WritingKeyCount > 0)
                writer.Write(JsWriter.QuoteChar);

            writer.Write(JsWriter.MapStartChar);

            var i = 0;
            if (WriteTypeInfo != null || JsState.IsWritingDynamic)
            {
                if (JsConfig.PreferInterfaces && TryWriteSelfType(writer)) i++;
                else if (TryWriteTypeInfo(writer, value)) i++;
                JsState.IsWritingDynamic = false;
            }

            if (PropertyWriters != null)
            {
                var len = PropertyWriters.Length;
                for (int index = 0; index < len; index++)
                {
                    var propertyWriter = PropertyWriters[index];

                    if (propertyWriter.shouldSerialize != null && !propertyWriter.shouldSerialize((T)value)) 
                        continue;

                    var dontSkipDefault = false;
                    if (propertyWriter.shouldSerializeDynamic != null)
                    {
                        var shouldSerialize = propertyWriter.shouldSerializeDynamic((T)value, propertyWriter.PropertyName);
                        if (shouldSerialize.HasValue)
                        {
                            if (shouldSerialize.Value)
                                dontSkipDefault = true;
                            else
                                continue;
                        }
                    }

                    var propertyValue = propertyWriter.GetterFn((T)value);

                    if (!dontSkipDefault)
                    {
                        if (!propertyWriter.ShouldWriteProperty(propertyValue))
                            continue;

                        if (JsConfig.ExcludePropertyReferences != null
                            && JsConfig.ExcludePropertyReferences.Contains(propertyWriter.propertyReferenceName)) continue;
                    }

                    if (i++ > 0)
                        writer.Write(JsWriter.ItemSeperator);

                    Serializer.WritePropertyName(writer, propertyWriter.PropertyName);
                    writer.Write(JsWriter.MapKeySeperator);

                    if (typeof(TSerializer) == typeof(JsonTypeSerializer)) JsState.IsWritingValue = true;
                    try
                    {
                        if (propertyValue == null)
                        {
                            writer.Write(JsonUtils.Null);
                        }
                        else
                        {
                            propertyWriter.WriteFn(writer, propertyValue);
                        }
                    }
                    finally
                    {
                        if (typeof(TSerializer) == typeof(JsonTypeSerializer)) JsState.IsWritingValue = false;
                    } 
                }
            }

            writer.Write(JsWriter.MapEndChar);

            if (typeof(TSerializer) == typeof(JsonTypeSerializer) && JsState.WritingKeyCount > 0)
                writer.Write(JsWriter.QuoteChar);
        }

        private static void WriteLateboundProperties(TextWriter writer, object value, Type valueType)
        {
            var writeFn = Serializer.GetWriteFn(valueType);
            if (!JsConfig<T>.ExcludeTypeInfo.GetValueOrDefault()) JsState.IsWritingDynamic = true;
            writeFn(writer, value);
            if (!JsConfig<T>.ExcludeTypeInfo.GetValueOrDefault()) JsState.IsWritingDynamic = false;
        }

        private static readonly char[] ArrayBrackets = new[] { '[', ']' };

        public static void WriteComplexQueryStringProperties(string typeName, TextWriter writer, object value)
        {
            var i = 0;
            if (PropertyWriters != null)
            {
                var len = PropertyWriters.Length;
                for (int index = 0; index < len; index++)
                {
                    var propertyWriter = PropertyWriters[index];
                    if (propertyWriter.shouldSerialize != null && !propertyWriter.shouldSerialize((T)value)) continue;

                    var propertyValue = value != null ? propertyWriter.GetterFn((T)value) : null;
                    if (propertyWriter.propertySuppressDefaultAttribute && Equals(propertyWriter.DefaultValue, propertyValue))
                        continue;

                    if ((propertyValue == null
                         || (propertyWriter.propertySuppressDefaultConfig && Equals(propertyWriter.DefaultValue, propertyValue)))
                        && !Serializer.IncludeNullValues)
                        continue;

                    if (JsConfig.ExcludePropertyReferences != null
                        && JsConfig.ExcludePropertyReferences.Contains(propertyWriter.propertyReferenceName)) continue;

                    if (i++ > 0)
                        writer.Write('&');

                    writer.Write(typeName);
                    writer.Write('[');
                    writer.Write(propertyWriter.PropertyName);
                    writer.Write(']');

                    writer.Write('=');

                    if (propertyValue == null)
                    {
                        writer.Write(JsonUtils.Null);
                    }
                    else
                    {
                        propertyWriter.WriteFn(writer, propertyValue);
                    }
                }
            }
        }

        public static void WriteQueryString(TextWriter writer, object value)
        {
            try
            {
                JsState.QueryStringMode = true;
                var i = 0;
                foreach (var propertyWriter in PropertyWriters)
                {
                    var propertyValue = propertyWriter.GetterFn((T)value);
                    if (propertyValue == null) continue;

                    if (i++ > 0)
                        writer.Write('&');

                    var propertyType = propertyValue.GetType();
                    var strValue = propertyValue as string;
                    var isEnumerable = strValue == null
                        && !(propertyType.IsValueType())
                        && propertyType.HasInterface(typeof(IEnumerable));
                    
                    if (QueryStringSerializer.ComplexTypeStrategy != null
                        && !isEnumerable && (propertyType.IsUserType() || propertyType.IsInterface()))
                    {
                        if (QueryStringSerializer.ComplexTypeStrategy(writer, propertyWriter.PropertyName, propertyValue))
                            continue;
                    }

                    Serializer.WritePropertyName(writer, propertyWriter.PropertyName);
                    writer.Write('=');

                    if (strValue != null)
                    {
                        writer.Write(strValue.UrlEncode());
                    }
                    else if (!isEnumerable)
                    {
                        propertyWriter.WriteFn(writer, propertyValue);
                    }
                    else
                    {                        
                        //Trim brackets in top-level lists in QueryStrings, e.g: ?a=[1,2,3] => ?a=1,2,3
                        using (var ms = MemoryStreamFactory.GetStream())
                        {
                            var enumerableWriter = new StreamWriter(ms); //ms disposed in using 
                            propertyWriter.WriteFn(enumerableWriter, propertyValue); 
                            enumerableWriter.Flush();
                            var output = ms.ToArray().FromUtf8Bytes();
                            output = output.Trim(ArrayBrackets);
                            writer.Write(output);
                        }
                    }
                }
            }
            finally
            {
                JsState.QueryStringMode = false;
            }
        }
    }
}
