using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ServiceStack.Text.Tests.Support;

namespace ServiceStack.Text.Tests.JsonTests
{
    class BlockBuster
    {
        public BlockBuster(string address)
        {
            this.Address = address;
            this.Movies = new List<Movie>();
        }

        public string Address { get; set; }
        public List<Movie> Movies { get; set; }
    }


    [TestFixture]
    class InheritanceTests
    {
        [Test]
        public void Can_serialize_class_with_list_that_classes_inherited_from_non_abstract_class_correctly()
        {
            var child = new MovieChild { ImdbId = "tt0068646", Title = "The Godfather", Rating = 9.2m, Director = "Francis Ford Coppola", ReleaseDate = new DateTime(1972, 3, 24), TagLine = "An offer you can't refuse.", Genres = new List<string> { "Crime", "Drama", "Thriller" }, };
            child.Oscar.Add("Best Picture - 1972");
            child.Oscar.Add("Best Actor - 1972");
            child.Oscar.Add("Best Adapted Screenplay - 1972");

            var blockBuster = new BlockBuster("Av. República do Líbano, 2175 - Indinópolis, São Paulo - SP, 04502-300");
            blockBuster.Movies.Add(MoviesData.Movies[0]);
            blockBuster.Movies.Add(child);

            // serialize to JSON using ServiceStack
            string jsonString = JsonSerializer.SerializeToString(blockBuster);

            const string oldWaay = "{\"Address\":\"Av. República do Líbano, 2175 - Indinópolis, São Paulo - SP, 04502-300\",\"Movies\":[{\"Title\":\"The Shawshank Redemption\",\"ImdbId\":\"tt0111161\",\"Rating\":9.2,\"Director\":\"Frank Darabont\",\"ReleaseDate\":\"\\/Date(792990000000-0000)\\/\",\"TagLine\":\"Fear can hold you prisoner. Hope can set you free.\",\"Genres\":[\"Crime\",\"Drama\"]},{\"Title\":\"The Godfather\",\"ImdbId\":\"tt0068646\",\"Rating\":9.2,\"Director\":\"Francis Ford Coppola\",\"ReleaseDate\":\"\\/Date(70254000000-0000)\\/\",\"TagLine\":\"An offer you can't refuse.\",\"Genres\":[\"Crime\",\"Drama\",\"Thriller\"]}]}";
            const string correct = "{\"Address\":\"Av. República do Líbano, 2175 - Indinópolis, São Paulo - SP, 04502-300\",\"Movies\":[{\"Title\":\"The Shawshank Redemption\",\"ImdbId\":\"tt0111161\",\"Rating\":9.2,\"Director\":\"Frank Darabont\",\"ReleaseDate\":\"\\/Date(792990000000-0000)\\/\",\"TagLine\":\"Fear can hold you prisoner. Hope can set you free.\",\"Genres\":[\"Crime\",\"Drama\"]},{\"__type\":\"ServiceStack.Text.Tests.Support.MovieChild, ServiceStack.Text.Tests\",\"Oscar\":[\"Best Picture - 1972\",\"Best Actor - 1972\",\"Best Adapted Screenplay - 1972\"],\"Title\":\"The Godfather\",\"ImdbId\":\"tt0068646\",\"Rating\":9.2,\"Director\":\"Francis Ford Coppola\",\"ReleaseDate\":\"\\/Date(70254000000-0000)\\/\",\"TagLine\":\"An offer you can't refuse.\",\"Genres\":[\"Crime\",\"Drama\",\"Thriller\"]}]}";

            Assert.AreEqual(correct, jsonString, "Service stack serialized wrong");
            Assert.IsFalse(oldWaay.Equals(correct));

        }

        [Test]
        public void Can_set_that_class_is_abstract_in_jsonconfig()
        {
            JsConfig<Movie>.TreatAsAbstract = true;

            var jsonString = JsonSerializer.SerializeToString(MoviesData.Movies[0]);

            Assert.True(jsonString.Contains("__type"));
        }

    }


}
