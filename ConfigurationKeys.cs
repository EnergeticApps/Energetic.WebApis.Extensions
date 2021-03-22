using System;
using System.Collections.Generic;
using System.Text;

namespace Energetic.WebApis
{
    internal static class ConfigurationKeys
    {
        //TODO: Maybe all these appSettings keys should be stored in their respective Options classes as a private field that can be
        //accessed by reflection? Or maybe just determined by the name of the options class? E.g. AccountsOptions => "Accounts"
        public const string ApiVersionsAppSettingsKey = "Versions";
        public const string SecurityCertificateAppSettingsKey = "SecurityCertificate";
        public const string AuthenticationProvidersAppSettingsKey = "Authentication";
        public const string AccountsAppSettingsKey = "Accounts";
    }
}
