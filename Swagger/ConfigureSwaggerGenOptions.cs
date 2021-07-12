using Energetic.WebApis;
using Energetic.WebApis.Extensions;
using IdentityModel.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;

namespace Microsoft.Extensions.Options
{
    public sealed class ConfigureSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _apiVersionDescriptionProvider;
        private readonly OpenApiInfo _apiInfo;
        private readonly SecurityOptions _swaggerUIClientSettings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAssemblyMarkerTypes _assemblyMarkerTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureSwaggerGenOptions"/> class.
        /// </summary>
        /// <param name="provider">The <see cref="IApiVersionDescriptionProvider">provider</see> used to generate Swagger documents.</param>
        public ConfigureSwaggerGenOptions(
            IApiVersionDescriptionProvider apiVersionDescriptionProvider,
            IHttpClientFactory httpClientFactory,
            IOptions<OpenApiInfo> apiInfoAccessor,
            IOptions<SecurityOptions> swaggerUIClientSettings,
            IAssemblyMarkerTypes assemblyMarkerTypes,
            IWebHostEnvironment environment)
        {
            _apiVersionDescriptionProvider = apiVersionDescriptionProvider ?? throw new ArgumentNullException(nameof(apiVersionDescriptionProvider));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _assemblyMarkerTypes = assemblyMarkerTypes ?? throw new ArgumentNullException(nameof(assemblyMarkerTypes));
            _apiInfo = apiInfoAccessor.Value ?? throw new ArgumentNullException(nameof(apiInfoAccessor));

            if (string.IsNullOrWhiteSpace(_apiInfo?.Title))
                throw new InvalidOperationException("No API information (such as title etc.) has been passed." +
                    "Maybe it is missing from the configuration file or not registered in the dependency injection container.");

            _swaggerUIClientSettings = swaggerUIClientSettings.Value ?? throw new ArgumentNullException(nameof(swaggerUIClientSettings));

            // We don't ensure that valid JWT information (such as Authority etc.) has been passed at this stage.
            // If it has not, we simply won't configure the Swagger UI to authenticate endpoint calls.

            bool isDevelopmentEnvironment = environment.IsDevelopment();

            if (_swaggerUIClientSettings.Jwt is null && !isDevelopmentEnvironment)
            {
                throw new InvalidOperationException("No JWT information (such as Authority etc.) has been passed. Maybe it is missing from the configuration file or not registered in the dependency injection container.");
            }
        }



        public void Configure(SwaggerGenOptions options)
        {
            MapValueObjectsToSwaggerPrimitiveSchemaTypes(options, _assemblyMarkerTypes.ValueObjects);
            IncludeXmlComments(options);
            ApplyAestheticImprovements(options);
            AddSecurityIfJwtConfigured(options);
            AddVersions(options);
        }

        private void AddSecurityIfJwtConfigured(SwaggerGenOptions options)
        {
            if (!string.IsNullOrWhiteSpace(_swaggerUIClientSettings.Jwt?.Authority))
            {
                AddSecurity(options);
            }
        }

        private void MapValueObjectsToSwaggerPrimitiveSchemaTypes(SwaggerGenOptions options, params Type[] markerTypes)
        {
            var assemblies = markerTypes.GetContainingAssemblies();
            var valueObjectTypes = assemblies.GetConcreteTypes().GetValueObjectTypes();

            foreach (var valueObjectType in valueObjectTypes)
            {
                Type baseType = valueObjectType.GetValueObjectBaseType();
                Type primitiveType = baseType.GenericTypeArguments[1];

                options.MapType(valueObjectType, () => GetEquivalentOpenApiSchema(primitiveType));
            }
        }

        private void ApplyAestheticImprovements(SwaggerGenOptions options)
        {
            options.DescribeAllParametersInCamelCase();
            options.DocumentFilter<UIShouldDisplayControllerActionMethodsInLowercase>();
        }

