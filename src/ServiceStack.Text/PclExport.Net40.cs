﻿#if !(XBOX || SL5 || NETFX_CORE || WP || PCL || NETSTANDARD1_1)
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.Text;
using ServiceStack.Text.Common;
using ServiceStack.Text.Json;
using ServiceStack.Text.Support;

#if !__IOS__
using System.Reflection.Emit;
using FastMember = ServiceStack.Text.FastMember;
#endif

#if __UNIFIED__
using Preserve = Foundation.PreserveAttribute;
#elif __IOS__
using Preserve = MonoTouch.Foundation.PreserveAttribute;
#endif

namespace ServiceStack
{
    public class Net40PclExport : PclExport
    {
        public static Net40PclExport Provider = new Net40PclExport();

        public Net40PclExport()
        {
            this.SupportsEmit = SupportsExpression = true;
            this.DirSep = Path.DirectorySeparatorChar;
            this.AltDirSep = Path.DirectorySeparatorChar == '/' ? '\\' : '/';
            this.RegexOptions = RegexOptions.Compiled;
#if DNXCORE50
            this.InvariantComparison = CultureInfo.InvariantCulture.CompareInfo.GetStringComparer();
            this.InvariantComparisonIgnoreCase = CultureInfo.InvariantCultureIgnoreCase.CompareInfo.GetStringComparer();
#else
            this.InvariantComparison = StringComparison.InvariantCulture;
            this.InvariantComparisonIgnoreCase = StringComparison.InvariantCultureIgnoreCase;
#endif
            this.InvariantComparer = StringComparer.InvariantCulture;
            this.InvariantComparerIgnoreCase = StringComparer.InvariantCultureIgnoreCase;

            this.PlatformName = Environment.OSVersion.Platform.ToString();
        }

        public static PclExport Configure()
        {
            Configure(Provider);
            return Provider;
        }

        public override string ReadAllText(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        public override string ToTitleCase(string value)
        {
            return TextInfo.ToTitleCase(value).Replace("_", String.Empty);
        }

        public override string ToInvariantUpper(char value)
        {
            return value.ToString(CultureInfo.InvariantCulture).ToUpper();
        }

        public override bool IsAnonymousType(Type type)
        {
            return type.HasAttribute<CompilerGeneratedAttribute>()
                   && type.IsGeneric() && type.Name.Contains("AnonymousType")
                   && (type.Name.StartsWith("<>", StringComparison.Ordinal) || type.Name.StartsWith("VB$", StringComparison.Ordinal))
                   && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }

        public override bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public override bool DirectoryExists(string dirPath)
        {
            return Directory.Exists(dirPath);
        }

        public override void CreateDirectory(string dirPath)
        {
            Directory.CreateDirectory(dirPath);
        }

        public override string[] GetFileNames(string dirPath, string searchPattern = null)
        {
            if (!Directory.Exists(dirPath))
                return TypeConstants.EmptyStringArray;

            return searchPattern != null
                ? Directory.GetFiles(dirPath, searchPattern)
                : Directory.GetFiles(dirPath);
        }

        public override string[] GetDirectoryNames(string dirPath, string searchPattern = null)
        {
            if (!Directory.Exists(dirPath))
                return TypeConstants.EmptyStringArray;

            return searchPattern != null
                ? Directory.GetDirectories(dirPath, searchPattern)
                : Directory.GetDirectories(dirPath);
        }

        public const string AppSettingsKey = "servicestack:license";
        public const string EnvironmentKey = "SERVICESTACK_LICENSE";

        public override void RegisterLicenseFromConfig()
        {
#if ANDROID
#elif __IOS__
#elif __MAC__
#else
            string licenceKeyText;
            try
            {
                //Automatically register license key stored in <appSettings/>
                licenceKeyText = System.Configuration.ConfigurationManager.AppSettings[AppSettingsKey];
                if (!string.IsNullOrEmpty(licenceKeyText))
                {
                    LicenseUtils.RegisterLicense(licenceKeyText);
                    return;
                }
            }
            catch (Exception ex)
            {
                licenceKeyText = Environment.GetEnvironmentVariable(EnvironmentKey)?.Trim();
                if (string.IsNullOrEmpty(licenceKeyText))
                    throw;
                try
                {
                    LicenseUtils.RegisterLicense(licenceKeyText);
                }
                catch
                {
                    throw ex;
                }
            }

            //or SERVICESTACK_LICENSE Environment variable
            licenceKeyText = Environment.GetEnvironmentVariable(EnvironmentKey)?.Trim();
            if (!string.IsNullOrEmpty(licenceKeyText))
            {
                LicenseUtils.RegisterLicense(licenceKeyText);
            }
#endif
        }

        public override string GetEnvironmentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name);
        }

        public override void WriteLine(string line)
        {
            Console.WriteLine(line);
        }

        public override void WriteLine(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public override void AddCompression(WebRequest webReq)
        {
            var httpReq = (HttpWebRequest)webReq;
            httpReq.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
            httpReq.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
        }

        public override Stream GetRequestStream(WebRequest webRequest)
        {
            return webRequest.GetRequestStream();
        }

        public override WebResponse GetResponse(WebRequest webRequest)
        {
            return webRequest.GetResponse();
        }

#if !LITE
        public override bool IsDebugBuild(Assembly assembly)
        {
            return assembly.AllAttributes()
                           .OfType<System.Diagnostics.DebuggableAttribute>()
                           .Select(attr => attr.IsJITTrackingEnabled)
                           .FirstOrDefault();
        }
#endif

        public override string MapAbsolutePath(string relativePath, string appendPartialPathModifier)
        {
            if (relativePath.StartsWith("~"))
            {
                var assemblyDirectoryPath = Path.GetDirectoryName(new Uri(typeof(PathUtils).Assembly.EscapedCodeBase).LocalPath);

                // Escape the assembly bin directory to the hostname directory
                var hostDirectoryPath = appendPartialPathModifier != null
                                            ? assemblyDirectoryPath + appendPartialPathModifier
                                            : assemblyDirectoryPath;

                return Path.GetFullPath(relativePath.Replace("~", hostDirectoryPath));
            }
            return relativePath;
        }

        public override Assembly LoadAssembly(string assemblyPath)
        {
            return Assembly.LoadFrom(assemblyPath);
        }

        public override void AddHeader(WebRequest webReq, string name, string value)
        {
            webReq.Headers.Add(name, value);
        }

        public override Assembly[] GetAllAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }

