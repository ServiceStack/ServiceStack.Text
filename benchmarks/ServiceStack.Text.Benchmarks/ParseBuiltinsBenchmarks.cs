using System;
using System.Globalization;
using BenchmarkDotNet.Attributes;
using ServiceStack.Text;
#if NETCOREAPP1_1
using Microsoft.Extensions.Primitives;
#endif
using ServiceStack.Text.Support;

namespace ServiceStack.Text.Benchmarks
{
    public class ParseBuiltinBenchmarks
    {
        const string int32_1 = "1234";
        const string int32_2 = "-1234";
        const string decimal_1 = "1234.5678";
        const string decimal_2 = "-1234.5678";
        const string decimal_3 = "1234.5678901234567890";
        const string decimal_4 = "-1234.5678901234567890";
        const string guid_1 = "{b6170a18-3dd7-4a9b-b5d6-21033b5ad162}";

        readonly StringSegment ss_int32_1 = new StringSegment(int32_1);
        readonly StringSegment ss_int32_2 = new StringSegment(int32_2);
        readonly StringSegment ss_decimal_1 = new StringSegment(decimal_1);
        readonly StringSegment ss_decimal_2 = new StringSegment(decimal_2);
        readonly StringSegment ss_decimal_3 = new StringSegment(decimal_3);
        readonly StringSegment ss_decimal_4 = new StringSegment(decimal_4);
        readonly StringSegment ss_guid_1 = new StringSegment(guid_1);

        [Benchmark]
        public void Int32Parse()
        {
            var res1 = int.Parse(int32_1, CultureInfo.InvariantCulture);
            var res2 = int.Parse(int32_2, CultureInfo.InvariantCulture);
        }

        [Benchmark]
        public void StringSegment_Int32Parse()
        {
            var res1 = ss_int32_1.ParseInt32();
            var res2 = ss_int32_2.ParseInt32();
        }

        [Benchmark]
        public void DecimalParse()
        {
            var res1 = decimal.Parse(decimal_1, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);
            var res2 = decimal.Parse(decimal_2, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);
        }

        [Benchmark]
        public void BigDecimalParse()
        {
            var res1 = decimal.Parse(decimal_3, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);
            var res2 = decimal.Parse(decimal_4, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);
        }


        [Benchmark]
        public void StringSegment_DecimalParse()
        {
            var res1 = ss_decimal_1.ParseDecimal(true);
            var res2 = ss_decimal_2.ParseDecimal(true);
        }

        [Benchmark]
        public void StringSegment_BigDecimalParse()
        {
            var res1 = ss_decimal_3.ParseDecimal(true);
            var res2 = ss_decimal_4.ParseDecimal(true);
        }

        [Benchmark]
        public void GuidParse()
        {
            var res1 = Guid.Parse(guid_1);
        }

        [Benchmark]
        public void StringSegment_GuidParse()
        {
            var res1 = ss_guid_1.ParseGuid();
        }
    }
}