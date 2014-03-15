// Copyright (c) Service Stack LLC. All Rights Reserved.
// License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt


using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ServiceStack.Text;

namespace ServiceStack
{
    public static class PathUtils
    {
        public static string MapAbsolutePath(this string relativePath, string appendPartialPathModifier)
        {
            return PclExport.Instance.MapAbsolutePath(relativePath, appendPartialPathModifier);
        }

        /// <summary>
        /// Maps the path of a file in the context of a VS project
        /// </summary>
        /// <param name="relativePath">the relative path</param>
        /// <returns>the absolute path</returns>
        /// <remarks>Assumes static content is two directories above the /bin/ directory,
        /// eg. in a unit test scenario  the assembly would be in /bin/Debug/.</remarks>
        public static string MapProjectPath(this string relativePath)
        {
            var mapPath = MapAbsolutePath(relativePath, string.Format("{0}..{0}..", PclExport.Instance.DirSep));
            return mapPath;
        }

        /// <summary>
        /// Maps the path of a file in a self-hosted scenario
        /// </summary>
        /// <param name="relativePath">the relative path</param>
        /// <returns>the absolute path</returns>
        /// <remarks>Assumes static content is copied to /bin/ folder with the assemblies</remarks>
        public static string MapAbsolutePath(this string relativePath)
        {
            var mapPath = MapAbsolutePath(relativePath, null);
            return mapPath;
        }

        /// <summary>
        /// Maps the path of a file in an Asp.Net hosted scenario
        /// </summary>
        /// <param name="relativePath">the relative path</param>
        /// <returns>the absolute path</returns>
        /// <remarks>Assumes static content is in the parent folder of the /bin/ directory</remarks>
        public static string MapHostAbsolutePath(this string relativePath)
        {
            var mapPath = MapAbsolutePath(relativePath, string.Format("{0}..", PclExport.Instance.DirSep));
            return mapPath;
        }

        internal static string CombinePaths(StringBuilder sb, params string[] paths)
        {
            foreach (var path in paths)
            {
                if (string.IsNullOrEmpty(path))
                    continue;

                if (sb.Length > 0 && sb[sb.Length - 1] != '/')
                    sb.Append("/");

                sb.Append(path.Replace('\\', '/').TrimStart('/'));
            }

            return sb.ToString();
        }

        public static string CombinePaths(params string[] paths)
        {
            return CombinePaths(new StringBuilder(), paths);
        }

        public static string AssertDir(this string dirPath)
        {
            if (!dirPath.DirectoryExists())
                dirPath.CreateDirectory();
            return dirPath;
        }

        public static string CombineWith(this string path, params string[] thesePaths)
        {
            if (path == null)
                path = "";

            if (thesePaths.Length == 1 && thesePaths[0] == null) return path;
            var startPath = path.Length > 1 ? path.TrimEnd('/', '\\') : path;
            return CombinePaths(new StringBuilder(startPath), thesePaths);
        }

        public static string CombineWith(this string path, params object[] thesePaths)
        {
            if (thesePaths.Length == 1 && thesePaths[0] == null) return path;
            return CombinePaths(new StringBuilder(path.TrimEnd('/', '\\')),
                Map(thesePaths, x => x.ToString()).ToArray());
        }

        internal static List<To> Map<To>(System.Collections.IEnumerable items, Func<object, To> converter)
        {
            if (items == null)
                return new List<To>();

            var list = new List<To>();
            foreach (var item in items)
            {
                list.Add(converter(item));
            }
            return list;
        }    
    }

}