using NUnit.Framework;

namespace ServiceStack.Text.Tests
{
    [TestFixture]
    public class JsonObjectTests : TestBase
    {
        private const string JsonCentroid = @"{""place"":{ ""woeid"":12345, ""placeTypeName"":""St\\a\/te"" } }";

        [Test]
        public void Can_dynamically_parse_JSON_with_escape_chars()
        {
            var placeTypeName = JsonObject.Parse(JsonCentroid).Object("place").Get("placeTypeName");
            Assert.That(placeTypeName, Is.EqualTo("St\\a/te"));

            placeTypeName = JsonObject.Parse(JsonCentroid).Object("place").Get<string>("placeTypeName");
            Assert.That(placeTypeName, Is.EqualTo("St\\a/te"));
        }

        private const string JsonEscapedByteArray = @"{""universalId"":""09o4bFTeBq3hTKhoJVCkzSLRG\/o1SktTPqxgZ3L3Xss=""}";

        [Test]
        public void Can_dynamically_parse_JSON_with_escape_byte_array()
        {
            var parsed = JsonObject.Parse(JsonEscapedByteArray).Get<byte[]>("universalId");
            Assert.That(parsed, Is.EqualTo(new byte[] {
                0xd3, 0xda, 0x38, 0x6c, 0x54, 0xde, 0x06, 0xad,
                0xe1, 0x4c, 0xa8, 0x68, 0x25, 0x50, 0xa4, 0xcd,
                0x22, 0xd1, 0x1b, 0xfa, 0x35, 0x4a, 0x4b, 0x53,
                0x3e, 0xac, 0x60, 0x67, 0x72, 0xf7, 0x5e, 0xcb}));
        }

        [Test]
        public void Does_escape_string_access()
        {
            string test = "\"quoted string\"";
            var json = JsonSerializer.SerializeToString(new { a = test });
            var jsonObject = JsonObject.Parse(json);

            var actual = jsonObject["a"];
            Assert.That(actual, Is.EqualTo(test));
            Assert.That(jsonObject.Get("a"), Is.EqualTo(test));
            Assert.That(jsonObject.Get<string>("a"), Is.EqualTo(test));

            Assert.That(jsonObject.GetUnescaped("a"), Is.EqualTo(test.Replace("\"","\\\"")));
        }

        [Test]
        public void Does_encode_unicode()
        {
            string test = "<\"I get this : 􏰁􏰂􏰃􏰄􏰂􏰅􏰆􏰇􏰈􏰀􏰉􏰊􏰇􏰋􏰆􏰌􏰀􏰆􏰊􏰀􏰍􏰄􏰎􏰆􏰏􏰐􏰑􏰑􏰆􏰒􏰆􏰂􏰊􏰀";
            var obj = new { test };
            using (var mem = new System.IO.MemoryStream())
            {
                ServiceStack.Text.JsonSerializer.SerializeToStream(obj, obj.GetType(), mem);

                var encoded = System.Text.Encoding.UTF8.GetString(mem.ToArray());

                var copy1 = JsonObject.Parse(encoded);

                Assert.That(test, Is.EqualTo(copy1["test"]));

                System.Diagnostics.Debug.WriteLine(copy1["test"]);
            }
        }

        [Test]
        public void Can_parse_Twitter_response()
        {
            var json = @"[{""is_translator"":false,""geo_enabled"":false,""profile_background_color"":""000000"",""protected"":false,""default_profile"":false,""profile_background_tile"":false,""created_at"":""Sun Nov 23 17:42:51 +0000 2008"",""name"":""Demis Bellot TW"",""profile_background_image_url_https"":""https:\/\/si0.twimg.com\/profile_background_images\/192991651\/twitter-bg.jpg"",""profile_sidebar_fill_color"":""2A372F"",""listed_count"":36,""notifications"":null,""utc_offset"":0,""friends_count"":267,""description"":""StackExchangarista, JavaScript, C#, Web & Mobile developer. Creator of the ServiceStack.NET projects. "",""following"":null,""verified"":false,""profile_sidebar_border_color"":""D9D082"",""followers_count"":796,""profile_image_url"":""http:\/\/a2.twimg.com\/profile_images\/1598852740\/avatar_normal.png"",""contributors_enabled"":false,""profile_image_url_https"":""https:\/\/si0.twimg.com\/profile_images\/1598852740\/avatar_normal.png"",""status"":{""possibly_sensitive"":false,""place"":null,""retweet_count"":37,""in_reply_to_screen_name"":null,""created_at"":""Mon Nov 07 02:34:23 +0000 2011"",""retweeted"":false,""in_reply_to_status_id_str"":null,""in_reply_to_user_id_str"":null,""contributors"":null,""id_str"":""133371690876022785"",""retweeted_status"":{""possibly_sensitive"":false,""place"":null,""retweet_count"":37,""in_reply_to_screen_name"":null,""created_at"":""Mon Nov 07 02:32:15 +0000 2011"",""retweeted"":false,""in_reply_to_status_id_str"":null,""in_reply_to_user_id_str"":null,""contributors"":null,""id_str"":""133371151551447041"",""in_reply_to_user_id"":null,""in_reply_to_status_id"":null,""source"":""\u003Ca href=\""http:\/\/www.arstechnica.com\"" rel=\""nofollow\""\u003EArs auto-tweeter\u003C\/a\u003E"",""geo"":null,""favorited"":false,""id"":133371151551447041,""coordinates"":null,""truncated"":false,""text"":""Google: Microsoft uses patents when products \""stop succeeding\"": http:\/\/t.co\/50QFc1uJ by @binarybits""},""in_reply_to_user_id"":null,""in_reply_to_status_id"":null,""source"":""web"",""geo"":null,""favorited"":false,""id"":133371690876022785,""coordinates"":null,""truncated"":false,""text"":""RT @arstechnica: Google: Microsoft uses patents when products \""stop succeeding\"": http:\/\/t.co\/50QFc1uJ by @binarybits""},""profile_use_background_image"":true,""favourites_count"":238,""location"":""New York"",""id_str"":""17575623"",""default_profile_image"":false,""show_all_inline_media"":false,""profile_text_color"":""ABB8AF"",""screen_name"":""demisbellot"",""statuses_count"":9638,""profile_background_image_url"":""http:\/\/a0.twimg.com\/profile_background_images\/192991651\/twitter-bg.jpg"",""url"":""http:\/\/www.servicestack.net\/mythz_blog\/"",""time_zone"":""London"",""profile_link_color"":""43594A"",""id"":17575623,""follow_request_sent"":null,""lang"":""en""}]";
            var objs = JsonObject.ParseArray(json);
            var obj = objs[0];

            Assert.That(obj.Get("name"), Is.EqualTo("Demis Bellot TW"));
        }
    }
}