using System;
using System.Reflection;
using ServiceStack.Text.Common;

namespace ServiceStack.Text.Jsv
{
	public static class JsvDeserializeType
	{
		public static SetPropertyDelegate GetSetPropertyMethod(Type type, PropertyInfo propertyInfo)
		{
			return ParseUtils.GetSetPropertyMethod(type, propertyInfo);
		}
	}
}