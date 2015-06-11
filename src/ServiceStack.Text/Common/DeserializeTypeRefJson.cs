using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using ServiceStack.Text.Json;

namespace ServiceStack.Text.Common
{
    // Provides a contract for mapping properties to their type accessors
    internal interface IPropertyNameResolver
    {
        TypeAccessor GetTypeAccessorForProperty(string propertyName, Dictionary<string, TypeAccessor> typeAccessorMap);
    }
    // The default behavior is that the target model must match property names exactly
    internal class DefaultPropertyNameResolver : IPropertyNameResolver
    {
        public virtual TypeAccessor GetTypeAccessorForProperty(string propertyName, Dictionary<string, TypeAccessor> typeAccessorMap)
        {
            TypeAccessor typeAccessor;
            typeAccessorMap.TryGetValue(propertyName, out typeAccessor);
            return typeAccessor;
        }
    }
    // The lenient behavior is that properties on the target model can be .NET-cased, while the source JSON can differ
    internal class LenientPropertyNameResolver : DefaultPropertyNameResolver
    {

        public override TypeAccessor GetTypeAccessorForProperty(string propertyName, Dictionary<string, TypeAccessor> typeAccessorMap)
        {
            TypeAccessor typeAccessor;

            // map is case-insensitive by default, so simply remove hyphens and underscores
            return typeAccessorMap.TryGetValue(RemoveSeparators(propertyName), out typeAccessor)
                       ? typeAccessor
                       : base.GetTypeAccessorForProperty(propertyName, typeAccessorMap);
        }

        private static string RemoveSeparators(string propertyName)
        {
            // "lowercase-hyphen" or "lowercase_underscore" -> lowercaseunderscore
            return propertyName.Replace("-", String.Empty).Replace("_", String.Empty);
        }

    }

    internal static class DeserializeTypeRefJson
    {
        private static readonly JsonTypeSerializer Serializer = (JsonTypeSerializer)JsonTypeSerializer.Instance;

