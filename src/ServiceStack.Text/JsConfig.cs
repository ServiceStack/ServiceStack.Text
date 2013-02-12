using System;
using System.Collections.Generic;
using System.IO;
using ServiceStack.Text.Common;
using ServiceStack.Text.Json;
using ServiceStack.Text.Jsv;


#if WINDOWS_PHONE && !WP8
using ServiceStack.Text.WP;
#endif

namespace ServiceStack.Text
{
    public static class
        JsConfig
    {
        static JsConfig()
        {
            //In-built default serialization, to Deserialize Color struct do:
            //JsConfig<System.Drawing.Color>.SerializeFn = c => c.ToString().Replace("Color ", "").Replace("[", "").Replace("]", "");
            //JsConfig<System.Drawing.Color>.DeSerializeFn = System.Drawing.Color.FromName;
            Reset();
        }

        public static JsConfigScope BeginScope()
        {
            return new JsConfigScope();
        }

        public static JsConfigScope With(
            bool? convertObjectTypesIntoStringDictionary = null,
            bool? tryToParsePrimitiveTypeValues = null,
            bool? includeNullValues = null,
            bool? excludeTypeInfo = null,
            bool? includeTypeInfo = null,
            bool? emitCamelCaseNames = null,
            bool? emitLowercaseUnderscoreNames = null,
            JsonDateHandler? dateHandler = null,
            JsonTimeSpanHandler? timeSpanHandler = null,
            bool? preferInterfaces = null,
            bool? throwOnDeserializationError = null,
            string typeAttr = null,
            Func<Type, string> typeWriter = null,
            Func<string, Type> typeFinder = null,
			bool? treatEnumAsInteger = null,
            bool? alwaysUseUtc = null,
            bool? includePublicFields = null,
            EmptyCtorFactoryDelegate modelFactory = null)
        {
            return new JsConfigScope {
                ConvertObjectTypesIntoStringDictionary = convertObjectTypesIntoStringDictionary ?? sConvertObjectTypesIntoStringDictionary,
                TryToParsePrimitiveTypeValues = tryToParsePrimitiveTypeValues ?? sTryToParsePrimitiveTypeValues,
                IncludeNullValues = includeNullValues ?? sIncludeNullValues,
                ExcludeTypeInfo = excludeTypeInfo ?? sExcludeTypeInfo,
                IncludeTypeInfo = includeTypeInfo ?? sIncludeTypeInfo,
                EmitCamelCaseNames = emitCamelCaseNames ?? sEmitCamelCaseNames,
                EmitLowercaseUnderscoreNames = emitLowercaseUnderscoreNames ?? sEmitLowercaseUnderscoreNames,
                DateHandler = dateHandler ?? sDateHandler,
                TimeSpanHandler = timeSpanHandler ?? sTimeSpanHandler,
                PreferInterfaces = preferInterfaces ?? sPreferInterfaces,
                ThrowOnDeserializationError = throwOnDeserializationError ?? sThrowOnDeserializationError,
                TypeAttr = typeAttr ?? sTypeAttr,
                TypeWriter = typeWriter ?? sTypeWriter,
                TypeFinder = typeFinder ?? sTypeFinder,
                TreatEnumAsInteger = treatEnumAsInteger ?? sTreatEnumAsInteger,
                AlwaysUseUtc = alwaysUseUtc ?? sAlwaysUseUtc,
                IncludePublicFields = includePublicFields ?? sIncludePublicFields,
                ModelFactory = modelFactory ?? ModelFactory,
            };
        }

        private static bool? sConvertObjectTypesIntoStringDictionary;
        public static bool ConvertObjectTypesIntoStringDictionary
        {
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.ConvertObjectTypesIntoStringDictionary: null)
                    ?? sConvertObjectTypesIntoStringDictionary 
                    ?? false;
            }
            set
            {
                if (!sConvertObjectTypesIntoStringDictionary.HasValue) sConvertObjectTypesIntoStringDictionary = value;
            }
        }

        private static bool? sTryToParsePrimitiveTypeValues;
        public static bool TryToParsePrimitiveTypeValues
        {
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.TryToParsePrimitiveTypeValues: null)
                    ?? sTryToParsePrimitiveTypeValues 
                    ?? false;
            }
            set
            {
                if (!sTryToParsePrimitiveTypeValues.HasValue) sTryToParsePrimitiveTypeValues = value;
            }
        }

        private static bool? sIncludeNullValues;
        public static bool IncludeNullValues
        {
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.IncludeNullValues: null)
                    ?? sIncludeNullValues 
                    ?? false;
            }
            set
            {
                if (!sIncludeNullValues.HasValue) sIncludeNullValues = value;
            }
        }

        private static bool? sTreatEnumAsInteger;
        public static bool TreatEnumAsInteger
        {
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.TreatEnumAsInteger: null)
                    ?? sTreatEnumAsInteger 
                    ?? false;
            }
            set
            {
                if (!sTreatEnumAsInteger.HasValue) sTreatEnumAsInteger = value;
            }
        }

        private static bool? sExcludeTypeInfo;
        public static bool ExcludeTypeInfo
        {
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.ExcludeTypeInfo: null)
                    ?? sExcludeTypeInfo 
                    ?? false;
            }
            set
            {
                if (!sExcludeTypeInfo.HasValue) sExcludeTypeInfo = value;
            }
        }

        private static bool? sIncludeTypeInfo;
        public static bool IncludeTypeInfo
        {
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.IncludeTypeInfo: null)
                    ?? sIncludeTypeInfo 
                    ?? false;
            }
            set
            {
                if (!sIncludeTypeInfo.HasValue) sIncludeTypeInfo = value;
            }
        }

        private static string sTypeAttr;
        public static string TypeAttr
        {
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.TypeAttr: null)
                    ?? sTypeAttr 
                    ?? JsWriter.TypeAttr;
            }
            set
            {
                if (sTypeAttr == null) sTypeAttr = value;
                JsonTypeAttrInObject = JsonTypeSerializer.GetTypeAttrInObject(value);
                JsvTypeAttrInObject = JsvTypeSerializer.GetTypeAttrInObject(value);
            }
        }

        private static string sJsonTypeAttrInObject;
        private static readonly string defaultJsonTypeAttrInObject = JsonTypeSerializer.GetTypeAttrInObject(TypeAttr);
        internal static string JsonTypeAttrInObject
        {
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.JsonTypeAttrInObject: null)
                    ?? sJsonTypeAttrInObject 
                    ?? defaultJsonTypeAttrInObject;
            }
            set
            {
                if (sJsonTypeAttrInObject == null) sJsonTypeAttrInObject = value;
            }
        }

        private static string sJsvTypeAttrInObject;
        private static readonly string defaultJsvTypeAttrInObject = JsvTypeSerializer.GetTypeAttrInObject(TypeAttr);
        internal static string JsvTypeAttrInObject
        {
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.JsvTypeAttrInObject: null)
                    ?? sJsvTypeAttrInObject 
                    ?? defaultJsvTypeAttrInObject;
            }
            set
            {
                if (sJsvTypeAttrInObject == null) sJsvTypeAttrInObject = value;
            }
        }

        private static Func<Type, string> sTypeWriter;
        public static Func<Type, string> TypeWriter
        {
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.TypeWriter: null)
                    ?? sTypeWriter 
                    ?? AssemblyUtils.WriteType;
            }
            set
            {
                if (sTypeWriter == null) sTypeWriter = value;
            }
        }

        private static Func<string, Type> sTypeFinder;
        public static Func<string, Type> TypeFinder
        {
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.TypeFinder: null)
                    ?? sTypeFinder 
                    ?? AssemblyUtils.FindType;
            }
            set
            {
                if (sTypeFinder == null) sTypeFinder = value;
            }
        }

        private static JsonDateHandler? sDateHandler;
        public static JsonDateHandler DateHandler
        {
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.DateHandler: null)
                    ?? sDateHandler 
                    ?? JsonDateHandler.TimestampOffset;
            }
            set
            {
                if (!sDateHandler.HasValue) sDateHandler = value;
            }
        }

        /// <summary>
        /// Sets which format to use when serializing TimeSpans
        /// </summary>
        private static JsonTimeSpanHandler? sTimeSpanHandler;
        public static JsonTimeSpanHandler TimeSpanHandler
        {
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.TimeSpanHandler : null)
                    ?? sTimeSpanHandler
                    ?? JsonTimeSpanHandler.DurationFormat;
            }
            set
            {
                if (!sTimeSpanHandler.HasValue) sTimeSpanHandler = value;
            }
        }


        /// <summary>
        /// <see langword="true"/> if the <see cref="ITypeSerializer"/> is configured
        /// to take advantage of <see cref="CLSCompliantAttribute"/> specification,
        /// to support user-friendly serialized formats, ie emitting camelCasing for JSON
        /// and parsing member names and enum values in a case-insensitive manner.
        /// </summary>
        private static bool? sEmitCamelCaseNames;
        public static bool EmitCamelCaseNames
        {
            // obeying the use of ThreadStatic, but allowing for setting JsConfig once as is the normal case
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.EmitCamelCaseNames: null)
                    ?? sEmitCamelCaseNames 
                    ?? false;
            }
            set
            {
                if (!sEmitCamelCaseNames.HasValue) sEmitCamelCaseNames = value;
            }
        }

        /// <summary>
        /// <see langword="true"/> if the <see cref="ITypeSerializer"/> is configured
        /// to support web-friendly serialized formats, ie emitting lowercase_underscore_casing for JSON
        /// </summary>
        private static bool? sEmitLowercaseUnderscoreNames;
        public static bool EmitLowercaseUnderscoreNames
        {
            // obeying the use of ThreadStatic, but allowing for setting JsConfig once as is the normal case
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.EmitLowercaseUnderscoreNames: null)
                    ?? sEmitLowercaseUnderscoreNames 
                    ?? false;
            }
            set
            {
                if (!sEmitLowercaseUnderscoreNames.HasValue) sEmitLowercaseUnderscoreNames = value;
            }
        }

        /// <summary>
        /// Define how property names are mapped during deserialization
        /// </summary>
        private static JsonPropertyConvention propertyConvention;
        public static JsonPropertyConvention PropertyConvention
        {
            get { return propertyConvention; }
            set
            {
                propertyConvention = value;
                switch (propertyConvention)
                {
                    case JsonPropertyConvention.ExactMatch:
                        DeserializeTypeRefJson.PropertyNameResolver = DeserializeTypeRefJson.DefaultPropertyNameResolver;
                        break;
                    case JsonPropertyConvention.Lenient:
                        DeserializeTypeRefJson.PropertyNameResolver = DeserializeTypeRefJson.LenientPropertyNameResolver;
                        break;
                }
            }
        }


        /// <summary>
        /// Gets or sets a value indicating if the framework should throw serialization exceptions
        /// or continue regardless of deserialization errors. If <see langword="true"/>  the framework
        /// will throw; otherwise, it will parse as many fields as possible. The default is <see langword="false"/>.
        /// </summary>
        private static bool? sThrowOnDeserializationError;
        public static bool ThrowOnDeserializationError
        {
            // obeying the use of ThreadStatic, but allowing for setting JsConfig once as is the normal case
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.ThrowOnDeserializationError: null)
                    ?? sThrowOnDeserializationError 
                    ?? false;
            }
            set
            {
                if (!sThrowOnDeserializationError.HasValue) sThrowOnDeserializationError = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if the framework should always convert <see cref="DateTime"/> to UTC format instead of local time. 
        /// </summary>
        private static bool? sAlwaysUseUtc;
        public static bool AlwaysUseUtc
        {
            // obeying the use of ThreadStatic, but allowing for setting JsConfig once as is the normal case
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.AlwaysUseUtc: null)
                    ?? sAlwaysUseUtc 
                    ?? false;
            }
            set
            {
                if (!sAlwaysUseUtc.HasValue) sAlwaysUseUtc = value;
            }
        }

        internal static HashSet<Type> HasSerializeFn = new HashSet<Type>();

        public static HashSet<Type> TreatValueAsRefTypes = new HashSet<Type>();

        private static bool? sPreferInterfaces;
        /// <summary>
        /// If set to true, Interface types will be prefered over concrete types when serializing.
        /// </summary>
        public static bool PreferInterfaces
        {
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.PreferInterfaces: null)
                    ?? sPreferInterfaces 
                    ?? false;
            }
            set
            {
                if (!sPreferInterfaces.HasValue) sPreferInterfaces = value;
            }
        }

        internal static bool TreatAsRefType(Type valueType)
        {
            return TreatValueAsRefTypes.Contains(valueType.IsGenericType ? valueType.GetGenericTypeDefinition() : valueType);
        }


        /// <summary>
        /// If set to true, Interface types will be prefered over concrete types when serializing.
        /// </summary>
        private static bool? sIncludePublicFields;
        public static bool IncludePublicFields
        {
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.IncludePublicFields : null)
                    ?? sIncludePublicFields
                    ?? false;
            }
            set
            {
                if (!sIncludePublicFields.HasValue) sIncludePublicFields = value;
            }
        }

        /// <summary>
        /// Set this to enable your own type construction provider.
        /// This is helpful for integration with IoC containers where you need to call the container constructor.
        /// Return null if you don't know how to construct the type and the parameterless constructor will be used.
        /// </summary>
        private static EmptyCtorFactoryDelegate sModelFactory;
        public static EmptyCtorFactoryDelegate ModelFactory
        {
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.ModelFactory : null)
                    ?? sModelFactory
                    ?? null;
            }
            set
            {
                if (sModelFactory != null) sModelFactory = value;
            }
        }

        public static void Reset()
        {
            sModelFactory = ReflectionExtensions.GetConstructorMethodToCache;
            sTryToParsePrimitiveTypeValues = null;
            sConvertObjectTypesIntoStringDictionary = null;
            sIncludeNullValues = null;
            sExcludeTypeInfo = null;
            sEmitCamelCaseNames = null;
            sEmitLowercaseUnderscoreNames = null;
            sDateHandler = null;
            sTimeSpanHandler = null;
            sPreferInterfaces = null;
            sThrowOnDeserializationError = null;
            sTypeAttr = null;
            sJsonTypeAttrInObject = null;
            sJsvTypeAttrInObject = null;
            sTypeWriter = null;
            sTypeFinder = null;
			sTreatEnumAsInteger = null;
            sAlwaysUseUtc = null;
            sIncludePublicFields = null;
            HasSerializeFn = new HashSet<Type>();
            TreatValueAsRefTypes = new HashSet<Type> { typeof(KeyValuePair<,>) };
            PropertyConvention = JsonPropertyConvention.ExactMatch;
        }