        public override Type FindType(string typeName, string assemblyName)
        {
            var binPath = AssemblyUtils.GetAssemblyBinPath(Assembly.GetExecutingAssembly());
            Assembly assembly = null;
            var assemblyDllPath = binPath + $"{assemblyName}.dll";
            if (File.Exists(assemblyDllPath))
            {
                assembly = AssemblyUtils.LoadAssembly(assemblyDllPath);
            }
            var assemblyExePath = binPath + $"{assemblyName}.exe";
            if (File.Exists(assemblyExePath))
            {
                assembly = AssemblyUtils.LoadAssembly(assemblyExePath);
            }
            return assembly != null ? assembly.GetType(typeName) : null;
        }

        public override string GetAssemblyCodeBase(Assembly assembly)
        {
            return assembly.CodeBase;
        }

        public override string GetAssemblyPath(Type source)
        {
            var assemblyUri = new Uri(source.Assembly.EscapedCodeBase);
            return assemblyUri.LocalPath;
        }

        public override string GetAsciiString(byte[] bytes, int index, int count)
        {
            return Encoding.ASCII.GetString(bytes, index, count);
        }

        public override byte[] GetAsciiBytes(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }

        public override bool InSameAssembly(Type t1, Type t2)
        {
            return t1.GetAssembly() == t2.GetAssembly();
        }

        public override Type GetGenericCollectionType(Type type)
        {
            return type.FindInterfaces((t, critera) =>
                t.IsGenericType
                && t.GetGenericTypeDefinition() == typeof(ICollection<>), null).FirstOrDefault();
        }

        public override GetMemberDelegate CreateGetter(PropertyInfo propertyInfo)
        {
            return
#if NET45
                SupportsEmit ? PropertyInvoker.GetEmit(propertyInfo) :
#endif
                SupportsExpression
                    ? PropertyInvoker.GetExpression(propertyInfo)
                    : base.CreateGetter(propertyInfo);
        }

        public override GetMemberDelegate<T> CreateGetter<T>(PropertyInfo propertyInfo)
        {
            return
#if NET45
                SupportsEmit ? PropertyInvoker.GetEmit<T>(propertyInfo) :
#endif
                    SupportsExpression
                        ? PropertyInvoker.GetExpression<T>(propertyInfo)
                        : base.CreateGetter<T>(propertyInfo);
        }

        public override SetMemberDelegate CreateSetter(PropertyInfo propertyInfo)
        {
            return
#if NET45
                SupportsEmit ? PropertyInvoker.SetEmit(propertyInfo) :
#endif
                SupportsExpression
                    ? PropertyInvoker.SetExpression(propertyInfo)
                    : base.CreateSetter(propertyInfo);
        }

        public override SetMemberDelegate<T> CreateSetter<T>(PropertyInfo propertyInfo)
        {
            return SupportsExpression
                ? PropertyInvoker.SetExpression<T>(propertyInfo)
                : base.CreateSetter<T>(propertyInfo);
        }

        public override GetMemberDelegate CreateGetter(FieldInfo fieldInfo)
        {
            return
#if NET45
                SupportsEmit ? FieldInvoker.GetEmit(fieldInfo) :
#endif
                SupportsExpression
                    ? FieldInvoker.GetExpression(fieldInfo)
                    : base.CreateGetter(fieldInfo);
        }

        public override GetMemberDelegate<T> CreateGetter<T>(FieldInfo fieldInfo)
        {
            return
#if NET45
                SupportsEmit ? FieldInvoker.GetEmit<T>(fieldInfo) :
#endif
                SupportsExpression
                    ? FieldInvoker.GetExpression<T>(fieldInfo)
                    : base.CreateGetter<T>(fieldInfo);
        }

        public override SetMemberDelegate CreateSetter(FieldInfo fieldInfo)
        {
            return
#if NET45
                SupportsEmit ? FieldInvoker.SetEmit(fieldInfo) :
#endif
                SupportsExpression
                    ? FieldInvoker.SetExpression(fieldInfo)
                    : base.CreateSetter(fieldInfo);
        }

        public override SetMemberDelegate<T> CreateSetter<T>(FieldInfo fieldInfo)
        {
            return SupportsExpression
                ? FieldInvoker.SetExpression<T>(fieldInfo)
                : base.CreateSetter<T>(fieldInfo);
        }

        public override string ToXsdDateTimeString(DateTime dateTime)
        {
#if !LITE
            return System.Xml.XmlConvert.ToString(dateTime.ToStableUniversalTime(), System.Xml.XmlDateTimeSerializationMode.Utc);
#else
            return dateTime.ToStableUniversalTime().ToString(DateTimeSerializer.XsdDateTimeFormat);
#endif
        }

        public override string ToLocalXsdDateTimeString(DateTime dateTime)
        {
#if !LITE
            return System.Xml.XmlConvert.ToString(dateTime, System.Xml.XmlDateTimeSerializationMode.Local);
#else
            return dateTime.ToString(DateTimeSerializer.XsdDateTimeFormat);
#endif
        }

        public override DateTime ParseXsdDateTime(string dateTimeStr)
        {
#if !LITE
            return System.Xml.XmlConvert.ToDateTime(dateTimeStr, System.Xml.XmlDateTimeSerializationMode.Utc);
#else
            return DateTime.ParseExact(dateTimeStr, DateTimeSerializer.XsdDateTimeFormat, CultureInfo.InvariantCulture);
#endif
        }

#if !LITE
        public override DateTime ParseXsdDateTimeAsUtc(string dateTimeStr)
        {
            return System.Xml.XmlConvert.ToDateTime(dateTimeStr, System.Xml.XmlDateTimeSerializationMode.Utc).Prepare(parsedAsUtc: true);
        }
#endif

