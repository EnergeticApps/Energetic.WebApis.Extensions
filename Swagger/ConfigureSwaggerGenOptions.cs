using Energetic.WebApis;
using Energetic.WebApis.Extensions;
using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
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
            IAssemblyMarkerTypes assemblyMarkerTypes)
        {
            _apiVersionDescriptionProvider = apiVersionDescriptionProvider ?? throw new ArgumentNullException(nameof(apiVersionDescriptionProvider));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _assemblyMarkerTypes = assemblyMarkerTypes ?? throw new ArgumentNullException(nameof(assemblyMarkerTypes));
            _apiInfo = apiInfoAccessor.Value ?? throw new ArgumentNullException(nameof(apiInfoAccessor));

            if (string.IsNullOrWhiteSpace(_apiInfo?.Title))
                throw new InvalidOperationException("No API information (such as title etc.) has been passed." +
                    "Maybe it is missing from the configuration file or not registered in the dependency injection container.");

            _swaggerUIClientSettings = swaggerUIClientSettings.Value ?? throw new ArgumentNullException(nameof(swaggerUIClientSettings));

            if (_swaggerUIClientSettings?.Jwt is null || string.IsNullOrWhiteSpace(_swaggerUIClientSettings.Jwt?.Authority))
                throw new InvalidOperationException("No JWT information (such as Authority etc.) has been passed." +
                    "Maybe it is missing from the configuration file or not registered in the dependency injection container.");
        }



        public void Configure(SwaggerGenOptions options)
        {
            MapValueObjectsToSwaggerPrimitiveSchemaTypes(options, _assemblyMarkerTypes.ValueObjects);
            IncludeXmlComments(options);
            ApplyAestheticImprovements(options);
            AddSecurity(options);
            AddVersions(options);
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
            switch (primitiveType)
            {
                case Type type when type == typeof(int):
                    return ("integer", "int32");

                case Type type when type == typeof(long):
                    return ("integer", "int64");

                case Type type when type == typeof(decimal):
                    return ("number", "float");

                case Type type when type == typeof(double):
                    return ("number", "double");

                case Type type when type == typeof(string):
                    return ("string", null);

                case Type type when type == typeof(Guid):
                    return ("string", null);

                case Type type when type == typeof(bool):
                    return ("boolean", null);

                case Type type when type == typeof(DateTime):
                    return ("string", "date-time");

                case Type type when type == typeof(DateTimeOffset):
                    return ("string", "date-time");

                default:
                    throw new NotSupportedException($"Don't know which OpenApi primitive type schema is closest to {primitiveType.Name}.");
            }
        }
    }
}