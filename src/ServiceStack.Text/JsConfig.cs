using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using ServiceStack.Text.Common;
using ServiceStack.Text.Json;
using ServiceStack.Text.Jsv;

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
            LicenseUtils.Init();
        }

        // force deterministic initialization of static constructor
        public static void InitStatics() { }

        public static JsConfigScope BeginScope()
        {
            return new JsConfigScope();
        }

        public static JsConfigScope CreateScope(string config, JsConfigScope scope = null)
        {
            if (string.IsNullOrEmpty(config))
                return scope;

            if (scope == null)
                scope = BeginScope();

            var items = config.Split(',');
            foreach (var item in items)
            {
                var parts = item.SplitOnFirst(':');
                var key = parts[0].ToLower();
                var value = parts.Length == 2 ? parts[1].ToLower() : null;
                var boolValue = parts.Length == 1 || (value != "false" && value != "0");

                switch (key)
                {
                    case "cotisd":
                    case "convertobjecttypesintostringdictionary":
                        scope.ConvertObjectTypesIntoStringDictionary = boolValue;
                        break;
                    case "ttpptv":
                    case "trytoparseprimitivetypevalues":
                        scope.TryToParsePrimitiveTypeValues = boolValue;
                        break;
                    case "ttpnt":
                    case "trytoparsenumerictype":
                        scope.TryToParseNumericType = boolValue;
                        break;
                    case "edv":
                    case "excludedefaultvalues":
                        scope.ExcludeDefaultValues = boolValue;
                        break;
                    case "inv":
                    case "includenullvalues":
                        scope.IncludeNullValues = boolValue;
                        break;
                    case "invid":
                    case "includenullvaluesindictionaries":
                        scope.IncludeNullValuesInDictionaries = boolValue;
                        break;
                    case "ide":
                    case "includedefaultenums":
                        scope.IncludeDefaultEnums = boolValue;
                        break;
                    case "eti":
                    case "excludetypeinfo":
                        scope.ExcludeTypeInfo = boolValue;
                        break;
                    case "iti":
                    case "includetypeinfo":
                        scope.IncludeTypeInfo = boolValue;
                        break;
                    case "eccn":
                    case "emitcamelcasenames":
                        scope.EmitCamelCaseNames = boolValue;
                        break;
                    case "elun":
                    case "emitlowercaseunderscorenames":
                        scope.EmitLowercaseUnderscoreNames = boolValue;
                        break;
                    case "pi":
                    case "preferinterfaces":
                        scope.PreferInterfaces = boolValue;
                        break;
                    case "tode":
                    case "throwondeserializationerror":
                        scope.ThrowOnDeserializationError = boolValue;
                        break;
                    case "teai":
                    case "treatenumasinteger":
                        scope.TreatEnumAsInteger = boolValue;
                        break;
                    case "sdtc":
                    case "skipdatetimeconversion":
                        scope.SkipDateTimeConversion = boolValue;
                        break;
                    case "auu":
                    case "alwaysuseutc":
                        scope.AlwaysUseUtc = boolValue;
                        break;
                    case "au":
                    case "assumeutc":
                        scope.AssumeUtc = boolValue;
                        break;
                    case "auo":
                    case "appendutcoffset":
                        scope.AppendUtcOffset = boolValue;
                        break;
                    case "eu":
                    case "escapeunicode":
                        scope.EscapeUnicode = boolValue;
                        break;
                    case "ehc":
                    case "escapehtmlchars":
                        scope.EscapeHtmlChars = boolValue;
                        break;
                    case "ipf":
                    case "includepublicfields":
                        scope.IncludePublicFields = boolValue;
                        break;
                    case "dh":
                    case "datehandler":
                        switch (value)
                        {
                            case "timestampoffset":
                            case "to":
                                scope.DateHandler = DateHandler.TimestampOffset;
                                break;
                            case "dcjsc":
                            case "dcjscompatible":
                                scope.DateHandler = DateHandler.DCJSCompatible;
                                break;
                            case "iso8601":
                                scope.DateHandler = DateHandler.ISO8601;
                                break;
                            case "iso8601do":
                            case "iso8601dateonly":
                                scope.DateHandler = DateHandler.ISO8601DateOnly;
                                break;
                            case "iso8601dt":
                            case "iso8601datetime":
                                scope.DateHandler = DateHandler.ISO8601DateTime;
                                break;
                            case "rfc1123":
                                scope.DateHandler = DateHandler.RFC1123;
                                break;
                            case "ut":
                            case "unixtime":
                                scope.DateHandler = DateHandler.UnixTime;
                                break;
                            case "utm":
                            case "unixtimems":
                                scope.DateHandler = DateHandler.UnixTimeMs;
                                break;
                        }
                        break;
                    case "tsh":
                    case "timespanhandler":
                        switch (value)
                        {
                            case "df":
                            case "durationformat":
                                scope.TimeSpanHandler = TimeSpanHandler.DurationFormat;
                                break;
                            case "sf":
                            case "standardformat":
                                scope.TimeSpanHandler = TimeSpanHandler.StandardFormat;
                                break;
                        }
                        break;
                    case "pc":
                    case "propertyconvention":
                        switch (value)
                        {
                            case "l":
                            case "lenient":
                                scope.PropertyConvention = PropertyConvention.Lenient;
                                break;
                            case "s":
                            case "strict":
                                scope.PropertyConvention = PropertyConvention.Strict;
                                break;
                        }
                        break;
                }
            }

            return scope;
        }

        public static JsConfigScope With(
            bool? convertObjectTypesIntoStringDictionary = null,
            bool? tryToParsePrimitiveTypeValues = null,
            bool? tryToParseNumericType = null,
            ParseAsType? parsePrimitiveFloatingPointTypes = null,
            ParseAsType? parsePrimitiveIntegerTypes = null,
            bool? excludeDefaultValues = null,
            bool? includeNullValues = null,
            bool? includeNullValuesInDictionaries = null,
            bool? includeDefaultEnums = null,
            bool? excludeTypeInfo = null,
            bool? includeTypeInfo = null,
            bool? emitCamelCaseNames = null,
            bool? emitLowercaseUnderscoreNames = null,
            DateHandler? dateHandler = null,
            TimeSpanHandler? timeSpanHandler = null,
            PropertyConvention? propertyConvention = null,
            bool? preferInterfaces = null,
            bool? throwOnDeserializationError = null,
            string typeAttr = null,
            Func<Type, string> typeWriter = null,
            Func<string, Type> typeFinder = null,
            bool? treatEnumAsInteger = null,
            bool? skipDateTimeConversion = null,
            bool? alwaysUseUtc = null,
            bool? assumeUtc = null,
            bool? appendUtcOffset = null,
            bool? escapeUnicode = null,
            bool? includePublicFields = null,
            int? maxDepth = null,
            EmptyCtorFactoryDelegate modelFactory = null,
            string[] excludePropertyReferences = null)
        {
            return new JsConfigScope
            {
                ConvertObjectTypesIntoStringDictionary = convertObjectTypesIntoStringDictionary ?? sConvertObjectTypesIntoStringDictionary,
                TryToParsePrimitiveTypeValues = tryToParsePrimitiveTypeValues ?? sTryToParsePrimitiveTypeValues,
                TryToParseNumericType = tryToParseNumericType ?? sTryToParseNumericType,

                ParsePrimitiveFloatingPointTypes = parsePrimitiveFloatingPointTypes ?? sParsePrimitiveFloatingPointTypes,
                ParsePrimitiveIntegerTypes = parsePrimitiveIntegerTypes ?? sParsePrimitiveIntegerTypes,

                ExcludeDefaultValues = excludeDefaultValues ?? sExcludeDefaultValues,
                IncludeNullValues = includeNullValues ?? sIncludeNullValues,
                IncludeNullValuesInDictionaries = includeNullValuesInDictionaries ?? sIncludeNullValuesInDictionaries,
                IncludeDefaultEnums = includeDefaultEnums ?? sIncludeDefaultEnums,
                ExcludeTypeInfo = excludeTypeInfo ?? sExcludeTypeInfo,
                IncludeTypeInfo = includeTypeInfo ?? sIncludeTypeInfo,
                EmitCamelCaseNames = emitCamelCaseNames ?? sEmitCamelCaseNames,
                EmitLowercaseUnderscoreNames = emitLowercaseUnderscoreNames ?? sEmitLowercaseUnderscoreNames,
                DateHandler = dateHandler ?? sDateHandler,
                TimeSpanHandler = timeSpanHandler ?? sTimeSpanHandler,
                PropertyConvention = propertyConvention ?? sPropertyConvention,
                PreferInterfaces = preferInterfaces ?? sPreferInterfaces,
                ThrowOnDeserializationError = throwOnDeserializationError ?? sThrowOnDeserializationError,
                TypeAttr = typeAttr ?? sTypeAttr,
                TypeWriter = typeWriter ?? sTypeWriter,
                TypeFinder = typeFinder ?? sTypeFinder,
                TreatEnumAsInteger = treatEnumAsInteger ?? sTreatEnumAsInteger,
                SkipDateTimeConversion = skipDateTimeConversion ?? sSkipDateTimeConversion,
                AlwaysUseUtc = alwaysUseUtc ?? sAlwaysUseUtc,
                AssumeUtc = assumeUtc ?? sAssumeUtc,
                AppendUtcOffset = appendUtcOffset ?? sAppendUtcOffset,
                EscapeUnicode = escapeUnicode ?? sEscapeUnicode,
                IncludePublicFields = includePublicFields ?? sIncludePublicFields,
                MaxDepth = maxDepth ?? sMaxDepth,
                ModelFactory = modelFactory ?? ModelFactory,
                ExcludePropertyReferences = excludePropertyReferences ?? sExcludePropertyReferences
            };
        }

        private static bool? sConvertObjectTypesIntoStringDictionary;
        public static bool ConvertObjectTypesIntoStringDictionary
        {
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.ConvertObjectTypesIntoStringDictionary : null)
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
                return (JsConfigScope.Current != null ? JsConfigScope.Current.TryToParsePrimitiveTypeValues : null)
                    ?? sTryToParsePrimitiveTypeValues
                    ?? false;
            }
            set
            {
                if (!sTryToParsePrimitiveTypeValues.HasValue) sTryToParsePrimitiveTypeValues = value;
            }
        }

        private static bool? sTryToParseNumericType;
        public static bool TryToParseNumericType
        {
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.TryToParseNumericType : null)
                    ?? sTryToParseNumericType
                    ?? false;
            }
            set
            {
                if (!sTryToParseNumericType.HasValue) sTryToParseNumericType = value;
            }
        }

        private static ParseAsType? sParsePrimitiveFloatingPointTypes;
        public static ParseAsType ParsePrimitiveFloatingPointTypes
        {
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.ParsePrimitiveFloatingPointTypes : null)
                    ?? sParsePrimitiveFloatingPointTypes
                    ?? ParseAsType.Decimal;
            }
            set
            {
                if (sParsePrimitiveFloatingPointTypes == null) sParsePrimitiveFloatingPointTypes = value;
            }
        }

        private static ParseAsType? sParsePrimitiveIntegerTypes;
        public static ParseAsType ParsePrimitiveIntegerTypes
        {
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.ParsePrimitiveIntegerTypes : null)
                    ?? sParsePrimitiveIntegerTypes
                    ?? ParseAsType.Byte | ParseAsType.SByte | ParseAsType.Int16 | ParseAsType.UInt16 | ParseAsType.Int32 | ParseAsType.UInt32 | ParseAsType.Int64 | ParseAsType.UInt64;
            }
            set
            {
                if (!sParsePrimitiveIntegerTypes.HasValue) sParsePrimitiveIntegerTypes = value;
            }
        }

        private static bool? sExcludeDefaultValues;
        public static bool ExcludeDefaultValues
        {
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.ExcludeDefaultValues : null)
                    ?? sExcludeDefaultValues
                    ?? false;
            }
            set
            {
                if (!sExcludeDefaultValues.HasValue) sExcludeDefaultValues = value;
            }
        }

        private static bool? sIncludeNullValues;
        public static bool IncludeNullValues
        {
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.IncludeNullValues : null)
                    ?? sIncludeNullValues
                    ?? false;
            }
            set
            {
                if (!sIncludeNullValues.HasValue) sIncludeNullValues = value;
            }
        }

        private static bool? sIncludeNullValuesInDictionaries;
        public static bool IncludeNullValuesInDictionaries
        {
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.IncludeNullValuesInDictionaries : null)
                    ?? sIncludeNullValuesInDictionaries
                    ?? false;
            }
            set
            {
                if (!sIncludeNullValuesInDictionaries.HasValue) sIncludeNullValuesInDictionaries = value;
            }
        }

        private static bool? sIncludeDefaultEnums;
        public static bool IncludeDefaultEnums
        {
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.IncludeDefaultEnums : null)
                    ?? sIncludeDefaultEnums
                    ?? true;
            }
            set
            {
                if (!sIncludeDefaultEnums.HasValue) sIncludeDefaultEnums = value;
            }
        }

        private static bool? sTreatEnumAsInteger;
        public static bool TreatEnumAsInteger
        {
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.TreatEnumAsInteger : null)
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
                return (JsConfigScope.Current != null ? JsConfigScope.Current.ExcludeTypeInfo : null)
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
                return (JsConfigScope.Current != null ? JsConfigScope.Current.IncludeTypeInfo : null)
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
                return (JsConfigScope.Current != null ? JsConfigScope.Current.TypeAttr : null)
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
                return (JsConfigScope.Current != null ? JsConfigScope.Current.JsonTypeAttrInObject : null)
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
                return (JsConfigScope.Current != null ? JsConfigScope.Current.JsvTypeAttrInObject : null)
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
                return (JsConfigScope.Current != null ? JsConfigScope.Current.TypeWriter : null)
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
                return (JsConfigScope.Current != null ? JsConfigScope.Current.TypeFinder : null)
                    ?? sTypeFinder
                    ?? AssemblyUtils.FindType;
            }
            set
            {
                if (sTypeFinder == null) sTypeFinder = value;
            }
        }

        private static Func<string, object> sParsePrimitiveFn;
        public static Func<string, object> ParsePrimitiveFn
        {
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.ParsePrimitiveFn : null)
                    ?? sParsePrimitiveFn
                    ?? null;
            }
            set
            {
                if (sParsePrimitiveFn == null) sParsePrimitiveFn = value;
            }
        }

        private static DateHandler? sDateHandler;
        public static DateHandler DateHandler
        {
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.DateHandler : null)
                    ?? sDateHandler
                    ?? DateHandler.TimestampOffset;
            }
            set
            {
                if (!sDateHandler.HasValue) sDateHandler = value;
            }
        }

        /// <summary>
        /// Sets which format to use when serializing TimeSpans
        /// </summary>
        private static TimeSpanHandler? sTimeSpanHandler;
        public static TimeSpanHandler TimeSpanHandler
        {
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.TimeSpanHandler : null)
                    ?? sTimeSpanHandler
                    ?? TimeSpanHandler.DurationFormat;
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
                return (JsConfigScope.Current != null ? JsConfigScope.Current.EmitCamelCaseNames : null)
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
                return (JsConfigScope.Current != null ? JsConfigScope.Current.EmitLowercaseUnderscoreNames : null)
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
        private static PropertyConvention? sPropertyConvention;
        public static PropertyConvention PropertyConvention
        {
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.PropertyConvention : null)
                    ?? sPropertyConvention
                    ?? PropertyConvention.Strict;
            }
            set
            {
                if (!sPropertyConvention.HasValue) sPropertyConvention = value;
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
                return (JsConfigScope.Current != null ? JsConfigScope.Current.ThrowOnDeserializationError : null)
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
                return (JsConfigScope.Current != null ? JsConfigScope.Current.AlwaysUseUtc : null)
                    ?? sAlwaysUseUtc
                    ?? false;
            }
            set
            {
                if (!sAlwaysUseUtc.HasValue) sAlwaysUseUtc = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if the framework should skip automatic <see cref="DateTime"/> conversions.
        /// Dates will be handled literally, any included timezone encoding will be lost and the date will be treaded as DateTimeKind.Local
        /// Utc formatted input will result in DateTimeKind.Utc output. Any input without TZ data will be set DateTimeKind.Unspecified
        /// This will take precedence over other flags like AlwaysUseUtc 
        /// JsConfig.DateHandler = DateHandler.ISO8601 should be used when set true for consistent de/serialization.
        /// </summary>
        private static bool? sSkipDateTimeConversion;
        public static bool SkipDateTimeConversion
        {
            // obeying the use of ThreadStatic, but allowing for setting JsConfig once as is the normal case
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.SkipDateTimeConversion : null)
                    ?? sSkipDateTimeConversion
                    ?? false;
            }
            set
            {
                if (!sSkipDateTimeConversion.HasValue) sSkipDateTimeConversion = value;
            }
        }
        /// <summary>
        /// Gets or sets a value indicating if the framework should always assume <see cref="DateTime"/> is in UTC format if Kind is Unspecified. 
        /// </summary>
        private static bool? sAssumeUtc;
        public static bool AssumeUtc
        {
            // obeying the use of ThreadStatic, but allowing for setting JsConfig once as is the normal case
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.AssumeUtc : null)
                    ?? sAssumeUtc
                    ?? false;
            }
            set
            {
                if (!sAssumeUtc.HasValue) sAssumeUtc = value;
            }
        }

        /// <summary>
        /// Gets or sets whether we should append the Utc offset when we serialize Utc dates. Defaults to no.
        /// Only supported for when the JsConfig.DateHandler == JsonDateHandler.TimestampOffset
        /// </summary>
        private static bool? sAppendUtcOffset;
        public static bool? AppendUtcOffset
        {
            // obeying the use of ThreadStatic, but allowing for setting JsConfig once as is the normal case
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.AppendUtcOffset : null)
                    ?? sAppendUtcOffset
                    ?? null;
            }
            set
            {
                if (sAppendUtcOffset == null) sAppendUtcOffset = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if unicode symbols should be serialized as "\uXXXX".
        /// </summary>
        private static bool? sEscapeUnicode;
        public static bool EscapeUnicode
        {
            // obeying the use of ThreadStatic, but allowing for setting JsConfig once as is the normal case
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.EscapeUnicode : null)
                       ?? sEscapeUnicode
                       ?? false;
            }
            set
            {
                if (!sEscapeUnicode.HasValue) sEscapeUnicode = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if HTML entity chars [&gt; &lt; &amp; = '] should be escaped as "\uXXXX".
        /// </summary>
        private static bool? sEscapeHtmlChars;
        public static bool EscapeHtmlChars
        {
            // obeying the use of ThreadStatic, but allowing for setting JsConfig once as is the normal case
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.EscapeHtmlChars : null)
                       ?? sEscapeHtmlChars
                       ?? false;
            }
            set
            {
                if (!sEscapeHtmlChars.HasValue) sEscapeHtmlChars = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if the framework should call an error handler when
        /// an exception happens during the deserialization.
        /// </summary>
        /// <remarks>Parameters have following meaning in order: deserialized entity, property name, parsed value, property type, caught exception.</remarks>
        private static DeserializationErrorDelegate sOnDeserializationError;
        public static DeserializationErrorDelegate OnDeserializationError
        {
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.OnDeserializationError : null)
                    ?? sOnDeserializationError;
            }
            set { sOnDeserializationError = value; }
        }

        internal static HashSet<Type> HasSerializeFn = new HashSet<Type>();

        internal static HashSet<Type> HasIncludeDefaultValue = new HashSet<Type>();

        public static HashSet<Type> TreatValueAsRefTypes = new HashSet<Type>();

        private static bool? sPreferInterfaces;
        /// <summary>
        /// If set to true, Interface types will be prefered over concrete types when serializing.
        /// </summary>
        public static bool PreferInterfaces
        {
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.PreferInterfaces : null)
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
            return TreatValueAsRefTypes.Contains(valueType.IsGeneric() ? valueType.GenericTypeDefinition() : valueType);
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
        /// Sets the maximum depth to avoid circular dependencies
        /// </summary>
        private static int? sMaxDepth;
        public static int MaxDepth
        {
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.MaxDepth : null)
                    ?? sMaxDepth
                    ?? int.MaxValue;
            }
            set
            {
                if (!sMaxDepth.HasValue) sMaxDepth = value;
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

        private static string[] sExcludePropertyReferences;
        public static string[] ExcludePropertyReferences
        {
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.ExcludePropertyReferences : null)
                       ?? sExcludePropertyReferences;
            }
            set
            {
                if (sExcludePropertyReferences != null) sExcludePropertyReferences = value;
            }
        }

        private static HashSet<Type> sExcludeTypes;
        public static HashSet<Type> ExcludeTypes
        {
            get
            {
                return (JsConfigScope.Current != null ? JsConfigScope.Current.ExcludeTypes : null)
                       ?? sExcludeTypes;
            }
            set
            {
                if (sExcludePropertyReferences != null) sExcludeTypes = value;
            }
        }

        public static string[] IgnoreAttributesNamed
        {
            set { ReflectionExtensions.IgnoreAttributesNamed = value; }
            get { return ReflectionExtensions.IgnoreAttributesNamed; }
        }

        public static HashSet<string> AllowRuntimeTypeWithAttributesNamed { get; set; }

        public static HashSet<string> AllowRuntimeTypeWithInterfacesNamed { get; set; }

        public static HashSet<string> AllowRuntimeTypeInTypesWithNamespaces { get; set; }

        public static Func<Type, bool> AllowRuntimeType { get; set; }

        public static void Reset()
        {
            foreach (var rawSerializeType in HasSerializeFn.ToArray())
            {
                Reset(rawSerializeType);
            }
            foreach (var rawSerializeType in HasIncludeDefaultValue.ToArray())
            {
                Reset(rawSerializeType);
            }
            foreach (var uniqueType in __uniqueTypes.ToArray())
            {
                Reset(uniqueType);
            }

            sModelFactory = ReflectionExtensions.GetConstructorMethodToCache;
            sTryToParsePrimitiveTypeValues = null;
            sTryToParseNumericType = null;
            sConvertObjectTypesIntoStringDictionary = null;
            sExcludeDefaultValues = null;
            sIncludeNullValues = null;
            sIncludeNullValuesInDictionaries = null;
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
            sParsePrimitiveFn = null;
            sTreatEnumAsInteger = null;
            sAlwaysUseUtc = null;
            sAssumeUtc = null;
            sSkipDateTimeConversion = null;
            sAppendUtcOffset = null;
            sEscapeUnicode = null;
            sEscapeHtmlChars = null;
            sOnDeserializationError = null;
            sIncludePublicFields = null;
            HasSerializeFn = new HashSet<Type>();
            HasIncludeDefaultValue = new HashSet<Type>();
            TreatValueAsRefTypes = new HashSet<Type> { typeof(KeyValuePair<,>) };
            sPropertyConvention = null;
            sExcludePropertyReferences = null;
            sExcludeTypes = new HashSet<Type> { typeof(Stream) };
            __uniqueTypes = new HashSet<Type>();
            sMaxDepth = 50;
            sParsePrimitiveIntegerTypes = null;
            sParsePrimitiveFloatingPointTypes = null;
            AllowRuntimeType = null;
            AllowRuntimeTypeWithAttributesNamed = new HashSet<string>
            {
                nameof(DataContractAttribute),
                nameof(RuntimeSerializableAttribute),
                "SerializableAttribute",
            };
            AllowRuntimeTypeWithInterfacesNamed = new HashSet<string>
            {
                "IConvertible",
                "ISerializable",
                "IRuntimeSerializable",
                "IMeta",
                "IReturn`1",
                "IReturnVoid",
            };
            AllowRuntimeTypeInTypesWithNamespaces = new HashSet<string>
            {
                "ServiceStack.Messaging",
            };
            PlatformExtensions.ClearRuntimeAttributes();
            ReflectionExtensions.Reset();
            JsState.Reset();
        }

        static void Reset(Type cachesForType)
        {
            typeof(JsConfig<>).MakeGenericType(new[] { cachesForType }).InvokeReset();
            typeof(TypeConfig<>).MakeGenericType(new[] { cachesForType }).InvokeReset();
        }

        internal static void InvokeReset(this Type genericType)
        {
            var methodInfo = genericType.GetStaticMethod("Reset");
            methodInfo.Invoke(null, null);
        }

        internal static HashSet<Type> __uniqueTypes = new HashSet<Type>();
        internal static int __uniqueTypesCount = 0;

        internal static void AddUniqueType(Type type)
        {
            if (__uniqueTypes.Contains(type))
                return;

            HashSet<Type> newTypes, snapshot;
            do
            {
                snapshot = __uniqueTypes;
                newTypes = new HashSet<Type>(__uniqueTypes) { type };
                __uniqueTypesCount = newTypes.Count;

            } while (!ReferenceEquals(
                Interlocked.CompareExchange(ref __uniqueTypes, newTypes, snapshot), snapshot));
        }
    }

    public class JsConfig<T>
    {
        static JsConfig()
        {
            // Run the type's static constructor (which may set OnDeserialized, etc.) before we cache any information about it
            RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);
        }

        /// <summary>
        /// Always emit type info for this type.  Takes precedence over ExcludeTypeInfo
        /// </summary>
        public static bool? IncludeTypeInfo = null;

        /// <summary>
        /// Never emit type info for this type
        /// </summary>
        public static bool? ExcludeTypeInfo = null;

        /// <summary>
        /// <see langword="true"/> if the <see cref="ITypeSerializer"/> is configured
        /// to take advantage of <see cref="CLSCompliantAttribute"/> specification,
        /// to support user-friendly serialized formats, ie emitting camelCasing for JSON
        /// and parsing member names and enum values in a case-insensitive manner.
        /// </summary>
        public static bool? EmitCamelCaseNames = null;

        public static bool? EmitLowercaseUnderscoreNames = null;

        public static bool IncludeDefaultValue
        {
            get { return JsConfig.HasIncludeDefaultValue.Contains(typeof(T)); }
            set
            {
                if (value)
                    JsConfig.HasIncludeDefaultValue.Add(typeof(T));
                else
                    JsConfig.HasIncludeDefaultValue.Remove(typeof(T));

                ClearFnCaches();
            }
        }

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

                ClearFnCaches();
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
            get { return !JsState.InSerializer<T>() && (serializeFn != null || rawSerializeFn != null); }
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

                ClearFnCaches();
            }
        }

        /// <summary>
        /// Define custom serialization hook
        /// </summary>
        private static Func<T, T> onSerializingFn;
        public static Func<T, T> OnSerializingFn
        {
            get { return onSerializingFn; }
            set { onSerializingFn = value; RefreshWrite(); }
        }

        /// <summary>
        /// Define custom after serialization hook
        /// </summary>
        private static Action<T> onSerializedFn;
        public static Action<T> OnSerializedFn
        {
            get { return onSerializedFn; }
            set { onSerializedFn = value; RefreshWrite(); }
        }

        /// <summary>
        /// Define custom deserialization fn for BCL Structs
        /// </summary>
        private static Func<string, T> deSerializeFn;
        public static Func<string, T> DeSerializeFn
        {
            get { return deSerializeFn; }
            set { deSerializeFn = value; RefreshRead(); }
        }

        /// <summary>
        /// Define custom raw deserialization fn for objects
        /// </summary>
        private static Func<string, T> rawDeserializeFn;
        public static Func<string, T> RawDeserializeFn
        {
            get { return rawDeserializeFn; }
            set { rawDeserializeFn = value; RefreshRead(); }
        }

        public static bool HasDeserializeFn
        {
            get { return !JsState.InDeserializer<T>() && (DeSerializeFn != null || RawDeserializeFn != null); }
        }

        private static Func<T, T> onDeserializedFn;
        public static Func<T, T> OnDeserializedFn
        {
            get { return onDeserializedFn; }
            set { onDeserializedFn = value; RefreshRead(); }
        }

        public static bool HasDeserialingFn
        {
            get { return OnDeserializingFn != null; }
        }

        private static Func<T, string, object, object> onDeserializingFn;
        public static Func<T, string, object, object> OnDeserializingFn
        {
            get { return onDeserializingFn; }
            set { onDeserializingFn = value; RefreshRead(); }
        }

        /// <summary>
        /// Exclude specific properties of this type from being serialized
        /// </summary>
        public static string[] ExcludePropertyNames;

        public static void WriteFn<TSerializer>(TextWriter writer, object obj)
        {
            if (RawSerializeFn != null && !JsState.InSerializer<T>())
            {
                JsState.RegisterSerializer<T>();
                try
                {
                    writer.Write(RawSerializeFn((T)obj));
                }
                finally
                {
                    JsState.UnRegisterSerializer<T>();
                }
            }
            else if (SerializeFn != null && !JsState.InSerializer<T>())
            {
                JsState.RegisterSerializer<T>();
                try
                {
                    var serializer = JsWriter.GetTypeSerializer<TSerializer>();
                    serializer.WriteString(writer, SerializeFn((T)obj));
                }
                finally
                {
                    JsState.UnRegisterSerializer<T>();
                }
            }
            else
            {
                var writerFn = JsonWriter.Instance.GetWriteFn<T>();
                writerFn(writer, obj);
            }
        }

        public static object ParseFn(string str)
        {
            return DeSerializeFn(str);
        }

        internal static object ParseFn(ITypeSerializer serializer, string str)
        {
            if (RawDeserializeFn != null && !JsState.InDeserializer<T>())
            {
                JsState.RegisterDeserializer<T>();
                try
                {
                    return RawDeserializeFn(str);
                }
                finally
                {
                    JsState.UnRegisterDeserializer<T>();
                }
            }
            else if (DeSerializeFn != null && !JsState.InDeserializer<T>())
            {
                JsState.RegisterDeserializer<T>();
                try
                {
                    return DeSerializeFn(serializer.UnescapeString(str));
                }
                finally
                {
                    JsState.UnRegisterDeserializer<T>();
                }
            }
            else
            {
                var parseFn = JsonReader.Instance.GetParseFn<T>();
                return parseFn(str);
            }
        }

        internal static void ClearFnCaches()
        {
            JsonWriter<T>.Reset();
            JsvWriter<T>.Reset();
        }

        public static void Reset()
        {
            RawSerializeFn = null;
            DeSerializeFn = null;
            ExcludePropertyNames = null;
            EmitCamelCaseNames = EmitLowercaseUnderscoreNames = IncludeTypeInfo = ExcludeTypeInfo = null;
        }

        public static void RefreshRead()
        {
            JsonReader<T>.Refresh();
            JsvReader<T>.Refresh();
        }

        public static void RefreshWrite()
        {
            JsonWriter<T>.Refresh();
            JsvWriter<T>.Refresh();
        }
    }

    public enum PropertyConvention
    {
        /// <summary>
        /// The property names on target types must match property names in the JSON source
        /// </summary>
        Strict,
        /// <summary>
        /// The property names on target types may not match the property names in the JSON source
        /// </summary>
        Lenient
    }

    public enum DateHandler
    {
        TimestampOffset,
        DCJSCompatible,
        ISO8601,
        ISO8601DateOnly,
        ISO8601DateTime,
        RFC1123,
        UnixTime,
        UnixTimeMs,
    }

    public enum TimeSpanHandler
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

