//Copyright (c) Service Stack LLC. All Rights Reserved.
//License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

#if NETSTANDARD1_1
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

#if NETSTANDARD1_3
using System.Collections.Specialized;
using System.Linq.Expressions;
#endif

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
                ?.SetMethod()?.CreateDelegate(typeof(Action<HttpWebRequest, string>));

        static readonly Action<HttpWebRequest, bool> SetAllowAutoRedirectDelegate =
            (Action<HttpWebRequest, bool>)typeof(HttpWebRequest)
                .GetProperty("AllowAutoRedirect")
                ?.SetMethod()?.CreateDelegate(typeof(Action<HttpWebRequest, bool>));

        static readonly Action<HttpWebRequest, bool> SetKeepAliveDelegate =
            (Action<HttpWebRequest, bool>)typeof(HttpWebRequest)
                .GetProperty("KeepAlive")
                ?.SetMethod()?.CreateDelegate(typeof(Action<HttpWebRequest, bool>));

        static readonly Action<HttpWebRequest, long> SetContentLengthDelegate =
            (Action<HttpWebRequest, long>)typeof(HttpWebRequest)
                .GetProperty("ContentLength")
                ?.SetMethod()?.CreateDelegate(typeof(Action<HttpWebRequest, long>));

        private bool allowToChangeRestrictedHeaders;

        public NetStandardPclExport()
        {
            this.PlatformName = Platforms.NetStandard;
#if NETSTANDARD1_3
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
#if NETSTANDARD1_3
            using (StreamReader rdr = File.OpenText(filePath))
            {
                return rdr.ReadToEnd();
            }
#else            
            return String.Empty;
#endif
        }

#if NETSTANDARD1_3
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
        
#elif NETSTANDARD1_1
        public string BinPath = null;

        public override string MapAbsolutePath(string relativePath, string appendPartialPathModifier)
        {
            if (BinPath == null)
            {
                var codeBase = GetAssemblyCodeBase(typeof(PclExport).GetTypeInfo().Assembly);
                if (codeBase == null)
                    throw new Exception("NetStandardPclExport.BinPath must be initialized");

                BinPath = Path.GetDirectoryName(codeBase.Replace("file:///", ""));
            }

            return relativePath.StartsWith("~")
                ? relativePath.Replace("~", BinPath)
                : relativePath;
        }
#endif
        public static PclExport Configure()
        {
            Configure(Provider);
            return Provider;
        }

        public override string GetEnvironmentVariable(string name)
        {
#if NETSTANDARD1_3
            return Environment.GetEnvironmentVariable(name);
#else
            return null;
#endif
        }

        public override void WriteLine(string line)
        {
#if NETSTANDARD1_3
            Console.WriteLine(line);
#else
            System.Diagnostics.Debug.WriteLine(line);
#endif
        }

        public override void WriteLine(string format, params object[] args)
        {
#if NETSTANDARD1_3
            Console.WriteLine(format, args);
#else
            System.Diagnostics.Debug.WriteLine(format, args);
#endif
        }

#if NETSTANDARD1_3
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
            var dll = typeof(PclExport).GetAssembly();
            var pi = dll.GetType().GetProperty("CodeBase");
            var codeBase = pi?.GetProperty(dll).ToString();
            return codeBase;
        }

#if NETSTANDARD1_3
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
            return t1.GetAssembly() == t2.GetAssembly();
        }

        public override Type GetGenericCollectionType(Type type)
        {
            return type.GetTypeInfo().ImplementedInterfaces.FirstOrDefault(t =>
                t.IsGenericType()
                && t.GetGenericTypeDefinition() == typeof(ICollection<>));
        }