        public override DateTime ToStableUniversalTime(DateTime dateTime)
        {
            // .Net 2.0 - 3.5 has an issue with DateTime.ToUniversalTime, but works ok with TimeZoneInfo.ConvertTimeToUtc.
            // .Net 4.0+ does this under the hood anyway.
            return TimeZoneInfo.ConvertTimeToUtc(dateTime);
        }

        public override ParseStringDelegate GetDictionaryParseMethod<TSerializer>(Type type)
        {
            if (type == typeof(Hashtable))
            {
                return SerializerUtils<TSerializer>.ParseHashtable;
            }
            return null;
        }

        public override ParseStringSegmentDelegate GetDictionaryParseStringSegmentMethod<TSerializer>(Type type)
        {
            if (type == typeof(Hashtable))
            {
                return SerializerUtils<TSerializer>.ParseHashtable;
            }
            return null;
        }

        public override ParseStringDelegate GetSpecializedCollectionParseMethod<TSerializer>(Type type)
        {
            if (type == typeof(StringCollection))
            {
                return SerializerUtils<TSerializer>.ParseStringCollection<TSerializer>;
            }
            return null;
        }

        public override ParseStringSegmentDelegate GetSpecializedCollectionParseStringSegmentMethod<TSerializer>(Type type)
        {
            if (type == typeof(StringCollection))
            {
                return SerializerUtils<TSerializer>.ParseStringCollection<TSerializer>;
            }
            return null;
        }


        public override ParseStringDelegate GetJsReaderParseMethod<TSerializer>(Type type)
        {
#if !(__IOS__ || LITE)
            if (type.AssignableFrom(typeof(System.Dynamic.IDynamicMetaObjectProvider)) ||
                type.HasInterface(typeof(System.Dynamic.IDynamicMetaObjectProvider)))
            {
                return DeserializeDynamic<TSerializer>.Parse;
            }
#endif
            return null;
        }

        public override ParseStringSegmentDelegate GetJsReaderParseStringSegmentMethod<TSerializer>(Type type)
        {
#if !(__IOS__ || LITE)
            if (type.AssignableFrom(typeof(System.Dynamic.IDynamicMetaObjectProvider)) ||
                type.HasInterface(typeof(System.Dynamic.IDynamicMetaObjectProvider)))
            {
                return DeserializeDynamic<TSerializer>.ParseStringSegment;
            }
#endif
            return null;
        }

        public override void InitHttpWebRequest(HttpWebRequest httpReq,
            long? contentLength = null, bool allowAutoRedirect = true, bool keepAlive = true)
        {
            httpReq.UserAgent = Env.ServerUserAgent;
            httpReq.AllowAutoRedirect = allowAutoRedirect;
            httpReq.KeepAlive = keepAlive;

            if (contentLength != null)
            {
                httpReq.ContentLength = contentLength.Value;
            }
        }

        public override void CloseStream(Stream stream)
        {
            stream.Close();
        }

        public override LicenseKey VerifyLicenseKeyText(string licenseKeyText)
        {
            LicenseKey key;
            if (!licenseKeyText.VerifyLicenseKeyText(out key))
                throw new ArgumentException("licenseKeyText");

            return key;
        }

        public override void BeginThreadAffinity()
        {
            Thread.BeginThreadAffinity();
        }

        public override void EndThreadAffinity()
        {
            Thread.EndThreadAffinity();
        }

        public override void Config(HttpWebRequest req,
            bool? allowAutoRedirect = null,
            TimeSpan? timeout = null,
            TimeSpan? readWriteTimeout = null,
            string userAgent = null,
            bool? preAuthenticate = null)
        {
            req.MaximumResponseHeadersLength = int.MaxValue; //throws "The message length limit was exceeded" exception
            if (allowAutoRedirect.HasValue) req.AllowAutoRedirect = allowAutoRedirect.Value;
            if (readWriteTimeout.HasValue) req.ReadWriteTimeout = (int)readWriteTimeout.Value.TotalMilliseconds;
            if (timeout.HasValue) req.Timeout = (int)timeout.Value.TotalMilliseconds;
            if (userAgent != null) req.UserAgent = userAgent;
            if (preAuthenticate.HasValue) req.PreAuthenticate = preAuthenticate.Value;
        }

        public override void SetUserAgent(HttpWebRequest httpReq, string value)
        {
            httpReq.UserAgent = value;
        }

        public override void SetContentLength(HttpWebRequest httpReq, long value)
        {
            httpReq.ContentLength = value;
        }

        public override void SetAllowAutoRedirect(HttpWebRequest httpReq, bool value)
        {
            httpReq.AllowAutoRedirect = value;
        }

        public override void SetKeepAlive(HttpWebRequest httpReq, bool value)
        {
            httpReq.KeepAlive = value;
        }

        public override string GetStackTrace()
        {
            return Environment.StackTrace;
        }

#if !__IOS__
        public override Type UseType(Type type)
        {
            if (type.IsInterface || type.IsAbstract)
            {
                return DynamicProxy.GetInstanceFor(type).GetType();
            }
            return type;
        }

        public override DataContractAttribute GetWeakDataContract(Type type)
        {
            return type.GetWeakDataContract();
        }

        public override DataMemberAttribute GetWeakDataMember(PropertyInfo pi)
        {
            return pi.GetWeakDataMember();
        }

        public override DataMemberAttribute GetWeakDataMember(FieldInfo pi)
        {
            return pi.GetWeakDataMember();
        }
#endif
    }

#if __MAC__
	public class MacPclExport : IosPclExport 
	{
		public static new MacPclExport Provider = new MacPclExport();

		public MacPclExport()
		{
			PlatformName = "MAC";
			SupportsEmit = SupportsExpression = true;
		}
		
		public new static void Configure()
		{
			Configure(Provider);
		}
	}
#endif

