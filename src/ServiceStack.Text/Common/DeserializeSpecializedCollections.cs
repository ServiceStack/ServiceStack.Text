using System;
using System.Collections.Generic;
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

			if (typeof(T).HasAnyTypeDefinitionsOf(typeof(ICollection<>)))
			{
				return GetGenericCollectionParseFn();
			}

			return null;
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

		internal static ParseStringDelegate GetGenericCollectionParseFn()
		{
			var enumerableInterface = typeof(T).GetTypeWithGenericInterfaceOf(typeof(IEnumerable<>));
			var elementType = enumerableInterface.GetGenericArguments()[0];

			var genericType = typeof(SpecializedCollectionElements<,>).MakeGenericType(typeof(T), elementType);

			var mi = genericType.GetMethod("Convert", BindingFlags.Static | BindingFlags.Public);

			var convertToCollection = (ConvertObjectDelegate)Delegate.CreateDelegate(typeof(ConvertObjectDelegate), mi);

			var parseFn = DeserializeEnumerable<T, TSerializer>.GetParseFn();

			return x => convertToCollection(parseFn(x));
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

	internal class SpecializedCollectionElements<TCollection, T>
	{
		public static object Convert(object enumerable)
		{
			var to = (ICollection<T>)typeof(TCollection).CreateInstance();
			var from = (IEnumerable<T>) enumerable;
			foreach (var item in from)
			{
				to.Add(item);
			}
			return to;
		}
	}
}