#if NETSTANDARD1_3

        public override PropertySetterDelegate GetPropertySetterFn(PropertyInfo propertyInfo)
        {
            var propertySetMethod = propertyInfo.SetMethod();
            if (propertySetMethod == null) return null;

            if (!SupportsExpression)
            {
                return (o, convertedValue) =>
                    propertySetMethod.Invoke(o, new[] { convertedValue });
            }

            try
            {
                var instance = Expression.Parameter(typeof(object), "i");
                var argument = Expression.Parameter(typeof(object), "a");

                var instanceParam = Expression.Convert(instance, propertyInfo.ReflectedType());
                var valueParam = Expression.Convert(argument, propertyInfo.PropertyType);

                var setterCall = Expression.Call(instanceParam, propertySetMethod, valueParam);

                return Expression.Lambda<PropertySetterDelegate>(setterCall, instance, argument).Compile();
            }
            catch //fallback for Android
            {
                return (o, convertedValue) =>
                    propertySetMethod.Invoke(o, new[] { convertedValue });
            }
        }

        public override PropertyGetterDelegate GetPropertyGetterFn(PropertyInfo propertyInfo)
        {
            if (!SupportsExpression)
                return base.GetPropertyGetterFn(propertyInfo);

            var getMethodInfo = propertyInfo.GetMethodInfo();
            if (getMethodInfo == null) return null;
            try
            {
                var oInstanceParam = Expression.Parameter(typeof(object), "oInstanceParam");
                var instanceParam = Expression.Convert(oInstanceParam, propertyInfo.ReflectedType()); //propertyInfo.DeclaringType doesn't work on Proxy types
                
                var exprCallPropertyGetFn = Expression.Call(instanceParam, getMethodInfo);
                var oExprCallPropertyGetFn = Expression.Convert(exprCallPropertyGetFn, typeof(object));

                var propertyGetFn = Expression.Lambda<PropertyGetterDelegate>
                    (
                        oExprCallPropertyGetFn,
                        oInstanceParam
                    ).Compile();

                return propertyGetFn;

            }
            catch (Exception ex)
            {
                Tracer.Instance.WriteError(ex);
                throw;
            }
        }

        private static readonly MethodInfo setFieldMethod =
            typeof(NetStandardPclExport).GetStaticMethod("SetField");

        internal static void SetField<TValue>(ref TValue field, TValue newValue)
        {
            field = newValue;
        }

        public override PropertySetterDelegate GetFieldSetterFn(FieldInfo fieldInfo)
        {
            if (!SupportsExpression)
                return base.GetFieldSetterFn(fieldInfo);

            var fieldDeclaringType = fieldInfo.DeclaringType;

            var sourceParameter = Expression.Parameter(typeof(object), "source");
            var valueParameter = Expression.Parameter(typeof(object), "value");

            var sourceExpression = this.GetCastOrConvertExpression(sourceParameter, fieldDeclaringType);

            var fieldExpression = Expression.Field(sourceExpression, fieldInfo);

            var valueExpression = this.GetCastOrConvertExpression(valueParameter, fieldExpression.Type);

            var genericSetFieldMethodInfo = setFieldMethod.MakeGenericMethod(fieldExpression.Type);

            var setFieldMethodCallExpression = Expression.Call(
                null, genericSetFieldMethodInfo, fieldExpression, valueExpression);

            var setterFn = Expression.Lambda<PropertySetterDelegate>(
                setFieldMethodCallExpression, sourceParameter, valueParameter).Compile();

            return setterFn;
        }

        public override PropertyGetterDelegate GetFieldGetterFn(FieldInfo fieldInfo)
        {
            if (!SupportsExpression)
                return base.GetFieldGetterFn(fieldInfo);

            try
            {
                var fieldDeclaringType = fieldInfo.DeclaringType;

                var oInstanceParam = Expression.Parameter(typeof(object), "source");
                var instanceParam = this.GetCastOrConvertExpression(oInstanceParam, fieldDeclaringType);

                var exprCallFieldGetFn = Expression.Field(instanceParam, fieldInfo);
                //var oExprCallFieldGetFn = this.GetCastOrConvertExpression(exprCallFieldGetFn, typeof(object));
                var oExprCallFieldGetFn = Expression.Convert(exprCallFieldGetFn, typeof(object));

                var fieldGetterFn = Expression.Lambda<PropertyGetterDelegate>
                    (
                        oExprCallFieldGetFn,
                        oInstanceParam
                    )
                    .Compile();

                return fieldGetterFn;
            }
            catch (Exception ex)
            {
                Tracer.Instance.WriteError(ex);
                throw;
            }
        }

        private Expression GetCastOrConvertExpression(Expression expression, Type targetType)
        {
            Expression result;
            var expressionType = expression.Type;

            if (targetType.IsAssignableFromType(expressionType))
            {
                result = expression;
            }
            else
            {
                // Check if we can use the as operator for casting or if we must use the convert method
                if (targetType.IsValueType() && !targetType.IsNullableType())
                {
                    result = Expression.Convert(expression, targetType);
                }
                else
                {
                    result = Expression.TypeAs(expression, targetType);
                }
            }

            return result;
        }