#if NET45 || NETFX_CORE
    public class Net45PclExport : Net40PclExport
    {
        public static new Net45PclExport Provider = new Net45PclExport();

        public Net45PclExport()
        {
            PlatformName = "NET45 " + Environment.OSVersion.Platform.ToString();
        }

        public new static void Configure()
        {
            Configure(Provider);
        }

        public override Task WriteAndFlushAsync(Stream stream, byte[] bytes)
        {
            return stream.WriteAsync(bytes, 0, bytes.Length)
                .ContinueWith(t => stream.FlushAsync());
        }
    }
#endif

#if __IOS__ || __MAC__
    [Preserve(AllMembers = true)]
    internal class Poco
    {
        public string Dummy { get; set; }
    }

    public class IosPclExport : Net40PclExport
    {
        public static new IosPclExport Provider = new IosPclExport();

        public IosPclExport()
        {
            PlatformName = "IOS";
            SupportsEmit = SupportsExpression = false;
        }

        public new static void Configure()
        {
            Configure(Provider);
        }

        public override void ResetStream(Stream stream)
        {
            // MonoTouch throws NotSupportedException when setting System.Net.WebConnectionStream.Position
            // Not sure if the stream is used later though, so may have to copy to MemoryStream and
            // pass that around instead after this point?
        }

        /// <summary>
        /// Provide hint to IOS AOT compiler to pre-compile generic classes for all your DTOs.
        /// Just needs to be called once in a static constructor.
        /// </summary>
        [Preserve]
        public static void InitForAot()
        {
        }

        [Preserve]
        public override void RegisterForAot()
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
            RegisterTypeForAot<DateTimeOffset>();

            RegisterTypeForAot<Guid?>();
            RegisterTypeForAot<TimeSpan?>();
            RegisterTypeForAot<DateTime?>();
            RegisterTypeForAot<DateTimeOffset?>();
        }

        [Preserve]
        public static void RegisterTypeForAot<T>()
        {
            AotConfig.RegisterSerializers<T>();
        }

        [Preserve]
        public static void RegisterQueryStringWriter()
        {
            var i = 0;
            if (QueryStringWriter<Poco>.WriteFn() != null) i++;
        }

        [Preserve]
        public static int RegisterElement<T, TElement>()
        {
            var i = 0;
            i += AotConfig.RegisterSerializers<TElement>();
            AotConfig.RegisterElement<T, TElement, JsonTypeSerializer>();
            AotConfig.RegisterElement<T, TElement, Text.Jsv.JsvTypeSerializer>();
            return i;
        }

        ///<summary>
        /// Class contains Ahead-of-Time (AOT) explicit class declarations which is used only to workaround "-aot-only" exceptions occured on device only. 
        /// </summary>			
        [Preserve(AllMembers = true)]
        internal class AotConfig
        {
            internal static JsReader<JsonTypeSerializer> jsonReader;
            internal static JsWriter<JsonTypeSerializer> jsonWriter;
            internal static JsReader<Text.Jsv.JsvTypeSerializer> jsvReader;
            internal static JsWriter<Text.Jsv.JsvTypeSerializer> jsvWriter;
            internal static JsonTypeSerializer jsonSerializer;
            internal static Text.Jsv.JsvTypeSerializer jsvSerializer;

            static AotConfig()
            {
                jsonSerializer = new JsonTypeSerializer();
                jsvSerializer = new Text.Jsv.JsvTypeSerializer();
                jsonReader = new JsReader<JsonTypeSerializer>();
                jsonWriter = new JsWriter<JsonTypeSerializer>();
                jsvReader = new JsReader<Text.Jsv.JsvTypeSerializer>();
                jsvWriter = new JsWriter<Text.Jsv.JsvTypeSerializer>();
            }

            internal static int RegisterSerializers<T>()
            {
                var i = 0;
                i += Register<T, JsonTypeSerializer>();
                if (jsonSerializer.GetParseFn<T>() != null) i++;
                if (jsonSerializer.GetWriteFn<T>() != null) i++;
                if (jsonReader.GetParseFn<T>() != null) i++;
                if (jsonWriter.GetWriteFn<T>() != null) i++;

                i += Register<T, Text.Jsv.JsvTypeSerializer>();
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
                // No idea why this is happening because there is no visible exception raised.  Suspect IOS is swallowing an AOT exception somewhere.
                DeserializeArrayWithElements<TElement, TSerializer>.ParseGenericArray(null, null);
                DeserializeListWithElements<TElement, TSerializer>.ParseGenericList(null, null, null);

                // Cannot use the line below for some unknown reason - when trying to compile to run on device, mtouch bombs during native code compile.
                // Something about this line or its inner workings is offensive to mtouch. Luckily this was not needed for my List<Guide> issue.
                // DeserializeCollection<JsonTypeSerializer>.ParseCollection<TElement>(null, null, null);

                TranslateListWithElements<TElement>.LateBoundTranslateToGenericICollection(null, typeof(List<TElement>));
                TranslateListWithConvertibleElements<TElement, TElement>.LateBoundTranslateToGenericICollection(null, typeof(List<TElement>));
            }
        }
    }
#endif

#if ANDROID
    public class AndroidPclExport : Net40PclExport
    {
        public static new AndroidPclExport Provider = new AndroidPclExport();

        public AndroidPclExport()
        {
            PlatformName = "Android";
        }

        public new static void Configure()
        {
            Configure(Provider);
        }
    }
