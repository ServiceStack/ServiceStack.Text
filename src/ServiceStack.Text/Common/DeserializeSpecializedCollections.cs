using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ServiceStack.Text.Common
{
	internal static class DeserializeSpecializedCollections<T, TSerializer>
		where TSerializer : ITypeSerializer
	{
		private readonly static ParseStringDelegate CacheFn;

		static DeserializeSpecializedCollections()
		{
			CacheFn = GetParseFn();
		}

		public static ParseStringDelegate Parse
		{
			get { return CacheFn; }
		}

		public static ParseStringDelegate GetParseFn()
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

			return GetGenericEnumerableParseFn();
		}

		public static Queue<string> ParseStringQueue(string value)
		{
			var parse = (IEnumerable<string>)DeserializeList<List<string>, TSerializer>.Parse(value);
			return new Queue<string>(parse);
		}

		public static Queue<int> ParseIntQueue(string value)
		{
			var parse = (IEnumerable<int>)DeserializeList<List<int>, TSerializer>.Parse(value);
			return new Queue<int>(parse);
		}

		internal static ParseStringDelegate GetGenericQueueParseFn()
		{
			var enumerableInterface = typeof(T).GetTypeWithGenericInterfaceOf(typeof(IEnumerable<>));
			var elementType = enumerableInterface.GetGenericArguments()[0];

			var genericType = typeof(SpecializedQueueElements<>).MakeGenericType(elementType);

			var mi = genericType.GetMethod("ConvertToQueue", BindingFlags.Static | BindingFlags.Public);

			var convertToQueue = (ConvertObjectDelegate)Delegate.CreateDelegate(typeof(ConvertObjectDelegate), mi);

			var parseFn = DeserializeEnumerable<T, TSerializer>.GetParseFn();

			return x => convertToQueue(parseFn(x));
		}

		public static Stack<string> ParseStringStack(string value)
		{
			var parse = (IEnumerable<string>)DeserializeList<List<string>, TSerializer>.Parse(value);
			return new Stack<string>(parse);
		}

		public static Stack<int> ParseIntStack(string value)
		{
			var parse = (IEnumerable<int>)DeserializeList<List<int>, TSerializer>.Parse(value);
			return new Stack<int>(parse);
		}

		internal static ParseStringDelegate GetGenericStackParseFn()
		{
			var enumerableInterface = typeof(T).GetTypeWithGenericInterfaceOf(typeof(IEnumerable<>));
			var elementType = enumerableInterface.GetGenericArguments()[0];

			var genericType = typeof(SpecializedQueueElements<>).MakeGenericType(elementType);

			var mi = genericType.GetMethod("ConvertToStack", BindingFlags.Static | BindingFlags.Public);

			var convertToQueue = (ConvertObjectDelegate)Delegate.CreateDelegate(typeof(ConvertObjectDelegate), mi);

			var parseFn = DeserializeEnumerable<T, TSerializer>.GetParseFn();

			return x => convertToQueue(parseFn(x));
		}

		public static ParseStringDelegate GetGenericEnumerableParseFn()
		{
			var enumerableInterface = typeof(T).GetTypeWithGenericInterfaceOf(typeof(IEnumerable<>));
			var elementType = enumerableInterface.GetGenericArguments()[0];

			var genericType = typeof(SpecializedEnumerableElements<,>).MakeGenericType(typeof(T), elementType);

			var fi = genericType.GetField("ConvertFn", BindingFlags.Static | BindingFlags.Public);

			var convertFn = fi.GetValue(null) as ConvertObjectDelegate;
			if (convertFn == null) return null;

			var parseFn = DeserializeEnumerable<T, TSerializer>.GetParseFn();

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
					ConvertFn = fromObject => {
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