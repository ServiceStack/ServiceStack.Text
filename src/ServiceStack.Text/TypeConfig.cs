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
		private static readonly TypeConfig _internal;

		public static PropertyInfo[] Properties
		{
			get { return _internal.Properties; }
			set { _internal.Properties = value; }
		}

		public static bool EnableAnonymousFieldSetters
		{
			get { return _internal.EnableAnonymousFieldSetterses; }
			set { _internal.EnableAnonymousFieldSetterses = value; }
		}

		static TypeConfig()
		{
			_internal = new TypeConfig(typeof(T));
			
			var excludedProperties = JsConfig<T>.ExcludePropertyNames ?? new string[0];
			Properties = excludedProperties.Any() 
				? _internal.Type.GetSerializableProperties().Where(x => !excludedProperties.Contains(x.Name)).ToArray()
				: _internal.Type.GetSerializableProperties();
		}

		internal static TypeConfig GetState()
		{
			return _internal;
		}
	}
}