#endif

    internal class SerializerUtils<TSerializer>
        where TSerializer : ITypeSerializer
    {
        private static readonly ITypeSerializer Serializer = JsWriter.GetTypeSerializer<TSerializer>();

        private static int VerifyAndGetStartIndex(StringSegment value, Type createMapType)
        {
            var index = 0;
            if (!Serializer.EatMapStartChar(value, ref index))
            {
                //Don't throw ex because some KeyValueDataContractDeserializer don't have '{}'
                Tracer.Instance.WriteDebug("WARN: Map definitions should start with a '{0}', expecting serialized type '{1}', got string starting with: {2}",
                    JsWriter.MapStartChar, createMapType != null ? createMapType.Name : "Dictionary<,>", value.Substring(0, value.Length < 50 ? value.Length : 50));
            }
            return index;
        }

        public static Hashtable ParseHashtable(string value) => ParseHashtable(new StringSegment(value));

        public static Hashtable ParseHashtable(StringSegment value)
        {
            if (!value.HasValue)
                return null;

            var index = VerifyAndGetStartIndex(value, typeof(Hashtable));

            var result = new Hashtable();

            if (JsonTypeSerializer.IsEmptyMap(value, index)) return result;

            var valueLength = value.Length;
            while (index < valueLength)
            {
                var keyValue = Serializer.EatMapKey(value, ref index);
                Serializer.EatMapKeySeperator(value, ref index);
                var elementValue = Serializer.EatValue(value, ref index);
                if (!keyValue.HasValue) continue;

                var mapKey = keyValue.Value;
                var mapValue = elementValue.Value;

                result[mapKey] = mapValue;

                Serializer.EatItemSeperatorOrMapEndChar(value, ref index);
            }

            return result;
        }

        public static StringCollection ParseStringCollection<TS>(string value) where TS : ITypeSerializer => ParseStringCollection<TS>(new StringSegment(value));


        public static StringCollection ParseStringCollection<TS>(StringSegment value) where TS : ITypeSerializer
        {
            if ((value = DeserializeListWithElements<TS>.StripList(value)) == null) return null;
            return value.Length == 0
                   ? new StringCollection()
                   : ToStringCollection(DeserializeListWithElements<TSerializer>.ParseStringList(value));
        }

        public static StringCollection ToStringCollection(List<string> items)
        {
            var to = new StringCollection();
            foreach (var item in items)
            {
                to.Add(item);
            }
            return to;
        }
    }

    public static class PclExportExt
    {
        //HttpUtils
        public static WebResponse PostFileToUrl(this string url,
            FileInfo uploadFileInfo, string uploadFileMimeType,
            string accept = null,
            Action<HttpWebRequest> requestFilter = null)
        {
            var webReq = (HttpWebRequest)WebRequest.Create(url);
            using (var fileStream = uploadFileInfo.OpenRead())
            {
                var fileName = uploadFileInfo.Name;

                webReq.UploadFile(fileStream, fileName, uploadFileMimeType, accept: accept, requestFilter: requestFilter, method: "POST");
            }

            if (HttpUtils.ResultsFilter != null)
                return null;

            return webReq.GetResponse();
        }

        public static WebResponse PutFileToUrl(this string url,
            FileInfo uploadFileInfo, string uploadFileMimeType,
            string accept = null,
            Action<HttpWebRequest> requestFilter = null)
        {
            var webReq = (HttpWebRequest)WebRequest.Create(url);
            using (var fileStream = uploadFileInfo.OpenRead())
            {
                var fileName = uploadFileInfo.Name;

                webReq.UploadFile(fileStream, fileName, uploadFileMimeType, accept: accept, requestFilter: requestFilter, method: "PUT");
            }

            if (HttpUtils.ResultsFilter != null)
                return null;

            return webReq.GetResponse();
        }

        public static WebResponse UploadFile(this WebRequest webRequest,
            FileInfo uploadFileInfo, string uploadFileMimeType)
        {
            using (var fileStream = uploadFileInfo.OpenRead())
            {
                var fileName = uploadFileInfo.Name;

                webRequest.UploadFile(fileStream, fileName, uploadFileMimeType);
            }

            if (HttpUtils.ResultsFilter != null)
                return null;

            return webRequest.GetResponse();
        }

        //XmlSerializer
        public static void CompressToStream<TXmlDto>(TXmlDto from, Stream stream)
        {
#if __IOS__ || ANDROID
            throw new NotImplementedException("Compression is not supported on this platform");
#else
            using (var deflateStream = new System.IO.Compression.DeflateStream(stream, System.IO.Compression.CompressionMode.Compress))
            using (var xw = new System.Xml.XmlTextWriter(deflateStream, Encoding.UTF8))
            {
                var serializer = new DataContractSerializer(from.GetType());
                serializer.WriteObject(xw, from);
                xw.Flush();
            }
#endif
        }

        public static byte[] Compress<TXmlDto>(TXmlDto from)
        {
            using (var ms = MemoryStreamFactory.GetStream())
            {
                CompressToStream(from, ms);

                return ms.ToArray();
            }
        }

        //License Utils
        public static bool VerifySignedHash(byte[] DataToVerify, byte[] SignedData, RSAParameters Key)
        {
            try
            {
                var RSAalg = new RSACryptoServiceProvider();
                RSAalg.ImportParameters(Key);
                return RSAalg.VerifySha1Data(DataToVerify, SignedData);

            }
            catch (CryptographicException ex)
            {
                Tracer.Instance.WriteError(ex);
                return false;
            }
        }

        public static bool VerifyLicenseKeyText(this string licenseKeyText, out LicenseKey key)
        {
            var publicRsaProvider = new RSACryptoServiceProvider();
            publicRsaProvider.FromXmlString(LicenseUtils.LicensePublicKey);
            var publicKeyParams = publicRsaProvider.ExportParameters(false);

            key = licenseKeyText.ToLicenseKey();
            var originalData = key.GetHashKeyToSign().ToUtf8Bytes();
            var signedData = Convert.FromBase64String(key.Hash);

            return VerifySignedHash(originalData, signedData, publicKeyParams);
        }

        public static bool VerifyLicenseKeyTextFallback(this string licenseKeyText, out LicenseKey key)
        {
            RSAParameters publicKeyParams;
            try
            {
                var publicRsaProvider = new RSACryptoServiceProvider();
                publicRsaProvider.FromXmlString(LicenseUtils.LicensePublicKey);
                publicKeyParams = publicRsaProvider.ExportParameters(false);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not import LicensePublicKey", ex);
            }

            try
            {
                key = licenseKeyText.ToLicenseKeyFallback();
            }
            catch (Exception ex)
            {
                throw new Exception("Could not deserialize LicenseKeyText Manually", ex);
            }

            byte[] originalData;
            byte[] signedData;

            try
            {
                originalData = key.GetHashKeyToSign().ToUtf8Bytes();
            }
            catch (Exception ex)
            {
                throw new Exception("Could not convert HashKey to UTF-8", ex);
            }

            try
            {
                signedData = Convert.FromBase64String(key.Hash);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not convert key.Hash from Base64", ex);
            }

            try
            {
                return VerifySignedHash(originalData, signedData, publicKeyParams);
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not Verify License Key ({originalData.Length}, {signedData.Length})", ex);
            }
        }

        public static bool VerifySha1Data(this RSACryptoServiceProvider RSAalg, byte[] unsignedData, byte[] encryptedData)
        {
            using (var sha = new SHA1CryptoServiceProvider())
            {
                return RSAalg.VerifyData(unsignedData, sha, encryptedData);
            }
            //SL5 || WP
            //return RSAalg.VerifyData(unsignedData, encryptedData, new EMSAPKCS1v1_5_SHA1()); 
        }

#if !__IOS__
        //ReflectionExtensions
        const string DataContract = "DataContractAttribute";

        static readonly ConcurrentDictionary<Type, FastMember.TypeAccessor> typeAccessorMap
            = new ConcurrentDictionary<Type, FastMember.TypeAccessor>();

        public static DataContractAttribute GetWeakDataContract(this Type type)
        {
            var attr = type.AllAttributes().FirstOrDefault(x => x.GetType().Name == DataContract);
            if (attr != null)
            {
                var attrType = attr.GetType();

                FastMember.TypeAccessor accessor;
                if (!typeAccessorMap.TryGetValue(attrType, out accessor))
                    typeAccessorMap[attrType] = accessor = FastMember.TypeAccessor.Create(attr.GetType());

                return new DataContractAttribute
                {
                    Name = (string)accessor[attr, "Name"],
                    Namespace = (string)accessor[attr, "Namespace"],
                };
            }
            return null;
        }

        public static DataMemberAttribute GetWeakDataMember(this PropertyInfo pi)
        {
            var attr = pi.AllAttributes().FirstOrDefault(x => x.GetType().Name == ReflectionExtensions.DataMember);
            if (attr != null)
            {
                var attrType = attr.GetType();

                FastMember.TypeAccessor accessor;
                if (!typeAccessorMap.TryGetValue(attrType, out accessor))
                    typeAccessorMap[attrType] = accessor = FastMember.TypeAccessor.Create(attr.GetType());

                var newAttr = new DataMemberAttribute
                {
                    Name = (string)accessor[attr, "Name"],
                    EmitDefaultValue = (bool)accessor[attr, "EmitDefaultValue"],
                    IsRequired = (bool)accessor[attr, "IsRequired"],
                };

                var order = (int)accessor[attr, "Order"];
                if (order >= 0)
                    newAttr.Order = order; //Throws Exception if set to -1

                return newAttr;
            }
            return null;
        }

        public static DataMemberAttribute GetWeakDataMember(this FieldInfo pi)
        {
            var attr = pi.AllAttributes().FirstOrDefault(x => x.GetType().Name == ReflectionExtensions.DataMember);
            if (attr != null)
            {
                var attrType = attr.GetType();

                FastMember.TypeAccessor accessor;
                if (!typeAccessorMap.TryGetValue(attrType, out accessor))
                    typeAccessorMap[attrType] = accessor = FastMember.TypeAccessor.Create(attr.GetType());

                var newAttr = new DataMemberAttribute
                {
                    Name = (string)accessor[attr, "Name"],
                    EmitDefaultValue = (bool)accessor[attr, "EmitDefaultValue"],
                    IsRequired = (bool)accessor[attr, "IsRequired"],
                };

                var order = (int)accessor[attr, "Order"];
                if (order >= 0)
                    newAttr.Order = order; //Throws Exception if set to -1

                return newAttr;
            }
            return null;
        }
#endif
    }
}

