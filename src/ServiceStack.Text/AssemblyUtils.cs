using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
#if SILVERLIGHT
using System.Windows;
#endif
using ServiceStack.Common.Support;

namespace ServiceStack.Text
{
    /// <summary>
    /// Utils to load types
    /// </summary>
    public static class AssemblyUtils
    {
        private const string FileUri = "file:///";
        private const string DllExt = "dll";
        private const string ExeExt = "dll";
        private const char UriSeperator = '/';

#if !XBOX
        /// <summary>
        /// Find the type from the name supplied
        /// </summary>
        /// <param name="typeName">[typeName] or [typeName, assemblyName]</param>
        /// <returns></returns>
        public static Type FindType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null) return type;

            var typeDef = new AssemblyTypeDefinition(typeName);
            if (!String.IsNullOrEmpty(typeDef.AssemblyName))
            {
                return FindType(typeDef.TypeName, typeDef.AssemblyName);
            }
            else
            {
                return FindTypeFromLoadedAssemblies(typeDef.TypeName);
            }
        }
#endif

#if !XBOX
        /// <summary>
        /// Find type if it exists
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="assemblyName"></param>
        /// <returns>The type if it exists</returns>
        public static Type FindType(string typeName, string assemblyName)
        {
            var type = FindTypeFromLoadedAssemblies(typeName);
            if (type != null)
            {
                return type;
            }
            var binPath = GetAssemblyBinPath(Assembly.GetExecutingAssembly());
            Assembly assembly = null;
            var assemblyDllPath = binPath + String.Format("{0}.{1}", assemblyName, DllExt);
            if (File.Exists(assemblyDllPath))
            {
                assembly = LoadAssembly(assemblyDllPath);
            }
            var assemblyExePath = binPath + String.Format("{0}.{1}", assemblyName, ExeExt);
            if (File.Exists(assemblyExePath))
            {
                assembly = LoadAssembly(assemblyExePath);
            }
            return assembly != null ? assembly.GetType(typeName) : null;
        }
#endif

#if !XBOX
        public static Type FindTypeFromLoadedAssemblies(string typeName)
        {
#if SILVERLIGHT4
        	var assemblies = ((dynamic) AppDomain.CurrentDomain).GetAssemblies() as Assembly[];
#else
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
#endif
        	foreach (var assembly in assemblies)
            {
                var type = assembly.GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }
            return null;
        }
#endif

#if !SILVERLIGHT
private static Assembly LoadAssembly(string assemblyPath)
		{
			return Assembly.LoadFrom(assemblyPath);
		}
#else
        private static Assembly LoadAssembly(string assemblyPath)
        {            
            var sri = Application.GetResourceStream(new Uri(assemblyPath, UriKind.Relative));
            var myPart = new AssemblyPart();
            var assembly = myPart.Load(sri.Stream);
            return assembly;
        }
#endif

#if !XBOX
        public static string GetAssemblyBinPath(Assembly assembly)
        {
            var binPathPos = assembly.CodeBase.LastIndexOf(UriSeperator);
            var assemblyPath = assembly.CodeBase.Substring(0, binPathPos + 1);
            if (assemblyPath.StartsWith(FileUri))
            {
                assemblyPath = assemblyPath.Remove(0, FileUri.Length);
            }
            return assemblyPath;
        }
#endif

#if !SILVERLIGHT
		static readonly Regex versionRegEx = new Regex(", Version=[^\\]]+", RegexOptions.Compiled);
#else
        static readonly Regex versionRegEx = new Regex(", Version=[^\\]]+");
#endif
        public static string ToTypeString(this Type type)
        {
            return versionRegEx.Replace(type.AssemblyQualifiedName, "");
        }
    }
}