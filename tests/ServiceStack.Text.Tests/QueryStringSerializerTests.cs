using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ServiceStack.Text.Tests
{
	public class C
	{
		public int? A { get; set; }
		public int? B { get; set; }
	}

	[TestFixture]
	public class QueryStringSerializerTests
	{
        class D
        {
            public string A { get; set; }
            public string B { get; set; }
        }

		[Test]
		public void Can_serialize_query_string()
		{
			Assert.That(QueryStringSerializer.SerializeToString(new C { A = 1, B = 2 }),
				Is.EqualTo("A=1&B=2"));

			Assert.That(QueryStringSerializer.SerializeToString(new C { A = null, B = 2 }),
				Is.EqualTo("B=2"));
		}

        [Test]
        public void Can_Serialize_Unicode_Query_String()
        {
            Assert.That(QueryStringSerializer.SerializeToString(new D { A = "믬㼼摄䰸蠧蛛㙷뇰믓堐锗멮ᙒ덃", B = "八敁喖䉬ڵẀ똦⌀羭䥀主䧒蚭㾐타" }), 
                Is.EqualTo("A=%eb%af%ac%e3%bc%bc%e6%91%84%e4%b0%b8%e8%a0%a7%e8%9b%9b%e3%99%b7%eb%87%b0%eb%af%93%e5%a0" +
                "%90%e9%94%97%eb%a9%ae%e1%99%92%eb%8d%83&B=%e5%85%ab%e6%95%81%e5%96%96%e4%89%ac%da%b5%e1%ba%80%eb%98%a6%e2%8c%80%e7%be%ad%e4" +
                "%a5%80%e4%b8%bb%e4%a7%92%e8%9a%ad%e3%be%90%ed%83%80"));

            Assert.That(QueryStringSerializer.SerializeToString(new D { A = "崑⨹堡ꁀᢖ㤹ì㭡줪銬", B = null }),
                Is.EqualTo("A=%e5%b4%91%e2%a8%b9%e5%a0%a1%ea%81%80%e1%a2%96%e3%a4%b9%c3%ac%e3%ad%a1%ec%a4%aa%e9%8a%ac"));
        }

        class Empty {}

        [Test]
        public void Can_serialize_empty_object()
        {
            Assert.That(QueryStringSerializer.SerializeToString(new Empty()), Is.Empty);
        }

	    [Test]
	    public void Can_serialize_newline()
	    {
            Assert.That(QueryStringSerializer.SerializeToString(new {newline = "\r\n"}), Is.EqualTo("newline=%0d%0a"));
	    }

	    [Test]
	    public void Can_serialize_tab()
	    {
            Assert.That(QueryStringSerializer.SerializeToString(new { tab = "\t" }), Is.EqualTo("tab=%09"));
	    }

		// NOTE: QueryStringSerializer doesn't have Deserialize, but this is how QS is parsed in ServiceStack
	    [Test]
	    public void Can_deserialize_query_string_nullableInt_null_yields_null()
	    {
	    	Assert.That(ServiceStack.Text.Common.DeserializeBuiltin<int?>.Parse(null), Is.EqualTo(null));
	    }

	    [Test]
	    public void Can_deserialize_query_string_nullableInt_empty_yields_null()
	    {
	    	Assert.That(ServiceStack.Text.Common.DeserializeBuiltin<int?>.Parse(string.Empty), Is.EqualTo(null));
	    }

	    [Test]
	    public void Can_deserialize_query_string_nullableInt_intValues_yields_null()
	    {
	    	Assert.That(ServiceStack.Text.Common.DeserializeBuiltin<int?>.Parse(int.MaxValue.ToString()), Is.EqualTo(int.MaxValue));
            Assert.That(ServiceStack.Text.Common.DeserializeBuiltin<int?>.Parse(int.MinValue.ToString()), Is.EqualTo(int.MinValue));
	    	Assert.That(ServiceStack.Text.Common.DeserializeBuiltin<int?>.Parse(0.ToString()), Is.EqualTo(0));
	    	Assert.That(ServiceStack.Text.Common.DeserializeBuiltin<int?>.Parse((-1).ToString()), Is.EqualTo(-1));
	    	Assert.That(ServiceStack.Text.Common.DeserializeBuiltin<int?>.Parse(1.ToString()), Is.EqualTo(1));
	    }

	    [Test]
	    public void Can_deserialize_query_string_nullableInt_NaN_throws()
	    {
	    	Assert.Throws(typeof(FormatException), delegate { ServiceStack.Text.Common.DeserializeBuiltin<int?>.Parse("NaN"); });
    	}
	}
}