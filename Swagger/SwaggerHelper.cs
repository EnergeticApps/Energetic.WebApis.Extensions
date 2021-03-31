using Energetic.WebApis;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    internal static class SwaggerHelper
    {
        public static IEnumerable<OpenApiInfo> GetApiVersions(IConfiguration configuration)
        {
            return configuration.GetSection(ConfigurationKeys.ApiVersionsAppSettingsKey).Get<List<OpenApiInfo>>();
        }
    }
}