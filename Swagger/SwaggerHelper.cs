using Energetic.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Energetic.ValueObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using Swashbuckle.AspNetCore.SwaggerGen;
using Energetic.WebApis;
using Microsoft.Extensions.Configuration;

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
