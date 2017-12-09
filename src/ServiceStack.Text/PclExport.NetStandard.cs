//Copyright (c) ServiceStack, Inc. All Rights Reserved.
//License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

#if NETSTANDARD2_0
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ServiceStack.Text;
using ServiceStack.Text.Common;
using ServiceStack.Text.Json;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Net;

using System.Collections.Specialized;
using System.Linq.Expressions;
using Microsoft.Extensions.Primitives;

namespace ServiceStack
{
    public class NetStandardPclExport : PclExport
    {
        public static NetStandardPclExport Provider = new NetStandardPclExport();

        static string[] allDateTimeFormats = new string[]
        {
            "yyyy-MM-ddTHH:mm:ss.FFFFFFFzzzzzz",
            "yyyy-MM-ddTHH:mm:ss.FFFFFFF",
            "yyyy-MM-ddTHH:mm:ss.FFFFFFFZ",
            "HH:mm:ss.FFFFFFF",
            "HH:mm:ss.FFFFFFFZ",
            "HH:mm:ss.FFFFFFFzzzzzz",
            "yyyy-MM-dd",
            "yyyy-MM-ddZ",
            "yyyy-MM-ddzzzzzz",
            "yyyy-MM",
            "yyyy-MMZ",
            "yyyy-MMzzzzzz",
            "yyyy",
            "yyyyZ",
            "yyyyzzzzzz",
            "--MM-dd",
            "--MM-ddZ",
            "--MM-ddzzzzzz",
            "---dd",
            "---ddZ",
            "---ddzzzzzz",
            "--MM--",
            "--MM--Z",
            "--MM--zzzzzz",
        };

        static readonly Action<HttpWebRequest, string> SetUserAgentDelegate =
            (Action<HttpWebRequest, string>)typeof(HttpWebRequest)
                .GetProperty("UserAgent")
                ?.GetSetMethod(nonPublic:true)?.CreateDelegate(typeof(Action<HttpWebRequest, string>));

        static readonly Action<HttpWebRequest, bool> SetAllowAutoRedirectDelegate =
            (Action<HttpWebRequest, bool>)typeof(HttpWebRequest)
                .GetProperty("AllowAutoRedirect")
                ?.GetSetMethod(nonPublic:true)?.CreateDelegate(typeof(Action<HttpWebRequest, bool>));

        static readonly Action<HttpWebRequest, bool> SetKeepAliveDelegate =
            (Action<HttpWebRequest, bool>)typeof(HttpWebRequest)
                .GetProperty("KeepAlive")
                ?.GetSetMethod(nonPublic:true)?.CreateDelegate(typeof(Action<HttpWebRequest, bool>));

        static readonly Action<HttpWebRequest, long> SetContentLengthDelegate =
            (Action<HttpWebRequest, long>)typeof(HttpWebRequest)
                .GetProperty("ContentLength")
                ?.GetSetMethod(nonPublic:true)?.CreateDelegate(typeof(Action<HttpWebRequest, long>));

        private bool allowToChangeRestrictedHeaders;

        public NetStandardPclExport()
        {
            this.PlatformName = Platforms.NetStandard;
#if NETSTANDARD2_0
            this.DirSep = Path.DirectorySeparatorChar;
#else 
            this.DirSep = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? '\\' : '/';
#endif
            var req = HttpWebRequest.Create("http://servicestack.net");
            try
            {
                req.Headers[HttpRequestHeader.UserAgent] = "ServiceStack";
                allowToChangeRestrictedHeaders = true;
            } catch (ArgumentException)
            {
                allowToChangeRestrictedHeaders = false;
            }
        }

        public override string ReadAllText(string filePath)
        {
            //NET Standard 1.1 does not supported Stream Reader with string constructor
#if NETSTANDARD2_0
            using (StreamReader rdr = File.OpenText(filePath))
            {
                return rdr.ReadToEnd();
            }
#else            
            return String.Empty;
#endif
        }

#if NETSTANDARD2_0
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
            //Automatically register license key stored in <appSettings/> is done in .NET Core AppHost

            //or SERVICESTACK_LICENSE Environment variable
            var licenceKeyText = GetEnvironmentVariable(EnvironmentKey)?.Trim();
            if (!string.IsNullOrEmpty(licenceKeyText))
            {
                LicenseUtils.RegisterLicense(licenceKeyText);
            }
        }

        public override string MapAbsolutePath(string relativePath, string appendPartialPathModifier)
        {
            if (relativePath.StartsWith("~"))
            {
                var assemblyDirectoryPath = AppContext.BaseDirectory;

                // Escape the assembly bin directory to the hostname directory
                var hostDirectoryPath = appendPartialPathModifier != null
                    ? assemblyDirectoryPath + appendPartialPathModifier
                    : assemblyDirectoryPath;

                return Path.GetFullPath(relativePath.Replace("~", hostDirectoryPath));
            }
            return relativePath;
        }        
#endif
        public static PclExport Configure()
        {
            Configure(Provider);
            return Provider;
        }

        public override string GetEnvironmentVariable(string name)
        {
#if NETSTANDARD2_0
            return Environment.GetEnvironmentVariable(name);
#else
            return null;
#endif
        }

