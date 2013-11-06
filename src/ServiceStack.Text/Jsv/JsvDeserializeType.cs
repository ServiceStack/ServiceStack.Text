//Copyright (c) Service Stack LLC. All Rights Reserved.
//License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

using System;
using System.Reflection;
using ServiceStack.Text.Common;

namespace ServiceStack.Text.Jsv
{
	public static class JsvDeserializeType
	{
		public static SetPropertyDelegate GetSetPropertyMethod(Type type, PropertyInfo propertyInfo)
		{
			return TypeAccessor.GetSetPropertyMethod(type, propertyInfo);
		}

		public static SetPropertyDelegate GetSetFieldMethod(Type type, FieldInfo fieldInfo)
		{
			return TypeAccessor.GetSetFieldMethod(type, fieldInfo);
		}
	}
}