        private void AddVersions(SwaggerGenOptions options)
        {
            options.DocumentFilter<UIShouldHideMethodsOfDefaultApiVersionFilter>();

            foreach (var version in _apiVersionDescriptionProvider.ApiVersionDescriptions)
            {
                string groupName = version.GroupName.ToLowerInvariant();

                var openApiInfo = new OpenApiInfo()
                {
                    Title = $"{_apiInfo.Title} {version.ApiVersion}",
                    Version = version.ApiVersion.ToString(),
                    Description = _apiInfo.Description,
                    Contact = _apiInfo.Contact,
                    TermsOfService = _apiInfo.TermsOfService,
                    License = _apiInfo.License,
                };

                if (version.IsDeprecated)
                    openApiInfo.Description += " (deprecated)";

                options.SwaggerDoc(groupName, openApiInfo);
            }
        }

        private static void IncludeXmlComments(SwaggerGenOptions options)
        {
            var entryAssembly = Assembly.GetEntryAssembly();

            if (entryAssembly is { })
            {
                var xmlFile = $"{entryAssembly.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                try
                {
                    options.IncludeXmlComments(xmlPath);
                }
                catch (FileNotFoundException ex)
                {
                    throw new InvalidOperationException($"Couldn't find the XML documentation file at {xmlPath}. Ensure that XML documentation " +
                        $"is enabled in the project and that the path is set in the \"XML documentation file\" field of the \"Output\" section " +
                        $"of your project's \"Build\" settings.", ex);
                }
            }
        }

        private void AddSecurity(SwaggerGenOptions options)
        {
            var discoveryDocument = GetDiscoveryDocument();

            options.OperationFilter<AuthorizeOperationFilter>();

            options.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,

                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(discoveryDocument.AuthorizeEndpoint),
                        TokenUrl = new Uri(discoveryDocument.TokenEndpoint),
                        Scopes = new Dictionary<string, string>
                        {
                            { _swaggerUIClientSettings.Jwt.Audience, _apiInfo.Title }
                        },
                    }
                },
                Description = _apiInfo.Description,
            });
        }

        private DiscoveryDocumentResponse GetDiscoveryDocument()
        {
            string authority = _swaggerUIClientSettings.Jwt.Authority;
            var client = _httpClientFactory.CreateClient(NamedHttpClients.AuthenticationServerDiscoveryDocumentClient);
            var document = client.GetDiscoveryDocumentAsync(authority).GetAwaiter().GetResult();

            if (document.IsError)
                throw new InvalidOperationException($"Unable to retrieve the discovery document from the authentication server at {authority}. " +
                    $"Please check that it is serving requests.", document.Exception);

            return document;
        }

        private static OpenApiSchema GetEquivalentOpenApiSchema(Type dotNetType)
        {
            var (schema, format) = GetOpenApiSchemaTypeClosestTo(dotNetType);

            if (format is null)
            {
                return new OpenApiSchema { Type = schema };
            }

            return new OpenApiSchema { Type = schema, Format = format };
        }

        private static (string schema, string? format) GetOpenApiSchemaTypeClosestTo(Type primitiveType)
        {
            return primitiveType switch
            {
                Type type when type == typeof(int) => ("integer", "int32"),
                Type type when type == typeof(long) => ("integer", "int64"),
                Type type when type == typeof(decimal) => ("number", "float"),
                Type type when type == typeof(double) => ("number", "double"),
                Type type when type == typeof(string) => ("string", null),
                Type type when type == typeof(Guid) => ("string", null),
                Type type when type == typeof(bool) => ("boolean", null),
                Type type when type == typeof(DateTime) => ("string", "date-time"),
                Type type when type == typeof(DateTimeOffset) => ("string", "date-time"),
                _ => throw new NotSupportedException($"Don't know which OpenApi primitive type schema is closest to {primitiveType.Name}."),
            };
        }
    }
}