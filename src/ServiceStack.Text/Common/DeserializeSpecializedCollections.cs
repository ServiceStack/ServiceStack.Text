using System;
using System.Collections;
using System.Collections.Generic;
#if NETSTANDARD2_0  
using Microsoft.Extensions.Primitives;
#else
using ServiceStack.Text.Support;
#endif

namespace ServiceStack.Text.Common
{
    internal static class DeserializeSpecializedCollections<T, TSerializer>
        where TSerializer : ITypeSerializer
    {
        private readonly static ParseStringSegmentDelegate CacheFn;

        static DeserializeSpecializedCollections()
        {
            CacheFn = GetParseStringSegmentFn();
        }

        public static ParseStringDelegate Parse => v => CacheFn(new StringSegment(v));

        public static ParseStringSegmentDelegate ParseStringSegment => CacheFn;

        public static ParseStringDelegate GetParseFn() => v => GetParseStringSegmentFn()(new StringSegment(v));

        public static ParseStringSegmentDelegate GetParseStringSegmentFn()
        {
            if (typeof(T).HasAnyTypeDefinitionsOf(typeof(Queue<>)))
            {
                if (typeof(T) == typeof(Queue<string>))
                    return ParseStringQueue;

                if (typeof(T) == typeof(Queue<int>))
                    return ParseIntQueue;

                return GetGenericQueueParseFn();
            }

            if (typeof(T).HasAnyTypeDefinitionsOf(typeof(Stack<>)))
            {
                if (typeof(T) == typeof(Stack<string>))
                    return ParseStringStack;

                if (typeof(T) == typeof(Stack<int>))
                    return ParseIntStack;

                return GetGenericStackParseFn();
            }

            var fn = PclExport.Instance.GetSpecializedCollectionParseStringSegmentMethod<TSerializer>(typeof(T));
            if (fn != null)
                return fn;

            if (typeof(T) == typeof(IEnumerable) || typeof(T) == typeof(ICollection))
            {
                return GetEnumerableParseStringSegmentFn();
            }

            return GetGenericEnumerableParseStringSegmentFn();
        }

        public static Queue<string> ParseStringQueue(string value) => ParseStringQueue(new StringSegment(value));

        public static Queue<string> ParseStringQueue(StringSegment value)
        {
            var parse = (IEnumerable<string>)DeserializeList<List<string>, TSerializer>.ParseStringSegment(value);
            return new Queue<string>(parse);
        }

        public static Queue<int> ParseIntQueue(string value) => ParseIntQueue(new StringSegment(value));


        public static Queue<int> ParseIntQueue(StringSegment value)
        {
            var parse = (IEnumerable<int>)DeserializeList<List<int>, TSerializer>.ParseStringSegment(value);
            return new Queue<int>(parse);
        }

        internal static ParseStringSegmentDelegate GetGenericQueueParseFn()
        {
            var enumerableInterface = typeof(T).GetTypeWithGenericInterfaceOf(typeof(IEnumerable<>));
            var elementType = enumerableInterface.GetGenericArguments()[0];
            var genericType = typeof(SpecializedQueueElements<>).MakeGenericType(elementType);
            var mi = genericType.GetStaticMethod("ConvertToQueue");
            var convertToQueue = (ConvertObjectDelegate)mi.MakeDelegate(typeof(ConvertObjectDelegate));

            var parseFn = DeserializeEnumerable<T, TSerializer>.GetParseStringSegmentFn();

            return x => convertToQueue(parseFn(x));
        }

        public static Stack<string> ParseStringStack(string value) => ParseStringStack(new StringSegment(value));

        public static Stack<string> ParseStringStack(StringSegment value)
        {
            var parse = (IEnumerable<string>)DeserializeList<List<string>, TSerializer>.ParseStringSegment(value);
            return new Stack<string>(parse);
        }

        public static Stack<int> ParseIntStack(string value) => ParseIntStack(new StringSegment(value));

        public static Stack<int> ParseIntStack(StringSegment value)
        {
            var parse = (IEnumerable<int>)DeserializeList<List<int>, TSerializer>.ParseStringSegment(value);
            return new Stack<int>(parse);
        }

        internal static ParseStringSegmentDelegate GetGenericStackParseFn()
        {
            var enumerableInterface = typeof(T).GetTypeWithGenericInterfaceOf(typeof(IEnumerable<>));

            var elementType = enumerableInterface.GetGenericArguments()[0];
            var genericType = typeof(SpecializedQueueElements<>).MakeGenericType(elementType);
            var mi = genericType.GetStaticMethod("ConvertToStack");
            var convertToQueue = (ConvertObjectDelegate)mi.MakeDelegate(typeof(ConvertObjectDelegate));

            var parseFn = DeserializeEnumerable<T, TSerializer>.GetParseStringSegmentFn();

            return x => convertToQueue(parseFn(x));
        }

        public static ParseStringDelegate GetEnumerableParseFn() => DeserializeListWithElements<TSerializer>.ParseStringList;

        public static ParseStringSegmentDelegate GetEnumerableParseStringSegmentFn() => DeserializeListWithElements<TSerializer>.ParseStringList;

        public static ParseStringDelegate GetGenericEnumerableParseFn() => v => GetGenericEnumerableParseStringSegmentFn()(new StringSegment(v));

        public static ParseStringSegmentDelegate GetGenericEnumerableParseStringSegmentFn()
        {
            var enumerableInterface = typeof(T).GetTypeWithGenericInterfaceOf(typeof(IEnumerable<>));
            if (enumerableInterface == null) return null;
            var elementType = enumerableInterface.GetGenericArguments()[0];
            var genericType = typeof(SpecializedEnumerableElements<,>).MakeGenericType(typeof(T), elementType);
            var fi = genericType.GetPublicStaticField("ConvertFn");

            var convertFn = fi.GetValue(null) as ConvertObjectDelegate;
            if (convertFn == null) return null;

            var parseFn = DeserializeEnumerable<T, TSerializer>.GetParseStringSegmentFn();

            return x => convertFn(parseFn(x));
        }
    }

