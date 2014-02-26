using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;

using ServiceStack.Text.Json;
using ServiceStack.Text.Jsv;

namespace ServiceStack.Text.Common
{
    public static class JsWriter
    {
        public const string TypeAttr = "__type";

        public const char MapStartChar = '{';
        public const char MapKeySeperator = ':';
        public const char ItemSeperator = ',';
        public const char MapEndChar = '}';
        public const string MapNullValue = "\"\"";
        public const string EmptyMap = "{}";

        public const char ListStartChar = '[';
        public const char ListEndChar = ']';
        public const char ReturnChar = '\r';
        public const char LineFeedChar = '\n';

        public const char QuoteChar = '"';
        public const string QuoteString = "\"";
        public const string EscapedQuoteString = "\\\"";
        public const string ItemSeperatorString = ",";
        public const string MapKeySeperatorString = ":";

        public static readonly char[] CsvChars = new[] { ItemSeperator, QuoteChar };
        public static readonly char[] EscapeChars = new[] { QuoteChar, MapKeySeperator, ItemSeperator, MapStartChar, MapEndChar, ListStartChar, ListEndChar, ReturnChar, LineFeedChar };

        private const int LengthFromLargestChar = '}' + 1;
        private static readonly bool[] EscapeCharFlags = new bool[LengthFromLargestChar];

        static JsWriter()
        {
            foreach (var escapeChar in EscapeChars)
            {
                EscapeCharFlags[escapeChar] = true;
            }
            var loadConfig = JsConfig.EmitCamelCaseNames; //force load
        }

        public static void WriteDynamic(Action callback)
        {
            JsState.IsWritingDynamic = true;
            try
            {
                callback();
            }
            finally
            {
                JsState.IsWritingDynamic = false;
            }
        }

        /// <summary>
        /// micro optimizations: using flags instead of value.IndexOfAny(EscapeChars)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool HasAnyEscapeChars(string value)
        {
            var len = value.Length;
            for (var i = 0; i < len; i++)
            {
                var c = value[i];
                if (c >= LengthFromLargestChar || !EscapeCharFlags[c]) continue;
                return true;
            }
            return false;
        }

        internal static void WriteItemSeperatorIfRanOnce(TextWriter writer, ref bool ranOnce)
        {
            if (ranOnce)
                writer.Write(ItemSeperator);
            else
                ranOnce = true;
        }

        internal static bool ShouldUseDefaultToStringMethod(Type type)
        {
            return type == typeof(byte) || type == typeof(byte?)
                || type == typeof(short) || type == typeof(short?)
                || type == typeof(ushort) || type == typeof(ushort?)
                || type == typeof(int) || type == typeof(int?)
                || type == typeof(uint) || type == typeof(uint?)
                || type == typeof(long) || type == typeof(long?)
                || type == typeof(ulong) || type == typeof(ulong?)
                || type == typeof(bool) || type == typeof(bool?)
                || type == typeof(DateTime) || type == typeof(DateTime?)
                || type == typeof(Guid) || type == typeof(Guid?)
                || type == typeof(float) || type == typeof(float?)
                || type == typeof(double) || type == typeof(double?)
                || type == typeof(decimal) || type == typeof(decimal?);
        }

        public static ITypeSerializer GetTypeSerializer<TSerializer>()
        {
            if (typeof(TSerializer) == typeof(JsvTypeSerializer))
                return JsvTypeSerializer.Instance;

            if (typeof(TSerializer) == typeof(JsonTypeSerializer))
                return JsonTypeSerializer.Instance;

            throw new NotSupportedException(typeof(TSerializer).Name);
        }

        public static void WriteEnumFlags(TextWriter writer, object enumFlagValue)
        {
            if (enumFlagValue == null) return;

            var typeCode = Enum.GetUnderlyingType(enumFlagValue.GetType()).GetTypeCode();
            switch (typeCode)
            {
                case TypeCode.SByte:
                    writer.Write((sbyte)enumFlagValue);
                    break;
                case TypeCode.Byte:
                    writer.Write((byte)enumFlagValue);
                    break;
                case TypeCode.Int16:
                    writer.Write((short)enumFlagValue);
                    break;
                case TypeCode.UInt16:
                    writer.Write((ushort)enumFlagValue);
                    break;
                case TypeCode.Int32:
                    writer.Write((int)enumFlagValue);
                    break;
                case TypeCode.UInt32:
                    writer.Write((uint)enumFlagValue);
                    break;
                case TypeCode.Int64:
                    writer.Write((long)enumFlagValue);
                    break;
                case TypeCode.UInt64:
                    writer.Write((ulong)enumFlagValue);
                    break;
                default:
                    writer.Write((int)enumFlagValue);
                    break;
            }
        }
    }

