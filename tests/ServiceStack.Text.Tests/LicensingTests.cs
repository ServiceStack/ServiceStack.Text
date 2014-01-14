// Copyright (c) Service Stack LLC. All Rights Reserved.
// License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

using System;
using System.Collections;
using NUnit.Framework;

namespace ServiceStack.Text.Tests
{
    public class LicenseUseCase
    {
        public LicenseUseCase(LicenseFeature licenseFeature, QuotaType quotaType, int allowedLimit)
        {
            Feature = licenseFeature;
            QuotaType = quotaType;
            AllowedLimit = allowedLimit;
        }

        public LicenseFeature Feature { get; set; }
        public QuotaType QuotaType { get; set; }
        public int AllowedLimit { get; set; }
    }

    [TestFixture]
    public class LicensingTests
    {
        const string TestBusiness2000Text = "1001-e1JlZjoxMDAxLE5hbWU6VGVzdCBCdXNpbmVzcyxUeXBlOkJ1c2luZXNzLEhhc2g6dE14c3BSSVV0QkxLUFFIc0ZkdHhXanU3cGdpeDJRQnJDTjllbSs2ZWVrNzFMcTUrWnliN2F4LzRDU1B1WHl4OFR4RnpBc3lIS012WVVIeG5TYXRlRGNpK0FnVTdoME0rNFhETzdCT2hSdTNELzUzN3hMa0hleXBCcWFBMVVIalV4d3dIUzVqc25lZmVvUDEwcnhNSFhzeFdaUFBUcFdmV2o5V0dUQWZvNDE4PSxFeHBpcnk6MjAwMC0wMS0wMX0=";
        readonly LicenseKey TestBusiness2000 = new LicenseKey { Ref = "1001", Name = "Test Business", Type = LicenseType.Business, Expiry = new DateTime(2000, 01, 01) };
        const string TestIndie2000Text = "1001-e1JlZjoxMDAxLE5hbWU6VGVzdCBJbmRpZSxUeXBlOkluZGllLEhhc2g6SFdMNytKMWl2ZHN2SUpVOEpDTnVtL3NEZkIyUEQyOXM4OGsxcTJtK1J3V0tEbDdiN2ZZTFZnakVvN21xL3F1MTlnQzZKUVBxSjF3TzZIMFA4Y2U3YzkvTTB0YmE3azBHU2liUkdBa1Q0czJkVkIzNHNCVFQ2cHNLVlVZZWFBUmlWOVJ6UWJlMU0rbjVpTkpnM1ZOVHBXdlcrQlJnYVFraXNNOHZmS2EwVjIwPSxFeHBpcnk6MjAwMC0wMS0wMX0=";
        readonly LicenseKey TestIndie2000 = new LicenseKey { Ref = "1001", Name = "Test Indie", Type = LicenseType.Indie, Expiry = new DateTime(2000, 01, 01) };
        const string TestBusiness2013Text = "1001-e1JlZjoxMDAxLE5hbWU6VGVzdCBCdXNpbmVzcyxUeXBlOkJ1c2luZXNzLEhhc2g6UHVNTVRPclhvT2ZIbjQ5MG5LZE1mUTd5RUMzQnBucTFEbTE3TDczVEF4QUNMT1FhNXJMOWkzVjFGL2ZkVTE3Q2pDNENqTkQyUktRWmhvUVBhYTBiekJGUUZ3ZE5aZHFDYm9hL3lydGlwUHI5K1JsaTBYbzNsUC85cjVJNHE5QVhldDN6QkE4aTlvdldrdTgyTk1relY2eis2dFFqTThYN2lmc0JveHgycFdjPSxFeHBpcnk6MjAxMy0wMS0wMX0=";
        readonly LicenseKey TestBusiness2013 = new LicenseKey { Ref = "1001", Name = "Test Business", Type = LicenseType.Business, Expiry = new DateTime(2013, 01, 01) };
        const string TestIndie2013Text = "1001-e1JlZjoxMDAxLE5hbWU6VGVzdCBJbmRpZSxUeXBlOkluZGllLEhhc2g6UGJyTWlRL205YkpCN3NoUGFBelZKVkppZERHRjQwK2JiVWpvOWtrLzgrTUF3UmZZOE0rUkNHMTRYZ055S2ZFT29aNDY4c0FXS2dLRGlVZzEvVmViNjN5M2FpNTh5T2JTZ3RIL2tEdzhDL1VFOEZrazRhMEMrdEtNVU4xQlFxVHBEU21HQUZESUxuOHQ1M2lFWE9tK014MWZCNFEvbitFQUJTMVhvbjBlUE1zPSxFeHBpcnk6MjAxNC0wMS0wMX0=";
        readonly LicenseKey TestIndie2013 = new LicenseKey { Ref = "1001", Name = "Test Indie", Type = LicenseType.Indie, Expiry = new DateTime(2014, 01, 01) };
        const string TestText2013Text = "1001-e1JlZjoxMDAxLE5hbWU6VGVzdCBUZXh0LFR5cGU6VGV4dCxIYXNoOkppVjBGdmxMcmRxa3hsN3BiZGFmK3BTc1BHak53QS9Ha245ejJ6dklOUkIwZGpOS2xXNzl0WTdZWWhLN1NRYk1WMXNyOUVyUE1DRGZzVXZ6N2tlcGl6ZzF0QkZ4eXNPb3NFUlJkdzZUM05uY1ZKRW1JTzFSSlpEOC85R2FsakZRQzJIYXFHMTl4ejF6V0NiNzFCM05BbzZOS0tLMW9Rc3QzZitENzI2YXgwST0sRXhwaXJ5OjIwMTMtMDEtMDF9";
        readonly LicenseKey TestText2013 = new LicenseKey { Ref = "1001", Name = "Test Text", Type = LicenseType.Text, Expiry = new DateTime(2013, 01, 01) };

