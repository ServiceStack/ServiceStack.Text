using System;
using System.Collections;
using System.Collections.Generic;

namespace ServiceStack.Text.Common
{
	internal class JsReader<TSerializer>
		where TSerializer : ITypeSerializer
	{
		private static readonly ITypeSerializer Serializer = JsWriter.GetTypeSerializer<TSerializer>();

		public ParseStringDelegate GetParseFn<T>()
		{
			var type = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

			if (type.IsEnum)
			{
				return x => Enum.Parse(type, x, true);
			}

			if (type == typeof(string))
				return Serializer.ParseString;

			if (type == typeof(object))
				return DeserializeType<TSerializer>.ObjectStringToType;

			var specialParseFn = ParseUtils.GetSpecialParseMethod(type);
			if (specialParseFn != null)
				return specialParseFn;

			if (type.IsEnum)
				return x => Enum.Parse(type, x, true);

			if (type.IsArray)
			{
				return DeserializeArray<T, TSerializer>.Parse;
			}

			var builtInMethod = DeserializeBuiltin<T>.Parse;
			if (builtInMethod != null)
				return value => builtInMethod(Serializer.ParseRawString(value));

			if (JsConfig<T>.SerializeFn != null)
				return value => JsConfig<T>.ParseFn(Serializer.ParseRawString(value));

			if (type.IsGenericType())
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

				if (type.IsOrHasGenericInterfaceTypeOf(typeof(IEnumerable<>)))
					return DeserializeEnumerable<T, TSerializer>.Parse;
			}

			var isCollection = typeof(T).IsOrHasGenericInterfaceTypeOf(typeof(ICollection));
			if (isCollection)
			{
				var isDictionary = typeof(T).IsAssignableFrom(typeof(IDictionary))
					|| typeof(T).HasInterface(typeof(IDictionary));
				if (isDictionary)
				{
					return DeserializeDictionary<TSerializer>.GetParseMethod(type);
				}

				return DeserializeEnumerable<T, TSerializer>.Parse;
			}

			var isEnumerable = typeof(T).IsAssignableFrom(typeof(IEnumerable))
				|| typeof(T).HasInterface(typeof(IEnumerable));

			if (isEnumerable)
			{
				var parseFn = DeserializeSpecializedCollections<T, TSerializer>.Parse;
				if (parseFn != null) return parseFn;
			}

			if (type.IsValueType) {
				var staticParseMethod = StaticParseMethod<T>.Parse;
				if (staticParseMethod != null)
					return value => staticParseMethod(Serializer.ParseRawString(value));
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