using System;
using System.Collections.Generic;
using MonoTouch.Foundation;

namespace ServiceStack.Text.Tests
{
	public class Aot
	{
		public static void Init ()
		{
			JsConfig.RegisterForAot ();
			JsConfig.RegisterTypeForAot<int[]> ();
			JsConfig.RegisterTypeForAot<Dictionary<int, int>> ();
			JsConfig.RegisterTypeForAot<Dictionary<string, string>> ();
		}
	}
}

