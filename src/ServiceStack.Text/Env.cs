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
            var platformName = PclExport.Instance.PlatformName;

            IsMono = AssemblyUtils.FindType("Mono.Runtime") != null;

            IsMonoTouch = AssemblyUtils.FindType("MonoTouch.Foundation.NSObject") != null;

            IsAndroid = AssemblyUtils.FindType("Android.Manifest") != null;

            IsWinRT = AssemblyUtils.FindType("Windows.ApplicationModel") != null;

            IsWindowsPhone = AssemblyUtils.FindType("Microsoft.Phone.Info.DeviceStatus") != null;

            IsSilverlight = AssemblyUtils.FindType("System.Windows.Interop.SilverlightHost") != null;

            SupportsExpressions = SupportsEmit = !IsMonoTouch;

            ServerUserAgent = "ServiceStack/" +
                ServiceStackVersion + " "
                + platformName
                + (IsMono ? "/Mono" : "/.NET")
                + (IsMonoTouch ? " IOS" : "")
                + (IsAndroid ? " Android" : "")
                + (IsSilverlight ? " Silverlight" : "")
                + (IsWindowsPhone ? " WindowsPhone" : "")
                + (IsWinRT ? " WinRT" : "");

            __releaseDate = DateTime.Parse("2001-01-01");
        }

        public static decimal ServiceStackVersion = 4.001m;

        public static bool IsUnix { get; set; }

        public static bool IsMono { get; set; }

        public static bool IsMonoTouch { get; set; }

        public static bool IsAndroid { get; set; }

        public static bool IsWinRT { get; set; }

        public static bool IsSilverlight { get; set; }

        public static bool IsWindowsPhone { get; set; }

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
#if !SL5
                if (!IsMono && referenceAssembyPath == null)
                {
                    var programFilesPath = PclExport.Instance.GetEnvironmentVariable("ProgramFiles(x86)") ?? @"C:\Program Files (x86)";
                    var netFxReferenceBasePath = programFilesPath + @"\Reference Assemblies\Microsoft\Framework\.NETFramework\";
                    if ((netFxReferenceBasePath + @"v4.0\").DirectoryExists())
                        referenceAssembyPath = netFxReferenceBasePath + @"v4.0\";
                    if ((netFxReferenceBasePath + @"v4.5\").DirectoryExists())
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