#endif

        public override string ToXsdDateTimeString(DateTime dateTime)
        {
            return System.Xml.XmlConvert.ToString(dateTime.ToStableUniversalTime());
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

#if NETSTANDARD1_3
        public override ParseStringDelegate GetSpecializedCollectionParseMethod<TSerializer>(Type type)
        {
            if (type == typeof(StringCollection))
            {
                return ParseStringCollection<TSerializer>;
            }
            return null;
        }

        private static StringCollection ParseStringCollection<TSerializer>(string value) where TSerializer : ITypeSerializer
        {
            if ((value = DeserializeListWithElements<TSerializer>.StripList(value)) == null) return null;

            var result = new StringCollection();

            if (value != String.Empty)
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
            if (type.AssignableFrom(typeof(System.Dynamic.IDynamicMetaObjectProvider)) ||
                type.HasInterface(typeof(System.Dynamic.IDynamicMetaObjectProvider)))
            {
                return DeserializeDynamic<TSerializer>.Parse;
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
        
#if NETSTANDARD1_3
        public override string GetStackTrace()
        {
            return Environment.StackTrace;
        }
#endif

        public override SetPropertyDelegate GetSetPropertyMethod(PropertyInfo propertyInfo)
        {
            return CreateIlPropertySetter(propertyInfo);
        }

        public override SetPropertyDelegate GetSetFieldMethod(FieldInfo fieldInfo)
        {
            return CreateIlFieldSetter(fieldInfo);
        }

        public override SetPropertyDelegate GetSetMethod(PropertyInfo propertyInfo, FieldInfo fieldInfo)
        {
            return propertyInfo.CanWrite
                ? CreateIlPropertySetter(propertyInfo)
                : CreateIlFieldSetter(fieldInfo);
        }

        public override Type UseType(Type type)
        {
            if (type.IsInterface() || type.IsAbstract())
            {
                return DynamicProxy.GetInstanceFor(type).GetType();
            }
            return type;
        }

        public static SetPropertyDelegate CreateIlPropertySetter(PropertyInfo propertyInfo)
        {
            var propSetMethod = propertyInfo.SetMethod;
            if (propSetMethod == null)
                return null;

            var setter = CreateDynamicSetMethod(propertyInfo);

            var generator = setter.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
            generator.Emit(OpCodes.Ldarg_1);

            generator.Emit(propertyInfo.PropertyType.IsClass()
                               ? OpCodes.Castclass
                               : OpCodes.Unbox_Any,
                           propertyInfo.PropertyType);

            generator.EmitCall(OpCodes.Callvirt, propSetMethod, (Type[])null);
            generator.Emit(OpCodes.Ret);

            return (SetPropertyDelegate)setter.CreateDelegate(typeof(SetPropertyDelegate));
        }

        public static SetPropertyDelegate CreateIlFieldSetter(FieldInfo fieldInfo)
        {
            var setter = CreateDynamicSetMethod(fieldInfo);

            var generator = setter.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Castclass, fieldInfo.DeclaringType);
            generator.Emit(OpCodes.Ldarg_1);

            generator.Emit(fieldInfo.FieldType.IsClass()
                               ? OpCodes.Castclass
                               : OpCodes.Unbox_Any,
                           fieldInfo.FieldType);

            generator.Emit(OpCodes.Stfld, fieldInfo);
            generator.Emit(OpCodes.Ret);

            return (SetPropertyDelegate)setter.CreateDelegate(typeof(SetPropertyDelegate));
        }

        private static DynamicMethod CreateDynamicSetMethod(MemberInfo memberInfo)
        {
            var args = new[] { typeof(object), typeof(object) };
            var name = string.Format("_{0}{1}_", "Set", memberInfo.Name);
            var returnType = typeof(void);

            return !memberInfo.DeclaringType.IsInterface()
                       ? new DynamicMethod(name, returnType, args, memberInfo.DeclaringType, true)
                       : new DynamicMethod(name, returnType, args, memberInfo.Module, true);
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
