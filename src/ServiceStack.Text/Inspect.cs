using System;
using System.IO;

namespace ServiceStack.Text
{
    /// <summary>
    /// Helper utility for inspecting variables
    /// </summary>
    public static class Inspect
    {
        public static class Config
        {
            public static Action<object> VarsFilter { get; set; } = DefaultVarsFilter;

            public static void DefaultVarsFilter(object anonArgs)
            {
                try
                {
                    File.WriteAllText("vars.json", anonArgs.ToSafeJson());
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
        public static void Vars(object anonArgs) => Config.VarsFilter?.Invoke(anonArgs);
    }
}