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
            if (PclExport.Instance == null)
                throw new ArgumentException("PclExport.Instance needs to be initialized");

            var platformName = PclExport.Instance.PlatformName;
            if (platformName != PclExport.Platforms.WindowsStore)
            {
                IsMono = AssemblyUtils.FindType("Mono.Runtime") != null;

                IsMonoTouch = AssemblyUtils.FindType("MonoTouch.Foundation.NSObject") != null
                    || AssemblyUtils.FindType("Foundation.NSObject") != null;

                IsAndroid = AssemblyUtils.FindType("Android.Manifest") != null;

                //Throws unhandled exception if not called from the main thread
                //IsWinRT = AssemblyUtils.FindType("Windows.ApplicationModel") != null;

                IsWindowsPhone = AssemblyUtils.FindType("Microsoft.Phone.Info.DeviceStatus") != null;

                IsSilverlight = AssemblyUtils.FindType("System.Windows.Interop.SilverlightHost") != null;
            }
            else
            {
                IsWindowsStore = true;
            }

#if PCL
            IsUnix = IsMono;
#else
            var platform = (int)Environment.OSVersion.Platform;
            IsUnix = (platform == 4) || (platform == 6) || (platform == 128);
#endif

            ServerUserAgent = "ServiceStack/" +
                ServiceStackVersion + " "
                + platformName
                + (IsMono ? "/Mono" : "/.NET");

            VersionString = ServiceStackVersion.ToString();

            __releaseDate = DateTime.Parse("2001-01-01");
        }

        public static string VersionString { get; set; }

        public static decimal ServiceStackVersion = 4.00m;

        public static bool IsUnix { get; set; }

        public static bool IsMono { get; set; }

        public static bool IsMonoTouch { get; set; }

        public static bool IsAndroid { get; set; }

        public static bool IsWindowsStore { get; set; }

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
                    if ((netFxReferenceBasePath + @"v4.5.2\").DirectoryExists())
                        referenceAssembyPath = netFxReferenceBasePath + @"v4.5.2\";
                    else if ((netFxReferenceBasePath + @"v4.5.1\").DirectoryExists())
                        referenceAssembyPath = netFxReferenceBasePath + @"v4.5.1\";
                    else if ((netFxReferenceBasePath + @"v4.5\").DirectoryExists())
                        referenceAssembyPath = netFxReferenceBasePath + @"v4.5\";
                    else if ((netFxReferenceBasePath + @"v4.0\").DirectoryExists())
                        referenceAssembyPath = netFxReferenceBasePath + @"v4.0\";
                    else
                    {
                        var v4Dirs = PclExport.Instance.GetDirectoryNames(netFxReferenceBasePath, "v4*");
                        if (v4Dirs.Length == 0)
                        {
                            var winPath = PclExport.Instance.GetEnvironmentVariable("SYSTEMROOT") ?? @"C:\Windows";
                            var gacPath = winPath + @"\Microsoft.NET\Framework\";
                            v4Dirs = PclExport.Instance.GetDirectoryNames(gacPath, "v4*");                            
                        }
                        if (v4Dirs.Length > 0)
                        {
                            referenceAssembyPath = v4Dirs[v4Dirs.Length - 1] + @"\"; //latest v4
                        }
                        else
                        {
                            throw new FileNotFoundException(
                                "Could not infer .NET Reference Assemblies path, e.g '{0}'.\n".Fmt(netFxReferenceBasePath + @"v4.0\") +
                                "Provide path manually 'Env.ReferenceAssembyPath'.");
                        }
                    }
                }
#endif
                return referenceAssembyPath;
            }
            set { referenceAssembyPath = value; }
        }
    }
}