        public IEnumerable AllLicenseUseCases
        {
            get
            {
                return new[]
                {
                    new LicenseUseCase(LicenseFeature.Text, QuotaType.Types, LicenseUtils.FreeQuotas.TextTypes),
                    new LicenseUseCase(LicenseFeature.Redis, QuotaType.Types, LicenseUtils.FreeQuotas.RedisTypes),
                    new LicenseUseCase(LicenseFeature.OrmLite, QuotaType.Tables, LicenseUtils.FreeQuotas.OrmLiteTables),
                    new LicenseUseCase(LicenseFeature.ServiceStack, QuotaType.Operations, LicenseUtils.FreeQuotas.ServiceStackOperations),
                    new LicenseUseCase(LicenseFeature.Admin, QuotaType.PremiumFeature, LicenseUtils.FreeQuotas.PremiumFeature),
                    new LicenseUseCase(LicenseFeature.Premium, QuotaType.PremiumFeature, LicenseUtils.FreeQuotas.PremiumFeature),
                };
            }
        }

        [Test, TestCaseSource("AllLicenseUseCases")]
        public void Allows_access_to_all_use_cases_with_All_License(LicenseUseCase licenseUseCase)
        {
            LicenseUtils.ApprovedUsage(LicenseFeature.All, licenseUseCase.Feature, licenseUseCase.AllowedLimit, int.MinValue, "Failed");
            LicenseUtils.ApprovedUsage(LicenseFeature.All, licenseUseCase.Feature, licenseUseCase.AllowedLimit, 0, "Failed");
            LicenseUtils.ApprovedUsage(LicenseFeature.All, licenseUseCase.Feature, licenseUseCase.AllowedLimit, int.MaxValue, "Failed");
        }

        [Test, TestCaseSource("AllLicenseUseCases")]
        public void Allows_access_on_all_use_cases_with_no_or_max_allowed_usage_and_no_license(LicenseUseCase licenseUseCase)
        {
            LicenseUtils.ApprovedUsage(LicenseFeature.None, licenseUseCase.Feature, licenseUseCase.AllowedLimit, int.MinValue, "Failed");
            LicenseUtils.ApprovedUsage(LicenseFeature.None, licenseUseCase.Feature, licenseUseCase.AllowedLimit, 0, "Failed");
            LicenseUtils.ApprovedUsage(LicenseFeature.None, licenseUseCase.Feature, licenseUseCase.AllowedLimit, licenseUseCase.AllowedLimit, "Failed");
        }