#if !__IOS__

//Not using it here, but @marcgravell's stuff is too good not to include
// http://code.google.com/p/fast-member/ Apache License 2.0
namespace ServiceStack.Text.FastMember
{
    /// <summary>
    /// Represents an individual object, allowing access to members by-name
    /// </summary>
    public abstract class ObjectAccessor
    {
        /// <summary>
        /// Get or Set the value of a named member for the underlying object
        /// </summary>
        public abstract object this[string name] { get; set; }
        /// <summary>
        /// The object represented by this instance
        /// </summary>
        public abstract object Target { get; }
        /// <summary>
        /// Use the target types definition of equality
        /// </summary>
        public override bool Equals(object obj)
        {
            return Target.Equals(obj);
        }
        /// <summary>
        /// Obtain the hash of the target object
        /// </summary>
        public override int GetHashCode()
        {
            return Target.GetHashCode();
        }
        /// <summary>
        /// Use the target's definition of a string representation
        /// </summary>
        public override string ToString()
        {
            return Target.ToString();
        }

        /// <summary>
        /// Wraps an individual object, allowing by-name access to that instance
        /// </summary>
        public static ObjectAccessor Create(object target)
        {
            if (target == null) throw new ArgumentNullException("target");
            //IDynamicMetaObjectProvider dlr = target as IDynamicMetaObjectProvider;
            //if (dlr != null) return new DynamicWrapper(dlr); // use the DLR
            return new TypeAccessorWrapper(target, TypeAccessor.Create(target.GetType()));
        }

        sealed class TypeAccessorWrapper : ObjectAccessor
        {
            private readonly object target;
            private readonly TypeAccessor accessor;
            public TypeAccessorWrapper(object target, TypeAccessor accessor)
            {
                this.target = target;
                this.accessor = accessor;
            }
            public override object this[string name]
            {
                get { return accessor[target, name.ToUpperInvariant()]; }
                set { accessor[target, name.ToUpperInvariant()] = value; }
            }
            public override object Target
            {
                get { return target; }
            }
        }

        //sealed class DynamicWrapper : ObjectAccessor
        //{
        //    private readonly IDynamicMetaObjectProvider target;
        //    public override object Target
        //    {
        //        get { return target; }
        //    }
        //    public DynamicWrapper(IDynamicMetaObjectProvider target)
        //    {
        //        this.target = target;
        //    }
        //    public override object this[string name]
        //    {
        //        get { return CallSiteCache.GetValue(name, target); }
        //        set { CallSiteCache.SetValue(name, target, value); }
        //    }
        //}
    }

