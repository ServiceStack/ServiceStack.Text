using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;

namespace ServiceStack.Text.Tests.UseCases
{
    public class GithubRepository
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string Homepage { get; set; }
        public string Language { get; set; }
        public int Watchers { get; set; }
        public int Forks { get; set; }
    }

    [TestFixture]
    public class ServiceStack_Text_UseCase
    {
        [Test, Explicit]
        public void Dump_and_Write_GitHub_Organization_Repos_to_CSV()
        {
            var orgName = "ServiceStack";

            var orgRepos = "https://api.github.com/orgs/{0}/repos".Fmt(orgName)
                .GetJsonFromUrl(httpReq => httpReq.UserAgent = "ServiceStack.Text")
                .FromJson<List<GithubRepository>>();

            "Writing {0} Github Repositories:".Print(orgName);
            orgRepos.PrintDump(); //recursive, pretty-format dump of any C# POCOs

            var csvFilePath = "~/{0}-repos.csv".Fmt(orgName).MapAbsolutePath();
            File.WriteAllText(csvFilePath, orgRepos.ToCsv());

            Process.Start(csvFilePath);
        }
    }
}