        public override void WriteLine(string line)
        {
#if NETSTANDARD2_0
            Console.WriteLine(line);
#else
            System.Diagnostics.Debug.WriteLine(line);
#endif
        }

        public override void WriteLine(string format, params object[] args)
        {
#if NETSTANDARD2_0
            Console.WriteLine(format, args);
#else
            System.Diagnostics.Debug.WriteLine(format, args);
#endif
        }

#if NETSTANDARD2_0
        public override void AddCompression(WebRequest webReq)
        {
            var httpReq = (HttpWebRequest)webReq;
            //TODO: Restore when AutomaticDecompression added to WebRequest
            //httpReq.Headers[HttpRequestHeader.AcceptEncoding] = "gzip,deflate";
            //httpReq.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
        }

        public override void AddHeader(WebRequest webReq, string name, string value)
        {
            webReq.Headers[name] = value;
        }
#endif

        public override Assembly[] GetAllAssemblies()
        {
            return new Assembly[0];
        }

        public override string GetAssemblyCodeBase(Assembly assembly)
        {
            var dll = typeof(PclExport).Assembly;
            var pi = dll.GetType().GetProperty("CodeBase");
            var codeBase = pi?.GetProperty(dll).ToString();
            return codeBase;
        }

#if NETSTANDARD2_0
        public override string GetAssemblyPath(Type source)
        {
            var codeBase = GetAssemblyCodeBase(source.GetTypeInfo().Assembly);
            if (codeBase == null)
                return null;

            var assemblyUri = new Uri(codeBase);
            return assemblyUri.LocalPath;
        }

        public override string GetAsciiString(byte[] bytes, int index, int count)
        {
            return System.Text.Encoding.ASCII.GetString(bytes, index, count);
        }

        public override byte[] GetAsciiBytes(string str)
        {
            return System.Text.Encoding.ASCII.GetBytes(str);
        }
#endif

        public override bool InSameAssembly(Type t1, Type t2)
        {
            return t1.Assembly == t2.Assembly;
        }

        public override Type GetGenericCollectionType(Type type)
        {
            return type.GetTypeInfo().ImplementedInterfaces.FirstOrDefault(t =>
                t.IsGenericType
                && t.GetGenericTypeDefinition() == typeof(ICollection<>));
        }

        public override GetMemberDelegate CreateGetter(PropertyInfo propertyInfo)
        {
            return
#if NETSTANDARD2_0
                SupportsEmit ? PropertyInvoker.GetEmit(propertyInfo) :
#endif
                SupportsExpression
                    ? PropertyInvoker.GetExpression(propertyInfo)
                    : base.CreateGetter(propertyInfo);
        }

        public override GetMemberDelegate<T> CreateGetter<T>(PropertyInfo propertyInfo)
        {
            return
#if NETSTANDARD2_0
                SupportsEmit ? PropertyInvoker.GetEmit<T>(propertyInfo) :
#endif
                SupportsExpression
                    ? PropertyInvoker.GetExpression<T>(propertyInfo)
                    : base.CreateGetter<T>(propertyInfo);
        }

        public override SetMemberDelegate CreateSetter(PropertyInfo propertyInfo)
        {
            return
#if NETSTANDARD2_0
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
#if NETSTANDARD2_0
                SupportsEmit ? FieldInvoker.GetEmit(fieldInfo) :
#endif
                SupportsExpression
                    ? FieldInvoker.GetExpression(fieldInfo)
                    : base.CreateGetter(fieldInfo);
        }

        public override GetMemberDelegate<T> CreateGetter<T>(FieldInfo fieldInfo)
        {
            return
#if NETSTANDARD2_0
                SupportsEmit ? FieldInvoker.GetEmit<T>(fieldInfo) :
#endif
                SupportsExpression
                    ? FieldInvoker.GetExpression<T>(fieldInfo)
                    : base.CreateGetter<T>(fieldInfo);
        }

