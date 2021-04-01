using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;

namespace Microsoft.Extensions.Options
{
    public sealed class ConfigureSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _apiVersionDescriptionProvider;
        private readonly OpenApiInfo _apiInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureSwaggerGenOptions"/> class.
        /// </summary>
        /// <param name="provider">The <see cref="IApiVersionDescriptionProvider">provider</see> used to generate Swagger documents.</param>
        public ConfigureSwaggerGenOptions(IApiVersionDescriptionProvider apiVersionDescriptionProvider, IOptions<OpenApiInfo> apiInfoAccessor)
        {
            if (apiInfoAccessor is null)
                throw new ArgumentNullException(nameof(apiInfoAccessor));

            _apiInfo = apiInfoAccessor.Value ??
                throw new InvalidOperationException($"The {nameof(apiInfoAccessor.Value)} of the {typeof(IOptions<OpenApiInfo>)} was null.");

            if (string.IsNullOrWhiteSpace(_apiInfo.Title))
                throw new InvalidOperationException("No API information (such as title etc.) has been passed." +
                    "Maybe it is missing from the configuration file or not registered in the dependency injection container.");

            _apiVersionDescriptionProvider = apiVersionDescriptionProvider;
        }

        public void Configure(SwaggerGenOptions options)
        {
            options.IncludeXmlComments();
            options.DocumentFilter<DontDisplayMethodsOfDefaultApiVersionFilter>();

            foreach (var version in _apiVersionDescriptionProvider.ApiVersionDescriptions)
            {
                string groupName = version.GroupName;

                var openApiInfo = new OpenApiInfo()
                {
                    Title = $"{_apiInfo.Title} {version.ApiVersion}",
                    Description = _apiInfo.Description,
                    TermsOfService = _apiInfo.TermsOfService,
                    Contact = _apiInfo.Contact,
                    License = _apiInfo.License,
                    Version = version.ApiVersion.ToString(),
                };

                if (version.IsDeprecated)
                    openApiInfo.Description += " (deprecated)";

                options.SwaggerDoc(groupName, openApiInfo);
            }
        }
    }
}