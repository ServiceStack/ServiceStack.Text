using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceStack.Text.TestsConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //var licenseKey = Environment.GetEnvironmentVariable("SERVICESTACK_LICENSE");
            //if (licenseKey.IsNullOrEmpty())
            //    throw new ArgumentNullException("SERVICESTACK_LICENSE", "Add Environment variable for SERVICESTACK_LICENSE");

            //Licensing.RegisterLicense(licenseKey);
            //"ActivatedLicenseFeatures: ".Print(LicenseUtils.ActivatedLicenseFeatures());

            Thread.CurrentThread.CurrentCulture = new CultureInfo("ar");
            JsConfig.InitStatics();
            //JsonServiceClient client = new JsonServiceClient();

            Console.ReadLine();
        }
    }
}
