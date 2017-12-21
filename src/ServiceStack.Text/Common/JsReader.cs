using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
#if NETSTANDARD2_0
using Microsoft.Extensions.Primitives;
#else
using ServiceStack.Text.Support;
#endif

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

        public ParseStringSegmentDelegate GetParseStringSegmentFn<T>()
        {
            var onDeserializedFn = JsConfig<T>.OnDeserializedFn;
            if (onDeserializedFn != null)
            {
                var parseFn = GetCoreParseStringSegmentFn<T>();
                return value => onDeserializedFn((T)parseFn(value));
            }

            return GetCoreParseStringSegmentFn<T>();
        }

        private ParseStringDelegate GetCoreParseFn<T>()
        {
            return v => GetCoreParseStringSegmentFn<T>()(new StringSegment(v));
        }

        private ParseStringSegmentDelegate GetCoreParseStringSegmentFn<T>()
        {
            var type = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            if (JsConfig<T>.HasDeserializeFn)
                return value => JsConfig<T>.ParseFn(Serializer, value.Value);

            if (type.IsEnum)
                return x => ParseUtils.TryParseEnum(type, Serializer.UnescapeSafeString(x).Value);

            if (type == typeof(string))
                return v => Serializer.UnescapeString(v).Value;

            if (type == typeof(object))
                return DeserializeType<TSerializer>.ObjectStringToType;

            var specialParseFn = ParseUtils.GetSpecialParseMethod(type);
            if (specialParseFn != null)
                return v => specialParseFn(v.Value);

            if (type.IsArray)
            {
                return DeserializeArray<T, TSerializer>.ParseStringSegment;
            }

            var builtInMethod = DeserializeBuiltin<T>.ParseStringSegment;
            if (builtInMethod != null)
                return value => builtInMethod(Serializer.UnescapeSafeString(value));

            if (type.HasGenericType())
            {
                if (type.IsOrHasGenericInterfaceTypeOf(typeof(IList<>)))
                    return DeserializeList<T, TSerializer>.ParseStringSegment;

                if (type.IsOrHasGenericInterfaceTypeOf(typeof(IDictionary<,>)))
                    return DeserializeDictionary<TSerializer>.GetParseStringSegmentMethod(type);

                if (type.IsOrHasGenericInterfaceTypeOf(typeof(ICollection<>)))
                    return DeserializeCollection<TSerializer>.GetParseStringSegmentMethod(type);

                if (type.HasAnyTypeDefinitionsOf(typeof(Queue<>))
                    || type.HasAnyTypeDefinitionsOf(typeof(Stack<>)))
                    return DeserializeSpecializedCollections<T, TSerializer>.ParseStringSegment;

                if (type.IsOrHasGenericInterfaceTypeOf(typeof(KeyValuePair<,>)))
                    return DeserializeKeyValuePair<TSerializer>.GetParseStringSegmentMethod(type);

                if (type.IsOrHasGenericInterfaceTypeOf(typeof(IEnumerable<>)))
                    return DeserializeEnumerable<T, TSerializer>.ParseStringSegment;

                var customFn = DeserializeCustomGenericType<TSerializer>.GetParseStringSegmentMethod(type);
                if (customFn != null)
                    return customFn;
            }

            var pclParseFn = PclExport.Instance.GetJsReaderParseStringSegmentMethod<TSerializer>(typeof(T));
            if (pclParseFn != null)
                return pclParseFn;

            var isDictionary = typeof(T) != typeof(IEnumerable) && typeof(T) != typeof(ICollection)
                && (typeof(T).IsAssignableFrom(typeof(IDictionary)) || typeof(T).HasInterface(typeof(IDictionary)));
            if (isDictionary)
            {
                return DeserializeDictionary<TSerializer>.GetParseStringSegmentMethod(type);
            }

            var isEnumerable = typeof(T).IsAssignableFrom(typeof(IEnumerable))
                || typeof(T).HasInterface(typeof(IEnumerable));
            if (isEnumerable)
            {
                var parseFn = DeserializeSpecializedCollections<T, TSerializer>.ParseStringSegment;
                if (parseFn != null) return parseFn;
            }

            if (type.IsValueType)
            {
                //at first try to find more faster `ParseStringSegment` method
                var staticParseStringSegmentMethod = StaticParseMethod<T>.ParseStringSegment;
                if (staticParseStringSegmentMethod != null)
                    return value => staticParseStringSegmentMethod(Serializer.UnescapeSafeString(value));
                
                //then try to find `Parse` method
                var staticParseMethod = StaticParseMethod<T>.Parse;
                if (staticParseMethod != null)
                    return value => staticParseMethod(Serializer.UnescapeSafeString(value).Value);
            }
            else
            {
                var staticParseStringSegmentMethod = StaticParseRefTypeMethod<TSerializer, T>.ParseStringSegment;
                if (staticParseStringSegmentMethod != null)
                    return value => staticParseStringSegmentMethod(Serializer.UnescapeSafeString(value));

                var staticParseMethod = StaticParseRefTypeMethod<TSerializer, T>.Parse;
                if (staticParseMethod != null)
                    return value => staticParseMethod(Serializer.UnescapeSafeString(value).Value);
            }

            var typeConstructor = DeserializeType<TSerializer>.GetParseStringSegmentMethod(TypeConfig<T>.GetState());
            if (typeConstructor != null)
                return typeConstructor;

            var stringConstructor = DeserializeTypeUtils.GetParseStringSegmentMethod(type);
            if (stringConstructor != null) return stringConstructor;

            return DeserializeType<TSerializer>.ParseAbstractType<T>;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void InitAot<T>()
        {
            var hold = DeserializeBuiltin<T>.Parse;
            hold = DeserializeArray<T[], TSerializer>.Parse;
            DeserializeType<TSerializer>.ExtractType(null);
            DeserializeArrayWithElements<T, TSerializer>.ParseGenericArray(null, null);
            DeserializeCollection<TSerializer>.ParseCollection<T>(null, null, null);
            DeserializeListWithElements<T, TSerializer>.ParseGenericList(null, null, null);
        }
    }
}
