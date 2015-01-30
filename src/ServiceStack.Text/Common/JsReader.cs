using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ServiceStack.Text.Common
{
    public class JsReader<TSerializer>
        where TSerializer : ITypeSerializer
    {
        private static readonly ITypeSerializer Serializer = JsWriter.GetTypeSerializer<TSerializer>();

        public ParseStringDelegate GetParseFn<T>()
        {
            var onDeserializedFn = JsConfig<T>.OnDeserializedFn;
            if (onDeserializedFn != null)
            {
                var parseFn = GetCoreParseFn<T>();
                return value => onDeserializedFn((T)parseFn(value));
            }

            return GetCoreParseFn<T>();
        }

        private ParseStringDelegate GetCoreParseFn<T>()
        {
            var type = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            if (JsConfig<T>.HasDeserializeFn)
                return value => JsConfig<T>.ParseFn(Serializer, value);

            if (type.IsEnum())
                return x => ParseUtils.TryParseEnum(type, Serializer.UnescapeSafeString(x));

            if (type == typeof(string))
                return Serializer.UnescapeString;

            if (type == typeof(object))
                return DeserializeType<TSerializer>.ObjectStringToType;

            var specialParseFn = ParseUtils.GetSpecialParseMethod(type);
            if (specialParseFn != null)
                return specialParseFn;

            if (type.IsArray)
            {
                return DeserializeArray<T, TSerializer>.Parse;
            }

            var builtInMethod = DeserializeBuiltin<T>.Parse;
            if (builtInMethod != null)
                return value => builtInMethod(Serializer.UnescapeSafeString(value));

            if (type.HasGenericType())
            {
                if (type.IsOrHasGenericInterfaceTypeOf(typeof(IList<>)))
                    return DeserializeList<T, TSerializer>.Parse;

                if (type.IsOrHasGenericInterfaceTypeOf(typeof(IDictionary<,>)))
                    return DeserializeDictionary<TSerializer>.GetParseMethod(type);

                if (type.IsOrHasGenericInterfaceTypeOf(typeof(ICollection<>)))
                    return DeserializeCollection<TSerializer>.GetParseMethod(type);

                if (type.HasAnyTypeDefinitionsOf(typeof(Queue<>))
                    || type.HasAnyTypeDefinitionsOf(typeof(Stack<>)))
                    return DeserializeSpecializedCollections<T, TSerializer>.Parse;

                if (type.IsOrHasGenericInterfaceTypeOf(typeof(KeyValuePair<,>)))
                    return DeserializeKeyValuePair<TSerializer>.GetParseMethod(type);

                if (type.IsOrHasGenericInterfaceTypeOf(typeof(IEnumerable<>)))
                    return DeserializeEnumerable<T, TSerializer>.Parse;

                var customFn = DeserializeCustomGenericType<TSerializer>.GetParseMethod(type);
                if (customFn != null)
                    return customFn;
            }

			var pclParseFn = PclExport.Instance.GetJsReaderParseMethod<TSerializer>(typeof(T));
			if (pclParseFn != null)
				return pclParseFn;

            var isDictionary = typeof(T) != typeof(IEnumerable) && typeof(T) != typeof(ICollection)
                && (typeof(T).AssignableFrom(typeof(IDictionary)) || typeof(T).HasInterface(typeof(IDictionary)));
            if (isDictionary)
            {
                return DeserializeDictionary<TSerializer>.GetParseMethod(type);
            }

            var isEnumerable = typeof(T).AssignableFrom(typeof(IEnumerable))
                || typeof(T).HasInterface(typeof(IEnumerable));
            if (isEnumerable)
            {
                var parseFn = DeserializeSpecializedCollections<T, TSerializer>.Parse;
                if (parseFn != null) return parseFn;
            }

            if (type.IsValueType())
            {
                var staticParseMethod = StaticParseMethod<T>.Parse;
                if (staticParseMethod != null)
                    return value => staticParseMethod(Serializer.UnescapeSafeString(value));
            }
            else
            {
                var staticParseMethod = StaticParseRefTypeMethod<TSerializer, T>.Parse;
                if (staticParseMethod != null)
                    return value => staticParseMethod(Serializer.UnescapeSafeString(value));
            }

            var typeConstructor = DeserializeType<TSerializer>.GetParseMethod(TypeConfig<T>.GetState());
            if (typeConstructor != null)
                return typeConstructor;

            var stringConstructor = DeserializeTypeUtils.GetParseMethod(type);
            if (stringConstructor != null) return stringConstructor;

            return DeserializeType<TSerializer>.ParseAbstractType<T>;
        }

    }
}
