using System;
using System.IO;
using ServiceStack.Web;
using Xunit;

namespace ServiceStack.Text.Tests.xUnit
{
    public class RawRequest : IRequiresRequestStream
    {
        public Stream RequestStream { get; set; }
    }

    //Temporary create xUnit project to run .NET Core tests in VS2017
    public class UnitTest1
    {
        [Fact]
        public void Can_create_DTO_with_Stream()
        {
            var o = typeof(RawRequest).CreateInstance();
            var requestObj = AutoMappingUtils.PopulateWith(o);
        }
    }
}
