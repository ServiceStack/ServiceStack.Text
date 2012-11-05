using System;
using System.Collections.Generic;
using MonoTouch.Foundation;

namespace ServiceStack.Text.Tests
{
	public static class Aot
	{
		public static void Init ()
		{
			JsConfig.RegisterForAot ();

			JsConfig.RegisterTypeForAot<int[]> ();

			JsConfig.RegisterTypeForAot<KeyValuePair<int, int>> ();
			JsConfig.RegisterTypeForAot<Dictionary<int, int>> ();

			JsConfig.RegisterTypeForAot<KeyValuePair<string, string>> ();
			JsConfig.RegisterTypeForAot<Dictionary<string, string>> ();
		}
	}
}

