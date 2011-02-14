using System;

namespace ServiceStack.Text
{
	public static class JsConfig
	{
		[ThreadStatic]
		public static bool ConvertObjectTypesIntoStringDictionary = false;
	}
}