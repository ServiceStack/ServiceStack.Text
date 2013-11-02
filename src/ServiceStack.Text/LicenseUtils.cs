// Copyright (c) Service Stack LLC. All Rights Reserved.
// License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using ServiceStack.Text;

namespace ServiceStack
{
    public class LicenseException : Exception
    {
        public LicenseException(string message) : base(message) {}
    }

    public enum LicenseType
    {
        Free,
        Indie,
        Business,
        Enterprise
    }

    [Flags]
    public enum LicenseFeature : long
    {
        None = 0,
        All = Premium | Text | Client | Common | Redis | OrmLite | ServiceStack | Server | Razor | Admin,
        Free = None,
        Indie = All,
        Business = All,
        Enterprise = All,
        Premium = 1 << 0,
        Text = 1 << 1,
        Client = 1 << 2,
        Common = 1 << 3,
        Redis = 1 << 4,
        OrmLite = 1 << 5,
        ServiceStack = 1 << 6,
        Server = 1 << 7,
        Razor = 1 << 8,
        Admin = 1 << 9,
    }

    public enum QuotaType
    {
        Operations,      //ServiceStack
        Types,           //Text, Redis
        Tables,          //OrmLite
        PremiumFeature,  //AdminUI, Advanced Redis APIs, etc
    }

    /// <summary>
    /// Public Code API to register commercial license for ServiceStack.
    /// </summary>
    public static class Licensing
    {
        public static void RegisterLicense(string licenseKeyText)
        {
            LicenseUtils.RegisterLicense(licenseKeyText);
        }

        public static void RegisterLicenseFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new LicenseException("License file does not exist: " + filePath);