#if MONOTOUCH
        /// <summary>
        /// Provide hint to MonoTouch AOT compiler to pre-compile generic classes for all your DTOs.
        /// Just needs to be called once in a static constructor.
        /// </summary>
        [MonoTouch.Foundation.Preserve]
		public static void InitForAot() { 
		}

        [MonoTouch.Foundation.Preserve]
        public static void RegisterForAot()
        {
			RegisterTypeForAot<Poco>();

            RegisterElement<Poco, string>();

            RegisterElement<Poco, bool>();
            RegisterElement<Poco, char>();
            RegisterElement<Poco, byte>();
            RegisterElement<Poco, sbyte>();
            RegisterElement<Poco, short>();
            RegisterElement<Poco, ushort>();
            RegisterElement<Poco, int>();
            RegisterElement<Poco, uint>();

			RegisterElement<Poco, long>();
            RegisterElement<Poco, ulong>();
            RegisterElement<Poco, float>();
            RegisterElement<Poco, double>();
            RegisterElement<Poco, decimal>();

            RegisterElement<Poco, bool?>();
            RegisterElement<Poco, char?>();
            RegisterElement<Poco, byte?>();
            RegisterElement<Poco, sbyte?>();
            RegisterElement<Poco, short?>();
            RegisterElement<Poco, ushort?>();
            RegisterElement<Poco, int?>();
            RegisterElement<Poco, uint?>();
            RegisterElement<Poco, long?>();
            RegisterElement<Poco, ulong?>();
            RegisterElement<Poco, float?>();
            RegisterElement<Poco, double?>();
            RegisterElement<Poco, decimal?>();

			//RegisterElement<Poco, JsonValue>();

			RegisterTypeForAot<DayOfWeek>(); // used by DateTime

			// register built in structs
			RegisterTypeForAot<Guid>();
			RegisterTypeForAot<TimeSpan>();
			RegisterTypeForAot<DateTime>();
			RegisterTypeForAot<DateTime?>();
			RegisterTypeForAot<TimeSpan?>();
			RegisterTypeForAot<Guid?>();
        }

		[MonoTouch.Foundation.Preserve]
		public static void RegisterTypeForAot<T>()
		{
			AotConfig.RegisterSerializers<T>();
		}

        [MonoTouch.Foundation.Preserve]
        static void RegisterQueryStringWriter()
        {
            var i = 0;
            if (QueryStringWriter<Poco>.WriteFn() != null) i++;
        }
		        
        [MonoTouch.Foundation.Preserve]
		internal static int RegisterElement<T, TElement>()
        {
			var i = 0;
			i += AotConfig.RegisterSerializers<TElement>();
			AotConfig.RegisterElement<T, TElement, JsonTypeSerializer>();
			AotConfig.RegisterElement<T, TElement, JsvTypeSerializer>();
			return i;
		}

		///<summary>
		/// Class contains Ahead-of-Time (AOT) explicit class declarations which is used only to workaround "-aot-only" exceptions occured on device only. 
		/// </summary>			
		[MonoTouch.Foundation.Preserve(AllMembers=true)]
		internal class AotConfig
		{
			internal static JsReader<JsonTypeSerializer> jsonReader;
			internal static JsWriter<JsonTypeSerializer> jsonWriter;
			internal static JsReader<JsvTypeSerializer> jsvReader;
			internal static JsWriter<JsvTypeSerializer> jsvWriter;
			internal static JsonTypeSerializer jsonSerializer;
			internal static JsvTypeSerializer jsvSerializer;

			static AotConfig()
			{
				jsonSerializer = new JsonTypeSerializer();
				jsvSerializer = new JsvTypeSerializer();
				jsonReader = new JsReader<JsonTypeSerializer>();
				jsonWriter = new JsWriter<JsonTypeSerializer>();
				jsvReader = new JsReader<JsvTypeSerializer>();
				jsvWriter = new JsWriter<JsvTypeSerializer>();
			}

			internal static int RegisterSerializers<T>()
			{
				var i = 0;
				i += Register<T, JsonTypeSerializer>();
				if (jsonSerializer.GetParseFn<T>() != null) i++;
				if (jsonSerializer.GetWriteFn<T>() != null) i++;
				if (jsonReader.GetParseFn<T>() != null) i++;
				if (jsonWriter.GetWriteFn<T>() != null) i++;

				i += Register<T, JsvTypeSerializer>();
				if (jsvSerializer.GetParseFn<T>() != null) i++;
				if (jsvSerializer.GetWriteFn<T>() != null) i++;
				if (jsvReader.GetParseFn<T>() != null) i++;
				if (jsvWriter.GetWriteFn<T>() != null) i++;


				//RegisterCsvSerializer<T>();
				RegisterQueryStringWriter();
				return i;
			}

			internal static void RegisterCsvSerializer<T>()
			{
				CsvSerializer<T>.WriteFn();
				CsvSerializer<T>.WriteObject(null, null);
				CsvWriter<T>.Write(null, default(IEnumerable<T>));
				CsvWriter<T>.WriteRow(null, default(T));
			}

			public static ParseStringDelegate GetParseFn(Type type)
			{
				var parseFn = JsonTypeSerializer.Instance.GetParseFn(type);
				return parseFn;
			}

			internal static int Register<T, TSerializer>() where TSerializer : ITypeSerializer 
			{
				var i = 0;

				if (JsonWriter<T>.WriteFn() != null) i++;
				if (JsonWriter.Instance.GetWriteFn<T>() != null) i++;
				if (JsonReader.Instance.GetParseFn<T>() != null) i++;
				if (JsonReader<T>.Parse(null) != null) i++;
				if (JsonReader<T>.GetParseFn() != null) i++;
				//if (JsWriter.GetTypeSerializer<JsonTypeSerializer>().GetWriteFn<T>() != null) i++;
				if (new List<T>() != null) i++;
				if (new T[0] != null) i++;

				JsConfig<T>.ExcludeTypeInfo = false;
				
				if (JsConfig<T>.OnDeserializedFn != null) i++;
				if (JsConfig<T>.HasDeserializeFn) i++;
				if (JsConfig<T>.SerializeFn != null) i++;
				if (JsConfig<T>.DeSerializeFn != null) i++;
				//JsConfig<T>.SerializeFn = arg => "";
				//JsConfig<T>.DeSerializeFn = arg => default(T);
				if (TypeConfig<T>.Properties != null) i++;

/*
				if (WriteType<T, TSerializer>.Write != null) i++;
				if (WriteType<object, TSerializer>.Write != null) i++;
				
				if (DeserializeBuiltin<T>.Parse != null) i++;
				if (DeserializeArray<T[], TSerializer>.Parse != null) i++;
				DeserializeType<TSerializer>.ExtractType(null);
				DeserializeArrayWithElements<T, TSerializer>.ParseGenericArray(null, null);
				DeserializeCollection<TSerializer>.ParseCollection<T>(null, null, null);
				DeserializeListWithElements<T, TSerializer>.ParseGenericList(null, null, null);

				SpecializedQueueElements<T>.ConvertToQueue(null);
				SpecializedQueueElements<T>.ConvertToStack(null);
*/

				WriteListsOfElements<T, TSerializer>.WriteList(null, null);
				WriteListsOfElements<T, TSerializer>.WriteIList(null, null);
				WriteListsOfElements<T, TSerializer>.WriteEnumerable(null, null);
				WriteListsOfElements<T, TSerializer>.WriteListValueType(null, null);
				WriteListsOfElements<T, TSerializer>.WriteIListValueType(null, null);
				WriteListsOfElements<T, TSerializer>.WriteGenericArrayValueType(null, null);
				WriteListsOfElements<T, TSerializer>.WriteArray(null, null);

				TranslateListWithElements<T>.LateBoundTranslateToGenericICollection(null, null);
				TranslateListWithConvertibleElements<T, T>.LateBoundTranslateToGenericICollection(null, null);
				
				QueryStringWriter<T>.WriteObject(null, null);
				return i;
			}

			internal static void RegisterElement<T, TElement, TSerializer>() where TSerializer : ITypeSerializer
			{
				DeserializeDictionary<TSerializer>.ParseDictionary<T, TElement>(null, null, null, null);
				DeserializeDictionary<TSerializer>.ParseDictionary<TElement, T>(null, null, null, null);
				
				ToStringDictionaryMethods<T, TElement, TSerializer>.WriteIDictionary(null, null, null, null);
				ToStringDictionaryMethods<TElement, T, TSerializer>.WriteIDictionary(null, null, null, null);
				
				// Include List deserialisations from the Register<> method above.  This solves issue where List<Guid> properties on responses deserialise to null.
				// No idea why this is happening because there is no visible exception raised.  Suspect MonoTouch is swallowing an AOT exception somewhere.
				DeserializeArrayWithElements<TElement, TSerializer>.ParseGenericArray(null, null);
				DeserializeListWithElements<TElement, TSerializer>.ParseGenericList(null, null, null);
				
				// Cannot use the line below for some unknown reason - when trying to compile to run on device, mtouch bombs during native code compile.
				// Something about this line or its inner workings is offensive to mtouch. Luckily this was not needed for my List<Guide> issue.
				// DeserializeCollection<JsonTypeSerializer>.ParseCollection<TElement>(null, null, null);
				
				TranslateListWithElements<TElement>.LateBoundTranslateToGenericICollection(null, typeof(List<TElement>));
				TranslateListWithConvertibleElements<TElement, TElement>.LateBoundTranslateToGenericICollection(null, typeof(List<TElement>));
			}
		}