        [Test, TestCaseSource("AllLicenseUseCases")]
        public void Throws_on_all_use_cases_with_exceeded_usage_and_no_license(LicenseUseCase licenseUseCase)
        {
            Assert.Throws<LicenseException>(() =>
                LicenseUtils.ApprovedUsage(LicenseFeature.None, licenseUseCase.Feature, licenseUseCase.AllowedLimit, licenseUseCase.AllowedLimit + 1, "Failed"));

            Assert.Throws<LicenseException>(() =>
                LicenseUtils.ApprovedUsage(LicenseFeature.None, licenseUseCase.Feature, licenseUseCase.AllowedLimit, int.MaxValue, "Failed"));
        }

        [Test, Explicit("Licenses are expired")]
        public void Can_register_Text_License()
        {
            Licensing.RegisterLicense(TestText2013Text);

            var licensedFeatures = LicenseUtils.ActivatedLicenseFeatures();

            Assert.That(licensedFeatures, Is.EqualTo(LicenseFeature.Text));

            Assert.Throws<LicenseException>(() =>
                LicenseUtils.ApprovedUsage(LicenseFeature.None, LicenseFeature.Text, 1, 2, "Failed"));

            Assert.Throws<LicenseException>(() =>
                LicenseUtils.ApprovedUsage(LicenseFeature.OrmLite, LicenseFeature.Text, 1, 2, "Failed"));

            LicenseUtils.ApprovedUsage(LicenseFeature.Text, LicenseFeature.Text, 1, 2, "Failed");
        }

        [Test, Explicit("Licenses are expired")]
        public void Can_register_valid_licenses()
        {
            Licensing.RegisterLicense(TestBusiness2013Text);
            Assert.That(LicenseUtils.ActivatedLicenseFeatures(), Is.EqualTo(LicenseFeature.Business));

            Licensing.RegisterLicense(TestIndie2013Text);
            Assert.That(LicenseUtils.ActivatedLicenseFeatures(), Is.EqualTo(LicenseFeature.Indie));
        }

        [Test]
        public void Can_register_valid_license()
        {
#if !SL5
            Licensing.RegisterLicense(new ServiceStack.Configuration.AppSettings().GetString("servicestack:license"));
#else
            Licensing.RegisterLicense("1001-e1JlZjoxMDAxLE5hbWU6VGVzdCBCdXNpbmVzcyxUeXBlOkJ1c2luZXNzLEhhc2g6UHVNTVRPclhvT2ZIbjQ5MG5LZE1mUTd5RUMzQnBucTFEbTE3TDczVEF4QUNMT1FhNXJMOWkzVjFGL2ZkVTE3Q2pDNENqTkQyUktRWmhvUVBhYTBiekJGUUZ3ZE5aZHFDYm9hL3lydGlwUHI5K1JsaTBYbzNsUC85cjVJNHE5QVhldDN6QkE4aTlvdldrdTgyTk1relY2eis2dFFqTThYN2lmc0JveHgycFdjPSxFeHBpcnk6MjAxMy0wMS0wMX0=");
#endif
            Assert.That(LicenseUtils.ActivatedLicenseFeatures(), Is.EqualTo(LicenseFeature.Business));
        }

        [Test]
        public void Expired_licenses_throws_LicenseException()
        {
            try
            {
                Licensing.RegisterLicense(TestBusiness2000Text);
                Assert.Fail("Should throw Expired LicenseException");
            }
            catch (LicenseException ex)
            {
                ex.Message.Print();
                Assert.That(ex.Message, Is.StringStarting("This license has expired"));
            }

            try
            {
                Licensing.RegisterLicense(TestBusiness2000Text);
                Assert.Fail("Should throw Expired LicenseException");
            }
            catch (LicenseException ex)
            {
                ex.Message.Print();
                Assert.That(ex.Message, Is.StringStarting("This license has expired"));
            }
        }
    }
}