using System.Linq;
using System.Reflection;

namespace ServiceStack.Text
{
	public static class TypeConfig<T>
	{
		public static PropertyInfo[] Properties = new PropertyInfo[0];

		static TypeConfig()
		{
			var excludedProperties = JsConfig<T>.ExcludePropertyNames ?? new string[0];
			Properties = typeof(T).GetSerializableProperties()
				.Where(x => !excludedProperties.Contains(x.Name)).ToArray();
		}
	}
}