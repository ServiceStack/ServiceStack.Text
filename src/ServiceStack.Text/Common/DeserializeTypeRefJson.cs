using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using ServiceStack.Text.Json;
#if NETSTANDARD2_0
using Microsoft.Extensions.Primitives;
#endif
using ServiceStack.Text.Support;

namespace ServiceStack.Text.Common
{
    // Provides a contract for mapping properties to their type accessors
    internal interface IPropertyNameResolver
    {
        TypeAccessor GetTypeAccessorForProperty(StringSegment propertyName, Dictionary<HashedStringSegment, TypeAccessor> typeAccessorMap);
    }
    // The default behavior is that the target model must match property names exactly
    internal class DefaultPropertyNameResolver : IPropertyNameResolver
    {
        public virtual TypeAccessor GetTypeAccessorForProperty(StringSegment propertyName, Dictionary<HashedStringSegment, TypeAccessor> typeAccessorMap)
        {
            TypeAccessor typeAccessor;
            typeAccessorMap.TryGetValue(new HashedStringSegment(propertyName), out typeAccessor);
            return typeAccessor;
        }
    }
    // The lenient behavior is that properties on the target model can be .NET-cased, while the source JSON can differ
    internal class LenientPropertyNameResolver : DefaultPropertyNameResolver
    {

        public override TypeAccessor GetTypeAccessorForProperty(StringSegment propertyName, Dictionary<HashedStringSegment, TypeAccessor> typeAccessorMap)
        {
            TypeAccessor typeAccessor;

            // map is case-insensitive by default, so simply remove hyphens and underscores
            return typeAccessorMap.TryGetValue(new HashedStringSegment(RemoveSeparators(propertyName)), out typeAccessor)
                       ? typeAccessor
                       : base.GetTypeAccessorForProperty(propertyName, typeAccessorMap);
        }

        //TODO: optimize
        private static string RemoveSeparators(StringSegment propertyName)
        {
            // "lowercase-hyphen" or "lowercase_underscore" -> lowercaseunderscore
            return propertyName.Value.Replace("-", String.Empty).Replace("_", String.Empty);
        }

    }

    internal static class DeserializeTypeRefJson
    {
        private static readonly JsonTypeSerializer Serializer = (JsonTypeSerializer)JsonTypeSerializer.Instance;

        internal static object StringToType(
            TypeConfig typeConfig,
            string strType,
            EmptyCtorDelegate ctorFn,
            Dictionary<HashedStringSegment, TypeAccessor> typeAccessorMap) =>
            StringToType(typeConfig, new StringSegment(strType), ctorFn, typeAccessorMap);

        static readonly StringSegment typeAttr = new StringSegment(JsWriter.TypeAttr);

