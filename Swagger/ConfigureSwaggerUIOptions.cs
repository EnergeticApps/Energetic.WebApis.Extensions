using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Linq;

namespace Microsoft.Extensions.Options
{
    public sealed class ConfigureSwaggerUIOptions : IConfigureOptions<SwaggerUIOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;
        private readonly OpenApiInfo _apiInfo;
        private readonly SecurityOptions _securitySettings;

        public ConfigureSwaggerUIOptions(
            IApiVersionDescriptionProvider versionDescriptionProvider,
            IOptions<OpenApiInfo> apiInfoAccessor,
            IOptions<SecurityOptions> securitySettings)
        {
            _apiInfo = apiInfoAccessor.Value ?? throw new ArgumentNullException(nameof(apiInfoAccessor));

            if (string.IsNullOrWhiteSpace(_apiInfo?.Title))
                throw new InvalidOperationException("No API information (such as title etc.) has been passed." +
                    "Maybe it is missing from the configuration file or not registered in the dependency injection container.");

            _provider = versionDescriptionProvider ?? throw new ArgumentNullException(nameof(versionDescriptionProvider));

            _securitySettings = securitySettings.Value ?? throw new ArgumentNullException(nameof(securitySettings));

            if (_securitySettings?.Jwt is null || string.IsNullOrWhiteSpace(_securitySettings.Jwt?.Authority))
                throw new InvalidOperationException("No JWT information (such as Authority etc.) has been passed." +
                    "Maybe it is missing from the configuration file or not registered in the dependency injection container.");
        }

        public void Configure(SwaggerUIOptions options)
        {
            var versions = _provider.ApiVersionDescriptions;

            foreach (var version in versions)
            {
                string groupName = version.GroupName.ToLowerInvariant();

                string versionName = $"{_apiInfo.Title} version {version.ApiVersion}";
                options.SwaggerEndpoint($"{groupName}/swagger.json", versionName);
                // options.RoutePrefix = "api";
            }

            options.OAuthClientId("CanLearn.Clients.SwaggerUIClient"); //_securitySettings.Jwt.ClientId);
            options.OAuthClientSecret("Kq2w4DzbNNDUNiBiocXRv7SvJohT75KmqiGnNwSX7RE=");//_securitySettings.Jwt.ClientSecret);
            options.OAuthAppName("CanLearn Swagger UI Client");
            options.OAuthScopeSeparator(" ");
            options.OAuthUsePkce();
        }
    }
}