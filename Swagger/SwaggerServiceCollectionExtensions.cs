using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SwaggerServiceCollectionExtensions
    {      
        public static IServiceCollection AddSwaggerGenForEveryApiVersion(this IServiceCollection services, IConfiguration configuration, params Type[] valueObjectMarkerTypes)
        {
            // TODO: This method is so ugly. Refactor.

            services.MapValueObjectsToPrimitiveSchemaTypes(valueObjectMarkerTypes);

            var apis = SwaggerHelper.GetApiVersions(configuration);

            services.AddSwaggerGen(c =>
            {
                c.IncludeXmlComments();
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme. <br />
                          Enter 'Bearer' [space] and then your token in the text input below.
                          <br />Example: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(
                    new OpenApiSecurityRequirement()
                    {
                            {
                                new OpenApiSecurityScheme
                                {
                                    Reference = new OpenApiReference
                                    {
                                        Type = ReferenceType.SecurityScheme,
                                        Id = "Bearer"
                                    },
                                    Scheme = "oauth2",
                                    Name = "Bearer",
                                    In = ParameterLocation.Header,
                                },
                                new List<string>()
                            }
                    });

                foreach (OpenApiInfo api in SwaggerHelper.GetApiVersions(configuration))
                {
                    c.SwaggerDoc(api.Version.ToLowerInvariant(), api);
                }
            });

            return services;
        }

        private static void MapValueObjectsToPrimitiveSchemaTypes(this IServiceCollection services, params Type[] markerTypes)
        {
            var assemblies = markerTypes.GetContainingAssemblies();
            var valueObjectTypes = assemblies.GetConcreteTypes().GetValueObjectTypes();

            services.AddSwaggerGen(c =>
            {
                foreach (var valueObjectType in valueObjectTypes)
                {
                    Type valueObjectBaseType = valueObjectType.GetValueObjectBaseType();
                    Type primitiveType = valueObjectBaseType.GenericTypeArguments[1];

                    c.MapType(valueObjectType, () => GetEquivalentOpenApiSchema(primitiveType));
                }
            });
        }

        private static void IncludeXmlComments(this SwaggerGenOptions options)
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