#endif

    }

#if MONOTOUCH
    [MonoTouch.Foundation.Preserve(AllMembers=true)]
    internal class Poco
    {
        public string Dummy { get; set; }
    }
#endif

    public class JsConfig<T>
    {
        /// <summary>
        /// Always emit type info for this type.  Takes precedence over ExcludeTypeInfo
        /// </summary>
        public static bool IncludeTypeInfo = false;

        /// <summary>
        /// Never emit type info for this type
        /// </summary>
        public static bool ExcludeTypeInfo = false;

        /// <summary>
        /// <see langword="true"/> if the <see cref="ITypeSerializer"/> is configured
        /// to take advantage of <see cref="CLSCompliantAttribute"/> specification,
        /// to support user-friendly serialized formats, ie emitting camelCasing for JSON
        /// and parsing member names and enum values in a case-insensitive manner.
        /// </summary>
        public static bool EmitCamelCaseNames = false;

        public static bool EmitLowercaseUnderscoreNames = false;

        /// <summary>
        /// Define custom serialization fn for BCL Structs
        /// </summary>
        private static Func<T, string> serializeFn;
        public static Func<T, string> SerializeFn
        {
            get { return serializeFn; }
            set
            {
                serializeFn = value;
                if (value != null)
                    JsConfig.HasSerializeFn.Add(typeof(T));
                else
                    JsConfig.HasSerializeFn.Remove(typeof(T));
            }
        }

        /// <summary>
        /// Opt-in flag to set some Value Types to be treated as a Ref Type
        /// </summary>
        public static bool TreatValueAsRefType
        {
            get { return JsConfig.TreatValueAsRefTypes.Contains(typeof(T)); }
            set
            {
                if (value)
                    JsConfig.TreatValueAsRefTypes.Add(typeof(T));
                else
                    JsConfig.TreatValueAsRefTypes.Remove(typeof(T));
            }
        }

        /// <summary>
        /// Whether there is a fn (raw or otherwise)
        /// </summary>
        public static bool HasSerializeFn
        {
            get { return serializeFn != null || rawSerializeFn != null; }
        }

        /// <summary>
        /// Define custom raw serialization fn
        /// </summary>
        private static Func<T, string> rawSerializeFn;
        public static Func<T, string> RawSerializeFn
        {
            get { return rawSerializeFn; }
            set
            {
                rawSerializeFn = value;
                if (value != null)
                    JsConfig.HasSerializeFn.Add(typeof(T));
                else
                    JsConfig.HasSerializeFn.Remove(typeof(T));
            }
        }

        /// <summary>
        /// Define custom serialization hook
        /// </summary>
        private static Func<T, T> onSerializingFn;
        public static Func<T, T> OnSerializingFn
        {
            get { return onSerializingFn; }
            set { onSerializingFn = value; }
        }

        /// <summary>
        /// Define custom deserialization fn for BCL Structs
        /// </summary>
        public static Func<string, T> DeSerializeFn;

        /// <summary>
        /// Define custom raw deserialization fn for objects
        /// </summary>
        public static Func<string, T> RawDeserializeFn;

        public static bool HasDeserializeFn
        {
            get { return DeSerializeFn != null || RawDeserializeFn != null; }
        }

        private static Func<T, T> onDeserializedFn;
        public static Func<T, T> OnDeserializedFn
        {
            get { return onDeserializedFn; }
            set { onDeserializedFn = value; }
        }

        /// <summary>
        /// Exclude specific properties of this type from being serialized
        /// </summary>
        public static string[] ExcludePropertyNames;

        public static void WriteFn<TSerializer>(TextWriter writer, object obj)
        {
            if (RawSerializeFn != null)
            {
                writer.Write(RawSerializeFn((T)obj));
            }
            else
            {
                var serializer = JsWriter.GetTypeSerializer<TSerializer>();
                serializer.WriteString(writer, SerializeFn((T)obj));
            }
        }

        public static object ParseFn(string str)
        {
            return DeSerializeFn(str);
        }

        internal static object ParseFn(ITypeSerializer serializer, string str)
        {
            if (RawDeserializeFn != null)
            {
                return RawDeserializeFn(str);
            }
            else
            {
                return DeSerializeFn(serializer.UnescapeString(str));
            }
        }
    }

    public enum JsonPropertyConvention
    {
        /// <summary>
        /// The property names on target types must match property names in the JSON source
        /// </summary>
        ExactMatch,
        /// <summary>
        /// The property names on target types may not match the property names in the JSON source
        /// </summary>
        Lenient
    }

    public enum JsonDateHandler
    {
        TimestampOffset,
        DCJSCompatible,
        ISO8601
    }

    public enum JsonTimeSpanHandler
    {
        /// <summary>
        /// Uses the xsd format like PT15H10M20S
        /// </summary>
        DurationFormat,
        /// <summary>
        /// Uses the standard .net ToString method of the TimeSpan class
        /// </summary>
        StandardFormat
    }
}

