using System;
using System.Linq;
using System.Reflection;

namespace ServiceStack.Text
{
	internal class TypeConfig
	{
		internal readonly Type Type;
		internal bool EnableAnonymousFieldSetterses;
		internal PropertyInfo[] Properties;

		internal TypeConfig(Type type)
		{
			Type = type;
			EnableAnonymousFieldSetterses = false;
			Properties = new PropertyInfo[0];
		}
	}

	public static class TypeConfig<T>
	{
		private static readonly TypeConfig config;

		public static PropertyInfo[] Properties
		{
			get { return config.Properties; }
			set { config.Properties = value; }
		}

		public static bool EnableAnonymousFieldSetters
		{
			get { return config.EnableAnonymousFieldSetterses; }
			set { config.EnableAnonymousFieldSetterses = value; }
		}

		static TypeConfig()
		{
			config = new TypeConfig(typeof(T));
			
			var excludedProperties = JsConfig<T>.ExcludePropertyNames ?? new string[0];
			var properties = excludedProperties.Any() 
				? config.Type.GetSerializableProperties().Where(x => !excludedProperties.Contains(x.Name))
				: config.Type.GetSerializableProperties();
			Properties = properties.Where(x => x.GetIndexParameters().Length == 0).ToArray();
		}

		internal static TypeConfig GetState()
		{
			return config;
		}
	}
}