    /// <summary>
    /// Provides by-name member-access to objects of a given type
    /// </summary>
    public abstract class TypeAccessor
    {
        // hash-table has better read-without-locking semantics than dictionary
        private static readonly Hashtable typeLookyp = new Hashtable();

        /// <summary>
        /// Does this type support new instances via a parameterless constructor?
        /// </summary>
        public virtual bool CreateNewSupported => false;

        /// <summary>
        /// Create a new instance of this type
        /// </summary>
        public virtual object CreateNew() { throw new NotSupportedException(); }

        /// <summary>
        /// Provides a type-specific accessor, allowing by-name access for all objects of that type
        /// </summary>
        /// <remarks>The accessor is cached internally; a pre-existing accessor may be returned</remarks>
        public static TypeAccessor Create(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            var obj = (TypeAccessor)typeLookyp[type];
            if (obj != null) return obj;

            lock (typeLookyp)
            {
                // double-check
                obj = (TypeAccessor)typeLookyp[type];
                if (obj != null) return obj;

                obj = CreateNew(type);

                typeLookyp[type] = obj;
                return obj;
            }
        }

        //sealed class DynamicAccessor : TypeAccessor
        //{
        //    public static readonly DynamicAccessor Singleton = new DynamicAccessor();
        //    private DynamicAccessor(){}
        //    public override object this[object target, string name]
        //    {
        //        get { return CallSiteCache.GetValue(name, target); }
        //        set { CallSiteCache.SetValue(name, target, value); }
        //    }
        //}

        private static AssemblyBuilder assembly;
        private static ModuleBuilder module;
        private static int counter;

        private static void WriteGetter(ILGenerator il, Type type, PropertyInfo[] props, FieldInfo[] fields, bool isStatic)
        {
            LocalBuilder loc = type.IsValueType ? il.DeclareLocal(type) : null;
            OpCode propName = isStatic ? OpCodes.Ldarg_1 : OpCodes.Ldarg_2, target = isStatic ? OpCodes.Ldarg_0 : OpCodes.Ldarg_1;
            foreach (PropertyInfo prop in props)
            {
                if (prop.GetIndexParameters().Length != 0 || !prop.CanRead) continue;
                var getFn = prop.GetGetMethod();
                if (getFn == null) continue; //Mono

                Label next = il.DefineLabel();
                il.Emit(propName);
                il.Emit(OpCodes.Ldstr, prop.Name);
                il.EmitCall(OpCodes.Call, strinqEquals, null);
                il.Emit(OpCodes.Brfalse_S, next);
                // match:
                il.Emit(target);
                Cast(il, type, loc);
                il.EmitCall(type.IsValueType ? OpCodes.Call : OpCodes.Callvirt, getFn, null);
                if (prop.PropertyType.IsValueType)
                {
                    il.Emit(OpCodes.Box, prop.PropertyType);
                }
                il.Emit(OpCodes.Ret);
                // not match:
                il.MarkLabel(next);
            }
            foreach (FieldInfo field in fields)
            {
                Label next = il.DefineLabel();
                il.Emit(propName);
                il.Emit(OpCodes.Ldstr, field.Name);
                il.EmitCall(OpCodes.Call, strinqEquals, null);
                il.Emit(OpCodes.Brfalse_S, next);
                // match:
                il.Emit(target);
                Cast(il, type, loc);
                il.Emit(OpCodes.Ldfld, field);
                if (field.FieldType.IsValueType)
                {
                    il.Emit(OpCodes.Box, field.FieldType);
                }
                il.Emit(OpCodes.Ret);
                // not match:
                il.MarkLabel(next);
            }
            il.Emit(OpCodes.Ldstr, "name");
            il.Emit(OpCodes.Newobj, typeof(ArgumentOutOfRangeException).GetConstructor(new Type[] { typeof(string) }));
            il.Emit(OpCodes.Throw);
        }
        private static void WriteSetter(ILGenerator il, Type type, PropertyInfo[] props, FieldInfo[] fields, bool isStatic)
        {
            if (type.IsValueType)
            {
                il.Emit(OpCodes.Ldstr, "Write is not supported for structs");
                il.Emit(OpCodes.Newobj, typeof(NotSupportedException).GetConstructor(new Type[] { typeof(string) }));
                il.Emit(OpCodes.Throw);
            }
            else
            {
                OpCode propName = isStatic ? OpCodes.Ldarg_1 : OpCodes.Ldarg_2,
                       target = isStatic ? OpCodes.Ldarg_0 : OpCodes.Ldarg_1,
                       value = isStatic ? OpCodes.Ldarg_2 : OpCodes.Ldarg_3;
                LocalBuilder loc = type.IsValueType ? il.DeclareLocal(type) : null;
                foreach (PropertyInfo prop in props)
                {
                    if (prop.GetIndexParameters().Length != 0 || !prop.CanWrite) continue;
                    var setFn = prop.GetSetMethod();
                    if (setFn == null) continue; //Mono

                    Label next = il.DefineLabel();
                    il.Emit(propName);
                    il.Emit(OpCodes.Ldstr, prop.Name);
                    il.EmitCall(OpCodes.Call, strinqEquals, null);
                    il.Emit(OpCodes.Brfalse_S, next);
                    // match:
                    il.Emit(target);
                    Cast(il, type, loc);
                    il.Emit(value);
                    Cast(il, prop.PropertyType, null);
                    il.EmitCall(type.IsValueType ? OpCodes.Call : OpCodes.Callvirt, setFn, null);
                    il.Emit(OpCodes.Ret);
                    // not match:
                    il.MarkLabel(next);
                }
                foreach (FieldInfo field in fields)
                {
                    Label next = il.DefineLabel();
                    il.Emit(propName);
                    il.Emit(OpCodes.Ldstr, field.Name);
                    il.EmitCall(OpCodes.Call, strinqEquals, null);
                    il.Emit(OpCodes.Brfalse_S, next);
                    // match:
                    il.Emit(target);
                    Cast(il, type, loc);
                    il.Emit(value);
                    Cast(il, field.FieldType, null);
                    il.Emit(OpCodes.Stfld, field);
                    il.Emit(OpCodes.Ret);
                    // not match:
                    il.MarkLabel(next);
                }
                il.Emit(OpCodes.Ldstr, "name");
                il.Emit(OpCodes.Newobj, typeof(ArgumentOutOfRangeException).GetConstructor(new Type[] { typeof(string) }));
                il.Emit(OpCodes.Throw);
            }
        }
        private static readonly MethodInfo strinqEquals = typeof(string).GetMethod("op_Equality", new Type[] { typeof(string), typeof(string) });