        public override SetMemberDelegate CreateSetter(FieldInfo fieldInfo)
        {
            return
#if NETSTANDARD2_0
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

        public override DateTime ParseXsdDateTimeAsUtc(string dateTimeStr)
        {
            return DateTime.ParseExact(dateTimeStr, allDateTimeFormats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AllowLeadingWhite|DateTimeStyles.AllowTrailingWhite|DateTimeStyles.AdjustToUniversal)
                     .Prepare(parsedAsUtc: true);
        }

        //public override DateTime ToStableUniversalTime(DateTime dateTime)
        //{
        //    // .Net 2.0 - 3.5 has an issue with DateTime.ToUniversalTime, but works ok with TimeZoneInfo.ConvertTimeToUtc.
        //    // .Net 4.0+ does this under the hood anyway.
        //    return TimeZoneInfo.ConvertTimeToUtc(dateTime);
        //}

#if NETSTANDARD2_0
        public override ParseStringDelegate GetSpecializedCollectionParseMethod<TSerializer>(Type type)
        {
            if (type == typeof(StringCollection))
            {
                return v => ParseStringCollection<TSerializer>(new StringSegment(v));
            }
            return null;
        }

        public override ParseStringSegmentDelegate GetSpecializedCollectionParseStringSegmentMethod<TSerializer>(Type type)
        {
            if (type == typeof(StringCollection))
            {
                return ParseStringCollection<TSerializer>;
            }
            return null;
        }

        private static StringCollection ParseStringCollection<TSerializer>(StringSegment value) where TSerializer : ITypeSerializer
        {
            if (!(value = DeserializeListWithElements<TSerializer>.StripList(value)).HasValue) return null;

            var result = new StringCollection();

            if (value.Length > 0)
            {
                foreach (var item in DeserializeListWithElements<TSerializer>.ParseStringList(value))
                {
                    result.Add(item);
                }
            }

            return result;
        }
#endif
        public override ParseStringDelegate GetJsReaderParseMethod<TSerializer>(Type type)
        {
            if (type.IsAssignableFrom(typeof(System.Dynamic.IDynamicMetaObjectProvider)) ||
                type.HasInterface(typeof(System.Dynamic.IDynamicMetaObjectProvider)))
            {
                return DeserializeDynamic<TSerializer>.Parse;
            }

            return null;
        }

        public override ParseStringSegmentDelegate GetJsReaderParseStringSegmentMethod<TSerializer>(Type type)
        {
            if (type.IsAssignableFrom(typeof(System.Dynamic.IDynamicMetaObjectProvider)) ||
                type.HasInterface(typeof(System.Dynamic.IDynamicMetaObjectProvider)))
            {
                return DeserializeDynamic<TSerializer>.ParseStringSegment;
            }
            
            return null;
        }

        public override void SetUserAgent(HttpWebRequest httpReq, string value)
        {
            if (SetUserAgentDelegate != null)
            {
                SetUserAgentDelegate(httpReq, value);
            } else 
            {
                if (allowToChangeRestrictedHeaders)
                    httpReq.Headers[HttpRequestHeader.UserAgent] = value;
            }
        }

        public override void SetContentLength(HttpWebRequest httpReq, long value)
        {
            if (SetContentLengthDelegate != null)
            {
                SetContentLengthDelegate(httpReq, value);
            } else 
            {
                if (allowToChangeRestrictedHeaders)
                    httpReq.Headers[HttpRequestHeader.ContentLength] = value.ToString();
            }
        }

        public override void SetAllowAutoRedirect(HttpWebRequest httpReq, bool value)
        {
            SetAllowAutoRedirectDelegate?.Invoke(httpReq, value);
        }

        public override void SetKeepAlive(HttpWebRequest httpReq, bool value)
        {
            SetKeepAliveDelegate?.Invoke(httpReq, value);
        }

        public override void InitHttpWebRequest(HttpWebRequest httpReq,
            long? contentLength = null, bool allowAutoRedirect = true, bool keepAlive = true)
        {
            SetUserAgent(httpReq, Env.ServerUserAgent);
            SetAllowAutoRedirect(httpReq, allowAutoRedirect);
            SetKeepAlive(httpReq, keepAlive);

            if (contentLength != null)
            {
                SetContentLength(httpReq, contentLength.Value);
            }
        }

        public override void Config(HttpWebRequest req,
            bool? allowAutoRedirect = null,
            TimeSpan? timeout = null,
            TimeSpan? readWriteTimeout = null,
            string userAgent = null,
            bool? preAuthenticate = null)
        {
            //req.MaximumResponseHeadersLength = int.MaxValue; //throws "The message length limit was exceeded" exception
            if (allowAutoRedirect.HasValue) SetAllowAutoRedirect(req, allowAutoRedirect.Value);
            //if (readWriteTimeout.HasValue) req.ReadWriteTimeout = (int)readWriteTimeout.Value.TotalMilliseconds;
            //if (timeout.HasValue) req.Timeout = (int)timeout.Value.TotalMilliseconds;
            if (userAgent != null) SetUserAgent(req, userAgent);
            //if (preAuthenticate.HasValue) req.PreAuthenticate = preAuthenticate.Value;
        }
        
#if NETSTANDARD2_0
        public override string GetStackTrace()
        {
            return Environment.StackTrace;
        }
#endif

        public override Type UseType(Type type)
        {
            if (type.IsInterface || type.IsAbstract)
            {
                return DynamicProxy.GetInstanceFor(type).GetType();
            }
            return type;
        }

        public static void InitForAot()
        {
        }

        internal class Poco
        {
            public string Dummy { get; set; }
        }

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

        public static void RegisterTypeForAot<T>()
        {
            AotConfig.RegisterSerializers<T>();
        }

        public static void RegisterQueryStringWriter()
        {
            var i = 0;
            if (QueryStringWriter<Poco>.WriteFn() != null) i++;
        }

        public static int RegisterElement<T, TElement>()
        {
            var i = 0;
            i += AotConfig.RegisterSerializers<TElement>();
            AotConfig.RegisterElement<T, TElement, JsonTypeSerializer>();
            AotConfig.RegisterElement<T, TElement, Text.Jsv.JsvTypeSerializer>();
            return i;
        }

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
}

#endif
