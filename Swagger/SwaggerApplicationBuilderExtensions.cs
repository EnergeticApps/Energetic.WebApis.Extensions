using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
                    string apiVersion = api.Version.ToLowerInvariant();
                    string apiTitle = api.Title;
                    s.SwaggerEndpoint($"/swagger/{apiVersion}/swagger.json", apiTitle);
                });
            }

            return app;
        }
    }
}