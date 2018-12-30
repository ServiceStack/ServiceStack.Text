using System.Collections.Generic;

namespace ServiceStack.Text
{
    public static class SsConfig
    {
        static SsConfig()
        {
            CustomTypeConverters = new List<ICustomTypeConverter>();
        }

        public static List<ICustomTypeConverter> CustomTypeConverters { get; private set; }

        // HACK: Temporary fix for clearing type converters after each test. Need some sort of scoping mechanism
        public static void Clear()
        {
            CustomTypeConverters = new List<ICustomTypeConverter>();
        }
    }
}
