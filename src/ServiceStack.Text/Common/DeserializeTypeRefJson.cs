using System;
using System.Collections.Generic;
using System.Globalization;
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
        private static readonly TextInfo TextInfo = CultureInfo.InvariantCulture.TextInfo;
        public override TypeAccessor GetTypeAccessorForProperty(string propertyName, Dictionary<string, TypeAccessor> typeAccessorMap)
        {
            TypeAccessor typeAccessor;
            
            // camelCase is already supported by default, so no need to add another transform in the tree
            return typeAccessorMap.TryGetValue(TransformFromLowercaseUnderscore(propertyName), out typeAccessor)
                       ? typeAccessor
                       : base.GetTypeAccessorForProperty(propertyName, typeAccessorMap);
        }

        private static string TransformFromLowercaseUnderscore(string propertyName)
        {
            // "lowercase_underscore" -> LowercaseUnderscore
            return TextInfo.ToTitleCase(propertyName).Replace("_", string.Empty);
        }

    }

#if SILVERLIGHT
    public static class TextInfoSilverLightExtension
    {
        public static string ToTitleCase(this TextInfo textInfo, string str)
        {
            string[] words = str.Split('_');

            for (int i = 0; i <= words.Length - 1; i++)
            {
                if ((!object.ReferenceEquals(words[i], string.Empty)))
                {
                    string firstLetter = words[i].Substring(0, 1);
                    string rest = words[i].Substring(1);
                    string result = firstLetter.ToUpper() + rest.ToLower();
                    words[i] = result;
                }
            }
            return String.Join("_", words);
        }
    }
#endif

	internal static class DeserializeTypeRefJson
	{
	    public static readonly IPropertyNameResolver DefaultPropertyNameResolver = new DefaultPropertyNameResolver();
        public static readonly IPropertyNameResolver LenientPropertyNameResolver = new LenientPropertyNameResolver();
        public static IPropertyNameResolver PropertyNameResolver = DefaultPropertyNameResolver;

        private static readonly JsonTypeSerializer Serializer = (JsonTypeSerializer)JsonTypeSerializer.Instance;

		internal static object StringToType(
		Type type,
		string strType,
		EmptyCtorDelegate ctorFn,
		Dictionary<string, TypeAccessor> typeAccessorMap)
		{
			var index = 0;

			if (strType == null)
				return null;

			//if (!Serializer.EatMapStartChar(strType, ref index))
			for (; index < strType.Length; index++) { var c = strType[index]; if (c >= JsonTypeSerializer.WhiteSpaceFlags.Length || !JsonTypeSerializer.WhiteSpaceFlags[c]) break; } //Whitespace inline
			if (strType[index++] != JsWriter.MapStartChar)
				throw DeserializeTypeRef.CreateSerializationError(type, strType);

            if (JsonTypeSerializer.IsEmptyMap(strType)) return ctorFn();

			object instance = null;

			var strTypeLength = strType.Length;
			while (index < strTypeLength)
			{
				var propertyName = JsonTypeSerializer.ParseJsonString(strType, ref index);

				//Serializer.EatMapKeySeperator(strType, ref index);
				for (; index < strType.Length; index++) { var c = strType[index]; if (c >= JsonTypeSerializer.WhiteSpaceFlags.Length || !JsonTypeSerializer.WhiteSpaceFlags[c]) break; } //Whitespace inline
				if (strType.Length != index) index++;

				var propertyValueStr = Serializer.EatValue(strType, ref index);
				var possibleTypeInfo = propertyValueStr != null && propertyValueStr.Length > 1;

				if (possibleTypeInfo && propertyName == JsWriter.TypeAttr)
				{
					var explicitTypeName = Serializer.ParseString(propertyValueStr);
                    var explicitType = Type.GetType(explicitTypeName);
                    if (explicitType != null && !explicitType.IsInterface && !explicitType.IsAbstract) {
                        instance = explicitType.CreateInstance();
                    }

					if (instance == null)
					{
						Tracer.Instance.WriteWarning("Could not find type: " + propertyValueStr);
					}
					else
					{
						//If __type info doesn't match, ignore it.
						if (!type.IsInstanceOfType(instance)) {
						    instance = null;
						} else {
						    var derivedType = instance.GetType();
                            if (derivedType != type) {
						        var derivedTypeConfig = new TypeConfig(derivedType);
						        var map = DeserializeTypeRef.GetTypeAccessorMap(derivedTypeConfig, Serializer);
                                if (map != null) {
                                    typeAccessorMap = map;
                                }
                            }
						}
					}

					Serializer.EatItemSeperatorOrMapEndChar(strType, ref index);
					continue;
				}

				if (instance == null) instance = ctorFn();

                var typeAccessor = PropertyNameResolver.GetTypeAccessorForProperty(propertyName, typeAccessorMap);
                
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
					catch(Exception e)
					{
                        if (JsConfig.ThrowOnDeserializationError) throw DeserializeTypeRef.GetSerializationException(propertyName, propertyValueStr, propType, e);
						else Tracer.Instance.WriteWarning("WARN: failed to set dynamic property {0} with: {1}", propertyName, propertyValueStr);
					}
				}

				if (typeAccessor != null && typeAccessor.GetProperty != null && typeAccessor.SetProperty != null)
				{
					try
					{
						var propertyValue = typeAccessor.GetProperty(propertyValueStr);
						typeAccessor.SetProperty(instance, propertyValue);
					}
					catch(Exception e)
					{
                        if (JsConfig.ThrowOnDeserializationError) throw DeserializeTypeRef.GetSerializationException(propertyName, propertyValueStr, typeAccessor.PropertyType, e);
                        else Tracer.Instance.WriteWarning("WARN: failed to set property {0} with: {1}", propertyName, propertyValueStr);
					}
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