    public class JsWriter<TSerializer>
        where TSerializer : ITypeSerializer
    {
        private static readonly ITypeSerializer Serializer = JsWriter.GetTypeSerializer<TSerializer>();

        public JsWriter()
        {
            this.SpecialTypes = new Dictionary<Type, WriteObjectDelegate>
        	{
        		{ typeof(Uri), Serializer.WriteObjectString },
        		{ typeof(Type), WriteType },
        		{ typeof(Exception), Serializer.WriteException },
#if !(__IOS__ || SL5 || XBOX || ANDROID || PCL)
                { typeof(System.Data.Linq.Binary), Serializer.WriteLinqBinary },
#endif
        	};
        }

        public WriteObjectDelegate GetValueTypeToStringMethod(Type type)
        {
            if (type == typeof(char) || type == typeof(char?))
                return Serializer.WriteChar;
            if (type == typeof(int) || type == typeof(int?))
                return Serializer.WriteInt32;
            if (type == typeof(long) || type == typeof(long?))
                return Serializer.WriteInt64;
            if (type == typeof(ulong) || type == typeof(ulong?))
                return Serializer.WriteUInt64;
            if (type == typeof(uint) || type == typeof(uint?))
                return Serializer.WriteUInt32;

            if (type == typeof(byte) || type == typeof(byte?))
                return Serializer.WriteByte;

            if (type == typeof(short) || type == typeof(short?))
                return Serializer.WriteInt16;
            if (type == typeof(ushort) || type == typeof(ushort?))
                return Serializer.WriteUInt16;

            if (type == typeof(bool) || type == typeof(bool?))
                return Serializer.WriteBool;

            if (type == typeof(DateTime))
                return Serializer.WriteDateTime;

            if (type == typeof(DateTime?))
                return Serializer.WriteNullableDateTime;

            if (type == typeof(DateTimeOffset))
                return Serializer.WriteDateTimeOffset;

            if (type == typeof(DateTimeOffset?))
                return Serializer.WriteNullableDateTimeOffset;

            if (type == typeof(TimeSpan))
                return Serializer.WriteTimeSpan;

            if (type == typeof(TimeSpan?))
                return Serializer.WriteNullableTimeSpan;

            if (type == typeof(Guid))
                return Serializer.WriteGuid;

            if (type == typeof(Guid?))
                return Serializer.WriteNullableGuid;

            if (type == typeof(float) || type == typeof(float?))
                return Serializer.WriteFloat;

            if (type == typeof(double) || type == typeof(double?))
                return Serializer.WriteDouble;

            if (type == typeof(decimal) || type == typeof(decimal?))
                return Serializer.WriteDecimal;

            if (type.IsUnderlyingEnum())
                return type.FirstAttribute<FlagsAttribute>() != null
                    ? (WriteObjectDelegate)Serializer.WriteEnumFlags
                    : Serializer.WriteEnum;

            Type nullableType;
            if ((nullableType = Nullable.GetUnderlyingType(type)) != null && nullableType.IsEnum())
                return nullableType.FirstAttribute<FlagsAttribute>() != null
                    ? (WriteObjectDelegate)Serializer.WriteEnumFlags
                    : Serializer.WriteEnum;

            if (type.HasInterface(typeof (IFormattable)))
                return Serializer.WriteFormattableObjectString;

            return Serializer.WriteObjectString;
        }

        public WriteObjectDelegate GetWriteFn<T>()
        {
            if (typeof(T) == typeof(string))
            {
                return Serializer.WriteObjectString;
            }

            var onSerializingFn = JsConfig<T>.OnSerializingFn;
            if (onSerializingFn != null)
            {
                return (w, x) => GetCoreWriteFn<T>()(w, onSerializingFn((T)x));
            }

            if (JsConfig<T>.HasSerializeFn)
            {
                return JsConfig<T>.WriteFn<TSerializer>;
            }

            return GetCoreWriteFn<T>();
        }

        private WriteObjectDelegate GetCoreWriteFn<T>()
        {
            if ((typeof(T).IsValueType() && !JsConfig.TreatAsRefType(typeof(T))) || JsConfig<T>.HasSerializeFn)
            {
                return JsConfig<T>.HasSerializeFn
                    ? JsConfig<T>.WriteFn<TSerializer>
                    : GetValueTypeToStringMethod(typeof(T));
            }

            var specialWriteFn = GetSpecialWriteFn(typeof(T));
            if (specialWriteFn != null)
            {
                return specialWriteFn;
            }

            if (typeof(T).IsArray)
            {
                if (typeof(T) == typeof(byte[]))
                    return (w, x) => WriteLists.WriteBytes(Serializer, w, x);

                if (typeof(T) == typeof(string[]))
                    return (w, x) => WriteLists.WriteStringArray(Serializer, w, x);

                if (typeof(T) == typeof(int[]))
                    return WriteListsOfElements<int, TSerializer>.WriteGenericArrayValueType;
                if (typeof(T) == typeof(long[]))
                    return WriteListsOfElements<long, TSerializer>.WriteGenericArrayValueType;

                var elementType = typeof(T).GetElementType();
                var writeFn = WriteListsOfElements<TSerializer>.GetGenericWriteArray(elementType);
                return writeFn;
            }

            if (typeof(T).HasGenericType() ||
                typeof(T).HasInterface(typeof(IDictionary<string, object>))) // is ExpandoObject?
            {
                if (typeof(T).IsOrHasGenericInterfaceTypeOf(typeof(IList<>)))
                    return WriteLists<T, TSerializer>.Write;

                var mapInterface = typeof(T).GetTypeWithGenericTypeDefinitionOf(typeof(IDictionary<,>));
                if (mapInterface != null)
                {
                    var mapTypeArgs = mapInterface.GenericTypeArguments();
                    var writeFn = WriteDictionary<TSerializer>.GetWriteGenericDictionary(
                        mapTypeArgs[0], mapTypeArgs[1]);

                    var keyWriteFn = Serializer.GetWriteFn(mapTypeArgs[0]);
                    var valueWriteFn = typeof(T) == typeof(JsonObject)
                        ? JsonObject.WriteValue
                        : Serializer.GetWriteFn(mapTypeArgs[1]);

                    return (w, x) => writeFn(w, x, keyWriteFn, valueWriteFn);
                }
            }

            var enumerableInterface = typeof(T).GetTypeWithGenericTypeDefinitionOf(typeof(IEnumerable<>));
            if (enumerableInterface != null)
            {
                var elementType = enumerableInterface.GenericTypeArguments()[0];
                var writeFn = WriteListsOfElements<TSerializer>.GetGenericWriteEnumerable(elementType);
                return writeFn;
            }

            var isDictionary = typeof(T) != typeof(IEnumerable) && typeof(T) != typeof(ICollection)
                && (typeof(T).AssignableFrom(typeof(IDictionary)) || typeof(T).HasInterface(typeof(IDictionary)));
            if (isDictionary)
            {
                return WriteDictionary<TSerializer>.WriteIDictionary;
            }

            var isEnumerable = typeof(T).AssignableFrom(typeof(IEnumerable))
                || typeof(T).HasInterface(typeof(IEnumerable));
            if (isEnumerable)
            {
                return WriteListsOfElements<TSerializer>.WriteIEnumerable;
            }

            if (typeof(T).IsClass() || typeof(T).IsInterface() || JsConfig.TreatAsRefType(typeof(T)))
            {
                var typeToStringMethod = WriteType<T, TSerializer>.Write;
                if (typeToStringMethod != null)
                {
                    return typeToStringMethod;
                }
            }

            return Serializer.WriteBuiltIn;
        }

        public Dictionary<Type, WriteObjectDelegate> SpecialTypes;

        public WriteObjectDelegate GetSpecialWriteFn(Type type)
        {
            WriteObjectDelegate writeFn = null;
            if (SpecialTypes.TryGetValue(type, out writeFn))
                return writeFn;

            if (type.InstanceOfType(typeof(Type)))
                return WriteType;

            if (type.IsInstanceOf(typeof(Exception)))
                return Serializer.WriteException;

            return null;
        }

        public void WriteType(TextWriter writer, object value)
        {
            Serializer.WriteRawString(writer, JsConfig.TypeWriter((Type)value));
        }

    }
}
