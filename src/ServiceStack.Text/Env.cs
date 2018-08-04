//Copyright (c) ServiceStack, Inc. All Rights Reserved.
//License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

using System;
using System.Globalization;
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
            IsUWP = IsRunningAsUwp();
            if (!IsUWP)
            {
                IsMono = AssemblyUtils.FindType("Mono.Runtime") != null;

                IsIOS = AssemblyUtils.FindType("MonoTouch.Foundation.NSObject") != null
                    || AssemblyUtils.FindType("Foundation.NSObject") != null;

                IsAndroid = AssemblyUtils.FindType("Android.Manifest") != null;

                try
                {
                    IsOSX = AssemblyUtils.FindType("Mono.AppKit") != null;
#if NET45
                    IsWindows = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("windir"));
                    if (File.Exists(@"/System/Library/CoreServices/SystemVersion.plist"))
                        IsOSX = true;
                    string osType = File.Exists(@"/proc/sys/kernel/ostype") ? File.ReadAllText(@"/proc/sys/kernel/ostype") : null;
                    IsLinux = osType?.IndexOf("Linux", StringComparison.OrdinalIgnoreCase) >= 0;
#endif
                }
                catch (Exception) {}
            }

#if NETSTANDARD2_0
            IsNetStandard = true;
            try
            {
                IsLinux = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux);
                IsWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
                IsOSX  = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX);
                
                var fxDesc = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
                if (!IsIOS && IsOSX && fxDesc.Contains("Mono"))
                {
                    var runtimeDir = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();
                    //iOS detection no longer trustworthy so assuming iOS based on some current heuristics. TODO: improve iOS detection
                    IsIOS = runtimeDir.StartsWith("/private/var") ||
                            runtimeDir.Contains("/CoreSimulator/Devices/"); 
                }
                IsNetNative = fxDesc.Contains(".NET Native");
                IsNetCore = fxDesc.Contains(".NET Core");
            }
            catch (Exception) {} //throws PlatformNotSupportedException in AWS lambda
            IsUnix = IsOSX || IsLinux;
            HasMultiplePlatformTargets = true;
#elif NET45
            IsNetFramework = true;
            var platform = (int)Environment.OSVersion.Platform;
            IsUnix = platform == 4 || platform == 6 || platform == 128;
            IsLinux = IsUnix;
            if (Environment.GetEnvironmentVariable("OS")?.IndexOf("Windows", StringComparison.OrdinalIgnoreCase) >= 0)
                IsWindows = true;
#elif NETCORE2_1
            IsNetCore = true;
#endif
            
            SupportsExpressions = !IsIOS;
            SupportsEmit = !IsIOS && !IsUWP;

            if (IsUWP || IsIOS)
            {
                ReflectionOptimizer.Instance = ExpressionReflectionOptimizer.Provider;
            }

            ServerUserAgent = "ServiceStack/" +
                ServiceStackVersion + " "
                + platformName
                + (IsMono ? "/Mono" : "/.NET");

            VersionString = ServiceStackVersion.ToString(CultureInfo.InvariantCulture);

            __releaseDate = new DateTime(2001,01,01);
        }

        public static string VersionString { get; set; }

        public static decimal ServiceStackVersion = 5.00m;

        public static bool IsLinux { get; set; }

        public static bool IsOSX { get; set; }

        public static bool IsUnix { get; set; }

        public static bool IsWindows { get; set; }

        public static bool IsMono { get; set; }

        public static bool IsIOS { get; set; }

        public static bool IsAndroid { get; set; }

        public static bool IsNetNative { get; set; }

        public static bool IsUWP { get; set; }

        public static bool IsNetStandard { get; set; }

        public static bool IsNetFramework { get; set; }

        public static bool IsNetCore { get; set; }

        public static bool SupportsExpressions { get; private set; }

        public static bool SupportsEmit { get; private set; }

        public static bool StrictMode { get; set; }

        public static string ServerUserAgent { get; set; }

        public static bool HasMultiplePlatformTargets { get; set; }

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
                return referenceAssembyPath;
            }
            set => referenceAssembyPath = value;
        }
        
        //https://blogs.msdn.microsoft.com/appconsult/2016/11/03/desktop-bridge-identify-the-applications-context/
        //https://github.com/qmatteoq/DesktopBridgeHelpers/blob/master/DesktopBridge.Helpers/Helpers.cs        
        const long APPMODEL_ERROR_NO_PACKAGE = 15700L;

        [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
        static extern int GetCurrentPackageFullName(ref int packageFullNameLength, System.Text.StringBuilder packageFullName);

        private static bool IsRunningAsUwp()
        {
            try
            {
                if (IsWindows7OrLower)
                    return false;

                int length = 0;
                var sb = new System.Text.StringBuilder(0);
                int result = GetCurrentPackageFullName(ref length, sb);

                sb = new System.Text.StringBuilder(length);
                result = GetCurrentPackageFullName(ref length, sb);

                return result != APPMODEL_ERROR_NO_PACKAGE;
            }
            catch (TypeLoadException e) //of course the recommended code to detect UWP fails in .NET Native UWP
            {                
                return IsWindows && IsNetNative;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private static bool IsWindows7OrLower
        {
            get
            {
                int versionMajor = Environment.OSVersion.Version.Major;
                int versionMinor = Environment.OSVersion.Version.Minor;
                double version = versionMajor + (double)versionMinor / 10;
                return version <= 6.1;
            }
        }    
        
    }
}