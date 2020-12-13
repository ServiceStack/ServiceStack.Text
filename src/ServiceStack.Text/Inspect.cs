using System;
using System.IO;
using ServiceStack.Text;

namespace ServiceStack
{
    /// <summary>
    /// Helper utility for inspecting variables
    /// </summary>
    public static class Inspect
    {
        public static class Config
        {
            public const string VarsName = "vars.json";
            
            public static Action<object> VarsFilter { get; set; } = DefaultVarsFilter;

            public static void DefaultVarsFilter(object anonArgs)
            {
                try
                {
                    var inspectVarsPath = Environment.GetEnvironmentVariable("INSPECT_VARS");
                    if (string.IsNullOrEmpty(inspectVarsPath)) // Disable
                        return;
                    
                    var varsPath = Path.DirectorySeparatorChar == '\\'
                        ? inspectVarsPath.Replace('/','\\')
                        : inspectVarsPath.Replace('\\','/');

                    if (varsPath.IndexOf(Path.DirectorySeparatorChar) >= 0)
                        Path.GetDirectoryName(varsPath).AssertDir();
                    
                    File.WriteAllText(varsPath, anonArgs.ToSafeJson());
                }
                catch (Exception ex)
                {
                    Tracer.Instance.WriteError("Inspect.Vars() Error: " + ex);
                }
            }
        }

        /// <summary>
        /// Dump serialized values to 'vars.json'
        /// </summary>
        /// <param name="anonArgs">Anonymous object with named value</param>
        // ReSharper disable once InconsistentNaming
        public static void vars(object anonArgs) => Config.VarsFilter?.Invoke(anonArgs);
    }
}