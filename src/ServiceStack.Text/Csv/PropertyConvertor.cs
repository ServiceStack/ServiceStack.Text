using System;
using System.Reflection;

namespace ServiceStack.Text.Csv
{
	public static class PropertyConvertor
	{
		public static void SetValue<T>(object item, PropertyInfo property, T value)
		{
			var methodInfo = property.GetSetMethod();

			if (property.PropertyType == typeof(Boolean))
			{
				methodInfo.Invoke(item, new object[] { Convert.ToBoolean(value) });
			}
			else if (property.PropertyType == typeof(int))
			{
				methodInfo.Invoke(item, new object[] { Convert.ToInt32(value) });
			}
			else if (property.PropertyType == typeof(short))
			{
				methodInfo.Invoke(item, new object[] { Convert.ToInt16(value) });
			}
			else if (property.PropertyType == typeof(long))
			{
				methodInfo.Invoke(item, new object[] { Convert.ToInt64(value) });
			}
			else if (property.PropertyType == typeof(DateTime))
			{
				methodInfo.Invoke(item, new object[] { Convert.ToDateTime(value) });
			}
			else
			{
				methodInfo.Invoke(item, new object[] { value });
			}
		}

		public static PropertyInfo GetProperty<T>(string propertyName)
		{
			var type = typeof(T);
			var propertyInfo = type.GetProperty(propertyName)
			                   ?? type.GetProperty(propertyName.UppercaseFirst());

			if (propertyInfo == null)
				throw new CsvDeserializationException(String.Format("PropertyName \"{0}\" is not a property of type {1}", propertyName, type));

			return propertyInfo;
		}
	}
}