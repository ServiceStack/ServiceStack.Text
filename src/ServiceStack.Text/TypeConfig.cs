using System.Linq;
using System.Reflection;

namespace ServiceStack.Text
{
	public static class TypeConfig<T>
	{
		public static PropertyInfo[] Properties = new PropertyInfo[0];

		static TypeConfig()
		{
			Properties = typeof(T).GetSerializableProperties();
		}
	}
}