            var licenseKeyText = File.ReadAllText(filePath);
            LicenseUtils.RegisterLicense(licenseKeyText);
        }
    }

    public class LicenseKey
    {
        public string Ref { get; set; }
        public string Name { get; set; }
        public LicenseType Type { get; set; }
        public string Hash { get; set; }
        public DateTime Expiry { get; set; }
    }

    /// <summary>
    /// Internal Utilities to verify licensing
    /// </summary>
    public static class LicenseUtils
    {
        public const string AppSettingsKey = "servicestack:license";
        public const string RuntimePublicKey = "<RSAKeyValue><Modulus>nkqwkUAcuIlVzzOPENcQ+g5ALCe4LyzzWv59E4a7LuOM1Nb+hlNlnx2oBinIkvh09EyaxIX2PmaY0KtyDRIh+PoItkKeJe/TydIbK/bLa0+0Axuwa0MFShE6HdJo/dynpODm64+Sg1XfhICyfsBBSxuJMiVKjlMDIxu9kDg7vEs=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
        private const string LicensePublicKey = "<RSAKeyValue><Modulus>w2fTTfr2SrGCclwLUkrbH0XsIUpZDJ1Kei2YUwYGmIn5AUyCPLTUv3obDBUBFJKLQ61Khs7dDkXlzuJr5tkGQ0zS0PYsmBPAtszuTum+FAYRH4Wdhmlfqu1Z03gkCIo1i11TmamN5432uswwFCVH60JU3CpaN97Ehru39LA1X9E=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        private const string ContactDetails = " Please see www.servicestack.net or contact team@servicestack.net for more details.";

        static LicenseUtils()
        {
#if !(SILVERLIGHT || WP)
            //Automatically register license key stored in <appSettings/>
            var licenceKeyText = System.Configuration.ConfigurationManager.AppSettings[AppSettingsKey];
            if (!string.IsNullOrEmpty(licenceKeyText))
            {
                RegisterLicense(licenceKeyText);
            }
#endif
        }

        public static class ErrorMessages
        {
            private const string UpgradeInstructions = " Please see http://www.servicestack.net to upgrade to a commercial license.";
            internal const string ExceededTextTypes = "The free-quota limit on '{0} ServiceStack.Text Types' has been reached." + UpgradeInstructions;
            internal const string ExceededRedisTypes = "The free-quota limit on '{0} Redis Types' has been reached." + UpgradeInstructions;
            internal const string ExceededOrmLiteTables = "The free-quota limit on '{0} OrmLite Tables' has been reached." + UpgradeInstructions;
            internal const string ExceededServiceStackOperations = "The free-quota limit on '{0} ServiceStack Operations' has been reached." + UpgradeInstructions;
            internal const string ExceededAdminUi = "The Admin UI is a commerical-only premium feature." + UpgradeInstructions;
            internal const string ExceededPremiumFeature = "Unauthorized use of a commerical-only premium feature." + UpgradeInstructions;
        }

        public static class FreeQuotas
        {
            public const int ServiceStackOperations = 10;
            public const int TextTypes = 20;
            public const int RedisTypes = 10;
            public const int OrmLiteTables = 10;
            public const int PremiumFeature = 0;
        }
        
        /// <summary>
        /// TODO: will be removed during beta
        /// </summary>
        public static bool EnforceLicenseRestrictions = true;

        public static void AssertEvaluationLicense()
        {
            if (DateTime.UtcNow > new DateTime(2013, 12, 31))
                throw new LicenseException("The evaluation license for this software has expired. " +
                    "See http://www.servicestack.net to upgrade to a valid license.");
        }

        private static LicenseKey __activatedLicense;
        internal static void RegisterLicense(string licenseKeyText)
        {
            string cutomerId = null;
            try
            {
                var parts = licenseKeyText.SplitOnFirst('-');
                cutomerId = parts[0];

                LicenseKey key;
#if !(SILVERLIGHT || WP)
                if (!licenseKeyText.VerifyLicenseKeyText(out key))
                    throw new ArgumentException("licenseKeyText");
#else
            key = licenseKeyText.ToLicenseKey();
#endif
                var releaseDate = Env.GetReleaseDate();
                if (releaseDate > key.Expiry)
                    throw new LicenseException("This license has expired on {0} and is not valid for use with this release."
                        .Fmt(key.Expiry.ToShortDateString()) + ContactDetails);

                __activatedLicense = key;
            }
            catch (Exception ex)
            {
                if (ex is LicenseException)
                    throw;

                var msg = "This license is invalid." + ContactDetails;
                if (!string.IsNullOrEmpty(cutomerId))
                    msg += " The id for this license is '{0}'".Fmt(cutomerId);

                throw new LicenseException(msg);
            }
        }

        public static LicenseFeature ActivatedLicenseFeatures()
        {
            return __activatedLicense != null ? __activatedLicense.GetLicensedFeatures() : LicenseFeature.None;
        }

        public static void ApprovedUsage(LicenseFeature licenseFeature, LicenseFeature requestedFeature,
            int allowedUsage, int actualUsage, string message)
        {
            var hasFeature = (requestedFeature & licenseFeature) == requestedFeature;
            if (hasFeature)
                return;

            if (actualUsage > allowedUsage)
                throw new LicenseException(message.Fmt(allowedUsage));
        }

        public static void AssertValidUsage(LicenseFeature feature, QuotaType quotaType, int count)
        {
            if (!EnforceLicenseRestrictions) 
                return;

            var licensedFeatures = ActivatedLicenseFeatures();
            if ((LicenseFeature.All & licensedFeatures) == LicenseFeature.All) //Standard Usage
                return;

            //Free Quotas
            switch (feature)
            {
                case LicenseFeature.Text:
                    switch (quotaType)
                    {
                        case QuotaType.Types:
                            ApprovedUsage(licensedFeatures, feature, FreeQuotas.TextTypes, count, ErrorMessages.ExceededTextTypes);
                        return;
                    }
                    break;

                case LicenseFeature.Redis:
                    switch (quotaType)
                    {
                        case QuotaType.Types:
                            ApprovedUsage(licensedFeatures, feature, FreeQuotas.RedisTypes, count, ErrorMessages.ExceededRedisTypes);
                        return;
                    }
                    break;

                case LicenseFeature.OrmLite:
                    switch (quotaType)
                    {
                        case QuotaType.Tables:
                            ApprovedUsage(licensedFeatures, feature, FreeQuotas.OrmLiteTables, count, ErrorMessages.ExceededOrmLiteTables);
                        return;
                    }
                    break;

                case LicenseFeature.ServiceStack:
                    switch (quotaType)
                    {
                        case QuotaType.Operations:
                            ApprovedUsage(licensedFeatures, feature, FreeQuotas.ServiceStackOperations, count, ErrorMessages.ExceededServiceStackOperations);
                        return;
                    }
                    break;

                case LicenseFeature.Admin:
                    switch (quotaType)
                    {
                        case QuotaType.PremiumFeature:
                            ApprovedUsage(licensedFeatures, feature, FreeQuotas.PremiumFeature, count, ErrorMessages.ExceededAdminUi);
                        return;
                    }
                    break;

                case LicenseFeature.Premium:
                    switch (quotaType)
                    {
                        case QuotaType.PremiumFeature:
                            ApprovedUsage(licensedFeatures, feature, FreeQuotas.PremiumFeature, count, ErrorMessages.ExceededPremiumFeature);
                        return;
                    }
                    break;
            }

            throw new LicenseException("Unknown Quota Usage: {0}, {1}".Fmt(feature, quotaType));
        }

        public static LicenseFeature GetLicensedFeatures(this LicenseKey key)
        {
            switch (key.Type)
            {
                case LicenseType.Free:
                    return LicenseFeature.Free;
                case LicenseType.Indie:
                    return LicenseFeature.Indie;
                case LicenseType.Business:
                    return LicenseFeature.Business;
                case LicenseType.Enterprise:
                    return LicenseFeature.Enterprise;
            }
            throw new ArgumentException("Unknown License Type: " + key.Type);
        }
        
        public static bool VerifySignedHash(byte[] DataToVerify, byte[] SignedData, RSAParameters Key)
        {
            try
            {
                var RSAalg = new RSACryptoServiceProvider();
                RSAalg.ImportParameters(Key);
                return RSAalg.VerifySha1Data(DataToVerify, SignedData);

            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);

                return false;
            }
        }

        public static LicenseKey ToLicenseKey(this string licenseKeyText)
        {
            licenseKeyText = Regex.Replace(licenseKeyText, @"\s+", "");
            var parts = licenseKeyText.SplitOnFirst('-');
            var refId = parts[0];
            var base64 = parts[1];
            var jsv = Convert.FromBase64String(base64).FromUtf8Bytes();
            var key = jsv.FromJsv<LicenseKey>();

            if (key.Ref != refId)
                throw new LicenseException("The license '{0}' is not assigned to CustomerId '{1}'.".Fmt(base64));

            return key;
        }

        public static string GetHashKeyToSign(this LicenseKey key)
        {
            return "{0}:{1}:{2}".Fmt(key.Ref, key.Name, key.Type);
        }

        public static bool VerifyLicenseKeyText(this string licenseKeyText, out LicenseKey key)
        {
            var publicRsaProvider = new RSACryptoServiceProvider();
            publicRsaProvider.FromXmlString(LicensePublicKey);
            var publicKeyParams = publicRsaProvider.ExportParameters(false);

            key = licenseKeyText.ToLicenseKey();
            var originalData = key.GetHashKeyToSign().ToUtf8Bytes();
            var signedData = Convert.FromBase64String(key.Hash);

            return VerifySignedHash(originalData, signedData, publicKeyParams);
        }

        public static bool VerifySha1Data(this RSACryptoServiceProvider RSAalg, byte[] unsignedData, byte[] encryptedData)
        {
#if !(SILVERLIGHT || WP)          
                return RSAalg.VerifyData(unsignedData, new SHA1CryptoServiceProvider(), encryptedData);
#else
            return RSAalg.VerifyData(unsignedData, encryptedData, new EMSAPKCS1v1_5_SHA1());
#endif
        }
    }
}