        internal static object StringToType(
        TypeConfig typeConfig,
        string strType,
        EmptyCtorDelegate ctorFn,
        Dictionary<string, TypeAccessor> typeAccessorMap)
        {
            var index = 0;
            var type = typeConfig.Type;

            if (strType == null)
                return null;

            //if (!Serializer.EatMapStartChar(strType, ref index))
            for (; index < strType.Length; index++) { var c = strType[index]; if (c >= JsonTypeSerializer.WhiteSpaceFlags.Length || !JsonTypeSerializer.WhiteSpaceFlags[c]) break; } //Whitespace inline
            if (strType[index++] != JsWriter.MapStartChar)
                throw DeserializeTypeRef.CreateSerializationError(type, strType);

            if (JsonTypeSerializer.IsEmptyMap(strType, index)) return ctorFn();

            object instance = null;

            var propertyResolver = JsConfig.PropertyConvention == PropertyConvention.Lenient
                ? ParseUtils.LenientPropertyNameResolver
                : ParseUtils.DefaultPropertyNameResolver;

            var strTypeLength = strType.Length;
            while (index < strTypeLength)
            {
                var propertyName = JsonTypeSerializer.ParseJsonString(strType, ref index);

                //Serializer.EatMapKeySeperator(strType, ref index);
                for (; index < strType.Length; index++) { var c = strType[index]; if (c >= JsonTypeSerializer.WhiteSpaceFlags.Length || !JsonTypeSerializer.WhiteSpaceFlags[c]) break; } //Whitespace inline
                if (strType.Length != index) index++;

                var propertyValueStr = Serializer.EatValue(strType, ref index);
                var possibleTypeInfo = propertyValueStr != null && propertyValueStr.Length > 1;

                //if we already have an instance don't check type info, because then we will have a half deserialized object
                //we could throw here or just use the existing instance.
                if (instance == null && possibleTypeInfo && propertyName == JsWriter.TypeAttr)
                {
                    var explicitTypeName = Serializer.ParseString(propertyValueStr);
                    var explicitType = JsConfig.TypeFinder(explicitTypeName);

                    if (explicitType == null || explicitType.IsInterface() || explicitType.IsAbstract())
                    {
                        Tracer.Instance.WriteWarning("Could not find type: " + propertyValueStr);
                    }
                    else if (!type.IsAssignableFromType(explicitType))
                    {
                        Tracer.Instance.WriteWarning("Could not assign type: " + propertyValueStr);
                    }
                    else
                    {
                        instance = explicitType.CreateInstance();
                    }

                    if (instance != null)
                    {
                        //If __type info doesn't match, ignore it.
                        if (!type.InstanceOfType(instance))
                        {
                            instance = null;
                        }
                        else
                        {
                            var derivedType = instance.GetType();
                            if (derivedType != type)
                            {
                                var derivedTypeConfig = new TypeConfig(derivedType);
                                var map = DeserializeTypeRef.GetTypeAccessorMap(derivedTypeConfig, Serializer);
                                if (map != null)
                                {
                                    typeAccessorMap = map;
                                }
                            }
                        }
                    }

                    Serializer.EatItemSeperatorOrMapEndChar(strType, ref index);
                    continue;
                }

                if (instance == null) instance = ctorFn();

                var typeAccessor = propertyResolver.GetTypeAccessorForProperty(propertyName, typeAccessorMap);

                var propType = possibleTypeInfo && propertyValueStr[0] == '_' ? TypeAccessor.ExtractType(Serializer, propertyValueStr) : null;
                if (propType != null)
                {
                    try
                    {
                        if (typeAccessor != null)
                        {
                            //var parseFn = Serializer.GetParseFn(propType);
                            var parseFn = JsonReader.GetParseFn(propType);

                            var propertyValue = parseFn(propertyValueStr);
                            if (typeConfig.OnDeserializing != null)
                                propertyValue = typeConfig.OnDeserializing(instance, propertyName, propertyValue);
                            typeAccessor.SetProperty(instance, propertyValue);
                        }

                        //Serializer.EatItemSeperatorOrMapEndChar(strType, ref index);
                        for (; index < strType.Length; index++) { var c = strType[index]; if (c >= JsonTypeSerializer.WhiteSpaceFlags.Length || !JsonTypeSerializer.WhiteSpaceFlags[c]) break; } //Whitespace inline
                        if (index != strType.Length)
                        {
                            var success = strType[index] == JsWriter.ItemSeperator || strType[index] == JsWriter.MapEndChar;
                            index++;
                            if (success)
                                for (; index < strType.Length; index++) { var c = strType[index]; if (c >= JsonTypeSerializer.WhiteSpaceFlags.Length || !JsonTypeSerializer.WhiteSpaceFlags[c]) break; } //Whitespace inline
                        }

                        continue;
                    }
                    catch (Exception e)
                    {
                        if (JsConfig.OnDeserializationError != null) JsConfig.OnDeserializationError(instance, propType, propertyName, propertyValueStr, e);
                        if (JsConfig.ThrowOnDeserializationError) throw DeserializeTypeRef.GetSerializationException(propertyName, propertyValueStr, propType, e);
                        else Tracer.Instance.WriteWarning("WARN: failed to set dynamic property {0} with: {1}", propertyName, propertyValueStr);
                    }
                }

                if (typeAccessor != null && typeAccessor.GetProperty != null && typeAccessor.SetProperty != null)
                {
                    try
                    {
                        var propertyValue = typeAccessor.GetProperty(propertyValueStr);
                        if (typeConfig.OnDeserializing != null)
                            propertyValue = typeConfig.OnDeserializing(instance, propertyName, propertyValue);
                        typeAccessor.SetProperty(instance, propertyValue);
                    }
                    catch (Exception e)
                    {
                        if (JsConfig.OnDeserializationError != null) JsConfig.OnDeserializationError(instance, propType ?? typeAccessor.PropertyType, propertyName, propertyValueStr, e);
                        if (JsConfig.ThrowOnDeserializationError) throw DeserializeTypeRef.GetSerializationException(propertyName, propertyValueStr, typeAccessor.PropertyType, e);
                        else Tracer.Instance.WriteWarning("WARN: failed to set property {0} with: {1}", propertyName, propertyValueStr);
                    }
                }
                else if (typeConfig.OnDeserializing != null)
                {
                    // the property is not known by the DTO
                    typeConfig.OnDeserializing(instance, propertyName, propertyValueStr);
                }

                //Serializer.EatItemSeperatorOrMapEndChar(strType, ref index);
                for (; index < strType.Length; index++) { var c = strType[index]; if (c >= JsonTypeSerializer.WhiteSpaceFlags.Length || !JsonTypeSerializer.WhiteSpaceFlags[c]) break; } //Whitespace inline
                if (index != strType.Length)
                {
                    var success = strType[index] == JsWriter.ItemSeperator || strType[index] == JsWriter.MapEndChar;
                    index++;
                    if (success)
                        for (; index < strType.Length; index++) { var c = strType[index]; if (c >= JsonTypeSerializer.WhiteSpaceFlags.Length || !JsonTypeSerializer.WhiteSpaceFlags[c]) break; } //Whitespace inline
                }

            }

            return instance;
        }
    }
}
