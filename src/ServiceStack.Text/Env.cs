//Copyright (c) Service Stack LLC. All Rights Reserved.
//License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

using System;
using System.IO;

namespace ServiceStack.Text
{
    public static class Env
    {
        static Env()
        {
            string platformName = null;

#if NETFX_CORE
            platformName = "WinRT";
#else
            var platform = (int)Environment.OSVersion.Platform;
            IsUnix = (platform == 4) || (platform == 6) || (platform == 128);
            platformName = Environment.OSVersion.Platform.ToString();
#endif

            IsMono = AssemblyUtils.FindType("Mono.Runtime") != null;

            IsMonoTouch = AssemblyUtils.FindType("MonoTouch.Foundation.NSObject") != null;

            IsWinRT = AssemblyUtils.FindType("Windows.ApplicationModel") != null;

            SupportsExpressions = SupportsEmit = !IsMonoTouch;

            ServerUserAgent = "ServiceStack/" +
                ServiceStackVersion + " "
                + platformName
                + (IsMono ? "/Mono" : "/.NET")
                + (IsMonoTouch ? " MonoTouch" : "")
                + (IsWinRT ? ".NET WinRT" : "");

            __releaseDate = DateTime.Parse("2001-01-01");
        }

        public static decimal ServiceStackVersion = 4.001m;

        public static bool IsUnix { get; set; }

        public static bool IsMono { get; set; }

        public static bool IsMonoTouch { get; set; }

        public static bool IsWinRT { get; set; }

        public static bool SupportsExpressions { get; set; }

        public static bool SupportsEmit { get; set; }

        public static string ServerUserAgent { get; set; }

        private static readonly DateTime __releaseDate;
        public static DateTime GetReleaseDate()
        {
            return __releaseDate;
        }

        private static string referenceAssembyPath;
        public static string ReferenceAssembyPath
        {
            get
            {
#if !SILVERLIGHT
                if (!IsMono && referenceAssembyPath == null)
                {
                    var programFilesPath = Environment.GetEnvironmentVariable("ProgramFiles(x86)") ?? @"C:\Program Files (x86)";
                    var netFxReferenceBasePath = programFilesPath + @"\Reference Assemblies\Microsoft\Framework\.NETFramework\";
                    if (Directory.Exists(netFxReferenceBasePath + @"v4.0\"))
                        referenceAssembyPath = netFxReferenceBasePath + @"v4.0\";
                    if (Directory.Exists(netFxReferenceBasePath + @"v4.5\"))
                        referenceAssembyPath = netFxReferenceBasePath + @"v4.5\";
                    else
                        throw new FileNotFoundException(
                            "Could not infer .NET Reference Assemblies path, e.g '{0}'.\n".Fmt(netFxReferenceBasePath + @"v4.0\") +
                            "Provide path manually 'Env.ReferenceAssembyPath'.");
                }
#endif
                return referenceAssembyPath;
            }
            set { referenceAssembyPath = value; }
        }
    }
}