    internal class SpecializedQueueElements<T>
    {
        public static Queue<T> ConvertToQueue(object enumerable)
        {
            if (enumerable == null) return null;
            return new Queue<T>((IEnumerable<T>)enumerable);
        }

        public static Stack<T> ConvertToStack(object enumerable)
        {
            if (enumerable == null) return null;
            return new Stack<T>((IEnumerable<T>)enumerable);
        }
    }

    internal class SpecializedEnumerableElements<TCollection, T>
    {
        public static ConvertObjectDelegate ConvertFn;

        static SpecializedEnumerableElements()
        {
            foreach (var ctorInfo in typeof(TCollection).GetConstructors())
            {
                var ctorParams = ctorInfo.GetParameters();
                if (ctorParams.Length != 1) continue;
                var ctorParam = ctorParams[0];

                if (typeof(IEnumerable).IsAssignableFrom(ctorParam.ParameterType)
                    || ctorParam.ParameterType.IsOrHasGenericInterfaceTypeOf(typeof(IEnumerable<>)))
                {
                    ConvertFn = fromObject =>
                    {
                        var to = Activator.CreateInstance(typeof(TCollection), fromObject);
                        return to;
                    };
                    return;
                }
            }

            if (typeof(TCollection).IsOrHasGenericInterfaceTypeOf(typeof(ICollection<>)))
            {
                ConvertFn = ConvertFromCollection;
            }
        }

        public static object Convert(object enumerable)
        {
            return ConvertFn(enumerable);
        }

        public static object ConvertFromCollection(object enumerable)
        {
            var to = (ICollection<T>)typeof(TCollection).CreateInstance();
            var from = (IEnumerable<T>)enumerable;
            foreach (var item in from)
            {
                to.Add(item);
            }
            return to;
        }
    }
}
