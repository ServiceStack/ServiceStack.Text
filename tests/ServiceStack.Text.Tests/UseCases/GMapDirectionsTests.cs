using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using ServiceStack.Common.Extensions;

namespace ServiceStack.Text.Tests.UseCases
{
	[TestFixture]
	public class GMapDirectionsTests
	{
		static string json = @"
{
   ""routes"" : [
      {
         ""bounds"" : {
            ""northeast"" : {
               ""lat"" : 41.87811000000001,
               ""lng"" : -87.62979000000001
            },
            ""southwest"" : {
               ""lat"" : 34.052360,
               ""lng"" : -118.243560
            }
         },
         ""copyrights"" : ""Map data ©2011 Europa Technologies, Google"",
         ""legs"" : [
            {
               ""distance"" : {
                  ""text"" : ""583 mi"",
                  ""value"" : 938998
               },
               ""duration"" : {
                  ""text"" : ""9 hours 53 mins"",
                  ""value"" : 35567
               },
               ""end_address"" : ""Joplin, MO, USA"",
               ""end_location"" : {
                  ""lat"" : 37.084060,
                  ""lng"" : -94.51329000000001
               },
               ""start_address"" : ""Chicago, IL, USA"",
               ""start_location"" : {
                  ""lat"" : 41.87811000000001,
                  ""lng"" : -87.62979000000001
               },
               ""steps"" : [
                  {
                     ""distance"" : {
                        ""text"" : ""0.2 mi"",
                        ""value"" : 269
                     },
                     ""duration"" : {
                        ""text"" : ""1 min"",
                        ""value"" : 34
                     },
                     ""end_location"" : {
                        ""lat"" : 41.87570,
                        ""lng"" : -87.62969000000001
                     },
                     ""html_instructions"" : ""Head \u003cb\u003esouth\u003c/b\u003e on \u003cb\u003eS Federal St\u003c/b\u003e toward \u003cb\u003eW Van Buren St\u003c/b\u003e"",
                     ""polyline"" : {
                        ""levels"" : ""BB"",
                        ""points"" : ""eir~FdezuO`NS""
                     },
                     ""start_location"" : {
                        ""lat"" : 41.87811000000001,
                        ""lng"" : -87.62979000000001
                     },
                     ""travel_mode"" : ""DRIVING""
                  },
                  {
                     ""distance"" : {
                        ""text"" : ""0.6 mi"",
                        ""value"" : 1038
                     },
                     ""duration"" : {
                        ""text"" : ""1 min"",
                        ""value"" : 88
                     },
                     ""end_location"" : {
                        ""lat"" : 41.875640,
                        ""lng"" : -87.64223000000001
                     },
                     ""html_instructions"" : ""Turn \u003cb\u003eright\u003c/b\u003e onto \u003cb\u003eW Congress Pkwy\u003c/b\u003e"",
                     ""polyline"" : {
                        ""levels"" : ""B??B"",
                        ""points"" : ""czq~FpdzuODbTEtQJpe@""
                     },
                     ""start_location"" : {
                        ""lat"" : 41.87570,
                        ""lng"" : -87.62969000000001
                     },
                     ""travel_mode"" : ""DRIVING""
                  }
               ],
               ""via_waypoint"" : []
            },
            {
               ""distance"" : {
                  ""text"" : ""217 mi"",
                  ""value"" : 349335
               },
               ""duration"" : {
                  ""text"" : ""3 hours 30 mins"",
                  ""value"" : 12603
               },
               ""end_address"" : ""Oklahoma City, OK, USA"",
               ""end_location"" : {
                  ""lat"" : 35.46756000000001,
                  ""lng"" : -97.51647000000001
               },
               ""start_address"" : ""Joplin, MO, USA"",
               ""start_location"" : {
                  ""lat"" : 37.084060,
                  ""lng"" : -94.51329000000001
               },
               ""steps"" : [
                  {
                     ""distance"" : {
                        ""text"" : ""59 ft"",
                        ""value"" : 18
                     },
                     ""duration"" : {
                        ""text"" : ""1 min"",
                        ""value"" : 1
                     },
                     ""end_location"" : {
                        ""lat"" : 37.084060,
                        ""lng"" : -94.513490
                     },
                     ""html_instructions"" : ""Head \u003cb\u003ewest\u003c/b\u003e on \u003cb\u003eInterstate 44 Business Loop W\u003c/b\u003e toward \u003cb\u003eS Main St\u003c/b\u003e"",
                     ""polyline"" : {
                        ""levels"" : ""BB"",
                        ""points"" : ""k~iaF`sz_Q?f@""
                     },
                     ""start_location"" : {
                        ""lat"" : 37.084060,
                        ""lng"" : -94.51329000000001
                     },
                     ""travel_mode"" : ""DRIVING""
                  },
                  {
                     ""distance"" : {
                        ""text"" : ""0.3 mi"",
                        ""value"" : 485
                     },
                     ""duration"" : {
                        ""text"" : ""2 mins"",
                        ""value"" : 100
                     },
                     ""end_location"" : {
                        ""lat"" : 35.46756000000001,
                        ""lng"" : -97.51647000000001
                     },
                     ""html_instructions"" : ""Take the 2nd \u003cb\u003eleft\u003c/b\u003e onto \u003cb\u003eN Robinson Ave\u003c/b\u003e\u003cdiv style=\""font-size:0.9em\""\u003eDestination will be on the left\u003c/div\u003e"",
                     ""polyline"" : {
                        ""levels"" : ""BB"",
                        ""points"" : ""obowEbderQfZX""
                     },
                     ""start_location"" : {
                        ""lat"" : 35.471920,
                        ""lng"" : -97.51634000000003
                     },
                     ""travel_mode"" : ""DRIVING""
                  }
               ],
               ""via_waypoint"" : []
            },
            {
               ""distance"" : {
                  ""text"" : ""1,328 mi"",
                  ""value"" : 2137116
               },
               ""duration"" : {
                  ""text"" : ""20 hours 43 mins"",
                  ""value"" : 74552
               },
               ""end_address"" : ""Los Angeles, CA, USA"",
               ""end_location"" : {
                  ""lat"" : 34.052360,
                  ""lng"" : -118.243560
               },
               ""start_address"" : ""Oklahoma City, OK, USA"",
               ""start_location"" : {
                  ""lat"" : 35.46756000000001,
                  ""lng"" : -97.51647000000001
               },
               ""steps"" : [
                  {
                     ""distance"" : {
                        ""text"" : ""0.3 mi"",
                        ""value"" : 533
                     },
                     ""duration"" : {
                        ""text"" : ""1 min"",
                        ""value"" : 89
                     },
                     ""end_location"" : {
                        ""lat"" : 35.462780,
                        ""lng"" : -97.516220
                     },
                     ""html_instructions"" : ""Head \u003cb\u003esouth\u003c/b\u003e on \u003cb\u003eN Robinson Ave\u003c/b\u003e toward \u003cb\u003eW Sheridan Ave\u003c/b\u003e"",
                     ""polyline"" : {
                        ""levels"" : ""B?B"",
                        ""points"" : ""ggnwE|derQvSKbHe@""
                     },
                     ""start_location"" : {
                        ""lat"" : 35.46756000000001,
                        ""lng"" : -97.51647000000001
                     },
                     ""travel_mode"" : ""DRIVING""
                  },
                  {
                     ""distance"" : {
                        ""text"" : ""338 ft"",
                        ""value"" : 103
                     },
                     ""duration"" : {
                        ""text"" : ""1 min"",
                        ""value"" : 40
                     },
                     ""end_location"" : {
                        ""lat"" : 34.052360,
                        ""lng"" : -118.243560
                     },
                     ""html_instructions"" : ""Turn \u003cb\u003eleft\u003c/b\u003e onto \u003cb\u003eW 1st St\u003c/b\u003e\u003cdiv style=\""font-size:0.9em\""\u003eDestination will be on the right\u003c/div\u003e"",
                     ""polyline"" : {
                        ""levels"" : ""BB"",
                        ""points"" : ""{}ynEvrupUrBoD""
                     },
                     ""start_location"" : {
                        ""lat"" : 34.05294000000001,
                        ""lng"" : -118.244440
                     },
                     ""travel_mode"" : ""DRIVING""
                  }
               ],
               ""via_waypoint"" : []
            }
         ],
         ""overview_polyline"" : {
            ""levels"" : ""BBBAAAAABAABAAAAAABBAAABBAAAABBAAABABAAABABBAABAABAAAABABABABBABAABB"",
            ""points"" : ""eir~FdezuOren@|rfBtc~@tsE`vnApw{A`dw@~w\\buN|pf@f{Y|_Fblh@rxo@b}@xxS~xtAllk@`yaBoJxlcBb~t@zbh@jc|Bx}C`rv@rw|@rlhA~dVzeo@vrSnc}Axf]fjz@xfFbw~@dz{A~d{A|zOxbrBbdUvpo@`cFp~xBc`Hk@nurDznmFfwMbwz@bbl@lq~@loPpxq@bw_@v|{CvzY|}OelMdhaF|n\\~mbDzeVh_Wr|Efc\\x`Ij{kE}mAb~uF{cNd}xBjp]fulBiwJpgg@|kHntyArpb@bijCk_Kv~eGyqTj_|@`uV`k|DcsNdwxAknt@zpq@mmc@lbaCxvHdak@dse@x{p@zpiAp|gEicy@`omFbaEnko@ufQ|ilApqGze~AsyRzrjAb__@ftyBooIhr_BxjmAbwQftNboWzoAlzp@mz`@|}_@fda@jakEitAn{fB_a]lexClshBtmqAdmY_hLxiZd~XtaBndgC""
         },
         ""summary"" : ""I-40 W"",
         ""warnings"" : [],
         ""waypoint_order"" : [ 0, 1 ]
      }
   ],
   ""status"" : ""OK""
}
";

		[Test]
		public void Can_parse_GMaps_directions_json ()
		{
			var results = JsonObject.Parse(json).ConvertTo(x=> new RouteResult
			{
				Status = x.Get("status"),
				Routes = x.ArrayObjects("routes").ConvertAll(r=>new Route
				{
					Overview_Polyline = r.Object("overview_polyline").ConvertTo(p => 
						new Polyline
						{
							Points = p.Get("points")
						})
				})
			});
			
			Console.WriteLine("out: " + results.ToJson());
		}

		public class RouteResult
		{
			public string Status { get; set; }

			public List<Route> Routes { get; set; }
		}

		public class Route
		{
			public Polyline Overview_Polyline { get; set; }
		}

		public class Polyline
		{
			public string Points { get; set; }

			public string Levels { get; set; }	
		}

	}
}