        sealed class DelegateAccessor : TypeAccessor
        {
            private readonly Func<object, string, object> getter;
            private readonly Action<object, string, object> setter;
            private readonly Func<object> ctor;
            public DelegateAccessor(Func<object, string, object> getter, Action<object, string, object> setter, Func<object> ctor)
            {
                this.getter = getter;
                this.setter = setter;
                this.ctor = ctor;
            }
            public override bool CreateNewSupported => ctor != null;

            public override object CreateNew()
            {
                return ctor != null ? ctor() : base.CreateNew();
            }
            public override object this[object target, string name]
            {
                get => getter(target, name);
                set => setter(target, name, value);
            }
        }

        private static bool IsFullyPublic(Type type)
        {
            while (type.IsNestedPublic) type = type.DeclaringType;
            return type.IsPublic;
        }

        static TypeAccessor CreateNew(Type type)
        {
            //if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(type))
            //{
            //    return DynamicAccessor.Singleton;
            //}

            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            ConstructorInfo ctor = null;
            if (type.IsClass && !type.IsAbstract)
            {
                ctor = type.GetConstructor(Type.EmptyTypes);
            }
            ILGenerator il;
            if (!IsFullyPublic(type))
            {
                DynamicMethod dynGetter = new DynamicMethod(type.FullName + "_get", typeof(object), new Type[] { typeof(object), typeof(string) }, type, true),
                              dynSetter = new DynamicMethod(type.FullName + "_set", null, new Type[] { typeof(object), typeof(string), typeof(object) }, type, true);
                WriteGetter(dynGetter.GetILGenerator(), type, props, fields, true);
                WriteSetter(dynSetter.GetILGenerator(), type, props, fields, true);
                DynamicMethod dynCtor = null;
                if (ctor != null)
                {
                    dynCtor = new DynamicMethod(type.FullName + "_ctor", typeof(object), Type.EmptyTypes, type, true);
                    il = dynCtor.GetILGenerator();
                    il.Emit(OpCodes.Newobj, ctor);
                    il.Emit(OpCodes.Ret);
                }
                return new DelegateAccessor(
                    (Func<object, string, object>)dynGetter.CreateDelegate(typeof(Func<object, string, object>)),
                    (Action<object, string, object>)dynSetter.CreateDelegate(typeof(Action<object, string, object>)),
                    dynCtor == null ? null : (Func<object>)dynCtor.CreateDelegate(typeof(Func<object>)));
            }

            // note this region is synchronized; only one is being created at a time so we don't need to stress about the builders
            if (assembly == null)
            {
                AssemblyName name = new AssemblyName("FastMember_dynamic");
                assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
                module = assembly.DefineDynamicModule(name.Name);
            }
            TypeBuilder tb = module.DefineType("FastMember_dynamic." + type.Name + "_" + Interlocked.Increment(ref counter),
                (typeof(TypeAccessor).Attributes | TypeAttributes.Sealed) & ~TypeAttributes.Abstract, typeof(TypeAccessor));

            tb.DefineDefaultConstructor(MethodAttributes.Public);
            PropertyInfo indexer = typeof(TypeAccessor).GetProperty("Item");
            MethodInfo baseGetter = indexer.GetGetMethod(), baseSetter = indexer.GetSetMethod();
            MethodBuilder body = tb.DefineMethod(baseGetter.Name, baseGetter.Attributes & ~MethodAttributes.Abstract, typeof(object), new Type[] { typeof(object), typeof(string) });
            il = body.GetILGenerator();
            WriteGetter(il, type, props, fields, false);
            tb.DefineMethodOverride(body, baseGetter);

            body = tb.DefineMethod(baseSetter.Name, baseSetter.Attributes & ~MethodAttributes.Abstract, null, new Type[] { typeof(object), typeof(string), typeof(object) });
            il = body.GetILGenerator();
            WriteSetter(il, type, props, fields, false);
            tb.DefineMethodOverride(body, baseSetter);

            if (ctor != null)
            {
                MethodInfo baseMethod = typeof(TypeAccessor).GetProperty("CreateNewSupported").GetGetMethod();
                body = tb.DefineMethod(baseMethod.Name, baseMethod.Attributes, typeof(bool), Type.EmptyTypes);
                il = body.GetILGenerator();
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Ret);
                tb.DefineMethodOverride(body, baseMethod);

                baseMethod = typeof(TypeAccessor).GetMethod("CreateNew");
                body = tb.DefineMethod(baseMethod.Name, baseMethod.Attributes, typeof(object), Type.EmptyTypes);
                il = body.GetILGenerator();
                il.Emit(OpCodes.Newobj, ctor);
                il.Emit(OpCodes.Ret);
                tb.DefineMethodOverride(body, baseMethod);
            }

            return (TypeAccessor)Activator.CreateInstance(tb.CreateType());
        }

        private static void Cast(ILGenerator il, Type type, LocalBuilder addr)
        {
            if (type == typeof(object)) { }
            else if (type.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, type);
                if (addr != null)
                {
                    il.Emit(OpCodes.Stloc, addr);
                    il.Emit(OpCodes.Ldloca_S, addr);
                }
            }
            else
            {
                il.Emit(OpCodes.Castclass, type);
            }
        }

        /// <summary>
        /// Get or set the value of a named member on the target instance
        /// </summary>
        public abstract object this[object target, string name]
        {
            get;
            set;
        }
    }
}
#endif

#endif
