using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Energetic.WebApis;
using System.Collections.Generic;


namespace Microsoft.Extensions.DependencyInjection
{
    public static class VersioningServiceCollectionExtensions
    {
        //TODO: Make this work
        public static IServiceCollection AddVersioning(this IServiceCollection services)
        {
            var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
            services.Configure<List<OpenApiInfo>>(configuration.GetSection(ConfigurationKeys.ApiVersionsAppSettingsKey));

            services.AddApiVersioning(a =>
            {
                a.AssumeDefaultVersionWhenUnspecified = true;
                a.DefaultApiVersion = new ApiVersion(1, 0);
            });

            return services;
        }
    }
}
