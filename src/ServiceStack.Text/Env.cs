
using System;

namespace ServiceStack.Text
{
	public static class Env
	{
		static Env()
		{
#if NETFX_CORE
            IsUnix = false;
#else
            var platform = (int)Environment.OSVersion.Platform;
			IsUnix = (platform == 4) || (platform == 6) || (platform == 128);
#endif

            IsMono = AssemblyUtils.FindType("Mono.Runtime") != null;

            IsMonoTouch = AssemblyUtils.FindType("MonoTouch.Foundation.NSObject") != null;

            IsWinRT = AssemblyUtils.FindType("Windows.ApplicationModel") != null;

			SupportsExpressions = SupportsEmit = !IsMonoTouch;

            ServerUserAgent = "ServiceStack/" +
                ServiceStackVersion + " "
#if NETFX_CORE
                + "Microsoft Windows Store App"
#else
                + Environment.OSVersion.Platform
#endif
                + (IsMono ? "/Mono" : "/.NET")
                + (IsMonoTouch ? " MonoTouch" : "")
                + (IsWinRT ? ".NET WinRT" : "");
		}

		public static decimal ServiceStackVersion = 3.937m;

		public static bool IsUnix { get; set; }

		public static bool IsMono { get; set; }

		public static bool IsMonoTouch { get; set; }

		public static bool IsWinRT { get; set; }

		public static bool SupportsExpressions { get; set; }

		public static bool SupportsEmit { get; set; }

		public static string ServerUserAgent { get; set; }
	}
}