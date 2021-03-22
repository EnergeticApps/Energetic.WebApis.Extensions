using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Builder
{
    public static class SwaggerApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSwaggerUIForEveryApiVersion(this IApplicationBuilder app, IConfiguration configuration)
        {
            app.UseSwagger();

            foreach (var api in SwaggerHelper.GetApiVersions(configuration))
            {
                app.UseSwaggerUI(s =>
                {
                    s.SwaggerEndpoint($"/swagger/{api.Version.ToLowerInvariant()}/swagger.json", api.Title);
                });
            }

            return app;
        }
    }
}
