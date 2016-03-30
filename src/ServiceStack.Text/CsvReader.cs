using System;
using System.Collections.Generic;
using System.Linq;
using ServiceStack.Reflection;
using ServiceStack.Text.Common;
using ServiceStack.Text.Jsv;

namespace ServiceStack.Text
{
    public class CsvReader
    {
        static readonly ITypeSerializer Serializer = JsvTypeSerializer.Instance;

        public static List<string> ParseLines(string csv)
        {
            var rows = new List<string>();
            if (string.IsNullOrEmpty(csv))
                return rows;

            var withinQuotes = false;
            var lastPos = 0;

            var i = -1;
            var len = csv.Length;
            while (++i < len)
            {
                var c = csv[i];
                if (c == JsWriter.QuoteChar)
                {
                    var isLiteralQuote = i + 1 < len && csv[i + 1] == JsWriter.QuoteChar;
                    if (isLiteralQuote)
                    {
                        i++;
                        continue;
                    }

                    withinQuotes = !withinQuotes;
                }

                if (withinQuotes)
                    continue;

                if (c == JsWriter.LineFeedChar)
                {
                    var str = i > 0 && csv[i - 1] == JsWriter.ReturnChar
                        ? csv.Substring(lastPos, i - lastPos - 1)
                        : csv.Substring(lastPos, i - lastPos);

                    if (str.Length > 0)
                        rows.Add(str);
                    lastPos = i + 1;
                }
            }

            if (i > lastPos)
            {
                var str = csv.Substring(lastPos, i - lastPos);
                if (str.Length > 0)
                    rows.Add(str);
            }

            return rows;
        }

        public static List<string> ParseFields(string line)
        {
            var to = new List<string>();
            if (string.IsNullOrEmpty(line))
                return to;

            var i = -1;
            var len = line.Length;
            while (++i <= len)
            {
                var value = Serializer.EatValue(line, ref i);
                to.Add(value.FromCsvField());
            }

            return to;
        }
    }

    public class CsvReader<T>
    {
        public const char DelimiterChar = ',';

        public static List<string> Headers { get; set; }

        internal static List<Action<T, object>> PropertySetters;
        internal static Dictionary<string, Action<T, object>> PropertySettersMap;

        internal static List<ParseStringDelegate> PropertyConverters;
        internal static Dictionary<string, ParseStringDelegate> PropertyConvertersMap;

        private static readonly ParseStringDelegate OptimizedReader;

        static CsvReader()
        {
            //if (typeof(T) == typeof(string))
            //{
            //    OptimizedReader = ReadRow;
            //    return;
            //}

            Reset();
        }

        internal static void Reset()
        {
            Headers = new List<string>();

            PropertySetters = new List<Action<T, object>>();
            PropertySettersMap = new Dictionary<string, Action<T, object>>(PclExport.Instance.InvariantComparerIgnoreCase);

            PropertyConverters = new List<ParseStringDelegate>();
            PropertyConvertersMap = new Dictionary<string, ParseStringDelegate>(PclExport.Instance.InvariantComparerIgnoreCase);

            var isDataContract = typeof(T).IsDto();
            foreach (var propertyInfo in TypeConfig<T>.Properties)
            {
                if (!propertyInfo.CanWrite || propertyInfo.GetSetMethod() == null) continue;
                if (!TypeSerializer.CanCreateFromString(propertyInfo.PropertyType)) continue;

                var propertyName = propertyInfo.Name;
                var setter = propertyInfo.GetValueSetter<T>();
                PropertySetters.Add(setter);

                var converter = JsvReader.GetParseFn(propertyInfo.PropertyType);
                PropertyConverters.Add(converter);

                if (isDataContract)
                {
                    var dcsDataMemberName = propertyInfo.GetDataMemberName();
                    if (dcsDataMemberName != null)
                        propertyName = dcsDataMemberName;
                }

                Headers.Add(propertyName);
                PropertySettersMap[propertyName] = setter;
                PropertyConvertersMap[propertyName] = converter;
            }
        }

        internal static void ConfigureCustomHeaders(Dictionary<string, string> customHeadersMap)
        {
            Reset();

            for (var i = Headers.Count - 1; i >= 0; i--)
            {
                var oldHeader = Headers[i];
                string newHeaderValue;
                if (!customHeadersMap.TryGetValue(oldHeader, out newHeaderValue))
                {
                    Headers.RemoveAt(i);
                    PropertySetters.RemoveAt(i);
                }
                else
                {
                    Headers[i] = newHeaderValue.EncodeJsv();
                }
            }
        }

        private static List<T> GetSingleRow(IEnumerable<string> rows, Type recordType)
        {
            var row = new List<T>();
            foreach (var value in rows)
            {
                var to = recordType == typeof(string)
                   ? (T)(object)value
                   : TypeSerializer.DeserializeFromString<T>(value);

                row.Add(to);
            }
            return row;
        }

        public static List<T> GetRows(IEnumerable<string> records)
        {
            var rows = new List<T>();

            if (records == null) return rows;

            if (typeof(T).IsValueType() || typeof(T) == typeof(string))
            {
                return GetSingleRow(records, typeof(T));
            }

            foreach (var record in records)
            {
                var to = typeof(T).CreateInstance<T>();
                foreach (var propertySetter in PropertySetters)
                {
                    propertySetter(to, record);
                }
                rows.Add(to);
            }

            return rows;
        }

        public static object ReadObject(string csv)
        {
            if (csv == null) return null; //AOT

            return Read(CsvReader.ParseLines(csv));
        }

        public static object ReadObjectRow(string csv)
        {
            if (csv == null) return null; //AOT

            return ReadRow(csv);
        }

        public static List<T> Read(List<string> rows)
        {
            var to = new List<T>();
            if (rows == null || rows.Count == 0) return to; //AOT

            if (typeof(T).IsAssignableFromType(typeof(Dictionary<string, object>)))
            {
                return CsvDictionaryWriter.ReadObjectDictionary(rows)
                    .ConvertAll(x => (T)x.FromObjectDictionary(typeof(T)));
            }


            if (OptimizedReader != null)
            {
                foreach (var row in rows)
                {
                    to.Add((T)OptimizedReader(row));
                }
                return to;
            }

            List<string> headers = null;
            if (!CsvConfig<T>.OmitHeaders || Headers.Count == 0)
                headers = CsvReader.ParseFields(rows[0]);

            if (typeof(T).IsValueType() || typeof(T) == typeof(string))
            {
                return GetSingleRow(rows, typeof(T));
            }

            for (var rowIndex = headers == null ? 0 : 1; rowIndex < rows.Count; rowIndex++)
            {
                var row = rows[rowIndex];
                var o = typeof(T).CreateInstance<T>();

                var fields = CsvReader.ParseFields(row);
                for (int i = 0; i < fields.Count; i++)
                {
                    var setter = i < PropertySetters.Count ? PropertySetters[i] : null;
                    if (headers != null)
                        PropertySettersMap.TryGetValue(headers[i], out setter);

                    if (setter == null)
                        continue;

                    var converter = i < PropertyConverters.Count ? PropertyConverters[i] : null;
                    if (headers != null)
                        PropertyConvertersMap.TryGetValue(headers[i], out converter);

                    if (converter == null)
                        continue;

                    var field = fields[i];
                    var convertedValue = converter(field);
                    setter(o, convertedValue);
                }

                to.Add(o);
            }

            return to;
        }

        public static T ReadRow(string value)
        {
            if (value == null) return default(T); //AOT

            return Read(CsvReader.ParseLines(value)).FirstOrDefault();
        }

    }
}