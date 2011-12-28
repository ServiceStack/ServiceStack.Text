using System;
using System.Collections.Generic;
using NUnit.Framework;
using ServiceStack.Text.Tests.Support;

namespace ServiceStack.Text.Tests.JsonTests
{
	[TestFixture]
	public class CamelCaseTests
	{
		[SetUp]
		public void SetUp()
		{
			JsConfig.EmitCamelCaseNames = true;
		}

		[SetUp]
		public void TearDown()
		{
			JsConfig.Reset();
		}

		[Test]
		public void Does_serialize_To_CamelCase()
		{
			var dto = new Movie {
				ImdbId = "tt0111161",
				Title = "The Shawshank Redemption",
				Rating = 9.2m,
				Director = "Frank Darabont",
				ReleaseDate = new DateTime(1995, 2, 17),
				TagLine = "Fear can hold you prisoner. Hope can set you free.",
				Genres = new List<string> { "Crime", "Drama" },
			};

			var json = dto.ToJson();

			Assert.That(json, Is.EqualTo(
				"{\"id\":0,\"imdbId\":\"tt0111161\",\"title\":\"The Shawshank Redemption\",\"rating\":9.2,\"director\":\"Frank Darabont\",\"releaseDate\":\"\\/Date(792997200000+0000)\\/\",\"tagLine\":\"Fear can hold you prisoner. Hope can set you free.\",\"genres\":[\"Crime\",\"Drama\"]}"));
		}
	}
}