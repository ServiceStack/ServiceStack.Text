using System;
using System.Collections.Generic;
using ServiceStack.Text.Json;
using ServiceStack.Text.Jsv;

namespace ServiceStack.Text.Common
{
    internal static class DeserializeTypeRefJsv
    {
        private static readonly JsvTypeSerializer Serializer = (JsvTypeSerializer)JsvTypeSerializer.Instance;

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
            if (strType[index++] != JsWriter.MapStartChar)
                throw DeserializeTypeRef.CreateSerializationError(type, strType);

            if (JsonTypeSerializer.IsEmptyMap(strType)) return ctorFn();

            object instance = null;

            var propertyResolver = JsConfig.PropertyConvention == PropertyConvention.Lenient
                ? ParseUtils.LenientPropertyNameResolver
                : ParseUtils.DefaultPropertyNameResolver;

            var strTypeLength = strType.Length;
            while (index < strTypeLength)
            {
                var propertyName = Serializer.EatMapKey(strType, ref index);

                //Serializer.EatMapKeySeperator(strType, ref index);
                index++;

                var propertyValueStr = Serializer.EatValue(strType, ref index);
                var possibleTypeInfo = propertyValueStr != null && propertyValueStr.Length > 1;

                if (possibleTypeInfo && propertyName == JsWriter.TypeAttr)
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

                    //Serializer.EatItemSeperatorOrMapEndChar(strType, ref index);
                    if (index != strType.Length) index++;

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
                            var parseFn = Serializer.GetParseFn(propType);
                            var propertyValue = parseFn(propertyValueStr);
                            if (typeConfig.OnDeserializing != null)
                                propertyValue = typeConfig.OnDeserializing(instance, propertyName, propertyValue);
                            typeAccessor.SetProperty(instance, propertyValue);
                        }

                        //Serializer.EatItemSeperatorOrMapEndChar(strType, ref index);
                        if (index != strType.Length) index++;

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
                        if (JsConfig.ThrowOnDeserializationError) throw DeserializeTypeRef.GetSerializationException(propertyName, propertyValueStr, propType, e);
                        else Tracer.Instance.WriteWarning("WARN: failed to set property {0} with: {1}", propertyName, propertyValueStr);
                    }
                }
                else if (typeConfig.OnDeserializing != null)
                {
                    // the property is not known by the DTO
                    typeConfig.OnDeserializing(instance, propertyName, propertyValueStr);
                }

                //Serializer.EatItemSeperatorOrMapEndChar(strType, ref index);
                if (index != strType.Length) index++;
            }

            return instance;
        }
    }

    //The same class above but JSON-specific to enable inlining in this hot class.
}