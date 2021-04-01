using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Linq;

namespace Microsoft.Extensions.Options
{
    public sealed class ConfigureSwaggerUIOptions : IConfigureOptions<SwaggerUIOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;

        public ConfigureSwaggerUIOptions(IApiVersionDescriptionProvider versionDescriptionProvider)
        {
            _provider = versionDescriptionProvider ?? throw new ArgumentNullException(nameof(versionDescriptionProvider));
        }

        public void Configure(SwaggerUIOptions options)
        {
            var versions = _provider.ApiVersionDescriptions;

            foreach (var version in versions)
            {
                string groupName = version.GroupName.ToLowerInvariant();

                options.SwaggerEndpoint($"{groupName}/swagger.json", groupName);
                //options.RoutePrefix = string.Empty;
            }
        }
    }
}