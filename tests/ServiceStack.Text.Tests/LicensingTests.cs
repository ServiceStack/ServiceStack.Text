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
        const string TestBusiness2000Text = "1001-e1JlZjoxMDAxLE5hbWU6VGVzdCBCdXNpbmVzcyxUeXBlOkJ1c2luZXNzLEhhc2g6T3dIRSt5T1FOV2NmYzZWWGpXd09QN3Y3L3Z2a09vWFUrN0FZUm5RZis1bXNTenpkejFSQnJUUzltQXZHNkV1SjVUTTMzR2JhZXZ5OENVQ08rRXZoSTlmeXo5SGt6bm5HekVmakh5U3VXM3JQc1ZmVHRqclJzTVZBOWYrNGMyUk1wUnlHOWVWRmVUR3lodXpvQ1JCODdlRXZDWTc4K0hLUkxpNHd3ZUNTakFrPSxFeHBpcnk6MjAwMC0wMS0wMX0=";
        readonly LicenseKey TestBusiness2000 = new LicenseKey { Ref = "1001", Name = "Test Business", Type = LicenseType.Business, Expiry = new DateTime(2000, 01, 01) };
        const string TestIndie2000Text = "1001-e1JlZjoxMDAxLE5hbWU6VGVzdCBJbmRpZSxUeXBlOkluZGllLEhhc2g6RTUzckttMEtDRWsvbzBEQWNicUNKSFVwRG1jZjV0akc4YUFzVnIvWnorTGovdzBnWW4xY0FJRTh5T2hjUjBZNG56eE9yd3FQdVFSTy9qeGl5dlI5RmZwamc4U05ud0ppUW9DMlRhU3RNaEkwV1loVXA1Umc2bjJFeG1JdWViRFJ0a21DaFFFZHlXQTdid0pGOUFualFHQ1RCZ0w2UDUyL2o1cGFnQytKQS84PSxFeHBpcnk6MjAwMC0wMS0wMX0=";
        readonly LicenseKey TestIndie2000 = new LicenseKey { Ref = "1001", Name = "Test Indie", Type = LicenseType.Indie, Expiry = new DateTime(2000, 01, 01) };
        private const string TestBusiness2014Text = "1001-e1JlZjoxMDAxLE5hbWU6VGVzdCBCdXNpbmVzcyxUeXBlOkJ1c2luZXNzLEhhc2g6T3dIRSt5T1FOV2NmYzZWWGpXd09QN3Y3L3Z2a09vWFUrN0FZUm5RZis1bXNTenpkejFSQnJUUzltQXZHNkV1SjVUTTMzR2JhZXZ5OENVQ08rRXZoSTlmeXo5SGt6bm5HekVmakh5U3VXM3JQc1ZmVHRqclJzTVZBOWYrNGMyUk1wUnlHOWVWRmVUR3lodXpvQ1JCODdlRXZDWTc4K0hLUkxpNHd3ZUNTakFrPSxFeHBpcnk6MjAxNC0wMS0wMX0=";
        readonly LicenseKey TestBusiness2014 = new LicenseKey { Ref = "1001", Name = "Test Business", Type = LicenseType.Business, Expiry = new DateTime(2014, 01, 01) };
        const string TestIndie2014Text = "1001-e1JlZjoxMDAxLE5hbWU6VGVzdCBJbmRpZSxUeXBlOkluZGllLEhhc2g6RTUzckttMEtDRWsvbzBEQWNicUNKSFVwRG1jZjV0akc4YUFzVnIvWnorTGovdzBnWW4xY0FJRTh5T2hjUjBZNG56eE9yd3FQdVFSTy9qeGl5dlI5RmZwamc4U05ud0ppUW9DMlRhU3RNaEkwV1loVXA1Umc2bjJFeG1JdWViRFJ0a21DaFFFZHlXQTdid0pGOUFualFHQ1RCZ0w2UDUyL2o1cGFnQytKQS84PSxFeHBpcnk6MjAxNC0wMS0wMX0=";
        readonly LicenseKey TestIndie2014 = new LicenseKey { Ref = "1001", Name = "Test Indie", Type = LicenseType.Indie, Expiry = new DateTime(2014, 01, 01) };

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

        [Test]
        public void Can_register_valid_licenses()
        {
            Licensing.RegisterLicense(TestBusiness2014Text);
            Assert.That(LicenseUtils.ActivatedLicenseFeatures(), Is.EqualTo(LicenseFeature.Business));

            Licensing.RegisterLicense(TestIndie2014Text);
            Assert.That(LicenseUtils.ActivatedLicenseFeatures(), Is.EqualTo(LicenseFeature.Indie));
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