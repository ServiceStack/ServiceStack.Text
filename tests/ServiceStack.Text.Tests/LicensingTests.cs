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
        const string TestBusiness2000Text = "1001-e1JlZjoxMDAxLE5hbWU6VGVzdCBCdXNpbmVzcyxUeXBlOkJ1c2luZXNzLEhhc2g6Um1MNm9VcDMwQmpLRExZSzBxcEhhc0ZUZ1FWWTRRZWtKTC82cmVvbVBmeDZLV25mOEhGK2lGQU9BOGNsQWZvWmNpUHFVaEkxZllYdE1uRmE4UWhVU3hid25UYmw1VzNWc3lXM0hodXNOblhRWllkckkvVTk0QkZURUFzbUt2YVdFdVQ0T2RxWXozcUdoYmhLMCt0dHNVb2xXZ2hFbUVhZnhxUXp4Y2VFbTlJPSxFeHBpcnk6MjAwMC0wMS0wMX0=";
        readonly LicenseKey TestBusiness2000 = new LicenseKey { Ref = "1001", Name = "Test Business", Type = LicenseType.Business, Expiry = new DateTime(2000, 01, 01) };
        const string TestIndie2000Text = "1001-e1JlZjoxMDAxLE5hbWU6VGVzdCBJbmRpZSxUeXBlOkluZGllLEhhc2g6SXJvL25EVXQ0V1poRUkvczRoV2pkRURWNSsxL29yNjBXWjFLSElJTWpuVlBmczhnajByY0txZm9jTm9Ga0dlTjkvaHorY3R4blJjazA5SDE5Qm5tOERhRzZYck91QVYzM2IyRnY4NXZxSG50OFFSNXZYd3ZHYWpEZkYxRHZxVW5kS3BUOHdFVGVZanFKVHNkL2gvZmRYMDBicWlNcTd4RmRubWZvQWNpWmVnPSxFeHBpcnk6MjAwMC0wMS0wMX0=";
        readonly LicenseKey TestIndie2000 = new LicenseKey { Ref = "1001", Name = "Test Indie", Type = LicenseType.Indie, Expiry = new DateTime(2000, 01, 01) };
        public const string TestBusiness2013Text = "1001-e1JlZjoxMDAxLE5hbWU6VGVzdCBCdXNpbmVzcyxUeXBlOkJ1c2luZXNzLEhhc2g6UVNCZHVnanNhRnE4ZHZoNnU4WUFFa2QxOVhCRDMrSEZxaEU5WjAraVFaSERobmhYZ21JWXNQR0N4dksyMDhTc09Tdlo1azRlNjNsTFVtd1AvR2VndFM1cFJpVm5SdHkyM1lZdkRJcHZwcS9DTzJ4TGUwb0wvbDhrdjlKNWlTZUJPWXVlSjBvZFpIL1RPaGs5TnJDc2dNaVJiRmQ3UFBacE9ISittQzI1RnZBPSxFeHBpcnk6MjAxMy0wMS0wMX0=";
        readonly LicenseKey TestBusiness2013 = new LicenseKey { Ref = "1001", Name = "Test Business", Type = LicenseType.Business, Expiry = new DateTime(2013, 01, 01) };
        const string TestIndie2013Text = "1001-e1JlZjoxMDAxLE5hbWU6VGVzdCBJbmRpZSxUeXBlOkluZGllLEhhc2g6Y3AyVkhFdlIwZU8zNGVXSDd3NUYvTExXdm1JcGlRdHM1N0F0LzFDT25OczdCN1NiL2s3ZnZOeUlUV3BEaWVSQ0pSQlpOUkg5MDhDK3RmSnhUbXZMcldXZGptTVFDVU9RVlVYSGpGMWkveVgvc0xnUXM5cEY1MkRmWkNnc0drRVg2SmdNbVJwY3d1TGxpZlNFRjJwWVloWk04NndIOC9CNjJVWG1qRncrK2hjPSxFeHBpcnk6MjAxNC0wMS0wMX0=";
        readonly LicenseKey TestIndie2013 = new LicenseKey { Ref = "1001", Name = "Test Indie", Type = LicenseType.Indie, Expiry = new DateTime(2014, 01, 01) };

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
            Licensing.RegisterLicense(TestBusiness2013Text);
            Assert.That(LicenseUtils.ActivatedLicenseFeatures(), Is.EqualTo(LicenseFeature.Business));

            Licensing.RegisterLicense(TestIndie2013Text);
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