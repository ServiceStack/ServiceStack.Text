using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace ServiceStack.Text.Tests.UseCases
{
	public class GitHubRestTests
	{
		private const string JsonPullRequest = @"{
  ""pulls"": [
    {
      ""state"": ""open"",
      ""base"": {
        ""label"": ""technoweenie:master"",
        ""ref"": ""master"",
        ""sha"": ""53397635da83a2f4b5e862b5e59cc66f6c39f9c6"",
      },
      ""head"": {
        ""label"": ""smparkes:synchrony"",
        ""ref"": ""synchrony"",
        ""sha"": ""83306eef49667549efebb880096cb539bd436560"",
      },
      ""discussion"": [
        {
          ""type"": ""IssueComment"",
          ""gravatar_id"": ""821395fe70906c8290df7f18ac4ac6cf"",
          ""created_at"": ""2010/10/07 07:38:35 -0700"",
          ""body"": ""Did you intend to remove net/http?  Otherwise, this looks good.  Have you tried running the LIVE tests with it?\r\n\r\n    ruby test/live_server.rb # start the demo server\r\n    LIVE=1 rake"",
          ""updated_at"": ""2010/10/07 07:38:35 -0700"",
          ""id"": 453980,
        },
        {
          ""type"": ""Commit"",
          ""created_at"": ""2010-11-04T16:27:45-07:00"",
          ""sha"": ""83306eef49667549efebb880096cb539bd436560"",
          ""author"": ""Steven Parkes"",
          ""subject"": ""add em_synchrony support"",
          ""email"": ""smparkes@smparkes.net""
        }
      ],
      ""title"": ""Synchrony"",
      ""body"": ""Here's the pull request.\r\n\r\nThis isn't generic EM: require's Ilya's synchrony and needs to be run on its own fiber, e.g., via synchrony or rack-fiberpool.\r\n\r\nI thought about a \""first class\"" em adapter, but I think the faraday api is sync right now, right? Interesting idea to add something like rack's async support to faraday, but that's an itch I don't have right now."",
      ""position"": 4.0,
      ""number"": 15,
      ""votes"": 0,
      ""comments"": 4,
      ""diff_url"": ""https://github.com/technoweenie/faraday/pull/15.diff"",
      ""patch_url"": ""https://github.com/technoweenie/faraday/pull/15.patch"",
      ""labels"": [],
      ""html_url"": ""https://github.com/technoweenie/faraday/pull/15"",
      ""issue_created_at"": ""2010-10-04T12:39:18-07:00"",
      ""issue_updated_at"": ""2010-11-04T16:35:04-07:00"",
      ""created_at"": ""2010-10-04T12:39:18-07:00"",
      ""updated_at"": ""2010-11-04T16:30:14-07:00""
    }
  ]
}";
		public class Discussion
		{
			public string Type { get; set; }
			public string GravatarId { get; set; }
			public string Body { get; set; }
			public string CreatedAt { get; set; }
			public string UpdatedAt { get; set; }

			public int? Id { get; set; }
			public string Sha { get; set; }
			public string Author { get; set; }
			public string Subject { get; set; }
			public string Email { get; set; }

		}

		T Get<T>(Dictionary<string, string> map, string key)
		{
			string strVal;
			return map.TryGetValue(key, out strVal) ? TypeSerializer.DeserializeFromString<T>(strVal) : default(T);
		}

		string GetString(Dictionary<string, string> map, string key)
		{
			string strVal;
			return map.TryGetValue(key, out strVal) ? strVal : null;
		}


		[Test]
		public void Can_parse_GitHub_discussion()
		{
			var jsonObj = JsonSerializer.DeserializeFromString<List<Dictionary<string, string>>>(JsonPullRequest);
			var jsonPulls = JsonSerializer.DeserializeFromString<List<Dictionary<string, string>>>(jsonObj[0]["pulls"]);
			var discussions = JsonSerializer.DeserializeFromString<List<Dictionary<string, string>>>(jsonPulls[0]["discussion"])
				.ConvertAll(x => new Discussion {
					Type = GetString(x, "type"),
					GravatarId = GetString(x, "gravatar_id"),
					CreatedAt = GetString(x, "created_at"),
					Body = GetString(x, "body"),
					UpdatedAt = GetString(x, "updated_at"),

					Id = Get<int?>(x, "id"),
					Sha = GetString(x, "sha"),
					Author = GetString(x, "author"),
					Subject = GetString(x, "subject"),
					Email = GetString(x, "email"),
				});

			Console.WriteLine(discussions.Dump()); //See what's been parsed
			Assert.That(discussions.ConvertAll(x => x.Type), Is.EquivalentTo(new[] { "IssueComment", "Commit" }));
		}

	}
}