        internal static object StringToType(
        TypeConfig typeConfig,
        StringSegment strType,
        EmptyCtorDelegate ctorFn,
        Dictionary<HashedStringSegment, TypeAccessor> typeAccessorMap)
        {
            var index = 0;
            var type = typeConfig.Type;

            if (!strType.HasValue)
                return null;

            var buffer = strType.Buffer;
            var offset = strType.Offset;
            var strTypeLength = strType.Length;

            //if (!Serializer.EatMapStartChar(strType, ref index))
            for (; index < strTypeLength; index++) { if (!JsonUtils.IsWhiteSpace(buffer[offset + index])) break; } //Whitespace inline
            if (buffer[offset + index] != JsWriter.MapStartChar)
                throw DeserializeTypeRef.CreateSerializationError(type, strType.Value);

            index++;
            if (JsonTypeSerializer.IsEmptyMap(strType, index)) return ctorFn();

            object instance = null;

            var propertyResolver = JsConfig.PropertyConvention == PropertyConvention.Lenient
                ? ParseUtils.LenientPropertyNameResolver
                : ParseUtils.DefaultPropertyNameResolver;

            while (index < strTypeLength)
            {
                var propertyName = JsonTypeSerializer.ParseJsonString(strType, ref index);

                //Serializer.EatMapKeySeperator(strType, ref index);
                for (; index < strTypeLength; index++) { if (!JsonUtils.IsWhiteSpace(buffer[offset + index])) break; } //Whitespace inline
                if (strTypeLength != index) index++;

                var propertyValueStr = Serializer.EatValue(strType, ref index);
                var possibleTypeInfo = propertyValueStr != null && propertyValueStr.Length > 1;

                //if we already have an instance don't check type info, because then we will have a half deserialized object
                //we could throw here or just use the existing instance.
                if (instance == null && possibleTypeInfo && propertyName == typeAttr)
                {
                    var explicitTypeName = Serializer.ParseString(propertyValueStr);
                    var explicitType = JsConfig.TypeFinder(explicitTypeName);

                    if (explicitType == null || explicitType.IsInterface || explicitType.IsAbstract)
                    {
                        Tracer.Instance.WriteWarning("Could not find type: " + propertyValueStr);
                    }
                    else if (!type.IsAssignableFrom(explicitType))
                    {
                        Tracer.Instance.WriteWarning("Could not assign type: " + propertyValueStr);
                    }
                    else
                    {
                        JsWriter.AssertAllowedRuntimeType(explicitType);
                        instance = explicitType.CreateInstance();
                    }

                    if (instance != null)
                    {
                        //If __type info doesn't match, ignore it.
                        if (!type.IsInstanceOfType(instance))
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

                var propType = possibleTypeInfo && propertyValueStr.GetChar(0) == '_' ? TypeAccessor.ExtractType(Serializer, propertyValueStr) : null;
                if (propType != null)
                {
                    try
                    {
                        if (typeAccessor != null)
                        {
                            //var parseFn = Serializer.GetParseFn(propType);
                            var parseFn = JsonReader.GetParseStringSegmentFn(propType);

                            var propertyValue = parseFn(propertyValueStr);
                            if (typeConfig.OnDeserializing != null)
                                propertyValue = typeConfig.OnDeserializing(instance, propertyName.Value, propertyValue);
                            typeAccessor.SetProperty(instance, propertyValue);
                        }

                        //Serializer.EatItemSeperatorOrMapEndChar(strType, ref index);
                        for (; index < strTypeLength; index++) { if (!JsonUtils.IsWhiteSpace(buffer[offset + index])) break; } //Whitespace inline
                        if (index != strTypeLength)
                        {
                            var success = buffer[offset + index] == JsWriter.ItemSeperator || buffer[offset + index] == JsWriter.MapEndChar;
                            index++;
                            if (success)
                                for (; index < strTypeLength; index++) { if (!JsonUtils.IsWhiteSpace(buffer[offset + index])) break; } //Whitespace inline
                        }

                        continue;
                    }
                    catch (Exception e)
                    {
                        if (JsConfig.OnDeserializationError != null) JsConfig.OnDeserializationError(instance, propType, propertyName.Value, propertyValueStr.Value, e);
                        if (JsConfig.ThrowOnDeserializationError) throw DeserializeTypeRef.GetSerializationException(propertyName.Value, propertyValueStr.Value, propType, e);
                        else Tracer.Instance.WriteWarning("WARN: failed to set dynamic property {0} with: {1}", propertyName, propertyValueStr.Value);
                    }
                }

                if (typeAccessor?.GetProperty != null && typeAccessor.SetProperty != null)
                {
                    try
                    {
                        var propertyValue = typeAccessor.GetProperty(propertyValueStr);
                        if (typeConfig.OnDeserializing != null)
                            propertyValue = typeConfig.OnDeserializing(instance, propertyName.Value, propertyValue);
                        typeAccessor.SetProperty(instance, propertyValue);
                    }
                    catch (NotSupportedException) { throw; }
                    catch (Exception e)
                    {
                        if (JsConfig.OnDeserializationError != null) JsConfig.OnDeserializationError(instance, propType ?? typeAccessor.PropertyType, propertyName.Value, propertyValueStr.Value, e);
                        if (JsConfig.ThrowOnDeserializationError) throw DeserializeTypeRef.GetSerializationException(propertyName.Value, propertyValueStr.Value, typeAccessor.PropertyType, e);
                        else Tracer.Instance.WriteWarning("WARN: failed to set property {0} with: {1}", propertyName, propertyValueStr.Value);
                    }
                }
                else
                {
                    // the property is not known by the DTO
                    typeConfig.OnDeserializing?.Invoke(instance, propertyName.Value, propertyValueStr.Value);
                }

                //Serializer.EatItemSeperatorOrMapEndChar(strType, ref index);
                for (; index < strTypeLength; index++) { if (!JsonUtils.IsWhiteSpace(buffer[offset + index])) break; } //Whitespace inline
                if (index != strType.Length)
                {
                    var success = buffer[offset + index] == JsWriter.ItemSeperator || buffer[offset + index] == JsWriter.MapEndChar;
                    index++;
                    if (success)
                        for (; index < strTypeLength; index++) { if (!JsonUtils.IsWhiteSpace(buffer[offset + index])) break; } //Whitespace inline
                }

            }

            return instance;
        }
    }
}
