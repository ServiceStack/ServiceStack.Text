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

        readonly StringSegment ss_int32_1 = new StringSegment(int32_1);
        readonly StringSegment ss_int32_2 = new StringSegment(int32_2);
        readonly StringSegment ss_decimal_1 = new StringSegment(decimal_1);
        readonly StringSegment ss_decimal_2 = new StringSegment(decimal_2);

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
        public void StringSegment_DecimalParse()
        {
            var res1 = ss_decimal_1.ParseDecimal(true);
            var res2 = ss_decimal_2.ParseDecimal(true);
        }
    }
}