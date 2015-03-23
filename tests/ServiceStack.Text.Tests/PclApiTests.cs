using NUnit.Framework;

namespace ServiceStack.Text.Tests
{
    [TestFixture]
    public class PclApiTests
    {
        [Test, Explicit]
        public void Can_scan_reference_assemblies()
        {
            var refDir = @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\";

            PclExport.Instance.GetDirectoryNames(refDir).PrintDump();
            PclExport.Instance.GetDirectoryNames(refDir, "v4*").PrintDump();
        }
    }
}