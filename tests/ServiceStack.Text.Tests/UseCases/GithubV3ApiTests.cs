using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ServiceStack.Common;

namespace ServiceStack.Text.Tests.UseCases
{
    [TestFixture]
    public class GithubV3ApiGatewayTests
    {
        [Test]
        public void DO_ALL_THE_THINGS()
        {
            var client = new GithubV3ApiGateway();

            Console.WriteLine("\n-- GetUserRepos(mythz): \n" + client.GetUserRepos("mythz").Dump());
            Console.WriteLine("\n-- GetOrgRepos(ServiceStack): \n" + client.GetOrgRepos("ServiceStack").Dump());
            Console.WriteLine("\n-- GetUserRepo(ServiceStack,ServiceStack.Text): \n" + client.GetUserRepo("ServiceStack", "ServiceStack.Text").Dump());
            Console.WriteLine("\n-- GetAllUserAndOrgsRepos(mythz): \n" + client.GetAllUserAndOrgsRepos("mythz").Dump());
        }
    }

    public class GithubV3ApiGateway
    {
        public const string GithubApiBaseUrl = "https://api.github.com/";

        public List<GithubRepo> GetUserRepos(string githubUsername)
        {
            return GithubApiBaseUrl.CombineWith("users/{0}/repos".Fmt(githubUsername))
                .DownloadJsonFromUrl()
                .FromJson<List<GithubRepo>>();
        }

        public List<GithubRepo> GetOrgRepos(string githubUsername)
        {
            return GithubApiBaseUrl.CombineWith("orgs/{0}/repos".Fmt(githubUsername))
                .DownloadJsonFromUrl()
                .FromJson<List<GithubRepo>>();
        }

        public GithubRepo GetUserRepo(string githubUsername, string projectName)
        {
            return GithubApiBaseUrl.CombineWith("users/{0}/repos".Fmt(githubUsername))
                .DownloadJsonFromUrl()
                .FromJson<GithubRepo>();
        }

        public List<GithubOrg> GetUserOrgs(string githubUsername)
        {
            return GithubApiBaseUrl.CombineWith("users/{0}/orgs".Fmt(githubUsername))
                .DownloadJsonFromUrl()
                .FromJson<List<GithubOrg>>();
        }

        public List<GithubRepo> GetAllUserAndOrgsRepos(string githubUsername)
        {
            var map = new Dictionary<int, GithubRepo>();
            GetUserRepos(githubUsername).ForEach(x => map[x.Id] = x);

            GetUserOrgs(githubUsername).ForEach(org =>
                GetOrgRepos(org.Login)
                    .ForEach(repo => map[repo.Id] = repo));

            return map.Values.ToList();
        }
    }

    public class GithubRepo
    {
        public int Id { get; set; }
        public string Open_Issues { get; set; }
        public int Watchers { get; set; }
        public DateTime? Pushed_At { get; set; }
        public string Homepage { get; set; }
        public string Svn_Url { get; set; }
        public DateTime? Updated_At { get; set; }
        public string Mirror_Url { get; set; }
        public bool Has_Downloads { get; set; }
        public string Url { get; set; }
        public bool Has_issues { get; set; }
        public string Language { get; set; }
        public bool Fork { get; set; }
        public string Ssh_Url { get; set; }
        public string Html_Url { get; set; }
        public int Forks { get; set; }
        public string Clone_Url { get; set; }
        public int Size { get; set; }
        public string Git_Url { get; set; }
        public bool Private { get; set; }
        public DateTime Created_at { get; set; }
        public bool Has_Wiki { get; set; }
        public GithubUser Owner { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class GithubUser
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Avatar_Url { get; set; }
        public string Url { get; set; }
        public int? Followers { get; set; }
        public int? Following { get; set; }
        public string Type { get; set; }
        public int? Public_Gists { get; set; }
        public string Location { get; set; }
        public string Company { get; set; }
        public string Html_Url { get; set; }
        public int? Public_Repos { get; set; }
        public DateTime? Created_At { get; set; }
        public string Blog { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public bool? Hireable { get; set; }
        public string Gravatar_Id { get; set; }
        public string Bio { get; set; }
    }

    public class GithubOrg
    {
        public int Id { get; set; }
        public string Avatar_Url { get; set; }
        public string Url { get; set; }
        public string Login { get; set; }
    }
}