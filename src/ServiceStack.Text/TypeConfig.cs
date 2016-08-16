using System;
using System.Linq;
using System.Reflection;

namespace ServiceStack.Text
{
    internal class TypeConfig
    {
        internal readonly Type Type;
        internal bool EnableAnonymousFieldSetterses;
        internal PropertyInfo[] Properties;
        internal FieldInfo[] Fields;
        internal Func<object, string, object, object> OnDeserializing;
        internal bool IsUserType { get; set; }

        internal TypeConfig(Type type)
        {
            Type = type;
            EnableAnonymousFieldSetterses = false;
            Properties = TypeConstants.EmptyPropertyInfoArray;
            Fields = TypeConstants.EmptyFieldInfoArray;

            JsConfig.AddUniqueType(Type);
        }
    }

    public static class TypeConfig<T>
    {
        internal static TypeConfig config;

        static TypeConfig Config
        {
            get { return config ?? (config = Create()); }
        }

        public static PropertyInfo[] Properties
        {
            get { return Config.Properties; }
            set { Config.Properties = value; }
        }

        public static FieldInfo[] Fields
        {
            get { return Config.Fields; }
            set { Config.Fields = value; }
        }

        public static bool EnableAnonymousFieldSetters
        {
            get { return Config.EnableAnonymousFieldSetterses; }
            set { Config.EnableAnonymousFieldSetterses = value; }
        }

        public static bool IsUserType
        {
            get { return Config.IsUserType; }
            set { Config.IsUserType = value; }
        }

        static TypeConfig()
        {
            Init();
        }

        internal static void Init()
        {
            if (config == null)
            {
                Create();
            }
        }

        public static Func<object, string, object, object> OnDeserializing
        {
            get { return config.OnDeserializing; }
            set { config.OnDeserializing = value; }
        }

        static TypeConfig Create()
        {
            config = new TypeConfig(typeof(T));

            var excludedProperties = JsConfig<T>.ExcludePropertyNames ?? TypeConstants.EmptyStringArray;

            var properties = excludedProperties.Any()
                ? config.Type.GetSerializableProperties().Where(x => !excludedProperties.Contains(x.Name))
                : config.Type.GetSerializableProperties();
            Properties = properties.Where(x => x.GetIndexParameters().Length == 0).ToArray();

            Fields = config.Type.GetSerializableFields().ToArray();
    
            if (!JsConfig<T>.HasDeserialingFn)
                OnDeserializing = ReflectionExtensions.GetOnDeserializing<T>();
            else
                config.OnDeserializing = (instance, memberName, value) => JsConfig<T>.OnDeserializingFn((T)instance, memberName, value);

            IsUserType = !typeof(T).IsValueType() && typeof(T).Namespace != "System";

            return config;
        }

        public static void Reset()
        {
            config = null;
        }

        internal static TypeConfig GetState()
        {
            return Config;
        }
    }
}